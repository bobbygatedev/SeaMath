using Gate.LangBase;
using Gate.Tools.Programming;

namespace Gate.CLanguageTest
{
   public static class TestObjects
   {
      public static NumericConverter NumericConverter  { get; } = NumericConverter.CImplemented.Make(new CompileEnvGcc());
   }
}
