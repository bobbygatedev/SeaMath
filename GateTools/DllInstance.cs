using Gate.Tools.DesignPattern;
using Gate.Tools.Extensions;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Gate.Tools
{
   public class DllInstance : BaseClassWithFinalizer
   {
      [DllImport("kernel32.dll", EntryPoint = "LoadLibrary", CharSet = CharSet.Unicode, SetLastError = true)]
      private static extern DllSafeHandle myLoadLibrary(string lpFileName);

      [DllImport("kernel32.dll", EntryPoint = "GetProcAddress", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
      private static extern IntPtr myGetProcAddress(DllSafeHandle hModule, string lpProcName);

      private readonly PinvokeEmitHelper myPinvokeEmitHelper = new PinvokeEmitHelper();
      private Type? myCurrentPinvokeType = null;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="dllPath"></param>
      /// <exception cref="Win32Exception"></exception>
      public DllInstance(string? dllPath = null)
      {
         if (!dllPath.IsBlank()) { Load(dllPath); }
      }

      /// <summary>
      /// 
      /// </summary>
      public sealed class DllSafeHandle : SafeHandle
      {
         [DllImport("kernel32.dll", EntryPoint = "FreeLibrary", SetLastError = true)]
         private static extern bool myFreeLibrary(IntPtr hLibModule);

         /// <summary>
         /// Constructor allocates an invalid handle (IntPtr.Zero) and specifies that the handle should be released (ownsHandle: true).
         /// </summary>
         /// <remarks>  handle non valido = IntPtr.Zero</remarks>
         public DllSafeHandle() : base(IntPtr.Zero, ownsHandle: true) { }

         /// <summary>
         /// Gets a value indicating whether the handle is invalid.
         /// </summary>
         public override bool IsInvalid => handle == IntPtr.Zero;

         /// <summary>
         /// Called just once in any case even in finalizer, 
         /// even in case of exceptions. 
         /// It releases the unmanaged resource by calling FreeLibrary on the handle.
         /// </summary>
         /// <returns></returns>
         protected override bool ReleaseHandle() => myFreeLibrary(handle);
      }

      public class MethodDefine
      {
         public MethodDefine(string methodName, Type? returnType, Type[] args, CharSet charSet = CharSet.Ansi)
         {
            MethodName = methodName;
            ReturnType = returnType;
            Args = args.ToArray();
            CharSet = charSet;
         }

         public string MethodName { get; }
         public Type? ReturnType { get; }
         public Type[] Args { get; }
         public CharSet CharSet { get; internal set; }
      }

      public DllSafeHandle Handle { get; private set; } = null!;

      public string? DllPath { get; private set; }

      public double TimeoutDebuggerAttached { get; set; } = 30;

      public double TimeoutNotDebuggerAttached { get; set; } = 1;

      public double Timeout => Debugger.IsAttached ? TimeoutDebuggerAttached : TimeoutNotDebuggerAttached;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="dllPath"></param>
      /// <exception cref="Win32Exception"></exception>
      public void Load(string? dllPath = null)
      {
         dllPath ??= DllPath;

         var sw = new Stopwatch();

         sw.Start();

         for (; ; )
         {
            try
            {
               Handle = LoadLibrary((DllPath = dllPath) ?? throw new Crash());
               return;
            }
            catch
            {
               if (sw.Elapsed.TotalSeconds >= Timeout)
                  throw;
            }
         }
      }

      public void Unload()
      {
         // SafeHandle gestisce già il rilascio, ma puoi forzare qui
         Handle?.Dispose();
         Handle = null!;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="dllPath"></param>
      /// <returns></returns>
      /// <exception cref="Win32Exception"></exception>
      public static DllSafeHandle LoadLibrary(string dllPath)
      {
         var hnd = myLoadLibrary(dllPath);

         if (hnd == null || hnd.IsInvalid)
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"LoadLibrary failed for '{dllPath}'");

         return hnd;
      }

      public MethodInfo[] DefineMethods(params MethodDefine[] methodDefines)
      {
         var exs = DefinedMethods.Where(m => methodDefines.Any(m1 => m1.MethodName == m.Name)).ToArray();

         if (exs.Length == 0)
         {
            foreach (var def in methodDefines)
            {
               myPinvokeEmitHelper.AddPInvokeMethod(
                  def.MethodName, Path.GetFileName(DllPath??throw new Crash()), def.CharSet, def.ReturnType, def.Args);
            }

            myCurrentPinvokeType = myPinvokeEmitHelper.CreateType();

            return DefinedMethods;
         }
         else
         {
            throw new Crash($"{DllPath} already contains {string.Join(",", exs.Select(e => e.Name))}");
         }
      }

      public MethodInfo[] DefinedMethods => myCurrentPinvokeType?.GetMethods() ?? Array.Empty<MethodInfo>();

      public object? InvokeDirect(string methodName, Type? returnType, CharSet charSet, params object[] args)
      {
         var mth = DefinedMethods.FirstOrDefault(m => m.Name == methodName);

         if (mth == null)
         {
            mth = DefineMethods([new MethodDefine(methodName, returnType, args.Select(a => a.GetType()).ToArray(), charSet)])[0];
         }

         return mth.Invoke(null, args);
      }

      public object? InvokeDirect(string methodName, Type? returnType, params object[] args)
         => InvokeDirect(methodName, returnType, CharSet.Ansi, args);

      public void InvokeDirectVoid(string methodName, params object[] args)
         => InvokeDirect(methodName, null, args);

      public DEL GetDelegate<DEL>(string procName) where DEL : Delegate
         => GetDelegate<DEL>(procName, Handle);

      public IntPtr GetProcAddress(string name)
         => GetProcAddress(Handle, name);

      public static DEL GetDelegate<DEL>(string procName, DllSafeHandle libHandle) where DEL : Delegate
         => Marshal.GetDelegateForFunctionPointer<DEL>(GetProcAddress(libHandle, procName));

      public static IntPtr GetProcAddress(DllSafeHandle hModule, string functionName)
      {
         if (hModule == null || hModule.IsInvalid)
            throw new ObjectDisposedException(nameof(DllSafeHandle));

         var ptr = myGetProcAddress(hModule, functionName);
         if (ptr == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"GetProcAddress failed for '{functionName}'");

         return ptr;
      }

      protected override void myFreeManaged() => Unload();

      protected override void myFreeUnmanaged()
      {
      }
   }
}
