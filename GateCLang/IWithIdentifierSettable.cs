using Gate.LangBase;

namespace Gate.CLanguage
{
   /// <summary>
   /// Gets or sets the identifier (name) of the item, such as a variable, type, function, or macro name.
   /// </summary>
   public interface IWithIdentifierSettable : IWithIdentifier
   {
      /// <summary>
      /// Id(name) of the item, impersonate var/type/function/macro name.
      /// </summary>
      new string? Identifier { get; set; }
   }
}
