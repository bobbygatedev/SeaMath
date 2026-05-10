using System.Windows.Forms;

namespace Gate.ToolsView.Extended
{
   /// <summary>
   /// Use internally for cancel control flickering
   /// </summary>
   internal class InternalUserControl : UserControl
   {
      public InternalUserControl() => DoubleBuffered = true;
   }
}
