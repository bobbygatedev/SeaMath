using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Programming;
using System.Runtime.InteropServices;

namespace Gate.SeaMath.Workspace
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaMathLibDllCompiler
   {
      public SeaMathLibDllCompiler()
      {

      }

      public bool Compile(SeaMathDbgIde dbgIde)
      {
         var cur_cmp_env = dbgIde.Workspace.CompileEnvironment;
         var msg = new MsgCollection();
         var res = true;

         msg.OnMsg2DisplayAdded += m => dbgIde.MessageDisplayer.AddMsg(m);

         var lib_inc_drs = dbgIde.Workspace.Libs.IncludeDirsForLibraryOnly;

         var lib_c_fls = dbgIde.Workspace.Libs.AllFiles.Where(f => f.Extension.IsEqualNoContent(".c")).ToArray();
         var is_64 = Marshal.SizeOf(typeof(IntPtr)) == 8;

         foreach (var fil in lib_c_fls)
         {
            var dll_pth = new FileInfo($"{fil.FullName.Substring(0, fil.FullName.Length - 2)}.{(is_64 ? "64" : "32")}.dll");

            if (!cur_cmp_env.Compile(dll_pth, [fil], CompileEnvOut.dll, msg, lib_inc_drs)) { res = false; }
         }

         return res;
      }
   }
}
