using Gate.CLanguage.Compiler;
using Gate.Tools;
using Gate.Tools.Text;

namespace Gate.CLanguage.TokenParse
{
   /// <summary>
   ///
   /// </summary>
   public static class CTokenParserHelper
   {
      public static (string? body, bool isImaginary, bool isLong, bool isSingle) ExaminateFloat(
         TxtToken regexMatchToken, CCompilerInData inData)
      {
         var mat = regexMatchToken.Content.ToLower();
         var occ_i = Enumerable.Range(0, mat.Length).Where(i => mat[i] == 'i').ToArray();
         var occ_f = Enumerable.Range(0, mat.Length).Where(i => mat[i] == 'f').ToArray();
         var occ_l = Enumerable.Range(0, mat.Length).Where(i => mat[i] == 'l').ToArray();

         var min = occ_i.Concat(occ_l).Concat(occ_f).DefaultIfEmpty(mat.Length).Min();

         if (occ_i.Length > 1 || occ_l.Length > 1 || occ_f.Length > 1)
         {
            //suffix token
            var suf_tok = TxtTokenConst.FromToken(
               regexMatchToken,
               new Interval(
                  (regexMatchToken.From ?? throw new Crash()).StoreIdx + min,
                  (regexMatchToken.To ?? throw new Crash()).StoreIdx));

            inData.Messages.Add(CCompilerMsgs.InvalidSuffix(suf_tok));

            return (null, false, false, false);
         }
         else
         {
            return (mat.Substring(0, min), occ_i.Length == 1, occ_l.Length == 1, occ_f.Length == 1);
         }
      }

      public static (string? body, bool isImaginary, bool isLong, bool isLongLong, bool isUnsigned) ExaminateInt(
         TxtToken regexMatchToken, CCompilerInData inData)
      {
         var mat = regexMatchToken.Content.ToLower();

         var occ_i = Enumerable.Range(0, mat.Length).Where(i => mat[i] == 'i').ToArray();
         var occ_l = Enumerable.Range(0, mat.Length).Where(i => mat[i] == 'l').ToArray();
         var occ_u = Enumerable.Range(0, mat.Length).Where(i => mat[i] == 'u').ToArray();

         var min = occ_i.Concat(occ_u).Concat(occ_l).DefaultIfEmpty(mat.Length).Min();

         if (occ_i.Length > 1 || occ_u.Length > 1 || !myCheckLong(occ_l, out bool is_lng, out bool is_lng_lng))
         {
            //suffix token
            var suf_tok = TxtTokenConst.FromToken(
               regexMatchToken,
               new Interval(
                  (regexMatchToken.From ?? throw new Crash()).StoreIdx + min,
                  (regexMatchToken.To ?? throw new Crash()).StoreIdx));


            inData.Messages.Add(CCompilerMsgs.InvalidSuffix(suf_tok));

            return (null, false, false, false, false);
         }
         else
         {
            return (mat.Substring(0, min), occ_i.Length == 1, is_lng, is_lng_lng, occ_u.Length == 1);
         }
      }

      private static bool myCheckLong(int[] lOccurences, out bool isLong, out bool isLongLong)
      {
         isLong = isLongLong = false;

         switch (lOccurences.Length)
         {
            case 0: return true;

            case 1:
               isLong = true;
               return true;

            case 2:
               if (lOccurences[0] + 1 == lOccurences[1])
               {
                  isLongLong = true;

                  return true;
               }
               else { return false; }

            default: return false;
         }
      }
   }
}
