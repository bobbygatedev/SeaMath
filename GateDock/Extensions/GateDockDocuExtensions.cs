using Gate.Dock.DockDocu;
using Gate.Tools.Extensions;

namespace Gate.Dock.Extensions
{
   public static class GateDockDocuExtensions
   {
      public static string? GetMarkerPath(this IGateDockDocuText docuText) =>
         docuText.PpDocuPath.IsBlank() ? docuText.PpDocuName : docuText.PpDocuPath;
   }
}
