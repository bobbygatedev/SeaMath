using Gate.CLanguage.Decl;
using Gate.CLanguage.Types;
using Gate.LangBase;
using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using System.Text;

namespace Gate.CLanguage.Runtime.Object
{
   /// <summary>
   /// 
   /// </summary>
   public unsafe class CRtmObjPointer : CRtmObjScalar, ICRtmObjPointer
   {
      private Encoding? myStringEncoding;

      /// <summary>
      /// Constructor.
      /// </summary  
      /// <param name="rtmStrategy"></param>
      /// <param name="decl"></param>
      public CRtmObjPointer(ICRtmObjStrategy rtmStrategy, IDecl decl)
         : base(rtmStrategy.Allocator, decl)
      {
         RtmStrategy = rtmStrategy;
         DereferencedType = myGetDereferencedType();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="rtmStrategy"></param>
      /// <param name="address"></param>
      /// <param name="decl"></param>
      public CRtmObjPointer(ICRtmObjStrategy rtmStrategy, IntPtr address, IDecl decl) : base(address, decl)
      {
         RtmStrategy = rtmStrategy;
         DereferencedType = myGetDereferencedType();
      }

      /// <summary>
      /// Constructor.
      /// </summary  
      /// <param name="rtmStrategy"></param>
      /// <param name="address"></param>
      /// <param name="declType"></param>
      public CRtmObjPointer(ICRtmObjStrategy rtmStrategy, IntPtr address, IDeclType declType) : base(address, declType)
      {
         RtmStrategy = rtmStrategy;
         DereferencedType = myGetDereferencedType();
      }

      /// <summary>
      /// Constructor.
      /// </summary  
      /// <param name="rtmStrategy"></param>
      /// <param name="declType"></param>
      public CRtmObjPointer(ICRtmObjStrategy rtmStrategy, IDeclType declType) :
         base(rtmStrategy.Allocator, declType)
      {
         RtmStrategy = rtmStrategy;
         DereferencedType = myGetDereferencedType();
      }

      /// <summary>
      /// 
      /// </summary>
      public bool IsFunctionPointer => Decl is CDeclFunction;

      /// <summary>
      /// 
      /// </summary>
      public virtual unsafe IntPtr PointerValue
      {
         get => new IntPtr(*(void**)(Address ?? nint.Zero));
         set => *(void**)(Address ?? nint.Zero) = value.ToPointer();
      }

      /// <summary>
      /// 
      /// </summary>
      public ICRtmObjStrategy RtmStrategy { get; }


      /// <summary>
      /// 
      /// </summary>
      public CType? ItemType => DereferencedType.TypeBase;

      /// <summary>
      /// 
      /// </summary>
      public unsafe override ValueType? CSharpObj
      {
         get => PointerValue;
         set => PointerValue = (nint)(value ?? nint.Zero);
      }

      /// <summary>
      /// 
      /// </summary>
      public CRtmObj Dereference => RtmStrategy.Dereference(this);

      /// <summary>
      /// 
      /// </summary>
      public string? AsString
      {
         get => this.Convert2String();

         set => value?.Convert2IRtmPointer(this);
      }

      /// <summary>
      /// 
      /// </summary>
      public Encoding? StringEncoding
      {
         get => myStringEncoding ?? GeneralizedString.GetEncoding(PointerValue, DereferencedType.SizeOf);

         set => myStringEncoding = value;
      }

      /// <summary>
      /// 
      /// </summary>
      public CTypeAlias DereferencedType { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="indices"></param>
      /// <returns></returns>
      public CRtmObj this[params int[] indices] => (CRtmObj)RtmStrategy.GetArrayItem(this, indices);

      /// <summary>
      /// 
      /// </summary>
      IDeclType? ICRtmObjPointer.ItemType => ItemType;

      /// <summary>
      /// 
      /// </summary>
      IDeclType ICRtmObjPointer.DereferencedType => DereferencedType;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="indices"></param>
      /// <returns></returns>
      RtmObj ICRtmObjPointer.this[params int[] indices] => this[indices];

      private CTypeAlias myGetDereferencedType() =>
         RtmStrategy.DereferenceType(DeclType ?? throw new Crash()) as CTypeAlias ?? throw new Crash();
   }
}
