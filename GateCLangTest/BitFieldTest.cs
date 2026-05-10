using Gate.CLanguage.Decl;
using Gate.CLanguage.Source;
using Gate.CLanguage.Types;
using Gate.CLanguageTest.Properties;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Programming;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System.Reflection;
using static Gate.CLanguageTest.CompileWithExternalReadBackTestBase;
using static Gate.Tools.DllInstance;

namespace Gate.CLanguageTest
{
   public class BitFieldTest : TestBase.Group
   {
      public BitFieldTest() : base("Bit Field Test", myGetTests()) { }

      private static TestBase[] myGetTests() => [new InnerTestByCompile(ExternalCompilerType.gcc_msys)];


      private class InnerTestByCompile : CompileWithExternalReadBackTestBase
      {
         public InnerTestByCompile(ExternalCompilerType externalCompiler) : base(externalCompiler) => Description = $"Bit-field test by {externalCompiler}";

         public override TxtStore TxtStoreForTest { get; } = new TxtStore(Resources.bit_field_test_c);

         protected override bool myCheckCompileResult(bool compileResult, MsgCollection messages, CSource? source) => compileResult;

         private class InternalTest : TestBase
         {
            public InternalTest(string title, bool isOk)
            {
               Description = title;
               IsOk = isOk;
            }

            public bool IsOk { get; }

            protected override TxtElabResult myExecution() => IsOk ? TxtElabResult.success : TxtElabResult.failure;

            protected override void myTestPreSet() { }
         }

         private class InnerCompileAction : CompileActionByDllType
         {
            public InnerCompileAction(InnerTestByCompile parent) : base(parent) { }

            protected override TxtElabResult myCheckDll(ExtraDllCppCode dllCode, CSource cSource)
            {
               var fns = cSource.AllDecls.OfType<CDeclFunction>().ToArray();
               var fns_get = fns.Where(fn => fn.Identifier.ExtTrim().StartsWith("get")).ToArray();
               var fns_set = fns.Where(fn => fn.Identifier.ExtTrim().StartsWith("set")).ToArray();

               dllCode.DllInstance.DefineMethods(
                  fns_get.Select(f => new MethodDefine(f.Identifier ?? throw new Crash(), typeof(Int64), [])).
                  Concat(fns_set.
                     Select(f => new MethodDefine(f.Identifier ?? throw new Crash(), typeof(void), [typeof(Int64)]))).
                  ToArray());

               var glo_vrs = cSource.AllDecls.
                  OfType<CDeclVar>().
                  Where(v => v.Identifier.ExtTrim().StartsWith("u")).
                  ToArray();

               var tps =
                  cSource.AllDecls.
                     OfType<CDeclTypedef>().
                     Select(t => t.TypeAlias?.PrimitiveAlias?.TypeBase).
                     OfType<CTypeStruct>().
                     ToArray();

               foreach (var var in glo_vrs)
               {
                  var set_u_mth = myGetMethod($"set_{var.Identifier}", dllCode);
                  var get_u_mth = myGetMethod($"get_{var.Identifier}", dllCode);
                  var id = int.Parse(var.Identifier.ExtTrim().Substring(1));

                  var get_sof_mth = myGetMethod($"get_s{id}_sof", dllCode);
                  var typ = tps.FirstOrDefault(t => t.Identifier == $"S{id}") ?? throw new Crash();

                  var sof = myInvokeGet(get_sof_mth);

                  if (sof == typ.SizeOf)
                  {
                     Parent.AddSubTests(new InternalTest($"{get_sof_mth.Name} ({sof})", true));
                  }
                  else
                  {
                     Parent.AddSubTests(new InternalTest($"{get_sof_mth.Name} (Exp={sof} Eff={var.TypeAlias.SizeOf})", false));
                  }

                  var off = 0;
                  var fls = myGetFields(typ, ref off);

                  //check bit-fields using set_u[x] and get_u[x] methods
                  foreach (var fld in fls)
                  {
                     var set_fld = myGetMethod($"set_u{id}_{fld.fieldName}", dllCode);
                     var nb = myGetFieldBits(fld.field);
                     var bf_msk = -1L + (1 << nb);
                     var int_val = bf_msk << fld.bitOffset;//internal bit-field value  

                     myInvokeSet(set_u_mth, 0);
                     myInvokeSet(set_fld, bf_msk);

                     //external value (gcc/MSVS)
                     var ext_val = myInvokeGet(get_u_mth);

                     if (ext_val == int_val)
                     {
                        Parent.AddSubTests(new InternalTest($"{set_fld.Name}", true));
                     }
                     else
                     {
                        Parent.AddSubTests(new InternalTest($"{set_fld.Name} ({Parent.ExternalCompilerId} 0x{ext_val:x} Sea 0x{int_val:x})", false));
                     }
                  }
               }

               return TxtElabResult.success;
            }
         }

         private static (CDeclClassField field, string fieldName, int bitOffset)[] myGetFields(CTypeStruct typeStruct, ref int bitOffset)
         {
            var fls = typeStruct.Fields;
            var lst = new List<(CDeclClassField field, string fieldName, int bitOffset)>();

            foreach (var fld in fls)
            {
               if (fld.TypeAlias.PrimitiveAlias.IsBuiltIn)
               {
                  var bit_sof = fld.BitSizeof;

                  //rounding of bit field ..
                  if (fld.BitFieldNumBits.HasValue)
                  {
                     var byt_siz = fld.TypeAlias.SizeOf * 8;

                     var old_byt = bitOffset / byt_siz;
                     var new_bo = bitOffset + bit_sof;
                     var new_byt = new_bo / byt_siz;

                     //if bit field falls in next byte , then we need to round it
                     if (old_byt != new_byt)
                     {
                        bitOffset = new_byt * byt_siz;
                     }
                  }

                  lst.Add((fld, fld.Identifier.ExtTrim(), bitOffset));

                  bitOffset += bit_sof;
               }
               else if (fld.TypeAlias.PrimitiveAlias.IsClass)
               {
                  var str = fld.TypeAlias.PrimitiveAlias.TypeBase as CTypeStruct ?? throw new Crash();
                  var tps = myGetFields(str, ref bitOffset);

                  lst.AddRange(tps.Select(t => (t.field, $"{fld.Identifier}_{t.fieldName}", t.bitOffset)));
               }
            }

            return lst.ToArray();
         }

         private static int myGetFieldBits(CDeclClassField field) => field.BitFieldNumBits ?? field.TypeAlias.SizeOf * 8;

         private static MethodInfo myGetMethod(string method, ExtraDllCppCode dllCode) =>
            dllCode.DllInstance.DefinedMethods.FirstOrDefault(m => m.Name == method) ?? throw new Crash();

         private static void myInvokeSet(MethodInfo method, Int64 val) => method.Invoke(null, [val]);

         private static Int64 myInvokeGet(MethodInfo method) => (Int64)(method.Invoke(null, [])?? throw new Crash());

         protected override CompileActionType myMakeCompileAction() => new InnerCompileAction(this);
      }

      static unsafe void Main(string[] args)
      {
         var tst = new BitFieldTest();

         tst.Go();

         Console.WriteLine(tst.ReportString);
      }
   }
}