using Gate.SeaMath.Sea;
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
      public SeaMathLibDllCompiler() { }

      public bool Compile(SeaMathDbgIde dbgIde, bool isRebuild)
      {
         var cur_cmp_env = dbgIde.Workspace.CompileEnvironment.NnOrCrash();
         var msg = new MsgCollection();
         var res = true;

         msg.OnMsg2DisplayAdded += m => dbgIde.MessageDisplayer.AddMsg(m);

         var lib_inc_drs = dbgIde.Workspace.Libs.IncludeDirsForLibraryOnly;

         res = cur_cmp_env.Register(msg);

         if (res)
         {
            var lib_c_fls = dbgIde.Workspace.Libs.AllFiles.
               Where(f =>
                  f.Extension.IsEqualNoContent(".c") &&
                  (isRebuild || f.IsRequiredRebuildForDllFromCFile())).ToArray();

            foreach (var fil in lib_c_fls)
            {
               var dll_inf = fil.GetDllLibFile();

               if (dll_inf == null || !cur_cmp_env.Compile(dll_inf, [fil], CompileEnvOut.dll, msg, lib_inc_drs))
               {
                  res = false;

                  if (dll_inf?.Exists ?? false)
                  {
                     try
                     {
                        dll_inf.Delete();
                     }
                     catch (Exception exc)
                     {
                        msg.Add(new Msg(MsgType.error, $"Error while deleting {dll_inf.FullName}: {exc.Message}"));
                     }
                  }
               }
            }
         }

         return res;
      }
   }
}
