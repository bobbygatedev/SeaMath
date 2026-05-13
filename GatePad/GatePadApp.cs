using Gate.Dock.DockApp;
using Gate.Tools;
using Gate.Tools.Extensions;
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
            var fls = myGetFiles(cmdLine);

            if (fls != null)
            {
               myPadApp.OnLoadFinished += (_) =>
               {
                  foreach (var fil in fls)
                  {
                     myPadApp.MainForm.PpDocuHandler.OpenPath(myPadApp.MainForm, fil);
                  }
               };
            }

            myPadApp.MainForm.VisibleChanged += (s, e) =>
            {
               //force tab page to be the first tab page of first tab (or null if nothingt is open).
               if (myPadApp.MainForm.Visible)
               {
                  myPadApp.MainForm.PpTabPageCurrent =
                     myPadApp.MainForm.PpTabPageCurrent ??
                        myPadApp.MainForm.PpTabsAll.FirstOrDefault();
               }
            };

            myPadApp.MainForm.ShowDialog();
         }

         private string[] myGetFiles(string[] cmdLine) => cmdLine.Where(f => File.Exists(f)).ToArray();

         protected override void myEntryPointNoMutexRemote(string[] cmdLine, object? localToRemoteParams)
         {
            var fls = myGetFiles(cmdLine);

            if (fls.Length > 0)
            {
               //current
               Directory.SetCurrentDirectory(Path.GetDirectoryName(fls.FirstOrDefault().NnOrCrash()).NnOrCrash());
               myPadApp.MainForm.MthInvoke(() =>
               {

                  foreach (var fil in fls)
                  {
                     myPadApp.MainForm.PpDocuHandler.OpenPath(myPadApp.MainForm, fil);
                  }
               });
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
