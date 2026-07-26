using Gate.LangBase;
using Gate.LangBase.Expressions;
using Gate.Tools;
using Gate.Tools.Extensions;
using System.Runtime.InteropServices;
using static Gate.LangBase.NumericConverter;

namespace Gate.CLanguage.Runtime.Object
{
   /// <summary>
   /// The CRtmObjScalar class represents a scalar object in the C language runtime. 
   /// It is responsible for managing the memory and value of a scalar variable, which can be a primitive type (such as int, float, char) or an enumeration. 
   /// The class provides functionality for reading and writing the value of the scalar variable, as well as handling bit fields if applicable. 
   /// It also includes type analysis to determine the appropriate C# type for storage based on the declaration of the scalar variable.
   /// </summary>
   public class CRtmObjScalar : CRtmObj
   {
      /// <summary>
      /// Initializes a new instance of the CRtmObjScalar class with the specified allocator, declaration, and allocator
      /// context.
      /// </summary>
      /// <param name="allocator">The allocator used to manage memory for the object instance.</param>
      /// <param name="decl">The declaration that defines the type and metadata for the scalar object.</param>
      public CRtmObjScalar(CRtmObjAllocator allocator, IDecl decl) :
         base(allocator, decl) =>
         TypeDescriptor = TypeAnalise(CSharpTypeForStorage = DeclType?.CSharpTypeForStorage ?? throw new Crash());

      /// <summary>
      /// Initializes a new instance of the CRtmObjScalar class with the specified allocator, declaration type, and allocator context.
      /// </summary>
      /// <param name="allocator">The allocator used to manage memory for the object instance.</param>
      /// <param name="declType">The declaration type that defines the type and metadata for the scalar object.</param>
      public CRtmObjScalar(CRtmObjAllocator allocator, IDeclType declType) : base(allocator, declType) =>
         TypeDescriptor = TypeAnalise(CSharpTypeForStorage = DeclType?.CSharpTypeForStorage ?? throw new Crash());

      /// <summary>
      /// Initializes a new instance of the CRtmObjScalar class with the specified memory address, declaration and optional bit field information.
      /// </summary>
      /// <param name="address"></param>
      /// <param name="decl"></param>
      /// <param name="bitField"></param>
      public CRtmObjScalar(IntPtr address, IDecl decl, BitField? bitField = null) : base(address, decl)
      {
         BitField = bitField;
         TypeDescriptor = TypeAnalise(CSharpTypeForStorage = DeclType?.CSharpTypeForStorage ?? throw new Crash());
      }

      /// <summary>
      /// Initializes a new instance of the CRtmObjScalar class with the specified memory address, declaration type, and optional bit field information.
      /// </summary>
      /// <param name="address"></param>
      /// <param name="declType"></param>
      public CRtmObjScalar(IntPtr address, IDeclType declType) : base(address, declType) =>
         TypeDescriptor = TypeAnalise(CSharpTypeForStorage = DeclType?.CSharpTypeForStorage ?? throw new Crash());

      /// <summary>
      /// 
      /// </summary>
      /// <param name="destination"></param>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
      public override void CopyTo(CRtmObj destination) =>
         CSharpObj = CSharpObj?.GetType() == destination.CSharpObj?.GetType() ?
            destination.CSharpObj :
            throw new Gate.LangBase.Runtime.RtmException($"Can't copy {GetType().Name} to {GetType().Name}");

      /// <summary>
      /// 
      /// </summary>
      public Type CSharpTypeForStorage { get; }


      /// <summary>
      /// 
      /// </summary>
      public override int SizeOf => DeclType?.SizeOf ?? -1;

      /// <summary>
      /// 
      /// </summary>
      public override ValueType? CSharpObj
      {
         get
         {
            var nc = NumericConverter.StdImpl ?? throw new NullReferenceException();

            return DeclType?.CSharpTypeForStorage != null ?
            (BitField.HasValue ? nc.GetBitField(myGetValue(), BitField.Value) : myGetValue()) : null;
         }

         set
         {
            if (IsConstant) { throw new Gate.LangBase.Runtime.RtmException("Can't assign to constant"); }
            else if (value?.GetType() == CSharpTypeForStorage)
            {
               if (BitField.HasValue)
               {
                  var new_val = StdImpl.NnOrCrash().UpdateBitField(myGetValue(), value, BitField.Value);

                  mySetValue(new_val ?? throw new Crash());
               }
               else { mySetValue(value); }
            }
            else { throw new Gate.LangBase.Runtime.RtmException("Can't assign to a different type"); }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public TypeAnalyzeResult TypeDescriptor { get; }

      /// <summary>
      /// 
      /// </summary>
      public BitField? BitField { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      private ValueType myGetValue() => Marshal.PtrToStructure(Address ?? 0, CSharpTypeForStorage) as ValueType ?? 0;

      /// <summary>
      /// Set A NOT BIT FIELD value on memory.
      /// </summary>
      /// <param name="value"></param>
      private void mySetValue(ValueType value) => Marshal.StructureToPtr(value, Address ?? 0, false);
   }
}
