using Gate.CLanguage;
using Gate.CLanguage.Compiler;
using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclInterpreter;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaDeclInterpretArraySubscriptAndVarName : CDeclInterpretArraySubscriptAndVarName
   {
      public SeaDeclInterpretArraySubscriptAndVarName(
         CDeclInterpretContext context, CAttributesInterpret attributeInter, CExprStatementInterpreter exprInterpret) : base(context, attributeInter, exprInterpret)
      {
      }

      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         var dcl_spc = output.ItemsOnStack.OfType<CDeclSpecifiers>().FirstOrDefault();

         var res = base.Perform(input, inData, ref output);

         if (
            res == TxtElabResult.success &&
            output.TopItem is CDecl va &&
            (dcl_spc ?? throw new Crash()).TypeBase is SeaType &&
            va.TypeAlias.TypeSubscriptSet.SubscriptCount > 0)
         {
            inData.Messages.Add(SeaMathMessages.M005_SeaToBePureScalar(va.TypeAlias.TypeSubscriptSet.Subscripts[0].TxtToken));

            return TxtElabResult.failure;
         }

         return res;
      }
   }
}
