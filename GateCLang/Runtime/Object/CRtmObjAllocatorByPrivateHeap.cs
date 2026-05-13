using Gate.Tools;
using Gate.Tools.Extensions;
using System.Runtime.InteropServices;
using static Gate.Tools.AppParams.AppParam;

namespace Gate.CLanguage.Runtime.Object
{
   /// <summary>
   /// Allocator that manages a private heap. Allocations are done sequentially, and deallocation is tracked but does not free memory until reset. 
   /// Each allocation is surrounded by guard bands to detect buffer overruns. 
   /// The allocator provides methods to check the integrity of allocated blocks and the entire heap.
   /// </summary>
   public unsafe class CRtmObjAllocatorByPrivateHeap : CRtmObjAllocator
   {
      public static readonly byte GUARD_PATTERN = 0xEF;
      public static readonly uint GUARD_WORDS = 2;
      public static readonly uint GUARD_BYTES = GUARD_WORDS * (uint)sizeof(nint);

      private readonly LinkedList<InnerAllocation> myListAllocations = new LinkedList<InnerAllocation>();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="options"></param>
      /// <exception cref="OutOfMemoryException"></exception>
      public CRtmObjAllocatorByPrivateHeap(OptionsRecord? options = null)
      {
         Options = options ?? GlobalOptions;
         Memory = (byte*)NativeMemory.AlignedAlloc((nuint)Capacity, Alignement);

         if (Memory == null)
         {
            throw new OutOfMemoryException("AlignedAlloc failed");
         }

         CurrentPtr = Memory;
      }

      public class OptionsRecord : Record
      {
         private readonly Simple<UInt64> myCapacity = new Simple<UInt64>(64 * 1024 * 1024);
         private readonly Simple<bool> myDebugMode = new Simple<bool>(null, "Debug Mode", false);

         public OptionsRecord() : base("AllocOptions", "Allocator Options") { }

         /// <summary>
         /// Capacity of allocator requires restart of system.
         /// </summary>
         public UInt64 Capacity
         {
            get => myCapacity.Value;

            set => myCapacity.Value = value;
         }

         public bool IsDebugMode
         {
            get => myDebugMode.Value;

            set => myDebugMode.Value = value;
         }
      }

      private class InnerAllocation
      {

         public InnerAllocation(Allocation entry, nint basePtr, CRtmObjAllocatorByPrivateHeap allocator)
         {
            Entry = entry;
            PointerBase = myCheck(basePtr);
            Allocator = allocator;
         }

         private nint myCheck(nint basePtr) => basePtr % Alignement == 0 ? basePtr : throw new Crash();

         public uint NumBytesData => Entry.Size;

         public uint NumBytesAligned => (uint)Align(NumBytesData);

         public uint NumBytesTotal => NumBytesAligned + 2 * GUARD_BYTES;

         public Allocation Entry { get; }

         public nint PointerBase { get; }

         public nint PointerData => (nint)((byte*)PointerBase + GUARD_BYTES);

         public nint PointerNext => (nint)((byte*)PointerBase + NumBytesTotal);

         public CRtmObjAllocatorByPrivateHeap Allocator { get; }

         public string ContentPrintOut => PointerBase.GetHexDump(NumBytesTotal);

         public bool Check
         {
            get
            {
               var p1 = (byte*)PointerBase;
               var p2 = (byte*)(PointerData + NumBytesAligned);

               for (int i = 0; i < GUARD_BYTES; i++)
               {
                  if (p1[i] != GUARD_PATTERN || p2[i] != GUARD_PATTERN)
                  {
                     return false;
                  }
               }

               return true;
            }
         }

         public override string ToString() => $"{PointerData:x16} Len(Eff={NumBytesData} Rnd = {NumBytesAligned} Tot={NumBytesTotal})";

         public static InnerAllocation Make(Allocation entry, CRtmObjAllocatorByPrivateHeap allocator)
         {
            //aligned size
            var all = new InnerAllocation(entry, (nint)allocator.CurrentPtr, allocator);

            if (all.PointerNext > (nint)(allocator.Memory + allocator.Capacity))
            {
               throw new Gate.LangBase.Runtime.RtmException("Private heap exhausted");
            }
            else
            {
               all.WriteGuards();
            }

            // Advance the pointer
            allocator.CurrentPtr = (byte*)all.PointerNext;

            return all;
         }

         private void WriteGuards()
         {
            NativeMemory.Fill((void*)PointerBase, GUARD_BYTES, GUARD_PATTERN);
            NativeMemory.Fill((byte*)PointerData + NumBytesAligned, GUARD_BYTES, GUARD_PATTERN);
         }
      }

      protected override nint myAllocate(Allocation entry)
      {
         if (IsDebugMode)
         {
            CheckHeap("Allocation");
         }

         var all = InnerAllocation.Make(entry, this);

         // Traces allocation
         entry.Tag = myListAllocations.AddLast(all);

         return all.PointerData;
      }

      public static OptionsRecord GlobalOptions { get; } = new OptionsRecord();

      /// <summary>
      /// 
      /// </summary>
      public OptionsRecord Options { get; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsDebugMode => Options.IsDebugMode;

      /// <summary>
      /// 
      /// </summary>
      public UInt64 Capacity => Options.Capacity;

      public static uint Alignement { get; } = (uint)sizeof(nint);

      public override bool IsZeroingRequired => true;

      public unsafe byte* CurrentPtr { get; private set; }

      public unsafe byte* Memory { get; }

      /// <summary>
      /// Verifica l’integrità di tutte le allocazioni presenti.
      /// Ritorna true se tutto è integro, false se trova corruzioni.
      /// </summary>
      public void CheckHeap(string? during)
      {
         foreach (var all in myListAllocations)
         {
            if (!all.Check)
            {
               Console.WriteLine($"Failure in {all}");
               Console.WriteLine("Allocator state is!");

               foreach (var a1 in myListAllocations)
               {
                  Console.WriteLine(a1 + $"({(a1.Check ? "OK" : "FAIL")})");
               }

               foreach (var a1 in myListAllocations.Where(a => !a.Check))
               {
                  Console.WriteLine($"Failed allocation {a1} content:");
                  Console.WriteLine(a1.ContentPrintOut);
               }

               throw new Crash($"Allocation failure during {during}!");
            }
         }
      }

      /// <summary>
      /// Reset
      /// </summary>
      public void Reset()
      {
         CurrentPtr = Memory;
         myListAllocations.Clear();
      }

      public nint Offset => (nint)CurrentPtr - (nint)Memory;

      /// <summary>
      /// Deallocation
      /// </summary>
      /// <param name="dataPtr"></param>
      /// <exception cref="InvalidOperationException"></exception>
      protected override void myDeallocate(Allocation entry)
      {
         if (IsDebugMode)
         {
            CheckHeap("Deallocation");
         }

         // Cerca il blocco nella lista
         var nod = entry.Tag as LinkedListNode<InnerAllocation> ?? throw new Crash();

         myListAllocations.Remove(nod);

         var lst = myListAllocations.Last;

         if (lst == null)
         {
            CurrentPtr = Memory;
         }
         else
         {
            CurrentPtr = (byte*)(lst.Value.PointerNext);
         }
      }

      /// <summary>
      /// Freeing resource after dispose
      /// </summary>
      protected override void myFreeManaged()
      {
         base.myFreeManaged();
         NativeMemory.AlignedFree(Memory);
      }

      public override string ToString() => $"CAPACITY={Capacity}, USED={CurrentPtr - Memory}";

      public static nuint Align(uint size)
      {
         var mod = size % (int)Alignement;

         return mod == 0 ?
            (nuint)size : (nuint)(size + (int)Alignement - mod);
      }
   }
}
