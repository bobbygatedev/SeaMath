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
using static Gate.ToolsView.ControlFeature.FormFeatureCustomCaptionResize;

namespace Gate.ToolsView.Extended
{
   /// <summary>
   /// 
   /// </summary>
   public partial class CustomCaptionCtrl : Panel
   {
      /// <summary>
      /// 
      /// </summary>
      public enum EventType
      {
         /// <summary>
         /// 
         /// </summary>
         button_minimize = 0,

         /// <summary>
         /// 
         /// </summary>
         button_maximize,

         /// <summary>
         /// 
         /// </summary>
         button_close,

         /// <summary>
         /// 
         /// </summary>
         start_tracking,

         /// <summary>
         /// 
         /// </summary>
         end_tracking
      }

      public delegate void OnDockCaptionEventHandler(object? sender, EventType eventType);

      public event OnDockCaptionEventHandler? OnDockCaptionEvent;

      private ExtendedButtonCtrl myButtonMinimizeStandard = myMakeStandardButton("Minimize");
      private ExtendedButtonCtrl myButtonMaximizeStandard = myMakeStandardButton("Maximize");
      private ExtendedButtonCtrl myButtonRestoreStandard = myMakeStandardButton("Restore");
      private ExtendedButtonCtrl myButtonCloseStandard = myMakeStandardButton("Close");

      private Button? myButtonMinimize;
      private Button? myButtonMaximize;
      private Button? myButtonClose;
      private Button? myButtonRestore;
      private int myItemRightThreshold = -1;
      private readonly ControlObservableChildStyle myStyleObservable;

      private Image? myImage = null;
      private Form? myFormBound = null;
      private ControlObservableParent myObservableParent;

      public CustomCaptionCtrl()
      {
         myStyleObservable = new ControlObservableChildStyle(this);
         myObservableParent = new ControlObservableParent(this);
         myObservableParent.OnControlRemoved += MyObservableParent_OnControlRemoved;

         InitializeComponent();

         Controls.Add(CtrlPictureIcon);
         Resize += (s, e) => PpImageBox.Width = PpImageBox.Height = Height;

         CtrlImageList.Images.Clear();
         CtrlImageList.Images.Add(Resources.CtrlBtnMinimize_Image);
         CtrlImageList.Images.Add(Resources.CtrlBtnMaximizeImage);
         CtrlImageList.Images.Add(Resources.CtrlBtnRestoreImage);
         CtrlImageList.Images.Add(Resources.CtrlBtnClose_Image);

         //image list is used in
         myButtonMinimizeStandard.Image = CtrlImageList.Images[0];
         myButtonMaximizeStandard.Image = CtrlImageList.Images[1];
         myButtonRestoreStandard.Image = CtrlImageList.Images[2];
         myButtonCloseStandard.Image = CtrlImageList.Images[3];

         SetStyle(ControlStyles.ResizeRedraw, true);

         SuspendLayout();

         PpButtonMinimize = myButtonMinimizeStandard;
         PpButtonMaximize = myButtonMaximizeStandard;
         PpButtonRestore = myButtonRestoreStandard;
         PpButtonClose = myButtonCloseStandard;

         CtrlPictureIcon.BorderStyle = BorderStyle.None;
         ResumeLayout(true);
      }

      private class InnerLayoutEngine : LayoutEngine
      {
         public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
         {
            var par = container as CustomCaptionCtrl ?? throw new Crash();
            var pdr = par.DisplayRectangle;
            var loc = pdr.Location;
            var cts = par.Controls.OfType<Control>().
               Where(c => c.Visible && !par.PpStandardButtons.Contains(c) && c != par.PpImageBox).
               ToArray();
            var x = pdr.Right;

            if (cts.OfType<Label>().Any())
            {
               var lbs = cts.OfType<Label>().ToArray();
               var arr = lbs.Select(lb => lb.Text).ToArray();
            }

            if (par.PpButtonRestore != null)
            {
               par.PpButtonRestore.Visible = par.PpFormBound != null && par.PpFormBound.WindowState == FormWindowState.Maximized;
            }

            if (par.PpButtonMaximize != null)
            {
               par.PpButtonMaximize.Visible = par.PpFormBound != null && par.PpFormBound.WindowState != FormWindowState.Maximized;
            }

            //places standard buttons (close,min/maximize) at right
            foreach (var ctr in par.PpStandardButtons.Reverse())
            {
               ctr.Width = ctr.Height = pdr.Height;
               ctr.Left = x - ctr.Width;
               ctr.Top = 0;
               x -= (ctr.Height + par.PpStandardButtonSeparation);
            }

            var lft_cts = null as Control[];
            var rgt_cts = null as Control[];

            if (par.PpItemRightThreshold < 0 || par.PpItemRightThreshold >= cts.Length)
            {
               lft_cts = cts;
               rgt_cts = [];
            }
            else
            {
               lft_cts = cts.Take(par.PpItemRightThreshold).ToArray();
               rgt_cts = cts.Skip(par.PpItemRightThreshold).ToArray();
            }

            if (par.PpImageBox.Visible = (par.PpImageBox.Image != null))
            {
               lft_cts = new Control[] { par.PpImageBox }.Concat(lft_cts).ToArray();
            }

            foreach (var ctr in cts)
            {
               ctr.Margin = new Padding(0);
               ctr.Height = Math.Min(ctr.Height, pdr.Height);
               ctr.Top = (par.Height - ctr.Height) / 2;
            }

            x = 0;

            foreach (var ctr in lft_cts)
            {
               ctr.Left = x;
               x += ctr.Width + par.PpItemSeparation;
            }

            x = -par.PpStandardButtonSeparation +
               (par.PpStandardButtons.Count() > 0 ? par.PpStandardButtons.First().Left : par.Right);

            foreach (var ctr in rgt_cts.Reverse())
            {
               ctr.Left = x - ctr.Width;
               x -= ctr.Width + par.PpItemSeparation;
            }

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

            return false;
         }
      }

      public Color PpButtonTransparentColor { get => CtrlImageList.TransparentColor; set => CtrlImageList.TransparentColor = value; }

      public Button? PpButtonMinimize
      {
         get => myButtonMinimize;
         set
         {
            if (myButtonMinimize != null) { Controls.Remove(myButtonMinimize); }

            if ((myButtonMinimize = value) != null)
            {
               myDoStandardButtonRefresh(myButtonMinimize);
               myButtonMinimize.Click += (s, e) => myActionOnDockCaptionEvent(this, EventType.button_minimize);
            }
         }
      }

      public Button? PpButtonRestore
      {
         get => myButtonRestore;

         set
         {
            if (myButtonRestore != null) { Controls.Remove(myButtonRestore); }

            if ((myButtonRestore = value) != null)
            {
               myButtonRestore.Click += (s, e) => myActionOnDockCaptionEvent(this, EventType.button_maximize);
            }

            myDoStandardButtonRefresh(myButtonRestore);
         }
      }

      public Button? PpButtonMaximize
      {
         get => myButtonMaximize;

         set
         {
            if (myButtonMaximize != null) { Controls.Remove(myButtonMaximize); }

            if ((myButtonMaximize = value) != null)
            {
               myButtonMaximize.Click += (s, e) => myActionOnDockCaptionEvent(this, EventType.button_maximize);
            }

            myDoStandardButtonRefresh(myButtonMaximize);
         }
      }

      public Button? PpButtonClose
      {
         get => myButtonClose;

         set
         {
            if (myButtonClose != null) { Controls.Remove(myButtonClose); }

            if ((myButtonClose = value) != null)
            {
               myDoStandardButtonRefresh(myButtonClose);
               myButtonClose.Click += (s, e) => myActionOnDockCaptionEvent(this, EventType.button_close);
            }
         }
      }

      /// <summary>
      ///  the distance in pixel from a control to another.
      /// </summary>
      public int PpItemSeparation { get; set; } = 5;

      /// <summary>
      ///  the distance in pixel from a standar button(minimize,maximize,close) to another.
      /// </summary>
      public int PpStandardButtonSeparation { get; set; } = 5;

      /// <summary>
      /// 
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
      ///  the right threshold ( item with sub-control id >= of it are placed at right)
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
      /// 
      /// </summary>
      [Browsable(false)]
      public Form? PpFormBound
      {
         get => myFormBound;

         set
         {
            if (myFormBound != null)
            {
               var fea = myFormBound.GetFeature<FormFeatureCustomCaptionResize>();

               if (fea != null)
               {
                  fea.CustomCaptionControl = null;
                  fea.OnStateChanged -= MyExtendedFormBound_OnStateChange;
               }

               OnDockCaptionEvent -= MthDefaultBehaviour;
            }

            if ((myFormBound = value) != null)
            {
               var fea = myFormBound.GetFeature<FormFeatureCustomCaptionResize>() ?? throw new Crash();

               fea.CustomCaptionControl = this;
               fea.OnStateChanged += MyExtendedFormBound_OnStateChange;
               OnDockCaptionEvent += MthDefaultBehaviour;
               myFormBound.Refresh();
               myFormBound.PerformLayout();
            }
         }
      }

      private static Bitmap myResizeImage(Image image, int width, int height)
      {
         var destRect = new Rectangle(0, 0, width, height);
         var destImage = new Bitmap(width, height);

         destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

         using (var graphics = Graphics.FromImage(destImage))
         {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (var wrapMode = new ImageAttributes())
            {
               wrapMode.SetWrapMode(WrapMode.TileFlipXY);
               graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }
         }

         return destImage;
      }

      /// <summary>
      /// 
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
      /// 
      /// </summary>
      public PictureBox PpImageBox => CtrlPictureIcon;

      /// <summary>
      /// 
      /// </summary>
      public override LayoutEngine LayoutEngine => new InnerLayoutEngine();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="eventType"></param>
      public void MthDefaultBehaviour(object? sender, EventType eventType)
      {
         switch (eventType)
         {
            case EventType.button_minimize:
               (PpFormBound ?? throw new Crash()).WindowState = FormWindowState.Minimized;
               break;

            case EventType.button_maximize:
               (PpFormBound ?? throw new Crash()).WindowState = PpFormBound.WindowState == FormWindowState.Maximized ?
                  PpFormBound.WindowState = FormWindowState.Normal :
                  PpFormBound.WindowState = FormWindowState.Maximized;
               PerformLayout();
               break;

            case EventType.button_close:
               (PpFormBound ?? throw new Crash()).Close();
               break;

            case EventType.start_tracking:
            case EventType.end_tracking:
               break;//nothing

            default: throw new Crash();
         }
      }

      protected virtual void myActionOnDockCaptionEvent(object? sender, EventType eventType) => OnDockCaptionEvent?.Invoke(sender, eventType);

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

      private void myDoStandardButtonRefresh(Button? button)
      {
         var std_btn = PpStandardButtons;
         var num_ctr_oth = Controls.OfType<Control>().Count(c => !std_btn.Contains(c));

         if (button != null) { Controls.Add(button); }

         for (var i = 0; i < std_btn.Length; i++) { Controls.SetChildIndex(std_btn[i], num_ctr_oth + i); }
      }

      private static ExtendedButtonCtrl myMakeStandardButton(string name)
      {
         var but = new ExtendedButtonCtrl();

         but.Name = name;

         return but;
      }

      private void CtrlPictureIcon_MouseDown(object? sender, MouseEventArgs e) => myDoShowSysMenu(e);

      private void ExtendedCaptionCtrl_MouseDown(object? sender, MouseEventArgs e)
      {
         if (e.Button == MouseButtons.Right) { myDoShowSysMenu(e); }
      }

      private void MyExtendedFormBound_OnStateChange(object? sender, StateChangedArgs args)
      {
         switch (args.NewState)
         {
            case TrackState.normal:
               if (args.OldState == TrackState.moving) { myActionOnDockCaptionEvent(this, EventType.end_tracking); }
               break;

            case TrackState.moving:
               myActionOnDockCaptionEvent(this, EventType.start_tracking);
               break;

            case TrackState.resize: break;

            default: throw new Crash();
         }
      }

      private void MyObservableParent_OnControlRemoved(Control control)
      {
         if (control == PpFormBound) { PpFormBound = null; }
      }
   }
}

