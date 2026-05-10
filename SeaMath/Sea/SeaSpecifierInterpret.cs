using Gate.CLanguage;
using Gate.CLanguage.DeclInterpreter;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.TypeSpecifierInterpret;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaSpecifierInterpret : CTypeSpecifierInterpret
   {
      public SeaSpecifierInterpret(
         UsageId usage,
         CDeclInterpretFactory declInterpretFactory,
         CExprStatementInterpreter exprInterpret,
         CAttributesInterpret attributesInterpret) :
         base(usage, declInterpretFactory, exprInterpret, attributesInterpret)
      { }

      public override CTokenInterpreter[] MakeInterpreters() =>
         new[] { new SeaTypeSpecifierInterpret(Usage) }.Concat(base.MakeInterpreters()).ToArray();
   }
}
