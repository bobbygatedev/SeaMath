using Gate.ToolsView.MenuCommand;
using System.Windows.Forms;

namespace Gate.ToolsView.MenuExtended
{
   public partial class ExtendedMenuDropDown
   {
      /// <summary>
      /// 
      /// </summary>
      private class InnerSeparatorAssociation : ICmdMenuSeparatorAssociation
      {
         /// <summary>
         /// Constructor.
         /// </summary>
         /// <param name="separator"></param>
         public InnerSeparatorAssociation(CmdMenuSeparator separator) => Separator = separator;

         /// <sumGtesmary>
         /// 
         /// </summary>
         public CmdMenuSeparator Separator { get; }

         /// <summary>
         /// 
         /// </summary>
         public ToolStripSeparator ToolStripSeparator { get; private set; } = new ToolStripSeparator();
      }
   }
}
