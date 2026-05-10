using Gate.LangBase.ExtraTypes;
using Gate.Tools;
using System.Runtime.InteropServices;

namespace Gate.CLanguage.Types
{
   /// <summary>
   /// 
   /// </summary>
   public class CTypeBuiltIn : CTypePrimitive
   {
      private string myTypeSpecifier;
      private readonly Type myCSharpType;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="representedType"></param>
      /// <param name="isUnsigned"></param>
      /// <param name="typeSpecifier"></param>
      /// <param name="signature"></param>
      /// <param name="sizeOf"></param>
      public CTypeBuiltIn(CTypeBuiltInRepresent representedType, bool isUnsigned, string typeSpecifier, int sizeOf) :
         base(isUnsigned ? $"unsigned {typeSpecifier}" : typeSpecifier)
      {
         SizeOf = sizeOf;
         myTypeSpecifier = typeSpecifier;
         IsUnsigned = isUnsigned;
         RepresentedType = representedType;
         myCSharpType = myGetDotNetType(RepresentedType, SizeOf, isUnsigned) ?? throw new Crash();
         SizeOf = Marshal.SizeOf(myCSharpType);///overrides SizeOf (case of <see cref="LongDouble"/> <see cref="ComplexLongDouble"/>
      }

      /// <summary>
      /// 
      /// </summary>
      public override int SizeOf { get; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsUnsigned { get; }

      /// <summary>
      /// 
      /// </summary>
      public CTypeBuiltInRepresent RepresentedType { get; }

      /// <summary>
      /// 
      /// </summary>
      public override bool IsUserDefined => false;

      public override bool IsBuiltIn => true;

      public override bool IsClass => false;

      public override string TypeSpecifier => myTypeSpecifier;

      public override string DescriptorGcc
      {
         get
         {
            switch (RepresentedType)
            {
               case CTypeBuiltInRepresent.@void: return "v";
               case CTypeBuiltInRepresent.integer: return myGetDescriptorForInt(SizeOf);

               case CTypeBuiltInRepresent.floating_point:
                  if (SizeOf == 4) { return "f"; }
                  else if (SizeOf == 8) { return "d"; }
                  else if (SizeOf == 16) { return "e"; }
                  else { throw new Crash(); }

               case CTypeBuiltInRepresent.complex_float:
                  if (SizeOf == 4*2) { return "cf"; }
                  else if (SizeOf == 8*2) { return "cd"; }
                  else if (SizeOf == 16*2) { return "ce"; }
                  else { throw new Crash(); }

               case CTypeBuiltInRepresent.complex_int:
                  return $"c{myGetDescriptorForInt(SizeOf / 2)}";

               case CTypeBuiltInRepresent.boolean: return "b";

               default: throw new Crash();
            }
         }
      }

      private unsafe string myGetDescriptorForInt(int sizeOf)
      {
         if (IsLong) { return IsUnsigned ? "m" : "l"; }//long;
         else if (sizeOf == 1) { return IsUnsigned ? "h" : "c"; }//char
         else if (sizeOf == 2) { return IsUnsigned ? "t" : "s"; }//short
         else if (sizeOf == 4) { return IsUnsigned ? "j" : "i"; }//int 
         else if (sizeOf == 8) { return IsUnsigned ? "y" : "x"; }//long long
         else { throw new Crash(); }
      }

      /// <summary>
      /// True if built-in is an integer containing long descriptor 
      /// </summary>
      public bool IsLong => TypeSpecifier.Split(' ').Count(w => w == "long") == 1;

      public bool IsLongLong => TypeSpecifier.Split(' ').Count(w => w == "long") == 2;

      public override Type CSharpTypeForStorage => myCSharpType;

      public override CTypeBuiltIn? BuiltIn => this;

      public override string Descriptor => TypeSpecifier;

      public override bool IsEnum => false;

      public override bool IsConstant => false;

      public override string ToString() => RepresentedType == CTypeBuiltInRepresent.@void ? "void" : $"{TypeSpecifier}({RepresentedType}-{SizeOf * 8}bit)";

      private static Type myGetDotNetType(CTypeBuiltInRepresent representedType, int sizeOf, bool isUnsigned)
      {
         switch (representedType)
         {
            case CTypeBuiltInRepresent.@void: return typeof(void);

            case CTypeBuiltInRepresent.integer:
               if (isUnsigned)
               {
                  switch (sizeOf)
                  {
                     case 1: return typeof(byte);
                     case 2: return typeof(UInt16);
                     case 4: return typeof(UInt32);
                     case 8: return typeof(UInt64);
                     default: throw new Crash();
                  }
               }
               else
               {
                  switch (sizeOf)
                  {
                     case 1: return typeof(sbyte);
                     case 2: return typeof(Int16);
                     case 4: return typeof(Int32);
                     case 8: return typeof(Int64);
                     default: throw new Crash();
                  }
               }

            case CTypeBuiltInRepresent.floating_point:
               switch (sizeOf)
               {
                  case 4: return typeof(float);
                  case 8: return typeof(double);
                  case 16: return typeof(LongDouble);
                  default: throw new Crash();
               }

            case CTypeBuiltInRepresent.complex_float:
               switch (sizeOf)
               {
                  case 8: return typeof(ComplexFloat);
                  case 16: return typeof(ComplexDouble);
                  case 32: return typeof(ComplexLongDouble);
                  default: throw new Crash();
               }

            case CTypeBuiltInRepresent.complex_int:
               switch (sizeOf)
               {
                  case 2: return isUnsigned ? typeof(ComplexUint8) : typeof(ComplexInt8);
                  case 4: return isUnsigned ? typeof(ComplexUint16) : typeof(ComplexInt16);
                  case 8: return isUnsigned ? typeof(ComplexUint32) : typeof(ComplexInt32);
                  case 16: return isUnsigned ? typeof(ComplexUint64) : typeof(ComplexInt64);
                  default: throw new Crash();
               }

            case CTypeBuiltInRepresent.boolean: return typeof(byte);

            default: throw new Crash();
         }
      }
   }
}
