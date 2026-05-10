using Gate.CLanguage.Expressions;
using Gate.CLanguage.Initialisation;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaInitialisationInterpret : CInitialisationInterpret
   {
      public SeaInitialisationInterpret(CExprStatementInterpreter exprInterpret) : base(exprInterpret)
      {
      }

      protected override CInitialisationInterpretMatcher myMakeInitialisationInterpretMatcher() => 
         new SeaInitialisationInterpretMatcher();
   }
}
