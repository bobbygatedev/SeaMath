using Gate.CLanguage.Types;
using Gate.LangBase;
using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Arry;
using System.Text;

namespace Gate.CLanguage.Runtime.Object
{
   /// <summary>
   /// Represents a C-style array runtime object. 
   /// It provides properties and methods to access and manipulate the array elements, as well as to convert the array to a string using a specified encoding. 
   /// The class implements the ICRtmObjArray interface, which defines the contract for C-style array runtime objects. 
   /// </summary>
   public unsafe class CRtmObjArray : CRtmObj, ICRtmObjArray
   {
      private Encoding? myStringEncoding;

      /// <summary>
      /// Constructor allocated with forced sizes.
      /// </summary>
      /// <param name="rtmStrategy"></param>
      /// <param name="declType"></param>
      /// <param name="sizes"></param>
      public CRtmObjArray(ICRtmObjStrategy rtmStrategy, IDeclType declType, int[]? sizes)
         : base(rtmStrategy.Allocator, declType)
      {
         RtmStrategy = rtmStrategy;
         ForcedSizes = sizes;
         DereferencedType = myGetDereferencedType();
      }

      /// <summary>
      /// Initializes a new instance of the CRtmObjArray class using the specified RTM strategy, allocator context, and
      /// declaration.
      /// </summary>
      /// <param name="rtmStrategy">The RTM object strategy to use for allocation and type management. Cannot be null.</param>
      /// <param name="decl">The declaration information that describes the array type. Cannot be null.</param>
      public CRtmObjArray(ICRtmObjStrategy rtmStrategy, IDecl decl) :
         base(rtmStrategy.Allocator, decl)
      {
         RtmStrategy = rtmStrategy;
         DereferencedType = myGetDereferencedType();
      }

      /// <summary>
      /// Constructor by address.
      /// </summary>
      /// <param name="rtmStrategy"></param>
      /// <param name="address"></param>
      /// <param name="declType"></param>
      public CRtmObjArray(ICRtmObjStrategy rtmStrategy, IntPtr address, IDeclType declType)
         : base(address, declType)
      {
         RtmStrategy = rtmStrategy;
         DereferencedType = myGetDereferencedType();
      }

      /// <summary>
      /// Constructor by reference.
      /// </summary>
      /// <param name="arrayType"></param>
      /// <param name="address"></param>
      public CRtmObjArray(ICRtmObjStrategy rtmStrategy, IntPtr address, IDecl decl) :
         base(address, decl)
      {
         RtmStrategy = rtmStrategy;
         DereferencedType = myGetDereferencedType();
      }

      /// <summary>
      /// Type of items in array.
      /// </summary>
      public CTypeAlias? ItemType => this.GetTypeAlias()?.PrimitiveAlias.ArrayItemType;

      /// <summary>
      /// Csharp object (pointer) associated to c-style-array.
      /// </summary>
      /// <exception cref="Gate.LangBase.Runtime.RtmException">Set not implemeneted</exception>
      public override ValueType? CSharpObj
      {
         get => Address;
         set => throw new Crash($"Can't set an allocated c# obejct for {GetType().Name}");
      }

      /// <summary>
      /// Size of whole array in bytes (size of item * count of items).
      /// </summary>
      public override int SizeOf => (ItemType?.SizeOf ?? -1) * Sizes.Aggregate((s1, s2) => s1 * s2);

      /// <summary>
      /// Depth of array (= number of indices)
      /// </summary>
      public int Depth => Sizes.Length;

      /// <summary>
      /// Returns the total number of items in the array, calculated by multiplying the sizes of each dimension together.
      /// </summary>
      public int AllItemsCount => Sizes.Aggregate((s1, s2) => s1 * s2);

      /// <summary>
      /// Returns offset to <see cref="RtmObj.Address"/> of indices counted in items (item sizeof not taken into account).
      /// eg sizes { 2  3 } <see cref="GetItemOffset(int[])"/>(1 2) => 1*3 + 2*1 = 
      /// </summary>
      /// <param name="indices">The indices of the element for which to calculate the offset.</param>
      /// <returns></returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
      public int GetItemOffset(params int[] indices)
      {
         if (indices.Length > Sizes.Length) { throw new Gate.LangBase.Runtime.RtmException($"Indices lenght({indices.Length}) > sizes length{Sizes.Length}"); }
         else
         {
            var rng = Enumerable.Range(0, indices.Length).ToArray();

            if (rng.Any(i => indices[i] > Sizes[i]))
            {
               var idx = rng.First(i => indices[i] > Sizes[i]);

               throw new Gate.LangBase.Runtime.RtmException($"Index #{idx} {indices[idx]} > size({Sizes[idx]})");
            }

            //eg sizes = 2 3 4 => {12 4} = {4*3 4}  
            var s_a = Enumerable.Range(1, Sizes.Length - 1).Select(i => Sizes.Reverse().Take(i).Aggregate((p1, p2) => p1 * p2)).Reverse().Append(1).ToArray();

            // sum_of(indices[i]*s_a[i])
            return Enumerable.Range(0, indices.Length).Select(i => s_a[i] * indices[i]).Aggregate((s1, s2) => s1 + s2);
         }
      }

      /// <summary>
      /// Copies the contents of this array to the specified destination array.
      /// </summary>
      /// <param name="destination">The destination array to copy the contents to.</param>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
      public override void CopyTo(CRtmObj destination) => throw new Gate.LangBase.Runtime.RtmException($"Can't copy from {GetType().Name}");

      /// <summary>
      /// Returns the element at the specified indices in the C-style array. 
      /// The indices are provided as a variable-length parameter list, allowing for multi-dimensional arrays. 
      /// The method uses the RTM strategy to retrieve the array item based on the provided indices and returns it as a CRtmObj.
      /// </summary>
      /// <param name="indices">The indices of the element to retrieve.</param>
      /// <returns>The element at the specified indices as a CRtmObj.</returns>
      public CRtmObj this[params int[] indices] => (CRtmObj)RtmStrategy.GetArrayItem(this, indices);

      /// <summary>
      /// Gets or sets the string representation of the C-style array.
      /// </summary>
      public unsafe string? AsString
      {
         get => this.Convert2String();

         set => value?.Convert2IRtmPointer(this);
      }

      /// <summary>
      /// Returns a C# array that represents the elements of the C-style array. 
      /// The type of the items in the array is determined by the <see cref="CSharpItemType"/> property, and the dimensions of the array are determined by the <see cref="Sizes"/> property. Each element in the resulting C# array is obtained by accessing the corresponding element in the C-style array using the indexer and retrieving its C# object representation. 
      /// This allows for easy manipulation and access to the elements of the C-style array in a more familiar C# array format.
      /// </summary>
      public Array AsArray
      {
         get
         {
            var res = Array.CreateInstance(CSharpItemType ?? throw new RtmException(), Sizes);
            var ids = new ArrayIndicesEnumerable(ArrayIndicesEnumerable.DirectionId.right2left, Sizes);

            foreach (var idx in ids)
            {
               res.SetValue(this[idx].CSharpObj, idx);
            }

            return res;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public virtual Encoding? StringEncoding
      {
         get => myStringEncoding ?? GeneralizedString.GetEncoding(Address ?? 0, DereferencedType.SizeOf);

         set => myStringEncoding = value;
      }

      IntPtr ICRtmObjPointer.PointerValue
      {
         get => Address ?? 0;
         set => throw new Gate.LangBase.Runtime.RtmException($"Can't set pointer value of an {GetType().Name} object!");
      }

      public int[] Sizes => ForcedSizes ?? DeclType?.ArraySizes ?? [];

      public ICRtmObjStrategy RtmStrategy { get; }

      public int[]? ForcedSizes { get; }

      public Type? CSharpItemType => DereferencedType?.PrimitiveAlias?.ArrayItemType?.CSharpArrayItemType;

      public int ItemSizeOf => ItemType?.SizeOf ?? throw new RtmException();

      public CRtmObj Dereference => RtmStrategy.Dereference(this);

      public CTypeAlias DereferencedType { get; }

      IDeclType ICRtmObjPointer.DereferencedType => DereferencedType;

      IDeclType? ICRtmObjPointer.ItemType => ItemType;

      RtmObj ICRtmObjPointer.this[params int[] indices] => this[indices];

      private CTypeAlias myGetDereferencedType() =>
         RtmStrategy.DereferenceType(DeclType ?? throw new Crash()) as CTypeAlias ?? throw new Crash();
   }
}
