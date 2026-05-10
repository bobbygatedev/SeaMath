using Gate.CLanguage.Compiler;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System;

namespace Gate.CLanguage.TokenParse
{
   /// <summary>
   ///
   /// </summary>
   public class CTokenParserConstant : CTokenParserStep
   {
      private Lazy<Or> myLazyParserOr;

      public CTokenParserConstant() => myLazyParserOr = new Lazy<Or>(() => new Or(myMakeSteps()));
  
      protected virtual CTokenParserStep[] myMakeSteps() => new CTokenParserStep[]{
         myMakeNumericConstantParserStep(),
         myMakeCharConstantParserStep()};

      public virtual CTokenParserStep myMakeNumericConstantParserStep() => new CTokenParserNumConst();

      public virtual CTokenParserStep myMakeCharConstantParserStep() => new CTokenParserCharConstant();

      public override TxtElabResult Perform(TxtMarker inputMarker, CCompilerInData inData, ref CTokenParserOutput output) =>
         myLazyParserOr.Value.Perform(inputMarker, inData, ref output);
   }
}
