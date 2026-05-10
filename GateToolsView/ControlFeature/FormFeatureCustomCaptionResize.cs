using Gate.Tools;
using Gate.ToolsView.ControlFeature.Extensions;
using Gate.ToolsView.ControlObserve;
using Gate.ToolsView.Extensions;
using Gate.ToolsView.Native;
using static Gate.ToolsView.ControlFeature.CtrlFeatureMessageTransparent;

namespace Gate.ToolsView.ControlFeature
{
   /// <summary>
   /// <br>Add the capability to define a form without standard caption, but with the capability of being resized and tracked(not possible if you set FormBorderStyle != Sizable.</br>
   /// <br>User can add a custom caption control or choose to leave the feature to paint it.</br>
   /// <br>Differently from standard caption, user can add controls to caption area.</br>
   /// </summary>
   public class FormFeatureCustomCaptionResize : CtrlFeatureByWndHook.Specialized<Form>
   {
      /// <summary>
      /// State of the form regarding resize and moving.
      /// </summary>
      public enum TrackState
      {
         /// <summary>
         /// Form is neither moving nor resizing.
         /// </summary>
         normal = 0,

         /// <summary>
         /// Form being resized.
         /// </summary>
         resize,

         /// <summary>
         /// Form is moving.
         /// </summary>
         moving
      }

      /// <summary>
      /// If caption height is too small, form is not resized from top in order to have the possibility to track the form from caption.
      /// </summary>
      private const int RESIZE_STRIPE_MARGIN = 10;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="args"></param>
      public delegate void OnStateChangeHandler(object? sender, StateChangedArgs args);

      /// <summary>
      /// 
      /// </summary>
      public event OnStateChangeHandler? OnStateChanged;

      private readonly ControlObservableChild myObservable = new ControlObservableChild();
      private Color myCaptionColor = Color.Blue;
      private Color myBackColor = Color.White;
      private Brush? myBrushCaption;
      private Brush? myBrushBackground;
      private int myResizeStripe = 5;
      private TrackState myState;
      private TrackState myStateTestResult = TrackState.normal;
      private int myCaptionHeight = 30;
      private bool myIsAeroSnapEnable = true;
      private Control? myCaptionControl = null;
      private Control? myFrameControl = null;
      private bool myIsCaptionControlLayoutedByFeature = true;
      private bool myIsInTaskBar = true;

      /// <summary>
      /// Constructor.
      /// </summary>
      public FormFeatureCustomCaptionResize()
      {
         myObservable.OnControlAdded += MyObservable_OnControlAdded;
         myObservable.OnControlRemoved += MyObservable_OnControlRemoved;
      }

      /// <summary>
      /// 
      /// </summary>
      public class StateChangedArgs
      {
         public StateChangedArgs(TrackState oldState, TrackState newState, bool isByEscape)
         {
            OldState = oldState;
            NewState = newState;
            IsByEscape = isByEscape;
         }

         public TrackState OldState { get; }
         public TrackState NewState { get; }
         public bool IsByEscape { get; }
      }

      private class InnerTransparentCheckHandler : TransparentToMsgCheckHandler
      {
         public InnerTransparentCheckHandler(FormFeatureCustomCaptionResize formFeature) => FormFeature = formFeature;

         public FormFeatureCustomCaptionResize FormFeature { get; }

         public unsafe override bool IsTransparent(
            int screenX, int screenY, CtrlFeatureMessageTransparent featureTransparent)
         {
            var cap_ctr = FormFeature.CustomCaptionControl;
            var bnd_ctr = featureTransparent.BoundControl ?? throw new Crash();

            if (bnd_ctr == cap_ctr) { return true; }//client area of caption control always transparent
            else if (bnd_ctr.MthGetAnchestors().Contains(cap_ctr)) { return false; }//client area of child and child of child always opaque
            else
            {
               var frm = FormFeature.BoundControl;
               var res_str = FormFeature.myResizeStripe;

               //rect inside resize stripe(check is not inside)
               var rc_res_str_int =
                  FormFeature.BoundControl.
                  RectangleToScreen(new Rectangle(res_str, res_str, frm.Width - 2 * res_str, frm.Height - 2 * res_str));

               return
                  FormFeature.CaptionScreenRectangle.Contains(screenX, screenY) ||
                  !rc_res_str_int.Contains(screenX, screenY);
            }
         }
      }

      /// <summary>
      /// State of form ie <see cref="TrackState.normal"/>,<see cref="TrackState.moving"/>,<see cref="TrackState.resize"/>.
      /// </summary>
      public TrackState State
      {
         get => myState;

         private set
         {
            if (myState != value)
            {
               var is_by_esc = value == TrackState.normal && (Control.MouseButtons & MouseButtons.Left) != 0;
               var args = new StateChangedArgs(myState, value, is_by_esc);

               myState = value;
               myActionOnStateChanged(this, args);
            }
         }
      }

      /// <summary>
      /// Caption rect in screen coordinates ie custom control screen rect otw if it is null (0,0,Form.Width,CaptionHeight)
      /// </summary>
      public Rectangle CaptionScreenRectangle => CustomCaptionControl == null ?
         BoundControlParentForm.RectangleToScreen(new Rectangle(0, 0, BoundControl.Width, CaptionHeight)) :
         CustomCaptionControl.Parent?.RectangleToScreen(CustomCaptionControl.ClientRectangle) ?? Rectangle.Empty;

      /// <summary>
      /// True if it's true IsCaptionControlLayoutedByFeature and caption control has form as DIRECT parent.
      /// </summary>
      public bool IsCaptionControlLayoutedByFeatureEffective =>
         CustomCaptionControl != null && IsCaptionControlLayoutedByFeature && CustomCaptionControl.Parent == BoundControlParentForm;

      /// <summary>
      /// <br> Whether caption control is controlled by feature layout, otherwise form containing caption </br>
      /// <br> control layout position. In this case caption area is controlled by caption control.</br>
      /// <br> If caption control is not direct child of form, prop value is ignored and control is NOT feature-layouted. </br>
      /// </summary>
      public bool IsCaptionControlLayoutedByFeature
      {
         get => myIsCaptionControlLayoutedByFeature;
         set
         {
            myIsCaptionControlLayoutedByFeature = value;
            BoundControl.Refresh();
         }
      }

      /// <summary>
      /// <br>  the caption control.</br> 
      /// <br> Custom caption define area where the form can be tracked.</br>
      /// <br> Can be placed not necessary to form top.</br>
      /// <br> It's area is event transparent (uses <seealso cref="Gate.ToolsView.ControlFeature.CtrlFeatureMessageTransparent"/>) </br>
      /// <br> but all controls inside aren't  </br>
      /// </summary>
      public Control? CustomCaptionControl
      {
         get => myCaptionControl;

         set
         {
            if ((myCaptionControl = value) != null)
            {
               if (value?.Parent == null) { BoundControl.Controls.Add(value); }

               BoundControl.Refresh();
            }

            BoundControl.PerformLayout();
         }
      }

      /// <summary>
      /// Optional control used as frame (layout docks it under caption).
      /// </summary>
      public Control? FrameControl
      {
         get => myFrameControl;
         set
         {
            if ((myFrameControl = value) != null)
            {
               if (value?.Parent == null) { BoundControl.Controls.Add(value); }
               else if (value.Parent != BoundControl)
               {
                  throw new Crash($"Control {value.Name} of type{value.GetType()} shall not have a parent different from Bound form.");
               }
            }

            BoundControl.PerformLayout();
         }
      }

      /// <summary>
      /// Caption background color (remember caption control is made transparent either to color or windows msgs.
      /// </summary>
      public Color CaptionColor
      {
         get => myCaptionColor;

         set
         {
            myCaptionColor = value;
            myMakeBrushes();
         }
      }

      /// <summary>
      /// Height of caption. This property is significant, only if IsCaptionControlLayoutedByFeatureEffective is true or not CustomCaption control is defined.
      /// </summary>
      public int CaptionHeight
      {
         get => myCaptionHeight;
         set
         {
            myCaptionHeight = value;
            BoundControl.Refresh();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public int ResizeStripe { get => myResizeStripe; set => myResizeStripe = value; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsAeroSnapEnable
      {
         get => myIsAeroSnapEnable;
         set
         {
            myIsAeroSnapEnable = value;

            if (BoundControl is Form frm) { frm.FormBorderStyle = value ? FormBorderStyle.Sizable : FormBorderStyle.None; }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public bool IsInTaskBar
      {
         get => myIsInTaskBar;

         set
         {
            myIsInTaskBar = value;

            if (BoundControlParentForm != null && !BoundControlParentForm.IsDisposed)
            {
               if (BoundControlParentForm.Visible) { mySetIsInTaskBar(myIsInTaskBar); }
               else { BoundControlParentForm.Shown += (s, e) => mySetIsInTaskBar(myIsInTaskBar); }
            }
         }
      }

      /// <summary>
      /// Starts tracking the form on CaptionControl.
      /// </summary>
      /// <param name="captionRelLocation">Relative to caption location where mouse points while tracking (if null or outside caption bounds center caption is set).</param>
      public void TrackStart(Point? captionRelLocation = null)
      {
         if (State == TrackState.normal && Control.MouseButtons == MouseButtons.Left)
         {
            var cap_rc = CustomCaptionControl != null ? CustomCaptionControl.ClientRectangle : new Rectangle(0, 0, BoundControl.Width, CaptionHeight);

            //effective caption relative to 
            var cap_rel_pos =
               captionRelLocation != null && cap_rc.Contains(captionRelLocation.Value) ?
                  captionRelLocation.Value : new Point(cap_rc.Width / 2, cap_rc.Height / 2);

            var frm_rel_pos = CustomCaptionControl != null ?
               BoundControlParentForm.PointToClient(CustomCaptionControl.PointToScreen(cap_rel_pos)) :
               cap_rel_pos;

            var cp = Cursor.Position;

            BoundControlParentForm.Location = new Point(cp.X - frm_rel_pos.X, cp.Y - frm_rel_pos.Y);
            BoundControlParentForm.Activate();
            BoundControlParentForm.BringToFront();
            BoundControlParentForm.Focus();
            MouseNativeHelper.SetCapture(BoundControlParentForm.Handle);
            MouseNativeHelper.SimulateMouseEvent(MouseNativeHelper.MouseEventFlags.LeftUp);
            MouseNativeHelper.SimulateMouseEvent(MouseNativeHelper.MouseEventFlags.LeftDown);
         }
      }

      protected override void myOnControlAssociate(Control control)
      {
         myObservable.RootControl = control;
         myMakeBrushes();
         control.BackColorChanged += Control_BackColorChanged;
         control.Paint += Control_Paint;
         control.Layout += Control_Layout1;
         IsAeroSnapEnable = IsAeroSnapEnable;//applies property
         IsInTaskBar = IsInTaskBar;//applies property
         control.PerformLayout();
      }

      protected override void myOnControlDeassociate(Control control)
      {
         myObservable.RootControl = null;
         control.Paint -= Control_Paint;
         control.Layout -= Control_Layout1;
         control.BackColorChanged -= Control_BackColorChanged;
      }

      protected unsafe override bool myWndProc(ref Message msg)
      {
         switch ((WinMsgEnum)msg.Msg)
         {
            case WinMsgEnum.WM_NCLBUTTONDOWN:
               State = myStateTestResult;
               break;

            case WinMsgEnum.WM_NCHITTEST:
               var px = (int)msg.LParam;
               var scr_pnt = new Point(((ushort*)&px)[0], ((ushort*)&px)[1]);//mouse location(screen coordinates)
               var r_x = ((ushort*)&px)[0] - BoundControl.Left;//mouse x(form relative)
               var r_y = ((ushort*)&px)[1] - BoundControl.Top;//mouse y(form relative)
               var cr = CaptionScreenRectangle;

               //if bottom of caption rectangle smaller than resize stripe
               //check for caption has the precedence with respect to resize check
               if (cr.Bottom < myResizeStripe + RESIZE_STRIPE_MARGIN && cr.Contains(scr_pnt))
               {
                  msg.Result = (IntPtr)HitTestValuesEnum.HTCAPTION;
                  myStateTestResult = TrackState.moving;

                  return false;
               }
               else if (r_y <= myResizeStripe)
               {
                  if (r_x <= myResizeStripe) { msg.Result = (IntPtr)HitTestValuesEnum.HTTOPLEFT; }
                  else if (r_x >= BoundControl.Width - myResizeStripe) { msg.Result = (IntPtr)HitTestValuesEnum.HTTOPRIGHT; }
                  else { msg.Result = (IntPtr)HitTestValuesEnum.HTTOP; }

                  myStateTestResult = TrackState.resize;

                  return false;
               }
               else if (r_y >= BoundControl.Height - myResizeStripe)
               {
                  if (r_x <= myResizeStripe) { msg.Result = (IntPtr)HitTestValuesEnum.HTBOTTOMLEFT; }
                  else if (r_x >= BoundControl.Width - myResizeStripe) { msg.Result = (IntPtr)HitTestValuesEnum.HTBOTTOMRIGHT; }
                  else { msg.Result = (IntPtr)HitTestValuesEnum.HTBOTTOM; }

                  myStateTestResult = TrackState.resize;

                  return false;
               }
               else if (r_x <= myResizeStripe)
               {
                  msg.Result = (IntPtr)HitTestValuesEnum.HTLEFT;
                  myStateTestResult = TrackState.resize;

                  return false;
               }
               else if (r_x >= BoundControl.Width - myResizeStripe)
               {
                  msg.Result = (IntPtr)HitTestValuesEnum.HTRIGHT;
                  myStateTestResult = TrackState.resize;

                  return false;
               }
               else if (cr.Contains(scr_pnt))
               {
                  msg.Result = (IntPtr)HitTestValuesEnum.HTCAPTION;
                  myStateTestResult = TrackState.moving;

                  return false;
               }

               myStateTestResult = TrackState.normal;
               break;

            case WinMsgEnum.WM_NCCALCSIZE:
               msg.Result = (IntPtr)0;

               return false;

            case WinMsgEnum.WM_EXITSIZEMOVE:
               State = TrackState.normal;
               break;
         }

         return true;
      }

      protected virtual void myActionOnStateChanged(object? sender, StateChangedArgs args) => OnStateChanged?.Invoke(sender, args);

      private void mySetIsInTaskBar(bool isInTaskbar)
      {
         var hnd = BoundControl.Handle;
         var sty = NativeMethods.GetWindowLong(hnd, WindowLong.GWL_STYLE).ToInt32();

         if (isInTaskbar)
         {
            sty &= ~(int)WsExtStyles.WS_EX_TOOLWINDOW;
            sty |= (int)WsExtStyles.WS_EX_APPWINDOW;
         }
         else
         {
            sty |= (int)WsExtStyles.WS_EX_TOOLWINDOW;
            sty &= ~(int)WsExtStyles.WS_EX_APPWINDOW;
         }

         NativeMethods.ShowWindow(hnd, CmdShowEnum.HIDE); // hide the window
         NativeMethods.SetWindowLongPtr(hnd, WindowLong.GWL_EXSTYLE, (IntPtr)sty); // set the style
         NativeMethods.ShowWindow(hnd, CmdShowEnum.SHOW); // show the window for the new style to come into effect
      }

      private void myMakeBrushes()
      {
         var cap_pen = new Pen(CaptionColor);
         var bck_pen = new Pen(BoundControl.BackColor);

         myBrushCaption = cap_pen.Brush;
         myBrushBackground = bck_pen.Brush;
         BoundControl.Refresh();
      }

      private void Control_Paint(object? sender, PaintEventArgs e)
      {
         if (myBrushCaption != null)
         {
            e.Graphics.FillRectangle(myBrushCaption, 0, 0, BoundControl.Width, CaptionHeight);
         }

         if (myBrushBackground!= null)
         {
            e.Graphics.FillRectangle(myBrushBackground, 0, CaptionHeight, BoundControl.Width, BoundControl.Height - CaptionHeight);
         }
      }

      private void MyObservable_OnControlRemoved(Control control)
      {
         if (control == CustomCaptionControl) { CustomCaptionControl = null; }
         if (control == FrameControl) { FrameControl = null; }

         control.RemoveFeature<CtrlFeatureMessageTransparent>();
      }

      private void MyObservable_OnControlAdded(Control control)
      {
         if (!(control is Form))
         {
            var fea = control.AddFeature<CtrlFeatureMessageTransparent>();

            fea.TransparentToMsgCheck = new InnerTransparentCheckHandler(this);
         }
      }

      private void Control_Layout1(object? sender, LayoutEventArgs e)
      {
         if (IsCaptionControlLayoutedByFeatureEffective && CustomCaptionControl != null)
         {
            CustomCaptionControl.Location = new Point();
            CustomCaptionControl.Size = new Size(BoundControl.Width, CaptionHeight);
         }

         if (FrameControl != null)
         {
            FrameControl.Location = new Point(0, CaptionHeight);
            FrameControl.Size = new Size(BoundControl.Width, BoundControl.Height - CaptionHeight);
         }
      }

      private void Control_BackColorChanged(object? sender, EventArgs e) => myMakeBrushes();
   }
}
