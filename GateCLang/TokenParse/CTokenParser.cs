using Gate.CLanguage.Compiler;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.TokenParse
{
   /// <summary>
   /// Parses C preprocessed source code to token list.
   /// </summary>
   public class CTokenParser : CTokenParserStep
   {  
      private Lazy<IterateWhileSuccess> myLazyCompound;

      public CTokenParser() => myLazyCompound = new Lazy<IterateWhileSuccess>(() => new IterateWhileSuccess(new Or(myMakeSteps())));

      private class InnerError : CTokenParserStep
      {
         public InnerError(CTokenParserStep tokenParserStep) => TokenParserStep = tokenParserStep;

         public CTokenParserStep TokenParserStep { get; }

         public override TxtElabResult Perform(TxtMarker input, CCompilerInData inData, ref CTokenParserOutput output)
         {
            if (input.MoveToNextNoSpace())
            {
               inData.Messages.Add(CCompilerMsgs.UnexpectedCharDuringTokenise(input.GetMarkingToken(1)));

               return TxtElabResult.failure;
            }
            else
            {
               return TxtElabResult.continue_searching;//end of text reached
            }
         }
      }

      protected virtual CTokenParserStep[] myMakeSteps() => new CTokenParserStep[]{
               myGetTokenParserPunctuator(),
               myGetTokenParserKeyword(),
               myGetTokenParserStringLiteral(),
               myGetTokenParserConstant(),
               myGetTokenParserIdentifier(),
               new InnerError(this)};

      protected virtual CTokenParserStep myGetTokenParserPunctuator() => new CTokenParserStepPunctuator();

      protected virtual CTokenParserStep myGetTokenParserStringLiteral() => new CTokenParserStringLiteral();

      protected virtual CTokenParserStep myGetTokenParserConstant() => new CTokenParserConstant();

      protected virtual CTokenParserStep myGetTokenParserIdentifier() => new CTokenParserStepIdentifier();

      protected virtual CTokenParserStep myGetTokenParserKeyword() => new CTokenParserStepKeyword();

      public override TxtElabResult Perform(TxtMarker inputMarker, CCompilerInData inData, ref CTokenParserOutput output) => myLazyCompound.Value.Perform(inputMarker, inData, ref output);
   }
}
