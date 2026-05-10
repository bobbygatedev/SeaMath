using Gate.Tools.Programming;

namespace Gate.CLanguageTest
{
   /// <summary>
   /// 
   /// </summary>
   public class CompilerGcc : CompilerBase
   {
      public CompilerGcc()
      {
         
      }

      public override ICompileEnv CompileEnv => new CompileEnvGcc();

      public string OptionText => $"{(AreWarningEnabled ? "" : " -w")}";
   }
}
