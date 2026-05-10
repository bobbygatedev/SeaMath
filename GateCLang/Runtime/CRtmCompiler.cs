using Gate.CLanguage.Standards;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.Tools.Message;
using Gate.Tools.Text;

namespace Gate.CLanguage.Runtime
{
   /// <summary>
   /// 
   /// </summary>
   public class CRtmCompiler : IRtmDbgEngVirtCpuCompiler
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="cStandard"></param>
      public CRtmCompiler(CStandard cStandard) => CStandard = cStandard;

      /// <summary>
      /// 
      /// </summary>
      public CStandard CStandard { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="messages"></param>
      /// <param name="sourceFile"></param>
      /// <param name="srcFile"></param>
      /// <returns></returns>
      public bool Compiler(
         MsgCollection messages, FileInfo sourceFile, out IRtmDbgEngVirtCpuPseudoSource? srcFile)
      {
         if (CStandard.CCompiler.Compile(TxtStore.FromPath(sourceFile.FullName), messages, out var src))
         {
            srcFile = src;

            return true;
         }
         else
         {
            srcFile = null;

            return false;
         }
      }
   }
}
