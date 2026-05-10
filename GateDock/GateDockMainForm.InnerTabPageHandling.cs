using Gate.Dock.DockTab;
using Gate.Dock.DockWidget;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.Dockable;
using Gate.ToolsView.Extended;

namespace Gate.Dock
{
   public partial class GateDockMainForm
   {
      private class InnerTabPageHandling
      {
         private readonly List<GateDockTabCtrl> myListTabFloating = new List<GateDockTabCtrl>();
         private readonly RemoveVisitor myRemoveVisitor;

         public InnerTabPageHandling(GateDockMainForm mainForm)
         {
            MainForm = mainForm;
            myRemoveVisitor = new RemoveVisitor(this);
         }

         private class RemoveVisitor
         {
            public RemoveVisitor(InnerTabPageHandling parent) => Parent = parent;

            public InnerTabPageHandling Parent { get; }

            public void TryRemove(Control control) => myTryRemove((dynamic)control);

            private void myTryRemove(GateDockTabPageCtrl tabPage)
            {
               if (tabPage.PpMainFrm != null)
               {
                  if (tabPage.PpMainFrm == Parent.MainForm) { Parent.DocuForceControlOrWidgetClose(tabPage); }
                  else { throw new Gate.Dock.GateDockException("Docu already of another main form"); }
               }
            }

            private void myTryRemove(GateDockWidgetCtrl widget)
            {
               if (widget.PpMainFrm != null)
               {
                  if (widget.PpMainFrm == Parent.MainForm)
                  {
                     if ((widget.PpDockState & GateDockWidgetStateFlags.dock) != 0x0) { Parent.MainForm.MthWidgetHide(widget); }
                  }
                  else { throw new Gate.Dock.GateDockException("Docu already of another main form"); }
               }
            }

            private void myTryRemove(Control control) => throw new Crash();
         }

         /// <summary>
         /// 
         /// </summary>
         public GateDockTabCtrl[] TabsDocked => MainForm.CtrlDockArea.PpControlsDockedCenter.Cast<GateDockTabCtrl>().ToArray();

         /// <summary>
         /// 
         /// </summary>
         public GateDockTabCtrl[] TabsFloating => myListTabFloating.ToArray();

         /// <summary>
         /// 
         /// </summary>
         public GateDockTabCtrl[] TabsAll => myListTabFloating.Concat(TabsDocked).ToArray();

         /// <summary>
         /// 
         /// </summary>
         public Control[] TabPagesAll => TabsAll.SelectMany(t => t.PpTabControls).ToArray();

         /// <summary>
         /// 
         /// </summary>
         public GateDockWidgetCtrl[] TabWidgets => TabsAll.SelectMany(t => t.PpWidgets).ToArray();

         /// <summary>
         /// 
         /// </summary>
         public GateDockMainForm MainForm { get; private set; }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="widgetOrDocuTabs"></param>
         /// <param name="docuTabState"></param>
         /// <returns></returns>
         public GateDockTabCtrl TabDocuOrWidgets(Control[] widgetOrDocuTabs, GateDockTabStateEnum docuTabState, Point? floatStartLocation)
         {
            var dck_tab = myMakeFreshDocuTab();

            foreach (var ctr in widgetOrDocuTabs) { myRemoveVisitor.TryRemove(ctr); }
            foreach (var ctr in widgetOrDocuTabs) { dck_tab.MthWidgetOrTabPageAdd(ctr); }
            foreach (var ctr in widgetOrDocuTabs.OfType<GateDockTabPageCtrl>()) { ctr.PpMainFrm = MainForm; }

            if (docuTabState == GateDockTabStateEnum.docked) { MainForm.CtrlDockArea.MthControlDock(dck_tab, DockableAreaCtrlSlotAnchorModeEnum.center); }
            else { myMakeTabFloat(dck_tab, floatStartLocation); }

            return dck_tab;
         }

         public bool DocuForceControlOrWidgetClose(Control tabPageOrWidget)
         {
            var tab = null as GateDockTabCtrl;

            tab = TabsAll.FirstOrDefault(t => t.PpAllControls.Contains(tabPageOrWidget));

            if (tab == null) { return false; }
            else
            {
               if (tabPageOrWidget is GateDockTabPageCtrl tab_pag_1)
               {
                  MainForm.OnTabPageClosing?.Invoke(MainForm, tab_pag_1);
               }

               tab.MthWidgetOrTabPageRemove(tabPageOrWidget);
            }

            if (tab.PpTabControls.Length == 0)//empty tab is removed
            {
               var is_flt = myListTabFloating.Contains(tab);

               if (is_flt) { myListTabFloating.Remove(tab); }

               tab.OnAskForTabClose += Dck_tab_OnAskForTabClose;
               tab.OnAskForDragging += Dck_tab_OnAskForDragging;
               tab.PpMainFrm = null;

               if (is_flt)
               {
                  myListTabFloating.Remove(tab);
                  tab.ParentForm?.Close();
               }
               else { MainForm.CtrlDockArea.MthControlUndock(tab); }
            }

            if (tabPageOrWidget is GateDockTabPageCtrl tab_pag)
            {
               MainForm.OnTabPageClose?.Invoke(MainForm, tab_pag);
            }

            if (MainForm.PpTabPagesAll.Length == 0)
            {
               //otherwise short cut management could not to work (MainForm.ProcessCmdKey() not invoked).
               MainForm.CtrlDockArea.Focus();
            }

            return true;
         }

         public void DocuReopen(GateDockTabPageCtrl docToClose, GateDockTabPageCtrl docToOpen)
         {
            var tab = docToClose.PpParentTab;

            if (tab != null)
            {
               var is_sel = tab.PpTabPageVisible == docToClose;

               tab.MthWidgetOrTabPageRemove(docToClose);
               tab.MthWidgetOrTabPageAdd(docToOpen);

               if (is_sel) { tab.PpTabPageVisible = docToOpen; }
            }
         }

         public void TabWidgetOrDocu(Control control, GateDockTabCtrl? tabControl = null)
         {
            if (tabControl != null) { tabControl.MthWidgetOrTabPageAdd(control); }
            else { myDoGetFirstAvalaibleDockDocuTabOrMakeIt().MthWidgetOrTabPageAdd(control); }
         }

         /// <summary>
         /// Append a new docu tab and adds an array of widgets/docus.
         /// </summary>
         /// <param name="widgetsOrDocus"></param>
         /// <param name="afterTab"></param>
         /// <param name="direction"></param>
         public void TabWidgetOrDocuDocked(Control[] widgetsOrDocus, GateDockTabCtrl afterTab, DockableCtrlRowDirectionEnum direction)
         {
            if (afterTab != null && afterTab.PpState == GateDockTabStateEnum.floating)
            {
               throw new Crash("Operation not valid for floating tabs");
            }
            else if (MainForm.CtrlDockArea.PpCenterDirectionEffective.HasValue && MainForm.CtrlDockArea.PpCenterDirectionEffective.Value != direction)
            {
               throw new Crash("after more than one of docked docu tabs have been created direction can't change.");
            }
            else
            {
               var dck_tab = myMakeFreshDocuTab();

               MainForm.CtrlDockArea.MthControlDock(dck_tab, DockableAreaCtrlSlotAnchorModeEnum.center);

               foreach (var ctr in widgetsOrDocus) { dck_tab.MthWidgetOrTabPageAdd(ctr); }
            }
         }

         public void TabChangeState(GateDockTabCtrl tab, GateDockTabStateEnum state)
         {
            if (state != tab.PpState)
            {
               switch (state)
               {
                  case GateDockTabStateEnum.floating:
                     if (tab.PpAllControls.Length > 0)
                     {
                        var wds = tab.PpAllControls.OfType<GateDockWidgetCtrl>().ToArray();

                        foreach (var wdg in wds) { MainForm.MthWidgetShow(wdg, GateDockWidgetStateFlags.floating); }

                        myMakeTabFloat(tab, MainForm.CtrlDockArea.Parent.NnOrCrash().PointToScreen(new Point()));
                     }
                     else { throw new Crash(); }

                     break;

                  case GateDockTabStateEnum.docked:
                     if (tab.PpState == GateDockTabStateEnum.floating)
                     {
                        myListTabFloating.Remove(tab);
                        tab.ParentForm?.Close();
                        MainForm.CtrlDockArea.MthControlDock(tab, DockableAreaCtrlSlotAnchorModeEnum.center);
                     }
                     else { throw new Crash(); }
                     break;

                  case GateDockTabStateEnum.none:
                  default:
                     throw new Crash();
               }
            }
         }

         private GateDockTabCtrl myDoGetFirstAvalaibleDockDocuTabOrMakeIt()
         {
            var dck_tab = TabsDocked.FirstOrDefault();

            if (dck_tab == null)
            {
               dck_tab = myMakeFreshDocuTab();
               MainForm.CtrlDockArea.MthControlDock(dck_tab, DockableAreaCtrlSlotAnchorModeEnum.center);
            }

            return dck_tab;
         }

         private GateDockTabCtrl myMakeFreshDocuTab()
         {
            var tab_ctr = new GateDockTabCtrl();

            tab_ctr.PpMainFrm = MainForm;
            tab_ctr.OnAskForTabClose += Dck_tab_OnAskForTabClose;
            tab_ctr.OnAskForDragging += Dck_tab_OnAskForDragging;
            MainForm.myActionOnTabAdded(MainForm, tab_ctr);

            return tab_ctr;
         }


         private void Dck_tab_OnAskForDragging(object? sender, int tabIdx, Control controlInTab)
         {
            var but = (sender as ExtendedTabbedCtrl)?.PpTabButtons[tabIdx] ?? throw new Crash();
            var but_loc_scr = but.Parent?.PointToScreen(but.Location) ?? throw new Crash();

            if (controlInTab is GateDockWidgetCtrl wdg)
            {
               MainForm.MthWidgetShow(wdg, GateDockWidgetStateFlags.floating, but_loc_scr)?.MthStartTracking();
            }
            else if (controlInTab is GateDockTabPageCtrl tab_pag)
            {
               var tab = myMakeFreshDocuTab();

               DocuForceControlOrWidgetClose(controlInTab);
               tab.MthWidgetOrTabPageAdd(controlInTab);
               myMakeTabFloat(tab, new Point(but_loc_scr.X, but_loc_scr.Y - but.Height));
            }
         }

         private void myMakeTabFloat(GateDockTabCtrl tab, Point? fromPoint)
         {
            myListTabFloating.Add(tab);

            tab.Parent = null;
            var frm = MainForm.myDoMakeControlFloat(tab, fromPoint);

            frm.FormClosing += Frm_FormClosing;
            frm.MthStartTracking();
         }

         private void Frm_FormClosing(object? sender, FormClosingEventArgs e)
         {
            var tab_ctr = (sender as GateDockFloatContainerForm)?.PpAssociatedTabCtrl;

            if (tab_ctr != null)
            {
               tab_ctr.Parent = null;
               myListTabFloating.Remove(tab_ctr);
            }
            else { throw new Crash(); }
         }

         private void Dck_tab_OnAskForTabClose(object? sender, int tabIdx, Control controlInTab)
         {
            if (controlInTab is GateDockWidgetCtrl wdg) { MainForm.MthWidgetHide(wdg); }
            else { MainForm.PpDocuHandler.AskForClose(MainForm, (GateDockTabPageCtrl)controlInTab); }
         }
      }
   }
}
