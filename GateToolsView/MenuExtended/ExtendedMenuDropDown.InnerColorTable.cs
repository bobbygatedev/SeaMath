using System.Drawing;
using System.Windows.Forms;

namespace Gate.ToolsView.MenuExtended
{
   public partial class ExtendedMenuDropDown
   {
      /// <summary>
      /// 
      /// </summary>
      private class InnerColorTable : ProfessionalColorTable
      {
         public InnerColorTable(ExtendedMenuDropDown parentMenuStrip)
         {
            ParentMenuStrip = parentMenuStrip;
            base.UseSystemColors = false;
         }

         public ExtendedMenuDropDown ParentMenuStrip { get; internal set; }

         public override Color ButtonSelectedBorder => Color.Transparent;

         public override Color CheckBackground => ParentMenuStrip.PpCheckBoxBackground;

         public override Color CheckPressedBackground => ParentMenuStrip.PpCheckBoxBackground;

         public override Color CheckSelectedBackground => ParentMenuStrip.PpCheckBoxBackground;

         public override Color ToolStripDropDownBackground => ParentMenuStrip.BackColor != Color.Empty ? ParentMenuStrip.BackColor : base.ToolStripDropDownBackground;

         public override Color MenuItemSelected => ParentMenuStrip.PpBackColorSelected != Color.Empty ? ParentMenuStrip.PpBackColorSelected : base.MenuItemSelected;

         public override Color MenuItemBorder => ParentMenuStrip.PpBorderColor != Color.Empty ? ParentMenuStrip.PpBorderColor : MenuItemSelected;

         public override Color MenuBorder => ParentMenuStrip.PpItemBorderColor != Color.Empty ? ParentMenuStrip.PpItemBorderColor : MenuItemBorder;

         public override Color ImageMarginGradientBegin => ParentMenuStrip.PpBackColorMargin != Color.Empty ? ParentMenuStrip.PpBackColorMargin : ToolStripDropDownBackground;

         public override Color ImageMarginGradientMiddle => ImageMarginGradientBegin;

         public override Color ImageMarginGradientEnd => ImageMarginGradientBegin;
      }
   }
}
