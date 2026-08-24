using Gate.Tools.Extensions;

namespace Gate.Tools.Binary
{
   public unsafe class BinaryFileSection : HierarchicalItem
   {
      public BinaryFileSection(Interval interval, BinaryFile? binaryFile = null, string? name = null)
      {
         Name = name;
         Interval = interval;
         ParentItem = binaryFile;
      }

      public BinaryFileSection(BinaryFile binaryFile, int offset , int len) : 
         this(Interval.FromFromLen(offset, len), binaryFile) { }

      public BinaryFileSection(nint pointer, int len) : this(Interval.FromFromLen(0, len), new BinaryFile(pointer, len)) { }

      public string? Name { get; }

      public Interval Interval { get; }

      public BinaryFile? BinaryFile => ParentItem as BinaryFile;

      public nint? MemPointer =>
         BinaryFile?.Pointer.HasValue ?? false ?
         BinaryFile.Pointer.Value + Interval.From : throw new ToolsException("Binary file pointer is not pinned.");

      public nint? LogicalAddress
      {
         get
         {
            var la = BinaryFile?.LogicalAddress;

            return la.HasValue ? (nint?)((byte*)la + Interval.From) : null;
         }
      }

      public byte[] Bytes => BinaryFile?.Data.Skip(Interval.From).Take(Interval.Length).ToArray() ??
         throw new ToolsException("Binary file pointer not associated.");

      public string GetHexDump(
         uint length = 0,
         BitNumber bitNumber = BitNumber.Bit8,
         int? nWordPerRaw = null,
         bool isBigEndian = false) => 
            (MemPointer ?? throw new ToolsException()).GetHexDump(
               length == 0 ? (uint)Interval.Length : length, bitNumber, LogicalAddress, nWordPerRaw, isBigEndian);

      public string DataRepresentation => (MemPointer ?? throw new ToolsException()).
         GetHexDump((uint)Interval.Length, BitNumber.Bit8, Interval.From);

      public string DataRepresentationReduced => Interval.Length > RepresentationReducedValue ?
         $"{(MemPointer ?? throw new ToolsException()).GetHexDump(
            (uint)RepresentationReducedValue, BitNumber.Bit8, Interval.From, RepresentationColumns)}\n(...)" :
         (MemPointer ?? throw new ToolsException()).GetHexDump(
            (uint)Interval.Length, BitNumber.Bit8, Interval.From, RepresentationColumns);

      public static int RepresentationReducedValue { get; set; } = 128;

      public static int RepresentationColumns { get; set; } = 16;

      public override string ToString() => $"{Name} {Interval.HexRepresentation} From {BinaryFile?.FileInfo?.Name}";
   }
}
