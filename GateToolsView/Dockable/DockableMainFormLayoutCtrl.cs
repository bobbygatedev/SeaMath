using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.Extended;
using Gate.ToolsView.MenuCommand;
using System.Data;
using System.Windows.Forms.Layout;

namespace Gate.ToolsView.Dockable
{
   /// <summary>
   /// 
   /// </summary>
   public partial class DockableMainFormLayoutCtrl : UserControl
   {
      private int myControlSeparationPixels = 10;
      private int myBorderSidePixels = 10;
      private int myBorderBottomPixels = 7;
      private int myBorderTopPixels = 2;
      private int myMinimumClientHeight = 30;
      private Control? myCtrlCaption;
      private Control? myCtrlToolBarContainer;
      private Control? myCtrlClientArea = null;
      private Control? myCtrlFooter = null;

      public DockableMainFormLayoutCtrl()
      {
         InitializeComponent();

         PpCtrlCaption = new CustomCaptionCtrl();
         PpCtrlToolBarContainer = new CmdToolBarContainerCtrl();
      }

      private class InnerLayoutEngine : LayoutEngine
      {
         public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
         {
            var par = container as DockableMainFormLayoutCtrl ?? throw new Crash();

            var fix_ctr = par.Controls.Cast<Control>().Where(c => c != par.PpCtrlClientArea).ToArray();
            var min_cli_hei = Math.Max(par.PpMinimumClientHeight, par.PpCtrlClientArea != null ? par.PpCtrlClientArea.MinimumSize.Height : 0);
            var sep_h = par.PpControlSeparationPixels * (par.Controls.Count - 1);
            var min_h = par.PpBorderTopPixels + par.PpBorderBottomPixels + sep_h + fix_ctr.Select(c => c.Height).Sum() + min_cli_hei;

            par.MinimumSize = new Size(par.MinimumSize.Width, min_h);

            foreach (var ctr in par.Controls.Cast<Control>())
            {
               ctr.Left = par.PpBorderSidePixels;
               ctr.Width = par.Width - par.PpBorderSidePixels * 2;
            }

            foreach (var ct1 in fix_ctr) { ct1.Height = ct1.Height; }

            var top_cts = new[] { par.PpCtrlCaption, par.PpCtrlToolBarContainer }.Nn().ToArray();
            var y = par.PpBorderTopPixels;

            foreach (var ct1 in top_cts)
            {
               ct1.Top = y;
               y = ct1.Bottom + par.PpControlSeparationPixels;
            }

            if (par.PpCtrlClientArea != null)
            {
               par.PpCtrlClientArea.Top = y;
               par.PpCtrlClientArea.Height = 
                  par.Height - par.PpBorderBottomPixels - par.PpBorderTopPixels - sep_h - fix_ctr.Sum(c => c.Height);
            }

            if (par.PpCtrlFooter != null)
            {
               par.PpCtrlFooter.Top = par.Height - par.PpBorderBottomPixels - par.Height;
            }

            par.MinimumSize = new Size(par.MinimumSize.Width, min_h);

            if (par.ParentForm != null)
            {
               par.ParentForm.MinimumSize = par.MinimumSize;
            }

            return false;
         }
      }

      /// <summary>
      /// Separation pixels between controls.
      /// </summary>
      public int PpControlSeparationPixels
      {
         get => myControlSeparationPixels;
         set
         {
            myControlSeparationPixels = value;
            PerformLayout();
         }
      }

      /// <summary>
      /// Border width in pixels of left/right sides
      /// </summary>
      public int PpBorderSidePixels
      {
         get => myBorderSidePixels;
         set
         {
            myBorderSidePixels = value;
            PerformLayout();
         }
      }

      /// <summary>
      ///  border width in pixels of left/right sides
      /// </summary>
      public int PpBorderBottomPixels
      {
         get => myBorderBottomPixels;
         set
         {
            myBorderBottomPixels = value;
            PerformLayout();
         }
      }

      /// <summary>
      /// Border width in pixels of left/right sides
      /// </summary>
      public int PpBorderTopPixels
      {
         get => myBorderTopPixels;
         set
         {
            myBorderTopPixels = value;
            PerformLayout();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public int PpMinimumClientHeight
      {
         get => myMinimumClientHeight;
         set
         {
            myMinimumClientHeight = value;
            PerformLayout();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public Control? PpCtrlCaption
      {
         get => myCtrlCaption;
         set
         {
            if (myCtrlCaption != null) { Controls.Remove(myCtrlCaption); }

            if ((myCtrlCaption = value) != null) { Controls.Add(value); }

            PerformLayout();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public Control? PpCtrlToolBarContainer
      {
         get => myCtrlToolBarContainer;

         set
         {
            if (myCtrlToolBarContainer != null) { Controls.Remove(myCtrlToolBarContainer); }

            if ((myCtrlToolBarContainer = value) != null) { Controls.Add(value); }

            PerformLayout();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public Control? PpCtrlClientArea
      {
         get => myCtrlClientArea;

         set
         {
            if (myCtrlClientArea != null) { Controls.Remove(myCtrlClientArea); }

            if ((myCtrlClientArea = value) != null) { Controls.Add(value); }

            PerformLayout();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public Control? PpCtrlFooter
      {
         get => myCtrlFooter;
         set
         {
            if (myCtrlFooter != null) { Controls.Remove(myCtrlFooter); }

            if ((myCtrlFooter = value) != null) { Controls.Add(value); }

            PerformLayout();
         }
      }

      public override LayoutEngine LayoutEngine => new InnerLayoutEngine();
   }
}
