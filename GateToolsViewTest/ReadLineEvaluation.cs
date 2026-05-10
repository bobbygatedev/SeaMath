using System;

namespace Gate.ToolsViewTest
{
   internal class ReadLineEvaluation
   {
      class AutoCompletionHandler : IAutoCompleteHandler
      {
         // characters to start completion from
         public char[] Separators { get; set; } = new char[] { ' ', '.', '/' };

         // text - The current text entered in the console
         // index - The index of the terminal cursor within {text}
         public string[] GetSuggestions(string text, int index)
         {
            if (text.StartsWith("git "))
               return new string[] { "init", "clone", "pull", "push" };
            else
               return null;
         }
      }

      static void Main()
      {
         ReadLine.HistoryEnabled = true;
         ReadLine.AutoCompletionHandler = new AutoCompletionHandler();

         while (true)
         {
            var cmd = ReadLine.Read("> ");
            // Elabora il comando...
         }
      }
   }
}
