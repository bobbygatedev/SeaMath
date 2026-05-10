using Gate.Tools;
using Gate.Tools.AppParams;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using static Gate.Dock.DockSkin.GateDockSkin;
using static Gate.Tools.AppParams.AppParamLoadSaver;

namespace Gate.Dock.DockSkin
{
   public delegate void OnGateDockSkinChangeHandler(object? sender, GateDockSkin? skin);

   /// <summary>
   /// 
   /// </summary>
   public class GateDockSkin : AppParamContainerSpecialized<SkinParams>
   {
      public const string CURRENT = "Current";

      private string? myName = null;
      private bool myIsCurrent = false;

      public GateDockSkin(GateDockMainForm mainForm, string appName)
      {
         MainForm = mainForm;
         AppName = appName;
      }

      public class SkinParams : AppParam.Record
      {
         public SkinParams() : base("Skin") { }

         public readonly Simple<Color> BackFrameColor = new Simple<Color>(DefaultValues.BackFrameColor);
         public readonly Simple<Color> BackContentColor = new Simple<Color>(DefaultValues.BackContentColor);
         public readonly Simple<Color> BackWidgetColor = new Simple<Color>(DefaultValues.BackWidgetColor);
         public readonly Simple<Color> ForeColor = new Simple<Color>(DefaultValues.ForeColor);
         public readonly Simple<Color> TabSelectedOnlyColor = new Simple<Color>(DefaultValues.TabSelectedOnlyColor);
         public readonly Simple<Color> WidgetSelectedColor = new Simple<Color>(DefaultValues.WidgetSelectedColor);
         public readonly Simple<Color> WidgetUnselectedColor = new Simple<Color>(DefaultValues.WidgetUnselectedColor);
         public readonly Simple<Color> BorderColor = new Simple<Color>(DefaultValues.BorderColor);
         public readonly Simple<Color> DockingFrameBorderColor = new Simple<Color>(DefaultValues.DockingFrameBorderColor);
         public readonly Simple<Color> MenuBackColor = new Simple<Color>(DefaultValues.MenuBackColor);
         public readonly Simple<Color> MenuCheckBoxBackground = new Simple<Color>(DefaultValues.MenuCheckBoxBackground);
         public readonly Simple<Color> ScrollBarBackColor = new Simple<Color>(DefaultValues.ScrollBarBackColor);
         public readonly Simple<Color> ScrollBarArrowColor = new Simple<Color>(DefaultValues.ScrollBarArrowColor);
         public readonly Simple<Color> ScrollBarGripColor = new Simple<Color>(DefaultValues.ScrollBarGripColor);
         public readonly Simple<Color> ScrollBarpGripActiveColor = new Simple<Color>(DefaultValues.ScrollBarpGripActiveColor);
         public readonly Simple<Color> FindWindowLineHighlight = new Simple<Color>(DefaultValues.FindWindowLineHighlight);
         public readonly Simple<Font> ControlsFont = new Simple<Font>(DefaultValues.ControlsFont);
      }

      /// <summary>
      /// These constants are persistent.
      /// </summary>
      public static class Constants
      {
         /// <summary>
         /// 
         /// </summary>
         public const int WIDGET_CAPTION_HEIGHT = 20;

         /// <summary>
         /// Width of widget and docs and doc tabs border in pixels.
         /// </summary>
         public const int WIDGET_DOCUTAB_BORDER = 1;
      }

      public static class DefaultValues
      {
         public readonly static Color WidgetSelectedColor = Color.FromArgb(0, 40, 255);
         public readonly static Color TabSelectedOnlyColor = Color.FromArgb(0, 20, 160);
         public readonly static Color WidgetUnselectedColor = Color.Gray;
         public readonly static Color BorderColor = Color.FromArgb(50, 50, 50);
         public readonly static Color DockingFrameBorderColor = Color.Yellow;
         public readonly static Color ForeColor = Color.White;
         public readonly static Color BackFrameColor = Color.FromArgb(30, 30, 30);
         public readonly static Color BackContentColor = Color.FromArgb(35, 35, 35);
         public readonly static Color BackWidgetColor = Color.FromArgb(45, 45, 45);
         public readonly static Color MenuBackColor = Color.FromArgb(30, 30, 30);
         public readonly static Color MenuCheckBoxBackground = Color.FromArgb(32, 32, 32);
         public readonly static Color ScrollBarBackColor = Color.FromArgb(60, 60, 60);
         public readonly static Color ScrollBarArrowColor = Color.FromArgb(70, 70, 70);
         public readonly static Color ScrollBarGripColor = Color.FromArgb(70, 70, 70);
         public readonly static Color ScrollBarpGripActiveColor = Color.FromArgb(125, 125, 125);
         public readonly static Color FindWindowLineHighlight = Color.FromArgb(0, 0, 180);
         public readonly static Font ControlsFont = new Font("Microsoft Sans Serif", 9F);
         public readonly static int ScrollBarDocuSize = 22;
         public readonly static int ScrollBarWidgetSize = 17;
      }

      public GateDockSkin GetCopy()
      {
         var cpy = new GateDockSkin(MainForm, AppName);

         cpy.IsLoading = true;
         cpy.myName = myName;

         var dst_prs = cpy.Params.AllParams.OfType<AppParam.Scalar>().ToArray();
         var src_prs = Params.AllParams.OfType<AppParam.Scalar>().ToArray();

         if (dst_prs.Length == src_prs.Length)
         {
            for (int i = 0; i < dst_prs.Length; i++) { dst_prs[i].ObjValue = src_prs[i].ObjValue; }
         }

         cpy.IsLoading = false;

         return cpy;
      }

      /// <summary>
      /// 
      /// </summary>
      public bool IsCurrent { get => myIsCurrent; set => myIsCurrent = value; }

      /// <summary>
      /// 
      /// </summary>
      public string Name
      {
         get => IsCurrent ? CURRENT : myName.Nn();
         set
         {
            if (value.ToLower() == CURRENT.ToLower()) { throw new Crash(); }
            else { myName = value; }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override string FixedPath => Path.Combine(SkinsFolder, Name + ".xml");

      public string SkinsFolder => Path.Combine(GateDockAppFolder.GetStandard(AppName), "Skins");

      public GateDockMainForm MainForm { get; }

      public string AppName { get; }

      protected override void myActionOnAnyParamChanged(AppParam appParam)
      {
         base.myActionOnAnyParamChanged(appParam);

         if (!IsLoading) { Save(); }//auto save
      }

      protected override TxtStringConverter myMakeStringConverter() => new TxtStringConverter.Default();

      protected override AppParamLoadSaver myMakeLoadSaver() => new ByXDoc();
   }
}
