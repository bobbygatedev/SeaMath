namespace Gate.Tools.Text.TemplateExpand
{
   internal class TemplateExpanderContentScalar : TemplateExpanderContent
   {
      private object? myValue;

      public TemplateExpanderContentScalar(TemplateExpanderSymbol symbol) => Symbol = symbol;

      public override TemplateExpanderContent this[int id] => throw new Gate.Tools.ToolsException($"this[int id] not suitable for {GetType().Name}");

      public override TemplateExpanderContent this[string id] => throw new Gate.Tools.ToolsException($"this[string id] not suitable for {GetType().Name}");

      public override TemplateExpanderSymbol Symbol { get; }

      public override string Id => Symbol.Id ?? "";

      public override object? Value
      {
         get => myValue ?? ValueInherited;

         set
         {
            if (ParentItem is TemplateExpanderContentRecord rc)
            {
               //this for synchornize possibly multiple labels per scalar
               foreach (var sca in rc.SubContents.OfType<TemplateExpanderContentScalar>().Where(s => s.Id == Id))
               {
                  sca.myValue = value;
               }
            }
            else
            {
               myValue = value;
            }
         }
      }

      public override object? ValueInherited
      {
         get
         {
            if (TemplateExpander != null && TemplateExpander.IsInheritanceEnabled)
            {
               // anchestor scalar value 
               // check if any parent has set a value of same id
               // eg  ' [L1] {[A:][L1] [;]}' with L1 = xxx and A.Count = 2 =>' xxx {xxx xxx}'
               var rcs = ParentItemChain.Skip(2).OfType<TemplateExpanderContentRecord>().ToArray();

               foreach (var rc in rcs)
               {
                  if (rc.TryGet(Id, out var val) && val?.Value != null)
                  {
                     return val.Value;
                  }
               }

               return null;
            }
            else
            {
               return null;
            }
         }
      }

      public override string ContentDescriptor => $"{Id}:{Value}";

      public override TxtTokenConst ExpandedToken =>
         Value != null ?
            new TxtTokenConst(Value.ToString() ?? "") :
            throw new Gate.Tools.ToolsException($"Not a value for scalar content '{Id}'");

      public override int? ArrayCount
      {
         get => throw new Gate.Tools.ToolsException($"Array count not valid for {GetType().Name}");
         set => throw new Gate.Tools.ToolsException($"Array count not valid for {GetType().Name}");
      }

      public override string ToString() => $"{IdExtended}:{Value}";
   }
}
