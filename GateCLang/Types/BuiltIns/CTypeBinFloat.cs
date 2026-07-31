namespace Gate.CLanguage.Types.BuiltIns
{
   public class CTypeBinFloat : CTypeBuiltIn
   {
      protected CTypeBinFloat(string typeSpecifier, int sizeOf) : base(CTypeBuiltInRepresent.floating_point, false, typeSpecifier, sizeOf) { }

      public class LongDouble : CTypeBinFloat
      {
         public LongDouble(int sizeOf) : base("long double", sizeOf) { }
      }

      public class Single : CTypeBinFloat
      {
         public Single() : base("float", 4) { }
      }

      public class Double : CTypeBinFloat
      {
         public Double() : base("double", 8) { }
      }
   }
}
