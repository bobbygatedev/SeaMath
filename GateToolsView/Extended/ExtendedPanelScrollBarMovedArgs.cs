using System.Windows.Forms;

namespace Gate.ToolsView.Extended
{
   /// <summary>
   /// 
   /// </summary>
   public class ExtendedPanelScrollBarMovedArgs
   {
      public ExtendedPanelScrollBarMovedArgs(ScrollEventArgs e, int scrollHValue, int scrollVValue)
      {
         ScrollEventArgs = e;
         ScrollHValue = scrollHValue;
         ScrollVValue = scrollVValue;
      }

      public bool IsCancel { get; set; } = false;
      public ScrollEventArgs ScrollEventArgs { get; }
      public int ScrollHValue { get; }
      public int ScrollVValue { get; }
   }
}

