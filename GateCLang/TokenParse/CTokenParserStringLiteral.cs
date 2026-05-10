using Gate.CLanguage.Compiler;
using Gate.CLanguage.PrePx.LookForwards;
using Gate.CLanguage.Standards;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.TokenParse
{
   /// <summary>
   /// 
   /// </summary>
   public class CTokenParserStringLiteral : CTokenParserStep
   {
      /// <summary>
      /// 
      /// </summary>
      public CTokenParserStringLiteral() { }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="inMarker"></param>
      /// <param name="inData"></param>
      /// <param name="output"></param>
      /// <returns></returns>
      public override TxtElabResult Perform(TxtMarker inMarker, CCompilerInData inData, ref CTokenParserOutput output)
      {
         var c_enc = CharStandard.DetectEncodingFromDeclaration(inMarker, false);

         if (c_enc != CCharEncodingLabel.none)
         {
            var str_lok_fw = new LookFwStringToken();
            var oup = new TxtElabOutputList<TxtToken>();

            var str_idx = inMarker.CurrIdx;

            var res = str_lok_fw.Perform(inMarker, inData, ref oup);

            if (res == TxtElabResult.success)
            {
               var tok = oup?.ListProduct.LastOrDefault() ?? throw new Crash();
               var prf = c_enc.GetAttribute<CCharEncondingAttribute>()?.Prefix ?? "";
               var nud_tok = new TxtTokenConst(inMarker.Store, (str_idx + prf.Length + 1, (tok.To ?? throw new Crash()).StoreIdx - 1));
               var gen_str = CharStandard.GetGeneralizedString(
                  nud_tok, c_enc, 
                  inData.Messages, 
                  inData.Settings.CharEncodingEnv, 
                  inData.Settings.NarrowCharEncoding, 
                  inData.Settings.WideCharEncoding);

               if (gen_str == null) { return TxtElabResult.failure; }
               else
               {
                  var c_str_obj = inData.RtmStrategy.MakeString(gen_str, inData.Settings.BuiltInSet ?? throw new Crash(), c_enc);                  
                  var tok_str = new CTokenString(inMarker.Store, str_idx, tok.Interval.To, c_enc, c_str_obj);

                  output.ListProduct.Add(tok_str);

                  return TxtElabResult.success;
               }
            }

            return res;
         }

         return TxtElabResult.continue_searching;
      }
   }
}
