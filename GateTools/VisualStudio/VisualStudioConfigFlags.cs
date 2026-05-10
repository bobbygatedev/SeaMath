using System;

namespace Gate.Tools.VisualStudio
{
   [Flags]
   public enum VisualStudioConfigFlags
   {
      None = 0,
      Debug = 0x1,
      Release = 0x2,
      x86 = 0x4,
      x64 = 0x8,
      Win32 = 0x10,
      ARM = 0x20,
      ARM64 = 0x40,
      MIPS = 0x80,
      OtherConfig = 0x100,
      AllStdConfigurations = Debug | Release,
      AllPlatforms = x86 | x64 | Win32 | ARM | ARM64 | MIPS,
   }
}
