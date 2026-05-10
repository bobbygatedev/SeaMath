using Gate.Tools;
using Gate.Tools.Message;
using Gate.Tools.Text;
using System.Text;
using System.Text.RegularExpressions;

namespace Gate.CLanguage.Linker
{
   public static class CLinkerMessages
   {
      private static readonly Regex my_Regex = new Regex(@"cl(?<num>\d+)_", RegexOptions.IgnoreCase | RegexOptions.Compiled);

      public enum Id
      {
         /// <summary>
         /// 
         /// </summary>
         cl001_Identifier__not_found = 0,

         /// <summary>
         /// 
         /// </summary>
         cl002_More_than_one_occurence_of__ = 1,
      }

      public static Msg M001_IdentifierNotFound(TxtToken? token, string id ,MsgType msgType) => 
         myFromId(Id.cl001_Identifier__not_found, msgType, token?.From, $"'{id}'");

      public static Msg M002_MoreThanOneIdentifier(TxtToken? token, string id) => myFromId(Id.cl002_More_than_one_occurence_of__, MsgType.fail, token?.From, $"'{id}'");

      private static Msg myFromId(Id id, MsgType msgType, TxtPos? txtPos, params object[] pars)
      {
         var mat = my_Regex.Match(id.ToString());

         if (mat.Success && mat.Index == 0)
         {
            var num = int.Parse(mat.Groups["num"].Value);
           
            return new Msg(msgType, my_GetFormatExpression(num, id.ToString().Substring(mat.Length), pars));
         }
         else { throw new Crash(); }
      }
      
      private static string my_GetFormatExpression(int msgId, string formattedMsg, object[] pars)
      {
         var idx_of = 0;
         var tmp = formattedMsg;
         var lst_idx = new List<int>();
         var blk = "__";

         while (true)
         {
            idx_of = tmp.IndexOf(blk, idx_of);

            if (idx_of == -1) { break; }
            else
            {
               lst_idx.Add(idx_of);
               idx_of += blk.Length;
            }
         }

         if (lst_idx.Count != pars.Length) { throw new Crash(); }
         else
         {
            var sb = new StringBuilder();
            var in_idx = 0;

            for (int i = 0; i < lst_idx.Count; i++)
            {
               var idx = lst_idx[i];

               sb.Append(tmp.Substring(in_idx, idx - in_idx));
               sb.Append($" {pars[i]} ");
               in_idx = idx + blk.Length;
            }

            sb.Append(tmp.Substring(in_idx, tmp.Length - in_idx));

            return sb.ToString();
         }
      }
   }
}
