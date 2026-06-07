using Gate.CLanguage.Compiler;
using Gate.CLanguage.Decl;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Statement;
using Gate.CLanguage.TokenParse;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace GateCLang.Statement
{
   public class CStatementGoto : CStatement
   {
      public CStatementGoto() { }

      public class TokenInterpreter : CTokenInterpreter
      {
         private readonly And myAnd = 
            new Is("goto", true) &
            new IsCTokenType(CTokenType.identifier, true) & 
            new Expect(";",true);

         public TokenInterpreter() { }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var res = myAnd.Perform(input, inData, ref output);

            if (res == TxtElabResult.success)
            {
               var got = new CStatementGoto();

               got.LabelName = input[input.CurrIdx - 2].Content;
               got.TxtToken = TxtTokenConst.FromTokenInterval(input[input.CurrIdx - 3], input[input.CurrIdx - 1]);

               //check uniqueness of label
               if (output.TopItem is CStatementCompound cmp)
               {
                  cmp.AddStatements(got);

                  return TxtElabResult.success;
               }
               else if (output.TopItem is CStatementConditional cyc)
               {
                  cyc.SetBody(got);

                  return TxtElabResult.success;
               }
               else
               {
                  throw new Crash();
               }
            }

            return res;
         }
      }
      
      public string? LabelName { get; set; }

      public override string? Descriptor => $"{LabelName}:";

      public override string? Rebuilt => $"{LabelName}:";

      public CDeclFunction? Function => ParentItemChain.OfType<CDeclFunction>().FirstOrDefault();

      public CStatementGotoLabel? TargetLabel => Function?.Body?.Labels.FirstOrDefault(l => l.Name == LabelName);
   }
}
