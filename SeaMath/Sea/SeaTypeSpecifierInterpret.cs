using Gate.CLanguage.Compiler;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.TokenParse;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using static Gate.CLanguage.TypeSpecifierInterpret.CTypeSpecifierInterpret;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// <see cref="CTokenInterpreter"> for variant type name "sea".
   /// </summary>
   public class SeaTypeSpecifierInterpret : CTokenInterpreter
   {
      public SeaTypeSpecifierInterpret(UsageId usage) => Usage = usage;

      public UsageId Usage { get; }

      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         var set = inData.Settings;
         var tok = input.Peek<CToken>();

         if (tok?.TokenType == CTokenType.keyword && tok.Content == SeaType.NAME)
         {
            var typ_bas = GetOutputTypeBase(Usage, output);

            input.Dequeue();

            if (typ_bas == null)
            {
               SetOutputTypeBase(Usage, output, SeaType.Instance);
            
               return TxtElabResult.success;
            }
            else
            {
               inData.Messages.Add(SeaMathMessages.M002_NoExtraSpecifiersWithSea(tok));

               return TxtElabResult.failure;
            }
         }

         return TxtElabResult.continue_searching;
      }
   }
}
