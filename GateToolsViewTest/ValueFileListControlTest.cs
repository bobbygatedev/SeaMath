using System;
using System.Windows.Forms;

namespace Gate.ToolsViewTest
{
   internal partial class ValueFileListControlTest : Form
   {
      public ValueFileListControlTest()
      {
         InitializeComponent();
      }

      [STAThread]
      static void Main()
      {
         var frm = new ValueFileListControlTest();

         frm.ShowDialog();

         Console.WriteLine(string.Join("\n", frm.CtrlFileListControl.PpPaths));
      }

      private void ValueFileListControTest_Load(object sender, EventArgs e)
      {

      }
   }
}
