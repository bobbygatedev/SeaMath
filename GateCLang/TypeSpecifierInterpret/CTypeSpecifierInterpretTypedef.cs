using Gate.CLanguage.Compiler;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.TokenParse;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using static Gate.CLanguage.TypeSpecifierInterpret.CTypeSpecifierInterpret;

namespace Gate.CLanguage.TypeSpecifierInterpret
{
   /// <summary>
   /// 
   /// </summary>
   public class CTypeSpecifierInterpretTypedef : CTokenInterpreter
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="usage"></param>
      public CTypeSpecifierInterpretTypedef(UsageId usage) => Usage = usage;

      /// <summary>
      /// 
      /// </summary>
      public UsageId Usage { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="input"></param>
      /// <param name="inData"></param>
      /// <param name="output"></param>
      /// <returns></returns>
      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         var ctk = null as CToken;

         if (input.IsIn && (ctk = input.Peek<CToken>())?.TokenType == CTokenType.identifier)
         {
            var typ_bas = GetOutputTypeBase(Usage, output);
            var itm_scp = output.ScopeSpaceItem;
            var tdf = itm_scp?.Scope.TypedefsFunctionVisible.FirstOrDefault(t => t.Identifier == ctk.Content);

            if (tdf != null)
            {
               if (typ_bas == null)
               {
                  SetOutputTypeBase(Usage, output, tdf.TypeAlias);
                  input.CurrIdx++;

                  return TxtElabResult.success;
               }
               else
               {
                  inData.Messages.Add(CCompilerMsgId.two_or_more_types_in_declation.GetError(ctk));

                  return TxtElabResult.failure;
               }
            }
         }

         return TxtElabResult.continue_searching;
      }
   }
}
