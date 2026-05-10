using ScintillaNET;
using System.Drawing;

namespace Gate.ToolsView.TextCtrl.Lexers
{
   public class GateTextLexerAda : GateTextLexer
   {
      public override GateTextLexerEnum Id => GateTextLexerEnum.Ada;

      public override string[] Extensions => new string[] { ".ads", ".adb", ".ada" };

      public override string KeyWords0 =>
         "subtype and for out synchronized array function overriding at tagged generic package task begin goto pragma terminate body private then if procedure type case in protected constant interface until is raise use declare range delay limited " +
         "abort else new return abs elsif not reverse abstract end null accept entry select access exception of separate aliased exit or some all others record when delta loop rem while digits renames with do mod requeue xor";

      public override string KeyWords1 => "";

      public override string Name => "Ada";

      public override void Configure(GateTextControl docTextCtrl)
      {
         myConfigureCommon(docTextCtrl);

         docTextCtrl.PpScintilla.Styles[Style.Ada.Default].ForeColor = Color.Silver;
         docTextCtrl.PpScintilla.Styles[Style.Ada.CommentLine].ForeColor = Color.LightGreen;
         docTextCtrl.PpScintilla.Styles[Style.Ada.Number].ForeColor = Color.Orange;
         docTextCtrl.PpScintilla.Styles[Style.Ada.Word].ForeColor = Color.FromArgb(0, 120, 255);
         docTextCtrl.PpScintilla.Styles[Style.Ada.String].ForeColor = Color.Orange;
         docTextCtrl.PpScintilla.Styles[Style.Ada.Character].ForeColor = Color.Orange; // Red
         docTextCtrl.PpScintilla.Styles[Style.Ada.StringEol].BackColor = Color.Orange;
         docTextCtrl.PpScintilla.Styles[Style.Ada.Delimiter].ForeColor = Color.Aquamarine; // Red
         docTextCtrl.PpScintilla.Styles[Style.Ada.Label].ForeColor = Color.Pink;
         docTextCtrl.PpScintilla.Styles[Style.Ada.Identifier].ForeColor = Color.Aquamarine;
         docTextCtrl.PpScintilla.Styles[Style.Ada.CharacterEol].ForeColor = Color.Pink;
         docTextCtrl.PpScintilla.Styles[Style.Ada.Illegal].BackColor = Color.Red;

         myConfigureCommonEnd(docTextCtrl);
      }
   }
}
