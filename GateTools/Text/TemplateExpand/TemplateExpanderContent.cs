namespace Gate.Tools.Text.TemplateExpand
{
   public abstract class TemplateExpanderContent : HierarchicalItem
   {
      protected TemplateExpanderContent() { }

      public static TemplateExpanderContent Make(TemplateExpanderSymbol symbol) => InnerCreateContentVisitor.Make((dynamic)symbol);

      /// <summary>
      /// 
      /// </summary>
      private static class InnerCreateContentVisitor
      {
         public static TemplateExpanderContent Make(TemplateExpanderSymbolScalar symbolScalar) => new TemplateExpanderContentScalar(symbolScalar);

         public static TemplateExpanderContent Make(TemplateExpanderSymbolArray symbolArray) => new TemplateExpanderContentArray(symbolArray);

         /// <summary>
         /// Border not not corresponds to any content.
         /// </summary>
         /// <param name="symbolArray"></param>
         /// <returns></returns>
         public static TemplateExpanderContent? Make(TemplateExpanderSymbolStartBorderReplace symbolArray) => null;

         public static TemplateExpanderContent? Make(TemplateExpanderSymbol symbol) => throw new Crash($"Unexpected type {symbol.GetType().Name}");
      }

      public abstract TxtTokenConst ExpandedToken { get; }

      public abstract object? Value { get; set; }

      public abstract object? ValueInherited { get; }

      public abstract TemplateExpanderContent this[int id] { get; }

      public abstract TemplateExpanderContent this[string id] { get; }

      public abstract int? ArrayCount { get; set; }

      public abstract TemplateExpanderSymbol Symbol { get; }

      public TemplateExpander? TemplateExpander => Anchestor as TemplateExpander;

      public TemplateExpanderContent[] SubContents => SubItems.OfType<TemplateExpanderContent>().ToArray();

      public abstract string Id { get; }

      public abstract string ContentDescriptor { get; }

      public string IdExtended
      {
         get
         {
            var cs = ParentItemChain.OfType<TemplateExpanderContent>().Reverse().ToArray();

            return string.Join(".", cs.Select(c => c.Id));
         }
      }
   }
}
