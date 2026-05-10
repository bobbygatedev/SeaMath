using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Text;

namespace Gate.CLanguage.TokenParse
{
   /// <summary>
   /// 
   /// </summary>
   public class CToken : TxtTokenConst
   {
      public CToken(CTokenType tokenType, TxtStore txtStore, int fromStringIdx, int toStringIdx, RtmObj? runtimeObj) :
         base(txtStore, (fromStringIdx, toStringIdx))
      {
         TokenType = tokenType;
         Obj = runtimeObj;
      }

      public static CToken FromOther(CTokenType tokenType, TxtToken tokenOther, RtmObj? runtimeObj = null) =>
         new CToken(tokenType, tokenOther.Store ?? throw new Crash(), tokenOther.Interval.From, tokenOther.Interval.To, runtimeObj);

      public static CToken FromFromLength(CTokenType tokenType, TxtStore txtStore, TxtPos from, int tokenLen)
      {
         var idx_str_frm = txtStore.GetIdx(from);
         var idx_str_to = idx_str_frm + tokenLen - 1;

         return new CToken(tokenType, txtStore, idx_str_frm, idx_str_to, null);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="tokenOther"></param>
      /// <param name="runtimeObj"></param>
      /// <returns></returns>
      public static CToken MakeConstant(TxtToken tokenOther, RtmObj runtimeObj) => FromOther(CTokenType.constant, tokenOther, runtimeObj);

      /// <summary>
      /// 
      /// </summary>
      public CTokenType TokenType { get; }

      /// <summary>
      /// 
      /// </summary>
      public RtmObj? Obj { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override string ToString() => Content;
   }
}
