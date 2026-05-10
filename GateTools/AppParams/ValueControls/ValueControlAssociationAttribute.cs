namespace Gate.Tools.AppParams.ValueControls
{
   /// <summary>
   /// Specifies an association between a value and a control type for use in metadata or attribute-based scenarios.
   /// </summary>
   /// <remarks>This attribute can be applied to elements to indicate a relationship between a particular value
   /// and a control type, which may be used by frameworks or tools for mapping, validation, or UI generation
   /// purposes.</remarks>
   public class ValueControlAssociationAttribute : Attribute
   {
      /// <summary>
      /// 
      /// </summary>
      public object? Id { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public Type? Type { get; set; } = null;
   }
}
