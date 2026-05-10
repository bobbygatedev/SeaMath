using Gate.Tools;
using Gate.Tools.Extensions;
using System;
using System.Windows.Forms;

namespace Gate.ToolsView.ControlFeature
{
   public class CtrlFeatureToolTip : CtrlFeature
   {
      public CtrlFeatureToolTip()
      {
         ToolTip.AutomaticDelay = 7000;//7s stay on 
         ToolTip.InitialDelay = 100;//0.1 s before is shown after hoovering on select button
      }

      public override Type? SpecificControlType => null;

      public string? ToolTipText { get; set; } = "";

      public ToolTip ToolTip { get; private set; } = new ToolTip();

      protected override void myOnControlAssociate(Control control) => control.MouseHover += Control_MouseHover;

      protected override void myOnControlDeassociate(Control control) => control.MouseHover -= Control_MouseHover;

      private void Control_MouseHover(object? sender, EventArgs e)
      {
         if (!ToolTipText.IsBlank()) { ToolTip.Show(ToolTipText, BoundControl ?? throw new Crash()); }
      }
   }
}
