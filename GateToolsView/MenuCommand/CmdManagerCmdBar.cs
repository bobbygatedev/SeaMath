using System.Windows.Forms;

namespace Gate.ToolsView.MenuCommand
{
   public partial class CmdManagerCmdBar : UserControl
   {
      public enum Cmd
      {
         up = 0,
         down = 1,
         delete = 2,
         add = 3
      }

      public delegate void OnCmdHandler(Button? sender, Cmd cmd);

      public event OnCmdHandler? OnCmd;

      public CmdManagerCmdBar()
      {
         InitializeComponent();
      }

      private void CtrlButtonDelete_Click(object? sender, System.EventArgs e) => OnCmd?.Invoke(sender as Button, Cmd.delete);

      private void CtrlButtonMoveUp_Click(object? sender, System.EventArgs e) => OnCmd?.Invoke(sender as Button, Cmd.up);

      private void CtrlButtonMoveDown_Click(object? sender, System.EventArgs e) => OnCmd?.Invoke(sender as Button, Cmd.down);

      private void CtrlButtonAdd_Click(object? sender, System.EventArgs e) => OnCmd?.Invoke(sender as Button, Cmd.add);
   }
}
