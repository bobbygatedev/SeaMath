using System.Diagnostics;

namespace Gate.Pad   
{
   static class Program
   {
      /// <summary>
      /// The main entry point for the application.
      /// </summary>
      [STAThread]
      static void Main(string[] cmdLine)
      {
         var app = new GatePadApp();
         var ico_fac = new GatePadIconFactory(Color.White, 64, 64);

         app.MainForm.PpImage = ico_fac.MakeImage();
         app.Start(cmdLine);
         Process.GetCurrentProcess().Kill();
      }
   }
}
