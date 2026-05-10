using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Gate.ToolsView.TextCtrl
{
   /// <summary>
   /// 
   /// </summary>
   public partial class GateTextToolWinGoto : Form
   {
      private string myText;

      public GateTextToolWinGoto()
      {
         InitializeComponent();

         myText = CtrlTextNumber.Text = "1";
      }

      public int PpLine
      {
         get => int.Parse(CtrlTextNumber.Text);

         set
         {
            CtrlTextNumber.Text = value.ToString();
            CtrlTextNumber.SelectAll();
         }
      }

      protected override void OnActivated(EventArgs e)
      {
         base.OnActivated(e);

         CtrlTextNumber.Focus();
         CtrlTextNumber.SelectAll();
      }

      private void CtrlTextNumber_KeyDown(object? sender, KeyEventArgs e)
      {
         if (e.KeyCode == Keys.Enter)
         {
            e.Handled = true;
            e.SuppressKeyPress = true;
            DialogResult = DialogResult.OK;
            Close();
         }
         else if (myRepresentsPrintableChar(e.KeyData) && (e.KeyData < Keys.D0 || e.KeyData > Keys.D9))
         {
            e.SuppressKeyPress = true;
         }
      }

      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      private static extern int MapVirtualKey(int uCode, int uMapType);

      private static bool myRepresentsPrintableChar(Keys key) => !char.IsControl((char)MapVirtualKey((int)key, 2));

      private void CtrlTextNumber_TextChanged(object? sender, EventArgs e)
      {
         if (!int.TryParse(CtrlTextNumber.Text, out _)) { CtrlTextNumber.Text = myText; }
         else { myText = CtrlTextNumber.Text; }
      }
   }
}
