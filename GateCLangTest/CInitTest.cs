using Gate.CLanguage.Compiler;
using Gate.CLanguage.Decl;
using Gate.CLanguage.Initialisation;
using Gate.CLanguage.Source;
using Gate.LangBase.ExtraTypes;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System.Runtime.InteropServices;
using static Gate.CLanguageTest.CompileWithExternalReadBackTestBase;

namespace Gate.CLanguageTest
{
   /// <summary>
   /// 
   /// </summary>
   public class CInitTest : TestBase.Group
   {
      /// <summary>
      /// 
      /// </summary>
      public CInitTest() => AddSubTests([
         new ErrorTest("char a[4][2][3] = \"a\";",CCompilerMsgId.invalid_scalar_init , ExternalCompilerType.gcc_msys),
         new GeneralTest("char a[2][10] = { \"a\",\"b\" };",ExternalCompilerType.gcc_msys),
         new GeneralTest("char a[2][3][10] = { { \"a\",\"b\" } };",ExternalCompilerType.gcc_msys),
         new GeneralTest("char* a[4] = {\"abc\"};",ExternalCompilerType.gcc_msys),
         new GeneralTest("char* a = \"abc\";",ExternalCompilerType.gcc_msys),
         new GeneralTest("char a[] = \"ab\";",ExternalCompilerType.gcc_msys),
         new GeneralTest("char a[1][10] = { \"a\",\"b\" };",ExternalCompilerType.gcc_msys),
         new GeneralTest("char a[3] = { { { 3 } } };",ExternalCompilerType.gcc_msys),
         new GeneralTest("char ax[][3] = { \"a\" ,\"b\"};",ExternalCompilerType.gcc_msys),
         new GeneralTest("char a[2][3][10] = { \"ciao\"};",ExternalCompilerType.gcc_msys),
         new GeneralTest("char a[] = { \"ab\"};",ExternalCompilerType.gcc_msys),
         new GeneralTest("char a[5] = \"ab\";",ExternalCompilerType.gcc_msys),
         new GeneralTest("int a[][2] = { {1,2, 3}, {1,2} , {1,2}};",ExternalCompilerType.gcc_msys),
         new GeneralTest("int a[] = {1,2,};",ExternalCompilerType.gcc_msys),
         new GeneralTest("int a[] = {{1,11},{2,22},};",ExternalCompilerType.gcc_msys),
         new GeneralTest("int a = {{1}};",ExternalCompilerType.gcc_msys),
         new GeneralTest("int a[] = {};",ExternalCompilerType.gcc_msys),
         new ErrorTest("int a={}",CCompilerMsgId.empty_scalar_init,ExternalCompilerType.gcc_msys),
         new GeneralTest("int z= {3,2};", ExternalCompilerType.gcc_msys),
         new ErrorTest("struct { int f1; }z= 3;",CCompilerMsgId.invalid_scalar_init),
         new ErrorTest("int z[3]= 3;",CCompilerMsgId.invalid_scalar_init),
         new ErrorTest("struct { int f1; }z= 3;",CCompilerMsgId.invalid_scalar_init),
         new ErrorTest("int a={.f1= 2};",CCompilerMsgId.invalid_struct_init,ExternalCompilerType.gcc_msys),
         new ErrorTest("int a[]={.f1= 2};",CCompilerMsgId.invalid_struct_init,ExternalCompilerType.gcc_msys),
         new GeneralTest("int a = 3;",ExternalCompilerType.gcc_msys),
         new GeneralTest("int a[2][3][4]={ { 11,22,33} , 111 };",ExternalCompilerType.gcc_msys),
         new GeneralTest("int a[2][3]={ {1 , 2 , 3} , {11,22,33}};",ExternalCompilerType.gcc_msys),
         new GeneralTest("int a[2][3]={ 1 , 2 , 3 , {11,22,33} , 111 };",ExternalCompilerType.gcc_msys),
         new GeneralTest("int a[2][3]={ 1 , 2 , 3 , 4};",ExternalCompilerType.gcc_msys),
         new GeneralTest("int a[2]={ 1 , 2 , 3 , 4};",ExternalCompilerType.gcc_msys),
         new GeneralTest("struct { struct { int f11; }f1; int f2,f3; }z = { 1,2 , 3 , .f1 = 4 };",ExternalCompilerType.gcc_msys),
         new GeneralTest("struct { int f1[3],f2,f3; }z = {  {1,2 } , 3 , 4 };",ExternalCompilerType.gcc_msys),
         new GeneralTest("struct { int f1[3],f2,f3; }z = { 1,2 , 3 , .f1 = 4 };",ExternalCompilerType.gcc_msys),
         new GeneralTest("struct { int f1[3],f2,f3; }z = { 1,2 };",ExternalCompilerType.gcc_msys),
         new GeneralTest("struct { int f1[3],f2,f3; }z = { { 1,2 }};",ExternalCompilerType.gcc_msys),
         new GeneralTest("struct { int f1[3],f2,f3; }z[2] = { 1 , { 11,22 }};",ExternalCompilerType.gcc_msys),
      ]);

      public class GeneralTest : FromStore
      {
         public GeneralTest(string initExpr, ExternalCompilerType ExternalCompilerType = ExternalCompilerType.gcc_msys) : base(ExternalCompilerType) => Description = $"{ExternalCompilerType,-9}: '{InitExpression = initExpr}'";

         private class InnerCompileAction : CompileActionByExeType
         {
            public InnerCompileAction(GeneralTest generalTest) : base(generalTest) => GeneralTest = generalTest;

            public GeneralTest GeneralTest { get; }

            protected override TxtElabResult myCheckExeReadBack(TxtStore exeStdOutReadBack, CSource source)
            {
               var dcl = source.AllDescendant.OfType<CDeclVar>().FirstOrDefault() ?? throw new Crash();
               var pri_ali = dcl.TypeAlias.PrimitiveAlias;
               var val_ins = dcl.OwnedInit?.EffectiveInits;
               var ini = dcl.OwnedInit;

               if (pri_ali.IsScalar)
               {
                  var exp_ln = exeStdOutReadBack.Lines.FirstOrDefault(l => l.Content.Trim() != "") ?? throw new Crash();

                  if (dcl.OwnedInit is CInitialisationString ini_str)
                  {
                     if (ini_str.TokenString.RtmObjStringLiteral.AsString != exp_ln.Content)
                     {
                        Console.WriteLine($"Exp: {exp_ln} Found: {ini_str.TokenString.RtmObjStringLiteral.AsString}!");

                        return TxtElabResult.failure;

                     }
                     else { return TxtElabResult.success; }
                  }
                  else
                  {
                     var exp_val = int.Parse(exp_ln.Content);
                     var sof = dcl.TypeBase?.SizeOf;
                     var msk = 0xff;

                     switch (sof)
                     {
                        case 1: break;

                        case 2:
                           msk = 0xffff;
                           break;

                        case 4:
                           msk = -1;
                           break;

                        default: throw new Crash();
                     }

                     var eff_val = (val_ins?.FirstOrDefault()?.ScalarExpression?.ConstIntValue ?? throw new Crash()) & msk;

                     if (val_ins?.Length != 1)
                     {
                        Console.WriteLine($"Too many inits!");

                        return TxtElabResult.failure;
                     }
                     else if (exp_val != eff_val)
                     {
                        Console.WriteLine($"Exp: {exp_val:x8} Found: {ini?.ScalarExpression?.ConstIntValue:x8}!");

                        return TxtElabResult.failure;
                     }
                     else { return TxtElabResult.success; }
                  }
               }
               else
               {
                  //all values in init associated (eg 'int x[2] = {1,2,3}' 3 is excluded)
                  foreach (var sca_ini in dcl.OwnedInit?.EffectiveInits ?? [])
                  {
                     var ids = sca_ini.Indices ?? throw new Crash();

                     if (sca_ini is CInitialisationString ini_str)
                     {
                        var exp_lns = exeStdOutReadBack.Lines.Where(
                           l =>
                              !l.Content.IsBlank() &&
                              l.Content.ExtTrim().StartsWith(ids.StringVal)).Select(l => l.Content).ToArray();

                        var arr = ini_str.TokenString.RtmObjStringLiteral;
                        var len = Math.Min(arr.Sizes[0], exp_lns.Length);
                        var ch_typ = arr.CSharpItemType;

                        for (var i = 0; i < len; i++)
                        {
                           var ch_exp = exp_lns[i].Split(':')[1];
                           var ch_eff = (int)(dynamic)(arr[i].CSharpObj ?? throw new Crash());

                           if (ch_eff != int.Parse(ch_exp))
                           {
                              Console.WriteLine($"{ids + i}: Exp: {ch_exp} Found: {arr[i]}!");
                              return TxtElabResult.failure;
                           }
                        }
                     }
                     else
                     {
                        var exp_ln = exeStdOutReadBack.Lines.FirstOrDefault(
                           l => l.Content.Trim().StartsWith(sca_ini.Indices.Value.StringVal)) ?? throw new Crash();
                        var exp_val = int.Parse(exp_ln.Content.Split(':')[1]);

                        if (exp_val != (sca_ini.ScalarExpression?.ConstIntValue ?? throw new Crash()))
                        {
                           Console.WriteLine($"{ids}: Exp: {exp_val} Found: {sca_ini.ScalarExpression.ConstIntValue}!");

                           return TxtElabResult.failure;
                        }
                     }
                  }
               }

               return TxtElabResult.success;
            }

            protected override TxtStore myPrepareSourceCodeForCompile(CSource source)
            {
               var dcs = source.AllDescendant.OfType<CDeclVar>().ToArray();

               if (dcs.Length == 1)
               {
                  var dcl = dcs[0];
                  var src = new TxtStore();
                  var pri_ali = dcl.TypeAlias.PrimitiveAlias;

                  src.AddLines("#include <stdio.h>");
                  src.AddLines("void main(){");
                  src.AddLines($"{GeneralTest.InitExpression}");

                  var is_str = dcl.OwnedInit?.AllDescendant.OfType<CInitialisationString>().FirstOrDefault() != null;

                  if (pri_ali.IsScalar)
                  {
                     if (dcl.OwnedInit is CInitialisationString ini_str)
                     {
                        src.AddLines($"printf(\"%s\\n\" , {dcl.Identifier});");
                     }
                     else
                     {
                        src.AddLines($"printf(\"%d\\n\" , {dcl.Identifier});");
                     }
                  }
                  else
                  {
                     foreach (var sst in pri_ali.ScalarSubscriptAndType)
                     {
                        if (sst.type.IsBuiltIn || sst.type.IsClass)
                        {
                           src.AddLines($"printf(\"{sst.indices.StringVal}:%d\\n\" , {dcl.Identifier}{sst.indices.StringVal});");
                        }
                        else if (sst.type.IsPointer)
                        {
                           var dt = sst.type.PrimitiveAlias.DereferencedType ?? throw new Crash();

                           if (dt.IsBuiltIn && (dt.TypeBase?.TypeSpecifier == "char" || dt.TypeBase?.TypeSpecifier == "wchar_t"))
                           {
                              //init at certain index
                              var idx_ini = dcl.OwnedInit?.EffectiveInits.FirstOrDefault(i => i.Indices == sst.indices);

                              if (idx_ini is CInitialisationString ini_str)
                              {
                                 var str = ini_str.TokenString.GeneralizedString.UInt32Array;

                                 for (int i = 0; i < str.Length; i++)
                                 {
                                    src.AddLines(
                                       $"printf(\"{sst.indices.StringVal}[{i}]:%u\\n\" , {dcl.Identifier}{sst.indices.StringVal}[{i}]);");
                                 }
                              }
                              else if (idx_ini != null) { throw new Crash($"Expected a string init"); }
                           }
                        }
                     }
                  }

                  src.AddLines("}");

                  return src;
               }
               else { throw new Crash(); }
            }
         }

         protected override CompileActionType myMakeCompileAction() => new InnerCompileAction(this);

         public string InitExpression { get; }

         protected override TxtStore myGetContentStore() => new TxtStore(InitExpression);

         protected override bool myCheckCompileResult(bool compileResult, MsgCollection messages, CSource? source)
         {
            if (!compileResult)
            {
               Console.WriteLine($"Compile failed for: {InitExpression}");
               return false;
            }

            return compileResult;
         }
      }

      [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
      public unsafe delegate int Handler(LongDouble ld);

      static unsafe void Main(string[] args)
      {
         var tst = new CInitTest();
     
         tst.Go();
         Console.WriteLine(tst.ReportString);
      }
   }
}

