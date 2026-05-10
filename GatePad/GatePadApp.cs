using Gate.Dock.DockApp;
using Gate.Tools;
using Gate.ToolsView.Extensions;

namespace Gate.Pad
{
   public class GatePadApp : GateDockApp
   {
      public GatePadApp()
      {

      }

      private class InnerSingleInstanceApp : SingleInstanceApp
      {
         private readonly GatePadApp myPadApp;

         public InnerSingleInstanceApp(GatePadApp gatePadApp) => myPadApp = gatePadApp;

         public string CurrDir => System.IO.Directory.GetCurrentDirectory();

         public override string AppId => "Local\\037EA27E-42CD-46FB-9787-7D0977C6C685";

         protected override void myEntryPointWithToken(string[] cmdLine)
         {
            var fil_nam = myGetFile(cmdLine);

            if (fil_nam != null) { myPadApp.OnLoadFinished += (_) => myPadApp.MainForm.PpDocuHandler.OpenPath(myPadApp.MainForm, fil_nam); }

            myPadApp.MainForm.VisibleChanged += (s, e) =>
            {
               //force tab page to be the first tab page of first tab (or null if nothingt is open).
               if (myPadApp.MainForm.Visible)
               {
                  myPadApp.MainForm.PpTabPageCurrent = myPadApp.MainForm.PpTabPageCurrent ?? myPadApp.MainForm.PpTabsAll.FirstOrDefault();
               }
            };

            myPadApp.MainForm.ShowDialog();
         }

         private string? myGetFile(string[] cmdLine) => (cmdLine ?? []).FirstOrDefault(f => File.Exists(f));

         protected override void myEntryPointNoMutexRemote(string[] cmdLine, object? localToRemoteParams)
         {
            var fil_nam = myGetFile(cmdLine);

            if (fil_nam != null)
            {
               //current
               Directory.SetCurrentDirectory(Path.GetDirectoryName(fil_nam) ?? throw new Crash());
               myPadApp.MainForm.MthInvoke(() => myPadApp.MainForm.PpDocuHandler.OpenPath(myPadApp.MainForm, fil_nam));
            }
         }

         /// <summary>
         /// 
         /// </summary>
         /// <returns></returns>
         protected override object? myGetEntryPointNoMutexLocalToRemoteParams() => null;
      }

      public override string Name => "GatePad";

      public void Start(string[] cmdLine) => new InnerSingleInstanceApp(this).Start(cmdLine);
   }
}
