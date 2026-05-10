using Gate.LangBase.Expressions;
using Gate.Tools;
using System.Runtime.InteropServices;

namespace Gate.CLanguage.Runtime.Object
{
   /// <summary>
   /// 
   /// </summary>
   public unsafe class CRtmObjRecord : CRtmObj
   {
      private CRtmObj[]? myFields;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="rtmStrategy"></param>
      /// <param name="decl"></param>
      public CRtmObjRecord(ICRtmObjStrategy rtmStrategy, IDecl decl) :
         base(rtmStrategy.Allocator, decl) => RtmStrategy = rtmStrategy;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="rtmStrategy"></param>
      /// <param name="declType"></param>
      public CRtmObjRecord(ICRtmObjStrategy rtmStrategy, IDeclType declType) :
         base(rtmStrategy.Allocator, declType) =>
         RtmStrategy = rtmStrategy;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="rtmStrategy"></param>
      /// <param name="address"></param>
      /// <param name="declType"></param>
      public CRtmObjRecord(ICRtmObjStrategy rtmStrategy, IntPtr address, IDeclType declType) : base(address, declType) => RtmStrategy = rtmStrategy;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="rtmStrategy"></param>
      /// <param name="address"></param>
      /// <param name="exprDeclRecord"></param>
      public CRtmObjRecord(ICRtmObjStrategy rtmStrategy, IntPtr address, IDecl exprDeclRecord) : base(address, exprDeclRecord) => RtmStrategy = rtmStrategy;

      /// <summary>
      /// 
      /// </summary>
      public CRtmObj[] RtmFields
      {
         get
         {
            if (myFields == null)
            {
               myFields = (CRtmObj[])RtmStrategy.GetRecordMembers(this);
            }

            return myFields;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public ICRtmObjStrategy RtmStrategy { get; }

      /// <summary>
      /// 
      /// </summary>
      public override int SizeOf => DeclType?.SizeOf ?? -1;

      /// <summary>
      /// 
      /// </summary>
      public override ValueType? CSharpObj { get => null; set => throw new Crash(); }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="recordField"></param>
      /// <returns></returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"> <paramref name="recordField"/> not existing as member</exception>
      public CRtmObj? this[string recordField] => RtmFields.FirstOrDefault(m => m.Decl?.Identifier == recordField);

      public override void CopyTo(CRtmObj? destination)
      {
         if (DeclType != null && DeclType.IsEquivalent(destination?.DeclType ?? throw new Crash()))
         {
            var d_p = (byte*)(destination?.Address ?? nint.Zero);
            var s_p = (byte*)(Address ?? nint.Zero);

            NativeMemory.Copy(s_p, d_p, (nuint)SizeOf);
         }
      }
   }
}
