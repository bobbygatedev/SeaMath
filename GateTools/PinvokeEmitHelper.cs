using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Gate.Tools
{
   public class PinvokeEmitHelper
   {
      private TypeBuilder? myTypeBuilder = null;
      private AssemblyBuilder? myAssemblyBuilder = null;

      public PinvokeEmitHelper(string assemblyStr = "DummyAssembly", string className = "DummyClass")
      {
         AssemblyStr = assemblyStr;
         ClassName = className;
      }

      public string ClassName { get; private set; }

      public string AssemblyStr { get; private set; }

      public static object? ExecuteMethodStatic(
         string methodName, string dllName, CharSet charSet, CallingConvention callingConventions, Type? returnType, params object[] @params) =>
            ExecuteMethod(methodName, dllName, charSet, callingConventions, null as object, returnType, @params);

      public static object? ExecuteMethodStatic(
         string methodName, string dllName, CharSet charSet, Type? returnType, params object[] @params) =>
            ExecuteMethod(methodName, dllName, charSet, null, returnType, @params);

      public static object? ExecuteMethod(
         string methodName,
         string dllName, CharSet charSet,
         CallingConvention callingConvention,
         object? @object,
         Type? returnType,
         params object[] @params)
      {
         var res = null as object;

         if (@params.LastOrDefault() is Array)
         {
            var arr = @params.LastOrDefault() as object[] ?? throw new Crash();

            @params = @params.Take(@params.Length - 1).Concat(arr).ToArray();
         }

         var in_tps = @params.Select(p => p.GetType()).ToArray();
         var mif = CreatePInvokeMethod(methodName, dllName, charSet, callingConvention, returnType, in_tps);
         res = mif?.Invoke(@object, @params);

         GC.Collect();

         return res;
      }

      public static object? ExecuteMethod(
         string methodName, string dllName, CharSet charSet, object? @object, Type? returnType, params object[] @params) =>
         ExecuteMethod(methodName, dllName, charSet, CallingConvention.Cdecl, @object, returnType, @params);

      public static MethodInfo? CreatePInvokeMethod(
         string methodName, string dllName, CharSet charSet, Type? returnType, params Type[] paramTypes) =>
         CreatePInvokeMethod(methodName, dllName, charSet, CallingConvention.Cdecl, returnType, paramTypes);

      public static MethodInfo? CreatePInvokeMethod(
         string methodName, string dllName, CharSet charSet, CallingConvention callingConventions, Type? returnType, params Type[] paramTypes)
      {
         var hlp = new PinvokeEmitHelper();

         hlp.AddPInvokeMethod(methodName, dllName, charSet, callingConventions, returnType, paramTypes);

         var typ = hlp.CreateType();

         return typ?.GetMethod(methodName);
      }

      public void AddPInvokeMethod(
         string methodName, string dllName, CharSet charSet, Type? returnType, params Type[] paramTypes) =>
         AddPInvokeMethod(methodName, dllName, charSet, CallingConvention.Cdecl, returnType, paramTypes);

      public void AddPInvokeMethod(
         string methodName, string dllName, CharSet charSet, CallingConvention callingConvention, Type? returnType, params Type[] paramTypes)
      {
         myCreateMethodBuilder();

         var mth_bui = myTypeBuilder?.DefinePInvokeMethod(
            methodName,
            dllName,
            MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.PinvokeImpl,
            CallingConventions.Standard,
            returnType == null ? typeof(void) : returnType,
            paramTypes,
            callingConvention,
            charSet);

         mth_bui?.SetImplementationFlags(mth_bui.GetMethodImplementationFlags() | MethodImplAttributes.PreserveSig);
      }

      public Type? CreateType() => myTypeBuilder?.CreateType();

      private void myCreateMethodBuilder()
      {
         if (myTypeBuilder == null)
         {
            var asm_nam = new AssemblyName(AssemblyStr);

            myAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(asm_nam, AssemblyBuilderAccess.Run);
            //AppDomain.CurrentDomain.DefineDynamicAssembly(asm_nam, AssemblyBuilderAccess.RunAndSave);

            // For a single-module assembly, the module name is usually
            // the assembly name plus an extension.
            var mb = myAssemblyBuilder.DefineDynamicModule(asm_nam?.Name ?? "");

            myTypeBuilder = mb.DefineType(ClassName, TypeAttributes.Public | TypeAttributes.Class);
         }
      }
   }
}
