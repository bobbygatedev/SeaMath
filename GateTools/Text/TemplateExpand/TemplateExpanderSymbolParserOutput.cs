namespace Gate.Tools.Text.TemplateExpand
{
   public class TemplateExpanderSymbolParserOutput : ICloneable
   {
      public TemplateExpanderSymbolParserOutput()
      {
         
      }

      public List<TemplateExpanderSymbol> ListSymbols { get; } = new List<TemplateExpanderSymbol>();

      public Stack<(TxtTokenConst startToken, string id, List<TemplateExpanderSymbol> listSubSymbols)> Stack { get; } = 
         new Stack<(TxtTokenConst startToken, string id, List<TemplateExpanderSymbol> listSubSymbols)> ();

      public void AddSymbol(TemplateExpanderSymbol symbol)
      {
         if (Stack.Count == 0)
         {
            ListSymbols.Add (symbol);
         }
         else
         {
            Stack.Peek().listSubSymbols.Add (symbol);
         }
      }

      public object Clone()
      {
         var cpy = new TemplateExpanderSymbolParserOutput();

         cpy.ListSymbols.AddRange(ListSymbols);
         
         foreach (var sym in Stack.Reverse())
         {
            cpy.Stack.Push(sym);
         }

         return cpy;
      }
   }
}
