using Gate.ToolsView.TextCtrl;
using System;
using System.Windows.Forms;

namespace Gate.ToolsViewTest
{
   public class GateTextControlTest
   {
      static void Main(string[] args)
      {
         //Console.WriteLine($"Me allocated: {GC.GetAllocatedBytesForCurrentThread() / (1024 * 1024.0)} Mb, Total = {GC.GetTotalMemory(true) / (1024 * 1024.0)} Mb");

         //var sto = TxtStore.FromPath(@"c:\test\tst.c");

         //Console.WriteLine($"Me allocated: {GC.GetAllocatedBytesForCurrentThread() / (1024 * 1024.0)} Mb, Total = {GC.GetTotalMemory(true) / (1024 * 1024.0)} Mb");

         //var ln = sto[1];

         //Console.WriteLine($"Me allocated: {GC.GetAllocatedBytesForCurrentThread() / (1024 * 1024.0)} Mb, Total = {GC.GetTotalMemory(true) / (1024 * 1024.0)} Mb");

         //return;

         var frm = new Form();

         var ctr = new GateTextControl();

         ctr.Dock = DockStyle.Fill;

         Console.WriteLine($"Memory: {GC.GetAllocatedBytesForCurrentThread() / (1024 * 1024.0)} Mb");

         frm.Controls.Add(ctr);
         frm.Load += (s, e) =>
         {
            ctr.MthOpenFile(@"c:\test\tst.c");
            Console.WriteLine($"Memory: {GC.GetAllocatedBytesForCurrentThread() / (1024 * 1024.0)} Mb");
         };
         frm.ShowDialog();
      }

   }
}
