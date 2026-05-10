using Gate.Tools.Text;
using Gate.ToolsView.MenuCommand;
using System.Drawing;
using System.Windows.Forms;

namespace Gate.ToolsView.ConIO
{
   /// <summary>
   /// 
   /// </summary>
   /// <param name="char"></param>
   public delegate void OnCharHandler(char @char);

   /// <summary>
   /// 
   /// </summary>
   /// <param name="keys"></param>
   public delegate void OnKeyDownHandler(Keys keys);

   /// <summary>
   /// Interface a control shall implement in order to interact with ConsoleController.
   /// </summary>
   public interface IConsoleControl : IControlWithManagedCmds
   {
      /// <summary>
      /// 
      /// </summary>
      ConsoleController? ConsoleController { get; set; }

      /// <summary>
      /// 
      /// </summary>
      ConsoleInputKeyEventStroke ConsoleInputKeyEventStroke { get; }

      /// <summary>
      /// 
      /// </summary>
      TxtPos CurrentPos { get; set; }

      /// <summary>
      /// 
      /// </summary>
      Point CurrentScreenPos { get; }

      /// <summary>
      /// 
      /// </summary>
      string[]? ScreenBuffer { get; } 

      /// <summary>
      /// 
      /// </summary>
      /// <param name="string"></param>
      void Insert2CurrentPos(string @string);
      
      /// <summary>
      /// 
      /// </summary>
      /// <param name="isBackward"></param>
      void CancelChar(bool isBackward);

      /// <summary>
      /// 
      /// </summary>
      void ClearScreen();

      /// <summary>
      /// Returns a line by index.
      /// </summary>
      /// <param name="line1">Line index [1-] if 0 return current line</param>
      /// <returns></returns>
      string GetLine(int line1 = 0);

      /// <summary>
      /// Sets a line by index.
      /// </summary>
      /// <param name="lineText">New line text.</param>
      /// <param name="line1">Line index [1-] if 0 return current line</param>
      /// <returns></returns>
      string SetLine(string lineText , int line1 = 0);
      
      /// <summary>
      /// Replaces the current selection in the text editor with the specified text.
      /// </summary>
      /// <remarks>If there is no active selection, the specified text will be inserted at the current
      /// cursor position. This method does not preserve the original selection after replacement.</remarks>
      /// <param name="newText">The text to replace the current selection with. If the selection is empty, the text will be inserted at the
      /// cursor position.</param>
      void ReplaceSelection(string newText);

      /// <summary>
      /// Gets or sets the current text selection range.
      /// </summary>
      /// <remarks>The selection range is inclusive of the start position and exclusive of the end position.
      /// If the start and end positions are the same, the selection is considered empty (a caret position).</remarks>
      (TxtPos? start, TxtPos? end) Selection { get; set; }
   }
}
