using Gate.LangBase;

namespace Gate.CLanguage
{
   public interface IMayBeDefinition : IWithIdentifier
   {
      /// <summary>
      /// Whether the object is a defintion too, ie is a fu
      /// </summary>
      bool IsDefinition { get; }
   }
}
