using System;
using System.Drawing;

namespace Gate.Tools.Extensions
{
   /// <summary>
   /// Extensions for Color
   /// </summary>
   public static class ColorExtender
   {
      /// <summary>
      /// Returns the <see cref="ConsoleColor"/> that most closely matches the specified <see cref="Color"/>.
      /// </summary>
      /// <remarks>This method selects the <see cref="ConsoleColor"/> whose RGB values are nearest to those
      /// of the specified <see cref="Color"/>. The result may not be an exact match, as <see cref="ConsoleColor"/>
      /// supports a limited set of colors.</remarks>
      /// <param name="color">The <see cref="Color"/> to convert to a corresponding <see cref="ConsoleColor"/>.</param>
      /// <returns>The <see cref="ConsoleColor"/> that is the closest visual match to the specified <paramref name="color"/>.</returns>
      public static ConsoleColor ToConsoleColor(this Color color)
      {
         var bst_col = ConsoleColor.Black;
         var bst_dst = double.MaxValue;

         foreach (ConsoleColor cc in Enum.GetValues(typeof(ConsoleColor)))
         {
            var c = ConsoleColorToColor(cc);
            
            var dst =
                Math.Pow(c.R - color.R, 2) +
                Math.Pow(c.G - color.G, 2) +
                Math.Pow(c.B - color.B, 2);

            if (dst < bst_dst)
            {
               bst_dst = dst;
               bst_col = cc;
            }
         }

         return bst_col;
      }

      /// <summary>
      /// Converts a <see cref="ConsoleColor"/> value to its corresponding <see cref="Color"/> representation.
      /// </summary>
      /// <remarks>This method provides an approximate mapping between <see cref="ConsoleColor"/> values and
      /// <see cref="Color"/> objects. The returned color may not be an exact match, but is intended to visually
      /// resemble the console color as closely as possible.</remarks>
      /// <param name="consoleColor">The console color to convert.</param>
      /// <returns>A <see cref="Color"/> that closely matches the specified <paramref name="consoleColor"/>.</returns>
      public static Color ConsoleColorToColor(this ConsoleColor consoleColor)
      {
         switch (consoleColor)
         {
            case ConsoleColor.Black: return Color.Black;
            case ConsoleColor.DarkBlue: return Color.DarkBlue;
            case ConsoleColor.DarkGreen: return Color.DarkGreen;
            case ConsoleColor.DarkCyan: return Color.DarkCyan;
            case ConsoleColor.DarkRed: return Color.DarkRed;
            case ConsoleColor.DarkMagenta: return Color.DarkMagenta;
            case ConsoleColor.DarkYellow: return Color.FromArgb(128, 128, 0);
            case ConsoleColor.Gray: return Color.Gray;
            case ConsoleColor.DarkGray: return Color.DarkGray;
            case ConsoleColor.Blue: return Color.Blue;
            case ConsoleColor.Green: return Color.Green;
            case ConsoleColor.Cyan: return Color.Cyan;
            case ConsoleColor.Red: return Color.Red;
            case ConsoleColor.Magenta: return Color.Magenta;
            case ConsoleColor.Yellow: return Color.Yellow;
            case ConsoleColor.White: return Color.White;
            default: return Color.White;
         }
      }
   }
}
