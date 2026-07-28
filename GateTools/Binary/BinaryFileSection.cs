using Gate.Tools.Extensions;

namespace Gate.Tools.Binary
{
   public class BinaryFileSection
   {
      public BinaryFileSection(Interval interval, BinaryFile? binaryFile = null, string? name = null)
      {
         Name = name;
         Interval = interval;
         BinaryFile = binaryFile;
      }

      public BinaryFileSection(nint pointer, int len) : this(Interval.FromFromLen(0, len), new BinaryFile(pointer, len)) { }

      public string? Name { get; }

      public Interval Interval { get; }

      public BinaryFile? BinaryFile { get; set; }

      public nint? Pointer =>
         BinaryFile?.Pointer.HasValue ?? false ?
         BinaryFile.Pointer.Value + Interval.From : throw new ToolsException("Binary file pointer is not pinned.");

      public byte[] Bytes => BinaryFile?.Data.Skip(Interval.From).Take(Interval.Length).ToArray() ??
         throw new ToolsException("Binary file pointer not associated.");

      public string GetHexDump(
         uint length = 0,
         BitNumber bitNumber = BitNumber.Bit8,
         nint? offset = null,
         int? nWordPerRaw = null,
         bool isBigEndian = false) => 
            (Pointer ?? throw new ToolsException()).GetHexDump(
               length == 0 ? (uint)Interval.Length : length, bitNumber, offset, nWordPerRaw, isBigEndian);

      public string DataRepresentation => (Pointer ?? throw new ToolsException()).
         GetHexDump((uint)Interval.Length, BitNumber.Bit8, Interval.From);

      public string DataRepresentationReduced => Interval.Length > RepresentationReducedValue ?
         $"{(Pointer ?? throw new ToolsException()).GetHexDump(
            (uint)RepresentationReducedValue, BitNumber.Bit8, Interval.From, RepresentationColumns)}\n(...)" :
         (Pointer ?? throw new ToolsException()).GetHexDump(
            (uint)Interval.Length, BitNumber.Bit8, Interval.From, RepresentationColumns);

      public static int RepresentationReducedValue { get; set; } = 128;

      public static int RepresentationColumns { get; set; } = 16;

      public override string ToString() => $"{Name} {Interval.HexRepresentation} From {BinaryFile?.FileInfo?.Name}";
   }
}
