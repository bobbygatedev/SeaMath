using System.Runtime.InteropServices;

namespace Gate.ToolsView.Native
{
   [StructLayout(LayoutKind.Sequential)]
   public struct Rect
   {
      public int Left;
      public int Top;
      public int Right;
      public int Bottom;

      public Rect(int left, int top, int right, int bottom)
      {
         this.Left = left;
         this.Top = top;
         this.Right = right;
         this.Bottom = bottom;
      }

      public static Rect FromXYWH(int x, int y, int width, int height) => new Rect(x, y, x + width, y + height);

      public Rectangle GetRectangle() => Rectangle.FromLTRB(Left, Top, Right, Bottom);
   }
}
