using Gate.CLanguage.DeclInterpreter;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Expressions.COperators;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Source;
using Gate.Tools;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaInterpret : CInterpreter
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="langFlags"></param>
      /// <param name="parent"></param>
      public SeaInterpret(CLangFlags langFlags, SeaCCompiler parent) : base(langFlags) => Parent = parent;

      /// <summary>
      /// 
      /// </summary>
      public SeaCCompiler Parent { get; }

      public new SeaInterpretDeclFactory DeclInterpretFactory => (SeaInterpretDeclFactory)base.DeclInterpretFactory;

      protected override CDeclInterpretFactory myMakeCDeclInterpretFactory() => new SeaInterpretDeclFactory();

      protected override CSourceInterpreter myMakeSourceInterpreter() => new SeaSourceInterpreter(myMakeRootSubInterpreters());

      protected override CExprStatementInterpreter myMakeExprStatementInterpreter() =>
         new SeaExprStatementInterpreter(DeclInterpretFactory ?? throw new Crash(), AttributesInterpret, LangFlags);

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected override CTokenInterpreter[] myMakeRootSubInterpreters() => [
         new CDeclInterpret(
            CDeclInterpretContext.global_var, DeclInterpretFactory ?? throw new Crash(), ExprInterpret, AttributesInterpret) ,
               new CExprStatementInterpreter.WrapCondition (ExprInterpret,";"),
      ];
   }
}
