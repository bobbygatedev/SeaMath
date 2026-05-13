using Gate.CLanguage.Compiler;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.TokenParse
{
   /// <summary>
   ///
   /// </summary>
   public class CTokenParserNumConst : CTokenParserStep
   {
      private Or myComposed;

      public CTokenParserNumConst() => myComposed = new Or(ConstantParsers);

      public virtual CTokenParserStep[] ConstantParsers => [
         new CTokenParserNumConstFloatComplex(),
         new CTokenParserNumConstHex(),
         new CTokenParserNumConstBin(),
         new CTokenParserNumConstInt(),      
      ];

      public override TxtElabResult Perform(TxtMarker inputMarker, CCompilerInData inData, ref CTokenParserOutput output)
      {
         var res = myComposed.Perform(inputMarker, inData, ref output);

         if (res == TxtElabResult.success)
         {
            var ch = inputMarker.MarkedChar;

            //valid expression if at end or is folllowed by allowed punctuator(+,-,..) or space
            if (ch.HasValue && !char.IsWhiteSpace(ch.Value) && !inputMarker.IsMarkingAnySign(inData.Settings.Punctuators ?? []))
            {
               inData.Messages.Add(CCompilerMsgs.WrongCharAfterNumericConstant(TxtTokenConst.FromFromLen(inputMarker.Store, inputMarker.CurrIdx, 1)));

               return TxtElabResult.failure;
            }
         }

         return res;
      }
   }
}



