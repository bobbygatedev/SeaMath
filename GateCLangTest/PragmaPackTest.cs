using Gate.CLanguage.Compiler;
using Gate.CLanguage.PrePx;
using Gate.CLanguage.PrePx.Directives.PragmaKinds;
using Gate.CLanguage.Source;
using Gate.Tools;
using Gate.Tools.Message;
using Gate.Tools.Text;
using static Gate.CLanguage.PrePx.Directives.PragmaKinds.CPragmaKindPack;
using static Gate.CLanguageTest.CompileWithExternalReadBackTestBase;

namespace Gate.CLanguageTest
{
   public class PragmaPackTest : TestBase.Group
   {
      private static CompileWithExternalReadBackTestBase[] myTests = [
         new InnerPackMapTest(),
         new ErrorCheck("#pragma pack(3)","Wrong pack",CPrePxMsgId.cprepx025_pragma_pack_not_a_power_of_2),
         new ErrorCheck("#pragma pack(push)\n#pragma pack(pop)\n#pragma pack(pop)\n",
            "Wrong pop",CCompilerMsgId.not_a_pragma_pack_to_pop),
      ];

      public PragmaPackTest() : base(myTests) { }

      private class InnerPackMapTest : CompileWithExternalReadBackTestBase
      {
         private (TypeId, int?)[] myItems = [
            (TypeId.push,2),
            (TypeId.push,null),
            (TypeId.simple,8),
            (TypeId.pop,4),
            (TypeId.pop,null),
            (TypeId.pop,null),
            (TypeId.pop,null),
         ];

         public InnerPackMapTest() => Description = "Pack Map Test";

         public CPragmaKindPack[] Packs
         {
            get
            {
               var lst = new List<CPragmaKindPack>();

               foreach (var itm in myItems)
               {
                  lst.Add(new CPragmaKindPack());
                  lst.Last().Type = itm.Item1;
                  lst.Last().Pack = itm.Item2;
               }

               return lst.ToArray();
            }
         }

         protected override CompileActionType? myMakeCompileAction() => null;

         public override TxtStore TxtStoreForTest => new TxtStore(
            string.Join("\r\n", Packs.Select(p => $"#pragma {p.Rebuilt}").ToArray()) + "\r\n");

         public int BasePack => Standard?.CCompiler.Settings.Pack ?? -1;

         public int[] ExpectedResult
         {
            get
            {
               var lst = new[] { BasePack }.ToList();
               var stk = new Stack<int>();
               var cur_val = BasePack;

               foreach (var itm in myItems)
               {
                  switch (itm.Item1)
                  {
                     case TypeId.simple:
                     case TypeId.show: break;

                     case TypeId.push:
                        stk.Push(cur_val);
                        break;

                     case TypeId.pop:
                        if (stk.Count > 0) { cur_val = stk.Pop(); }
                        break;

                     case TypeId.wrong: break;
                     default: throw new Crash();
                  }

                  if (itm.Item2.HasValue) { cur_val = itm.Item2.Value; }

                  lst.Add(cur_val);
               }

               return lst.ToArray();
            }
         }

         protected override bool myCheckCompileResult(bool compileResult, MsgCollection messages, CSource? source) =>
            compileResult && (source?.PackMap?.SequenceEqual(ExpectedResult) ?? false);
      }

      static unsafe void Main(string[] args)
      {
         var tst = new PragmaPackTest();

         tst.Go();
         Console.WriteLine(tst.ReportString);
      }
   }
}