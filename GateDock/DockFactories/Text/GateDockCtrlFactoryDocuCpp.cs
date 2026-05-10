using Gate.ToolsView.TextCtrl;
using Gate.ToolsView.TextCtrl.Lexers;

namespace Gate.Dock.DockFactories.Text
{
   public class GateDockCtrlFactoryDocuCpp : GateDockCtrlFactoryDocuText
   {
      public const string CONTEXT_MENU_ID = "MenuContext.TextFile.Cpp";

      public override string DocuTypeGuid => "{B0F7C156-136E-4D6E-949B-A5180AB9898B}";

      public override string LanguageName => "C/C++";

      public override string LanguageId => "Cpp";

      public override GateTextLexer Lexer => new GateTextLexerCpp();

      public override string[] AssociatedExtensions => new string[] { ".cpp", ".c", ".cxx", ".h", ".hpp", ".hxx" };

      public override string MenuContextId => CONTEXT_MENU_ID;

      public override GateTextControl.FoldZoneGetter FoldZoneGetter => new GateTextControl.FoldZoneGetter.ForCpp();
   }
}

