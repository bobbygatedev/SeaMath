namespace Gate.Pad
{
   public class GatePadIconFactory
   {
      public GatePadIconFactory(Color? foreColor = null, int height = 50, int width = 50)
      {
         ForeColor = foreColor ?? Color.White;
         Height = height;
         Width = width;
      }

      public Color ForeColor { get; set; }

      public int Height { get; set; }

      public int Width { get; set; }

      public Icon MakeIcon() => Icon.FromHandle(MakeBitmap().GetHicon());

      public Bitmap MakeBitmap()
      {
         var bmp = new Bitmap(MakeImage());

         for (var x = 0; x < bmp.Height; x++)
         {
            for (var y = 0; y < bmp.Width; y++)
            {
               if (bmp.GetPixel(x, y).A != 0) { bmp.SetPixel(x, y, Color.DarkGray); }
            }
         }

         return bmp;
      }

      public Image MakeImage() => myGetImage(ForeColor, Height, Width);

      public void SaveIcon(string path)
      {
         var ico = MakeIcon();

         using (var fs = new FileStream(path, FileMode.Create))
         {
            ico.Save(fs);
         }
      }

      private static Image myGetImage(Color foreColor, int height, int width)
      {
         var sml_siz = Math.Min(height, width);
         var mar = (int)(0.32 * sml_siz);
         var pen_wdt = 2.1f;
         var wdt = width - 2 * mar;
         var hei = height - 2 * mar;
         var g_h = (int)(0.75 * hei);
         var g_h_c = hei - g_h;

         var bmp = new Bitmap(width, height);

         using (var gr = Graphics.FromImage(bmp))
         {
            var pen = new Pen(new SolidBrush(foreColor), pen_wdt);

            gr.DrawLine(pen, new Point(width / 2, mar + g_h), new Point(width / 2, mar));
            gr.DrawLine(pen, new Point(mar, mar + g_h_c), new Point(mar, mar + hei));
            gr.DrawLine(pen, new Point(mar, mar + g_h_c), new Point(width / 2, mar));
            gr.DrawLine(pen, new Point(width / 2, mar + g_h), new Point(mar, mar + hei));
            gr.DrawLine(pen, new Point(mar + wdt, mar + g_h_c), new Point(mar + wdt, mar + hei));
            gr.DrawLine(pen, new Point(mar + wdt, mar + g_h_c), new Point(width / 2, mar));
            gr.DrawLine(pen, new Point(width / 2, mar + g_h), new Point(mar + wdt, mar + hei));
         }

         bmp.MakeTransparent();

         return bmp;
      }

   }
}
