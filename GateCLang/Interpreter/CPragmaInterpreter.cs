using Gate.CLanguage.Compiler;
using Gate.CLanguage.PrePx.Directives;
using Gate.CLanguage.PrePx.Directives.PragmaKinds;
using Gate.CLanguage.Source;
using Gate.Tools;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Interpreter
{
   /// <summary>
   /// Standard pragma interpreter (interprets pragma pack only).
   /// </summary>
   public class CPragmaInterpreter
   {
      public virtual TxtElabResult Interpret(CSource source, CCompilerInData inData) => myInterpretPragmaPack(source, inData);

      protected TxtElabResult myInterpretPragmaPack(CSource source, CCompilerInData inData)
      {
         var ppx_src = source.PrePxSource ?? throw new Crash();
         var prg_pks = ppx_src.PrePxProducts.OfType<CPrePxDirectivePragma>().
            Select(p => p.PragmaKind).
            OfType<CPragmaKindPack>().
            ToArray();

         var stk = new Stack<int>();
         var cur_pak = inData.Settings.Pack;

         source.PackMap =
            Enumerable.Range(0, ppx_src?.ToCompileStore?.LineCount ?? 0).Select(_ => -1).ToArray();

         foreach (var pak in prg_pks)
         {
            var pak_ln = pak.ParentItem?.TxtToken?.From?.Line ?? throw new Crash();

            switch (pak.Type)
            {
               case CPragmaKindPack.TypeId.simple:
                  myFillTo(source, cur_pak, pak_ln);
                  cur_pak = pak.Pack ?? throw new Crash();
                  break;

               case CPragmaKindPack.TypeId.push:
                  myFillTo(source, cur_pak, pak_ln);
                  stk.Push(cur_pak);

                  if (pak.Pack.HasValue) { cur_pak = pak.Pack.Value; }
                  break;

               case CPragmaKindPack.TypeId.pop:
                  myFillTo(source, cur_pak, pak_ln);

                  if (stk.Count > 0)
                  {
                     cur_pak = stk.Pop();

                     if (pak.Pack.HasValue) { cur_pak = pak.Pack.Value; }
                  }
                  else { inData.Messages.Add(CCompilerMsgs.NotPackToPop(pak)); }

                  break;

               case CPragmaKindPack.TypeId.show:
                  inData.Messages.Add(CCompilerMsgs.PragmaShow(pak, cur_pak));
                  break;

               case CPragmaKindPack.TypeId.wrong: break;

               default: throw new Crash();
            }
         }

         myFillTo(source, cur_pak, source?.PrePxSource?.ToCompileStore?.LineCount ?? 0);

         return TxtElabResult.success;
      }

      /// <summary>
      /// Fills pack map with <paramref name="currentPack"/> in range [0,<paramref name="toLineId"/>-1] where pack map[i] != -1. 
      /// </summary>
      /// <param name="source"></param>
      /// <param name="currentPack">Pack to </param>
      /// <param name="toLineId"></param>
      private static void myFillTo(CSource source, int currentPack, int toLineId)
      {
         for (var i = 0; i < toLineId; i++)
         {
            if (source.PackMap?.ElementAtOrDefault(i) == -1)
            {
               source.PackMap[i] = currentPack;
            }
         }
      }
   }
}
