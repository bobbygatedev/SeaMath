using Gate.LangBase;
using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.Object;

namespace Gate.CLanguage.Runtime.Object
{
   /// <summary>
   /// 
   /// </summary>
   public interface ICRtmObjStrategy : IRtmObjStrategy
   {
      /// <summary>
      /// 
      /// </summary>
      CRtmObjAllocator Allocator { get; }

      /// <summary>
      /// 
      /// </summary>
      NumericConverter NumericConverter { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="rtmObjPointer"></param>
      /// <returns></returns>
      CRtmObj Dereference(RtmObj rtmObjPointer);
   
      /// <summary>
      /// 
      /// </summary>
      /// <param name="declType"></param>
      /// <returns></returns>
      IDeclType DereferenceType(IDeclType declType);
   }
}

