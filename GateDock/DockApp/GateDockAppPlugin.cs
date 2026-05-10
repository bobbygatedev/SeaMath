using Gate.Dock.DockFactories;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.ToolsView.MenuCommand;
using System.Reflection;
using System.Runtime.Loader;

namespace Gate.Dock.DockApp
{
   /// <summary>
   /// Serves as the base class for plugins within the GateDock application.
   /// </summary>
   /// <remarks>The <see cref="GateDockAppPlugin"/> class provides a framework for creating plugins that extend
   /// the functionality of the GateDock application. Derived classes must implement abstract members to define specific
   /// behaviors, such as providing factories for documents, tabs, and widgets, or modifying menus and options. <para>
   /// This class is designed to be inherited and cannot be instantiated directly. It includes lifecycle methods for
   /// initialization and cleanup, which are invoked by the application during plugin loading and unloading.
   /// </para></remarks>
   public abstract class GateDockAppPlugin
   {
      private GateDockApp? myApp;

      /// <summary>
      /// Initializes a new instance of the <see cref="GateDockAppPlugin"/> class.
      /// </summary>
      /// <remarks>This constructor is protected to restrict instantiation of the <see
      /// cref="GateDockAppPlugin"/> class to derived classes. It is intended to be used as a base class for plugins
      /// within the GateDock application.</remarks>
      protected GateDockAppPlugin() { }

      /// <summary>
      /// 
      /// </summary>
      public class Manager
      {
         public Manager() { }

         private class InnerPluginLoadContext : AssemblyLoadContext
         {
            private readonly AssemblyDependencyResolver resolver;

            public InnerPluginLoadContext(Manager manager, FileInfo pluginInfo)
            {
               resolver = new AssemblyDependencyResolver(manager.PlugInDirectory);
               Manager = manager;
               PluginFileInfo = pluginInfo ?? throw new Crash();
               PluginDirInfo = pluginInfo.Directory ?? throw new Crash();

               try
               {
                  Assembly = LoadFromAssemblyPath(pluginInfo.FullName);

                  var tps = Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(GateDockAppPlugin))).ToArray();

                  switch (tps.Length)
                  {
                     case 0: throw new Crash($"Not found a {typeof(GateDockAppPlugin).Name} instance in {Assembly}");

                     case 1:
                        PluginEntry = tps[0].InstanciateOrCrash() as GateDockAppPlugin ?? throw new Crash();
                        break;

                     default: throw new Crash($"More than an intstance of {typeof(GateDockAppPlugin).Name} in {Assembly}");
                  }
               }
               catch (System.Reflection.ReflectionTypeLoadException exc) { throw new Crash(-1, exc.LoaderExceptions?.FirstOrDefault()); }
               catch (Exception exc) { throw new Crash(-1, exc); }
            }

            public Manager Manager { get; }

            public FileInfo PluginFileInfo { get; }

            public DirectoryInfo PluginDirInfo { get; }

            public Assembly Assembly { get; }

            public GateDockAppPlugin PluginEntry { get; internal set; }

            protected override Assembly? Load(AssemblyName assemblyName)
            {
               string? path = resolver.ResolveAssemblyToPath(assemblyName);

               var ass = AppDomain.CurrentDomain.GetAssemblies();

               //already loaded assembly
               var ass_ld = ass.FirstOrDefault(a => a.FullName == assemblyName.FullName);

               if (ass_ld != null) { return ass_ld; }
               else if (path != null) { return LoadFromAssemblyPath(path); }
               else
               {
                  var fil = PluginDirInfo.GetCombinedToFile($"{assemblyName.Name}.dll");

                  if (fil.Exists)
                  {
                     return LoadFromAssemblyPath(fil.FullName);
                  }
               }

               return null;
            }
         }

         public void Load(GateDockApp app, MsgCollection logMessages)
         {
            App = app;
            DetectedPlugInClasses = myGetDetectedPlugIns();

            foreach (var pin in DetectedPlugInClasses)
            {
               pin.myDoInitPlugin(app, logMessages);
            }
         }

         public void Unload(MsgCollection logMessages)
         {
            if (DetectedPlugInClasses != null)
            {
               foreach (var pin in DetectedPlugInClasses)
               {
                  pin.myDoClosePlugin(App ?? throw new Crash(), logMessages);
               }
            }
         }

         protected virtual GateDockAppPlugin[] myGetDetectedPlugIns()
         {
            var plu_dir = new DirectoryInfo(PlugInDirectory);
            var lst_pin = new List<GateDockAppPlugin>();


            foreach (var dir in plu_dir.EnumerateDirectories())
            {
               var fil = dir.GetCombinedToFile($"{dir.Name}.dll");

               if (fil.Exists)
               {
                  var tmp = new InnerPluginLoadContext(this, fil);

                  lst_pin.Add(tmp.PluginEntry);
               }
            }

            return lst_pin.ToArray();
         }

         public GateDockAppPlugin[]? DetectedPlugInClasses { get; private set; }

         public virtual string PlugInDirectory => Path.Combine(Application.StartupPath, "Plugins");

         public GateDockApp? App { get; private set; }

         public AssemblyDependencyResolver? Resolver { get; private set; }
      }

      /// <summary>
      /// 
      /// </summary>
      public abstract GateDockDocuFactory[] DocuFactories { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract GateDockTabPageFactory[] TabPageFactories { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract GateDockWidgetFactory[] WidgetFactories { get; }

      /// <summary>
      /// 
      /// </summary>
      public GateDockApp App => myApp ?? throw new NullReferenceException();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="app"></param>
      /// <param name="listMenuBuilders"></param>
      public abstract void ModifyMainMenuBuilders(GateDockApp app, List<CmdMenuBuilder> listMenuBuilders);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="app"></param>
      /// <param name="listMenuBuilders"></param>
      public abstract void ModifyContextMenuBuilders(GateDockApp app, List<CmdMenuBuilder> listMenuBuilders);

      /// <summary>
      /// Sub classes may optionally add option pages.
      /// </summary>
      /// <param name="optionContainer"></param>
      protected abstract void myModifyOptionContainer(GateDockAppOptionContainer optionContainer);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="app"></param>
      /// <param name="msgs"></param>
      protected abstract void myUserInitPlugin(GateDockApp app, MsgCollection logMessages);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="app"></param>
      /// <param name="logMessages"></param>
      protected abstract void myUserClosePlugin(GateDockApp app, MsgCollection logMessages);

      /// <summary>
      /// Handles the configuration of the specified GateDockApp instance.
      /// </summary>
      /// <remarks>This method is intended to be overridden in a derived class to provide custom
      /// configuration logic for the <see cref="GateDockApp"/> instance. The base implementation does
      /// nothing.</remarks>
      /// <param name="gateDockApp">The <see cref="GateDockApp"/> instance to be configured. Cannot be null.</param>
      protected virtual void myAppSetting(GateDockApp? gateDockApp)
      {
         if (gateDockApp != null)
         {
            gateDockApp.OnLoadFinished += myOnLoadFinished;
         }
      }

      /// <summary>
      /// Called when the GateDockApp has finished loading.
      /// </summary>
      /// <param name="gateDockApp"></param>
      protected virtual void myOnLoadFinished(GateDockApp gateDockApp) { }

      private void myDoInitPlugin(GateDockApp app, MsgCollection logMessages)
      {
         myAppSetting(myApp = app);
         myUserInitPlugin(app, logMessages);
         myModifyOptionContainer(myApp.OptionContainer);
      }

      private void myDoClosePlugin(GateDockApp app, MsgCollection logMessages) => myUserClosePlugin(app, logMessages);
   }
}

