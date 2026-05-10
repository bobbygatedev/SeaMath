using System.Runtime.InteropServices;

namespace Gate.Tools.Extensions
{
   /// <summary>
   /// 
   /// </summary>
   public static class TypeToStringExtender
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="intPtr"></param>
      /// <returns></returns>
      public static string ToStringExt(this IntPtr intPtr) => 
         Marshal.SizeOf(typeof(IntPtr)) == 4 ? 
            $"0x{intPtr.ToInt32():X8}" : //32bit 
            $"0x{intPtr.ToInt64():X16}"; //64bit
   }
}
