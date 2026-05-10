using ScintillaNET;
using System.Collections;

namespace Gate.ToolsView.TextCtrl
{
   public class GateTextStyle
   {
      private Style myScintillaStyle;

      public const int ID_MIN = 0;
      public const int ID_MAX = 255;

      internal GateTextStyle(Style style)
      {
         myScintillaStyle = style;
      }

      public class Collection : IEnumerable<GateTextStyle>
      {
         private GateTextStyle[] myStyles;
         private Scintilla myScintilla;

         internal Collection(Scintilla scintilla)
         {
            myScintilla = scintilla;
            myStyles = Enumerable.Range(ID_MIN, ID_MAX - ID_MIN + 1).Select(i => new GateTextStyle(scintilla.Styles[i])).ToArray();
         }

         public GateTextStyle this[GateTextStyleEnum index] => myStyles[(int)index];

         public int Count => myStyles.Length;

         public IEnumerator<GateTextStyle> GetEnumerator() => myStyles.ToList().GetEnumerator();

         IEnumerator IEnumerable.GetEnumerator() => myStyles.GetEnumerator();
      }

      /// <summary>
      /// Background color of the style.
      ///</summary>
      /// <returns>A Color object representing the style background color. The default is White.</returns>
      /// <remarks>Alpha color values are ignored.</remarks>
      public Color BackColor { get => myScintillaStyle.BackColor; set => myScintillaStyle.BackColor = value; }

      /// <summary>
      /// Whether the style font is bold.
      ///</summary>
      /// <returns>true if bold; otherwise, false. The default is false.</returns>
      /// <remarks>Setting this property affects the <see cref="Weight" /> property.</remarks>
      public bool Bold { get => myScintillaStyle.Bold; set => myScintillaStyle.Bold = value; }

      /// <summary>
      /// Casing used to display the styled text.
      /// </summary>
      /// <returns>One of the <see cref="StyleCase" /> enum values. The default is <see cref="StyleCase.Mixed" />.</returns>
      /// <remarks>This does not affect how text is stored, only displayed.</remarks>
      public StyleCase Case { get => myScintillaStyle.Case; set => myScintillaStyle.Case = value; }

      /// <summary>
      /// <br> Whether the remainder of the line is filled with the <see cref="BackColor" /> </br>
      /// <br> when this style is used on the last character of a line. </br>
      ///</summary>
      /// <returns>true to fill the line; otherwise, false. The default is false.</returns>
      public bool FillLine { get => myScintillaStyle.FillLine; set => myScintillaStyle.FillLine = value; }

      /// <summary>
      /// Style font name.
      /// </summary>
      /// <returns>The style font name. The default is Verdana.</returns>
      /// <remarks>Scintilla caches fonts by name so font names and casing should be consistent.</remarks>
      public string Font { get => myScintillaStyle.Font; set => myScintillaStyle.Font = value; }

      /// <summary>
      /// Foreground color of the style.
      /// </summary>
      /// <returns>A Color object representing the style foreground color. The default is Black.</returns>
      /// <remarks>Alpha color values are ignored.</remarks>
      public Color ForeColor { get => myScintillaStyle.ForeColor; set => myScintillaStyle.ForeColor = value; }

      /// <summary>
      /// Whether hovering the mouse over the style text exhibits hyperlink behavior.
      /// </summary>
      /// <returns>true to use hyperlink behavior; otherwise, false. The default is false.</returns>
      public bool Hotspot { get => myScintillaStyle.Hotspot; set => myScintillaStyle.Hotspot = value; }

      /// <summary>
      /// Zero-based style definition index.
      /// </summary>
      /// <returns>The style definition index within the <see cref="StyleCollection" />.</returns>
      public GateTextStyleEnum Index => (GateTextStyleEnum)myScintillaStyle.Index;

      /// <summary>
      /// Whether the style font is italic.
      /// </summary>
      /// <returns>true if italic; otherwise, false. The default is false.</returns>
      public bool Italic { get => myScintillaStyle.Italic; set => myScintillaStyle.Italic = value; }

      /// <summary>
      /// Size of the style font in points.
      ///</summary>
      /// <returns>The size of the style font as a whole number of points. The default is 8.</returns>
      public int Size { get => myScintillaStyle.Size; set => myScintillaStyle.Size = value; }

      /// <summary>
      /// Size of the style font in fractoinal points.
      /// </summary>
      /// <returns>The size of the style font in fractional number of points. The default is 8.</returns>
      public float SizeF { get => myScintillaStyle.SizeF; set => myScintillaStyle.SizeF = value; }

      /// <summary>
      /// Whether the style is underlined.
      /// </summary>
      /// <returns>true if underlined; otherwise, false. The default is false.</returns>
      public bool Underline { get => myScintillaStyle.Underline; set => myScintillaStyle.Underline = value; }

      /// <summary>
      /// Whether the style text is visible.
      ///</summary>
      /// <returns>true to display the style text; otherwise, false. The default is true.</returns>
      public bool Visible { get => myScintillaStyle.Visible; set => myScintillaStyle.Visible = value; }

      /// <summary>
      /// Style font weight.
      ///</summary>
      /// <returns>The font weight. The default is 400.</returns>
      /// <remarks>Setting this property affects the <see cref="Bold" /> property.</remarks>
      public int Weight { get => myScintillaStyle.Weight; set => myScintillaStyle.Weight = value; }
   }
}
