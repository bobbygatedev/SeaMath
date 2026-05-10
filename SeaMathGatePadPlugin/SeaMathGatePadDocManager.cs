using Gate.Dock.DockApp;
using Gate.Dock.DockDocu;
using Gate.DockRuntimePlugin;
using Gate.LangBase.Runtime.DbgEng;
using Gate.SeaMath;
using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.SeaMathGatePadPlugin
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaMathGatePadDocManager : SeaMathDocManager
   {
      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="seaMathGatePadSession"></param>
      public SeaMathGatePadDocManager()
      {
      }

      protected override void myActionOnParentSet(HierarchicalItem parentItem)
      {
         base.myActionOnParentSet(parentItem);

         if (parentItem is SeaMathSession ses)
         {
            var app = App ?? throw new Crash();

            app.MainForm.OnTabPageCurrentChanged += MainForm_OnTabPageCurrentChanged;
            myUpdateStartPath(App.MainForm.PpTabPageCurrent);
            Breakpoints = myConvertBreakpoints(App.MarkerHandler?.Breakpoints ?? []);
            app.MarkerHandler.OnBreakpointsChanged += MarkerHandler_OnBreakpointsChanged;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public GateDockRuntimePlugin? RuntimePlugin => App?.PlugInManager?.DetectedPlugInClasses?.OfType<GateDockRuntimePlugin>().FirstOrDefault();

      /// <summary>
      /// 
      /// </summary>
      public GateDockApp? App => SeaMathGatePadSession?.App;

      /// <summary>
      /// 
      /// </summary>
      public override FileInfo[] AllOpenFiles =>
         (App?.MainForm?.PpTabPagesAll ?? []).OfType<IGateDockDocu>().
         Select(d=>d.PpDocuPath).
         Nn().
         Where(d => !d.IsBlank()).
         Select(d => new FileInfo(d)).
         Where(p => p.Extension.IsEqualNoContent(".c")).ToArray();

      public SeaMathGatePadSession? SeaMathGatePadSession => ParentItem as SeaMathGatePadSession;

      public override void ToggleBreakpoint()
      {
         if (App?.MainForm.PpTabCurrent is IGateDockDocuText txt_doc) { txt_doc.MthToggleBreakpoint(); }
      }

      private void myUpdateStartPath(Control? newTopLevelPage) =>
         StartInfo = newTopLevelPage is IGateDockDocuText ctr_txt ?
            (Path.GetExtension(ctr_txt.PpDocuPath).IsEqualNoContent(".c") ? new FileInfo(ctr_txt.PpDocuPath.Nn()) : null) :
            null;

      private RtmDbgEngBreakpoint[] myConvertBreakpoints(GateDockDocuMarkerBreakpoint[] breakpoints) =>
         breakpoints.Select(b => new RtmDbgEngBreakpoint(b.BreakpointPath.ExtTrim(), b.Line)).ToArray();

      private void MarkerHandler_OnBreakpointsChanged(object? sender, GateDockDocuMarkerBreakpoint[]? breakpoints) =>
         Breakpoints = myConvertBreakpoints(breakpoints ?? []);

      private void MainForm_OnTabPageCurrentChanged(object? sender, Control? newTopLevelPage) => myUpdateStartPath(newTopLevelPage);
   }
}
