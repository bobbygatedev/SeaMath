using Gate.CLanguage.Compiler;
using Gate.CLanguage.Decl;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.DeclInterpreter
{
   /// <summary>
   /// Interpreter for function declaration/definition and function pointer 
   /// parameters list (eg '(int a, float b, ...)' in 'void func(int a, float b, ...)' )
   /// </summary>
   public class CDeclInterpretParams : CTokenInterpreter
   {
      private And myAnd;

      public CDeclInterpretParams(
         CDeclInterpretContext context,
         CDeclInterpretFactory declInterpretFactory,
         CExprStatementInterpreter exprInterpret,
         CAttributesInterpret attributesInterpret)
      {
         Context = context;
         DeclInterpretFactory = declInterpretFactory;
         ExprInterpret = exprInterpret;
         myAnd = new And(new Is("(", true), new InnerParams(attributesInterpret, this), new Expect(")", true));
      }

      private class InnerParams : CTokenInterpreter
      {
         private And myVaArgs = new Is(",", true) & new Is("...", true);

         public InnerParams(CAttributesInterpret attributesInterpret, CDeclInterpretParams interpretParams)
         {
            AttributesInterpret = attributesInterpret;
            InterpretParams = interpretParams;
            SingleParamInterpreter = myGetSingleParamInterpreter();
         }

         private CDeclInterpret myGetSingleParamInterpreter()
         {
            var par_ctx = InterpretParams.Context;

            switch (par_ctx)
            {
               case CDeclInterpretContext.function_decl_param:
               case CDeclInterpretContext.function_def_param:
                  break;

               //function pointer params interpret
               case CDeclInterpretContext.console_var:
               case CDeclInterpretContext.local_var:
               case CDeclInterpretContext.global_var:
               case CDeclInterpretContext.expr_type_name:
               case CDeclInterpretContext.cclass_field:
                  //always declaration params context is used, since param name is optional
                  par_ctx = CDeclInterpretContext.function_decl_param;
                  break;

               case CDeclInterpretContext.class_method:
               case CDeclInterpretContext.function_id:
               default:
                  throw new Crash();
            }


            return new CDeclInterpret(par_ctx, InterpretParams.DeclInterpretFactory, InterpretParams.ExprInterpret, AttributesInterpret);
         }

         public CAttributesInterpret AttributesInterpret { get; }

         public CDeclInterpretParams InterpretParams { get; }

         public CDeclInterpret SingleParamInterpreter { get; }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var dcl = output.PeekOrCrash<CDecl>();

            //function parameters container
            var fun_prs = dcl.TypeAlias.FunctionContainer;

            if (fun_prs == null)
            {
               //we likely parsing a function parameter which is a function pointer
               //(eg 'int func( int (*fptr)(int) )' ), in this case we need to create function container for this parameter
               dcl.TypeAlias.SetAsFunction(true);
               fun_prs = dcl.TypeAlias.FunctionContainer;
            }

            //interpret parameters list (we are at int func(-> int p1 ) 
            var res = myNested(
               fun_prs, input, inData, ref output,
               new May(new Lst(",", SingleParamInterpreter, false)), NestedMode.once_not_continue);

            if (res == TxtElabResult.success)
            {
               if (
                  InterpretParams.Context == CDeclInterpretContext.function_def_param &&
                  !inData.Settings.CanFunctionDefinitionParamsBeAnonimous &&
                  fun_prs != null && !fun_prs.IsVoid && fun_prs.Parameters.Any(p => p.IsAnonimous))
               {
                  //anonimous params (eg  'int func(int )' ) not valid in this context
                  inData.Messages.Add(
                     CCompilerMsgId.expected_identifier.GetError(
                        fun_prs.Parameters.FirstOrDefault(p => p.IsAnonimous)?.TxtToken));

                  return TxtElabResult.failure;
               }

               var re1 = myVaArgs.PerformNoOutput(input, inData);

               if (re1 == TxtElabResult.success)
               {
                  var fnc_cnt = output.PeekOrCrash<CDeclFunction>().FunctionContainer.NnOrCrash();

                  fnc_cnt.HasVarArgs = true;
               }
            }

            return res;
         }
      }

      public CDeclInterpretContext Context { get; }
      public CDeclInterpretFactory DeclInterpretFactory { get; }
      public CExprStatementInterpreter ExprInterpret { get; }

      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output) =>
         myAnd.Perform(input, inData, ref output);
   }
}
