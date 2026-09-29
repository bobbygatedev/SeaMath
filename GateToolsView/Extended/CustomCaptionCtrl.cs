using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.ControlFeature;
using Gate.ToolsView.ControlFeature.Extensions;
using Gate.ToolsView.ControlObserve;
using Gate.ToolsView.Extensions;
using Gate.ToolsView.MenuCommand;
using Gate.ToolsView.Properties;
using System.ComponentModel;
using System.Data;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms.Layout;
using static Gate.ToolsView.ControlFeature.FormFeatureCustomCaptionResizeAndTrack;

namespace Gate.ToolsView.Extended
{
   /// <summary>
   /// A panel that replaces the standard window caption (title bar) of a form.
   /// It hosts an optional icon, a title text, the standard minimize / maximize-restore / close
   /// buttons and any other child control, which are laid out by a custom layout engine.
   /// When bound to a form (see <see cref="PpFormBound"/>, see also <see cref="PpIsFormAutobound"/> ) it also drives the form's window state
   /// and its move/resize tracking.
   /// </summary>
   public partial class CustomCaptionCtrl : Panel
   {
      /// <summary>
      /// Kinds of events raised by the caption control through <see cref="OnDockCaptionEvent"/>.
      /// </summary>
      public enum EventType
      {
         /// <summary>
         /// The minimize button was clicked.
         /// </summary>
         button_minimize = 0,

         /// <summary>
         /// The maximize (or restore) button was clicked.
         /// </summary>
         button_maximize,

         /// <summary>
         /// The close button was clicked.
         /// </summary>
         button_close,

         /// <summary>
         /// The bound form started to be moved by dragging the caption.
         /// </summary>
         start_tracking,

         /// <summary>
         /// The bound form stopped being moved.
         /// </summary>
         end_tracking
      }

      /// <summary>
      /// Handler for caption events (button clicks and tracking start/end).
      /// </summary>
      public delegate void OnDockCaptionEventHandler(object? sender, EventType eventType);

      /// <summary>
      /// Raised whenever a caption event occurs (after the default form-bound behaviour has been applied).
      /// </summary>
      public event OnDockCaptionEventHandler? OnDockCaptionEvent;

      // Default (built-in) buttons, used until the user replaces them with custom ones.
      private ExtendedButtonCtrl myButtonMinimizeStandard = myMakeStandardButton("Minimize");
      private ExtendedButtonCtrl myButtonMaximizeStandard = myMakeStandardButton("Maximize");
      private ExtendedButtonCtrl myButtonRestoreStandard = myMakeStandardButton("Restore");
      private ExtendedButtonCtrl myButtonCloseStandard = myMakeStandardButton("Close");

      // Buttons currently in use (either the default ones above or custom replacements).
      private Button? myButtonMinimize;
      private Button? myButtonMaximize;
      private Button? myButtonClose;
      private Button? myButtonRestore;

      // Index from which child controls are right-aligned (-1 = all left-aligned).
      private int myItemRightThreshold = -1;
      private readonly ControlObservableChildStyle myStyleObservable;

      private Image? myImage = null;
      private Form? myFormBound = null;
      private ControlObservableParent myObservableParent;
      private string myText = "";
      private bool myIsFormAutobound = false;

      /// <summary>
      /// Creates the control, wires the parent observer, loads the default button images
      /// and installs the default standard buttons.
      /// </summary>
      public CustomCaptionCtrl()
      {
         myStyleObservable = new ControlObservableChildStyle(this);

         // Observe the parent chain so the control can auto-bind/unbind to a form.
         myObservableParent = new ControlObservableParent(this);
         myObservableParent.OnControlRemoved += MyObservableParent_OnControlRemoved;
         myObservableParent.OnControlAdded += MyObservableParent_OnControlAdded;

         InitializeComponent();

         Controls.Add(CtrlPictureIcon);

         // Keep the icon box square, sized to the caption height.
         Resize += (s, e) => PpImageBox.Width = PpImageBox.Height = Height;

         // Load the default images (order matters: minimize, maximize, restore, close).
         CtrlImageList.Images.Clear();
         CtrlImageList.Images.Add(Resources.CtrlBtnMinimize_Image);
         CtrlImageList.Images.Add(Resources.CtrlBtnMaximizeImage);
         CtrlImageList.Images.Add(Resources.CtrlBtnRestoreImage);
         CtrlImageList.Images.Add(Resources.CtrlBtnClose_Image);

         // Assign the images from the image list to the default buttons.
         myButtonMinimizeStandard.Image = CtrlImageList.Images[0];
         myButtonMaximizeStandard.Image = CtrlImageList.Images[1];
         myButtonRestoreStandard.Image = CtrlImageList.Images[2];
         myButtonCloseStandard.Image = CtrlImageList.Images[3];

         // Repaint the whole control when it is resized (the caption text is drawn manually).
         SetStyle(ControlStyles.ResizeRedraw, true);

         // Suspend layout while the buttons are installed to avoid redundant layout passes.
         SuspendLayout();

         PpButtonMinimize = myButtonMinimizeStandard;
         PpButtonMaximize = myButtonMaximizeStandard;
         PpButtonRestore = myButtonRestoreStandard;
         PpButtonClose = myButtonCloseStandard;

         CtrlPictureIcon.BorderStyle = BorderStyle.None;
         ResumeLayout(true);
      }

      /// <summary>
      /// Custom layout engine. Places the standard buttons on the right edge,
      /// the icon and the "left" controls from the left edge, and the "right" controls
      /// (those at or beyond <see cref="PpItemRightThreshold"/>) just before the standard buttons.
      /// </summary>
      private class InnerLayoutEngine : LayoutEngine
      {
         public InnerLayoutEngine() { }

         public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
         {
            var par = container as CustomCaptionCtrl ?? throw new Crash();
            var pdr = par.DisplayRectangle;
            var loc = pdr.Location;

            // "Other" controls: visible children that are neither standard buttons nor the icon box.
            var cts = par.Controls.OfType<Control>().
               Where(c => c.Visible && !par.PpStandardButtons.Contains(c) && c != par.PpImageBox).
               ToArray();
            var x = pdr.Right;

            // (Leftover debug code: collects label texts but does not use them.)
            if (cts.OfType<Label>().Any())
            {
               var lbs = cts.OfType<Label>().ToArray();
               var arr = lbs.Select(lb => lb.Text).ToArray();
            }

            // Show "restore" only when the form is maximized...
            if (par.PpButtonRestore != null)
            {
               par.PpButtonRestore.Visible =
                  par.PpFormBound != null && par.PpFormBound.WindowState == FormWindowState.Maximized;
            }

            // ...and "maximize" only when it is not.
            if (par.PpButtonMaximize != null)
            {
               par.PpButtonMaximize.Visible =
                  par.PpFormBound != null && par.PpFormBound.WindowState != FormWindowState.Maximized;
            }

            // Place the standard buttons (minimize, maximize/restore, close) at the right edge,
            // iterating from the rightmost (close) to the leftmost. Each button is square.
            foreach (var ctr in par.PpStandardButtons.Reverse())
            {
               ctr.Width = ctr.Height = pdr.Height;
               ctr.Left = x - ctr.Width;
               ctr.Top = 0;
               x -= (ctr.Height + par.PpStandardButtonSeparation);
            }

            // Split the other controls into a left group and a right group according to the threshold.
            var lft_cts = null as Control[];
            var rgt_cts = null as Control[];

            if (par.PpItemRightThreshold < 0 || par.PpItemRightThreshold >= cts.Length)
            {
               // No right-aligned controls.
               lft_cts = cts;
               rgt_cts = [];
            }
            else
            {
               lft_cts = cts.Take(par.PpItemRightThreshold).ToArray();
               rgt_cts = cts.Skip(par.PpItemRightThreshold).ToArray();
            }

            // The icon, when it has an image, is always the first item on the left.
            if (par.PpImageBox.Visible = (par.PpImageBox.Image != null))
            {
               lft_cts = new Control[] { par.PpImageBox }.Concat(lft_cts).ToArray();
            }

            // Vertically center every control and clamp its height to the caption height.
            foreach (var ctr in cts)
            {
               ctr.Margin = new Padding(0);
               ctr.Height = Math.Min(ctr.Height, pdr.Height);
               ctr.Top = (par.Height - ctr.Height) / 2;
            }

            // Lay out the left group from left to right.
            x = 0;

            foreach (var ctr in lft_cts)
            {
               ctr.Left = x;
               x += ctr.Width + par.PpItemSeparation;
            }

            // Lay out the right group from right to left, starting just before the standard buttons
            // (or at the control's right edge if there are none).
            x = -par.PpStandardButtonSeparation +
               (par.PpStandardButtons.Count() > 0 ? par.PpStandardButtons.First().Left : par.Right);

            foreach (var ctr in rgt_cts.Reverse())
            {
               ctr.Left = x - ctr.Width;
               x -= ctr.Width + par.PpItemSeparation;
            }

            // Emulate DockStyle.Fill for a single left control: stretch it up to the first right
            // control (or up to the end of the caption if there is none).
            if (lft_cts.Length == 1 && lft_cts[0].Dock == DockStyle.Fill)
            {
               if (rgt_cts.Length > 0)
               {
                  lft_cts[0].Width = rgt_cts[0].Left - par.PpItemSeparation - lft_cts[0].Left;
               }
               else
               {
                  lft_cts[0].Width = par.Width - par.PpItemSeparation - lft_cts[0].Left;
               }
            }

            // false = the parent does not need to perform another layout pass.
            return false;
         }
      }

      /// <summary>
      /// Color treated as transparent in the images of the internal image list (used by the default buttons).
      /// </summary>
      public Color PpButtonTransparentColor { get => CtrlImageList.TransparentColor; set => CtrlImageList.TransparentColor = value; }

      /// <summary>
      /// The minimize button. Setting it detaches the previous button (unsubscribing its click handler
      /// and removing it from the controls) and attaches the new one.
      /// </summary>
      public Button? PpButtonMinimize
      {
         get => myButtonMinimize;
         set
         {
            if (myButtonMinimize != null)
            {
               myButtonMinimize.Click -= MyButtonMinimize_Click;
               Controls.Remove(myButtonMinimize);
            }

            if ((myButtonMinimize = value) != null)
            {
               myDoStandardButtonRefresh(myButtonMinimize);
               myButtonMinimize.Click += MyButtonMinimize_Click;
            }
         }
      }

      /// <summary>
      /// The restore button (shown instead of maximize when the form is maximized).
      /// </summary>
      public Button? PpButtonRestore
      {
         get => myButtonRestore;

         set
         {
            if (myButtonRestore != value)
            {
               if (myButtonRestore != null)
               {
                  myButtonRestore.Click -= MyButtonRestore_Click;
                  Controls.Remove(myButtonRestore);
               }

               if ((myButtonRestore = value) != null)
               {
                  myButtonRestore.Click += MyButtonRestore_Click;
               }

               myDoStandardButtonRefresh(myButtonRestore);
            }
         }
      }

      /// <summary>
      /// The maximize button (shown when the form is not maximized).
      /// </summary>
      public Button? PpButtonMaximize
      {
         get => myButtonMaximize;

         set
         {
            if (myButtonMaximize != value)
            {
               if (myButtonMaximize != null)
               {
                  myButtonMaximize.Click -= MyButtonMaximize_Click;
                  Controls.Remove(myButtonMaximize);
               }

               if ((myButtonMaximize = value) != null)
               {
                  myButtonMaximize.Click += MyButtonMaximize_Click;
               }

               myDoStandardButtonRefresh(myButtonMaximize);
            }
         }
      }

      /// <summary>
      /// The close button.
      /// </summary>
      public Button? PpButtonClose
      {
         get => myButtonClose;

         set
         {
            if (myButtonClose != null)
            {
               myButtonClose.Click -= MyButtonClose_Click;
               Controls.Remove(myButtonClose);
            }

            if ((myButtonClose = value) != null)
            {
               myDoStandardButtonRefresh(myButtonClose);
               myButtonClose.Click += MyButtonClose_Click;
            }
         }
      }

      /// <summary>
      /// If true, the control automatically binds to the parent form when added to a form by calling <see cref="PpFormBound"/>.
      /// </summary>
      public bool PpIsFormAutobound
      {
         get => myIsFormAutobound;

         set
         {
            if (myIsFormAutobound != value)
            {
               // When enabling, bind immediately to the form that currently contains the control.
               if (myIsFormAutobound = value)
               {
                  PpFormBound = this.MthGetParentForm();
               }
            }
         }
      }

      /// <summary>
      /// Distance in pixel from a control to another.
      /// </summary>
      public int PpItemSeparation { get; set; } = 5;

      /// <summary>
      /// Distance in pixel from a standard button (minimize, maximize, close) to another.
      /// </summary>
      public int PpStandardButtonSeparation { get; set; } = 5;

      /// <summary>
      /// The standard buttons currently active, in left-to-right order:
      /// minimize, then maximize or restore (depending on the bound form's window state), then close.
      /// Null entries (unset buttons) are filtered out. Without a bound form, no maximize/restore button is returned.
      /// </summary>
      public Button[] PpStandardButtons
      {
         get
         {
            var max_but = PpFormBound != null ?
               (PpFormBound.WindowState == FormWindowState.Maximized ? PpButtonRestore : PpButtonMaximize) :
               null;

            return new Button?[] { PpButtonMinimize, max_but, PpButtonClose }.Nn().Cast<Button>().ToArray();
         }
      }

      /// <summary>
      /// Right threshold ( item with sub-control id >= of it are placed at right)
      /// </summary>
      public int PpItemRightThreshold
      {
         get => myItemRightThreshold;
         set
         {
            myItemRightThreshold = value;
            PerformLayout();
         }
      }

      /// <summary>
      /// Form bound to the control. 
      /// If set, the control will automatically handle minimize, maximize, restore and close events for the form.
      /// Furthermore, the control will also handle tracking form tracking.
      /// </summary>
      [Browsable(false)]
      public Form? PpFormBound
      {
         get => myFormBound;

         set
         {
            if (value != myFormBound)
            {
               // Unbind from the previous form: detach this control from its feature and unsubscribe.
               if (myFormBound != null)
               {
                  var fea = myFormBound.GetFeature<FormFeatureCustomCaptionResizeAndTrack>();

                  if (fea != null)
                  {
                     fea.CustomCaptionControl = null;
                     fea.OnStateChanged -= MyExtendedFormBound_OnStateChange;
                  }
               }

               // Bind to the new form: reuse its resize/track feature or add it if missing.
               if ((myFormBound = value) != null)
               {
                  var fea = myFormBound.
                     GetFeature<FormFeatureCustomCaptionResizeAndTrack>() ?? myFormBound.AddFeature<FormFeatureCustomCaptionResizeAndTrack>();

                  fea.CustomCaptionControl = this;
                  fea.OnStateChanged += MyExtendedFormBound_OnStateChange;
                  myFormBound.Refresh();
                  myFormBound.PerformLayout();
               }
            }
         }
      }

      /// <summary>
      /// Icon shown at the left of the caption. The image is resized to the current size of the icon box.
      /// Setting null hides the icon.
      /// </summary>
      public Image? PpImage
      {
         get => myImage;
         set
         {
            PpImageBox.Visible = value != null;

            if (value != null)
            {
               PpImageBox.Image = myImage = myResizeImage(value, PpImageBox.Width, PpImageBox.Height);
            }
            else
            {
               myImage = null;
            }
         }
      }

      /// <summary>
      /// The picture box that hosts the caption icon.
      /// </summary>
      public PictureBox PpImageBox => CtrlPictureIcon;

      /// <summary>
      /// Caption text, drawn manually in <see cref="OnPaint"/>. Setting it forces a repaint.
      /// </summary>
      public string PpText
      {
         get => myText;

         set
         {
            myText = value.Nn();
            Refresh();
         }
      }

      /// <summary>
      /// Replaces the default layout engine with <see cref="InnerLayoutEngine"/>.
      /// </summary>
      public override LayoutEngine LayoutEngine => new InnerLayoutEngine();

      /// <summary>
      /// Central handler for caption events: applies the default behaviour on the bound form
      /// and then notifies external subscribers.
      /// </summary>
      protected virtual void myActionOnDockCaptionEvent(object? sender, EventType eventType)
      {
         myDoFormBoundBehaviour(eventType, PpFormBound);
         OnDockCaptionEvent?.Invoke(sender, eventType);
      }

      /// <summary>
      /// Draws the caption text at the top-left corner of the control.
      /// </summary>
      protected override void OnPaint(PaintEventArgs e)
      {
         base.OnPaint(e);

         using (var gr = e.Graphics)
         {
            using (var br = new SolidBrush(ForeColor))
            {
               e.Graphics.DrawString(PpText, Font, br, new PointF());
            }
         }
      }

      /// <summary>
      /// Applies the default action for the given event to the bound form (if any).
      /// </summary>
      private void myDoFormBoundBehaviour(EventType eventType, Form? formBound)
      {
         if (formBound != null)
         {
            switch (eventType)
            {
               case EventType.button_minimize:
                  formBound.WindowState = FormWindowState.Minimized;
                  break;

               case EventType.button_maximize:
                  // Toggle between maximized and normal; relayout to swap maximize/restore buttons.
                  formBound.WindowState =
                     formBound.WindowState == FormWindowState.Maximized ?
                        FormWindowState.Normal : FormWindowState.Maximized;
                  PerformLayout();
                  break;

               case EventType.button_close:
                  formBound.Close();
                  break;

               case EventType.start_tracking:
               case EventType.end_tracking:
                  break;//nothing

               default: throw new Crash();
            }
         }
      }

      /// <summary>
      /// Shows the system (window) menu of the parent form at the mouse position.
      /// </summary>
      private void myDoShowSysMenu(MouseEventArgs e)
      {
         var par = this.MthGetParentForm();

         if (par != null)
         {
            var sys = new SystemMenuHelper(par);
            var abs_pnt = PointToScreen(new Point(e.X, e.Y));

            sys.Show(abs_pnt.X, abs_pnt.Y);
         }
      }

      /// <summary>
      /// Adds the given button to the controls and reorders the standard buttons so that they
      /// are always the last children (after all the other controls), keeping their relative order.
      /// </summary>
      private void myDoStandardButtonRefresh(Button? button)
      {
         var std_btn = PpStandardButtons;
         var num_ctr_oth = Controls.OfType<Control>().Count(c => !std_btn.Contains(c));

         if (button != null) { Controls.Add(button); }

         for (var i = 0; i < std_btn.Length; i++) { Controls.SetChildIndex(std_btn[i], num_ctr_oth + i); }
      }

      /// <summary>
      /// Creates a default caption button with the given name.
      /// </summary>
      private static ExtendedButtonCtrl myMakeStandardButton(string name)
      {
         var but = new ExtendedButtonCtrl();

         but.Name = name;

         return but;
      }

      /// <summary>
      /// Returns a copy of <paramref name="image"/> scaled to the given size, using high-quality
      /// interpolation and tile-flip wrapping to avoid edge artifacts.
      /// </summary>
      private static Bitmap myResizeImage(Image image, int width, int height)
      {
         var dts_rc = new Rectangle(0, 0, width, height);
         var dst_img = new Bitmap(width, height);

         dst_img.SetResolution(image.HorizontalResolution, image.VerticalResolution);

         using (var gr = Graphics.FromImage(dst_img))
         {
            gr.CompositingMode = CompositingMode.SourceCopy;
            gr.CompositingQuality = CompositingQuality.HighQuality;
            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gr.SmoothingMode = SmoothingMode.HighQuality;
            gr.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (var ia = new ImageAttributes())
            {
               ia.SetWrapMode(WrapMode.TileFlipXY);
               gr.DrawImage(image, dts_rc, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, ia);
            }
         }

         return dst_img;
      }

      /// <summary>
      /// Click handler of the minimize button.
      /// </summary>
      private void MyButtonMinimize_Click(object? sender, EventArgs e) =>
         myActionOnDockCaptionEvent(sender, EventType.button_minimize);

      /// <summary>
      /// Click handler of the restore button (restore is handled as a maximize toggle).
      /// </summary>
      private void MyButtonRestore_Click(object? sender, EventArgs e) =>
         myActionOnDockCaptionEvent(sender, EventType.button_maximize);

      /// <summary>
      /// Click handler of the maximize button.
      /// </summary>
      /// <param name="sender">The button that raised the event.</param>
      /// <param name="e">Event data.</param>
      private void MyButtonMaximize_Click(object? sender, EventArgs e) =>
         myActionOnDockCaptionEvent(sender, EventType.button_maximize);

      /// <summary>
      /// Click handler of the close button.
      /// </summary>
      private void MyButtonClose_Click(object? sender, EventArgs e) =>
         myActionOnDockCaptionEvent(sender, EventType.button_close);

      /// <summary>
      /// Shows the system menu when the icon is clicked.
      /// </summary>
      private void CtrlPictureIcon_MouseDown(object? sender, MouseEventArgs e) => myDoShowSysMenu(e);

      /// <summary>
      /// Shows the system menu when the caption is right-clicked.
      /// </summary>
      private void ExtendedCaptionCtrl_MouseDown(object? sender, MouseEventArgs e)
      {
         if (e.Button == MouseButtons.Right) { myDoShowSysMenu(e); }
      }

      /// <summary>
      /// Translates the bound form's track state changes (moving/normal/resize) into
      /// start/end tracking caption events.
      /// </summary>
      private void MyExtendedFormBound_OnStateChange(object? sender, StateChangedArgs args)
      {
         switch (args.NewState)
         {
            case TrackState.normal:
               // Going back to normal after moving means the move has ended.
               if (args.OldState == TrackState.moving)
               {
                  myActionOnDockCaptionEvent(sender, EventType.end_tracking);
               }
               break;

            case TrackState.moving:
               myActionOnDockCaptionEvent(sender, EventType.start_tracking);
               break;

            case TrackState.resize: break; // resizing raises no caption event

            default: throw new Crash();
         }
      }

      /// <summary>
      /// Unbinds the form when it is removed from the observed parent chain.
      /// </summary>
      private void MyObservableParent_OnControlRemoved(Control control)
      {
         if (control == PpFormBound) { PpFormBound = null; }
      }

      /// <summary>
      /// When auto-bind is enabled, binds to a form as soon as it is added to the observed parent chain.
      /// </summary>
      private void MyObservableParent_OnControlAdded(Control control)
      {
         if (PpIsFormAutobound && control is Form frm)
         {
            PpFormBound = frm;
         }
      }
   }
}