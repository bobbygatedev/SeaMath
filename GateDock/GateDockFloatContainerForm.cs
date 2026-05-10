using Gate.Dock.DockTab;
using Gate.Dock.DockWidget;
using Gate.Tools;
using Gate.ToolsView.ControlFeature;
using Gate.ToolsView.ControlFeature.Extensions;
using Gate.ToolsView.ControlObserve;
using Gate.ToolsView.Extended;
using Gate.ToolsView.Extensions;
using System.ComponentModel;
using static Gate.ToolsView.ControlFeature.FormFeatureCustomCaptionResize;

namespace Gate.Dock
{
   /// <summary>
   /// Form for containing dock-control when in state float.
   /// </summary>
   public partial class GateDockFloatContainerForm : Form
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      public delegate void OnTrackStartHandler(object? sender);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="isByEscape"></param>
      public delegate void OnTrackEndHandler(object? sender, bool isByEscape);

      /// <summary>
      /// 
      /// </summary>
      public event OnTrackStartHandler? OnTrackingStart;

      /// <summary>
      /// 
      /// </summary>
      public event OnTrackEndHandler? OnTrackingEnd;

      /// <summary>
      /// 
      /// </summary>
      private FormFeatureCustomCaptionResize myFormFeatureNoBorderResize;
      private CtrlFeatureBorder myFeatureBorder;

      /// <summary>
      /// 
      /// </summary>
      private ControlObservableChild myObservableChild;

      /// <summary>
      /// Constructor.
      /// </summary>
      public GateDockFloatContainerForm()
      {
         myFormFeatureNoBorderResize = this.AddFeature<FormFeatureCustomCaptionResize>();
         myFeatureBorder = this.AddFeature<CtrlFeatureBorder>();
         myObservableChild = new ControlObservableChild(this);
         myObservableChild.OnControlAdded += MyObservableChild_OnControlAdded;
         myObservableChild.OnControlRemoved += MyObservableChild_OnControlRemoved;

         InitializeComponent();

         myFormFeatureNoBorderResize.OnStateChanged += PpFeature_OnStateChanged;
         PpBorderWidth = 1;
      }

      /// <summary>
      /// Track state of form.
      /// </summary>
      public TrackState PpTrackState => myFormFeatureNoBorderResize.State;

      /// <summary>
      /// Border color.
      /// </summary>
      public Color PpBorderColor
      {
         get => myFeatureBorder.BorderColor;
         set => myFeatureBorder.BorderColor = value;
      }

      /// <summary>
      ///  width in pixel of border.
      /// </summary>
      public float PpBorderWidth
      {
         get => myFeatureBorder.BorderWidth;
         set => myFeatureBorder.BorderWidth = value;
      }

      /// <summary>
      /// Main form.
      /// </summary>
      public GateDockMainForm? PpMainFrm
      {
         get
         {
            if (PpAssociatedWidget != null) { return PpAssociatedWidget.PpMainFrm; }
            else if (PpAssociatedTabCtrl != null) { return PpAssociatedTabCtrl.PpMainFrm; }
            else { return null; }
         }
      }

      /// <summary>
      /// Associated widget if form is associated to widget.
      /// </summary>
      public GateDockWidgetCtrl? PpAssociatedWidget => this.MthGetNephew<GateDockWidgetCtrl>();

      /// <summary>
      /// 
      /// </summary>
      public GateDockTabCtrl? PpAssociatedTabCtrl => this.MthGetNephew<GateDockTabCtrl>();

      /// <summary>
      /// 
      /// </summary>
      public Control? PpAssociatedControl => PpAssociatedWidget ?? PpAssociatedTabCtrl as Control;

      /// <summary>
      /// 
      /// </summary>
      public bool PpIsInTaskBar { get => myFormFeatureNoBorderResize.IsInTaskBar; set => myFormFeatureNoBorderResize.IsInTaskBar = value; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="captionRelLocation">Relative to caption location where mouse points while tracking (if null or outside caption bounds center caption is set).</param>
      public void MthStartTracking(Point? captionRelLocation = null) => myFormFeatureNoBorderResize.TrackStart(captionRelLocation);

      protected override void OnClosing(CancelEventArgs e)
      {
         if (PpAssociatedWidget != null) { PpAssociatedWidget.Parent = null; }

         Owner = null;
         base.OnClosing(e);
      }

      protected virtual void myDoActionOnTrackingEnd(object? sender, bool isByEscape) => OnTrackingEnd?.Invoke(this, isByEscape);

      protected virtual void myDoActionOnTrackingStart(object? sender) => OnTrackingStart?.Invoke(this);

      protected override bool ProcessCmdKey(ref Message msg, Keys keyData) =>
         PpMainFrm != null && PpMainFrm.PpCmdMainMenu != null ?
            PpMainFrm.PpCmdMainMenu.HandleKeyForShortcuts(keyData, false) :
            base.ProcessCmdKey(ref msg, keyData);

      private void PpFeature_OnStateChanged(object? sender, StateChangedArgs args)
      {
         switch (args.NewState)
         {
            case TrackState.normal:
               if (args.OldState == TrackState.moving) { myDoActionOnTrackingEnd(this, args.IsByEscape); }
               break;

            case TrackState.resize: break;
            case TrackState.moving:
               myDoActionOnTrackingStart(this);
               break;

            default: throw new Crash();
         }
      }

      private void MyObservableChild_OnControlRemoved(Control control)
      {
         if (control is CustomCaptionCtrl cap) { cap.PpFormBound = null; }
      }

      private void MyObservableChild_OnControlAdded(Control control)
      {
         if (control is GateDockWidgetCtrl) { myFormFeatureNoBorderResize.IsAeroSnapEnable = false; }
         else if (control is CustomCaptionCtrl cap) { cap.PpFormBound = this; }

         if (PpMainFrm != null) { Icon = PpMainFrm.Icon; }
      }
   }
}
