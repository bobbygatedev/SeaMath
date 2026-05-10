namespace Gate.ToolsView.TextCtrl.Lexers
{
   /// <summary>
   /// 
   /// </summary>
   public class GateTextLexerSet
   {
      public virtual GateTextLexer[] Lexers => new GateTextLexer[] {
                  new GateTextLexerPlainText() ,
                  new GateTextLexerCpp() ,
                  new GateTextLexerCSharp(),
                  new GateTextLexerAda(),
                  new GateTextLexerXml()};

      public virtual GateTextLexer Default => Lexers[0];

      public string Filter
      {
         get
         {
            var lxs = Lexers.Select(lex =>
            {
               var ln = string.Format("Files {0} ({1})|{2}", lex.Name, string.Join(",", lex.Extensions.Select(e => "*" + e)), string.Join(";", lex.Extensions.Select(e => "*" + e)));

               return ln;
            });

            return string.Join("|", lxs.Concat(new string[] { "All files (*.*)|*.*" }));
         }
      }

      public GateTextLexer GetLexer(string extension)
      {
         var ext = my_GetReworkedExtension(extension);
         var lex = Lexers.FirstOrDefault(l => l.Extensions.Any(e => string.Compare(e, extension, true) == 0));

         return lex ?? Default;
      }

      private string my_GetReworkedExtension(string extension)
      {
         extension = extension.Trim();

         if (!extension.StartsWith(".")) { extension = "." + extension; }

         return extension;
      }
   }
}
