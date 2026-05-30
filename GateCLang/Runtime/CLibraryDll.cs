using Gate.CLanguage.Compiler;
using Gate.CLanguage.Decl;
using Gate.CLanguage.Linker;
using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Source;
using Gate.CLanguage.Types;
using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Message;
using Gate.Tools.Text;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using static Gate.CLanguage.CAttribute;

namespace Gate.CLanguage.Runtime
{
   /// <summary>
   /// Represents a dll library. It is built from a dll file and a header file. 
   /// The header file is used to get the functions decls and their parameters types. 
   /// The dll file is used to check if the functions are really present in the dll and to invoke them. 
   /// </summary>
   public class CLibraryDll : CLibrary
   {
      private bool myHasInit = false;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="dllPath"></param>
      /// <param name="headerSource"></param>
      /// <param name="rtmStrategy"></param>
      /// <param name="envDirs"></param>
      private CLibraryDll(FileInfo dllPath, CSource headerSource, CRtmObjStrategy rtmStrategy, params string[] envDirs)
      {
         RtmStrategy = rtmStrategy;
         EnvDirs = envDirs;
         DllPath = dllPath;
         myAddSubItem(headerSource);
         FileInfo = headerSource.FileInfo;
      }

      /// <summary>
      /// 
      /// </summary>
      public override CSource? Source => SubItems.OfType<CSource>().FirstOrDefault();

      /// <summary>
      /// 
      /// </summary>
      public FileInfo DllPath { get; }

      /// <summary>
      /// 
      /// </summary>
      public CRtmObjStrategy RtmStrategy { get; }

      /// <summary>
      /// 
      /// </summary>
      public string[] EnvDirs { get; }

      /// <summary>
      /// 
      /// </summary>
      public override FileInfo? FileInfo { get; }

      /// <summary>
      /// 
      /// </summary>
      public override string? Name => FileInfo?.Name;

      /// <summary>
      /// 
      /// </summary>
      public override string? Descriptor => null;

      /// <summary>
      /// 
      /// </summary>
      public override string? Rebuilt => null;

      /// <summary>
      /// 
      /// </summary>
      public override IDeclFunction? InitDeclFunction => null;

      /// <summary>
      /// 
      /// </summary>
      public override IDeclFunction? CleanupDeclFunction => null;

      /// <summary>
      /// 
      /// </summary>
      public override CDecl[] Decls
      {
         get
         {
            var dcl_fns =
               Source?.AllGlobals.OfType<CDeclFunction>().
               Where(f =>
                  !f.GetGccAttributesId<CLibraryDllGccAttributesIds>().
                  Contains(CLibraryDllGccAttributesIds.noimpl)).ToArray();

            if (!IsSuccessfullChecked)
            {
               throw new Gate.LangBase.Runtime.RtmException($"Dll {FileInfo?.FullName} not checked yet!");
            }
            else if (!myHasInit)
            {
               myHasInit = true;

               foreach (var dcl_fnc in dcl_fns ?? [])
               {
                  dcl_fnc.Instructions = [
                     new RtmDbgEngVirtCpuInstructionByAction(null, (stk,str) => myRunAction(dcl_fnc, stk))];
               }

               return dcl_fns ?? [];
            }
            else
            {
               return dcl_fns ?? [];
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public bool IsSuccessfullChecked { get; private set; }

      public override IDeclType[] Types => [];

      public bool Build(MsgCollection messages)
      {
         var fns = Source?.AllGlobals.OfType<CDeclFunction>().ToArray();

         foreach (var fnc in fns ?? [])
         {
            foreach (var atr in fnc.AttributesAll.OfType<GccAttribute>().SelectMany(a => a.AttributeTuples))
            {
               if (atr.name == CLibraryDllGccAttributesIds.altname.GetString())
               {
                  fnc.AlternateLinkName = atr.arg;
               }
            }
         }

         return true;
      }

      public bool Check(MsgCollection messages)
      {
         if (!IsSuccessfullChecked)
         {
            var dcl_fns =
               Source?.AllGlobals.OfType<CDeclFunction>().
               Where(f =>
                  !f.GetGccAttributesId<CLibraryDllGccAttributesIds>().
                  Contains(CLibraryDllGccAttributesIds.noimpl)).ToArray();
            var sav = Environment.GetEnvironmentVariable("PATH");

            try
            {
               var res = true;

               Environment.SetEnvironmentVariable("PATH", string.Join(";", EnvDirs));

               using (var dll_ins = new DllInstance(DllPath.FullName))
               {
                  foreach (var dcl_fnc in dcl_fns ?? [])
                  {
                     var ptr = dll_ins.GetProcAddress(dcl_fnc.AlternateLinkName ?? dcl_fnc.Identifier ?? "");

                     if (ptr == IntPtr.Zero)
                     {
                        messages.Add(new Msg(MsgType.error, $"Function {dcl_fnc.Identifier} not found in dll {DllPath}"));
                        res = false;
                     }
                  }

                  if (!res)
                  {
                     messages.Add(new Msg(MsgType.error, $"Failed check of dll {FileInfo?.FullName}"));

                     return false;
                  }
                  else
                  {
                     IsSuccessfullChecked = true;
                  }
               }
            }
            catch (Win32Exception exc)
            {
               messages.Add(new Msg(MsgType.error, $"Failed to load {DllPath}"));
               messages.Add(new Msg(MsgType.error, exc.Message));

               return false;
            }
            finally
            {
               Environment.SetEnvironmentVariable("PATH", sav);
            }
         }

         return IsSuccessfullChecked;
      }

      public static CLibraryDll? Make(CCompiler compiler, FileInfo dllPath, string headerFile, MsgCollection messages, params string[] envDirs)
      {
         if (compiler.Compile(TxtStore.FromPath(headerFile), messages, out var h_src))
         {
            var dll = new CLibraryDll(dllPath, h_src ?? throw new Crash(), compiler.RtmStrategy, envDirs);

            if (dll.Build(messages) && dll.Check(messages))
            {
               return dll;
            }
         }

         return null;
      }

      private RtmObj? myRunAction(IDeclFunction declFunction, RtmDbgEngStackVirtCpu stack)
      {
         if (stack.TopFunctionFrame?.ObjInStackOnly.All(o => o is CRtmObj) ?? false)
         {
            var prs = stack.TopFunctionFrame.ObjInStackOnly.Cast<CRtmObj>().Reverse().ToArray();
            var mth_inf = myCreateMethodInfo(declFunction, DllPath.FullName, prs);

            myParamsCheck(declFunction, prs);

            var res_cs = null as ValueType;

            try
            {
               res_cs = mth_inf?.Invoke(null, prs.Select(p => p.CSharpObj).ToArray()) as ValueType ?? throw new Crash();
            }
            catch (System.Reflection.TargetInvocationException)
            {
               throw new Gate.LangBase.Runtime.RtmException($"Can't invoke {declFunction.Identifier} function not existing in {DllPath}");
            }

            if (mth_inf?.ReturnType != null && mth_inf.ReturnType != typeof(void))
            {
               return RtmStrategy.MakeConstant(res_cs, declFunction.ReturnType as CType ?? throw new Crash());
            }
            else
            {
               return null;
            }
         }
         else
         {
            throw new Crash();
         }
      }

      private static void myParamsCheck(IDeclFunction declFunction, CRtmObj[] @params)
      {
         if (@params.Length < declFunction.Parameters.Length)
         {
            throw new Gate.LangBase.Runtime.RtmException("Too few parameters");
         }
         else if (@params.Length > declFunction.Parameters.Length && !declFunction.HasVarArgs)
         {
            throw new Gate.LangBase.Runtime.RtmException("Too many parameters");
         }
      }

      private MethodInfo? myCreateMethodInfo(IDeclFunction declFunction, string libPath, params CRtmObj[] @params)
      {
         var par_tps = null as Type[];
         var ret_typ = null as Type;
         var c_prs = @params.Cast<CRtmObj>();
         var alo_str = (IRtmObjStrategy)RtmStrategy;

         myParamsCheck(declFunction, @params);
         par_tps = declFunction.Parameters.Select(p => alo_str.GetCSharpType(p.DeclType)).ToArray();
         par_tps = par_tps.
            Concat(
               c_prs.Skip(declFunction.Parameters.Length).
               Select(p => alo_str.GetCSharpType(p.DeclType))).ToArray();
         ret_typ = alo_str.GetCSharpType(declFunction.ReturnType);

         var dcl_fnc = declFunction as CDeclFunction;

         return PinvokeEmitHelper.CreatePInvokeMethod(
            dcl_fnc?.AlternateLinkName ?? dcl_fnc?.Identifier ?? "", libPath, CharSet.Auto, ret_typ, par_tps);
      }

      public override string ToString() => FileInfo?.Name ?? "";
   }
}
