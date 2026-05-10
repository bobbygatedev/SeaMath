using Gate.ToolsView.TextCtrl;
using Gate.ToolsView.TextCtrl.Lexers;

namespace Gate.Dock.DockFactories.Text
{
   public class GateDockCtrlFactoryDocuXml : GateDockCtrlFactoryDocuText
   {
      public const string CONTEXT_MENU_ID = "MenuContext.TextFile.Xml";

      public override GateTextLexer Lexer => new GateTextLexerXml();

      public override string MenuContextId => CONTEXT_MENU_ID;

      public override GateTextControl.FoldZoneGetter FoldZoneGetter => new GateTextControl.FoldZoneGetter.ForXml();

      public override string[] AssociatedExtensions => Lexer.Extensions;

      public override string LanguageName => "Xml";

      public override string LanguageId => LanguageName;

      public override string DocuTypeGuid => "{BD6E3322-80F9-46A8-819C-4E44B0801AA8}";
   }
}

