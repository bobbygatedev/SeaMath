using Gate.CLanguage.Compiler;
using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Initialisation;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Types;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.DeclInterpreter
{
   /// <summary>
   /// Interpreter for a SINGLE variable declaration/definition plus initialization instance (eg 'a=0' in 'int a = 0 )
   /// </summary>
   public class CDeclInterpretPartVar : CTokenInterpreter
   {
      private readonly And myAnd;
      private readonly May myMayDeclInterpretParams;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="context"></param>
      /// <param name="attributesInterpret"></param>
      /// <param name="arraySubInter"></param>
      /// <param name="initInter"></param>
      /// <param name="interpreterBitField"></param>
      /// <param name="declInterpretFactory"></param>
      /// <param name="exprInterpret"></param>
      public CDeclInterpretPartVar(
         CDeclInterpretContext context,
         CAttributesInterpret attributesInterpret,
         CDeclInterpretArraySubscriptAndVarName arraySubInter,
         CInitialisationInterpret? initInter,
         CDeclInterpreterBitField? interpreterBitField,
         CDeclInterpretFactory declInterpretFactory,
         CExprStatementInterpreter exprInterpret)
      {
         Context = context;
         AttributesInterpret = attributesInterpret;
         InitializerInterpret = initInter;
         ArraySubscriptAndIntentfierInterpreter = arraySubInter;
         InterpreterBitField = interpreterBitField;
         myMayDeclInterpretParams = new May(new CDeclInterpretParams(Context, declInterpretFactory, exprInterpret, attributesInterpret));
         myAnd = myGetComposedInterpreter();
      }

      /// <summary>
      /// 
      /// </summary>
      public CDeclInterpreterBitField? InterpreterBitField { get; }

      /// <summary>
      /// 
      /// </summary>
      public CInitialisationInterpret? InitializerInterpret { get; }

      /// <summary>
      /// 
      /// </summary>
      public CDeclInterpretContext Context { get; }

      /// <summary>
      /// 
      /// </summary>
      public CAttributesInterpret AttributesInterpret { get; }

      /// <summary>
      /// Interpreter for <see cref="CDeclVar"/> array subscript.
      /// </summary>
      public CDeclInterpretArraySubscriptAndVarName ArraySubscriptAndIntentfierInterpreter { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="input"></param>
      /// <param name="inData"></param>
      /// <param name="output"></param>
      /// <returns></returns>
      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         var dcl_spc = output.PeekOrCrash<CDeclSpecifiers>();
         var beg_idx = input.CurrIdx;
         var dcl = myGetFreshDecl(dcl_spc);

         var res = myNested(dcl, input, inData, ref output, myAnd, NestedMode.once_continue);

         if (res != TxtElabResult.success)
         {
            if (dcl.DeclSpecifiers != null) { dcl_spc.RemoveDecl(dcl); }
         }
         else
         {
            //in case it's first declarator inside decls specifier token includes decl_spec as well
            dcl.TxtToken = dcl == dcl?.DeclSpecifiers?.Decls.ElementAtOrDefault(0) && dcl.DeclSpecifiers.TxtToken != null ?
               TxtTokenConst.FromTokenInterval(dcl.DeclSpecifiers.TxtToken, input[input.CurrIdx - 1]) :
               input.GetTokenFrom(beg_idx);
         }

         return res;
      }

      private CDecl myGetFreshDecl(CDeclSpecifiers declSpecifiers)
      {
         if (Context == CDeclInterpretContext.cclass_field)
         {
            return new CDeclClassField();
         }
         else
         {
            return declSpecifiers.StorageClass == CTypeStorageClass.typedef ? new CDeclTypedef() : new CDeclVar();
         }
      }

      private And myGetComposedInterpreter()
      {
         switch (Context)
         {
            case CDeclInterpretContext.console_var:
            case CDeclInterpretContext.local_var:
            case CDeclInterpretContext.global_var:
               return new And(
                  ArraySubscriptAndIntentfierInterpreter,
                  myMayDeclInterpretParams,
                  new CDeclInterpretTryToAddSpecifier(),
                  new May(new Is("=", true) & (InitializerInterpret ?? throw new Crash())));

            case CDeclInterpretContext.function_decl_param:
            case CDeclInterpretContext.function_def_param:
               return new And(ArraySubscriptAndIntentfierInterpreter, myMayDeclInterpretParams, new CDeclInterpretTryToAddSpecifier());

            case CDeclInterpretContext.cclass_field:
               return new And(
                  ArraySubscriptAndIntentfierInterpreter, 
                  myMayDeclInterpretParams, 
                  InterpreterBitField ?? throw new Crash(), 
                  new CDeclInterpretTryToAddSpecifier());

            case CDeclInterpretContext.expr_type_name:
            case CDeclInterpretContext.function_id:
            case CDeclInterpretContext.class_method:
            default:
               throw new Crash();
         }
      }
   }
}
