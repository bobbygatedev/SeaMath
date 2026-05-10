using Gate.Tools;
using Gate.ToolsView.ControlFeature;
using Gate.ToolsView.ControlFeature.Extensions;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// Associate a virtual command just for setting caption at language changes
   /// </summary>
   public class CmdControlForLanguageAssociation : ICmdControlAssociation
   {
      /// <summary></summary>
      /// <param name="control"></param>
      /// <param name="command"></param>
      public CmdControlForLanguageAssociation(Control control, Cmd command)
      {
         Command = command;
         Control = control;
      }

      /// <summary>
      /// Command associated.
      /// </summary>
      public Cmd Command { get; }

      /// <summary>
      /// 
      /// </summary>
      public Control Control { get; }

      public bool? IsChecked { get => null; set { } }

      public bool IsEnabled { get => Control.Enabled; set => Control.Enabled = value; }

      public bool IsVisible { get => Control.Visible; set => Control.Visible = value; }

      /// <summary>
      /// Action not set.
      /// </summary>
      /// <param name="action"></param>
      public void SetAction(Action<Cmd>? action) { }

      /// <summary></summary>
      /// <param name="cmdCaption"></param>
      public void SetCmdCaption(string? cmdCaption)
      {
         var fea = Control.GetFeature<CtrlFeatureToolTip>() ?? throw new Crash();
         
         fea.ToolTipText =
            $"{cmdCaption?.Replace("&", "")} {(Command.ShortCutText.Trim() != "" ? $"({Command.ShortCutText})" : "")}".Trim();
      }

      /// <summary></summary>
      /// <param name="image"></param>
      public void SetImage(Image? image) { }
      public void SetShortCutPair(Keys shortCut, Keys shortCut2) { }
   }
}
