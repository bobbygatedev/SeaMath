using Gate.Dock.DockDocu;
using Gate.Dock.DockFactories;
using Gate.Dock.DockTab;
using Gate.Dock.DockWidget;
using Gate.Tools;
using Gate.Tools.AppParams;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text;
using Gate.ToolsView.Dockable;
using Gate.ToolsView.Extensions;
using System.CodeDom;
using static Gate.Dock.DockApp.GateDockAppFormScenario;
using static Gate.Dock.DockApp.GateDockAppFormScenario.TabRecord;
using static Gate.Tools.AppParams.AppParamLoadSaver;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using Timer = System.Windows.Forms.Timer;

namespace Gate.Dock.DockApp
{
   public class GateDockAppFormScenario : AppParamContainerSpecialized<ScenarioParams>
   {
      private readonly Timer myTimer = new Timer();

      public GateDockAppFormScenario(GateDockApp app) => App = app;

      public interface IWithDockOrder
      {
         int DockOrder { get; set; }
      }

      public class ScenarioParams : AppParam.Record
      {
         public ScenarioParams() : base("Scenario") { }

         public readonly FormStateRecord FormState = new FormStateRecord();

         public readonly DockItemsRecord DockItems = new DockItemsRecord();

         public readonly FloatItemsRecord FloatItems = new FloatItemsRecord();
      }

      public class DockItemsRecord : AppParam.Record
      {
         public DockItemsRecord() { }

         public readonly Simple<DockableCtrlRowDirectionEnum> TabDirection = new Simple<DockableCtrlRowDirectionEnum>(DockableCtrlRowDirectionEnum.left_2_right);
         public readonly Arry<TabRecord> Tabs = new Arry<TabRecord>();
         public readonly Arry<WidgetRecord> Widgets = new Arry<WidgetRecord>();

         public readonly WidgetGroupRecord GroupUp = new WidgetGroupRecord();
         public readonly WidgetGroupRecord GroupDown = new WidgetGroupRecord();
         public readonly WidgetGroupRecord GroupLeft = new WidgetGroupRecord();
         public readonly WidgetGroupRecord GroupRight = new WidgetGroupRecord();

         /// <summary>
         /// 
         /// </summary>
         /// <param name="app"></param>
         /// <exception cref="Crash"></exception>
         public void ReadFromRecord(GateDockApp app)
         {
            // Order the items by DockOrder property, which is implemented by IWithDockOrder interface
            var its = SubItems;
            var lst = new List<IWithDockOrder>();

            foreach (var itm in SubItems)
            {
               if (itm is WidgetGroupRecord wr)
               {
                  lst.Add(wr);
               }
               else if (itm == Widgets)
               {
                  lst.AddRange(Widgets.Items);
               }
            }

            lst = lst.Where(i => i.DockOrder > 0).OrderBy(i => i.DockOrder).ToList();
            myDoReadFromRepo(Tabs, app);

            foreach (var itm in lst)
            {
               if (itm is WidgetGroupRecord gru_rec) { gru_rec.ReadWidgets(app); }
               else if (itm is WidgetRecord wdg_rec)
               {
                  var wdg = wdg_rec.ReadFromRepo(app);

                  if (wdg != null)
                  {
                     app.MainForm.MthWidgetShow(wdg, wdg_rec.DockState, null);
                  }
               }
               else { throw new Crash(); }
            }
         }

         /// <summary>
         /// Writes content of <see cref="DockableAreaCtrl"/> to <see cref="AppParam.Record"/>
         /// ordering the items by <see cref="DockableAreaCtrl.PpControlsDocked"/> property
         /// Ordering is implemented by <see cref="IWithDockOrder"/> interface, 
         /// which is implemented by <see cref="TabRecord"/>, <see cref="WidgetRecord"/> and 
         /// <see cref="WidgetGroupRecord"/>
         /// </summary>
         /// <param name="app"></param>
         public void WriteToRecord(GateDockApp app)
         {
            var dck_ctr = app.MainForm.MthGetNephew<DockableAreaCtrl>();
            var cts = dck_ctr?.PpControlsDocked ?? [];
            var id = 1;

            foreach (var ctr in cts)
            {
               /// Convert <see cref="AppParam.Record"/> to <see cref="IWithDockOrder"/> 
               /// to set the DockOrder property"/> 
               var itm = (myDoWriteToRepo((dynamic)ctr) as object).ConvertOrCrash<IWithDockOrder>();

               itm.DockOrder = id++;
            }
         }

         private void myDoReadFromRepo(Arry<TabRecord> arryTabRecord, GateDockApp app)
         {
            foreach (var tab_rep in arryTabRecord.Items)
            {
               var cts = tab_rep.ReadFromRepo(app);

               if (cts.Length > 0) { app.MainForm.MthTabAdd(cts, GateDockTabStateEnum.docked, TabDirection.Value); }
            }
         }

         private void myDoReadFromRepo(Arry<WidgetRecord> arryWidgetRecord, GateDockApp app)
         {
            foreach (var wdg_rec in arryWidgetRecord.Items)
            {
               var wdg = wdg_rec.ReadFromRepo(app);

               if (wdg != null)
               {
                  app.MainForm.MthWidgetShow(wdg, wdg_rec.DockState, null);
               }
            }
         }

         private void myDoReadFromRepo(AppParam appParam, GateDockApp app) { }//do nothing

         private void myDoReadFromRepo(object par, GateDockApp app) => throw new Crash();

         private IWithDockOrder myDoWriteToRepo(GateDockTabCtrl tabCtrl)
         {
            var tab_wrp = new TabRecord();

            foreach (var ctr in tabCtrl.PpAllControls) { myDoWriteToTabWrapper((dynamic)ctr, tab_wrp); }

            Tabs.AddParam(tab_wrp);

            return tab_wrp;
         }

         private IWithDockOrder myDoWriteToRepo(GateDockWidgetCtrl widgetCtrl)
         {
            if (widgetCtrl.PpFactory != null)//only widget with factory can be saved
            {
               var wdg_rec = new WidgetRecord();

               wdg_rec.WriteToRecord(widgetCtrl);
               Widgets.AddParam(wdg_rec);

               return wdg_rec;
            }
            else
            {
               throw new Crash();
            }
         }

         private IWithDockOrder myDoWriteToRepo(GateDockWidgetGroupCtrl widgetGroupCtrl)
         {
            var gru_rec = null as WidgetGroupRecord;

            switch (widgetGroupCtrl.PpAnchorMode)
            {
               case DockableAreaCtrlSlotAnchorModeEnum.left:
                  gru_rec = GroupLeft;
                  break;

               case DockableAreaCtrlSlotAnchorModeEnum.right:
                  gru_rec = GroupRight;
                  break;

               case DockableAreaCtrlSlotAnchorModeEnum.up:
                  gru_rec = GroupUp;
                  break;

               case DockableAreaCtrlSlotAnchorModeEnum.down:
                  gru_rec = GroupDown;
                  break;

               default: throw new Crash();
            }

            gru_rec.Widgets.Clear();
            gru_rec.WriteToRepo(widgetGroupCtrl);

            return gru_rec;
         }

         private void myDoWriteToTabWrapper(Control control, TabRecord tabRepo) => throw new Crash();

         private void myDoWriteToTabWrapper(GateDockWidgetCtrl widgetCtrl, TabRecord tabRecord)
         {
            var tab_itm = new TabItem();

            tab_itm.WidgetPage.WriteToRecord(widgetCtrl);
            tabRecord.TabItems.AddParam(tab_itm);
         }

         private void myDoWriteToTabWrapper(GateDockTabPageCtrl tabPage, TabRecord tabRecord)
         {
            var tab_itm = new TabItem();

            tabRecord.TabItems.AddParam(tab_itm);

            if (tabPage is IGateDockDocu doc) { tab_itm.DocuPage.WriteToRecord(doc); }
            else if (tabPage.PpFactory is GateDockTabPagePureFactory fac && fac.IsSavingToParams) { tab_itm.TabPage.WriteToRecord(tabPage); }
         }

         private GateDockWidgetStateFlags myGetDockState(DockableAreaCtrlSlotAnchorModeEnum anchorMode)
         {
            switch (anchorMode)
            {
               case DockableAreaCtrlSlotAnchorModeEnum.left: return GateDockWidgetStateFlags.dock_left;
               case DockableAreaCtrlSlotAnchorModeEnum.right: return GateDockWidgetStateFlags.dock_right;
               case DockableAreaCtrlSlotAnchorModeEnum.up: return GateDockWidgetStateFlags.dock_up;
               case DockableAreaCtrlSlotAnchorModeEnum.down: return GateDockWidgetStateFlags.dock_down;
               default: throw new Crash();
            }
         }
      }

      public class FloatItemsRecord : AppParam.Record
      {
         public FloatItemsRecord() { }

         public readonly Arry<TabRecord> Tabs = new Arry<TabRecord>();
         public readonly Arry<WidgetRecord> Widgets = new Arry<WidgetRecord>();
         public readonly WidgetGroupRecord GroupUp = new WidgetGroupRecord();
         public readonly WidgetGroupRecord GroupDown = new WidgetGroupRecord();
         public readonly WidgetGroupRecord GroupLeft = new WidgetGroupRecord();
         public readonly WidgetGroupRecord GroupRight = new WidgetGroupRecord();

         public void ReadFromRecord(GateDockApp app)
         {
            foreach (var wdg_rep in Widgets.Items)
            {
               app.MainForm.MthWidgetShow((wdg_rep?.ReadFromRepo(app)).NnOrCrash(),
                  GateDockWidgetStateFlags.floating, wdg_rep.NnOrCrash().FloatLocation);
            }

            foreach (var tab_rep in Tabs.Items)
            {
               var cts = tab_rep.ReadFromRepo(app);

               if (cts.Length > 0) { app.MainForm.MthTabAdd(cts, GateDockTabStateEnum.floating, null, tab_rep.FloatLocation); }
            }
         }

         public void WriteToRecord(GateDockApp app)
         {
            var flo_wds = app.MainForm.PpAllWidgets.Where(w => w.PpDockState == GateDockWidgetStateFlags.floating).ToArray();

            foreach (var wdg in flo_wds)
            {
               if (wdg.ParentForm != null)//only widget with factory can be saved
               {
                  var wdg_rec = new WidgetRecord();

                  wdg_rec.FloatLocation = wdg.ParentForm.Location;
                  wdg_rec.WriteToRecord(wdg);
                  Widgets.AddParam(wdg_rec);
               }
            }

            var tbs = app.MainForm.PpTabsAll.Where(t => t.PpState == GateDockTabStateEnum.floating).ToArray();

            foreach (var tab in tbs)
            {
               if (tab.ParentForm != null)
               {
                  var tab_rec = new TabRecord();

                  tab_rec.FloatLocation = tab.ParentForm.Location;
                  tab_rec.WriteToRepo(tab);
                  Tabs.AddParam(tab_rec);
               }
            }
         }
      }

      public class TabRecord : AppParam.Record, IWithDockOrder
      {
         public TabRecord() : base("Tab") { }

         /// <summary>
         /// 
         /// </summary>
         public class TabItem : Record
         {
            private const string CTRL_GUID_FIELD = "CtrlGuid";

            public TabItem()
            {
               if (!SubRecords.All(r => r.SubParams.Any(r1 => r1.ParamName == CTRL_GUID_FIELD)))
               {
                  throw new Crash();
               }
            }

            public readonly TabPageRecord TabPage = new TabPageRecord();
            public readonly DocuTabPageRecord DocuPage = new DocuTabPageRecord();
            public readonly WidgetRecord WidgetPage = new WidgetRecord();

            public Record? CurrParam
            {
               get
               {
                  if (!TabPage.CtrlGuid.Value.IsBlank()) { return TabPage; }
                  else if (!DocuPage.CtrlGuid.Value.IsBlank()) { return DocuPage; }
                  else if (!WidgetPage.CtrlGuid.Value.IsBlank()) { return WidgetPage; }
                  else { return null; }
               }
            }

            protected override void myActionOnAnyChange(AppParam changedParamField)
            {
               if (changedParamField is Record rec)
               {
                  base.myActionOnAnyChange(changedParamField);

                  var pg = rec.SubParams.OfType<Scalar>().FirstOrDefaultUnique(p1 => p1.ParamName == CTRL_GUID_FIELD) ?? throw new Crash();

                  if (pg.ObjValue is string s2 && !s2.IsBlank())
                  {
                     foreach (var or in SubRecords.Except([rec]))
                     {
                        var p = or.SubParams.OfType<Scalar>().FirstOrDefault(p1 => p1.ParamName == CTRL_GUID_FIELD) ?? throw new Crash();

                        if (p.ObjValue is string s && !s.IsBlank())
                        {
                           throw new Crash();
                        }
                     }
                  }
               }
               else
               {
                  throw new Crash();
               }
            }
         }

         public readonly Arry<TabItem> TabItems = new Arry<TabItem>();
         public readonly Simple<int> FloatLeft = new Simple<int>();
         public readonly Simple<int> FloatTop = new Simple<int>();
         public readonly Simple<int> DockOrder = new Simple<int>();

         public Point FloatLocation
         {
            get => new Point(FloatLeft.Value, FloatTop.Value);
            set
            {
               FloatLeft.Value = value.X;
               FloatTop.Value = value.Y;
            }
         }

         int IWithDockOrder.DockOrder { get => DockOrder.Value; set => DockOrder.Value = value; }

         public void WriteToRepo(GateDockTabCtrl tab)
         {
            WriteProperties(tab, false);

            foreach (var tab_ctr in tab.PpAllControls)
            {
               var is_sav_ena = true;

               if (tab_ctr is GateDockTabPageCtrl pag && pag.PpFactory is GateDockTabPagePureFactory pur_fac)
               {
                  is_sav_ena = pur_fac.IsSavingToParams;
               }

               if (is_sav_ena)
               {
                  var tab_itm = new TabItem();

                  TabItems.AddParam(tab_itm);

                  if (tab_ctr is IGateDockDocu doc) { tab_itm.DocuPage.WriteToRecord(doc); }
                  else if (tab_ctr is GateDockTabPageCtrl tab_pag) { tab_itm.TabPage.WriteToRecord(tab_pag); }
                  else if (tab_ctr is GateDockWidgetCtrl wdg) { tab_itm.WidgetPage.WriteToRecord(wdg); }
               }
            }
         }

         public Control[] ReadFromRepo(GateDockApp app)
         {
            var lst_ctr = new List<Control?>();

            foreach (var tab_itm in TabItems.Items)
            {
               if (tab_itm.CurrParam is DocuTabPageRecord doc_pag_rec)
               {
                  lst_ctr.Add(doc_pag_rec.ReadFromRecord(app) as Control);
               }
               else if (tab_itm.CurrParam is TabPageRecord tab_pag_rec)
               {
                  lst_ctr.Add(tab_pag_rec.ReadFromRecord(app));
               }
               else if (tab_itm.CurrParam is WidgetRecord wdg_rec)
               {
                  lst_ctr.Add(wdg_rec.ReadFromRepo(app));
               }
            }

            return lst_ctr.Nn().ToArray();
         }
      }

      public class TabPageRecord : AppParam.Record
      {
         public TabPageRecord() : base("TabPage") { }

         protected TabPageRecord(string name) : base(name) { }

         /// <summary>
         /// 
         /// </summary>
         public readonly Simple<string> Title = new Simple<string>();

         /// <summary>
         /// 
         /// </summary>
         public readonly Simple<string> CtrlGuid = new Simple<string>();

         public GateDockTabPageCtrl? ReadFromRecord(GateDockApp app)
         {
            var fac = app.TabPageFactories.FirstOrDefault(f => f.CtrlGuid == CtrlGuid.Value) as GateDockTabPagePureFactory ?? throw new Crash();

            if (fac == null) { return null; }
            else
            {
               var ctr = fac.MakeTabPageControl(app.MainForm);

               ctr.PpTitle = Title.Value;
               ReadProperties(ctr, false);
               fac.ReadingFromParams(this);

               return ctr;
            }
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="tabPage"></param>
         /// <exception cref="Crash"></exception>
         public void WriteToRecord(GateDockTabPageCtrl tabPage)
         {
            var fac = tabPage.PpFactory as GateDockTabPagePureFactory ?? throw new Crash();

            Title.Value = tabPage.PpTitle;
            WriteProperties(tabPage, false);
            WriteProperties(tabPage.PpFactory.NnOrCrash(), false);
            fac.SavingToParams(this);
         }
      }

      public class DocuTabPageRecord : AppParam.Record
      {
         public DocuTabPageRecord() : base("Docu") { }

         public IGateDockDocu? ReadFromRecord(GateDockApp app)
         {
            var fac = app.DocuFactories.FirstOrDefault(f => f.DocuTypeGuid == DocuTypeGuid.Value);

            if (fac == null) { return null; }
            else
            {
               var ctr = fac.MakeTabPageControl(app.MainForm);

               ctr.PpFactory = fac;
               ctr.PpTitle = Title.Value;

               if (ctr is IGateDockDocu doc_ctr)
               {
                  doc_ctr.PpDocuName = DocuName.Value;

                  if (DocuPath.Value == "")
                  {
                     var pth = Path.Combine(app.OpenFilesDir, doc_ctr.PpDocuName.Nn());

                     if (File.Exists(pth))
                     {
                        doc_ctr.MthOpenFile(pth);
                        doc_ctr.PpDocuPath = "";//in order to associate to path (and activate observer)
                     }
                     else { return null; }
                  }
                  else if (File.Exists(DocuPath.Value)) { doc_ctr.MthOpenFile(doc_ctr.PpDocuPath = DocuPath.Value); }
                  else { return null; }//file discarded
               }

               ReadProperties(ctr, false);

               return (IGateDockDocu)ctr;
            }
         }

         public void WriteToRecord(IGateDockDocu docu)
         {
            var tab_pag = (GateDockTabPageCtrl)docu;

            Title.Value = tab_pag.PpTitle;

            if (tab_pag is IGateDockDocu cnt_doc)
            {
               DocuPath.Value = cnt_doc.PpDocuPath;
               DocuName.Value = cnt_doc.PpDocuName;
            }
            else { throw new Crash(); }

            WriteProperties(tab_pag, false);
            WriteProperties(tab_pag.PpFactory.NnOrCrash(), false);
         }

         /// <summary>
         /// 
         /// </summary>
         public readonly Simple<string> DocuTypeGuid = new Simple<string>();
         public readonly Simple<string> CtrlGuid = new Simple<string>();

         /// <summary>
         /// 
         /// </summary>
         public readonly Simple<string> DocuPath = new Simple<string>();

         /// <summary>
         /// 
         /// </summary>
         public readonly Simple<string> Title = new Simple<string>();

         public readonly Simple<string> DocuName = new Simple<string>();

         public readonly Simple<int> Width = new Simple<int>();

         public readonly Simple<int> Height = new Simple<int>();

         public readonly Simple<string> ContentDescriptor = new Simple<string>();
      }

      public class WidgetGroupRecord : AppParam.Record, IWithDockOrder
      {
         public WidgetGroupRecord() { }

         public readonly Simple<DockableAreaCtrlSlotAnchorModeEnum> AnchorMode =
            new Simple<DockableAreaCtrlSlotAnchorModeEnum>();

         public readonly Arry<WidgetRecord> Widgets = new Arry<WidgetRecord>();

         public readonly Simple<int> DockOrder = new Simple<int>();

         public readonly Simple<int> Width = new Simple<int>();

         public readonly Simple<int> Height = new Simple<int>();

         public Size Size => new Size(Width.Value, Height.Value);

         int IWithDockOrder.DockOrder { get => DockOrder.Value; set => DockOrder.Value = value; }

         public void WriteToRepo(GateDockWidgetGroupCtrl widgetGroup)
         {
            widgetGroup.MthSaveLast();
            WriteProperties(widgetGroup, false);

            var siz = widgetGroup.PpSavedSize ?? widgetGroup.Size;

            Width.Value = siz.Width;
            Height.Value = siz.Height;
            AnchorMode.Value = widgetGroup.PpAnchorMode;

            foreach (var wdg in widgetGroup.PpWidgets)
            {
               var wdg_rec = new WidgetRecord();

               wdg_rec.WriteToRecord(wdg);
               Widgets.AddParam(wdg_rec);
            }
         }

         public void ReadWidgets(GateDockApp app)
         {
            foreach (var wdg_rec in Widgets.Items)
            {
               var wdg = wdg_rec.ReadFromRepo(app).NnOrCrash();

               app.MainForm.MthWidgetShow(wdg, wdg_rec.DockStateParam.Value);
            }
         }
      }

      public class WidgetRecord : AppParam.Record, IWithDockOrder
      {
         public WidgetRecord() : base("Widget") { }

         /// <summary>
         ///  
         /// </summary>
         public readonly Simple<int> DockOrder = new Simple<int>();

         /// <summary>
         /// 
         /// </summary>
         public readonly Simple<string> CtrlGuid = new Simple<string>();

         public readonly Simple<int> FloatLeft = new Simple<int>();
         public readonly Simple<int> FloatTop = new Simple<int>();
         public readonly Simple<int> Width = new Simple<int>();

         public readonly Simple<int> Height = new Simple<int>();

         public readonly Simple<GateDockWidgetStateFlags> DockStateParam = new Simple<GateDockWidgetStateFlags>("DockState", null, GateDockWidgetStateFlags.invisible);

         public readonly Simple<string> ContentDescriptor = new Simple<string>();

         public GateDockWidgetStateFlags DockState
         {
            get => DockStateParam.Value;

            set => DockStateParam.Value = value;
         }

         public Point FloatLocation
         {
            get => new Point(FloatLeft.Value, FloatTop.Value);
            set
            {
               FloatLeft.Value = value.X;
               FloatTop.Value = value.Y;
            }
         }

         int IWithDockOrder.DockOrder { get => DockOrder.Value; set => DockOrder.Value = value; }

         public GateDockWidgetCtrl? ReadFromRepo(GateDockApp app)
         {
            var fac = app.WidgetFactories.FirstOrDefault(f => f.CtrlGuid == CtrlGuid.Value);//todo put on log

            if (fac != null)
            {
               var wdg_ctr = app.AppWidgets.First(w => w.PpFactory == fac);

               ReadProperties(wdg_ctr, false);
               wdg_ctr.PpFactory = fac;

               return wdg_ctr;
            }
            else
            {
               app.Messages.Add(new Msg(MsgType.fail, $"Can't find a widget factory for {CtrlGuid.Value}!"));

               return null;
            }
         }

         public void WriteToRecord(GateDockWidgetCtrl widget)
         {
            DockState = widget.PpDockState;

            widget.MthSaveLast();

            WriteProperties(widget, false);
            WriteProperties(widget.PpFactory.NnOrCrash(), false);

            var siz = widget.PpSavedSize ?? widget.Size;

            Width.Value = siz.Width;
            Height.Value = siz.Height;
         }
      };

      public class FormStateRecord : AppParam.Record
      {
         public FormStateRecord() : base("FormState") { }

         public readonly Simple<int> Left = new Simple<int>();
         public readonly Simple<int> Top = new Simple<int>();
         public readonly Simple<int> Width = new Simple<int>();
         public readonly Simple<int> Height = new Simple<int>();
         public readonly Simple<FormWindowState> WindowState = new Simple<FormWindowState>(FormWindowState.Normal);

         public void ReadFromRecord(Form form)
         {
            var lft = Left.Value;
            var top = Top.Value;
            var wdt = Width.Value;
            var hei = Height.Value;

            ReadProperties(form, true);

            var wrk_rc = Screen.FromHandle(form.Handle).WorkingArea;
            var frm_rc = new Rectangle(lft, top, wdt, hei);

            if (!frm_rc.IntersectsWith(wrk_rc))
            {
               wdt = Math.Max(10, Math.Min(wrk_rc.Width, wdt));
               hei = Math.Max(10, Math.Min(wrk_rc.Height, hei));
               frm_rc = new Rectangle(0, 0, wdt, hei);
            }

            form.Location = frm_rc.Location;
            form.Size = frm_rc.Size;
         }

         public void WriteToRecord(Form form)
         {
            var bns = (WindowState.Value = form.WindowState) == FormWindowState.Normal ? form.Bounds : form.RestoreBounds;

            Left.Value = bns.Left;
            Top.Value = bns.Top;
            Width.Value = bns.Width;
            Height.Value = bns.Height;
            WindowState.Value = form.WindowState;
         }
      }

      public override string FixedPath => Path.Combine(App.AppFolder, "DefaultScenario.xml");

      public bool AreFilesToSave { get; set; } = true;
      public GateDockApp App { get; }

      public override bool Load(MsgCollection? msgs, string? path = null)
      {
         var res = base.Load(msgs, path);

         myApply(!res);
         myTimer.Interval = 5000;
         myTimer.Enabled = true;
         myTimer.Tick += (s, e) => App.MainForm.MthInvoke(() => Save(msgs));

         return res;
      }

      public override bool Save(MsgCollection? msgs, string? path = null)
      {
         myReRead();

         return base.Save(msgs, path);
      }

      protected virtual void myReRead()
      {
         Params.Clear();
         Params.FormState.WriteToRecord(App.MainForm);
         Params.DockItems.WriteToRecord(App);
         Params.FloatItems.WriteToRecord(App);
      }

      protected virtual void myApply(bool isDefault)
      {
         if (!isDefault)
         {
            Params.FormState.ReadFromRecord(App.MainForm);
            Params.DockItems.ReadFromRecord(App);
            Params.FloatItems.ReadFromRecord(App);
         }
      }

      protected override TxtStringConverter myMakeStringConverter() => new TxtStringConverter.Default();

      protected override AppParamLoadSaver myMakeLoadSaver() => new ByXDoc();
   }
}