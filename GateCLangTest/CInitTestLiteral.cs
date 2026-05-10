using Gate.CLanguage.Compiler;
using static Gate.CLanguageTest.CInitTest;
using static Gate.CLanguageTest.CompileWithExternalReadBackTestBase;

namespace Gate.CLanguageTest
{
   /// <summary>
   /// For string and chars
   /// </summary>
   public class CInitTestLiteral : TestBase.Group
   {
      public readonly string[] STRING_TESTS = [""];

      /// <summary>
      /// \U0001D161  //U+1D161 = MUSICAL SYMBOL SIXTEENTH NOTE
      /// \u00e8      //u+00e8  = è
      /// </summary>
      public CInitTestLiteral() => AddSubTests(new TestBase[] {
            new GeneralTest(@"unsigned int a = '\U0010FFFF';"),
            new GeneralTest(@"unsigned int a = '\U000000a1uiop';"),

            new GeneralTest("unsigned int a = u'\\U00001001a';" ),
            new GeneralTest("unsigned int a = u'\\U00001001';"),
            new GeneralTest("unsigned int a = u'\\U00011001';"),

            new GeneralTest("unsigned int a = U'\\U7fffffff';"),
            new ErrorTest("unsigned int a = U'\\UA0000000';",CCompilerMsgId.not_a_valid_universal_character),

            new ErrorTest("unsigned int a = '\\U0000DFFF';", CCompilerMsgId.not_a_valid_universal_character ),
            new ErrorTest("unsigned int a = '\\U0000D800';", CCompilerMsgId.not_a_valid_universal_character ),
            new ErrorTest("unsigned int a[15] = U\"\\U00000000\";", CCompilerMsgId.not_a_valid_universal_character),

            new GeneralTest("unsigned int a[15] = U\"\\U0001D161\";"),
            new GeneralTest("unsigned int a[] = U\"\\xaaaa0000zz\";"),
            new GeneralTest("char a[] = \"è\";"),

            new GeneralTest("char a[] = \"abc\";"),
            new GeneralTest("char a[] = \"abc\"  \"ab\";"),

            new GeneralTest("unsigned int a = U'\\U000100000';"),//excess char (cuases an error ON msvs)
            new GeneralTest("unsigned int a = u'\\u00e8';"),
            new GeneralTest("unsigned int a = u'a\\u00e8';"),
            new GeneralTest("unsigned int a = u'\\u00e8a';"),

            new GeneralTest("char a = 'a';"),
            new GeneralTest("char a = 'ab';"),
            new GeneralTest("char a = 'abc';"),
            new GeneralTest("char a = 'abcd';"),
            new GeneralTest("char a = 'abcde';"),
            new GeneralTest("char a = '\\1';"),
            new GeneralTest("char a = '\\12';"),
            new GeneralTest("char a = '\\123';"),
            new GeneralTest("char a = 'ab';"),

            new GeneralTest("__WCHAR_TYPE__ a[] = L\"abc\";"),
            new GeneralTest("__WCHAR_TYPE__ a[] = \"abc\" L\"d\";"),
            new GeneralTest("__WCHAR_TYPE__ a = L'\\u00e8';"),
            new GeneralTest("__WCHAR_TYPE__ a = L'\\u00e8a';"),
            new GeneralTest("__WCHAR_TYPE__ a = L'a\\u00e8b';"),
            new GeneralTest("__WCHAR_TYPE__ w[] = L\"\\u00e8\";"),
            new ErrorTest(@"__WCHAR_TYPE__ a[] = L""\uxx"";", CCompilerMsgId.not_a_valid_universal_character ),
         });

      static unsafe void Main(string[] args)
      {
         var tst = new CInitTestLiteral();
   
         tst.Go();
         Console.WriteLine(tst.ReportString);
      }
   }
}
