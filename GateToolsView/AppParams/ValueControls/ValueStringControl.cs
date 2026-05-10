using Gate.Tools;
using Gate.Tools.AppParams;
using Gate.Tools.AppParams.ValueControls;
using System.ComponentModel;
using System.Windows.Forms.Layout;

namespace Gate.ToolsView.AppParams.ValueControls
{
   /// <summary>
   /// 
   /// </summary>
   [ValueControlAssociation(Id = ValueControlStandardId.simple_scalar)]
   public partial class ValueStringControl : UserControl, IValueControl
   {
      public const int LEFT_MARGIN = 10;

      /// <summary>
      /// Minimum distance between TextBox and Label
      /// </summary>
      private const int LAB_MIN_SPAN = 30;

      public delegate void OnAcceptCancelHandler(object? sender);
      public delegate bool TextConvertHandler(string @string, out object? value);
      public delegate string? ToStringHandler(object? value);

      private int myMinTextWidth = 80;
      private object? myValue;

      public event OnValueChangeHandler? OnValueChange;
      public event OnAcceptCancelHandler? OnAccept;
      public event OnAcceptCancelHandler? OnCancel;

      public ValueStringControl()
      {
         InitializeComponent();
      }

      private class InnerLayoutEngine : LayoutEngine
      {
         public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
         {
            var ctr = container as ValueStringControl ?? throw new Crash();

            var all_vc = ctr.Parent != null ? ctr.Parent.Controls.OfType<ValueStringControl>().ToArray() : [];
            var lab_max = all_vc.Length > 0 ? all_vc.Max(c => c.CtrlLabel.GetPreferredSize(new Size()).Width) + LAB_MIN_SPAN : LAB_MIN_SPAN;
            var tw = Math.Max(ctr.myMinTextWidth, ctr.Width - lab_max);

            ctr.CtrlLabel.PerformLayout();
            ctr.CtrlTextBox.PerformLayout();
            ctr.CtrlTextBox.Left = LEFT_MARGIN;
            ctr.CtrlTextBox.Width = tw;
            ctr.CtrlTextBox.Top = (ctr.Height - ctr.CtrlTextBox.Height) / 2;
            ctr.CtrlTextBox.Height = ctr.CtrlTextBox.PreferredHeight;
            ctr.CtrlLabel.Top = (ctr.Height - ctr.CtrlLabel.Height) / 2;
            ctr.CtrlLabel.Left = ctr.CtrlTextBox.Right + LEFT_MARGIN;
            ctr.CtrlLabel.Width = ctr.Width - ctr.CtrlLabel.Left;//label to the end

            return false;
         }
      }

      public override Size GetPreferredSize(Size proposedSize) =>
         new Size(
            3 * LEFT_MARGIN + myMinTextWidth + CtrlLabel.PreferredWidth, 
            Math.Max(CtrlTextBox.PreferredHeight, CtrlLabel.PreferredHeight));

      public TextConvertHandler? PpTextConverter { get; set; }

      public ToStringHandler? PpToStringHandler { get; set; }

      public int PpMinTextWidth
      {
         get => myMinTextWidth;

         set
         {
            myMinTextWidth = Math.Max(30, value);
            CtrlLabel.PerformLayout();
            PerformLayout();
         }
      }

      public string PpParamName { get => CtrlLabel.Text; set => CtrlLabel.Text = value; }

      public object? PpParamValue
      {
         get => myValue;

         set
         {
            myValue = value;

            if (myValue == null) { CtrlTextBox.Text = ""; }
            else if (PpToStringHandler != null) { CtrlTextBox.Text = PpToStringHandler(myValue); }
            else if (myValue is string) { CtrlTextBox.Text = (string)myValue; }
            else { CtrlTextBox.Text = ""; }

            OnValueChange?.Invoke(this, myValue);
         }
      }

      string IValueControl.ParamName { get => CtrlLabel.Text; set => CtrlLabel.Text = value; }

      object? IValueControl.ParamValue { get => PpParamValue; set => PpParamValue = value; }

      public override LayoutEngine LayoutEngine => new InnerLayoutEngine();

      private void CtrlTextBox_Validating(object? sender, CancelEventArgs e)
      {
         var txt = CtrlTextBox.Text;

         if (PpTextConverter != null)
         {
            if (PpTextConverter(txt, out var val))
            {
               myValue = val;
               OnValueChange?.Invoke(this, myValue);
            }
            else { e.Cancel = true; }
         }
         else
         {
            myValue = txt;
            OnValueChange?.Invoke(this, myValue);
         }
      }

      private void CtrlTextBox_KeyPress(object? sender, KeyPressEventArgs e)
      {
         if (e.KeyChar == '\r') { OnAccept?.Invoke(this); }
         else if (e.KeyChar == 27) { OnCancel?.Invoke(this); }
      }

      void IValueControl.ActionOnAppParamAssociationAction(AppParam param) { }
   }
}
