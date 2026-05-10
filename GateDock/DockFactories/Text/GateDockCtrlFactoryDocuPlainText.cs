using Gate.ToolsView.TextCtrl;
using Gate.ToolsView.TextCtrl.Lexers;

namespace Gate.Dock.DockFactories.Text
{
   public class GateDockCtrlFactoryDocuPlainText : GateDockCtrlFactoryDocuText
   {
      public const string CONTEXT_MENU_ID = "MenuContext.TextFile.PlainText";

      public override string DocuTypeGuid => "{B86575CC-E23B-42DB-B321-0EFCA4B84746}";

      public override string LanguageName => "Text";

      public override string LanguageId => LanguageName;

      public override GateTextLexer Lexer => new GateTextLexerPlainText();

      public override string[] AssociatedExtensions => new string[] { ".txt" };

      public override string MenuContextId => CONTEXT_MENU_ID;

      public override GateTextControl.FoldZoneGetter? FoldZoneGetter => null;
   }
}

