using ScintillaNET;

namespace Gate.ToolsView.TextCtrl.Lexers
{
   public class GateTextLexerXml : GateTextLexer
   {
      public override string[] Extensions => new string[] { ".xml", ".csproj" };

      public override string Name => "Xml";

      public override GateTextLexerEnum Id => GateTextLexerEnum.Xml;

      public override string KeyWords0 => "";

      public override string KeyWords1 => "";

      public override void Configure(GateTextControl docTextCtrl)
      {
         myConfigureCommon(docTextCtrl);

         docTextCtrl.PpScintilla.Styles[Style.Xml.Comment].BackColor = Color.Green;
         docTextCtrl.PpScintilla.Styles[Style.Xml.Question].ForeColor = Color.Silver;
         docTextCtrl.PpScintilla.Styles[Style.Xml.CData].ForeColor = Color.Green;
         docTextCtrl.PpScintilla.Styles[Style.Xml.AspAt].ForeColor = Color.Silver;
         docTextCtrl.PpScintilla.Styles[Style.Xml.Asp].ForeColor = Color.Silver;
         docTextCtrl.PpScintilla.Styles[Style.Xml.Script].ForeColor = Color.Silver;
         docTextCtrl.PpScintilla.Styles[Style.Xml.XmlEnd].ForeColor = Color.Silver;
         docTextCtrl.PpScintilla.Styles[Style.Xml.XmlStart].ForeColor = Color.Silver;
         docTextCtrl.PpScintilla.Styles[Style.Xml.TagEnd].ForeColor = Color.Silver;
         docTextCtrl.PpScintilla.Styles[Style.Xml.Value].ForeColor = Color.Green;
         docTextCtrl.PpScintilla.Styles[Style.Xml.Entity].ForeColor = Color.Silver;
         docTextCtrl.PpScintilla.Styles[Style.Xml.Other].ForeColor = Color.Green;
         docTextCtrl.PpScintilla.Styles[Style.Xml.SingleString].ForeColor = Color.LightGreen;
         docTextCtrl.PpScintilla.Styles[Style.Xml.DoubleString].ForeColor = Color.FromArgb(0, 255, 0);
         docTextCtrl.PpScintilla.Styles[Style.Xml.Number].ForeColor = Color.Orange;
         docTextCtrl.PpScintilla.Styles[Style.Xml.AttributeUnknown].ForeColor = Color.Silver;
         docTextCtrl.PpScintilla.Styles[Style.Xml.Attribute].ForeColor = Color.DarkGray;
         docTextCtrl.PpScintilla.Styles[Style.Xml.TagUnknown].ForeColor = Color.DarkGreen;
         docTextCtrl.PpScintilla.Styles[Style.Xml.Tag].ForeColor = Color.FromArgb(255, 90, 80);
         docTextCtrl.PpScintilla.Styles[Style.Xml.Comment].ForeColor = Color.Yellow;
         docTextCtrl.PpScintilla.Styles[Style.Xml.XcComment].ForeColor = Color.LightGoldenrodYellow;

         myConfigureCommonEnd(docTextCtrl);
      }
   }
}
