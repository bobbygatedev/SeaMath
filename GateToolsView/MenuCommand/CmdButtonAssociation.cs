using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.ControlFeature;
using Gate.ToolsView.ControlFeature.Extensions;
using Gate.ToolsView.Extended;

namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// 
   /// </summary>
   public class CmdButtonAssociation : ICmdControlAssociation
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="button"></param>
      /// <param name="command"></param>
      public CmdButtonAssociation(Button button, Cmd command)
      {
         Command = command;
         Button = button;
         button.AddFeature<CtrlFeatureToolTip>();
      }

      /// <summary>
      /// Command associated.
      /// </summary>
      public Cmd Command { get; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsEnabled { get => Button.Enabled; set => Button.Enabled = value; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsVisible { get => Button.Visible; set => Button.Visible = value; }

      /// <summary>
      /// 
      /// </summary>
      public bool? IsChecked
      {
         get => Button is IButtonToggable tog ? tog.IsToggled : null;

         set
         {
            if (Button is IButtonToggable tog) { tog.IsToggled = value; }
         }
      }

      /// <summary>
      /// Associated button.
      /// </summary>
      public Button Button { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="action"></param>
      public void SetAction(Action<Cmd>? action) => Button.Click += (s, e) => action?.Invoke(Command);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="cmdCaption"></param>
      public void SetCmdCaption(string? cmdCaption)
      {
         var fea = Button.GetFeature<CtrlFeatureToolTip>() ?? throw new Crash();

         fea.ToolTipText =
            $"{cmdCaption.ExtTrim().Replace("&", "")} " +
            $"{(Command.ShortCutText.IsBlank() ? "" : $"({Command.ShortCutText})")}".Trim();

         if (Button.Image == null) { Button.Text = cmdCaption; }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="image"></param>
      public void SetImage(Image? image)
      {
         if ((Button.Image = image) == null) { Button.Text = Command.Caption; }
         else { Button.Text = ""; }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="shortCut"></param>
      /// <param name="shortCut2"></param>
      public void SetShortCutPair(Keys shortCut, Keys shortCut2) { }
   }
}
