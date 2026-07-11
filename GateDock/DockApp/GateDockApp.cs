using Gate.Dock.DockAppMenu;
using Gate.Dock.DockAppWidgets;
using Gate.Dock.DockDocu;
using Gate.Dock.DockFactories;
using Gate.Dock.DockFactories.Text;
using Gate.Dock.DockTab;
using Gate.Dock.DockWidget;
using Gate.Tools;
using Gate.Tools.AppParams;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.ToolsView.TextCtrl;
using Gate.ToolsView.TextSearch;
using ScintillaNET.Gate;
using System.Diagnostics;
using static Gate.Dock.DockDocu.GateDockDocuCloseForm;
using static Gate.Tools.AppParams.AppParamLoadSaver;

namespace Gate.Dock.DockApp
{

   /// <summary>
   /// 
   /// </summary>
   public abstract class GateDockApp
   {
      public delegate void OnLoadFinishedHandler(GateDockApp app);

      /// <summary>
      /// Invoked when loading and therefore init is finished.
      /// </summary>
      public event OnLoadFinishedHandler? OnLoadFinished;

      private const string VC_REDIST_NAME = "VC_redist.x64.exe";
      private static AppParamLoadSaver myDefaultSaver = new ByXDoc();
      private readonly Lazy<TextSearchInfrastructure> myLazyFindInfrastructure;
      private readonly Lazy<GateDockAppMenuHelper> myLazyAppMenuHelper;
      private readonly Lazy<GateDockAppFormScenario> myLazyFormScenario;
      private readonly Lazy<GateDockAppStateContainer> myLazyStateContainer;
      private readonly Lazy<GateDockAppOptionContainer> myLazyOptionContainer;
      private readonly Lazy<GateDockWidgetFactory[]> myLazyWidgetFactories;
      private readonly Lazy<GateDockDocuFactory[]> myLazyDocuFactories;
      private readonly Lazy<GateDockTabPageFactory[]> myLazyTabPageFactories;
      private readonly Lazy<AppParamLanguageFileCollection> myLazyParamLanguageFiles;

      public GateDockApp()
      {
         myLazyFormScenario = new Lazy<GateDockAppFormScenario>(myMakeFormScenario);
         myLazyStateContainer = new Lazy<GateDockAppStateContainer>(myMakeAppStateContainer);
         myLazyOptionContainer = new Lazy<GateDockAppOptionContainer>(myMakeOptionContainer);
         myLazyAppMenuHelper = new Lazy<GateDockAppMenuHelper>(myMakeAppMenuHelper);
         myLazyDocuFactories = new Lazy<GateDockDocuFactory[]>(myMakeDocuFactories);
         myLazyTabPageFactories = new Lazy<GateDockTabPageFactory[]>(myMakeTabPageFactories);
         myLazyWidgetFactories = new Lazy<GateDockWidgetFactory[]>(myMakeWidgetFactories);
         myLazyFindInfrastructure = new Lazy<TextSearchInfrastructure>(myMakeFindInfrastructure);
         myLazyParamLanguageFiles = new Lazy<AppParamLanguageFileCollection>(myMakeParamLanguageFiles);
         MainForm = myMakeMainForm();
         MainForm.PpAppName = Name;
         MainForm.Name = Name;
         MainForm.PpDocuHandler = new GateDockAppDocuHandler(this);
         MainForm.OnTabPageOpen += MainForm_OnDocuOpen;
         MainForm.Load += (s, e) => myActionOnMainFormLoad();
         MainForm.OnMainFormClosing += MainForm_OnMainFormClosing;
         MarkerHandler = new GateDockDocuMarkerHandler(this);
      }

      public MsgCollection Messages { get; } = new MsgCollection();

      protected virtual AppParamLanguageFileCollection myMakeParamLanguageFiles() => new GateDockAppLanguageFileCollection(this);
      protected virtual GateDockTabPageFactory[] myMakeTabPageFactories() =>
         [.. (PlugInManager?.DetectedPlugInClasses ?? []).SelectMany(p => p.TabPageFactories ?? [])];

      protected virtual GateDockWidgetFactory[] myMakeWidgetFactories() => new GateDockWidgetFactory[] {
         new GateDockFindResultWidgetCtrl.Factory(this) ,
         new GateDockWidgetMessageCtrl.Factory(this)}.Concat(
            (PlugInManager?.DetectedPlugInClasses ?? []).SelectMany(p => p.WidgetFactories ?? [])).ToArray();

      protected virtual GateDockDocuFactory[] myMakeDocuFactories() => new GateDockDocuFactory[] {
         new GateDockCtrlFactoryDocuPlainText() ,
         new GateDockCtrlFactoryDocuCpp(),
         new GateDockCtrlFactoryDocuCSharp(),
         new GateDockCtrlFactoryDocuAda(),
         new GateDockCtrlFactoryDocuXml(),
         new GateDockCtrlFactoryDocuImageReader()}.
            Concat((PlugInManager?.DetectedPlugInClasses ?? []).
            SelectMany(p => p.DocuFactories)).
            ToArray();

      /// <summary>
      /// Returns instance for AppMenuContainer (factory method). 
      /// </summary>
      /// <returns></returns>
      protected virtual GateDockAppMenuHelper myMakeAppMenuHelper() => new GateDockAppMenuHelper(this);

      /// <summary>
      /// Returns instance for State Container (factory method). 
      /// </summary>
      /// <returns></returns>
      protected virtual GateDockAppStateContainer myMakeAppStateContainer() => new GateDockAppStateContainer(this);

      /// <summary>
      /// Returns instance for AppMenuContainer (factory method). 
      /// </summary>
      /// <returns></returns>
      protected virtual GateDockAppOptionContainer myMakeOptionContainer() => new GateDockAppOptionContainer(this);

      /// <summary>
      /// Returns instance for AppMenuContainer (factory method). 
      /// </summary>
      /// <returns></returns>
      protected virtual GateDockAppFormScenario myMakeFormScenario() => new GateDockAppFormScenario(this);

      /// <summary>
      /// Returns instance for AppMenuContainer (factory method). 
      /// </summary>
      /// <returns></returns>
      protected virtual GateDockMainForm myMakeMainForm() => new GateDockMainForm();

      /// <summary>
      /// Returns instance for AppMenuContainer (factory method). 
      /// </summary>
      /// <returns></returns>
      protected virtual TextSearchInfrastructure myMakeFindInfrastructure() => new GateDockAppFindInfrastructure(this);

      /// <summary>
      /// Returns instance for AppMenuContainer (factory method). 
      /// </summary>
      /// <returns></returns>
      protected virtual GateDockAppPlugin.Manager myMakePlugInLoader() => new GateDockAppPlugin.Manager();

      /// <summary>
      /// 
      /// </summary>
      public abstract string Name { get; }

      /// <summary>
      /// 
      /// </summary>
      public TextSearchInfrastructure PpFindInfrastructure => myLazyFindInfrastructure.Value;

      /// <summary>
      /// 
      /// </summary>
      public GateDockMainForm MainForm { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public GateDockAppMenuHelper AppMenuHelper => myLazyAppMenuHelper.Value;

      /// <summary>
      /// 
      /// </summary>
      public GateDockAppStateContainer StateContainer => myLazyStateContainer.Value;

      /// <summary>
      /// 
      /// </summary>
      public GateDockAppOptionContainer OptionContainer => myLazyOptionContainer.Value;

      public GateDockAppFormScenario DefaultFormScenario => myLazyFormScenario.Value;

      public GateDockWidgetFactory[] WidgetFactories => myLazyWidgetFactories.Value;

      public GateDockDocuFactory[] DocuFactories => myLazyDocuFactories.Value;

      public GateDockTabPageFactory[] TabPageFactories => myLazyTabPageFactories.Value;

      public GateDockCtrlFactory[] AllFactories =>
         WidgetFactories.
         Cast<GateDockCtrlFactory>().
         Concat(DocuFactories).
         Concat(TabPageFactories).
         ToArray();

      public GateDockAppPlugin.Manager? PlugInManager { get; private set; }

      public AppParamLanguageFileCollection ParamLanguageFiles => myLazyParamLanguageFiles.Value;

      public static AppParamLoadSaver DefaultSaver
      {
         get => myDefaultSaver;

         set { if (value != null) { myDefaultSaver = value; } }
      }

      /// <summary>
      /// 
      /// </summary>
      public virtual GateDockDocuFactory? DefaultDocuFactory => DocuFactories.FirstOrDefault(f => f is GateDockCtrlFactoryDocuPlainText);

      /// <summary>
      /// 
      /// </summary>
      public GateDockDocuMarkerHandler MarkerHandler { get; }

      /// <summary>
      /// 
      /// </summary>
      public GateDockWidgetCtrl[] AppWidgets => WidgetFactories.Select(h => h.Widget).ToArray();

      /// <summary>
      /// Directory path for open documents not associate to valid Path (PpDocuPath == ""). 
      /// </summary>
      public virtual string OpenFilesDir => Path.Combine(AppFolder, "OpenFiles");

      /// <summary>
      /// Application folder
      /// </summary>
      public virtual string AppFolder => GateDockAppFolder.GetStandard(Name);

      /// <summary>
      /// 
      ///  
      /// </summary>
      protected virtual bool myActionOnClose()
      {
         DefaultFormScenario.Save();
         (MarkerHandler ?? throw new Crash()).ActionOnClosing();

         var res = MainForm.PpDocuHandler.DocuClosing(MainForm, MainForm.PpTabPagesAll.OfType<IGateDockDocu>().ToArray(), false);

         if (res != SaveResultEnum.cancel)
         {
            StateContainer.Save();
            OptionContainer.Save();

            Directory.CreateDirectory(OpenFilesDir);

            foreach (var doc in MainForm.PpTabPagesAll.OfType<IGateDockDocu>().Where(d => d.PpDocuPath == ""))
            {
               doc.MthSaveFileCopy(Path.Combine(OpenFilesDir, doc.PpDocuName.Nn()));
            }
         }

         (PlugInManager ?? throw new Crash()).Unload(Messages);

         return res != SaveResultEnum.cancel;
      }

      /// <summary>
      /// Operation on form load 
      /// </summary>
      /// <param name="mainForm"></param>
      protected virtual void myActionOnMainFormLoad()
      {
         var msg = new MsgCollection();

         PlugInManager = myMakePlugInLoader();
         PlugInManager.Load(this, msg);

         StateContainer.Load(msg);
         OptionContainer.DocuTextFactories = DocuFactories.OfType<GateDockCtrlFactoryDocuText>().ToArray();
         OptionContainer.Load(msg);

         AppMenuHelper.LoadFirstTime(msg);

         if (!ScintillaExtension.CheckDll())
         {
            MessageBox.Show(
               $"Can't load Scintilla dll {ScintillaExtension.SciLexerDllPath}\n" +
               $"Please install {VC_REDIST_NAME}!");

            var exe_dir = (new FileInfo(GetType().Assembly.Location).Directory).NnOrCrash();
            var rds_inf = exe_dir.GetCombinedToFile(VC_REDIST_NAME);

            if (rds_inf.Exists)
            {
               Process.Start(new ProcessStartInfo
               {
                  FileName = "explorer.exe",
                  Arguments = $"/select,\"{rds_inf.FullName}\"",
                  UseShellExecute = true
               });
            }

            Environment.Exit(-1);
         }

         DefaultFormScenario.Load(msg);
         (MarkerHandler ?? throw new Crash()).Load();

         //todo define program log and put err msgs in

         OnLoadFinished?.Invoke(this);
      }

      private void MainForm_OnDocuOpen(object? sender, GateDockTabPageCtrl? tabPage)
      {
         if (tabPage is GateDockDocuTextCtrl txt_ctr)
         {
            txt_ctr.PpAreLineNumberActive = StateContainer.Params.AreLineNumberActive.Value;
            txt_ctr.PpFindInfrastructure = (GateTextControlFindInfrastructure)PpFindInfrastructure;
            StateContainer.Params.AreLineNumberActive.OnAnyChange +=
               (_) => txt_ctr.PpAreLineNumberActive = StateContainer.Params.AreLineNumberActive.Value;
         }
      }

      private void MainForm_OnMainFormClosing(object? sender, GateDockMainFormClosingEventArgs args) => args.IsClose2Confirm = myActionOnClose();
   }
}

