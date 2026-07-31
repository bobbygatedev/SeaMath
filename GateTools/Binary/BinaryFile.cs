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

      public BinaryFile(FileInfo fileInfo, bool doNotPin = false)
      {
         FileInfo = fileInfo;
         Data = File.ReadAllBytes(fileInfo.FullName);

         if (!doNotPin)
         {
            PinPointer();
         }
      }

      public BinaryFile(string path, bool doNotPin = false) : this(new FileInfo(path), doNotPin) { }

      public BinaryFile(byte[] data, bool doNotPin = false)
      {
         Data = data;

         if (!doNotPin)
         {
            PinPointer();
         }
      }

      public BinaryFile(nint pointer, int lenBytes)
      {
         Data = new byte[lenBytes];
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
   }
}
