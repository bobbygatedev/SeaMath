using Gate.ToolsView.TextCtrl;
using Gate.ToolsView.TextCtrl.Lexers;

namespace Gate.Dock.DockFactories.Text
{
   public class GateDockCtrlFactoryDocuCSharp : GateDockCtrlFactoryDocuText
   {
      public const string CONTEXT_MENU_ID = "MenuContext.TextFile.CSharp";

      public override string DocuTypeGuid => "{F23D2176-16C5-4F59-B730-A4B53B8A94DC}";

      public override string LanguageName => "C#";

      public override string LanguageId => "Csharp";

      public override GateTextLexer Lexer => new GateTextLexerCSharp();

      public override string[] AssociatedExtensions => new string[] { ".cs" };

      public override string MenuContextId => CONTEXT_MENU_ID;

      public override GateTextControl.FoldZoneGetter FoldZoneGetter => new GateTextControl.FoldZoneGetter.ForCpp();
   }
}

