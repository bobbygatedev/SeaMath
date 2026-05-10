using Gate.Tools.Text;

namespace Gate.ToolsView.TextSearch
{
   /// <summary>
   /// 
   /// </summary>
   public interface ITextSearchCtrlInteraction
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="docTextCtrl"></param>
      /// <param name="textToken"></param>
      void SetMultilineSelectionToken(Control docTextCtrl, TxtToken? textToken);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="docTextCtrl"></param>
      /// <returns></returns>
      TxtToken? GetMultilineSelectionToken(Control docTextCtrl);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="docTextCtrl"></param>
      /// <returns></returns>
      string? GetOpenFile(Control docTextCtrl);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="docTextCtrl"></param>
      /// <returns></returns>
      TxtPos? GetTextPos(Control docTextCtrl);
      
      /// <summary>
      /// 
      /// </summary>
      /// <param name="findToken"></param>
      /// <param name="docTextCtrl"></param>
      void SelectToken(TxtToken findToken, Control docTextCtrl);
      
      /// <summary>
      /// 
      /// </summary>
      /// <param name="docTextCtrl"></param>
      /// <returns></returns>
      string? GetFileContent(Control docTextCtrl);
      
      /// <summary>
      /// 
      /// </summary>
      /// <param name="docTextCtrl"></param>
      /// <returns></returns>
      TxtToken? GetSelection(Control docTextCtrl);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="replaceText"></param>
      /// <param name="docTextCtrl"></param>
      void ReplaceSelected(string replaceText, Control docTextCtrl);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="docTextCtrl"></param>
      /// <param name="content"></param>
      void ResetText(Control docTextCtrl, string content);
   }
}