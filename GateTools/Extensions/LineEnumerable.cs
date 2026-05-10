using System.Collections;
using System.Text.RegularExpressions;

namespace Gate.Tools
{
   /// <summary>
   /// 
   /// </summary>
   public class LineEnumerable : IEnumerable<string>
   {
      private static Regex myRegexNewLine = new Regex(@"\r\n|\n", RegexOptions.Compiled);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="string"></param>
      public LineEnumerable(string @string) => String = @string;

      private class InnerEnumerator : IEnumerator<string>
      {
         public InnerEnumerator(string @string) => String = @string;

         public string String { get; }

         public int LineIndex { get; private set; } = -1;

         public int StringOffset { get; private set; } = 0;

         public string Current { get; private set; } = "";

         object IEnumerator.Current => Current;

         public void Dispose()
         {
         }

         public bool MoveNext()
         {
            var mat = myRegexNewLine.Match(String, StringOffset);

            if (mat.Success)
            {
               LineIndex++;
               Current = String.Substring(StringOffset, mat.Index - StringOffset);
               StringOffset = mat.Index + mat.Length;

               return true;
            }
            else if (StringOffset < String.Length)
            {
               LineIndex++;
               Current = String.Substring(StringOffset);
               StringOffset = String.Length;

               return true;
            }
            else
            {
               return false;
            }
         }

         public void Reset()
         {
            LineIndex = -1;
            StringOffset = 0;
         }
      }

      public string String { get; }

      public IEnumerator<string> GetEnumerator() => new InnerEnumerator(String);

      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
   }
}
