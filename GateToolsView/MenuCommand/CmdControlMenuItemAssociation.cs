using Gate.Tools;

namespace Gate.ToolsView.MenuCommand
{
   public class CmdControlMenuItemAssociation : ICmdControlAssociation
   {
      private Action<Cmd>? myAction;
      private bool? myIsChecked;
      private bool myIsVisible = true;

      public CmdControlMenuItemAssociation(ToolStripMenuItem menuItem, Cmd command)
      {
         Command = command ?? throw new Crash();
         MenuItem = menuItem ?? throw new Crash();
         MenuItem.Checked = false;//forced to false
      }

      public Cmd Command { get; }

      public bool IsEnabled
      {
         get => MenuItem.Enabled;

         set => MenuItem.Enabled = value;
      }

      public bool IsVisible
      {
         get => myIsVisible;

         set => myIsVisible = MenuItem.Visible = value;
      }

      public bool? IsChecked
      {
         get => myIsChecked;
         set
         {
            myIsChecked = value;
            MenuItem.Checked = value.HasValue && value.Value;
         }
      }

      public ToolStripMenuItem MenuItem { get; }

      public void SetAction(Action<Cmd>? action)
      {
         myAction = action;
         MenuItem.Click += MenuItem_Click;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="cmdCaption"></param>
      public void SetCmdCaption(string? cmdCaption) => MenuItem.Text = cmdCaption;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="image"></param>
      public void SetImage(Image? image) => MenuItem.Image = image;

      /// <summary>
      /// <br> In order to avoid System.ComponentModel.InvalidEnumArgumentException on setting ShortCutKey </br>
      /// <br> for certain Keys (such as ESC) ShortcutKeyDisplayString is used. </br>
      /// <br> Applications check for shortcut using HandleKeyForShortcuts. </br>
      /// </summary>
      /// <param name="shortCut"></param>
      public void SetShortCutPair(Keys shortCut, Keys shortCut2) => MenuItem.ShortcutKeyDisplayString = Command.ShortCutText;

      private void MenuItem_Click(object? sender, EventArgs e) => myAction?.Invoke(Command);
   }
}
