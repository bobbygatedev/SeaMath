using ScintillaNET;
using System.Collections;

namespace Gate.ToolsView.TextCtrl
{
   public partial class GateTextControl
   {
      public class ScintillaMarkerCollection : IEnumerable<Marker>
      {
         public ScintillaMarkerCollection(GateTextControl gateTextControl) => GateTextControl = gateTextControl;

         public GateTextControl GateTextControl { get; }

         public Marker this[GateTextMarkerScintillaIdEnum markerId] => GateTextControl.PpScintilla.Markers.First(m => m.Index == (int)markerId);

         public IEnumerator<Marker> GetEnumerator() => GateTextControl.PpScintilla.Markers.GetEnumerator();

         IEnumerator IEnumerable.GetEnumerator() => GateTextControl.PpScintilla.Markers.GetEnumerator();
      }
   }
}


