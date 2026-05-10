using Gate.Dock.DockDocu;
using Gate.Dock.DockSkin;
using Gate.Dock.DockWidget;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.ControlObserve;
using Gate.ToolsView.Dockable;
using Gate.ToolsView.Extended;
using Gate.ToolsView.Extensions;
using Gate.ToolsView.MenuCommand;
using Gate.ToolsView.MenuExtended;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using static Gate.ToolsView.Extended.CustomCaptionCtrl;
using static Gate.ToolsView.Extended.ExtendedTabbedCtrl;

namespace Gate.Dock.DockTab
{
   /// <summary>
   /// Tab container control, can contain tab page and documents
   /// </summary>
   public partial class GateDockTabCtrl : UserControl, IGateDockCtrlWithSkin
   {
      public delegate void OnStateChangedHandler(object? sender, GateDockTabStateEnum newState);
      public delegate void OnVisibleControlHandler(GateDockTabCtrl sender, Control? newVisibleControl);

      public event OnStateChangedHandler? OnStateChanged;
      public event OnVisibleControlHandler? OnVisibleTabPageChanged;
      public event OnAskForDraggingHandler? OnAskForDragging;
      public event OnAskForTabCloseHandler? OnAskForTabClose;

      private GateDockTabStateEnum myState = GateDockTabStateEnum.none;
      private GateDockMainForm? myMainFrm = null;
      private GateDockTabCtrlSkinDispacther mySkinChildCtrlDispacther;
      private Label myLabelTitle = new Label();

      public GateDockTabCtrl()
      {
         InitializeComponent();

         myLabelTitle.BorderStyle = BorderStyle.None;
         myLabelTitle.Text = "";
         CtrlCaption.Controls.Add(myLabelTitle);
         mySkinChildCtrlDispacther = new GateDockTabCtrlSkinDispacther(this);
         CtrlTabbed.PpIsFixedTabHeightToUse = true;
         CtrlTabbed.PpFixedTabHeight =
            GateDockSkin.Constants.WIDGET_CAPTION_HEIGHT + GateDockSkin.Constants.WIDGET_DOCUTAB_BORDER;

         var obs = new ControlObservableParent(this);

         obs.OnControlAdded += myActionOnAnyAnchestorControlChanged;
         obs.OnControlRemoved += myActionOnAnyAnchestorControlChanged;
         CtrlTabbed.OnSelectedTabChanged += CtrlTabbed_OnSelectedTabChanged;
      }

      private void CtrlTabbed_OnSelectedTabChanged(object? sender, int newTabIdx, Control? newTabbedControl) =>
         OnVisibleTabPageChanged?.Invoke(this, CtrlTabbed.PpTabVisible);

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
               if (myMainFrm != null)
               {
                  myMainFrm.OnTabPageCurrentChanged += MainFrm_OnTabPageCurrentChanged;
                  myMainFrm.OnSkinChange -= MainFrm_OnSkinChange;
               }

               myMainFrm = value;

               if (myMainFrm != null)
               {
                  PpSkin = myMainFrm.PpSkin;
                  myMainFrm.OnSkinChange += MainFrm_OnSkinChange;
                  myMainFrm.OnTabPageCurrentChanged += MainFrm_OnTabPageCurrentChanged;
                  PpPageButtonCmdMenuRef = new CmdMenu.Ref(myMainFrm.PpPageButtonContextCmdMenu.NnOrCrash(), true);
               }
            }
         }
      }

      private void MainFrm_OnTabPageCurrentChanged(object? sender, Control? newTopLevelPage)
      {
         var vis_col = PpSkin != null ? PpSkin.Params.WidgetSelectedColor.Value : GateDockSkin.DefaultValues.WidgetSelectedColor;
         var tab_sel_onl = PpSkin != null ? PpSkin.Params.TabSelectedOnlyColor.Value : GateDockSkin.DefaultValues.TabSelectedOnlyColor;
         var not_sel_col = PpSkin != null ? PpSkin.Params.WidgetUnselectedColor.Value : GateDockSkin.DefaultValues.WidgetUnselectedColor;

         if (PpIsSelected)
         {
            CtrlTabbed.PpButtonBackColorVisible = vis_col;
            CtrlTabbed.PpButtonBackColorSelected = tab_sel_onl;
         }
         else
         {
            CtrlTabbed.PpButtonBackColorVisible = PpIsSelected ? vis_col : not_sel_col;
            CtrlTabbed.MthClearMultiSelection();
         }

         CtrlTabbed.PerformLayout();

         if (PpState == GateDockTabStateEnum.floating)
         {
            Text = CtrlTabbed.PpTabVisible != null ? CtrlTabbed.PpTabVisible.Text : "";

            if (CtrlTabbed.PpTabVisible != null)
            {
               CtrlTabbed.PpTabVisible.TextChanged += (s, _) =>
               {
                  if (PpTabPageVisible == s && ParentForm != null) { ParentForm.Text = PpTabPageVisible?.Text; }
               };
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public GateDockTabStateEnum PpState
      {
         get => myState;

         private set
         {
            if (myState != value) { myActionOnStateChanged(this, myState = value); }
         }
      }

      /// <summary>
      /// Index(0,..) of docu tab, or -1 if docu tab is floating.
      /// </summary>
      public int PpDockedIndex
      {
         get
         {
            var dck_ctr = this.MthGetAnchestor<DockableAreaCtrl>();

            return dck_ctr != null ? dck_ctr.PpControlsDockedCenter.ToList().IndexOf(this) : -1;
         }
      }

      /// <summary>
      /// All controls contained(widget+docu).
      /// </summary>
      public Control[] PpAllControls => CtrlTabbed.PpTabs;

      /// <summary>
      /// 
      /// </summary>
      public bool PpIsSelected => PpMainFrm == null || PpMainFrm.PpTabCurrent == this;

      /// <summary>
      ///  array of selected controls.
      /// </summary>
      public Control[] PpPagesSelected => CtrlTabbed.PpTabsSelected;

      /// <summary>
      ///  
      /// </summary>
      public Control? PpTabPageVisible
      {
         get => CtrlTabbed.PpTabVisible;

         set => CtrlTabbed.PpTabVisible = value;
      }

      /// <summary>
      /// 
      /// </summary>
      public GateDockWidgetCtrl[] PpWidgets => CtrlTabbed.PpTabs.OfType<GateDockWidgetCtrl>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public Control[] PpTabControls => CtrlTabbed.PpTabs.ToArray();

      /// <summary>
      /// 
      /// </summary>
      [Browsable(false)]
      public GateDockSkin? PpSkin { get => PpSkinChildCtrlDispacther.Skin; set => PpSkinChildCtrlDispacther.Skin = value; }

      /// <summary>
      /// 
      /// </summary>
      public GateDockTabCtrlSkinDispacther PpSkinChildCtrlDispacther
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
      [AllowNull]
      public override string Text
      {
         get => myLabelTitle.Text;
         set => myLabelTitle.Text = value;
      }

      /// <summary>
      /// 
      /// </summary>
      public CmdMenu.Ref? PpPageButtonCmdMenuRef { get; private set; } = null;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="tabPageOrWidget"></param>
      public void MthWidgetOrTabPageAdd(Control tabPageOrWidget)
      {
         myDoAdd((dynamic)tabPageOrWidget);

         var but = CtrlTabbed.MthGetButton(tabPageOrWidget);

         but.PpMenuDropDown = new ExtendedMenuDropDown();
         but.PpMenuDropDown.PpCmdMenuRef = PpPageButtonCmdMenuRef;
         PpSkin = PpSkin;//refreshes skin
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="tabPageOrWidget"></param>
      public void MthWidgetOrTabPageRemove(Control tabPageOrWidget) => myDoRemove((dynamic)tabPageOrWidget);

      protected virtual void myActionOnStateChanged(object? sender, GateDockTabStateEnum newState)
      {
         CtrlCaption.Visible = newState != GateDockTabStateEnum.docked;

         if (newState == GateDockTabStateEnum.docked)
         {
            CtrlCaption.Visible = false;
            CtrlPanel.PpBorderWidth = 0;
         }
         else
         {
            CtrlCaption.Visible = true;
            CtrlPanel.PpBorderWidth = GateDockSkin.Constants.WIDGET_DOCUTAB_BORDER;
         }

         OnStateChanged?.Invoke(this, newState);
      }

      private void myDoAdd(GateDockTabPageCtrl tabPage)
      {
         CtrlTabbed.MthControlAdd(tabPage);

         var but = CtrlTabbed.MthGetButton(tabPage);

         but.PpText = tabPage.PpTitle;
         tabPage.OnTitleChange += Docu_OnTitleChange;

         if (tabPage is IGateDockDocu doc)
         {
            var tab_pag = (GateDockTabPageCtrl)doc;

            but.PpToolTipText = doc.PpDocuPath;
            doc.OnDocuPathChange += (s, e) => but.PpToolTipText = doc.PpDocuPath;
         }
      }

      private void myDoAdd(GateDockWidgetCtrl widget)
      {
         PpMainFrm?.MthWidgetShow(widget, GateDockWidgetStateFlags.invisible, null);
         widget.PpIsCaptionVisible = false;
         widget.PpMainFrm = PpMainFrm;
         CtrlTabbed.MthControlAdd(widget);
         CtrlTabbed.MthGetButton(widget).PpText = widget.PpTitle;
         widget.OnTitleChange += Widget_OnTitleChange;
      }

      private void myDoRemove(GateDockTabPageCtrl docu)
      {
         if (PpTabControls.Contains(docu))
         {
            docu.OnTitleChange -= Docu_OnTitleChange;
            CtrlTabbed.MthControlRemove(docu);
         }
      }

      private void myDoRemove(GateDockWidgetCtrl widget)
      {
         if (PpWidgets.Contains(widget))
         {
            widget.OnTitleChange -= Widget_OnTitleChange;
            CtrlTabbed.MthControlRemove(widget);
         }
      }

      private void myActionOnAnyAnchestorControlChanged(Control control)
      {
         if (Parent is GateDockFloatContainerForm) { PpState = GateDockTabStateEnum.floating; }
         else if (this.MthGetAnchestor<DockableAreaCtrl>() != null) { PpState = GateDockTabStateEnum.docked; }
         else { PpState = GateDockTabStateEnum.none; }
      }

      private void CtrlTabbed_OnAskForTabClose(object? sender, int tabIdx, Control controlInTab) => OnAskForTabClose?.Invoke(this, tabIdx, controlInTab);

      private void CtrlTabbed_OnAskForDragging(object? sender, int tabIdx, Control controlInTab) => OnAskForDragging?.Invoke(sender, tabIdx, controlInTab);

      private void MainFrm_OnSkinChange(object? sender, GateDockSkin? skin) => PpSkin = skin;

      private void CtrlCaption_OnDockCaptionEvent(object? sender, EventType eventType)
      {
         switch (eventType)
         {
            case EventType.button_minimize:
            case EventType.button_maximize:
            case EventType.button_close:
            case EventType.start_tracking:
            case EventType.end_tracking:
               this.MthGetNephew<CustomCaptionCtrl>()?.MthDefaultBehaviour(sender, eventType);
               break;

            default: throw new Crash();
         }
      }

      private void Widget_OnTitleChange(object? sender, EventArgs e) =>
         CtrlTabbed.MthGetButton(sender as Control ?? throw new Crash()).PpText =
            (sender as GateDockWidgetCtrl)?.PpTitle;

      private void Docu_OnTitleChange(object? sender, EventArgs e) =>
         CtrlTabbed.MthGetButton(
            sender as GateDockTabPageCtrl ?? throw new Crash()).PpText =
               (sender as GateDockTabPageCtrl ?? throw new Crash()).PpTitle;
   }
}

