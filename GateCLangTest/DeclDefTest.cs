using Gate.CLanguage.Compiler;
using Gate.LangBase.Expressions;
using static Gate.CLanguageTest.CompileWithExternalReadBackTestBase;

namespace Gate.CLanguageTest
{
   /// <summary>
   /// 
   /// </summary>
   public class DeclDefTest : TestBase.Group
   {
      private static ErrorTest[] myErrorTests = new[] {
         new ErrorTest("void main(){ x+5;}",ExprSolverMessages.Id.identifier_not_found),
         new ErrorTest("int a = (int((((*))0;",CCompilerMsgId.unexpected_token),
         new ErrorTest("int (*a = 0;",CCompilerMsgId.end_of_file_reached),
         new ErrorTest("void main(){ int a = 2; int v[a]; }"),
         new ErrorTest("typedef int i_t;int i_t;",CCompilerMsgId.two_or_more_types_in_declation),
         new ErrorTest("typedef struct {int s1,s2; struct { int s1; }; };",CCompilerMsgId.redefined_identifier),
         new ErrorTest("typedef struct {struct { int s1; }; int s1,s2;};",CCompilerMsgId.redefined_identifier),
         new ErrorTest("typedef struct {struct { int s1; }; int s2,s3;};"),
         new ErrorTest("typedef struct {int a;}St;St v1[3][2];"),
         new ErrorTest("typedef struct St Stt;Stt v1;", CCompilerMsgId.incomplete_type_not_allowed),//shall cause an error 
         new ErrorTest("struct S1 v1;",CCompilerMsgId.incomplete_type_not_allowed),//shall cause an error 
         new ErrorTest("struct S1{ int a; }var;"),//shall succeed
         new ErrorTest("struct S1* v1;struct S1{ int a; }v2;"),//shall succeed
         new ErrorTest("struct S1{ int a; }v1; struct S1 v2;"),//shall succeed
         new ErrorTest("int v1 = 0; float v2 = 0;") ,
         new ErrorTest("int v1[]; int v2[5];") ,
         new ErrorTest( "int a=2;int a= 2;", CCompilerMsgId.redefined_identifier),
         new ErrorTest( "int a;int a;"),
         new ErrorTest( "int a;float a;", CCompilerMsgId.conflicting_types),
         new ErrorTest( "int f(void){ int fn(void){ } }"),
         new ErrorTest( "int f(int){ }"),
         new ErrorTest( "void tst(int); int main(){void tst(float) {} }"),
         new ErrorTest( "void tst(int); int main(){  void tst(int); void tst(float) {} }", CCompilerMsgId.conflicting_types),
         new ErrorTest("enum { l1, };"),
         new ErrorTest("enum { l1,l2 };"),
         new ErrorTest("enum { l1,l2, };"),
         new ErrorTest("typedef int FILE; char* fgets(int a, FILE* f){ return 0; }"),
         new ErrorTest("typedef int FILE; FILE* fgets(int a, FILE* f){ return 0; }"),
         new ErrorTest("typedef int FILE; int fgets(int a, FILE* f){ return 0; }"),
         new ErrorTest("int fgets(int a, int* f){ return 0; }"),
         new ErrorTest("enum { , };",CCompilerMsgId.expected_token),
         new ErrorTest("enum { };",CCompilerMsgId.empty_enum_is_invalid),
         new ErrorTest("enum { l1 = 2, l2 };"),
         new ErrorTest("enum { l1, l1 };",CCompilerMsgId.redeclared_enumerator),
         new ErrorTest("int l1;enum { l1, l2 };",CCompilerMsgId.redefined_identifier),
      };

      public DeclDefTest() : base(myErrorTests) { }

      static unsafe void Main(string[] args)
      {
         var tst = new DeclDefTest();
  
         tst.Go();
         Console.WriteLine(tst.ReportString);
      }
   }
}
