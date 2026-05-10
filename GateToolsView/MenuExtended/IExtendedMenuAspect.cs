using System.Drawing;

namespace Gate.ToolsView.MenuExtended
{
   public interface IExtendedMenuAspect
   {
      Color PpBackColorMargin { get; set; }
      Color PpBackColorSelected { get; set; }
      Color PpBorderColor { get; set; }
      Color PpItemBorderColor { get; set; }
      Color PpCheckBoxBackground { get; set; }
   }
}