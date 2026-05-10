using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.Tools;
using Gate.Tools.DesignPattern;
using Gate.Tools.Extensions;
using System.Runtime.InteropServices;

namespace Gate.CLanguage.Runtime.Object
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class CRtmObjAllocator : HierarchicalItemWithFinalizer
   {
      private readonly List<RtmDbgEngVirtCpuProcess> myListProcess = new List<RtmDbgEngVirtCpuProcess>();
      private static readonly List<Allocation> myListAllocationsGlobal = new List<Allocation>();

      /// <summary>
      /// Terminated either by process end or allocator end.
      /// Prevent deallocation exceptions due to anticipated dispose (by process end/allocator dispose). 
      /// </summary>
      private static readonly List<Allocation> myListTerminatedObject = new List<Allocation>();
  
      protected CRtmObjAllocator()
      {

      }

      protected class ThreadEntry
      {
         public readonly List<Allocation> AllocatedPointers = new List<Allocation>();

         public ThreadEntry(IRtmDbgEngThread thread) => Thread = thread;

         public IRtmDbgEngThread Thread { get; }

         public override string ToString() => $"{Thread} ({AllocatedPointers.Count} allocations)";
      }


      /// <summary>
      /// 
      /// </summary>
      public class Allocation : HierarchicalItem
      {
         /// <summary>
         /// 
         /// </summary>
         /// <param name="ptr"></param>
         /// <param name="size"></param>
         /// <param name="thread"></param>
         public Allocation(uint size, IRtmDbgEngThread? thread)
         {
            Size = size;
            Thread = thread;
            Process = thread?.Process;
         }

         public IntPtr Pointer { get; internal set; }

         public uint Size { get; }
         public IRtmDbgEngThread? Thread { get; }
         public IRtmDbgEngProcess? Process { get; }

         public object? Tag { get; set; }

         public CRtmObjAllocator? Allocator => ParentItem as CRtmObjAllocator;

         public override string ToString() => $"Ptr: 0x{Pointer:x16}, Size: {Size}, Thread: {Thread}, Process: {Process}";
      }

      public uint TotalMemoryAllocatedGlobal => (uint)myListAllocationsGlobal.Sum(p => p.Size);

      public int NumberOfAllocationsGlobal => myListAllocationsGlobal.Count;

      public uint TotalMemoryAllocatedLocal => (uint)AllocationsLocal.Sum(p => p.Size);

      public Allocation[] AllocationsLocal => SubItems.OfType<Allocation>().ToArray();


      public abstract bool IsZeroingRequired { get; }

      protected abstract nint myAllocate(Allocation entry);

      protected abstract void myDeallocate(Allocation entry);

      public unsafe Allocation Allocate(int totalSizeof)
      {
         lock (myListAllocationsGlobal)
         {
            var cur_thr = RtmDbgEngVirtCpuThread.GetRunningThread();
            var cur_pro = cur_thr?.Process;
            var all = new Allocation((uint)totalSizeof, cur_thr);

            all.Pointer = myAllocate(all);
       
            if (IsZeroingRequired)
            {
               NativeMemory.Clear((void*)all.Pointer, (nuint)all.Size);
            }

            if (cur_pro != null && !myListProcess.Contains(cur_pro))
            {
               myListProcess.Add(cur_pro);
               cur_pro.OnProcessChangeState += Cur_pro_OnProcessChangeState;
            }

            myListAllocationsGlobal.Add(all);
            myRemoveSubItem(all);

            return all;
         }
      }

      private void Cur_pro_OnProcessChangeState(IRtmDbgEngProcess process, RtmDbgEngRunState newState, RtmDbgEngRunState oldState)
      {
         if (newState == RtmDbgEngRunState.terminated)
         {
            lock (myListAllocationsGlobal)
            {
               var to_rem = myListAllocationsGlobal.Where(p => p.Process == process).ToArray();

               foreach (var ent in to_rem)
               {
                  FreeAllocation(ent);
               }

               //add to terminated loist (avoid crash after RtmObj dispose)
               myListTerminatedObject.AddRange(to_rem);
            }
         }
      }

      public void FreeAllocation(Allocation allocation)
      {
         lock (myListAllocationsGlobal)
         {
            if (myListAllocationsGlobal.Contains(allocation) && allocation.Allocator != null)
            {
               myDeallocate(allocation);
               myListAllocationsGlobal.Remove(allocation);
               myRemoveSubItem(allocation);
            }
         }
      }

      public unsafe void* Reallocate(void* ptr, uint size) => myFreeOrReallocate((nint)ptr, size);

      private unsafe void* myFreeOrReallocate(nint dataPtr, UInt32? reallocateSize)
      {
         lock (myListAllocationsGlobal)
         {
            if (dataPtr == IntPtr.Zero) { return null; }
            else
            {
               var thr = RtmDbgEngVirtCpuThread.GetRunningThread();
               var all = myListAllocationsGlobal.FirstOrDefaultUnique(p => p.Pointer == dataPtr);

               if (all != null)
               {
                  var new_mem = (nint)null;

                  if (reallocateSize.HasValue)
                  {
                     var nb = Math.Min(all.Size, reallocateSize.Value);

                     var new_all = Allocate((int)reallocateSize.Value);
                     NativeMemory.Copy((void*)dataPtr, (void*)new_all.Pointer, (nuint)nb);
                  }

                  FreeAllocation(all);

                  return (void*)new_mem;
               }
               else
               {
                  myListTerminatedObject.RemoveAll(e => e.Pointer == dataPtr);
           
                  return null;
               }
            }
         }
      }

      public unsafe void Free(nint dataPtr) => myFreeOrReallocate(dataPtr, null);

      protected override void myFreeManaged()
      {
         lock (myListAllocationsGlobal)
         {
            var to_del = AllocationsLocal.ToArray();

            foreach (var ent in to_del)
            {
               FreeAllocation(ent);
               myListTerminatedObject.Add(ent);
            }
         }
      }

      protected override void myFreeUnmanaged() { }
   }
}
