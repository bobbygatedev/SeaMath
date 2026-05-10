using System;

namespace Gate.ToolsView.Extended
{
   public class ExtendedListEditorAttribute : Attribute
   {
      public int FixedWidth { get; set; } = 0;

      public int MinimumWidth { get; set; } = 0;

      public string? ShownName { get; set; }

      public bool IsReadOnly { get; set; }

      public string? AlternativeProperty { get; set; }

      public bool IsAlternativeStrict { get; set; }

      public bool IsAutoSize { get; set; }      
   }
}
