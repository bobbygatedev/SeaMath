using Gate.CLanguage.Compiler;
using Gate.CLanguage.PrePx.LookForwards;
using Gate.CLanguage.Standards;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.TokenParse
{
   /// <summary>
   /// 
   /// </summary>
   public class CTokenParserCharConstant : CTokenParserStep
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="inputMarker"></param>
      /// <param name="inData"></param>
      /// <param name="output"></param>
      /// <returns></returns>
      public override TxtElabResult Perform(TxtMarker inputMarker, CCompilerInData inData, ref CTokenParserOutput output)
      {
         inputMarker.MoveToNextNoSpace();

         var is_wid = (inData.Settings.StringFlags & CharStandardStringFlags.widechar_allowed) != 0;

         var c_enc = CharStandard.DetectEncodingFromDeclaration(inputMarker, true);

         if (is_wid && c_enc != CCharEncodingLabel.none || !is_wid && c_enc == CCharEncodingLabel.narrowchar)
         {
            var str_lok_fw = new LookFwCharToken();
            var oup = new TxtElabOutputList<TxtToken>();
            var sta_idx = inputMarker.CurrIdx;

            var res = str_lok_fw.Perform(inputMarker, inData, ref oup);

            if (res == TxtElabResult.success)
            {
               var ch_tok = new TxtTokenConst(
                  inputMarker.Store,
                  (sta_idx, (oup?.ListProduct.LastOrDefault()?.To ?? throw new Crash()).StoreIdx));
               var enc_ch = CharStandard.EncodeChar(
                  ch_tok,
                  inData.Messages,
                  c_enc,
                  inData.Settings.CharEncodingEnv,
                  inData.Settings.NarrowCharEncoding,
                  inData.Settings.WideCharEncoding);

               if (enc_ch == null) { return TxtElabResult.failure; }

               var chr_typ = CharStandard.GetConstantCharType(c_enc, inData.Settings.BuiltInSet ?? throw new Crash(), true) ?? throw new Crash();

               var rtm = inData.RtmStrategy.MakeConstant(enc_ch, chr_typ);
               var c_tok = CToken.MakeConstant(ch_tok, rtm);

               output.ListProduct.Add(c_tok);
            }

            return res;
         }
         else
         {
            return TxtElabResult.continue_searching;
         }
      }
   }
}
