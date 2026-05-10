using Gate.Tools;
using Gate.Tools.Extensions;

namespace GateTools.Binary
{
   public class BinaryFileSection
   {
      public BinaryFileSection(Interval interval, BinaryFile? binaryFile = null, string? name = null)
      {
         Name = name;
         Interval = interval;
         BinaryFile = binaryFile;
      }

      public string? Name { get; }

      public Interval Interval { get; }

      public BinaryFile? BinaryFile { get; set; }

      public nint? Pointer =>
         BinaryFile?.Pointer.HasValue ?? false ?
         BinaryFile.Pointer.Value + Interval.From : throw new ToolsException("Binary file pointer is not pinned.");

      public byte[] Bytes => BinaryFile?.Data.Skip(Interval.From).Take(Interval.Length).ToArray() ??
         throw new ToolsException("Binary file pointer not associated.");

      public string DataRepresentation => (Pointer ?? throw new ToolsException()).DumpHex(Interval.Length, Interval.From);

      public string DataRepresentationReduced => Interval.Length > RepresentationReducedValue ?
         $"{(Pointer ?? throw new ToolsException()).DumpHex(RepresentationReducedValue, Interval.From, RepresentationColumns)}\n(...)" :
         (Pointer ?? throw new ToolsException()).DumpHex(Interval.Length, Interval.From, RepresentationColumns);

      public static int RepresentationReducedValue { get; set; } = 128;

      public static int RepresentationColumns { get; set; } = 16;

      public override string ToString() => $"{Name} {Interval.HexRepresentation} From {BinaryFile?.FileInfo?.Name}";
   }
}
