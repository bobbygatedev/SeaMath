using Gate.Tools;
using Gate.ToolsView.ControlObserve;
using Gate.ToolsView.Extensions;
using Gate.ToolsView.Native;
using System.ComponentModel;
using System.Drawing.Imaging;

namespace Gate.ToolsView.Extended
{
   /// <summary>
   /// A custom scrollbar control.
   /// </summary>
   [Designer(typeof(ExtendedScrollBarCtrlDesigner))]
   [DefaultEvent("Scroll")]
   [DefaultProperty("Value")]
   public partial class ExtendedScrollBar : Control
   {
      #region enums
      /// <summary>
      /// The scrollbar states.
      /// </summary>
      private enum InnerStateEnum
      {
         /// <summary>
         /// Indicates a normal scrollbar state.
         /// </summary>
         Normal,

         /// <summary>
         /// Indicates a hot scrollbar state.
         /// </summary>
         Hot,

         /// <summary>
         /// Indicates an active scrollbar state.
         /// </summary>
         Active,

         /// <summary>
         /// Indicates a pressed scrollbar state.
         /// </summary>
         Pressed,

         /// <summary>
         /// Indicates a disabled scrollbar state.
         /// </summary>
         Disabled
      }

      /// <summary>
      /// The scrollbar arrow button states.
      /// </summary>
      private enum InnerArrowButtonStateEnum
      {
         /// <summary>
         /// Indicates the up arrow is in normal state.
         /// </summary>
         UpNormal,

         /// <summary>
         /// Indicates the up arrow is in hot state.
         /// </summary>
         UpHot,

         /// <summary>
         /// Indicates the up arrow is in active state.
         /// </summary>
         UpActive,

         /// <summary>
         /// Indicates the up arrow is in pressed state.
         /// </summary>
         UpPressed,

         /// <summary>
         /// Indicates the up arrow is in disabled state.
         /// </summary>
         UpDisabled,

         /// <summary>
         /// Indicates the down arrow is in normal state.
         /// </summary>
         DownNormal,

         /// <summary>
         /// Indicates the down arrow is in hot state.
         /// </summary>
         DownHot,

         /// <summary>
         /// Indicates the down arrow is in active state.
         /// </summary>
         DownActive,

         /// <summary>
         /// Indicates the down arrow is in pressed state.
         /// </summary>
         DownPressed,

         /// <summary>
         /// Indicates the down arrow is in disabled state.
         /// </summary>
         DownDisabled
      }
      #endregion

      #region fields

      /// <summary>
      /// Redraw const.
      /// </summary>
      private const int SETREDRAW = 11;

      /// <summary>
      /// Indicates many changes to the scrollbar are happening, so stop painting till finished.
      /// </summary>
      private bool myIsInUpdate;

      /// <summary>
      /// The scrollbar orientation - horizontal / vertical.
      /// </summary>
      private ExtendedScrollBarOrientationEnum myOrientation = ExtendedScrollBarOrientationEnum.Vertical;

      /// <summary>
      /// The scroll orientation in scroll events.
      /// </summary>
      private ScrollOrientation myScrollOrientation = ScrollOrientation.VerticalScroll;

      /// <summary>
      /// The clicked channel rectangle.
      /// </summary>
      private Rectangle myClickedBarRectangle;

      /// <summary>
      /// The thumb rectangle.
      /// </summary>
      private Rectangle myThumbRectangle;

      /// <summary>
      /// The top arrow rectangle.
      /// </summary>
      private Rectangle myTopArrowRectangle;

      /// <summary>
      /// The bottom arrow rectangle.
      /// </summary>
      private Rectangle myBottomArrowRectangle;

      /// <summary>
      /// The channel rectangle.
      /// </summary>
      private Rectangle myChannelRectangle;

      /// <summary>
      /// Indicates if top arrow was clicked.
      /// </summary>
      private bool myTopArrowClicked;

      /// <summary>
      /// Indicates if bottom arrow was clicked.
      /// </summary>
      private bool myBottomArrowClicked;

      /// <summary>
      /// Indicates if channel rectangle above the thumb was clicked.
      /// </summary>
      private bool myTopBarClicked;

      /// <summary>
      /// Indicates if channel rectangle under the thumb was clicked.
      /// </summary>
      private bool myBottomBarClicked;

      /// <summary>
      /// Indicates if the thumb was clicked.
      /// </summary>
      private bool myThumbClicked;

      /// <summary>
      /// The state of the thumb.
      /// </summary>
      private InnerStateEnum myThumbState = InnerStateEnum.Normal;

      /// <summary>
      /// The scrollbar value minimum.
      /// </summary>
      private int myMinimum;

      /// <summary>
      /// The scrollbar value maximum.
      /// </summary>
      private int myMaximum = 100;

      /// <summary>
      /// The small change value.
      /// </summary>
      private int mySmallChange = 1;

      /// <summary>
      /// The large change value.
      /// </summary>
      private int myLargeChange = 10;

      /// <summary>
      /// The value of the scrollbar.
      /// </summary>
      private int myValue;

      /// <summary>
      /// The width of the thumb.
      /// </summary>
      private int myThumbWidth = 15;

      /// <summary>
      /// The height of the thumb.
      /// </summary>
      private int myThumbHeight;

      /// <summary>
      /// The width of an arrow.
      /// </summary>
      private int myArrowWidth = 15;

      /// <summary>
      /// The height of an arrow.
      /// </summary>
      private int myArrowHeight = 17;

      /// <summary>
      /// The bottom limit for the thumb bottom.
      /// </summary>
      private int myThumbBottomLimitBottom;

      /// <summary>
      /// The bottom limit for the thumb top.
      /// </summary>
      private int myThumbBottomLimitTop;

      /// <summary>
      /// The top limit for the thumb top.
      /// </summary>
      private int myThumbTopLimit;

      /// <summary>
      /// The current position of the thumb.
      /// </summary>
      private int myThumbPosition;

      /// <summary>
      /// The track position.
      /// </summary>
      private int myTrackPosition;

      /// <summary>
      /// The progress timer for moving the thumb.
      /// </summary>
      private System.Windows.Forms.Timer myProgressTimer = new System.Windows.Forms.Timer();

      /// <summary>
      /// The border color.
      /// </summary>
      private Color myBorderColor = Color.FromArgb(93, 140, 201);

      /// <summary>
      /// The border color in disabled state.
      /// </summary>
      private Color myDisabledBorderColor = Color.Gray;

      /// <summary>
      /// 
      /// </summary>
      private ControlObservableChild? myMouseWheelControlChilObserver;

      private InnerRenderer myScrollBarExRenderer = new InnerRenderer();

      #region context menu items

      /// <summary>
      /// Context menu strip.
      /// </summary>

      /// <summary>
      /// Container for components.
      /// </summary>

      /// <summary>
      /// Menu item.
      /// </summary>

      /// <summary>
      /// Menu separator.
      /// </summary>

      /// <summary>
      /// Menu item.
      /// </summary>

      /// <summary>
      /// Menu item.
      /// </summary>

      /// <summary>
      /// Menu separator.
      /// </summary>

      /// <summary>
      /// Menu item.
      /// </summary>

      /// <summary>
      /// Menu item.
      /// </summary>

      /// <summary>
      /// Menu separator.
      /// </summary>

      /// <summary>
      /// Menu item.
      /// </summary>

      /// <summary>
      /// Menu item.
      /// </summary>
      private bool myIsBorderToDraw = false;
      private Color myArrowColor = Color.Black;
      private Color myGripColor = Color.DarkGray;
      private Color myGripActiveColor = Color.Gray;
      private Control? myMouseWheelControl;
      #endregion

      #endregion

      #region constructor

      /// <summary>
      /// Initializes a new instance of the <see cref="ExtendedScrollBar"/> class.
      /// </summary>
      public ExtendedScrollBar()
      {
         // sets the control styles of the control
         SetStyle(
            ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint,
            true);

         InitializeComponent();// initializes the context menu

         Width = 19;
         Height = 200;

         //forces back/fore color update
         BackColor = BackColor;
         ForeColor = ForeColor;

         // sets the scrollbar up
         myDoSetUpScrollBar();

         // timer for clicking and holding the mouse button
         // over/below the thumb and on the arrow buttons
         myProgressTimer.Interval = 20;
         myProgressTimer.Tick += myDoProgressTimerTick;

         // no image margin in context menu
         CtrlContextMenu.ShowImageMargin = false;
         ContextMenuStrip = CtrlContextMenu;
      }

      #endregion

      /// <summary>
      /// The scrollbar renderer class.
      /// </summary>
      private class InnerRenderer
      {
         /// <summary>
         /// 
         /// </summary>
         public Color BackColor { get; set; } = Color.Empty;

         /// <summary>
         /// Draws the grip of the thumb.
         /// </summary>
         /// <param name="graphics">The <see cref="Graphics"/> used to paint.</param>
         /// <param name="rect">The rectangle in which to paint.</param>
         /// <param name="orientation">The <see cref="ExtendedScrollBarOrientationEnum"/>.</param>
         public void DrawThumbGrip(Graphics graphics, Rectangle rect, ExtendedScrollBarOrientationEnum orientation, Color gripColor)
         {
            if (graphics == null) { throw new ArgumentNullException("graphics"); }
            else if (!rect.IsEmpty && !graphics.IsVisibleClipEmpty && graphics.VisibleClipBounds.IntersectsWith(rect))
            {
               graphics.FillRectangle(new SolidBrush(gripColor), rect);
            }
         }

         /// <summary>
         /// Draws an arrow button.
         /// </summary>
         /// <param name="graphics">The <see cref="Graphics"/> used to paint.</param>
         /// <param name="arrowRect">The rectangle in which to paint.</param>
         /// <param name="state">The <see cref="InnerArrowButtonStateEnum"/> of the arrow button.</param>
         /// <param name="arrowUp">true for an up arrow, false otherwise.</param>
         /// <param name="orientation">The <see cref="ExtendedScrollBarOrientationEnum"/>.</param>
         public void DrawArrowButton(
            Graphics graphics,
            Rectangle arrowRect,
            bool arrowUp,
            ExtendedScrollBarOrientationEnum orientation,
            Color arrowBackColor)
         {
            if (graphics == null) { throw new ArgumentNullException("graphics"); }
            else if (!arrowRect.IsEmpty && !graphics.IsVisibleClipEmpty && graphics.VisibleClipBounds.IntersectsWith(arrowRect))
            {
               if (orientation == ExtendedScrollBarOrientationEnum.Vertical)
               {
                  myDrawArrowButtonVertical(graphics, arrowRect, arrowUp, arrowBackColor);
               }
               else
               {
                  myDrawArrowButtonHorizontal(graphics, arrowRect, arrowUp, arrowBackColor);
               }
            }
         }

         /// <summary>
         /// Draws an arrow button.
         /// </summary>
         /// <param name="graphics">The <see cref="Graphics"/> used to paint.</param>
         /// <param name="arrowRect">The rectangle in which to paint.</param>
         /// <param name="arrowUp">true for an up arrow, false otherwise.</param>
         private static void myDrawArrowButtonVertical(
            Graphics graphics, Rectangle arrowRect, bool arrowUp, Color arrowBackColor)
         {
            using (var arr_img = myGetArrowDownButtonImage(arrowRect.Width, arrowBackColor))
            {
               if (arrowUp) { arr_img.RotateFlip(RotateFlipType.Rotate180FlipNone); }

               graphics.DrawImage(arr_img, arrowRect);
            }
         }

         /// <summary>
         /// Draws an arrow button.
         /// </summary>
         /// <param name="graphics">The <see cref="Graphics"/> used to paint.</param>
         /// <param name="arrowRect">The rectangle in which to paint.</param>
         /// <param name="state">The <see cref="InnerArrowButtonStateEnum"/> of the arrow button.</param>
         /// <param name="arrowUp">true for an up arrow, false otherwise.</param>
         private static void myDrawArrowButtonHorizontal(Graphics graphics, Rectangle arrowRect, bool arrowUp, Color arrowBackColor)
         {
            using (var arr_img = myGetArrowDownButtonImage(arrowRect.Height, arrowBackColor))
            {
               if (arrowUp) { arr_img.RotateFlip(RotateFlipType.Rotate90FlipNone); }
               else { arr_img.RotateFlip(RotateFlipType.Rotate270FlipNone); }

               graphics.DrawImage(arr_img, arrowRect);
            }
         }

         /// <summary>
         /// Draws the arrow down button for the scrollbar.
         /// </summary>
         /// <param name="W"></param>
         /// <param name="arrowBackColor"></param>
         /// <returns>The arrow down button as <see cref="Image"/>.</returns>
         private static Image myGetArrowDownButtonImage(int W, Color arrowBackColor)
         {
            var H = (int)(W * 15.0 / 17);
            var bmp = new Bitmap(W, H, PixelFormat.Format32bppArgb);
            var arr_w = (int)(W * 0.55);
            var arr_h = (int)(H * 0.30);
            var arr_x = (W - arr_w) / 2;
            var arr_y = (H - arr_h) / 2;

            bmp.SetResolution(72f, 72f);

            using (var gra = Graphics.FromImage(bmp))
            {
               var pen = new Pen(new SolidBrush(Color.Blue));
               var pts = new Point[] { new Point(arr_x, arr_y), new Point(arr_x + arr_w, arr_y), new Point(arr_x + arr_w / 2, arr_y + arr_h) };

               gra.FillPolygon(new SolidBrush(arrowBackColor), pts);
            }

            return bmp;
         }
      }

      #region events
      /// <summary>
      /// Occurs when the scrollbar scrolled.
      /// </summary>
      [Category("Behavior")]
      [Description("Is raised, when the scrollbar was scrolled.")]
      public event ScrollEventHandler? Scroll;
      #endregion

      #region properties

      /// <summary>
      ///  Back Color.
      /// </summary>
      [Category("Appearance")]
      [Description(" Back Color.")]
      public override Color BackColor
      {
         get => base.BackColor;
         set
         {
            myScrollBarExRenderer.BackColor = base.BackColor = value;
            myDoSetUpScrollBar();
         }
      }

      /// <summary>
      /// Orientation.
      /// </summary>
      [Category("Layout")]
      [Description("Orientation.")]
      [DefaultValue(ExtendedScrollBarOrientationEnum.Vertical)]
      public ExtendedScrollBarOrientationEnum Orientation
      {
         get => myOrientation;

         set
         {
            // no change - return
            if (value == myOrientation) { return; }

            myOrientation = value;

            // change text of context menu entries
            myDoChangeContextMenuItems();

            // save scroll orientation for scroll event
            myScrollOrientation = value == ExtendedScrollBarOrientationEnum.Vertical ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll;

            // only in DesignMode switch width and height
            if (DesignMode) { Size = new Size(Height, Width); }

            // sets the scrollbar up
            myDoSetUpScrollBar();
         }
      }

      /// <summary>
      /// Minimum value.
      /// </summary>
      [Category("Behavior")]
      [Description("Minimum value.")]
      [DefaultValue(0)]
      public int Minimum
      {
         get => myMinimum;

         set
         {
            // no change or value invalid - return
            if (myMinimum == value || value < 0 || value >= myMaximum) { return; }

            myMinimum = value;

            // current value less than new minimum value - adjust
            if (myValue < value) { myValue = value; }

            // is current large change value invalid - adjust
            if (myLargeChange > myMaximum - myMinimum) { myLargeChange = myMaximum - myMinimum; }

            myDoSetUpScrollBar();

            // current value less than new minimum value - adjust
            if (myValue < value) { Value = value; }
            else
            {
               // current value is valid - adjust thumb position
               myDoChangeThumbPosition(myDoGetThumbPosition());

               Refresh();
            }
         }
      }

      /// <summary>
      /// Maximum value.
      /// </summary>
      [Category("Behavior")]
      [Description("Maximum value.")]
      [DefaultValue(100)]
      public int Maximum
      {
         get => myMaximum;

         set
         {
            // no change or new max. value invalid - return
            if (value == myMaximum || value < 1 || value <= myMinimum) { return; }

            myMaximum = value;

            // is large change value invalid - adjust
            if (myLargeChange > myMaximum - myMinimum) { myLargeChange = myMaximum - myMinimum; }

            myDoSetUpScrollBar();

            // is current value greater than new maximum value - adjust
            if (myValue > value) { Value = myMaximum; }
            else
            {
               // current value is valid - adjust thumb position
               myDoChangeThumbPosition(myDoGetThumbPosition());
               Refresh();
            }
         }
      }

      /// <summary>
      /// Small change amount.
      /// </summary>
      [Category("Behavior")]
      [Description("Small change value.")]
      [DefaultValue(1)]
      public int SmallChange
      {
         get => mySmallChange;

         set
         {
            // no change or new small change value invalid - return
            if (value == mySmallChange || value < 1 || value >= myLargeChange) { return; }

            mySmallChange = value;

            myDoSetUpScrollBar();
         }
      }

      /// <summary>
      /// Large change amount.
      /// </summary>
      [Category("Behavior")]
      [Description("Large change value.")]
      [DefaultValue(10)]
      public int LargeChange
      {
         get => myLargeChange;

         set
         {
            // no change or new large change value is invalid - return
            if (value == myLargeChange || value < mySmallChange || value < 2) { return; }

            // if value is greater than scroll area - adjust
            if (value > myMaximum - myMinimum) { myLargeChange = myMaximum - myMinimum; }
            else { myLargeChange = value; }// set new value

            myDoSetUpScrollBar();
         }
      }

      /// <summary>
      /// Value.
      /// </summary>
      [Category("Behavior")]
      [Description("Current value.")]
      [DefaultValue(0)]
      public int Value
      {
         get => myValue;

         set
         {
            // no change or invalid value - return
            if (myValue == value || value < myMinimum || value > myMaximum) { return; }

            myDoUpdateScroll(value, true);
         }
      }

      private void myDoUpdateScroll(int newValue, bool isRaiseEvents)
      {
         myValue = newValue;

         // adjust thumb position
         myDoChangeThumbPosition(myDoGetThumbPosition());

         if (isRaiseEvents)
         {
            // raise scroll event
            OnScroll(new ScrollEventArgs(ScrollEventType.ThumbPosition, -1, myValue, myScrollOrientation));
         }

         Refresh();
      }

      /// <summary>
      /// Border color.
      /// </summary>
      [Category("Appearance")]
      [Description("Border color.")]
      [DefaultValue(typeof(Color), "93, 140, 201")]
      public Color PpBorderColor
      {
         get => myBorderColor;

         set
         {
            myBorderColor = value;

            Invalidate();
         }
      }

      /// <summary>
      /// Border color in disabled state.
      /// </summary>
      [Category("Appearance")]
      [Description("Border color in disabled state.")]
      [DefaultValue(typeof(Color), "Gray")]
      public Color PpDisabledBorderColor
      {
         get => myDisabledBorderColor;

         set
         {
            myDisabledBorderColor = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Opacity of the context menu (from 0 - 1).
      /// </summary>
      [Category("Appearance")]
      [Description("Opacity of the context menu (from 0 - 1).")]
      [DefaultValue(typeof(double), "1")]
      public double Opacity
      {
         get => CtrlContextMenu.Opacity;

         set
         {
            // no change - return
            if (value == CtrlContextMenu.Opacity) { return; }

            CtrlContextMenu.AllowTransparency = value != 1;
            CtrlContextMenu.Opacity = value;
         }
      }

      /// <summary>
      /// Border color is to draw.
      /// </summary>
      [Category("Appearance")]
      [Description("Border color in disabled state.")]
      [DefaultValue(false)]
      public bool PpIsBorderToDraw
      {
         get => myIsBorderToDraw;
         set
         {
            myIsBorderToDraw = value;
            myDoSetUpScrollBar();
         }
      }

      [Category("Appearance")]
      [Description("Border color in disabled state.")]
      [DefaultValue(typeof(Color), "Black")]
      public Color PpArrowColor
      {
         get => myArrowColor;
         set
         {
            myArrowColor = value;
            myDoSetUpScrollBar();
         }
      }

      [Category("Appearance")]
      [Description("Solid grip back color.")]
      [DefaultValue(typeof(Color), "DarkGray")]
      public Color PpGripColor
      {
         get => myGripColor;
         set
         {
            myGripColor = value;
            myDoSetUpScrollBar();
         }
      }

      [Category("Appearance")]
      [Description("Solid grip back color when active (pressed or selected).")]
      [DefaultValue(typeof(Color), "DarkGray")]
      public Color PpGripActiveColor
      {
         get => myGripActiveColor;
         set
         {
            myGripActiveColor = value;
            myDoSetUpScrollBar();
         }
      }

      /// <summary>
      /// <see cref = "PpMouseWheelControl"/> and all its descendents have the ability to change <see cref="Value"/> using mouse wheel.
      /// </summary>
      public Control? PpMouseWheelControl
      {
         get => myMouseWheelControl;

         set
         {
            if (myMouseWheelControl != value)
            {
               if (myMouseWheelControl != null)
               {
                  foreach (var ctr in myMouseWheelControlChilObserver?.ObservedControls ?? [])
                  {
                     ctr.MouseWheel -= myActionOnMouseWheel;
                  }
               }

               myMouseWheelControl = value;

               if (myMouseWheelControl != null)
               {
                  var dsd = myMouseWheelControl.MthGetNephewsAndMe();

                  myMouseWheelControlChilObserver = new ControlObservableChild(myMouseWheelControl);
                  myMouseWheelControlChilObserver.OnControlAdded += myActionOnMouseWheelControlChilObserverAdded;
                  myMouseWheelControlChilObserver.OnControlRemoved += myActionOnMouseWheelControlChilObserverRemoved;
               }
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public int PpWheelSensitivityMultiplier { get; set; } = 4;

      public void MthUpdateValueWithoutFireEvents(int newValue) => myDoUpdateScroll(newValue, false);

      private void myActionOnMouseWheelControlChilObserverRemoved(Control control) => control.MouseWheel -= myActionOnMouseWheel;

      private void myActionOnMouseWheelControlChilObserverAdded(Control control) => control.MouseWheel += myActionOnMouseWheel;

      private void myActionOnMouseWheel(object? sender, MouseEventArgs e)
      {
         if (Visible && Enabled)
         {
            Value += -(e.Delta * PpWheelSensitivityMultiplier / 120);
         }
      }

      #endregion

      #region methods

      #region public methods

      /// <summary>
      /// Prevents the drawing of the control until <see cref="EndUpdate"/> is called.
      /// </summary>
      public void BeginUpdate()
      {
         NativeMethods.SendMessage(Handle, (WinMsgEnum)SETREDRAW, 0, 0);
         myIsInUpdate = true;
      }

      /// <summary>
      /// Ends the updating process and the control can draw itself again.
      /// </summary>
      public void EndUpdate()
      {
         NativeMethods.SendMessage(Handle, (WinMsgEnum)SETREDRAW, 1, 0);
         myIsInUpdate = false;
         myDoSetUpScrollBar();
         Refresh();
      }

      #endregion

      #region protected methods

      /// <summary>
      /// Raises the <see cref="Scroll"/> event.
      /// </summary>
      /// <param name="e">The <see cref="ScrollEventArgs"/> that contains the event data.</param>
      protected virtual void OnScroll(ScrollEventArgs e) => Scroll?.Invoke(this, e);// if event handler is attached - raise scroll event

      /// <summary>
      /// Paints the control.
      /// </summary>
      /// <param name="e">A <see cref="PaintEventArgs"/> that contains information about the control to paint.</param>
      protected override void OnPaint(PaintEventArgs e)
      {
         // sets the smoothing mode to none
         e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

         // save client rectangle
         var cli_rc = ClientRectangle;

         // adjust the rectangle
         if (myOrientation == ExtendedScrollBarOrientationEnum.Vertical)
         {
            cli_rc.X++;
            cli_rc.Y += myArrowHeight + 1;
            cli_rc.Width -= 2;
            cli_rc.Height -= (myArrowHeight * 2) + 2;
         }
         else
         {
            cli_rc.X += myArrowWidth + 1;
            cli_rc.Y++;
            cli_rc.Width -= (myArrowWidth * 2) + 2;
            cli_rc.Height -= 2;
         }

         if (PpBorderColor != Color.Empty)
         {
            e.Graphics.DrawRectangle(new Pen(new SolidBrush(PpBorderColor)), ClientRectangle);
         }

         if (Enabled)
         {
            myScrollBarExRenderer.DrawThumbGrip(e.Graphics, myThumbRectangle, myOrientation, myDoGetGripColor());
         }

         // draw arrows
         myScrollBarExRenderer.DrawArrowButton(e.Graphics, myTopArrowRectangle, true, myOrientation, PpArrowColor);
         myScrollBarExRenderer.DrawArrowButton(e.Graphics, myBottomArrowRectangle, false, myOrientation, PpArrowColor);

         // check if top or bottom bar was clicked
         if (myTopBarClicked)
         {
            if (myOrientation == ExtendedScrollBarOrientationEnum.Vertical)
            {
               myClickedBarRectangle.Y = myThumbTopLimit;
               myClickedBarRectangle.Height = myThumbRectangle.Y - myThumbTopLimit;
            }
            else
            {
               myClickedBarRectangle.X = myThumbTopLimit;
               myClickedBarRectangle.Width = myThumbRectangle.X - myThumbTopLimit;
            }
         }
         else if (myBottomBarClicked)
         {
            if (myOrientation == ExtendedScrollBarOrientationEnum.Vertical)
            {
               myClickedBarRectangle.Y = myThumbRectangle.Bottom + 1;
               myClickedBarRectangle.Height = myThumbBottomLimitBottom - myClickedBarRectangle.Y + 1;
            }
            else
            {
               myClickedBarRectangle.X = myThumbRectangle.Right + 1;
               myClickedBarRectangle.Width = myThumbBottomLimitBottom - myClickedBarRectangle.X + 1;
            }
         }

         // draw border
         using (var pen = new Pen(Enabled ? myBorderColor : myDisabledBorderColor))
         {
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
         }
      }

      /// <summary>
      /// Raises the MouseDown event.
      /// </summary>
      /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
      protected override void OnMouseDown(MouseEventArgs e)
      {
         base.OnMouseDown(e);

         Focus();

         if (e.Button == MouseButtons.Left)
         {
            // prevents showing the context menu if pressing the right mouse
            // button while holding the left
            ContextMenuStrip = null;

            var mou_loc = e.Location;

            if (myThumbRectangle.Contains(mou_loc))
            {
               myThumbClicked = true;
               myThumbPosition = myOrientation == ExtendedScrollBarOrientationEnum.Vertical ? mou_loc.Y - myThumbRectangle.Y : mou_loc.X - myThumbRectangle.X;
               myThumbState = InnerStateEnum.Pressed;

               Invalidate(myThumbRectangle);
            }
            else if (myTopArrowRectangle.Contains(mou_loc))
            {
               myTopArrowClicked = true;
               Invalidate(myTopArrowRectangle);
               myDoProgressThumb(true);
            }
            else if (myBottomArrowRectangle.Contains(mou_loc))
            {
               myBottomArrowClicked = true;
               Invalidate(myBottomArrowRectangle);
               myDoProgressThumb(true);
            }
            else
            {
               myTrackPosition = myOrientation == ExtendedScrollBarOrientationEnum.Vertical ? mou_loc.Y : mou_loc.X;

               if (myTrackPosition < (myOrientation == ExtendedScrollBarOrientationEnum.Vertical ? myThumbRectangle.Y : myThumbRectangle.X))
               {
                  myTopBarClicked = true;
               }
               else
               {
                  myBottomBarClicked = true;
               }

               myDoProgressThumb(true);
            }
         }
         else if (e.Button == MouseButtons.Right)
         {
            myTrackPosition =
               myOrientation == ExtendedScrollBarOrientationEnum.Vertical ? e.Y : e.X;
         }
      }

      /// <summary>
      /// Raises the MouseUp event.
      /// </summary>
      /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
      protected override void OnMouseUp(MouseEventArgs e)
      {
         base.OnMouseUp(e);

         if (e.Button == MouseButtons.Left)
         {
            ContextMenuStrip = CtrlContextMenu;

            if (myThumbClicked)
            {
               myThumbClicked = false;
               myThumbState = InnerStateEnum.Normal;

               OnScroll(new ScrollEventArgs(ScrollEventType.EndScroll, -1, myValue, myScrollOrientation));
            }
            else if (myTopArrowClicked)
            {
               myTopArrowClicked = false;
               myDoStopTimer();
            }
            else if (myBottomArrowClicked)
            {
               myBottomArrowClicked = false;
               myDoStopTimer();
            }
            else if (myTopBarClicked)
            {
               myTopBarClicked = false;
               myDoStopTimer();
            }
            else if (myBottomBarClicked)
            {
               myBottomBarClicked = false;
               myDoStopTimer();
            }

            Invalidate();
         }
      }

      /// <summary>
      /// Raises the MouseEnter event.
      /// </summary>
      /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
      protected override void OnMouseEnter(EventArgs e)
      {
         base.OnMouseEnter(e);

         myThumbState = InnerStateEnum.Active;
         Invalidate();
      }

      /// <summary>
      /// Raises the MouseLeave event.
      /// </summary>
      /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
      protected override void OnMouseLeave(EventArgs e)
      {
         base.OnMouseLeave(e);

         myDoResetScrollStatus();
      }

      /// <summary>
      /// Raises the MouseMove event.
      /// </summary>
      /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
      protected override void OnMouseMove(MouseEventArgs e)
      {
         base.OnMouseMove(e);

         // moving and holding the left mouse button
         if (e.Button == MouseButtons.Left)
         {
            // Update the thumb position, if the new location is within the bounds.
            if (myThumbClicked)
            {
               var old_scr_val = myValue;
               var pos = myOrientation == ExtendedScrollBarOrientationEnum.Vertical ? e.Location.Y : e.Location.X;

               // The thumb is all the way to the top
               if (pos <= (myThumbTopLimit + myThumbPosition))
               {
                  myDoChangeThumbPosition(myThumbTopLimit);
                  myValue = myMinimum;
               }
               else if (pos >= (myThumbBottomLimitTop + myThumbPosition))
               {
                  // The thumb is all the way to the bottom
                  myDoChangeThumbPosition(myThumbBottomLimitTop);
                  myValue = myMaximum;
               }
               else
               {
                  // The thumb is between the ends of the track.
                  myDoChangeThumbPosition(pos - myThumbPosition);

                  int pix_rng, thm_pos, arr_siz;

                  // calculate the value - first some helper variables
                  // dependent on the current orientation
                  if (myOrientation == ExtendedScrollBarOrientationEnum.Vertical)
                  {
                     pix_rng = Height - (2 * myArrowHeight) - myThumbHeight;
                     thm_pos = myThumbRectangle.Y;
                     arr_siz = myArrowHeight;
                  }
                  else
                  {
                     pix_rng = Width - (2 * myArrowWidth) - myThumbWidth;
                     thm_pos = myThumbRectangle.X;
                     arr_siz = myArrowWidth;
                  }

                  var prc = 0f;

                  // percent of the new position
                  if (pix_rng != 0) { prc = (float)(thm_pos - arr_siz) / (float)pix_rng; }

                  // the new value is somewhere between max and min, starting
                  // at min position
                  myValue = Convert.ToInt32((prc * (myMaximum - myMinimum)) + myMinimum);
               }

               // raise scroll event if new value different
               if (old_scr_val != myValue)
               {
                  OnScroll(new ScrollEventArgs(ScrollEventType.ThumbTrack, old_scr_val, myValue, myScrollOrientation));
                  Refresh();
               }
            }
         }
         else if (!ClientRectangle.Contains(e.Location)) { myDoResetScrollStatus(); }
         else if (e.Button == MouseButtons.None) // only moving the mouse
         {
            if (myTopArrowRectangle.Contains(e.Location)) { Invalidate(myTopArrowRectangle); }
            else if (myBottomArrowRectangle.Contains(e.Location)) { Invalidate(myBottomArrowRectangle); }
            else if (myThumbRectangle.Contains(e.Location))
            {
               myThumbState = InnerStateEnum.Hot;
               Invalidate(myThumbRectangle);
            }
            else if (ClientRectangle.Contains(e.Location))
            {
               myThumbState = InnerStateEnum.Active;
               Invalidate();
            }
         }
      }

      /// <summary>
      /// Raises the <see cref="System.Windows.Forms.Control.SizeChanged"/> event.
      /// </summary>
      /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
      protected override void OnSizeChanged(EventArgs e)
      {
         base.OnSizeChanged(e);
         myDoSetUpScrollBar();
      }

      /// <summary>
      /// Processes a dialog key.
      /// </summary>
      /// <param name="keyData">One of the <see cref="System.Windows.Forms.Keys"/> values that represents the key to process.</param>
      /// <returns>true, if the key was processed by the control, false otherwise.</returns>
      protected override bool ProcessDialogKey(Keys keyData)
      {
         // key handling is here - keys recognized by the control
         // Up&Down or Left&Right, PageUp, PageDown, Home, End
         var key_up = Keys.Up;
         var key_dwn = Keys.Down;

         if (myOrientation == ExtendedScrollBarOrientationEnum.Horizontal)
         {
            key_up = Keys.Left;
            key_dwn = Keys.Right;
         }

         if (keyData == key_up)
         {
            Value -= mySmallChange;

            return true;
         }

         if (keyData == key_dwn)
         {
            Value += mySmallChange;

            return true;
         }

         if (keyData == Keys.PageUp)
         {
            Value = myDoGetValue(false, true);

            return true;
         }

         if (keyData == Keys.PageDown)
         {
            if (myValue + myLargeChange > myMaximum) { Value = myMaximum; }
            else { Value += myLargeChange; }

            return true;
         }

         if (keyData == Keys.Home)
         {
            Value = myMinimum;

            return true;
         }

         if (keyData == Keys.End)
         {
            Value = myMaximum;

            return true;
         }

         return base.ProcessDialogKey(keyData);
      }

      /// <summary>
      /// Raises the <see cref="System.Windows.Forms.Control.EnabledChanged"/> event.
      /// </summary>
      /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
      protected override void OnEnabledChanged(EventArgs e)
      {
         base.OnEnabledChanged(e);

         myThumbState = Enabled ? InnerStateEnum.Normal : InnerStateEnum.Disabled;
         Refresh();
      }

      #endregion

      #region misc methods

      /// <summary>
      /// Sets up the scrollbar.
      /// </summary>
      private void myDoSetUpScrollBar()
      {
         // if no drawing - return
         if (myIsInUpdate) { return; }

         var mrg_fac = 0.2;

         // set up the width's, height's and rectangles for the different
         // elements
         if (myOrientation == ExtendedScrollBarOrientationEnum.Vertical)
         {
            var mrg = Width > 5 ? (int)(mrg_fac * Width) : 0;

            myArrowWidth = Width - 2 * mrg;
            myArrowHeight = (int)(myArrowWidth * 17.0 / 15);
            myThumbWidth = myArrowWidth;
            myThumbHeight = myDoGetThumbSize();

            myClickedBarRectangle = ClientRectangle;
            myClickedBarRectangle.Inflate(-1, -1);
            myClickedBarRectangle.Y += myArrowHeight;
            myClickedBarRectangle.Height -= myArrowHeight * 2;

            myChannelRectangle = myClickedBarRectangle;
            myThumbRectangle = new Rectangle(
               ClientRectangle.X + mrg, ClientRectangle.Y + myArrowHeight + 1, myThumbWidth - 1, myThumbHeight);
            myTopArrowRectangle = new Rectangle(
               ClientRectangle.X + mrg, ClientRectangle.Y + 1, myArrowWidth, myArrowHeight);
            myBottomArrowRectangle = new Rectangle(
               ClientRectangle.X + mrg, ClientRectangle.Bottom - myArrowHeight - 1, myArrowWidth, myArrowHeight);

            // Set the default starting thumb position.
            myThumbPosition = myThumbRectangle.Height / 2;

            // Set the bottom limit of the thumb's bottom border.
            myThumbBottomLimitBottom = ClientRectangle.Bottom - myArrowHeight - 2;

            // Set the bottom limit of the thumb's top border.
            myThumbBottomLimitTop = myThumbBottomLimitBottom - myThumbRectangle.Height;

            // Set the top limit of the thumb's top border.
            myThumbTopLimit = ClientRectangle.Y + myArrowHeight + 1;
         }
         else
         {
            var mrg = Width > 5 ? (int)(mrg_fac * Height) : 0;

            myArrowHeight = Height - 2 * mrg;
            myArrowWidth = (int)(17.0 * myArrowHeight / 15);
            myThumbHeight = myArrowHeight;
            myThumbWidth = myDoGetThumbSize();

            myClickedBarRectangle = ClientRectangle;
            myClickedBarRectangle.Inflate(-1, -1);
            myClickedBarRectangle.X += myArrowWidth;
            myClickedBarRectangle.Width -= myArrowWidth * 2;

            myChannelRectangle = myClickedBarRectangle;

            myThumbRectangle = new Rectangle(
               ClientRectangle.X + myArrowWidth + 1, ClientRectangle.Y + mrg, myThumbWidth, myThumbHeight - 1);
            myTopArrowRectangle = new Rectangle(
               ClientRectangle.X + 1, ClientRectangle.Y + mrg, myArrowWidth, myArrowHeight);
            myBottomArrowRectangle = new Rectangle(
               ClientRectangle.Right - myArrowWidth - 1, ClientRectangle.Y + mrg, myArrowWidth, myArrowHeight);

            // Set the default starting thumb position.
            myThumbPosition = myThumbRectangle.Width / 2;

            // Set the bottom limit of the thumb's bottom border.
            myThumbBottomLimitBottom = ClientRectangle.Right - myArrowWidth - 2;

            // Set the bottom limit of the thumb's top border.
            myThumbBottomLimitTop = myThumbBottomLimitBottom - myThumbRectangle.Width;

            // Set the top limit of the thumb's top border.
            myThumbTopLimit = ClientRectangle.X + myArrowWidth + 1;
         }

         myDoChangeThumbPosition(myDoGetThumbPosition());
         Refresh();
      }

      /// <summary>
      /// Handles the updating of the thumb.
      /// </summary>
      /// <param name="sender">The sender.</param>
      /// <param name="e">An object that contains the event data.</param>
      private void myDoProgressTimerTick(object? sender, EventArgs e) => myDoProgressThumb(true);

      /// <summary>
      /// Resets the scroll status of the scrollbar.
      /// </summary>
      private void myDoResetScrollStatus()
      {
         // get current mouse position
         var pos = PointToClient(Cursor.Position);

         // set appearance of thumb
         myThumbState = myThumbRectangle.Contains(pos) ? InnerStateEnum.Hot : InnerStateEnum.Normal;
         myBottomArrowClicked = myBottomBarClicked = myTopArrowClicked = myTopBarClicked = false;
         myDoStopTimer();
         Refresh();
      }

      /// <summary>
      /// Calculates the new value of the scrollbar.
      /// </summary>
      /// <param name="smallIncrement">true for a small change, false otherwise.</param>
      /// <param name="up">true for up movement, false otherwise.</param>
      /// <returns>The new scrollbar value.</returns>
      private int myDoGetValue(bool smallIncrement, bool up)
      {
         int newValue;

         // calculate the new value of the scrollbar
         // with checking if new value is in bounds (min/max)
         if (up)
         {
            newValue = myValue - (smallIncrement ? mySmallChange : myLargeChange);

            if (newValue < myMinimum)
            {
               newValue = myMinimum;
            }
         }
         else
         {
            newValue = myValue + (smallIncrement ? mySmallChange : myLargeChange);

            if (newValue > myMaximum)
            {
               newValue = myMaximum;
            }
         }

         return newValue;
      }

      /// <summary>
      /// Calculates the new thumb position.
      /// </summary>
      /// <returns>The new thumb position.</returns>
      private int myDoGetThumbPosition()
      {
         int pix_rng;
         int arr_siz;

         if (myOrientation == ExtendedScrollBarOrientationEnum.Vertical)
         {
            pix_rng = Height - (2 * myArrowHeight) - myThumbHeight;
            arr_siz = myArrowHeight;
         }
         else
         {
            pix_rng = Width - (2 * myArrowWidth) - myThumbWidth;
            arr_siz = myArrowWidth;
         }

         var rea_rng = myMaximum - myMinimum;
         var prc = 0f;

         if (rea_rng != 0) { prc = ((float)myValue - (float)myMinimum) / (float)rea_rng; }

         return Math.Max(myThumbTopLimit, Math.Min(myThumbBottomLimitTop, Convert.ToInt32((prc * pix_rng) + arr_siz)));
      }

      /// <summary>
      /// Calculates the height of the thumb.
      /// </summary>
      /// <returns>The height of the thumb.</returns>
      private int myDoGetThumbSize()
      {
         var trk_siz = myOrientation == ExtendedScrollBarOrientationEnum.Vertical ?
            Height - (2 * myArrowHeight) :
            Width - (2 * myArrowWidth);

         if (myMaximum == 0 || myLargeChange == 0) { return trk_siz; }
         else
         {
            var thm_siz = ((float)myLargeChange * (float)trk_siz) / (float)myMaximum;

            return Convert.ToInt32(Math.Min((float)trk_siz, Math.Max(thm_siz, 10f)));
         }
      }

      /// <summary>
      /// Enables the timer.
      /// </summary>
      private void myDoEnableTimer()
      {
         // if timer is not already enabled - enable it
         if (!myProgressTimer.Enabled)
         {
            myProgressTimer.Interval = 600;
            myProgressTimer.Start();
         }
         else
         {
            // if already enabled, change tick time
            myProgressTimer.Interval = 10;
         }
      }

      /// <summary>
      /// Stops the progress timer.
      /// </summary>
      private void myDoStopTimer() => myProgressTimer.Stop();

      /// <summary>
      /// Changes the position of the thumb.
      /// </summary>
      /// <param name="position">The new position.</param>
      private void myDoChangeThumbPosition(int position)
      {
         if (myOrientation == ExtendedScrollBarOrientationEnum.Vertical) { myThumbRectangle.Y = position; }
         else { myThumbRectangle.X = position; }
      }

      /// <summary>
      /// Controls the movement of the thumb.
      /// </summary>
      /// <param name="enableTimer">true for enabling the timer, false otherwise.</param>
      private void myDoProgressThumb(bool enableTimer)
      {
         var scrollOldValue = myValue;
         var typ = ScrollEventType.First;
         int thumbSize;
         int thumbPos;

         if (myOrientation == ExtendedScrollBarOrientationEnum.Vertical)
         {
            thumbPos = myThumbRectangle.Y;
            thumbSize = myThumbRectangle.Height;
         }
         else
         {
            thumbPos = myThumbRectangle.X;
            thumbSize = myThumbRectangle.Width;
         }

         // arrow down or shaft down clicked
         if (myBottomArrowClicked || (myBottomBarClicked && (thumbPos + thumbSize) < myTrackPosition))
         {
            typ = myBottomArrowClicked ? ScrollEventType.SmallIncrement : ScrollEventType.LargeIncrement;
            myValue = myDoGetValue(myBottomArrowClicked, false);

            if (myValue == myMaximum)
            {
               myDoChangeThumbPosition(myThumbBottomLimitTop);
               typ = ScrollEventType.Last;
            }
            else { myDoChangeThumbPosition(Math.Min(myThumbBottomLimitTop, myDoGetThumbPosition())); }
         }
         else if (myTopArrowClicked || (myTopBarClicked && thumbPos > myTrackPosition))
         {
            typ = myTopArrowClicked ? ScrollEventType.SmallDecrement : ScrollEventType.LargeDecrement;

            // arrow up or shaft up clicked
            myValue = myDoGetValue(myTopArrowClicked, true);

            if (myValue == myMinimum)
            {
               myDoChangeThumbPosition(myThumbTopLimit);
               typ = ScrollEventType.First;
            }
            else { myDoChangeThumbPosition(Math.Max(myThumbTopLimit, myDoGetThumbPosition())); }
         }
         else if (!((myTopArrowClicked && thumbPos == myThumbTopLimit) || (myBottomArrowClicked && thumbPos == myThumbBottomLimitTop)))
         {
            myDoResetScrollStatus();

            return;
         }

         if (scrollOldValue != myValue)
         {
            OnScroll(new ScrollEventArgs(typ, scrollOldValue, myValue, myScrollOrientation));

            Invalidate(myChannelRectangle);

            if (enableTimer) { myDoEnableTimer(); }
         }
         else
         {
            if (myTopArrowClicked) { typ = ScrollEventType.SmallDecrement; }
            else if (myBottomArrowClicked) { typ = ScrollEventType.SmallIncrement; }

            OnScroll(new ScrollEventArgs(typ, myValue));
         }
      }

      /// <summary>
      /// Changes the displayed text of the context menu items dependent of the current <see cref="ExtendedScrollBarOrientationEnum"/>.
      /// </summary>
      private void myDoChangeContextMenuItems()
      {
         if (myOrientation == ExtendedScrollBarOrientationEnum.Vertical)
         {
            MenuItemTop.Text = "Top";
            MenuItemBottom.Text = "Bottom";
            MenuItemLargeDown.Text = "Page down";
            MenuItemLargeUp.Text = "Page up";
            MenuItemSmallDown.Text = "Scroll down";
            MenuItemSmallUp.Text = "Scroll up";
            CtrlTsmiScrollHere.Text = "Scroll here";
         }
         else
         {
            MenuItemTop.Text = "Left";
            MenuItemBottom.Text = "Right";
            MenuItemLargeDown.Text = "Page left";
            MenuItemLargeUp.Text = "Page right";
            MenuItemSmallDown.Text = "Scroll right";
            MenuItemSmallUp.Text = "Scroll left";
            CtrlTsmiScrollHere.Text = "Scroll here";
         }
      }

      private Color myDoGetGripColor()
      {
         switch (myThumbState)
         {
            case InnerStateEnum.Normal:
            case InnerStateEnum.Disabled:
               return PpGripColor;

            case InnerStateEnum.Hot:
            case InnerStateEnum.Active:
            case InnerStateEnum.Pressed:
               return PpGripActiveColor;

            default: throw new Crash();
         }
      }
      #endregion

      #region context menu methods

      /// <summary>
      /// Initializes the context menu.
      /// </summary>

      /// <summary>
      /// Context menu handler.
      /// </summary>
      /// <param name="sender">The sender.</param>
      /// <param name="e">The event arguments.</param>
      private void myDoScrollHereClick(object? sender, EventArgs e)
      {
         int thm_siz, thm_pos, arr_siz, siz;

         if (myOrientation == ExtendedScrollBarOrientationEnum.Vertical)
         {
            thm_siz = myThumbHeight;
            arr_siz = myArrowHeight;
            siz = Height;

            myDoChangeThumbPosition(Math.Max(myThumbTopLimit, Math.Min(myThumbBottomLimitTop, myTrackPosition - (myThumbRectangle.Height / 2))));

            thm_pos = myThumbRectangle.Y;
         }
         else
         {
            thm_siz = myThumbWidth;
            arr_siz = myArrowWidth;
            siz = Width;

            myDoChangeThumbPosition(Math.Max(myThumbTopLimit, Math.Min(myThumbBottomLimitTop, myTrackPosition - (myThumbRectangle.Width / 2))));

            thm_pos = myThumbRectangle.X;
         }

         var pix_rng = siz - (2 * arr_siz) - thm_siz;
         var prc = 0f;

         if (pix_rng != 0) { prc = (float)(thm_pos - arr_siz) / (float)pix_rng; }

         var old_val = myValue;

         myValue = Convert.ToInt32((prc * (myMaximum - myMinimum)) + myMinimum);
         OnScroll(new ScrollEventArgs(ScrollEventType.ThumbPosition, old_val, myValue, myScrollOrientation));
         Refresh();
      }

      /// <summary>
      /// Context menu handler.
      /// </summary>
      /// <param name="sender">The sender.</param>
      /// <param name="e">The event arguments.</param>
      private void myDoTopClick(object? sender, EventArgs e) => Value = myMinimum;

      /// <summary>
      /// Context menu handler.
      /// </summary>
      /// <param name="sender">The sender.</param>
      /// <param name="e">The event arguments.</param>
      private void myDoBottomClick(object? sender, EventArgs e) => Value = myMaximum;

      /// <summary>
      /// Context menu handler.
      /// </summary>
      /// <param name="sender">The sender.</param>
      /// <param name="e">The event arguments.</param>
      private void myDoLargeUpClick(object? sender, EventArgs e) => Value = myDoGetValue(false, true);

      /// <summary>
      /// Context menu handler.
      /// </summary>
      /// <param name="sender">The sender.</param>
      /// <param name="e">The event arguments.</param>
      private void myDoLargeDownClick(object? sender, EventArgs e) => Value = myDoGetValue(false, false);

      /// <summary>
      /// Context menu handler.
      /// </summary>
      /// <param name="sender">The sender.</param>
      /// <param name="e">The event arguments.</param>
      private void myDoSmallUpClick(object? sender, EventArgs e) => Value = myDoGetValue(true, true);

      /// <summary>
      /// Context menu handler.
      /// </summary>
      /// <param name="sender">The sender.</param>
      /// <param name="e">The event arguments.</param>
      private void myDoSmallDownClick(object? sender, EventArgs e) => Value = myDoGetValue(true, false);

      #endregion

      #endregion
   }
}