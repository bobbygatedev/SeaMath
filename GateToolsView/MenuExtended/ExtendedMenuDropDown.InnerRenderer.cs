using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Gate.ToolsView.MenuExtended
{
   public partial class ExtendedMenuDropDown
   {
      private class InnerRenderer : ToolStripProfessionalRenderer
      {
         public InnerRenderer(InnerColorTable colorTable) : base(colorTable) { }

         protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
         {
            var pen = new Pen(((InnerColorTable)ColorTable).ParentMenuStrip.ForeColor, 2.5f);
            var r = new Rectangle(e.ImageRectangle.Location, e.ImageRectangle.Size);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            r.Inflate(-4, -6);
            e.Graphics.DrawLines(pen, new Point[]{
               new Point(r.Left, r.Bottom - r.Height /2),
               new Point(r.Left + (int)(r.Width /2.0),  r.Bottom),
               new Point(r.Right, r.Top)});
         }

         /// <summary>
         /// Draws the arrow (of a submenu)
         /// </summary>
         /// <param name="e"></param>
         protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
         {
            var pen = new Pen(((InnerColorTable)ColorTable).ParentMenuStrip.ForeColor, 2.5f);
            var r = e.ArrowRectangle;
            var h_s = 0.4;
            var v_s = 0.4;
            var hei = (int)(v_s * (r.Bottom - r.Top));
            var len = (int)(h_s * (r.Right - r.Left));
            var cnt = (r.Top + r.Bottom) / 2;
            var top = cnt + hei / 2;
            var bot = cnt - hei / 2;
            var lft = r.Left + (int)(len * h_s);
            var rgt = lft + len;

            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            //fills a triangle arrow with ForeColor = Text Color
            e.Graphics.FillPolygon(
               pen.Brush,
               new Point[] { new Point(lft, top), new Point(lft, bot), new Point(rgt, cnt) }); ;

            return;
         }
      }
   }
}
