namespace Gate.CLanguage.Types.BuiltIns
{
   /// <summary>
   /// 
   /// </summary>
   public class CTypeBinBool : CTypeBinInt
   {
      public const string C99_TYPE_SPEC = "_Bool";
      public const string CPP_TYPE_SPEC = "bool";

      public CTypeBinBool(string typeSpecifier) : base(typeSpecifier) { }
   }
}
