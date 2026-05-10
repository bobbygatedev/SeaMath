using Gate.LangBase.Expressions;
using System;

namespace Gate.CLanguage.Runtime.Object
{
   /// <summary>
   /// C-Rtm object for literal constant don't use allocator and have not a valid address
   /// </summary>
   public class CRtmObjLiteral : CRtmObjScalar
   {
      private ValueType myLiteralConstValue;

      public CRtmObjLiteral(ValueType literalConstValue, IDeclType declType) : base(IntPtr.Zero, declType)
      {
         myLiteralConstValue = literalConstValue;
         IsConstant = true;
      }

      public override ValueType? CSharpObj
      {
         get => myLiteralConstValue;
         set => throw new Gate.LangBase.Runtime.RtmException($"Can't set a literal constant!");
      }
   }
}
