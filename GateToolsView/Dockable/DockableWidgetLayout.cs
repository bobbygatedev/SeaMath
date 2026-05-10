using System.Windows.Forms.Layout;

namespace Gate.ToolsView.Dockable
{
   /// <summary>
   /// 
   /// </summary>
   public partial class DockableWidgetLayout : UserControl
   {
      private Control? myCaptionControl = null;
      private Control? myCtrlBody = null;

      public DockableWidgetLayout() { }

      private class InnerLayoutEngine : LayoutEngine
      {
         public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
         {
            var par = (DockableWidgetLayout)container;
            var y = 0;

            par.MinimumSize = new Size(par.MinimumSize.Width, 10);

            if (par.PpCaptionControl != null && par.PpCaptionControl.Visible)
            {
               y = par.PpCaptionControl.Height;
               par.PpCaptionControl.Top = 0;
               par.PpCaptionControl.Left = 0;
               par.PpCaptionControl.Width = par.Width;
               par.MinimumSize = new Size(par.MinimumSize.Width, 10 + y);
            }

            par.Height = Math.Max(par.Height, par.MinimumSize.Height);

            if (par.PpBody != null)
            {
               par.PpBody.Top = y;
               par.PpBody.Left = 0;
               par.PpBody.Width = par.Width;
               par.PpBody.Height = par.Height - y;
            }

            return false;
         }
      }

      public Control? PpCaptionControl
      {
         get => myCaptionControl;
         set
         {
            if ((myCaptionControl = value) != null)
            {
               if (myCaptionControl.Parent != this)
               {
                  myCaptionControl.Parent = this;
                  PerformLayout();
               }
            }
         }
      }

      public Control? PpBody
      {
         get => myCtrlBody;

         set
         {
            if ((myCtrlBody = value) != null)
            {
               if (myCtrlBody.Parent != this)
               {
                  myCtrlBody.Parent = this;
                  PerformLayout();
               }
            }
         }
      }

      public override LayoutEngine LayoutEngine => new InnerLayoutEngine();
   }
}
