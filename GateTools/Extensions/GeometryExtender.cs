using System.Drawing;

namespace Gate.Tools.Extensions
{
   public static class GeometryExtender
   {
      public static bool IsInside(this Rectangle rect, Point point) =>
         point.X >= rect.Left && point.X < rect.Right && point.Y >= rect.Top && point.Y < rect.Bottom;

      public static int Compare(this Point p1, Point p2)
      {
         var c1 = p1.Y.CompareTo(p2.Y);

         return c1 != 0 ? c1 : p1.X.CompareTo(p2.X);
      }
   }
}
