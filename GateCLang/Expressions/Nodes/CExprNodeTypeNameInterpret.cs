using Gate.CLanguage.Compiler;
using Gate.CLanguage.DeclInterpreter;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Types;
using Gate.LangBase.Expressions.Nodes;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using static Gate.CLanguage.TypeSpecifierInterpret.CTypeSpecifierInterpret;

namespace Gate.CLanguage.Expressions.Nodes
{
   /// <summary>
   /// Match for a type enclosed by '()' (eg '(int*)' '(const void*)'
   /// </summary>
   public class CExprNodeTypeNameInterpreter : ExprNodeInterpret<CCompilerInData, CTokenInterpreterOutput>
   {
      private readonly InnerTokenInterpreter myTokenInterpreter;

      public CExprNodeTypeNameInterpreter(
         CDeclInterpretFactory declInterpretFactory, CExprStatementInterpreter exprInterpret, CAttributesInterpret attributesInterpret)
      {
         ExprInterpret = exprInterpret;
         DeclInterpretFactory = declInterpretFactory;
         AttributesInterpret = attributesInterpret;
         myTokenInterpreter = new InnerTokenInterpreter(this);
      }

      private class InnerTokenInterpreter : CTokenInterpreter
      {
         private readonly And myAnd;

         public InnerTokenInterpreter(CExprNodeTypeNameInterpreter typeNameInterpreter)
         {
            TypeNameInterpreter = typeNameInterpreter;

            var typ_spc_int = TypeNameInterpreter.DeclInterpretFactory.MakeTypeSpecifierInterprer(
               typeNameInterpreter.ExprInterpret,
               typeNameInterpreter.AttributesInterpret,
               UsageId.type_name);

            var arr_sub_int = new CDeclInterpretArraySubscriptAndVarName(
               CDeclInterpretContext.expr_type_name, typeNameInterpreter.AttributesInterpret, null);

            var atr_int = new IterateWhileSuccess(typeNameInterpreter.AttributesInterpret);

            myAnd =
               new Is("(", true) &
               new Lst((atr_int & typ_spc_int & atr_int | new CDeclSpecifiersInterpretTypeQualifiers()) &
                  arr_sub_int) &
               new Expect(")", true);
         }

         public CExprNodeTypeNameInterpreter TypeNameInterpreter { get; }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var typ_ali = new CTypeAlias();
            var beg_idx = input.CurrIdx;

            output.Push(typ_ali);//this causes type-alias is removed at the and of process

            var res = myNested(typ_ali, input, inData, ref output, myAnd, NestedMode.once_continue);

            if (res == TxtElabResult.success)
            {
               typ_ali.TxtToken = input.GetTokenFrom(beg_idx);
            }

            return res;
         }
      }

      public CDeclInterpretFactory DeclInterpretFactory { get; }

      public CExprStatementInterpreter ExprInterpret { get; }

      public CAttributesInterpret AttributesInterpret { get; }

      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref ExprNodeOutput<CTokenInterpreterOutput> output)
      {
         var dcl_in_out = output.InterpreterOutput.NnOrCrash();
  
         var res = myTokenInterpreter.Perform(input, inData, ref dcl_in_out);

         switch (res)
         {
            case TxtElabResult.success:
               output.InterpreterOutput = dcl_in_out;//could be updated

               var typ_ali = dcl_in_out?.PopOrCrash<CTypeAlias>() ?? throw new Crash();
               var exp = new CExprNodeTypeName(typ_ali);

               output.ListProduct.Add(exp);

               return res;

            case TxtElabResult.continue_searching:
               var dcl_in_out_clo = (dcl_in_out?.Clone() as CTokenInterpreterOutput).NnOrCrash();

               output.InterpreterOutput = dcl_in_out_clo;
               return res;

            case TxtElabResult.failure:
            case TxtElabResult.failure_unrecoverable:
               return res;

            default: throw new Crash();
         }
      }
   }
}
