using Gate.Dock.DockFactories;
using Gate.Dock.DockSkin;
using Gate.Dock.DockTab;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.ControlFeature;
using Gate.ToolsView.ControlFeature.Extensions;
using Gate.ToolsView.ControlObserve;
using Gate.ToolsView.Dockable;
using Gate.ToolsView.Extended;
using Gate.ToolsView.Extensions;
using Gate.ToolsView.MenuCommand;
using Gate.ToolsView.Native;
using static Gate.ToolsView.Extended.CustomCaptionCtrl;

namespace Gate.Dock.DockWidget
{
   /// <summary>
   /// Base widget control class.
   /// </summary>
   public partial class GateDockWidgetCtrl : UserControl, IGateDockCtrlWithSkin
   {
      public delegate void OnChanginMainFormHandler(object? sender, GateDockMainForm? mainForm);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="isSelected"></param>
      public delegate void OnSelectedChangedHandler(object? sender, bool isSelected);

      /// <summary>
      /// 
      /// </summary>
      public event OnSelectedChangedHandler? OnSelectedChanged;

      /// <summary>
      /// 
      /// </summary>
      public event EventHandler? OnTitleChange;

      /// <summary>
      /// 
      /// </summary>
      public event OnChanginMainFormHandler? OnChangingMainForm;

      private ControlObservableFocused myControlFocusedObservable;
      private ControlObservableParent myParentObservable;
      private Color myBorderColor = GateDockSkin.DefaultValues.BorderColor;
      private int myBorderPixels;
      private GateDockMainForm? myMainFrm = null;
      private GateDockWidgetSkinDispacther? mySkinChildCtrlDispacther = null;
      private bool myIsSelected = false;
      private string myTitle = "";

      /// <summary>
      /// Constructor.
      /// </summary>
      public GateDockWidgetCtrl()
      {
         InitializeComponent();

         PpSkinChildCtrlDispacther = new GateDockWidgetSkinDispacther(this);
         myControlFocusedObservable = new ControlObservableFocused(this);
         myControlFocusedObservable.OnIsFocusedChanged += MyControlFocusedObservable_OnIsFocusedChanged;
         myParentObservable = new ControlObservableParent(this);
         myParentObservable.OnControlAdded += MyParentObservable_OnControlAddedOrRemoved;
         myParentObservable.OnControlRemoved += MyParentObservable_OnControlAddedOrRemoved;
         PpBorderWidth = GateDockSkin.Constants.WIDGET_DOCUTAB_BORDER;
         CtrlWidgetCaption.Height = GateDockSkin.Constants.WIDGET_CAPTION_HEIGHT;

         var fea = (this.MthGetNephew<CustomCaptionCtrl>()?.AddFeature<CtrlFeatureTrackStartSense>()).NnOrCrash();

         fea.OnStartingDragging += Fea_OnAskForDragging;
      }

      /// <summary>
      /// 
      /// </summary>
      public Control? PpBody
      {
         get => CtrlWidgetLayout?.PpBody;
         set
         {
            if (CtrlWidgetLayout != null)
            {
               CtrlWidgetLayout.PpBody = value;
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public GateDockWidgetSkinDispacther? PpSkinChildCtrlDispacther
      {
         get => mySkinChildCtrlDispacther;
         set
         {
            if (value != null) { mySkinChildCtrlDispacther = value; }
            else { MessageBox.Show(string.Format("{0}.PpSkinChildCtrlDispacther can't be null", GetType().Name)); }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public DockableAreaCtrlSlotAnchorModeEnum PpAnchorMode
      {
         get
         {
            switch (PpDockState)
            {
               case GateDockWidgetStateFlags.invisible: return DockableAreaCtrlSlotAnchorModeEnum.none;
               case GateDockWidgetStateFlags.floating: return DockableAreaCtrlSlotAnchorModeEnum.none;

               case GateDockWidgetStateFlags.group_left:
               case GateDockWidgetStateFlags.dock_left:
                  return DockableAreaCtrlSlotAnchorModeEnum.left;

               case GateDockWidgetStateFlags.group_right:
               case GateDockWidgetStateFlags.dock_right:
                  return DockableAreaCtrlSlotAnchorModeEnum.right;

               case GateDockWidgetStateFlags.group_up:
               case GateDockWidgetStateFlags.dock_up:
                  return DockableAreaCtrlSlotAnchorModeEnum.up;

               case GateDockWidgetStateFlags.group_down:
               case GateDockWidgetStateFlags.dock_down:
                  return DockableAreaCtrlSlotAnchorModeEnum.down;

               case GateDockWidgetStateFlags.tabbed: return DockableAreaCtrlSlotAnchorModeEnum.center;

               default: throw new Crash();
            }
         }
      }

      /// <summary>
      ///  
      /// </summary>
      public GateDockMainForm? PpMainFrm
      {
         get => myMainFrm;

         internal set
         {
            if (myMainFrm != value)
            {
               if (myMainFrm != null) { myMainFrm.OnSkinChange -= MyMainFrm_OnSkinChange; }

               if ((myMainFrm = value) != null)
               {
                  if (myMainFrm.PpSkin != null) { PpSkin = myMainFrm.PpSkin; }

                  if (myMainFrm.PpCmdMainMenu != null && myMainFrm.PpCmdMainMenu.CmdContainer != null)
                  {
                     //searches for widget drop down menu
                     var men = myMainFrm.PpCmdMainMenu.CmdContainer.AllMenus.
                        FirstOrDefault(m => m.Id == GateDockWidgetCommandsBuilder.MENU_ID);

                     if (men != null) { CtrlWidgetCaption.PpCmdMenuRef = new CmdMenu.Ref(men, true); }
                  }

                  PpRestoreSize = Size;
                  myMainFrm.OnSkinChange += MyMainFrm_OnSkinChange;
                  PpIsSelected = myControlFocusedObservable.IsSelected;
               }

               myDoIsSelectedEvaluate();
               OnChangingMainForm?.Invoke(this, myMainFrm);
            }
         }
      }

      public Size PpRestoreSize { get; private set; }

      protected override void OnResize(EventArgs e)
      {
         if (myMainFrm?.WindowState != FormWindowState.Minimized) { PpRestoreSize = Size; }

         base.OnResize(e);
      }

      /// <summary>
      /// Is widget is visible independently from dock-state (not necessary equal to Control.Visible)?
      /// </summary>
      public bool PpIsWidgetVisible => ParentForm != null && ParentForm.Visible;

      /// <summary>
      /// 
      /// </summary>
      public bool PpIsCaptionVisible
      {
         get => CtrlWidgetCaption.Visible;

         set
         {
            CtrlWidgetCaption.Visible = value;
            PerformLayout();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public GateDockWidgetGroupCtrl? PpWidgetGroup => this.MthGetAnchestor<GateDockWidgetGroupCtrl>();

      /// <summary>
      /// 
      /// </summary>
      public Point? PpLastFloatLocation { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public Size? PpLastSize { get; private set; }

      /// <summary>
      ///  
      /// </summary>
      public bool PpIsSelected
      {
         get => myIsSelected;

         private set
         {
            if (value != myIsSelected)
            {
               myIsSelected = value;
               OnSelectedChanged?.Invoke(this, value);
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public GateDockTabCtrl? PpContainingDocuTab => this.MthGetAnchestor<GateDockTabCtrl>();

      /// <summary>
      /// 
      /// </summary>
      public Color PpBorderColor
      {
         get => myBorderColor;

         private set
         {
            myBorderColor = value;
            myDoMakeBorder();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public int PpBorderWidth
      {
         get => myBorderPixels;

         private set
         {
            myBorderPixels = value;
            myDoMakeBorder();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public GateDockWidgetStateFlags PpDockState
      {
         get
         {
            var hie = this.MthGetAnchestors();

            if (Parent == null) { return GateDockWidgetStateFlags.invisible; }
            else if (hie.Any(c => c is GateDockWidgetGroupCtrl))
            {
               return
                  GateDockWidgetStateFlags.group |
                  my_GetPosFromAnchor(((GateDockWidgetGroupCtrl)hie.First(c => c is GateDockWidgetGroupCtrl)).PpAnchorMode);
            }
            else if (hie.Any(c => c is GateDockTabCtrl)) { return GateDockWidgetStateFlags.tabbed; }
            else if (ParentForm is GateDockMainForm)//docked
            {
               return
                  GateDockWidgetStateFlags.dock |
                  my_GetPosFromAnchor(((DockableAreaCtrl)hie.First(c => c is DockableAreaCtrl)).MthGetAnchorFromControl(this));
            }
            else if (ParentForm is GateDockFloatContainerForm) { return GateDockWidgetStateFlags.floating; }
            else { return GateDockWidgetStateFlags.invisible; }
         }
      }

      public GateDockWidgetStateFlags PpLastDockState { get; private set; } = GateDockWidgetStateFlags.invisible;

      public GateDockSkin? PpSkin
      {
         get => PpSkinChildCtrlDispacther?.Skin;
         set
         {
            if (PpSkinChildCtrlDispacther != null)
            {
               PpSkinChildCtrlDispacther.Skin = value;
            }
         }
      }

      public GateDockWidgetFactory? PpFactory { get; internal set; }

      public string PpTitle
      {
         get => myTitle;

         set
         {
            myTitle = value ?? "";
            OnTitleChange?.Invoke(this, new EventArgs());

            if (CtrlWidgetCaption != null) { CtrlWidgetCaption.Text = myTitle; }
         }
      }

      public void MthBring2Front()
      {
         switch (PpDockState & (GateDockWidgetStateFlags.dock | GateDockWidgetStateFlags.group | GateDockWidgetStateFlags.floating | GateDockWidgetStateFlags.tabbed))
         {
            case GateDockWidgetStateFlags.dock:
            case GateDockWidgetStateFlags.floating:
               Focus();
               break;

            case GateDockWidgetStateFlags.group:
               PpWidgetGroup?.MthWidgetBring2Front(this);
               break;

            case GateDockWidgetStateFlags.tabbed:
               if (PpContainingDocuTab != null)
               {
                  PpContainingDocuTab.PpTabPageVisible = this;
               }
               break;

            default: throw new Crash();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public void MthShow()
      {
         if (PpMainFrm != null)
         {
            var dck_sta = PpLastDockState;

            if (dck_sta == GateDockWidgetStateFlags.invisible)
            {
               dck_sta = PpFactory != null ? PpFactory.DefaultState : GateDockWidgetStateFlags.floating;
            }

            PpMainFrm.MthWidgetShow(this, dck_sta, PpLastFloatLocation);
         }
      }

      internal void MthSaveLast()
      {
         PpLastSize = Size;

         if ((PpLastDockState = PpDockState) == GateDockWidgetStateFlags.floating)
         {
            PpLastFloatLocation = Parent?.PointToScreen(Location);
            PpLastDockState = PpDockState;
         }
      }

      protected override void OnParentChanged(EventArgs e)
      {
         if (ParentForm is GateDockFloatContainerForm) { ParentForm.FormClosing += (s, e1) => myDoBeforeFloatingCloseAction(); }

         myDoMakeBorder();
         base.OnParentChanged(e);
      }

      private void myDoMakeBorder()
      {
         CtrlPanel.PpBorderWidth = PpBorderWidth;
         CtrlPanel.PpBorderColor = PpBorderColor;
      }

      private void myDoBeforeFloatingCloseAction()
      {
         if (ParentForm is GateDockFloatContainerForm) { ParentForm.Visible = false; }
         Parent = null;
      }

      private void myDoIsSelectedEvaluate()
      {
         //if widget is invisible (not bound to any visible control) sel = false
         if (ParentForm == null) { PpIsSelected = false; }
         else if (NativeMethods.GetActiveWindow() != IntPtr.Zero)//focus to another application
         {
            //if is focused change to false (moving to another control)
            //the selected state is mantained when focus is moving outside application(active window == null)
            PpIsSelected = myControlFocusedObservable.IsSelected;
         }
      }

      private static GateDockWidgetStateFlags my_GetPosFromAnchor(DockableAreaCtrlSlotAnchorModeEnum anchor)
      {
         switch (anchor)
         {
            case DockableAreaCtrlSlotAnchorModeEnum.left: return GateDockWidgetStateFlags.left;
            case DockableAreaCtrlSlotAnchorModeEnum.right: return GateDockWidgetStateFlags.right;
            case DockableAreaCtrlSlotAnchorModeEnum.up: return GateDockWidgetStateFlags.up;
            case DockableAreaCtrlSlotAnchorModeEnum.down: return GateDockWidgetStateFlags.down;
            default: return GateDockWidgetStateFlags.invisible;
         }
      }

      private void Fea_OnAskForDragging(object? sender)
      {
         var pos_rel = CtrlWidgetCaption.PointToClient(Cursor.Position);

         if (PpDockState != GateDockWidgetStateFlags.floating)
         {
            PpMainFrm?.MthWidgetShow(this, GateDockWidgetStateFlags.floating)?.MthStartTracking(pos_rel);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="eventType"></param>
      private void CtrlWidgetCaption_OnDockCaptionEvent(object? sender, EventType eventType)
      {
         if (eventType == EventType.button_close)
         {
            PpMainFrm?.MthWidgetHide(this);
         }
      }

      private void MyMainFrm_OnSkinChange(object? sender, GateDockSkin? skin) => PpSkin = skin;

      private void MyParentObservable_OnControlAddedOrRemoved(Control control) => myDoIsSelectedEvaluate();

      private void MyControlFocusedObservable_OnIsFocusedChanged(
         Control? observable, Control? focusedControl, bool isSelected)
      {
         myDoIsSelectedEvaluate();

         var col_sel = PpSkin != null ? PpSkin.Params.WidgetSelectedColor.Value : GateDockSkin.DefaultValues.WidgetSelectedColor;
         var col_bor = PpSkin != null ? PpSkin.Params.BorderColor.Value : GateDockSkin.DefaultValues.BorderColor;

         PpBorderColor = PpIsSelected ? col_sel : col_bor;
         CtrlWidgetCaption.PpIsWidgetSelected = PpIsSelected;
      }
   }
}
