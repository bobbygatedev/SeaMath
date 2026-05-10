using Gate.Tools;
using Gate.ToolsView.ControlFeature;
using Gate.ToolsView.ControlFeature.Extensions;
using Gate.ToolsView.MenuExtended;
using System.Windows.Forms.Layout;
using static Gate.ToolsView.ControlFeature.CtrlFeatureTrackStartSense;

namespace Gate.ToolsView.Dockable
{
   /// <summary>
   /// 
   /// </summary>
   public partial class DockableTabbedCtrlSelectButtonCtrl : UserControl
   {
      private bool myHasCloseButton = false;

      public delegate void OnAskForCloseHandler(object? sender);
      public delegate void OnAskForSelectHandler(object? sender);

      public event OnStartingDraggingHandler? OnAskForDragging;
      public event OnAskForCloseHandler? OnAskForClose;
      public event OnAskForSelectHandler? OnAskForSelect;

      private int myGuardWidth = 10;
      private ExtendedMenuDropDown? myMenuDropDown = null;

      public DockableTabbedCtrlSelectButtonCtrl()
      {
         InitializeComponent();

         CtrlImageList.Images.Add(Properties.Resources.CtrlBtnMinimize_Image);
         CtrlImageList.Images.Add(Properties.Resources.CtrlBtnMaximizeImage);
         CtrlImageList.Images.Add(Properties.Resources.CtrlBtnClose_Image);

         //for tool tip
         CtrlSelectButton.AddFeature<CtrlFeatureToolTip>();
         CtrlSelectButton.AddFeature<CtrlFeatureTrackStartSense>().OnStartingDragging += (_) => OnAskForDragging?.Invoke(this);
         SetStyle(ControlStyles.ResizeRedraw, true);
         PerformLayout();
      }

      private class InnerLayoutEngine : LayoutEngine
      {
         public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
         {
            var par = (DockableTabbedCtrlSelectButtonCtrl)container;
            var clo_w = 0;

            if (par.CtrlAskForClose.Visible = par.PpHasCloseButton)
            {
               clo_w = par.CtrlAskForClose.Width = par.CtrlAskForClose.Height = par.Height;
               par.CtrlAskForClose.Location = new Point(par.Width - par.Height, 0);
               par.CtrlSelectButton.Height = par.Height;
            }

            par.MinimumSize = par.PreferredSize;
            par.CtrlSelectButton.Width = par.Width - clo_w;
            par.CtrlSelectButton.Location = new Point(0, 0);

            return false;
         }
      }

      /// <summary>
      ///  the timeout for start dragging (from button down).
      /// </summary>
      public double PpDraggingDelaySeconds
      {
         get => this.GetFeature<CtrlFeatureTrackStartSense>()?.DraggingDelaySeconds ?? -1.0;
         set
         {
            (this.GetFeature<CtrlFeatureTrackStartSense>() ?? throw new Crash()).DraggingDelaySeconds = value;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public string? PpText
      {
         get => CtrlSelectButton.Text;

         set
         {
            CtrlSelectButton.Text = value;
            PerformLayout();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public bool PpHasCloseButton
      {
         get => myHasCloseButton;

         set => myHasCloseButton = value;
      }

      /// <summary>
      ///  num pixels before and after text.
      /// </summary>
      public int PpGuardWidth
      {
         get => myGuardWidth;
         set
         {
            myGuardWidth = value;
            PerformLayout();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override LayoutEngine LayoutEngine => new InnerLayoutEngine();

      /// <summary>
      /// Tooltip text associated to select button.
      /// </summary>
      public string? PpToolTipText
      {
         get => CtrlSelectButton.GetFeature<CtrlFeatureToolTip>()?.ToolTipText;

         set
         {
            (CtrlSelectButton.GetFeature<CtrlFeatureToolTip>() ?? throw new Crash()).ToolTipText = value;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public ExtendedMenuDropDown? PpMenuDropDown
      {
         get => myMenuDropDown;

         set => CtrlSelectButton.ContextMenuStrip = myMenuDropDown = value;
      }

      public override Size GetPreferredSize(Size proposedSize)
      {
         var h = 10;
         var w = 0;

         using (var gr = Graphics.FromImage(new Bitmap(1, 1)))
         {
            var sz = gr.MeasureString(CtrlSelectButton.Text, CtrlSelectButton.Font);

            h = Math.Max(h, (int)Math.Ceiling(sz.Height));
            w = h + (int)Math.Ceiling(sz.Width) + PpGuardWidth * 2;
         }

         return new Size(w, h);
      }

      private void CtrlSelectButton_MouseDown(object? sender, MouseEventArgs e)
      {
         if (e.Button == MouseButtons.Left && OnAskForSelect != null) { OnAskForSelect.Invoke(this); }
      }

      private void CtrlAskForClose_Click(object? sender, EventArgs e) => OnAskForClose?.Invoke(this);

      protected override void OnBackColorChanged(EventArgs e)
      {
         base.OnBackColorChanged(e);

         CtrlSelectButton.BackColor = CtrlAskForClose.BackColor = BackColor;
      }

      protected override void OnForeColorChanged(EventArgs e)
      {
         base.OnForeColorChanged(e);

         CtrlSelectButton.ForeColor = CtrlAskForClose.ForeColor = ForeColor;
      }
   }
}

