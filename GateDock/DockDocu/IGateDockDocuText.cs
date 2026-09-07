using Gate.Tools.Text;
using Gate.ToolsView.TextCtrl;

namespace Gate.Dock.DockDocu
{
   /// <summary>
   /// 
   /// </summary>
   public interface IGateDockDocuText : IGateDockDocu
   {
      /// <summary>
      /// 
      /// </summary>
      event OnBreakpointsChangedHandler? OnBreakpointsChanged;
      
      /// <summary>
      /// 
      /// </summary>
      event OnBoomarksChangedHandler? OnBookmarksChanged;

      /// <summary>
      /// 
      /// </summary>
      event OnSaveHandler? OnSave;

      /// <summary>
      /// 
      /// </summary>
      event EventHandler? TextChanged;

      /// <summary>
      /// 
      /// </summary>
      event OnDocumentInsertHandler? OnDocumentInsert;

      /// <summary>
      /// 
      /// </summary>
      event OnDocumentDeleteHandler? OnDocumentDelete;

      /// <summary>
      /// 
      /// </summary>
      int PpCurrLine { get; set; }

      /// <summary>
      /// 
      /// </summary>
      int PpCurrCol { get; set; }

      /// <summary>
      /// 
      /// </summary>
      string PpContentText { get; set; }

      /// <summary>
      /// 
      /// </summary>
      GateDockDocuMarkerBreakpoint[]? PpBreakpoints { get; set; }

      /// <summary>
      /// 
      /// </summary>
      GateDockDocuMarkerBookmark[]? PpBookmarks { get; set; }

      /// <summary>
      /// Token on execution in case a debug session is actives.
      /// </summary>
      TxtToken? PpDbgPointCurrent { get; set; }

      /// <summary>
      /// True when content has saved and has not been modified.
      /// </summary>
      bool IsSaved { get; }

      /// <summary>
      /// Selection token or null when no selection.
      /// </summary>
      TxtToken? PpSelection { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="token"></param>
      void MthSelectTokenFromOtherControl(TxtToken token);

      /// <summary>
      ///  
      /// </summary>
      GateDockDocuMarkerBookmark? MthToggleBookmark();

      /// <summary>
      /// 
      /// </summary>
      GateDockDocuMarkerBreakpoint? MthToggleBreakpoint();

      /// <summary>
      /// 
      /// </summary>
      void MthGoToWindow();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="line"></param>
      void MthOutlineToggle(int line = -1);

      /// <summary>
      /// 
      /// </summary>
      void MthOutlineToggleAll();

      /// <summary>
      /// 
      /// </summary>
      void MthOutlineCollapseToFunction();
   }
}