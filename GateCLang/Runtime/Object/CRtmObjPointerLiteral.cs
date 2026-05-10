using Gate.LangBase.Expressions;

namespace Gate.CLanguage.Runtime.Object
{
   /// <summary>
   /// Run-time object for literal constant don't use allocator and have not a valid address
   /// </summary>
   public class CRtmObjPointerLiteral : CRtmObjPointer
   {
      private IntPtr myPointerConstValue;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="rtmStrategy"></param>
      /// <param name="pointedType"></param>
      /// <param name="pointedValue"></param>
      /// <param name="declType"></param>
      public CRtmObjPointerLiteral(ICRtmObjStrategy rtmStrategy, IntPtr pointedValue, IDeclType declType)
         : base(rtmStrategy , pointedValue , declType)
      {
         myPointerConstValue = pointedValue;
         IsConstant = true;
      }

      public override IntPtr PointerValue
      {
         get => myPointerConstValue;
         set => throw new Gate.LangBase.Runtime.RtmException("Can't set Pointer Value");
      }

      public override ValueType? CSharpObj
      {
         get => myPointerConstValue;
         set => throw new Gate.LangBase.Runtime.RtmException("Can't set a constant");
      }
   }
}
