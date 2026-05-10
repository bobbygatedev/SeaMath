using Gate.LangBase;
using Gate.LangBase.Expressions;
using Gate.Tools;
using System.Runtime.InteropServices;
using System.Text;

namespace Gate.CLanguage.Runtime.Object
{
   /// <summary>
   /// 
   /// </summary>
   public unsafe class CRtmObjLiteralString : CRtmObjArray
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="rtmStrategy"></param>
      /// <param name="declType"></param>
      /// <param name="itemType"></param>
      /// <param name="generalizedString">String without \"</param>
      public CRtmObjLiteralString(
         ICRtmObjStrategy rtmStrategy, IDeclType declType, GeneralizedString generalizedString) :
         base(rtmStrategy, declType, [ generalizedString.UInt32Array.Length])
      {
         GeneralizedString = generalizedString;

         var bys = GeneralizedString.Bytes;

         fixed (byte* ptr = bys)
         {
            Marshal.Copy(bys, 0, Address ?? throw new Crash(), bys.Length);
         }
      }

      public override Encoding? StringEncoding
      {
         get => GeneralizedString.Encoding;
         set => throw new Gate.LangBase.LangBaseException($"Setting a {GetType().Name} not allowed");
      }

      /// <summary>
      /// 
      /// </summary>
      public GeneralizedString GeneralizedString { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override string ToString() => $"Nude: \"{GeneralizedString}\"({StringEncoding})";
   }
}
