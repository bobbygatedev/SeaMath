namespace Gate.CLanguage.Types
{
   /// <summary>
   /// Body of struct/union/class.
   /// </summary>
   public interface ITypeClassBody
   {
      ITypeClass? ParentClass { get; }
   }
}