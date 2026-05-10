using Gate.Dock.DockDocu;
using Gate.Dock.DockSkin;
using Gate.Dock.DockTab;
using Gate.Dock.DockWidget;
using Gate.Tools;
using Gate.Tools.AppParams;
using Gate.Tools.Message;
using Gate.ToolsView.ControlFeature;
using Gate.ToolsView.ControlFeature.Extensions;
using Gate.ToolsView.ControlObserve;
using Gate.ToolsView.Dockable;
using Gate.ToolsView.Extended;
using Gate.ToolsView.Extensions;
using Gate.ToolsView.MenuCommand;
using Gate.ToolsView.MenuExtended;
using Gate.ToolsView.Native;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using Timer = System.Windows.Forms.Timer;

namespace Gate.Dock
{
   /// <summary> 
   ///  --TICKETS
   ///    -review docking in general
   ///       - widget state box not working after migration to .net 8.0
   ///       - widget dragging not working after migration to .net 8.0
   ///       - floating tab tab not working
   ///       - console and message widget shall be accomodated on same group
   ///    - main form state not to be saved when minimized
   /// </summary>
   public partial class GateDockMainForm : Form, IGateDockCtrlWithSkin
   {
      public delegate void OnTabPageHandler(object? sender, GateDockTabPageCtrl? tabPage);

      public delegate void OnMainFromClosingHandler(object? sender, GateDockMainFormClosingEventArgs args);

      public delegate void OnTabPageCurrentChangedHandler(object? sender, Control? newTopLevelPage);

      public delegate void OnTabCtrlEventHandler(object? sender, GateDockTabCtrl? tabCtrl);

      public delegate void OnWidgetSelectedChangeHandler(object? sender, GateDockWidgetCtrl? widget);

      public event OnGateDockSkinChangeHandler? OnSkinChange;
      public event OnTabPageHandler? OnTabPageOpen;
      public event OnTabPageHandler? OnTabPageClosing;
      public event OnTabPageHandler? OnTabPageClose;
      public event OnMainFromClosingHandler? OnMainFormClosing;
      public event OnTabPageCurrentChangedHandler? OnTabPageCurrentChanged;
      public event OnTabCtrlEventHandler? OnTabAdded;
      public event OnTabCtrlEventHandler? OnTabRemoved;
      public event OnTabCtrlEventHandler? OnTabCurrentChanged;
      public event OnWidgetSelectedChangeHandler? OnWidgetSelectedChange;

      private readonly List<GateDockFloatContainerForm> myListFloatContainerForm = new List<GateDockFloatContainerForm>();
      private readonly List<ControlObservableFocused> myListFocusObserver = new List<ControlObservableFocused>();
      private readonly InnerTabPageHandling myTabPageHandling;
      private readonly InnerFloatingMovingHandler myFloatingMovingHandler;
      private SkinChildCtrlDispacther mySkinChildCtrlDispacther;
      private bool myIsCloseActionStarted = false;
      private CmdMainMenu? myCmdMainMenu = new CmdMainMenu("DockMainForm.MainMenu");
      private GateDockTabCtrl? myTabCurrent;
      private Image? myImage;

      /// <summary>
      /// Constructor.
      /// </summary>
      public GateDockMainForm()
      {
         this.AddFeature<FormFeatureCustomCaptionResize>();
         this.AddFeature<CtrlFeatureBorder>();

         myTabPageHandling = new InnerTabPageHandling(this);
         myFloatingMovingHandler = new InnerFloatingMovingHandler(this);
         PpBorderColor = GateDockSkin.DefaultValues.BorderColor;
         ForeColor = GateDockSkin.DefaultValues.ForeColor;
         mySkinChildCtrlDispacther = new SkinChildCtrlDispacther(this);
         AllFocusedFormsObserver = new ControlObservableFocusedAllForms(this);

         InitializeComponent();

         CtrlCaption.PpFormBound = this;
         PpBorderWidth = 1.0f;
         Icon = Icon;
      }

      /// <summary>
      /// 
      /// </summary>
      public class SkinChildCtrlDispacther : GateDockSkinChildCtrlDispatcher
      {
         public SkinChildCtrlDispacther(GateDockMainForm parent) : base(parent) { }
      }

      /// <summary>
      /// 
      /// </summary>
      public ControlObservableFocusedAllForms AllFocusedFormsObserver { get; }

      /// <summary>
      /// 
      /// </summary>
      public string PpAppName { get; set; } = "GateDockRawApp";

      /// <summary>
      /// 
      /// </summary>
      public Color PpBorderColor
      {
         get => this.GetFeature<CtrlFeatureBorder>()?.BorderColor ?? Color.Empty;

         set
         {
            var fea = this.GetFeature<CtrlFeatureBorder>() ?? throw new Crash();

            fea.BorderColor = value;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public float PpBorderWidth
      {
         get => this.GetFeature<CtrlFeatureBorder>()?.BorderWidth ?? 0.0f;
         set
         {
            var fea = this.GetFeature<CtrlFeatureBorder>() ?? throw new Crash();

            fea.BorderWidth = value;
         }
      }

      /// <summary>
      ///  main menu object.
      /// </summary>
      public CmdMainMenu? PpCmdMainMenu
      {
         get => myCmdMainMenu;

         set
         {
            if (myCmdMainMenu != value)
            {
               (this.MthGetNephew<ExtendedMainMenu>() ?? throw new Crash()).PpCmdMainMenu = myCmdMainMenu = value;
            }
         }
      }

      /// <summary>
      ///  array of all tabs.
      /// </summary>
      public GateDockTabCtrl[] PpTabsAll => myTabPageHandling.TabsAll;

      /// <summary>
      ///  array of docked tabs.
      /// </summary>
      public GateDockTabCtrl[] PpTabsDocked => myTabPageHandling.TabsDocked;

      /// <summary>
      /// Array of floating tabs.
      /// </summary>
      public GateDockTabCtrl[] PpTabsFloating => myTabPageHandling.TabsFloating;

      /// <summary>
      /// 
      /// </summary>
      public Control[] PpTabPagesAll => myTabPageHandling.TabPagesAll;

      /// <summary>
      /// Direction tab group are placed.
      /// </summary>
      public DockableCtrlRowDirectionEnum? PpTabGroupDirection => PpTabsAll.Length >= 2 ? CtrlDockArea.PpCenterDirectionToSet : null as DockableCtrlRowDirectionEnum?;

      /// <summary>
      /// All widgets
      /// </summary>
      public GateDockWidgetCtrl[] PpAllWidgets =>
         CtrlDockArea.PpControlsDocked.OfType<GateDockWidgetCtrl>().
            Concat(CtrlDockArea.PpControlsDocked.OfType<GateDockTabCtrl>().SelectMany(t => t.PpWidgets)).
            Concat(myListFloatContainerForm.SelectMany(f => f.MthGetNephews()).OfType<GateDockWidgetCtrl>()).ToArray();

      /// <summary>
      /// All widgets.
      /// </summary>
      public GateDockWidgetGroupCtrl[] PpWidgetGroups => CtrlDockArea.PpControlsDocked.OfType<GateDockWidgetGroupCtrl>().ToArray();

      /// <summary>
      /// Selected widget.
      /// </summary>
      public GateDockWidgetCtrl? PpSelectedWidget
      {
         get => PpAllWidgets.FirstOrDefault(w => w.PpIsSelected);

         set => value?.MthBring2Front();
      }

      /// <summary>
      /// Current tab which contains selected(s) tab page(s).
      /// </summary>
      public GateDockTabCtrl? PpTabCurrent
      {
         get => myTabCurrent;

         private set
         {
            if (myTabCurrent != value)
            {
               myTabCurrent = value;
               myActionOnTabCurrentChanged(this, myTabCurrent);
            }
         }
      }

      /// <summary>
      /// Tab page(either a doc or a widget) which is currently focus selected.
      /// </summary>
      public Control? PpTabPageCurrent
      {
         get => PpTabCurrent?.PpTabPageVisible;
         set
         {
            if (value != null)
            {
               var tab = PpTabsAll.FirstOrDefault(t => t.PpAllControls.Contains(value));

               if (tab != null)
               {
                  var tmr = new Timer();
                  var tab_frm = tab.ParentForm ?? throw new Crash();

                  tab.PpTabPageVisible = value;

                  //workaround to allow the form is on top most
                  //not active in debug mode in order to avoid form covering visual studio window
                  if (!Debugger.IsAttached) { tab_frm.TopMost = true; }

                  if (WindowState == FormWindowState.Minimized) { NativeMethods.ShowWindow(tab_frm.Handle, CmdShowEnum.RESTORE); }

                  value.Focus();
                  BringToFront();
                  tmr.Enabled = true;
                  tmr.Interval = 50;
                  tmr.Tick += (s, e) => tmr.Enabled = tab_frm.TopMost = false;//top-most disabled after 50ms delay
               }
            }
         }
      }

      /// <summary> 
      /// Selected tab pages(either doc or widget) ie selected(even multiple) of current selected tab.
      /// </summary>
      public Control[] PpTabPagesSelected => PpTabsAll.Where(t => t.PpIsSelected).SelectMany(t => t.PpPagesSelected).ToArray();

      /// <summary>
      /// 
      /// </summary>
      public SkinChildCtrlDispacther PpSkinChildCtrlDispacther
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
      [Browsable(false)]
      public GateDockSkin? PpSkin
      {
         get => PpSkinChildCtrlDispacther?.Skin;

         set
         {
            if (PpSkin != value)
            {
               if (PpSkin != null) { PpSkin.OnAppParamChanged -= PpSkin_OnAnyAppParamChanged; }

               PpSkinChildCtrlDispacther.Skin = value;

               if (PpSkin != null)
               {
                  PpSkin.OnAppParamChanged += PpSkin_OnAnyAppParamChanged;
                  PpSkin.IsCurrent = true;
               }

               myActionOnSkinChange(this, PpSkin);
            }
            else if (PpSkin != null) { PpSkinChildCtrlDispacther.Skin = value; }

            PpSkin?.Save();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public GateDockTabPageHandler PpDocuHandler { get; set; } = new GateDockTabPageHandlerDefault();

      /// <summary>
      /// CmdMenu obj for the dropdown menu at page button.
      /// </summary>
      public CmdMenu? PpPageButtonContextCmdMenu { get; set; } = null;

      public Image? PpImage
      {
         get => myImage;
         set
         {
            myImage = value;

            if (myImage != null)
            {
               var bmp = new Bitmap(myImage);

               CtrlCaption.PpImage = myImage = value;

               for (var x = 0; x < bmp.Height; x++)
               {
                  for (var y = 0; y < bmp.Width; y++)
                  {
                     if (bmp.GetPixel(x, y).A != 0) { bmp.SetPixel(x, y, Color.DarkGray); }
                  }
               }

               Icon = Icon.FromHandle(bmp.GetHicon());
            }
            else
            {
               Icon = null;
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="widget"></param>
      public void MthWidgetHide(GateDockWidgetCtrl widget)
      {
         var sm = new InnerWdgStateTransitionHandler(this, widget);

         widget.MthSaveLast();
         sm.SetState(GateDockWidgetStateFlags.invisible, null);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="widget"></param>
      /// <returns></returns>
      public GateDockFloatContainerForm? MthWidgetShow(GateDockWidgetCtrl widget)
      {
         var res = null as GateDockFloatContainerForm;

         this.MthInvoke(new Action(() =>
         {
            var dck_sta = widget.PpLastDockState == GateDockWidgetStateFlags.invisible ?
               widget.PpFactory?.DefaultState : widget.PpLastDockState;

            res = MthWidgetShow(widget, dck_sta ?? throw new Crash(), widget.PpLastFloatLocation);
         }));

         return res;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="isSelected"></param>
      private void Widget_OnSelectedChanged(object? sender, bool isSelected) =>
         OnWidgetSelectedChange?.Invoke(this, isSelected ? sender as GateDockWidgetCtrl : null);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="widget"></param>
      /// <param name="dockState"></param>
      /// <param name="floatLocation"></param>
      /// <returns></returns>
      public GateDockFloatContainerForm? MthWidgetShow(GateDockWidgetCtrl widget, GateDockWidgetStateFlags dockState, Point? floatLocation = null)
      {
         var sm = new InnerWdgStateTransitionHandler(this, widget);

         sm.SetState(dockState, floatLocation);
         widget.OnSelectedChanged += Widget_OnSelectedChanged;
         widget.MthSaveLast();

         return widget.PpDockState == GateDockWidgetStateFlags.floating ? widget?.ParentForm as GateDockFloatContainerForm : null;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="anchorMode"></param>
      /// <param name="groupControlSize"></param>
      /// <param name="widgets"></param>
      /// <returns></returns>
      public GateDockWidgetGroupCtrl MthGroupWidgets(
         DockableAreaCtrlSlotAnchorModeEnum anchorMode, Size? groupControlSize, params GateDockWidgetCtrl[] widgets)
      {
         if (widgets.Any(w => w.PpMainFrm != null && w.PpMainFrm != this)) { throw new Gate.Dock.GateDockException("Widget bound to other form"); }
         else
         {
            var gru = new GateDockWidgetGroupCtrl();

            foreach (var wdg in widgets.Where(w => w.PpIsWidgetVisible)) { MthWidgetHide(wdg); }

            gru.PpAnchorMode = anchorMode;
            gru.PpMainFrm = this;
            gru.Size = groupControlSize.HasValue ? groupControlSize.Value : widgets.Last().Size;
            gru.MthWidgetAdd(widgets);
            CtrlDockArea.MthControlDock(gru, anchorMode);

            return gru;
         }
      }

      public void MthTabPageMove(Control[] widgetOrTabPageArray, GateDockTabCtrl tab)
      {
         foreach (var ctr in widgetOrTabPageArray) { myTabPageHandling.DocuForceControlOrWidgetClose(ctr); }
         foreach (var ctr in widgetOrTabPageArray) { myTabPageHandling.TabWidgetOrDocu(ctr); }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="tabPage"></param>
      /// <param name="tabControl"></param>
      public void MthTabPageAdd(GateDockTabPageCtrl tabPage, GateDockTabCtrl? tabControl = null)
      {
         if (!PpTabPagesAll.Contains(tabPage))
         {
            tabPage.PpMainFrm = this;
            myTabPageHandling.TabWidgetOrDocu(tabPage, tabControl);
            OnTabPageOpen?.Invoke(this, tabPage);
         }
         else if (tabControl != null && tabControl != tabPage.PpParentTab)
         {
            MthTabPageForceClose(tabPage);
            tabControl.MthWidgetOrTabPageAdd(tabPage);
         }

         PpTabPageCurrent = tabPage;
      }

      /// <summary>
      /// Adds a docu tab containing the widget/tabPage specified.
      /// </summary>
      /// <param name="tabPages">Tab age array (either widget or tab page)</param>
      /// <param name="docuTabState"></param>
      /// <param name="direction"></param>
      /// <param name="floatLocation"></param>
      /// <returns></returns>
      /// <exception cref="Gate.Dock.GateDockException">Grouping can be horizontal/vertical at certain time(multiple direction not allowed).</exception>
      public GateDockTabCtrl MthTabAdd(
         Control[] tabPages,
         GateDockTabStateEnum docuTabState,
         DockableCtrlRowDirectionEnum? direction = null,
         Point? floatLocation = null)
      {
         var dcs_to_sgn = tabPages.OfType<GateDockTabPageCtrl>().Where(d => !PpTabPagesAll.Contains(d)).ToArray();

         if (CtrlDockArea.PpControlsDockedCenter.Length <= 1 && direction.HasValue)
         {
            CtrlDockArea.PpCenterDirectionToSet = direction.Value;
         }
         else if (direction.HasValue && CtrlDockArea.PpCenterDirectionToSet != direction)
         {
            throw new Gate.Dock.GateDockException("Can't change direction when more than one tab is present!");
         }

         var res = myTabPageHandling.TabDocuOrWidgets(tabPages, docuTabState, floatLocation);

         foreach (var doc in dcs_to_sgn) { OnTabPageOpen?.Invoke(this, doc); }

         return res;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="tabPage"></param>
      public bool MthTabPageForceClose(GateDockTabPageCtrl tabPage) => myTabPageHandling.DocuForceControlOrWidgetClose(tabPage);

      /// <summary>
      /// Replace the document with a new one 
      /// </summary>
      /// <param name="tabPage2Close"></param>
      /// <param name="tabPage2Open"></param>
      public void MthDocuReopen(GateDockTabPageCtrl tabPage2Close, GateDockTabPageCtrl tabPage2Open) => myTabPageHandling.DocuReopen(tabPage2Close, tabPage2Open);

      /// <summary>
      /// In order to avoid trouble close should invoke through this method.
      /// </summary>
      public virtual bool MthExit()
      {
         if (!myIsCloseActionStarted)
         {
            var args = new GateDockMainFormClosingEventArgs();

            myIsCloseActionStarted = true;
            myActionOnFormClosing(this, args);

            if (args.IsClose2Confirm) { Close(); }
            else { myIsCloseActionStarted = false; }

            return args.IsClose2Confirm;
         }

         return false;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="toolBar"></param>
      public void MthToolBarAdd(CmdToolBarCtrl toolBar) => 
         (CtrlMainFormLayout.PpCtrlToolBarContainer as CmdToolBarContainerCtrl)?.MthToolBarAdd(toolBar);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="tabPages"></param>
      public void MthTapPagesFloat(Control[] tabPages)
      {
         if (tabPages?.Length > 0)
         {
            var tab = PpTabsAll.First(t => t.PpAllControls.Contains(tabPages[0]));
            var loc = (tab.Parent ?? throw new Crash()).PointToScreen(tab.Location);

            foreach (var pag in tabPages) { myTabPageHandling.DocuForceControlOrWidgetClose(pag); }

            myTabPageHandling.TabDocuOrWidgets(tabPages, GateDockTabStateEnum.floating, loc);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="tabPages"></param>
      public void MthTapPagesUnfloat(Control[] tabPages)
      {
         if (PpTabsDocked.Length > 0) { MthTabPageMove(tabPages, PpTabsDocked.First()); }
         else { MthTabAdd(tabPages, GateDockTabStateEnum.docked); }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="tab"></param>
      public void MthTabFloat(GateDockTabCtrl tab) => myTabPageHandling.TabChangeState(tab, GateDockTabStateEnum.floating);

      protected virtual void myActionOnTabCurrentChanged(object? sender, GateDockTabCtrl? tabCtrl)
      {
         OnTabCurrentChanged?.Invoke(sender, tabCtrl);
         myActionOnTabPageCurrentChanged(sender, PpTabPageCurrent);
      }

      protected virtual void myActionOnFormClosing(object? sender, GateDockMainFormClosingEventArgs args) =>
         OnMainFormClosing?.Invoke(this, args);

      protected virtual void myActionOnSkinChange(object? sender, GateDockSkin? skin)
      {
         PpSkin = skin;
         OnSkinChange?.Invoke(sender, skin);
      }

      protected virtual void myActionOnTabAdded(object? sender, GateDockTabCtrl tabCtrl)
      {
         var foc_obs = new ControlObservableFocused(tabCtrl);

         tabCtrl.OnVisibleTabPageChanged += TabCtrl_VisiblePageChanged;
         OnTabAdded?.Invoke(sender, tabCtrl);
         myListFocusObserver.Add(foc_obs);
         //focus observer tab page add
         foc_obs.OnIsFocusedChanged += Foc_obs_OnIsFocusedChanged;
      }

      private void Foc_obs_OnIsFocusedChanged(Control? observable, Control? focusedControl, bool isSelected)
      {
         if (isSelected) { PpTabCurrent = observable as GateDockTabCtrl; }
      }

      protected virtual void myActionOnTabRemoved(object? sender, GateDockTabCtrl tabCtrl)
      {
         var foc_obs = myListFocusObserver.First(o => o.RootControl == tabCtrl);

         tabCtrl.OnVisibleTabPageChanged -= TabCtrl_VisiblePageChanged;
         OnTabRemoved?.Invoke(sender, tabCtrl);
         foc_obs.OnIsFocusedChanged -= Foc_obs_OnIsFocusedChanged; //focus observer tab page remove
         myListFocusObserver.Remove(foc_obs);
      }

      protected virtual void myActionOnTabPageCurrentChanged(object? sender, Control? newTabPageTopLevel) =>
         OnTabPageCurrentChanged?.Invoke(sender, newTabPageTopLevel);

      protected override void OnLoad(EventArgs e)
      {
         var ski = new GateDockSkin(this, PpAppName);

         ski.IsCurrent = true;

         var mgs = new MsgCollection();

         if (!ski.Load(mgs))
         {
            //todo save to log
            ski.Save();
         }

         PpSkin = ski;

         if (PpTabsDocked.Length > 0) { PpTabPageCurrent = PpTabsDocked[0].PpAllControls[0]; }
         else if (PpTabsFloating.Length > 0) { PpTabPageCurrent = PpTabsFloating[0].PpAllControls[0]; }
         else { CtrlDockArea.Focus(); }//otherwise short cut management could not to work (MainForm.ProcessCmdKey() not invoked).

         base.OnLoad(e);
      }

      protected override void OnFormClosing(FormClosingEventArgs e)
      {
         if (!myIsCloseActionStarted) { throw new Crash("Close shall be initiziated by 'myDoActionOnClose'"); }

         e.Cancel = false;//forces closing of form otheriwe blocked by owned forms (widgets)
      }

      protected override void OnKeyDown(KeyEventArgs e)
      {
         e.SuppressKeyPress = true;
         base.OnKeyDown(e);
      }

      protected override bool ProcessCmdKey(ref Message msg, Keys keyData) =>
         PpCmdMainMenu != null ?
            PpCmdMainMenu.HandleKeyForShortcuts(keyData, true) : base.ProcessCmdKey(ref msg, keyData);

      protected override void WndProc(ref Message m)
      {
         if (m.Msg == (int)WinMsgEnum.WM_CLOSE) { MthExit(); }

         base.WndProc(ref m);
      }

      protected override void OnActivated(EventArgs e)
      {
         base.OnActivated(e);

         if (!Debugger.IsAttached) { TopMost = true; }
         BringToFront();
         TopMost = false;
      }

      private GateDockFloatContainerForm myDoMakeControlFloat(Control control, Point? startPos)
      {
         if (control.Parent == null)
         {
            var frm = new GateDockFloatContainerForm();
            var ctr_siz = control.Size;

            control.Dock = DockStyle.Fill;
            frm.StartPosition = FormStartPosition.Manual;
            myListFloatContainerForm.Add(frm);
            frm.FormClosed += (s, e) => myListFloatContainerForm.Remove(frm);
            frm.Load += (s, e) => frm.Size = ctr_siz;
            frm.PpIsInTaskBar = !(control is GateDockWidgetCtrl);
            frm.Location = startPos != null ? startPos.Value : new Point(Left + Width / 2 - ctr_siz.Width / 2, Top + Height / 2 - ctr_siz.Height / 2);
            myFloatingMovingHandler.OnNewFloatForm(frm);
            frm.Controls.Add(control);
            frm.Show();
            control.Focus();

            return frm;
         }
         else { throw new Crash(); }
      }

      private void PpSkin_OnAnyAppParamChanged(AppParam appParam) => myActionOnSkinChange(this, PpSkin);

      private void CtrlCaption_OnDockCaptionEvent(object? sender, CustomCaptionCtrl.EventType eventType)
      {
         if (eventType == CustomCaptionCtrl.EventType.button_close) { MthExit(); }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void CtrlDockArea_DragDrop(object? sender, DragEventArgs e)
      {
         var fls =
            (e.Data?.GetData(DataFormats.FileDrop) as string[])?.
            Where(f => f != null && File.Exists(f)).
            ToArray() ?? [];

         foreach (var fil in fls) { PpDocuHandler.OpenPath(this, fil); }
      }

      private void CtrlDockArea_DragEnter(object? sender, DragEventArgs e) =>
         e.Effect = (e.Data?.GetDataPresent(DataFormats.FileDrop) ?? false) ? DragDropEffects.Copy : DragDropEffects.None;

      private void TabCtrl_VisiblePageChanged(object? sender, Control? newVisibleControl) => 
         OnTabPageCurrentChanged?.Invoke(this, PpTabPageCurrent);
   }
}
