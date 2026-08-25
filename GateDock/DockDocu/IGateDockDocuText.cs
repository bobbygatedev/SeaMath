using Gate.Tools.Text;

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
      event OnBreakpointsChangedHandler OnBreakpointsChanged;
      
      /// <summary>
      /// 
      /// </summary>
      event OnBoomarksChangedHandler OnBoomarksChanged;

      /// <summary>
      /// 
      /// </summary>
      event OnSaveHandler? OnSave;

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
      GateDockDocuMarkerBreakpoint[]? PpBreakpoints { get; set; }

      /// <summary>
      /// 
      /// </summary>
      GateDockDocuMarkerBookmark[]? PpBookmarks { get; set; }

      /// <summary>
      /// Tokenb on execution in case a debug session is actives.
      /// </summary>
      TxtToken? PpDbgPointCurrent { get; set; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="token"></param>
      void MthSelectTokenFromOtherControl(TxtToken token);

      /// <summary>
      /// 
      /// </summary>
      void MthToggleBookmark();

      /// <summary>
      /// 
      /// </summary>
      void MthToggleBreakpoint();

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