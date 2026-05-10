using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.Object;
using System.Text;

namespace Gate.CLanguage.Runtime.Object
{
   /// <summary>
   /// Interface for copying with pointer/array duality (<see cref="CRtmObjPointer"/>, <see cref="CRtmObjArray"/>)
   /// </summary>
   public interface ICRtmObjPointer
   {
      /// <summary>
      ///  
      /// </summary>
      IntPtr PointerValue { get; set; }

      /// <summary>
      /// 
      /// </summary>
      IDeclType DereferencedType { get; }

      /// <summary>
      /// 
      /// </summary>
      IDeclType? ItemType { get; }

      /// <summary>
      /// 
      /// </summary>
      CRtmObj Dereference { get; }

      /// <summary>
      /// 
      /// </summary>
      string? AsString { get; set; }

      /// <summary>
      /// 
      /// </summary>
      Encoding? StringEncoding { get; set; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="indices"></param>
      /// <returns></returns>
      RtmObj this[params int[] indices] { get; }
   }
}