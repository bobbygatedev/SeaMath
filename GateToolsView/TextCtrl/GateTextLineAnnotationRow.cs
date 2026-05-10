namespace Gate.ToolsView.TextCtrl
{
   public class GateTextLineAnnotationRow
   {
      private string myText;
      private GateTextStyle myStyle;

      internal GateTextLineAnnotationRow(GateTextLineAnnotation lineAnnotation, string text, GateTextStyle style)
      {
         LineAnnotation = lineAnnotation;
         myText = text;
         myStyle = style;
      }

      public GateTextLineAnnotation LineAnnotation { get; }

      public string Text
      {
         get => myText;
         set
         {
            myText = value;
            LineAnnotation.UpdateRows();
         }
      }

      public GateTextStyle Style
      {
         get => myStyle;
         set
         {
            myStyle = value;
            LineAnnotation.UpdateRows();
         }
      }
   }
}
