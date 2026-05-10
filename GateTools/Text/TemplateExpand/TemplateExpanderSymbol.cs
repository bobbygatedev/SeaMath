namespace Gate.Tools.Text.TemplateExpand
{
   public abstract class TemplateExpanderSymbol : HierarchicalItem
   {
      protected TemplateExpanderSymbol(string? id, TxtTokenConst token)
      {
         Id = id;
         Token = token;
      }

      public TemplateExpander? TemplateExpander => Anchestor as TemplateExpander;

      public string? Id { get; }

      public TxtTokenConst Token { get; }

      public TemplateExpanderSymbol[] SubSymbols => SubItems.OfType<TemplateExpanderSymbol>().ToArray();
   }
}
