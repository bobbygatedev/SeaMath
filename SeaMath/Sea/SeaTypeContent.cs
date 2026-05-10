using System;
using System.Runtime.InteropServices;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// 
   /// </summary>
   [StructLayout(LayoutKind.Sequential)]
   public unsafe struct SeaTypeContent
   {
      public const UInt32 TAG = 0xfefefefe;
     
      public UInt32 Tag;
      public UInt32 Counter;
   }
}
