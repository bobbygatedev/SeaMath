using Gate.Tools.Extensions;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Gate.Tools
{
   /// <summary>
   /// Represents an integer value that can encapsulate any standard signed or unsigned integer type, including 8-, 16-,
   /// 32-, 64-, and 128-bit integers. Provides seamless implicit conversions between supported integer types and
   /// exposes properties for inspecting the value in various numeric representations.
   /// </summary>
   /// <remarks>UniversalInt enables flexible storage and manipulation of integer values without requiring
   /// explicit type conversions. It is useful when working with APIs or data structures that must handle multiple
   /// integer types transparently. The struct provides properties to access the value in hexadecimal, octal, and binary
   /// formats, as well as information about the underlying type and bit width. All conversions are lossless as long as
   /// the value fits within the target type's range; otherwise, standard overflow behavior applies.</remarks>
   public struct UniversalInt
   {
      private ValueType myValue = 0;

      public static string[] Units = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];

      public static readonly Type[] SupportedTypes =
      [
         typeof(UIntPtr), typeof(IntPtr),
         typeof(sbyte), typeof(byte),
         typeof(Int16), typeof(UInt16),
         typeof(int), typeof(uint),
         typeof(Int64), typeof(UInt64),
         typeof(Int128), typeof(UInt128)
      ];

      private UniversalInt(ValueType? valueType = null) => myValue = valueType ?? 0;

      public static implicit operator UIntPtr(UniversalInt universalInt) => (UIntPtr)(dynamic)universalInt.myValue;
      public static implicit operator IntPtr(UniversalInt universalInt) => (IntPtr)(dynamic)universalInt.myValue;
      public static implicit operator sbyte(UniversalInt universalInt) => (sbyte)(dynamic)universalInt.myValue;
      public static implicit operator byte(UniversalInt universalInt) => (byte)(dynamic)universalInt.myValue;
      public static implicit operator Int16(UniversalInt universalInt) => (Int16)(dynamic)universalInt.myValue;
      public static implicit operator UInt16(UniversalInt universalInt) => (UInt16)(dynamic)universalInt.myValue;
      public static implicit operator int(UniversalInt universalInt) => (int)(dynamic)universalInt.myValue;
      public static implicit operator uint(UniversalInt universalInt) => (uint)(dynamic)universalInt.myValue;
      public static implicit operator Int64(UniversalInt universalInt) => (Int64)(dynamic)universalInt.myValue;
      public static implicit operator UInt64(UniversalInt universalInt) => (UInt64)(dynamic)universalInt.myValue;
      public static implicit operator Int128(UniversalInt universalInt) => (Int128)(dynamic)universalInt.myValue;
      public static implicit operator UInt128(UniversalInt universalInt) => (UInt128)(dynamic)universalInt.myValue;

      public static implicit operator UniversalInt(UIntPtr value) => new UniversalInt(value);
      public static implicit operator UniversalInt(IntPtr value) => new UniversalInt(value);
      public static implicit operator UniversalInt(sbyte value) => new UniversalInt(value);
      public static implicit operator UniversalInt(byte value) => new UniversalInt(value);
      public static implicit operator UniversalInt(Int16 value) => new UniversalInt(value);
      public static implicit operator UniversalInt(UInt16 value) => new UniversalInt(value);
      public static implicit operator UniversalInt(int value) => new UniversalInt(value);
      public static implicit operator UniversalInt(uint value) => new UniversalInt(value);
      public static implicit operator UniversalInt(Int64 value) => new UniversalInt(value);
      public static implicit operator UniversalInt(UInt64 value) => new UniversalInt(value);
      public static implicit operator UniversalInt(Int128 value) => new UniversalInt(value);
      public static implicit operator UniversalInt(UInt128 value) => new UniversalInt(value);

      /// <summary>
      /// Parses a string representation of an integer value, 
      /// which may include optional unit suffixes (e.g., "KB", "MB", "GB") to indicate memory capacities.
      /// </summary>
      /// <param name="value"></param>
      /// <returns></returns>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public static UniversalInt Parse(string value)
      {
         var val = value.ExtTrim();
         var uni = Units.FirstOrDefault(u => val.EndsWith(u));

         if (uni != null)
         {
            var mul = myGetMultiplier(uni);

            var rst = val.Substring(0, val.Length - uni.Length).ExtTrim();
            var ui = Parse(rst);

            return ui * mul;
         }
         else if (val.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
         {
            var rst = val.Substring(2);

            if (UInt64.TryParse(rst, NumberStyles.HexNumber, null, out var u64))
            {
               return u64 <= UInt32.MaxValue ? (UniversalInt)(UInt32)u64 : (UniversalInt)u64;
            }
         }
         else if (val.StartsWith("-"))
         {
            var rst = val.Substring(1).ExtTrim();

            if (Int64.TryParse(rst, NumberStyles.HexNumber, null, out var i64))
            {
               return i64 >= Int32.MinValue ? (UniversalInt)(Int32)i64 : (UniversalInt)i64;
            }
         }
         else
         {
            if (UInt64.TryParse(val, NumberStyles.HexNumber, null, out var u64))
            {
               return u64 <= UInt32.MaxValue ? (UniversalInt)(UInt32)u64 : (UniversalInt)u64;
            }
         }

         throw new Gate.Tools.ToolsException($"Can't convert {value} to {typeof(UniversalInt).Name}!");
      }

      private static ulong myGetMultiplier(string uni)
      {
         var idx = Units.ToList().IndexOf(uni);

         if (idx == -1) { throw new Crash(); }
         else
         {
            var val = 1UL;

            for (int i = 0; i < idx; i++)
            {
               val *= 1024;
            }

            return val;
         }
      }

      public int SizeOf => Marshal.SizeOf(myValue);

      public string Hex => myGetHex(false);

      public string HexSplit => myGetHex(true);

      public Type InternalType => myValue.GetType();

      public UInt128 TypeMask => ((UInt128)1 << NumBits) - 1;

      public string Octal
      {
         get
         {
            var buf = new char[NumOctalDigits]; // max 22 cifre ottali per UInt64
            var idx = buf.Length;
            var v = (UInt128)(dynamic)myValue & TypeMask;

            while (v > 0)
            {
               dynamic dgt = v & 0b111; // estrai 3 bit (mod 8)

               buf[--idx] = (char)('0' + dgt);
               v >>= 3;
            }

            return new string('0', NumOctalDigits - (buf.Length - idx)) + new string(buf, idx, buf.Length - idx);
         }
      }

      public string Resume => $"{myValue}({Representation},{MemCapacity})\n{HexBinOctal}";

      public string HexBinOctal => $"HEX: {HexSplit}\nBIN: {BinarySplit}\nOCT: {Octal}";

      public string Binary => myGetBinary(false);

      public string BinarySplit => myGetBinary(true);

      public string Representation => TryGetRepresentation(
         myValue.GetType(), out var rpr) && rpr != null ?
            rpr : throw new ToolsException("Unsupported type in UniversalInt");

      public int NumBits => SizeOf * 8;

      public int NumOctalDigits => NumBits / 3 + (NumBits % 3 != 0 ? 1 : 0);

      /// <summary>
      /// Convert a memory capacity in bytes to a human-readable string with appropriate units (KB, MB, GB, etc.).
      /// </summary>
      /// <param name="value">The memory capacity in bytes.</param>
      /// <returns>A human-readable string representation of the memory capacity.</returns>
      public string MemCapacity
      {
         get
         {
            var siz = (double)(dynamic)myValue;
            var un_idx = 0;

            while (siz >= 1024 && un_idx < Units.Length - 1)
            {
               siz /= 1024;
               un_idx++;
            }

            return $"{siz:0.##}{Units[un_idx]}";
         }
      }

      public string LenRepresentation
      {
         get
         {
            if (this <= 8)
            {
               return $"{(int)this}";
            }
            else if(this <= 1024)
            {
               return $"{(int)this}(0x{Hex})";
            }
            else
            {
               return $"0x{Hex}({MemCapacity})";
            }
         }
      }

      public static bool TryGetRepresentation(Type type, out string? representation)
      {
         if (SupportedTypes.Contains(type))
         {
            var sof = Marshal.SizeOf(type);

            if (type == typeof(UIntPtr))
            {
               representation = $"UPtr{sof * 8}";
            }
            else if (type == typeof(IntPtr))
            {
               representation = $"SPtr{sof * 8}";
            }
            else
            {
               representation = $"{type.Name.Substring(0, 1)}{sof * 8}";
            }

            return true;
         }
         else
         {
            representation = null;

            return false;
         }
      }

      public override string ToString() => $"{Representation}:{myValue}(0x{Hex})";

      /// <summary>
      /// Generates a hexadecimal string representation of the encapsulated integer value. 
      /// If isSplit is true, the hexadecimal string is formatted with spaces separating groups of digits 
      /// according to the size of the underlying type (e.g., 4-digit groups for 16-bit integers, 8-digit groups for 32-bit integers, etc.). 
      /// If isSplit is false, the hexadecimal string is returned as a continuous sequence of digits without any separators.
      /// </summary>
      /// <param name="isSplit"></param>
      /// <returns></returns>
      private string myGetHex(bool isSplit)
      {
         unchecked
         {
            var u64 = (ulong)(dynamic)myValue;
            var hex = string.Format("{0:X16}", u64);
            var sof = Marshal.SizeOf(myValue);
            var n_dgt = 2 * sof;//num of digits in hex

            var tmp = hex.Substring(16 - n_dgt);
            var rst = n_dgt % 4;
            var ng = n_dgt / 4;
            var res = "";
            var off = 0;
            var arr = isSplit ? new[] { rst }.
               Concat(
                  Enumerable.Range(0, ng).
                  Select(_ => 4)).
                  Where(v => v > 0).ToArray() : [n_dgt];

            foreach (var l in arr)
            {
               res += $"{tmp.Substring(off, l)} ";
               off += l;
            }

            return res.ExtTrim();
         }
      }

      private string myGetBinary(bool isSplit)
      {
         var buf = Enumerable.Range(0, 128).Select(_ => '0').ToArray(); // massimo 64 bit
         var idx = buf.Length;

         var v = (UInt128)(dynamic)myValue;

         while (v > 0)
         {
            buf[--idx] = (v & 1) == 1 ? '1' : '0';
            v >>= 1;
         }

         var res = new string(buf.Skip(buf.Length - NumBits).ToArray());

         if (isSplit)
         {
            res = string.Join(" ", Enumerable.Range(0, SizeOf).Select(i => res.Substring(i * 8, 8)));
         }

         return res;
      }
   }
}
