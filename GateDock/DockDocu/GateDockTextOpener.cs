using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.ToolsView.Extended;
using System.IO;

namespace Gate.Dock.DockDocu
{
   /// <summary>
   /// 
   /// </summary>
   public class GateDockTextOpener : ITextOpener
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="mainForm"></param>
      public GateDockTextOpener(GateDockMainForm mainForm) => MainForm = mainForm;

      /// <summary>
      /// 
      /// </summary>
      public GateDockMainForm MainForm { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="msg"></param>
      public void Open(Msg msg)
      {
         var pth = msg.FilePath.ExtTrim();

         if (pth != "" && (MainForm.PpDocuHandler.GetOpenPath(MainForm, pth) != null || File.Exists(pth)))
         {
            var doc_txt = MainForm.PpDocuHandler.OpenPath(MainForm, pth) as IGateDockDocuText;

            if (msg.Token != null) { doc_txt?.MthSelectTokenFromOtherControl(msg.Token); }
         }
      }
   }
}