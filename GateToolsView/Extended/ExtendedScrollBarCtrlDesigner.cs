using Gate.Tools.Extensions;
using System.ComponentModel;
using System.Windows.Forms.Design;

namespace Gate.ToolsView.Extended
{
   /// <summary>
   /// The designer for the <see cref="ExtendedScrollBar"/> control.
   /// </summary>
   internal class ExtendedScrollBarCtrlDesigner : ControlDesigner
   {
      /// <summary>
      /// <see cref="SelectionRules"/> for the control.
      /// </summary>
      public override SelectionRules SelectionRules
      {
         get
         {
            // gets the property descriptor for the property "Orientation"
            var pro_dsc = TypeDescriptor.GetProperties(Component)["Orientation"];

            // if not null - we can read the current orientation of the scroll bar
            if (pro_dsc != null)
            {
               // get the current orientation
               var scr_ori = pro_dsc.GetValue(Component).ConvertOrCrash<ExtendedScrollBarOrientationEnum>();

               // if vertical orientation
               return scr_ori == ExtendedScrollBarOrientationEnum.Vertical ?
                  SelectionRules.Visible | SelectionRules.Moveable | SelectionRules.AllSizeable :
                  SelectionRules.Visible | SelectionRules.Moveable | SelectionRules.AllSizeable;
            }

            return base.SelectionRules;
         }
      }

      /// <summary>
      /// Prefilters the properties so that unnecessary properties are hidden
      /// in the property browser of Visual Studio.
      /// </summary>
      /// <param name="properties">The property dictionary.</param>
      protected override void PreFilterProperties(System.Collections.IDictionary properties)
      {
         properties.Remove("Text");
         properties.Remove("BackgroundImage");
         properties.Remove("ForeColor");
         properties.Remove("ImeMode");
         properties.Remove("Padding");
         properties.Remove("BackgroundImageLayout");
         properties.Remove("Font");
         properties.Remove("RightToLeft");

         base.PreFilterProperties(properties);
      }
   }
}
