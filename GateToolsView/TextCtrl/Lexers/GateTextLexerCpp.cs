using ScintillaNET;

namespace Gate.ToolsView.TextCtrl.Lexers
{
   public class GateTextLexerCpp : GateTextLexer
   {
      public override GateTextLexerEnum Id => GateTextLexerEnum.Cpp;

      public override string[] Extensions => new string[] { ".cpp", ".c", ".cxx", ".h", ".hpp", ".hxx" };

      public override string KeyWords0 => "" +
         "abstract inline as break case catch continue default do else explicit extern false finally fixed" +
         " for foreach goto if implicit in new null operator private protected public ref return sealed sizeof switch this throw true try typedef using virtual while static_cast dynamic_cast reinterpret_cast" +
         " struct class union interface partial namespace";

      public override string KeyWords1 => "bool unsigned char const decimal double enum float int long sbyte short static string uint ulong ushort void";

      public override string Name => "C/C++";

      public override void Configure(GateTextControl docTextCtrl)
      {
         myConfigureCommon(docTextCtrl);

         docTextCtrl.PpScintilla.Styles[Style.Cpp.Default].ForeColor = Color.Silver;
         docTextCtrl.PpScintilla.Styles[Style.Cpp.Comment].ForeColor = Color.FromArgb(0, 160, 0); // Green
         docTextCtrl.PpScintilla.Styles[Style.Cpp.CommentLine].ForeColor = Color.FromArgb(0, 160, 0); // Green
         docTextCtrl.PpScintilla.Styles[Style.Cpp.CommentDoc].ForeColor = Color.FromArgb(0, 160, 0); // Green
         docTextCtrl.PpScintilla.Styles[Style.Cpp.CommentLineDoc].ForeColor = Color.FromArgb(0, 160, 0); // Green
         docTextCtrl.PpScintilla.Styles[Style.Cpp.Number].ForeColor = Color.Orange;
         docTextCtrl.PpScintilla.Styles[Style.Cpp.Word].ForeColor = Color.FromArgb(0, 255, 150);
         docTextCtrl.PpScintilla.Styles[Style.Cpp.Word2].ForeColor = Color.FromArgb(0, 150, 255);
         docTextCtrl.PpScintilla.Styles[Style.Cpp.String].ForeColor = Color.Orange;
         docTextCtrl.PpScintilla.Styles[Style.Cpp.Character].ForeColor = Color.DarkOrange;
         docTextCtrl.PpScintilla.Styles[Style.Cpp.Verbatim].ForeColor = Color.Orange;
         docTextCtrl.PpScintilla.Styles[Style.Cpp.StringEol].BackColor = Color.Pink;
         docTextCtrl.PpScintilla.Styles[Style.Cpp.Operator].ForeColor = Color.Azure;
         docTextCtrl.PpScintilla.Styles[Style.Cpp.Preprocessor].ForeColor = Color.FromArgb(0, 150, 255);

         myConfigureCommonEnd(docTextCtrl);
      }
   }
}
