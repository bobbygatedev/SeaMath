using System.Runtime.InteropServices;

namespace Gate.Tools
{
   public unsafe static class BitFieldHelper
   {
      public const UInt64 AllBits = ~0x0ul;

      private static dynamic myGetBitField(dynamic input, int offset, int numBits)
      {
         unchecked
         {
            var sz_bts = Marshal.SizeOf(input) * 8;
            var inp_top = input << (sz_bts - offset - numBits);
            var msk_top = ((0x1ul << numBits) - 1) << (sz_bts - numBits);

            var msk = msk_top & inp_top;
            var res = (ulong)(msk >> (sz_bts - numBits));

            return Marshal.PtrToStructure(new IntPtr(&res), (Type)input.GetType()) ?? throw new Crash();
         }
      }

      public static bool[] GetBitSet<IT>(IT mask) where IT : struct
      {
         var nb = Marshal.SizeOf(typeof(IT)) * 8;
         var dyn_msk = (dynamic)mask;

         return Enumerable.Range(0, nb).Select(bi => (bool)(((0x1ul << bi) & dyn_msk) != 0)).ToArray();
      }

      public static (int offset, int numBits) MaskToBitOffset(UInt64 inValue)
      {
         var off = 0;
         var len = 0;
         var is_clo = false;

         for (var i = 0; i < 64; i++)
         {
            var bit_msk = 0x1ul << i;
            var bit = (bit_msk & inValue) != 0;

            if (is_clo && bit) { throw new Gate.Tools.ToolsException($"Not continous bit set"); }

            if (len == 0)
            {
               if (bit)
               {
                  off = i;
                  len = 1;
               }
            }
            else
            {
               if (bit) { len++; }
               else { is_clo = true; }
            }
         }

         return (off, len);
      }

      public static UInt64 BitOffsetToMask(int offset, int numBits)
      {
         unchecked
         {
            return (AllBits >> (64 - numBits)) << offset;
         }
      }

      public static UInt64 GetBitField(UInt64 inValue, UInt64 mask)
      {
         var tmp = MaskToBitOffset(mask);

         return GetBitField(inValue, tmp.offset, tmp.numBits);
      }

      public static UInt64 SetBitField(UInt64 inValue, UInt64 mask, UInt64 setValue)
      {
         var tmp = MaskToBitOffset(mask);

         return SetBitField(inValue, setValue, tmp.offset, tmp.numBits);
      }

      public static Int16 GetBitField(Int16 input, int offset, int numBits) => (Int16)myGetBitField((Int32)input, offset, numBits);
      public static Int32 GetBitField(Int32 input, int offset, int numBits) => myGetBitField(input, offset, numBits);
      public static Int64 GetBitField(Int64 input, int offset, int numBits) => myGetBitField(input, offset, numBits);
      public static sbyte GetBitField(sbyte input, int offset, int numBits) => (sbyte)myGetBitField((Int32)input, offset, numBits);

      public static UInt16 GetBitField(UInt16 input, int offset, int numBits) => (UInt16)myGetBitField((UInt32)input, offset, numBits);
      public static UInt32 GetBitField(UInt32 input, int offset, int numBits) => myGetBitField(input, offset, numBits);
      public static UInt64 GetBitField(UInt64 input, int offset, int numBits) => myGetBitField(input, offset, numBits);
      public static byte GetBitField(byte input, int offset, int numBits) => (byte)myGetBitField((UInt32)input, offset, numBits);

      public static unsafe sbyte SetBitField(sbyte input, dynamic value, int offset, int numBits) => mySetBitField(input, value, offset, numBits);
      public static unsafe Int16 SetBitField(Int16 input, dynamic value, int offset, int numBits) => mySetBitField(input, value, offset, numBits);
      public static unsafe Int32 SetBitField(Int32 input, dynamic value, int offset, int numBits) => mySetBitField(input, value, offset, numBits);
      public static unsafe Int64 SetBitField(Int64 input, dynamic value, int offset, int numBits) => mySetBitField(input, value, offset, numBits);

      public static unsafe byte SetBitField(byte input, dynamic value, int offset, int numBits) => mySetBitField(input, value, offset, numBits);
      public static unsafe UInt16 SetBitField(UInt16 input, dynamic value, int offset, int numBits) => mySetBitField(input, value, offset, numBits);
      public static unsafe UInt32 SetBitField(UInt32 input, dynamic value, int offset, int numBits) => mySetBitField(input, value, offset, numBits);
      public static unsafe UInt64 SetBitField(ulong input, dynamic value, int offset, int numBits) => mySetBitField(input, value, offset, numBits);

      private static unsafe dynamic mySetBitField(dynamic input, dynamic value, int offset, int numBits)
      {
         unchecked
         {
            var inp_u64 = (ulong)input;
            var msk = (ulong)(1 << numBits) - 1;
            var val_msk = (ulong)value & msk;
            var val_shf = val_msk << offset;
            var msk_shf = msk << offset;

            var res = inp_u64 & (~msk_shf) | val_shf;

            return Marshal.PtrToStructure(new IntPtr(&res), (Type)input.GetType()) ?? throw new Crash();
         }
      }
   }
}
