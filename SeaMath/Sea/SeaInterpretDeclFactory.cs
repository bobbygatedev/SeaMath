using Gate.CLanguage;
using Gate.CLanguage.DeclInterpreter;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Initialisation;
using Gate.CLanguage.TypeSpecifierInterpret;
using static Gate.CLanguage.TypeSpecifierInterpret.CTypeSpecifierInterpret;

namespace Gate.SeaMath.Sea
{

   /// <summary>
   /// 
   /// </summary>
   public class SeaInterpretDeclFactory : CDeclInterpretFactory
   {
      public SeaInterpretDeclFactory() { }

      public override CTypeSpecifierInterpret MakeTypeSpecifierInterprer(CExprStatementInterpreter exprInterpret, CAttributesInterpret attributesInterpret, UsageId usage) =>
         new SeaSpecifierInterpret(usage, this, exprInterpret, attributesInterpret);

      public override CInitialisationInterpret MakeInitializerInterpret(CExprStatementInterpreter exprInterpret) =>
         new SeaInitialisationInterpret(exprInterpret);

      public override CDeclInterpretArraySubscriptAndVarName MakeArraySubscriptAndVarNameInterpret(CDeclInterpretContext context, CExprStatementInterpreter exprInterpret, CAttributesInterpret attributesInterpret) =>
         new SeaDeclInterpretArraySubscriptAndVarName(context, attributesInterpret, exprInterpret);
   }
}
