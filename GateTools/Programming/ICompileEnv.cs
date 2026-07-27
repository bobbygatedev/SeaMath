using Gate.Tools.Message;

namespace Gate.Tools.Programming
{
   /// <summary>
   /// 
   /// </summary>
   public interface ICompileEnv
   {
      /// <summary>
      /// 
      /// </summary>
      CompileEnvId Id { get; }

      /// <summary>
      /// Compile the source files into the output path.
      /// </summary>
      /// <param name="outputPath"></param>
      /// <param name="sourceFiles"></param>
      /// <param name="compileOutput"></param>
      /// <param name="messages"></param>
      /// <param name="includeDirectories"></param>
      /// <returns></returns>
      bool Compile(FileInfo outputPath, FileInfo[] sourceFiles, CompileEnvOut compileOutput, MsgCollection messages, DirectoryInfo[]? includeDirectories = null);

      /// <summary>
      /// Check installation is correct.
      /// </summary>
      /// <param name="messages"></param>
      /// <returns></returns>
      bool Register(MsgCollection messages);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="messages"></param>
      void Deregister(MsgCollection messages);
   }
}
