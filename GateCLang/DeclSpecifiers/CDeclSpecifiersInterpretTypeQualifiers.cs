using Gate.CLanguage.Compiler;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.TokenParse;
using Gate.CLanguage.Types;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.DeclSpecifiers
{
   /// <summary>
   /// Const/volatile/restrict
   /// </summary>
   public class CDeclSpecifiersInterpretTypeQualifiers : CTokenInterpreter
   {
      private static string[] my_TypeQualifiersLabel = Enum.GetNames(typeof(CTypeQualifiersFlags)).ToArray();

      private And myComposed = new And(
         new IsCTokenType(CTokenType.keyword, false),
         new Condition(tl => my_TypeQualifiersLabel.Contains(tl?.Peek()?.Content), false),
         new Inner());

      private class Inner : CTokenInterpreter
      {
         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var typ_qlf = (CTypeQualifiersFlags)Enum.Parse(typeof(CTypeQualifiersFlags), input.Dequeue()?.Content ?? throw new Crash());

            if (typ_qlf == CTypeQualifiersFlags.restrict)
            {
               throw new NotImplementedException("Restrict not implemented!");
            }
            else
            {
               if (output.Peek() is CDeclSpecifiers dcl_spc)
               {
                  dcl_spc.TypeQualifiers |= typ_qlf;
               }
               else if (output.Peek() is CTypeAlias ali)
               {
                  ali.TypeQualifiers |= typ_qlf;
               }

               return TxtElabResult.success;
            }
         }
      }

      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         var res = myComposed.Perform(input, inData, ref output);

         return res;
      }
   }
}
