using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using static Gate.CLanguage.Runtime.Object.CRtmObjAllocator;

namespace Gate.CLanguage.Runtime.Object
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class CRtmObj : RtmObj
   {
      /// <summary>
      /// Object created by allocator is responsibile for deallocation of value.
      /// </summary>
      /// <param name="allocator"></param>
      /// <param name="decl"></param>
      protected CRtmObj(CRtmObjAllocator allocator, IDecl decl) : base(decl)
      {
         Allocator = allocator;
         Allocation = myPerformAllocation();
         Address = Allocation.Pointer;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="allocator"></param>
      /// <param name="declType"></param>
      protected CRtmObj(CRtmObjAllocator allocator, IDeclType declType) : base(declType)
      {
         Allocator = allocator;
         Allocation = myPerformAllocation();
         Address = Allocation.Pointer;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="address"></param>
      /// <param name="decl"></param>
      protected CRtmObj(IntPtr address, IDecl decl) : base(decl) => Address = address;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="address"></param>
      /// <param name="declType"></param>
      protected CRtmObj(IntPtr address, IDeclType declType) : base(declType) => Address = address;

      public abstract int SizeOf { get; }

      public abstract void CopyTo(CRtmObj destination);

      public override IntPtr? Address { get; }

      public CRtmObjAllocator? Allocator { get; private set; }
      
      public Allocation? Allocation { get; }

      public override RtmFormat CurrentFormat => CurrentCFormat;

      public static CRtmFormat CurrentCFormat { get; set; } = new CRtmFormat();

      protected override void myFreeUnmanaged()
      {
         if (Allocation?.Allocator != null)
         {
            Allocation.Allocator.FreeAllocation(Allocation);
         }
      }

      protected override void myFreeManaged() { }

      private Allocation myPerformAllocation() =>
         Allocator != null ? Allocator.Allocate(SizeOf) : throw new Crash();
   }
}

