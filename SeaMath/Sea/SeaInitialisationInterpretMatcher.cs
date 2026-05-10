using Gate.CLanguage.Compiler;
using Gate.CLanguage.Initialisation;
using Gate.CLanguage.Types;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaInitialisationInterpretMatcher : CInitialisationInterpretMatcher
   {
      /// <summary>
      /// 
      /// </summary>
      public SeaInitialisationInterpretMatcher() { }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="varTypeAlias"></param>
      /// <param name="scalarInit"></param>
      /// <param name="inData"></param>
      /// <returns></returns>
      protected override bool myCheckType(
         CTypeAlias varTypeAlias, CInitialisationScalar scalarInit, CCompilerInData inData)
      {
         if (varTypeAlias.IsSeaType())
         {
            inData.Messages.Add(SeaMathMessages.M003_NoSeaInitialisation(scalarInit.ScalarExpression?.TxtToken));

            return false;
         }
         else
         {
            return base.myCheckType(varTypeAlias, scalarInit, inData);
         }
      }
   }
}
