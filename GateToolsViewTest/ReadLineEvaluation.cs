namespace Gate.ToolsViewTest
{
   internal class ReadLineEvaluation
   {
      class AutoCompletionHandler : IAutoCompleteHandler
      {
         // characters to start completion from
         public char[] Separators { get; set; } = [' ', '.', '/'];

         // text - The current text entered in the console
         // index - The index of the terminal cursor within {text}
         public string[] GetSuggestions(string text, int index) => 
            text.StartsWith("git ") ? ["init", "clone", "pull", "push"] : [];
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
