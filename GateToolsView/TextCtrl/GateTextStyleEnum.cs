using ScintillaNET;

namespace Gate.ToolsView.TextCtrl
{
   public enum GateTextStyleEnum
   {
      /// <summary>
      /// 
      /// </summary>
      Default = Style.Default,

      /// <summary>
      /// Line number style index. This style is used for text in line number margins. The background color of this style also
      /// sets the background color for all margins that do not have any folding mask set.
      /// </summary>
      LineNumber = Style.LineNumber,

      /// <summary>
      /// Call tip style index. Only font name, size, foreground color, background color, and character set attributes
      /// can be used when displaying a call tip.
      /// </summary>
      CallTip = Style.CallTip,

      /// <summary>
      /// Indent guide style index. This style is used to specify the foreground and background colors of <see cref="Scintilla.IndentationGuides" />.
      /// </summary>
      IndentGuide = Style.IndentGuide,

      /// <summary>
      /// Brace highlighting style index. This style is used on a brace character when set with the <see cref="Scintilla.BraceHighlight" /> method
      /// or the indentation guide when used with the <see cref="Scintilla.HighlightGuide" /> property.
      /// </summary>
      BraceLight = Style.BraceLight,

      /// <summary>
      /// Bad brace style index. This style is used on an unmatched brace character when set with the <see cref="Scintilla.BraceBadLight" /> method.
      /// </summary>
      BraceBad = Style.BraceBad,

      /// <summary>
      /// Fold text tag style index. This is the style used for drawing text tags attached to folded text when
      /// <see cref="Scintilla.FoldDisplayTextSetStyle" /> and <see cref="Line.ToggleFoldShowText" /> are used.
      /// </summary>
      FoldDisplayText = Style.FoldDisplayText,

      User_Min = 40 ,

      User_0 = 40 ,
      User_1 ,
      User_2,
      User_3,
      User_4,
      User_5,
      User_6,
      User_7,
      User_8,



      User_Max = 255,
      /// <summary>
      /// 
      /// </summary>
      Max = 255,
   }
}
