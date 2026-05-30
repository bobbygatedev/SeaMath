using Gate.CLanguage.Compiler;
using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Statement;
using Gate.CLanguage.Types;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using static Gate.CLanguage.Decl.CDeclFunction;

namespace Gate.CLanguage.DeclInterpreter
{
   /// <summary>
   /// 
   /// </summary>
   public class CDeclInterpretPartFunction : CTokenInterpreter
   {
      private And myAnd;
      private CDeclInterpretArraySubscriptAndVarName myReturnValueArraySubscriptInterpret;

      public CDeclInterpretPartFunction(
         bool isForDefinition,
         CDeclInterpretArraySubscriptAndVarName returnValuearraySubscriptInterpret,
         CExprStatementInterpreter exprInterpret,
         CAttributesInterpret attributesInterpret,
         CDeclInterpretFactory declInterpretFactory)
      {
         IsForDefinition = isForDefinition;
         myReturnValueArraySubscriptInterpret = returnValuearraySubscriptInterpret;
         DeclInterpretFactory = declInterpretFactory;

         if (isForDefinition)
         {
            myAnd = new And(
                myReturnValueArraySubscriptInterpret,
                new InnerIsFunctionCheck(this),
                new CDeclInterpretParams(CDeclInterpretContext.function_def_param, DeclInterpretFactory, exprInterpret, attributesInterpret),
                new CDeclInterpretTryToAddSpecifier(),
                new InnerIsFunctionDefValidScope(),
                new CBlockInterpret(CBlockInterpret.ContextType.function , DeclInterpretFactory, attributesInterpret, exprInterpret));
         }
         else
         {
            myAnd = new And(
               myReturnValueArraySubscriptInterpret,
               new InnerIsFunctionCheck(this),
               new CDeclInterpretParams(CDeclInterpretContext.function_decl_param, DeclInterpretFactory, exprInterpret, attributesInterpret),
               new CDeclInterpretTryToAddSpecifier());
         }
      }

      /// <summary>
      /// Check wheather this is valid scope for function definition (effective when nested function are NOT supported).
      /// </summary>
      private class InnerIsFunctionDefValidScope : CTokenInterpreter
      {
         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var dcl_fnc = output.PeekOrCrash<CDeclFunction>();
            var is_glo = dcl_fnc.ContainingScope?.IsGlobal ?? false;

            if (!is_glo && !inData.Settings.AreNestedFunctionDefSupported)
            {
               inData.Messages.Add(CCompilerMsgId.nested_function_call_not_supported.GetError(dcl_fnc.TxtToken));

               return TxtElabResult.failure;
            }
            else { return TxtElabResult.success; }
         }
      }

      private class InnerIsFunctionCheck : CTokenInterpreter
      {
         private readonly BracketHelper myBracketHelper = new BracketHelper(
            [("(", ")", TxtElabResult.failure), ("{", "}", TxtElabResult.failure), ("[", "]", TxtElabResult.failure)],
            false, null, myOutputPostCondition);

         private static bool myOutputPostCondition(TxtTokenList input, Stack<TxtToken> stack) => stack.Count == 0;

         public InnerIsFunctionCheck(CDeclInterpretPartFunction parent) => Parent = parent;

         public CDeclInterpretPartFunction Parent { get; }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            if (input.IsMarking("("))
            {
               var cur_idx = input.CurrIdx;

               var res = myBracketHelper.Check(input, out _);

               switch (res)
               {
                  case TxtElabResult.success:
                     break;

                  case TxtElabResult.continue_searching:
                     input.CurrIdx = cur_idx;
                     inData.Messages.Add(CCompilerMsgs.EndOfFileReachedOpenBracket(input?.MarkedToken, false));
                     return TxtElabResult.failure;

                  case TxtElabResult.failure:
                     var wro_tok = input.MarkedToken;

                     input.CurrIdx = cur_idx;
                     inData.Messages.Add(CCompilerMsgs.NoMatchingBracket(wro_tok, input?.MarkedToken, false));
                     return TxtElabResult.failure;

                  case TxtElabResult.failure_unrecoverable:
                  default: throw new Crash();
               }

               if (res == TxtElabResult.success && Parent.IsForDefinition)
               {
                  input.CurrIdx++;

                  var mrk_txt = input.MarkedText;

                  input.CurrIdx = cur_idx;

                  return mrk_txt == "{" ? TxtElabResult.success : TxtElabResult.continue_searching;
               }

               input.CurrIdx = cur_idx;

               return res;
            }

            return TxtElabResult.continue_searching;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public bool IsForDefinition { get; }

      /// <summary>
      /// 
      /// </summary>
      public CDeclInterpretFactory DeclInterpretFactory { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="input"></param>
      /// <param name="inData"></param>
      /// <param name="output"></param>
      /// <returns></returns>
      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         //in order to make scope objects(typedef's) be visible to function paramater scope object is added to decl specifier(already a descendant of CSource)
         var dcl_spc = output.TopItem as CDeclSpecifiers ?? throw new Crash();
         var is_tdf = dcl_spc.StorageClass == CTypeStorageClass.typedef;//is a typedef? (or a function prototype/definition)

         ///declaration to be filled may be either a <see cref="CDeclFunction"/> or a <see cref="CDeclTypedef"/> 
         var dcl = is_tdf ?
            new CDeclTypedef() :
            //our position is 'int f(->int p1, int p2)'
            (CDecl)new CDeclFunction(IsForDefinition, KindType.ordinary, dcl_spc);

         var beg_idx = input.CurrIdx;

         var res = myNested(dcl, input, inData, ref output, myAnd, NestedMode.once_continue);

         if (res == TxtElabResult.success)
         {
            /// in case a function pointer is detected dcl type shall be of type <see cref="CDeclVar"/>
            /// therefore elaboration shall be repeated
            if (dcl.TypeAlias.IsFunctionPointer)
            {
               dcl_spc.RemoveDecl(dcl);

               return TxtElabResult.continue_searching;
            }

            //in case it's first declarator inside decls specifier token includes decl_spec as well
            dcl.TxtToken = dcl == dcl?.DeclSpecifiers?.Decls[0] && dcl.DeclSpecifiers?.TxtToken != null ?
               TxtTokenConst.FromTokenInterval(dcl.DeclSpecifiers.TxtToken, input[input.CurrIdx - 1]) :
               input.GetTokenFrom(beg_idx);

            if (!inData.ScopeHelper.NnOrCrash().CheckDecl(
               inData.Messages,
               dcl ?? throw new Crash(),
               dcl_spc?.ContainingScope?.ItemWithScopeSpace ?? throw new Crash()))
            {
               return TxtElabResult.failure;
            }
         }
         else
         {
            //if not a function translated function is removed
            dcl_spc.RemoveDecl(dcl);
         }

         if (res == TxtElabResult.success && dcl is CDeclFunction fnc && fnc.Body != null)
         {
            /// add instructions to <see cref="CDeclFunction"/>
            fnc.Instructions = inData.FunctionInstructionTranslator.GetInstructions(fnc);
         }

         return res;
      }

      public override string ToString() => $"{GetType().Name}({(IsForDefinition ? "ForDefinition" : "ForDeclaration")})";
   }
}
