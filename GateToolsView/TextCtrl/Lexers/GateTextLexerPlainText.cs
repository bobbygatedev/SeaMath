namespace Gate.ToolsView.TextCtrl.Lexers
{
   public class GateTextLexerPlainText : GateTextLexer
   {
      public override GateTextLexerEnum Id => GateTextLexerEnum.Null;
      public override string[] Extensions => new string[] { ".txt" };

      public override string KeyWords0 => "";

      public override string KeyWords1 => "";

      public override string Name => "Text";

      public override void Configure(GateTextControl docTextCtrl)
      {
         myConfigureCommon(docTextCtrl);
         myConfigureCommonEnd(docTextCtrl);
      }
   }
}
