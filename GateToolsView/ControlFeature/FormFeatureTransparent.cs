using System.Drawing;
using System.Windows.Forms;

namespace Gate.ToolsView.ControlFeature
{
   public class FormFeatureTransparent : CtrlFeature.Specialized<Form>
   {
      private Color myTransparentColor = Color.Black;

      public Color TransparentColor
      {
         get => myTransparentColor;
         set
         {
            myTransparentColor = value;

            if (BoundControl != null) { ((Form)BoundControl).TransparencyKey = myTransparentColor; }
         }
      }

      protected override void myOnControlAssociate(Control control)
      {
         if (control is Form frm)
         {
            mySetStyle(frm, ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            mySetStyle(frm, ControlStyles.Selectable, false);
            frm.TransparencyKey = TransparentColor;
            frm.BackColor = TransparentColor;
            frm.Paint += Control_Paint;
         }
      }

      protected override void myOnControlDeassociate(Control control) => control.Paint -= Control_Paint;

      private void Control_Paint(object? sender, PaintEventArgs e)
      {
         var pen = new Pen(TransparentColor);//transparent color

         e.Graphics.FillRectangle(pen.Brush, BoundControl.DisplayRectangle);
      }
   }
}
