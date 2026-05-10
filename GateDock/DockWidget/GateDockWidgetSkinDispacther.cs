using Gate.Dock.DockSkin;
using Gate.ToolsView.Extended;
using System.Data;
using System.Reflection;

namespace Gate.Dock.DockWidget
{
   /// <summary>
   /// 
   /// </summary>
   public class GateDockWidgetSkinDispacther : GateDockSkinChildCtrlDispatcher
   {
      private UpdateViUpdateWidgetControlsVisitor myUpdateWidgetControlsVisitor = new UpdateViUpdateWidgetControlsVisitor();

      public GateDockWidgetSkinDispacther(GateDockWidgetCtrl parent) : base(parent) { }

      protected class UpdateViUpdateWidgetControlsVisitor
      {
         public virtual void Visit(GateDockSkin skin, Control control)
         {
            //it's considered as a content
            control.BackColor = skin.Params.BackWidgetColor.Value;
            control.ForeColor = skin.Params.ForeColor.Value;
            control.Font = skin.Params.ControlsFont.Value;
         }

         public virtual void Visit(GateDockSkin skin, ExtendedScrollBar scrollBar)
         {
            scrollBar.ForeColor = skin.Params.ForeColor.Value;
            scrollBar.BackColor = skin.Params.ScrollBarBackColor.Value;
            scrollBar.PpArrowColor = skin.Params.ScrollBarArrowColor.Value;
            scrollBar.PpGripColor = skin.Params.ScrollBarGripColor.Value;
            scrollBar.PpGripActiveColor = skin.Params.ScrollBarpGripActiveColor.Value;

            if (scrollBar.Orientation == ExtendedScrollBarOrientationEnum.Horizontal)
            {
               scrollBar.Height = GateDockSkin.DefaultValues.ScrollBarWidgetSize;
            }
            else
            {
               scrollBar.Width = GateDockSkin.DefaultValues.ScrollBarWidgetSize;
            }
         }
      }

      private static FieldInfo[] myGateDockWidgetFields =
         typeof(GateDockWidgetCtrl).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).
         Where(f => f.FieldType.IsSubclassOf(typeof(Control))).ToArray();

      protected sealed override void myUpdate(GateDockSkin skin, Control control)
      {
         var wdg = RootControl as GateDockWidgetCtrl;
         //is a control inserted in UserControl designer of 'GateDockWidgetCtrl'.
         var is_in_dsn_dcl = myGateDockWidgetFields.Any(f => f.GetValue(wdg) == control);

         if (is_in_dsn_dcl) { base.myUpdate(skin, control); }
         else { myUpdateWidgetControls(skin, control); }
      }

      protected virtual void myUpdateWidgetControls(GateDockSkin skin, Control control) =>
         myUpdateWidgetControlsVisitor.Visit(skin, (dynamic)control);
   }
}
