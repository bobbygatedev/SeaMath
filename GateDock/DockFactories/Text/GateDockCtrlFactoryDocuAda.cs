using Gate.ToolsView.TextCtrl;
using Gate.ToolsView.TextCtrl.Lexers;

namespace Gate.Dock.DockFactories.Text
{
   public class GateDockCtrlFactoryDocuAda : GateDockCtrlFactoryDocuText
   {
      public const string CONTEXT_MENU_ID = "MenuContext.TextFile.Ada";

      public override string DocuTypeGuid => "{514FED5D-70B2-48A3-B9BE-BC443E7A486B}";

      public override string LanguageName => "Ada";

      public override string LanguageId => LanguageName;

      public override GateTextLexer Lexer => new GateTextLexerAda();

      public override string[] AssociatedExtensions => new string[] { ".ada", ".ads", ".adb" };

      /// <summary>
      /// 
      /// </summary>
      public override string MenuContextId => CONTEXT_MENU_ID;

      public override GateTextControl.FoldZoneGetter? FoldZoneGetter => null;
   }
}

