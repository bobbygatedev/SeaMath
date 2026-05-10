using Gate.Tools.Message;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// 
   /// </summary>
   public interface IRtmDbgEngVirtCpuCompiler
   {
      bool Compiler(MsgCollection messages, FileInfo sourceFile, out IRtmDbgEngVirtCpuPseudoSource? srcFile);
   }
}
