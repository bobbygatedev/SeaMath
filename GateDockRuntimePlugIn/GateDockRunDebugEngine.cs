using Gate.Dock.DockApp;
using Gate.Dock.DockDocu;
using Gate.LangBase.Runtime.DbgEng;
using Gate.Tools.Extensions;
using Gate.ToolsView.Extensions;

namespace Gate.DockRuntimePlugin
{
   /// <summary>
   /// 
   /// </summary>
   public class GateDockRunDebugDbgEng : RtmDbgEng
   {
      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="app"></param>
      public GateDockRunDebugDbgEng(GateDockApp app) => App = app;

      /// <summary>
      /// 
      /// </summary>
      public GateDockApp App { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="breakThread"></param>
      protected override void myActionOnCurrentBreakChange(IRtmDbgEngThread? breakThread)
      {
         App.MainForm.MthInvoke(() =>
         {
            var tok = breakThread?.Stack?.TopCall?.DbgPointCurrent?.Token?.PrimitiveTokens.FirstOrDefault();
       
            if (tok != null && File.Exists(tok?.From?.Store?.FileInfo?.FullName ?? ""))
            {
               if (
                  App.MainForm.PpDocuHandler.OpenPath(
                     App.MainForm, (tok?.From?.Primitive?.Store?.FileInfo).NnOrCrash().FullName) is IGateDockDocuText txt_doc)
               {
                  txt_doc.PpDbgPointCurrent = tok;
               }
            }
            else
            {
               foreach (var txt_doc in App.MainForm.PpTabPagesAll.OfType<IGateDockDocuText>()) { txt_doc.PpDbgPointCurrent = null; }
            }
         });
      }
   }
}
