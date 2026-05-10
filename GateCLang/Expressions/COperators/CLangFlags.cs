namespace Gate.CLanguage.Expressions.COperators
{
   public enum CLangFlags
   {
      none = 0x0 ,
      c = 0x1 ,
      cpp = 0x2 ,
      gnu = 0x4 ,
      ms = 0x8 ,
      all = c | cpp | gnu | ms ,
      cpp_only = cpp | gnu | ms ,
      c_only = c | gnu | ms,
      msvc = c | ms ,
      gcc = c | gnu ,
      gcpp = cpp | gnu,
   }
}
