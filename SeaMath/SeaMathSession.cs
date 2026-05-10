using Gate.LangBase.Runtime.DbgEng;
using Gate.SeaMath.Console;
using Gate.SeaMath.Plot;
using Gate.Tools;
using Gate.Tools.AppParams;
using Gate.Tools.Text;
using static Gate.Tools.AppParams.AppParamLoadSaver;

namespace Gate.SeaMath
{
   /// <summary>
   /// Represents an abstract session for managing and interacting with SeaMath components,  including debugging,
   /// plotting, and document management functionalities.
   /// </summary>
   /// <remarks>This class serves as the base for implementing specific SeaMath sessions. It provides  core
   /// functionality for initializing the application, managing debugging tools, and  creating visualizations such as
   /// scatter plots. Derived classes must implement abstract  members to define session-specific behavior.</remarks>
   public abstract class SeaMathSession : HierarchicalItem
   {
      public delegate void OnSessionHandler(SeaMathSession session);

      public event OnSessionHandler? OnSessionInit;

      public event OnSessionHandler? OnSessionClosing;

      /// <summary>
      /// 
      /// </summary>
      protected SeaMathSession(RtmDbgEng dbgEng) => DbgEng = dbgEng;

      /// <summary>
      /// 
      /// </summary>
      private class InnerStateContainer : AppParamContainerSpecialized<SeaMathOptionPage>
      {
         private readonly SeaMathSession mySession;

         public InnerStateContainer(SeaMathSession seaMathSession) => mySession = seaMathSession;

         public override string FixedPath => mySession.XmlStatePath;

         protected override AppParamLoadSaver myMakeLoadSaver() => new ByXDoc();

         protected override TxtStringConverter myMakeStringConverter() => new TxtStringConverter.Default();
      }

      /// <summary>
      /// 
      /// </summary>
      public abstract string XmlStatePath { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract ISeaMathPlotStrategy PlotStrategy { get; }

      /// <summary>
      ///  
      /// </summary>
      public abstract ISeaMathMessageDisplayer MessageDisplayer { get; }

      /// <summary>
      /// 
      /// </summary>
      protected abstract SeaMathConsoleStrategy myMakeConsoleStrategy();

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected abstract SeaMathDocManager myMakeDocManager();

      /// <summary>
      /// Gets the console interaction strategy used by the current instance.
      /// </summary>
      public SeaMathConsoleStrategy? ConsoleStrategy { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsInited { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public virtual void Init(bool skipCSLibs = false, bool skipDllLibs = false)
      {
         myAddSubItem(myMakeDocManager());

         var dbg_ide = new SeaMathDbgIde(ConsoleStrategy = myMakeConsoleStrategy());

         myAddSubItem(dbg_ide);
         (dbg_ide.Workspace ?? throw new Crash()).Build(false, skipCSLibs, skipDllLibs);
         dbg_ide.Breakpoints = DocManager?.Breakpoints ?? [];
         IsInited = true;
         OnSessionInit?.Invoke(this);
      }

      public void Close()
      {
         OnSessionClosing?.Invoke(this);
         DbgIde?.Dispose();
      }

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEng DbgEng { get; }

      /// <summary>
      /// 
      /// </summary>
      public SeaMathDbgIde DbgIde => SubItems.OfType<SeaMathDbgIde>().FirstOrDefault() ?? throw new Gate.LangBase.Runtime.RtmException("Not set yet!");

      /// <summary>
      /// 
      /// </summary>
      public SeaMathDocManager DocManager => SubItems.OfType<SeaMathDocManager>().FirstOrDefault() ?? throw new Gate.LangBase.Runtime.RtmException("Not set yet!");

      /// <summary>
      /// 
      /// </summary>
      public SeaMathOptionPage OptionPage { get; } = new SeaMathOptionPage();
   }
}
