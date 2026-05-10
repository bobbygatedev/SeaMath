using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Types;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.SeaMath.Console;
using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.SeaMath.Workspace.Libs
{
   /// <summary>
   /// Represents stdlib like library for SeaMath, 
   /// providing implementations for common C library functions such as memory management and process control.
   /// </summary>
   public unsafe class SeaMathLibStdLib : SeaMathLibCSharp
   {
      public const string NAME = "Stdlib";

      /// <summary>
      /// Initializes a new instance of the <see cref="SeaMathLibStdLib"/> class with the specified debugging IDE.
      /// </summary>
      /// <param name="ide"></param>
      public SeaMathLibStdLib(SeaMathDbgIde ide) : base(NAME, ide) { }

      /// <summary>
      /// Allocates a block of memory of the specified size and returns a pointer to the beginning of the block.
      /// </summary>
      /// <param name="size"></param>
      /// <returns></returns>
      [Method(Name = "malloc")]
      public void* DoMalloc(UInt32 size) => (void*)RtmStrategy.Allocator.Allocate((int)size).Pointer;

      /// <summary>
      /// Allocates a block of memory for an array of elements, 
      /// initializes all bytes in the allocated storage to zero, and returns a pointer to the beginning of the block.
      /// </summary>
      /// <param name="n"></param>
      /// <param name="size"></param>
      /// <returns></returns>
      [Method(Name = "calloc")]
      public void* DoCalloc(UInt32 n, UInt32 size) => (void*)RtmStrategy.Allocator.Allocate((int)(n * size)).Pointer;

      /// <summary>
      /// Changes the size of the memory block pointed to by ptr to size bytes.
      /// </summary>
      /// <param name="ptr"></param>
      /// <param name="size"></param>
      /// <returns></returns>
      [Method(Name = "realloc")]
      public void* DoRealloc(void* ptr, UInt32 size) => RtmStrategy.Allocator.Reallocate(ptr, size);

      /// <summary>
      /// Deallocates the memory previously allocated by a call to malloc, calloc, or realloc.
      /// </summary>
      /// <param name="ptr"></param>
      [Method(Name = "free")]
      public void DoFree(void* ptr) => RtmStrategy.Allocator.Free((int)ptr);

      /// <summary>
      /// Causes normal program termination with the specified exit status.
      /// </summary>
      /// <param name="exitCode"></param>
      /// <exception cref="Crash"></exception>
      [Method(Name = "exit")]
      public void DoExit(int exitCode)
      {
         var pro = RtmDbgEngVirtCpuThread.GetRunningThread()?.Process as SeaMathProcess ?? throw new Crash();

         pro.Terminate(false, exitCode);
      }

      /// <summary>
      /// Causes abnormal program termination without cleaning up.
      /// </summary>
      /// <exception cref="Crash"></exception>
      [Method(Name = "abort")]
      public void DoAbort()
      {
         var pro = RtmDbgEngVirtCpuThread.GetRunningThread()?.Process as SeaMathProcess ?? throw new Crash();

         pro.Terminate(true);
      }

      /// <summary>
      /// Registers a callback function to be invoked when the process exits.
      /// </summary>
      /// <remarks>The callback is executed when the process terminates. Only objects implementing <see
      /// cref="IRtmObjFunction"/> can be registered; other objects are rejected and an error message is written to the
      /// process's standard error stream.</remarks>
      /// <param name="exitCallBack">The callback object to execute at process termination. Must implement <see cref="IRtmObjFunction"/>.</param>
      /// <returns>Zero if the callback was successfully registered; otherwise, -1 if the callback is invalid.</returns>
      /// <exception cref="Crash">Thrown if the current thread's process is not a <see cref="SeaMathProcess"/> instance.</exception>
      [Method(Name = "atexit")]
      public RtmObj DoAtexit(RtmObj exitCallBack)
      {
         var pro = RtmDbgEngVirtCpuThread.GetRunningThread()?.Process as SeaMathProcess ?? throw new Crash();

         if (exitCallBack is IRtmObjFunction fnc)
         {
            pro.AddAtExit(fnc);

            return new CRtmObjLiteral(0, new CTypeAlias(RtmStrategy.Settings.BuiltInSet?["int"].NnOrCrash()));
         }
         else
         {
            using (var sw = new StreamWriter(pro.StdErr))
            {
               sw.WriteLine("Invalid callback for atexit.");
            }

            return new CRtmObjLiteral(-1, new CTypeAlias(RtmStrategy.Settings.BuiltInSet?["int"].NnOrCrash()));
         }
      }
   }
}
