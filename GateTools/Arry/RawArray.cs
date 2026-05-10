using Gate.Tools.DesignPattern;
using System.Runtime.InteropServices;

namespace Gate.Tools.Arry
{
   /// <summary>
   /// 
   /// </summary>
   public class RawArray : BaseClassWithFinalizer
   {
      public RawArray(IntPtr address, Type itemType, params int[] sizes)
      {
         ItemType = itemType;
         Sizes = sizes.ToArray();
         Address = address;
         IsAllocated = false;
      }

      public RawArray(Type itemType, params int[] sizes)
      {
         ItemType = itemType;
         Sizes = sizes.ToArray();
         Address = Marshal.AllocHGlobal(Capacity);
         IsAllocated = true;
      }


      public IntPtr Address { get; }
      public bool IsAllocated { get; }

      public int ItemSize => Marshal.SizeOf(ItemType);

      public int TotalItemCount => Sizes == null || Sizes.Length == 0 ? 0 : Sizes.Aggregate((i1, i2) => i2 * i2);

      public int Capacity => ItemSize * TotalItemCount;

      public ValueType? this[params int[] indices]
      {
         get
         {
            var ptr = GetItemPtr(indices);

            return Marshal.PtrToStructure(ptr, ItemType) as ValueType;
         }

         set
         {
            var ptr = GetItemPtr(indices);

            Marshal.StructureToPtr(value ?? throw new Crash("Can't be null"), ptr, false);
         }
      }

      public unsafe IntPtr GetItemPtr(params int[] indices)
      {
         if (indices.Length == Sizes.Length && Enumerable.Range(0, indices.Length).All(i => indices[i] >= 0 && indices[i] < Sizes[i]))
         {
            var ptr = (byte*)Address;

            for (int i = 0; i < indices.Length; i++)
            {
               var bl_siz = i < indices.Length - 1 ? Sizes.Skip(1 + i).Sum() : 1;

               ptr += bl_siz * ItemSize + indices[i];
            }

            return (IntPtr)ptr;
         }
         else
         {
            throw new Gate.Tools.ToolsException("Indices to be as of same size as Sizes and all its value i[i]>=0 && i[i] < Sizes[i]");
         }
      }

      protected override void myFreeManaged() { }

      protected override void myFreeUnmanaged()
      {
         if (IsAllocated)
         {
            Marshal.FreeHGlobal(Address);
         }
      }

      public Type ItemType { get; }

      public int[] Sizes { get; }
   }
}
