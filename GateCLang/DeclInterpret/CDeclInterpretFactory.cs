using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Initialisation;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.TypeSpecifierInterpret;
using static Gate.CLanguage.TypeSpecifierInterpret.CTypeSpecifierInterpret;

namespace Gate.CLanguage.DeclInterpreter
{
   /// <summary>
   /// 
   /// </summary>
   public class CDeclInterpretFactory
   {
      /// <summary>
      /// 
      /// </summary>
      public CDeclInterpretFactory() { }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="exprInterpret"></param>
      /// <param name="attributesInterpret"></param>
      /// <param name="usage"></param>
      /// <returns></returns>
      public virtual CTypeSpecifierInterpret MakeTypeSpecifierInterprer(
         CExprStatementInterpreter exprInterpret, CAttributesInterpret attributesInterpret, UsageId usage) =>
         new CTypeSpecifierInterpret(usage, this, exprInterpret, attributesInterpret);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="exprInterpret"></param>
      /// <returns></returns>
      public virtual CTokenInterpreter[] MakeDeclSpecifierInterprers(
         CExprStatementInterpreter exprInterpret, CAttributesInterpret attributesInterpret, UsageId usage) => new CTokenInterpreter[] {
            MakeTypeSpecifierInterprer(exprInterpret,attributesInterpret,usage),
            attributesInterpret,
            new CDeclSpecifiersInterpretStorageCls(),
            new CDeclSpecifiersInterpretTypeQualifiers(),
            new CDeclSpecifiersInterpretFunction()
         }.Where(i => i != null).ToArray();

      public virtual CDeclInterpretArraySubscriptAndVarName MakeArraySubscriptAndVarNameInterpret(
         CDeclInterpretContext context, CExprStatementInterpreter exprInterpret, CAttributesInterpret attributesInterpret) =>
         new CDeclInterpretArraySubscriptAndVarName(context, attributesInterpret, exprInterpret);

      public virtual CDeclInterpretPartFunction MakeDeclInterpretFunctionPart(
         bool isForDefinition, CExprStatementInterpreter exprInterpret, CAttributesInterpret attributesInterpret) =>
         new CDeclInterpretPartFunction(
            isForDefinition ,
            MakeArraySubscriptAndVarNameInterpret(CDeclInterpretContext.function_id, exprInterpret, attributesInterpret),
            exprInterpret,
            attributesInterpret,
            this);

      public virtual CInitialisationInterpret MakeInitializerInterpret(CExprStatementInterpreter exprInterpret) => 
         new CInitialisationInterpret(exprInterpret);
   }
}
