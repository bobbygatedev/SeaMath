using Gate.Tools.Text;

namespace Gate.ToolsView.TextSearch
{
   /// <summary>
   /// Item of text search window/widget
   /// </summary>
   public class TextSearchToken
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="findFile"></param>
      /// <param name="token"></param>
      public TextSearchToken(TextSearchFile findFile, TxtToken token)
      {
         FindFile = findFile;

         //creates an copy sector Whether token is result of a replacement and therefore has not null dest store.
         TxtToken = token;
      }

      /// <summary>
      /// 
      /// </summary>
      public TextSearchFile FindFile { get; }
      
      /// <summary>
      /// Effectively in widget shown token (it differs from input Whether token is sector  TxtToken != null ie a replace was performed.
      /// </summary>
      public TxtToken TxtToken { get; }
   }
}
