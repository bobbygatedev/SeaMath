using Gate.ToolsView.ControlFeature;
using Gate.ToolsView.ControlFeature.Extensions;
using Gate.ToolsView.Dockable;
using Gate.ToolsView.Extended;
using Gate.ToolsView.MenuExtended;
using System.Windows.Forms;

namespace Gate.Dock
{
   public partial class GateDockSimpleForm : Form
   {


      public GateDockSimpleForm()
      {
         InitializeComponent();

         var fea = this.AddFeature<FormFeatureCustomCaptionResize>();

         fea.CustomCaptionControl = customCaptionCtrl1;
      }
   }
}
