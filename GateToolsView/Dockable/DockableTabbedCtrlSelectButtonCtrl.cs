using Gate.Tools.Extensions;
using Gate.ToolsView.ControlFeature;
using Gate.ToolsView.ControlFeature.Extensions;
using Gate.ToolsView.MenuExtended;
using System.Windows.Forms.Layout;
using static Gate.ToolsView.ControlFeature.CtrlFeatureTrackStartSense;

namespace Gate.ToolsView.Dockable
{
   /// <summary>
   /// <br> Control representing a selectable tab-like button that optionally shows a close button
   /// and may host a context menu. </br>
   /// <br> The control exposes events for selection, close requests,
   /// and the start of a drag operation.</br>
   /// </summary>
   /// <remarks>
   /// <br>Pseudocode / Implementation plan:</br>
   /// <br>1) Constructor:</br>
   ///    <br>- Initialize components.</br>
   ///    <br>- Add images to the internal image list (minimize, maximize, close).</br>
   ///    <br>- Add feature CtrlFeatureToolTip to the select button.</br>
   ///    <br>- Add feature CtrlFeatureTrackStartSense to the select button and subscribe its
   ///      OnStartingDragging to forward to this control's OnAskForDragging event.</br>
   ///    <br>- Enable ControlStyles.ResizeRedraw and call PerformLayout() to compute initial layout.</br>
   ///
   /// <br>2) Layout (InnerLayoutEngine.Layout):</br>
   ///    <br>- Cast container to this control.</br>
   ///    <br>- If PpHasCloseButton is true, show the close button, set its width/height to control height,
   ///      position it on the right, and reserve its width for layout.</br>
   ///    <br>- Set the select button height to the control height and width to remaining width.</br>
   ///    <br>- Update MinimumSize to PreferredSize so parent layouts can query correctly.</br>
   ///
   /// <br>3) Properties:</br>
   ///    <br>- PpDraggingDelaySeconds: forward to CtrlFeatureTrackStartSense.DraggingDelaySeconds (null-safe).</br>
   ///    <br>- PpText: maps to CtrlSelectButton.Text and triggers PerformLayout().</br>
   ///    <br>- PpHasCloseButton: simple backing field toggle that affects layout when changed.</br>
   ///    <br>- PpGuardWidth: number of pixels before/after text; triggers PerformLayout() when changed.</br>
   ///    <br>- LayoutEngine: returns a new InnerLayoutEngine instance for custom layout logic.</br>
   ///    <br>- PpToolTipText: forwards to CtrlFeatureToolTip.ToolTipText (throws if missing).</br>
   ///    <br>- PpMenuDropDown: sets the select button's ContextMenuStrip and stores reference.</br>
   ///
   /// <br>4) GetPreferredSize:</br>
   ///    <br>- Measure the select button text using a temporary Graphics object.</br>
   ///    <br>- Compute height as max of measured text height and a minimum.</br>
   ///    <br>- Compute width as height (for possible icon/spacing) + measured text width + guard padding.</br>
   ///
   /// <br>5) Events:</br>
   ///    <br>- CtrlSelectButton_MouseDown: on left button, invoke OnAskForSelect.</br>
   ///    <br>- CtrlAskForClose_Click: invoke OnAskForClose.</br>
   ///
   /// <br>6) Styling:
   ///    - OnBackColorChanged / OnForeColorChanged: propagate colors to child controls (select and close button).</br>
   ///
   /// <br>Notes:</br>
   /// <br>- Use null-conditional / null-coalescing operators where appropriate and throw Crash() when a required feature is unexpectedly missing.</br>
   /// <br>- Keep layout calculations minimal and avoid allocating heavy resources in Layout; measuring occurs in GetPreferredSize.</br>
   /// </remarks>
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
         CtrlSelectButton.AddFeature<CtrlFeatureTrackStartSense>().OnStartingDragging += 
            (_) => OnAskForDragging?.Invoke(this);
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
      /// Timeout for start dragging (from button down).
      /// </summary>
      public double PpDraggingDelaySeconds
      {
         get => this.GetFeature<CtrlFeatureTrackStartSense>()?.DraggingDelaySeconds ?? -1.0; 
         set => this.GetFeature<CtrlFeatureTrackStartSense>().NnOrCrash().DraggingDelaySeconds = value;
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
      /// Num pixels before and after text.
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

         set => (CtrlSelectButton.GetFeature<CtrlFeatureToolTip>().NnOrCrash()).ToolTipText = value;
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

