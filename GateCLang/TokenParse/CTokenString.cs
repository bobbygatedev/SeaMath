using Gate.CLanguage.Compiler;
using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Standards;
using Gate.LangBase;
using Gate.Tools;
using Gate.Tools.Text;
using System.Text;

namespace Gate.CLanguage.TokenParse
{
   /// <summary>
   /// 
   /// </summary>
   public class CTokenString : CToken
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="store"></param>
      /// <param name="fromStringIdx"></param>
      /// <param name="toStringIdx"></param>
      /// <param name="cEncoding"></param>
      /// <param name="stringConstant"></param>
      public CTokenString(TxtStore store, int fromStringIdx, int toStringIdx, CCharEncodingLabel cEncoding, CRtmObjLiteralString stringConstant)
         : base(CTokenType.string_literal, store, fromStringIdx, toStringIdx, stringConstant)
      {
         CEncoding = cEncoding;
         RtmObjStringLiteral = stringConstant;
      }

      public static CTokenString? Concatenate(CTokenString[] tokenStrings, CCompilerInData inData)
      {
         var tks = tokenStrings ?? [];

         if (tks.Length == 0) { throw new Crash(); }
         else if (tks.Length == 1) { return tks.FirstOrDefault() ?? throw new Crash(); }
         else
         {
            var is_mix_enc = (inData.Settings.StringFlags & CharStandardStringFlags.mix_encoding) != 0;
            var enc_arr = tks.Select(t => t.CEncoding).Distinct().ToArray();
            var enc_no_mby = enc_arr.Where(e => e != CCharEncodingLabel.narrowchar).ToArray();

            var is_err = !is_mix_enc && enc_arr.Length > 1 || enc_no_mby.Length > 1;

            if (is_err)
            {
               inData.Messages.Add(CCompilerMsgId.cant_concatenate_strings.GetError(tks.FirstOrDefault()));

               return null;
            }

            var is_mby = enc_arr.Length == 1 && enc_arr[0] == CCharEncodingLabel.narrowchar;
            var no_mbs = (is_mby ? 
               tks.FirstOrDefault() : 
               tks.FirstOrDefault(t => t.CEncoding != CCharEncodingLabel.narrowchar)) ?? throw new Crash();

            var gen_sts = tks.Select(t => t.GeneralizedString).ToArray();

            foreach (var str in gen_sts) { str.Encoding = no_mbs?.Encoding; }

            var gen_str = tks.Select(t => t.GeneralizedString).Aggregate((s1, s2) => s1 + s2);

            gen_str.Encoding = no_mbs?.Encoding;

            var ls = new CRtmObjLiteralString(
               inData.RtmStrategy, 
               no_mbs?.RtmObjStringLiteral.DeclType ?? throw new Crash(), 
               gen_str);

            return new CTokenString(
               tks.FirstOrDefault()?.Store ?? throw new Crash(), 
               tks.FirstOrDefault()?.From?.StoreIdx ?? throw new Crash(), 
               tks.LastOrDefault()?.To?.StoreIdx ?? throw new Crash(), 
               no_mbs.CEncoding, 
               ls);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public CCharEncodingLabel CEncoding { get; }

      /// <summary>
      /// 
      /// </summary>
      public int NumBits => CharStandard.GetAttr(CEncoding)?.NumBits ?? -1;

      /// <summary>
      /// 
      /// </summary>
      public CRtmObjLiteralString RtmObjStringLiteral { get; }

      /// <summary>
      /// 
      /// </summary>
      public Encoding? Encoding => RtmObjStringLiteral.StringEncoding;

      /// <summary>
      /// 
      /// </summary>
      public GeneralizedString GeneralizedString => RtmObjStringLiteral.GeneralizedString;

      public override string ToString() => $"{base.ToString()}({RtmObjStringLiteral})";
   }
}
