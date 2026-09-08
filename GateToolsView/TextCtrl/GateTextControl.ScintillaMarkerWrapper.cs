using ScintillaNET;

namespace Gate.ToolsView.TextCtrl
{
   public partial class GateTextControl
   {
      public class ScintillaMarkerWrapper
      {
         public ScintillaMarkerWrapper(GateTextControl gateTextControl, MarkerHandle handle, GateTextMarkerScintillaIdEnum scintillaId)
         {
            GateTextControl = gateTextControl;
            Handle = handle;
            ScintillaId = scintillaId;
         }

         public GateTextControl GateTextControl { get; }
         public MarkerHandle Handle { get; }
         public GateTextMarkerScintillaIdEnum ScintillaId { get; }
         public Marker ScintillaMarker => GateTextControl.PpScintilla.Markers[(int)ScintillaId];
         public int Line => GateTextControl.PpScintilla.MarkerLineFromHandle(Handle) + 1;
      }
   }
}


