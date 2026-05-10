using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace Gate.ToolsView.Extended
{
   public partial class CtrlWithLabel : UserControl
   {
      private const int SPACING = 10;


      private int myFixedTextWidth = 30;
      private Control? myBasedControl;

      public CtrlWithLabel()
      {
         InitializeComponent();
      }

      private class InnerLayoutEngine : LayoutEngine
      {
         public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
         {
            var ctr = (CtrlWithLabel)container;

            ctr.CtrlLabel.PerformLayout();

            if (ctr.PpBasedControl != null)
            {
               ctr.PpBasedControl.PerformLayout();
               ctr.PpBasedControl.Left = SPACING;
               ctr.PpBasedControl.Width = ctr.myFixedTextWidth;
               ctr.PpBasedControl.Top = (ctr.Height - ctr.PpBasedControl.Height) / 2;
               ctr.PpBasedControl.Height = ctr.PpBasedControl.PreferredSize.Height;
               ctr.CtrlLabel.Top = (ctr.Height - ctr.CtrlLabel.Height) / 2;
               ctr.CtrlLabel.Left = ctr.PpBasedControl.Right + SPACING;
               ctr.CtrlLabel.Width = ctr.Width - ctr.CtrlLabel.Left;//label to the end
            }
            else
            {
               ctr.CtrlLabel.Location = new Point();
               ctr.CtrlLabel.Size = ctr.Size;
            }

            return false;
         }
      }

      public int PpFixedTextWidth
      {
         get => myFixedTextWidth;

         set
         {
            myFixedTextWidth = Math.Max(30, value);
            CtrlLabel.PerformLayout();
            PerformLayout();
         }
      }

      public Control? PpBasedControl
      {
         get => myBasedControl;

         set
         {
            if (myBasedControl != value)
            {
               if (myBasedControl != null) { Controls.Remove(myBasedControl); }

               if ((myBasedControl = value) != null) { Controls.Add(myBasedControl); }

               PerformLayout();
            }
         }
      }

      public string ParamName { get => CtrlLabel.Text; set => CtrlLabel.Text = value; }

      public override LayoutEngine LayoutEngine => new InnerLayoutEngine();

      public override Size GetPreferredSize(Size proposedSize) =>
         myBasedControl != null ?
            new Size(3 * SPACING + myFixedTextWidth + CtrlLabel.PreferredWidth, Math.Max(myBasedControl.PreferredSize.Height, CtrlLabel.PreferredHeight)) : proposedSize;
   }
}
