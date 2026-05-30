using Gate.CLanguage.Compiler;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Statement;
using Gate.CLanguage.TokenParse;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace GateCLang.Statement
{
   public class CStatementGotoLabel : CStatement
   {
      public CStatementGotoLabel() { }

      public class TokenInterpreter : CTokenInterpreter
      {
         private readonly And myAnd = new IsCTokenType(CTokenType.identifier, true) & new Is(":", true);

         public TokenInterpreter()
         {

         }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var res = myAnd.Perform(input, inData, ref output);

            if (res == TxtElabResult.success)
            {
               //check uniqueness of label
               throw new NotImplementedException(); //tododo
            }

            return res;
         }
      }

      public string? Label { get; set; }

      public override string? Descriptor => $"goto {Label}";

      public override string? Rebuilt => $"goto {Label};";
   }
}
