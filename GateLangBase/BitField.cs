namespace Gate.LangBase
{
   /// <summary>
   /// Bit field wrapper
   /// </summary>
   public readonly struct BitField
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="offset">Offset in bit from 0(right bit) to #bit-1(left bit)</param>
      /// <param name="size"></param>
      public BitField(int offset, int size)
      {
         if (size < 1 || size > 64) { throw new Gate.LangBase.Runtime.RtmException($"Not valid size {size}"); }
         if (offset < 0 || offset > 63) { throw new Gate.LangBase.Runtime.RtmException($"Not valid offset {offset}"); }

         BitOffset = offset;
         BitSize = size;

         unchecked
         {
            U64BitSizeMask = ((1ul << size) - 1);
            U64BitMask = U64BitSizeMask << offset;
         }
      }

      /// <summary>
      /// Distance from byte in bits
      /// </summary>
      public int BitOffset { get; }

      /// <summary>
      /// Size of bit field in bits
      /// </summary>
      public int BitSize { get; }

      /// <summary>
      /// Equal to 1 shift right of <see cref="BitSize"/>-1.
      /// </summary>
      public UInt64 U64BitSizeMask { get; }

      /// <summary>
      /// Equal to <see cref="U64BitSizeMask"/> shift right of <see cref="BitOffset"/>
      /// </summary>
      public UInt64 U64BitMask { get; }

      public override string ToString() => $"Offset={BitOffset}, Size ={BitSize}";
   }
}
