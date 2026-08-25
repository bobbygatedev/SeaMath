using System.Runtime.InteropServices;

namespace Gate.Tools.Binary
{
   /// <summary>
   /// Represents a binary file and provides access to its data and related file information.
   /// </summary>
   /// <remarks>The BinaryFile class encapsulates the contents of a binary file, allowing access to the file's
   /// data as a byte array and, if available, its associated file system information. It also provides methods to pin
   /// the underlying data in memory, which can be useful for interoperability scenarios that require a fixed memory
   /// address. Instances can be created from a file path, a FileInfo object, or directly from a byte array.</remarks>
   public unsafe class BinaryFile : HierarchicalItem
   {
      private BinaryFileSection? myWholeFileSection;

      /// <summary>
      /// Constructor. 
      /// </summary>
      /// <param name="fileInfo">File where data comes from</param>
      /// <param name="logicalAddress">If specified indicates logical address of first byte of file</param>
      /// <param name="doNotPin">If true data bytes are not pinned to pointer</param>
      public BinaryFile(FileInfo fileInfo, nint? logicalAddress = null, bool doNotPin = false)
      {
         FileInfo = fileInfo;
         LogicalAddress = logicalAddress;
         Data = File.ReadAllBytes(fileInfo.FullName);

         if (!doNotPin)
         {
            PinPointer();
         }
      }

      /// <summary>
      /// Constructor. 
      /// </summary>
      /// <param name="path"></param>
      /// <param name="logicalAddress">If specified indicates logical address of first byte of file</param>
      /// <param name="doNotPin">If true data bytes are not pinned to pointer</param>
      public BinaryFile(string path, nint? logicalAddress = null, bool doNotPin = false) : this(new FileInfo(path), logicalAddress, doNotPin) { }

      /// <summary>
      /// Constructor. 
      /// </summary>
      /// <param name="data"></param>
      /// <param name="logicalAddress">If specified indicates logical address of first byte of file</param>
      /// <param name="doNotPin">If true data bytes are not pinned to pointer</param>
      public BinaryFile(byte[] data, nint? logicalAddress = null, bool doNotPin = false)
      {
         Data = data;
         LogicalAddress = logicalAddress;

         if (!doNotPin)
         {
            PinPointer();
         }
      }

      /// <summary>
      /// Constructor. 
      /// </summary>
      /// <param name="pointer"></param>
      /// <param name="lenBytes"></param>
      /// <param name="logicalAddress">If specified indicates logical address of first byte of file</param>
      public BinaryFile(nint pointer, int lenBytes, nint? logicalAddress = null)
      {
         Data = new byte[lenBytes];
         LogicalAddress = logicalAddress;
         Marshal.Copy(pointer, Data, 0, lenBytes);
         Pointer = pointer;
      }

      public BinaryFileSection WholeFileSection
      {
         get
         {
            if (myWholeFileSection == null)
            {
               myWholeFileSection = new BinaryFileSection(this, 0, Data.Length);
            }

            return myWholeFileSection;
         }
      }

      public FileInfo? FileInfo { get; private set; }

      public byte[] Data { get; private set; }

      /// <summary>
      /// If has value indicates logical address of first byte of file</param>
      /// </summary>
      public nint? LogicalAddress { get; }

      public GCHandle? Handle { get; private set; }

      public nint? Pointer { get; private set; }

      public void PinPointer()
      {
         if (!Handle.HasValue && !Pointer.HasValue)
         {
            Handle = GCHandle.Alloc(Data ?? throw new Gate.Tools.ToolsException(), GCHandleType.Pinned);
            Pointer = Handle.Value.AddrOfPinnedObject();
         }
      }

      public void UnPinPointer()
      {
         if (Handle.HasValue)
         {
            Handle.Value.Free();
            Handle = null;
            Pointer = null;
         }
      }

      public void ReLoad(string? path = null, bool doNotPin = false)
      {
         UnPinPointer();

         if (path != null)
         {
            FileInfo = new FileInfo(path);
         }

         Data = File.ReadAllBytes(FileInfo?.FullName ?? throw new Gate.Tools.ToolsException());
      }

      public BinaryFileSection GetSection(int offset, int length) => new BinaryFileSection(this, offset, length);
   }
}
