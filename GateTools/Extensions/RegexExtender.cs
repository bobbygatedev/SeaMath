using System.Text.RegularExpressions;

namespace Gate.Tools.Extensions
{
   public static class RegexExtender
   {
      public static bool IsFullMatch(this Regex regex, string @string) => regex.IsFullMatch(@string, out _);

      public static bool IsFullMatch(this Regex regex, string @string, out Match mat)
      {
         mat = regex.Match(@string);

         return mat.Success && mat.Length == @string.Length;
      }
   }
}
