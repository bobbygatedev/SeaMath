using ScintillaNET;

namespace Gate.ToolsView.TextCtrl
{
   public enum GateTextAnnotationMode
   {
      hidden = Annotation.Hidden,

      /// <summary>
      /// Annotations are drawn left justified with no adornment.
      /// </summary>
      standard = Annotation.Standard,

      /// <summary>
      /// Annotations are indented to match the text and are surrounded by a box.
      /// </summary>
      boxed = Annotation.Boxed,

      /// <summary>
      /// Annotations are indented to match the text.
      /// </summary>
      indented = Annotation.Indented
   }
}

