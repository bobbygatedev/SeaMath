using Gate.Tools.Programming;

namespace Gate.CLanguageTest
{
   /// <summary>
   /// Visual studio C/C++ compiler.
   /// </summary>
   public class CompilerVcc : CompilerBase
   {
      public CompilerVcc()
      {
         
      }

      public override ICompileEnv CompileEnv => new CompileEnvMsVs();
   }
}
