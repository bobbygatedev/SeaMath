using Gate.Tools.Text;
using static Gate.Tools.Text.TxtStore;

namespace Gate.ToolsView.TextSearch
{
   /// <summary>
   /// Interface for text find/replace strategy (provides algorithm for text search)
   /// </summary>
   public interface ITextSearchFindStategy
   {
      /// <summary>
      /// Finds all text token inside a <paramref name="fileContent"/> text file.
      /// </summary>
      /// <param name="searchText">Text to search.</param>
      /// <param name="searchFlags">Search option flags.</param>
      /// <param name="fileContent">Content of file where search is made.</param>
      /// <param name="selection">Selection where search is made</param>
      /// <returns>Array of find token or empty array.</returns>
      TxtToken[] FindAll(string searchText, TextSearchFlags searchFlags, string fileContent, TxtToken? selection);

      /// <summary>
      /// Searches next occurence of <paramref name="searchText"/> starting from <paramref name="currentCursorPos"/>.
      /// </summary>
      /// <param name="searchText">Text to search.</param>
      /// <param name="searchFlags">Search option flags.</param>
      /// <param name="fileContent">Content of file where search is made.</param>
      /// <param name="currentCursorPos">Current doc cursor position (if null is set as beginning or end depending on <seealso cref="TextSearchFlags.Backward"/> ).</param>
      /// <param name="selection">Selection where search is made</param>
      /// <returns>Next find occurence token or null.</returns>
      TxtToken? FindNext(string searchText, TextSearchFlags searchFlags, string fileContent, TxtPos? currentCursorPos, TxtToken? selection);

      /// <summary>
      /// Returns user text reworked for replace unescaping extended chars (\n,\t,..) if <seealso cref="TextSearchFlags.UseExtendedChars"/> is used.
      /// </summary>
      /// <param name="userReplaceText">Replace text as input by user.</param>
      /// <param name="searchFlags">Search option flags.</param>
      /// <returns>Reworked replace text.</returns>
      string GetReplaceText(string userReplaceText, TextSearchFlags searchFlags);

      /// <summary>
      /// <br>Find all occurences of <paramref name="searchText"/></br>
      /// <br>see <seealso cref="FindAll(string, TextSearchFlags, string)"/></br>
      /// <br>then replace all token with <paramref name="replaceText"/></br>
      /// </summary>
      /// <param name="searchText">Text to search.</param>
      /// <param name="replaceText">Text to replace after call of <seealso cref="GetReplaceText(string, TextSearchFlags)"/>.</param>
      /// <param name="searchFlags">Search option flags.</param>
      /// <param name="fileContent">Content of file where search is made.</param>
      /// <returns>Array of token after replace is made.</returns>
      Sector[] ReplaceAll(string searchText, string replaceText, TextSearchFlags searchFlags, string fileContent);

      /// <summary>
      /// Performs replace inside current open document (takes into account current text position) 
      /// </summary>
      /// <param name="searchText">Text to search.</param>
      /// <param name="replaceText"></param>
      /// <param name="searchFlags">Search option flags.</param>
      /// <param name="fileContent">Content of file where search is made.</param>
      /// <param name="selection">Selection where search is made</param>
      /// /// <returns>Array of token after replace is made.</returns>
      Sector[] ReplaceCurrentDoc(string searchText, string replaceText, TextSearchFlags searchFlags, string fileContent, TxtToken? selection);


      /// <summary>
      /// Performs replace inside current open document (takes into account current text position) 
      /// </summary>
      /// <param name="searchText">Text to search.</param>
      /// <param name="replaceText"></param>
      /// <param name="searchFlags">Search option flags.</param>
      /// <param name="fileContent">Content of file where search is made.</param>
      /// <param name="currentCursorPos">Current doc cursor position.</param>
      /// <returns>Array of token after replace is made.</returns>
      Sector[] ReplaceCurrentDoc(string searchText, string replaceText, TextSearchFlags searchFlags, string fileContent, TxtPos? currentCursorPos);
   }
}