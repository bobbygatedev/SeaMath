using GatePad.Properties;
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

         app.MainForm.PpImage = Resources.GatePadPng;
         app.MainForm.Icon = Resources.GatePadIcon;   
         app.Start(cmdLine);
         Process.GetCurrentProcess().Kill();
      }
   }
}
