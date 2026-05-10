using Gate.CLanguage.Compiler;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.TokenParse;
using Gate.CLanguage.Types;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using static Gate.CLanguage.TypeSpecifierInterpret.CTypeSpecifierInterpret;

namespace Gate.CLanguage.TypeSpecifierInterpret
{
   /// <summary>
   /// 
   /// </summary>
   public class CTypeSpecifierInterpretBuiltIn : CTokenInterpreter
   {
      public CTypeSpecifierInterpretBuiltIn(UsageId usage) => Usage = usage;

      public UsageId Usage { get; }

      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         var set = inData.Settings;
         var tok = input.Peek<CToken>();

         if (tok?.TokenType == CTokenType.keyword)
         {
            var bin_new = inData.Settings.BuiltInSet?.FirstOrDefault(t => t.TypeSpecifier == tok.Content);

            if (bin_new != null)
            {
               var typ_bas = GetOutputTypeBase(Usage, output);

               input.Dequeue();

               if (typ_bas == null) { SetOutputTypeBase(Usage, output ?? throw new Crash(), bin_new); }
               else if (typ_bas is CTypeBuiltIn bin_old)
               {
                  var bin_cmp = null as CTypeBuiltIn;

                  var res = myCompose(tok, bin_old, inData, out bin_cmp);

                  if (res == TxtElabResult.success) { SetOutputTypeBase(Usage, output ?? throw new Crash(), bin_cmp); }

                  return res;
               }
               else
               {
                  inData.Messages.Add(CCompilerMsgId.two_or_more_types_in_declation.GetError(tok));

                  return TxtElabResult.failure;
               }

               return TxtElabResult.success;
            }
         }

         return TxtElabResult.continue_searching;
      }

      public override string ToString() => $"{GetType().Name}{Usage}";

      private TxtElabResult myCompose(CToken token, CTypeBuiltIn builtInOld, CCompilerInData inData, out CTypeBuiltIn? binComposed)
      {
         var old_wds = builtInOld.TypeSpecifier.Split(" ".ToArray(), StringSplitOptions.RemoveEmptyEntries);
         var mrg_wds = old_wds.Concat([token.Content]).Cast<string?>().ToArray();

         if (mrg_wds.Count(w => w == "signed") > 1 || mrg_wds.Count(w => w == "unsigned") > 1)
         {
            if (inData.Settings.AreDuplicatedUnsignedAllowed)
            {
               var ids = Enumerable.Range(0, mrg_wds.Length).Where(i => mrg_wds[i] == "signed").ToArray();

               //put to null where word == signed except the first one
               foreach (var id in ids.Skip(1)) { mrg_wds[id] = null; }

               ids = Enumerable.Range(0, mrg_wds.Length).Where(i => mrg_wds[i] == "unsigned").ToArray();

               //put to null where word == unsigned except the first one
               foreach (var id in ids.Skip(1)) { mrg_wds[id] = null; }

               mrg_wds = mrg_wds.Where(m => m != null).ToArray();
            }
            else
            {
               binComposed = null;
               inData.Messages.Add(CCompilerMsgId.duplicated_type_modifier.GetError(token));

               return TxtElabResult.failure;
            }
         }

         //search for built-in types among the 
         binComposed = inData.Settings.BuiltInSet?.FirstOrDefault(b =>
         {
            var b_spl = b.TypeSpecifier.Split(" ".ToArray(), StringSplitOptions.RemoveEmptyEntries).OrderBy(b1 => b1).ToArray();

            return mrg_wds.OrderBy(w => w).SequenceEqual(b_spl);
         });

         if (binComposed == null)
         {
            inData.Messages.Add(CCompilerMsgs.InvalidBuiltInType(token, string.Join(" ", mrg_wds)));

            return TxtElabResult.failure;
         }
         else
         {
            return TxtElabResult.success;
         }
      }
   }
}
