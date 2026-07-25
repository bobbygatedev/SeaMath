using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Types;
using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.Object;
using Gate.Tools.Extensions;
using System.Runtime.InteropServices;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// Represents a "sea" object in the runtime, which can hold any RTM object as its value. 
   /// The sea is a special type that can contain any other type of value, and it is identified by a unique object ID. 
   /// The content of the sea is defined by the SeaTypeContent struct, which includes a tag and a counter for tracking the number of sea objects created.
   /// </summary>
   public unsafe class SeaTypeRtmObj : CRtmObjScalar
   {
      private RtmObj? myRtmValue;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="allocator"></param>
      /// <param name="decl"></param>
      public SeaTypeRtmObj(CRtmObjAllocator allocator, IDecl decl, RtmObj? rtmValue = null) :
         base(allocator, decl)
      {
         myInit();

         if (rtmValue != null)
         {
            RtmValue = rtmValue;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="allocator"></param>
      /// <param name="rtmValue"></param>
      public SeaTypeRtmObj(CRtmObjAllocator allocator, RtmObj? rtmValue = null) :
         base(allocator, new CTypeAlias(SeaType.Instance))
      {
         myInit();
         RtmValue = rtmValue;
      }

      /// <summary>
      /// Gets or sets the content of the sea, which is represented by the SeaTypeContent struct. 
      /// The content includes a tag to identify the type of the sea and a counter to track the number of sea objects created. 
      /// The content is stored in unmanaged memory and accessed using marshaling to convert between the struct and the pointer representation.
      /// </summary>
      public SeaTypeContent Content
      {
         get => Marshal.PtrToStructure<SeaTypeContent>(Address ?? 0);
         set => Marshal.StructureToPtr(value, Address ?? 0, false);
      }

      /// <summary>
      /// Csharp object if object is !<see cref="Empty"/>
      /// </summary>
      public override ValueType? CSharpObj => RtmValue?.CSharpObj;

      /// <summary>
      /// 
      /// </summary>
      public RtmObj? RtmValue
      {
         get => myRtmValue;

         set => myRtmValue = value is SeaTypeRtmObj ?
               throw new Gate.LangBase.Runtime.RtmException($"A sea value can't be rtm-value of a sea") : value;
      }

      /// <summary>
      /// 
      /// </summary>
      public override int SizeOf => Marshal.SizeOf(typeof(SeaTypeContent));

      /// <summary>
      /// 
      /// </summary>
      public override string DisplayValue => RtmValue == null ?
         $"Sea(id={ObjectId}):Empty" :
         $"Sea(id={ObjectId}):{RtmValue.DisplayValue}";

      /// <summary>
      /// 
      /// </summary>
      public bool IsEmpty { get => RtmValue == null; }

      /// <summary>
      /// 
      /// </summary>
      public uint ObjectId => Content.Counter;

      /// <summary>
      /// 
      /// </summary>
      public static uint ObjectCreatedCounter { get; private set; }

      public string? AsString
      {
         get => this.GetRtmArrayFromSea()?.AsString;

         set
         {
            var arr = this.GetRtmArrayFromSea();

            if (arr != null)
            {
               arr.AsString = value;
            }
         }
      }

      private void myInit()
      {
         var cnt_p = (SeaTypeContent*)Address.NnOrCrash();

         cnt_p->Tag = SeaTypeContent.TAG;
         cnt_p->Counter = ++ObjectCreatedCounter;
      }
   }
}
