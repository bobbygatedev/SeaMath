using Gate.CLanguage.Compiler;
using Gate.CLanguage.DeclInterpreter;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Expressions.COperators;
using Gate.CLanguage.Source;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Interpreter
{
   /// <summary>
   /// Using factory method pattern.
   /// </summary>
   public class CInterpreter
   {
      private CDeclInterpretFactory? myDeclInterpretFactory;
      private CAttributesInterpret? myAttributesInterpret;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="langFlags"></param>
      public CInterpreter(CLangFlags langFlags)
      {
         LangFlags = langFlags;
         ExprInterpret = myMakeExprStatementInterpreter();
      }

      /// <summary>
      /// 
      /// </summary>
      public CLangFlags LangFlags { get; }

      /// <summary>
      /// 
      /// </summary>
      public CExprStatementInterpreter ExprInterpret { get; }

      /// <summary>
      /// 
      /// </summary>
      public CDeclInterpretFactory DeclInterpretFactory =>
         myDeclInterpretFactory = myDeclInterpretFactory ?? myMakeCDeclInterpretFactory();

      /// <summary>
      /// 
      /// </summary>
      public CAttributesInterpret AttributesInterpret =>
         myAttributesInterpret = myAttributesInterpret ?? myMakeAttributesInterpret();

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected virtual CExprStatementInterpreter myMakeExprStatementInterpreter() => new CExprStatementInterpreter(DeclInterpretFactory, AttributesInterpret, LangFlags);

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected virtual CAttributesInterpret myMakeAttributesInterpret() => new CAttributesInterpret();

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected virtual CTokenInterpreter[] myMakeRootSubInterpreters() =>
         new[] { new CDeclInterpret(CDeclInterpretContext.global_var, DeclInterpretFactory, ExprInterpret, AttributesInterpret) };

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected virtual CDeclInterpretFactory myMakeCDeclInterpretFactory() => new CDeclInterpretFactory();

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected virtual CPragmaInterpreter myMakePragmaInterpreter() => new CPragmaInterpreter();

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected virtual CSourceInterpreter myMakeSourceInterpreter() => new CSourceInterpreter(myMakeRootSubInterpreters());

      /// <summary>
      /// 
      /// </summary>
      /// <param name="tokenList"></param>
      /// <param name="inData"></param>
      /// <param name="source"></param>
      /// <returns></returns>
      public TxtElabResult Start(TxtTokenList tokenList, CCompilerInData inData, ref CSource source)
      {
         var itr_out = new CTokenInterpreterOutput();
         var src_itr = myMakeSourceInterpreter();

         source = source ?? new CSource();

         var res = myMakePragmaInterpreter().Interpret(source, inData);

         if (res == TxtElabResult.success)
         {
            itr_out.Push(source);
            res = src_itr.Perform(tokenList, inData, ref itr_out);
         }

         return res;
      }
   }
}
