using System.Drawing.Drawing2D;

namespace Gate.ToolsView.ControlFeature
{
   /// <summary>
   /// 
   /// </summary>
   public class CtrlFeatureBorder : CtrlFeature
   {
      private Color myBorderColor = Color.Yellow;
      private float myBorderWidth = 5.0f;
      private Lazy<Pen> myLazyBorderPen;
      private DashStyle myBorderDashStyle = DashStyle.Solid;

      public CtrlFeatureBorder() => myLazyBorderPen = new Lazy<Pen>(myMakePen);

      public override Type? SpecificControlType => null;

      public Color BorderColor
      {
         get => myBorderColor;
         set
         {
            myBorderColor = value;
            myLazyBorderPen = new Lazy<Pen>(myMakePen);
         }
      }

      public float BorderWidth
      {
         get => myBorderWidth;
         set
         {
            myBorderWidth = value;
            myLazyBorderPen = new Lazy<Pen>(myMakePen);
         }
      }

      public DashStyle BorderDashStyle
      {
         get => myBorderDashStyle;

         set
         {
            myBorderDashStyle = value;
            myLazyBorderPen = new Lazy<Pen>(myMakePen);
         }
      }

      protected override void myOnControlAssociate(Control control)
      {
         mySetStyle(control, ControlStyles.ResizeRedraw, true);
         control.Paint += Control_Paint;
      }

      protected override void myOnControlDeassociate(Control control) => control.Paint -= Control_Paint;

      private Pen myMakePen()
      {
         var pen = new Pen(BorderColor, BorderWidth);

         pen.DashStyle = BorderDashStyle;

         return pen;
      }

      private void Control_Paint(object? sender, PaintEventArgs e) =>
         e.Graphics.DrawRectangle(myLazyBorderPen.Value, BoundControl?.DisplayRectangle ?? Rectangle.Empty);
   }
}
