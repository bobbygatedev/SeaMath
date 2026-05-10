using Gate.Dock.DockDocu;
using Gate.Dock.DockFactories.Text;
using Gate.Dock.DockTab;
using Gate.Tools;
using Gate.Tools.AppParams;
using Gate.Tools.Text;
using System.Globalization;
using static Gate.Dock.DockApp.GateDockAppOptionContainer;
using static Gate.Dock.DockApp.GateDockAppOptionContainer.TextOptionPage;
using static Gate.Tools.AppParams.AppParam;
using static Gate.Tools.AppParams.AppParamLoadSaver;

namespace Gate.Dock.DockApp
{
   /// <summary>
   /// 
   /// </summary>
   public class GateDockAppOptionContainer : AppParamContainerSpecialized<ParamsType>
   {
      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="app"></param>
      public GateDockAppOptionContainer(GateDockApp app) => App = app;

      public class ParamsType : Record
      {
         public ParamsType() : base("Options") { }

         public readonly GeneralOptionPage GeneralOptionPage = new GeneralOptionPage();

         public readonly TextOptionPage TextOptionPage = new TextOptionPage();
      }

      public class LanguageFrame : Record
      {
         public LanguageFrame() : base("Language") { }

         public static CultureInfo DefaultCulture { get; } = CultureInfo.GetCultureInfo("en");

         /// <summary>
         /// 
         /// </summary>
         public readonly Simple<CultureInfo> Language = new Simple<CultureInfo>(DefaultCulture);

      }

      /// <summary>
      /// 
      /// </summary>
      public class GeneralOptionPage : Record
      {
         private readonly LanguageFrame myLanguageFrame = new LanguageFrame();

         public GeneralOptionPage() : base("General") { }

         public LanguageFrame LanguageFrame => myLanguageFrame;

         public CultureInfo Language
         {
            get => myLanguageFrame.Language.Value ?? LanguageFrame.DefaultCulture;
            set => myLanguageFrame.Language.Value = value;
         }

         public override bool IsInTreeNode => true;
      }

      public class TextOptionPage : Record
      {
         private GateDockCtrlFactoryDocuText[]? myDocuTextFactories;

         public TextOptionPage() : base("TextEditors", "Text Editors") { }

         public class DocuTextFactoryPage : Record
         {
            public DocuTextFactoryPage(GateDockCtrlFactoryDocuText factoryDocuText) : base(factoryDocuText.LanguageId, factoryDocuText.LanguageName)
            {
               FactoryDocuText = factoryDocuText;

               InsertSubParamDynamically(new TabsFrame());
            }

            public DocuTextFactoryPage() => InsertSubParamDynamically(new TabsFrame());

            public TabsFrame FrameTabs => SubRecords.OfType<TabsFrame>().FirstOrDefault() ?? throw new NullReferenceException();

            public class TabsFrame : Record
            {
               public TabsFrame() : base("Tabs") { }

               /// <summary>
               /// 
               /// </summary>
               public readonly Simple<int> TabSize = new Simple<int>(3);

               /// <summary>
               /// 
               /// </summary>
               public readonly Simple<bool> UseTabs = new Simple<bool>(false);
            }

            public GateDockCtrlFactoryDocuText? FactoryDocuText { get; }

            public override bool IsInTreeNode => true;
         }

         public GateDockCtrlFactoryDocuText[]? DocuTextFactories
         {
            get => myDocuTextFactories;
            set
            {
               if ((myDocuTextFactories = value) != null)
               {
                  foreach (var fac in myDocuTextFactories)
                  {
                     InsertSubParamDynamically(new DocuTextFactoryPage(fac));
                  }
               }
            }
         }

         public override bool IsInTreeNode => true;
      }

      public GateDockMainForm MainForm => App.MainForm;

      /// <summary>
      /// 
      /// </summary>
      public GateDockCtrlFactoryDocuText[] DocuTextFactories
      {
         get => PageText?.DocuTextFactories ?? [];
         set
         {
            if (PageText != null)
            {
               if ((PageText.DocuTextFactories = value) != null)
               {
                  MainForm.OnTabPageOpen += MainForm_OnTabPageOpen;
               }
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public GeneralOptionPage PageGeneral => Params.SubItems.OfType<GeneralOptionPage>().FirstOrDefault() ?? throw new NullReferenceException();

      /// <summary>
      /// 
      /// </summary>
      public TextOptionPage PageText => Params.SubItems.OfType<TextOptionPage>().FirstOrDefault() ?? throw new NullReferenceException();

      /// <summary>
      /// 
      /// </summary>
      public CultureInfo Language
      {
         get => PageGeneral.Language;

         set
         {
            if (PageGeneral != null)
            {
               PageGeneral.Language = value;
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override string FixedPath => Path.Combine(App.AppFolder, "Options.xml");

      public GateDockApp App { get; }

      protected override TxtStringConverter myMakeStringConverter() => new TxtStringConverter.Default();

      protected override AppParamLoadSaver myMakeLoadSaver() => new ByXDoc();

      private void MainForm_OnTabPageOpen(object? sender, GateDockTabPageCtrl? tabPage)
      {
         if (tabPage is GateDockDocuTextCtrl txt_ctr)
         {
            var fac_pag = AllDescendant.OfType<DocuTextFactoryPage>().FirstOrDefault(ff => ff.FactoryDocuText == txt_ctr.PpFactory);

            if (fac_pag != null)
            {
               txt_ctr.PpIsUseTab = fac_pag.FrameTabs.UseTabs.Value;
               txt_ctr.PpTabSpaces = fac_pag.FrameTabs.TabSize.Value;

               fac_pag.OnAnyChange += (_) =>
               {
                  txt_ctr.PpIsUseTab = fac_pag.FrameTabs.UseTabs.Value;
                  txt_ctr.PpTabSpaces = fac_pag.FrameTabs.TabSize.Value;
               };
            }
            else { throw new Crash($"Factory not found!"); }
         }
      }
   }
}

