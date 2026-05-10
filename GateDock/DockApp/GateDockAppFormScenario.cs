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
using static Gate.Dock.DockApp.GateDockAppFormScenario;
using static Gate.Dock.DockApp.GateDockAppFormScenario.TabRecord;
using static Gate.Tools.AppParams.AppParamLoadSaver;
using Timer = System.Windows.Forms.Timer;

namespace Gate.Dock.DockApp
{
   public class GateDockAppFormScenario : AppParamContainerSpecialized<ScenarioParams>
   {
      private readonly Timer myTimer = new Timer();

      public GateDockAppFormScenario(GateDockApp app) => App = app;

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
         public readonly Arry<WidgetGroupRecord> Groups = new Arry<WidgetGroupRecord>();

         public void ReadFromRecord(GateDockApp app)
         {
            foreach (var itm in SubItems) { myDoReadFromRepo((dynamic)itm, app); }
         }

         public void WriteToRecord(GateDockApp app)
         {
            var dck_ctr = app.MainForm.MthGetNephew<DockableAreaCtrl>();

            foreach (var ctr in dck_ctr?.PpControlsDocked ?? []) { myDoWriteToRepo((dynamic)ctr); }
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

         private void myDoReadFromRepo(Arry<WidgetGroupRecord> arryWidgetGroupRecord, GateDockApp app)
         {
            foreach (var wdg_gru_rec in arryWidgetGroupRecord.Items)
            {
               var wds = wdg_gru_rec.WidgetWrappers.Select(w => w.ReadFromRepo(app)).Nn().ToArray();

               app.MainForm.MthGroupWidgets(wdg_gru_rec.AnchorMode.Value, wdg_gru_rec.Size, wds);
            }
         }

         private void myDoReadFromRepo(AppParam appParam, GateDockApp app) { }//do nothing

         private void myDoReadFromRepo(object par, GateDockApp app) => throw new Crash();

         private void myDoWriteToRepo(GateDockTabCtrl tabCtrl)
         {
            var tab_wrp = new TabRecord();

            foreach (var ctr in tabCtrl.PpAllControls) { myDoWriteToTabWrapper((dynamic)ctr, tab_wrp); }

            Tabs.AddParam(tab_wrp);
         }

         private void myDoWriteToRepo(GateDockWidgetCtrl widgetCtrl)
         {
            if (widgetCtrl.PpFactory != null)//only widget with factory can be saved
            {
               var wdg_rec = new WidgetRecord();

               wdg_rec.WriteToRecord(widgetCtrl);
               Widgets.AddParam(wdg_rec);
            }
         }

         private void myDoWriteToRepo(GateDockWidgetGroupCtrl widgetGroupCtrl)
         {
            var gru_wrp = new WidgetGroupRecord();

            foreach (var wdg in widgetGroupCtrl.PpWidgets)
            {
               var wdg_rec = new WidgetRecord();

               wdg_rec.WriteToRecord(wdg);
               gru_wrp.Widgets.AddParam(wdg_rec);
            }

            gru_wrp.WriteToRepo(widgetGroupCtrl);
         }

         private void myDoWriteToTabWrapper(Control ctr, TabRecord tabRepo) => throw new Crash();

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

      public class TabRecord : AppParam.Record
      {
         public TabRecord() : base("Tab") { }

         public readonly Arry<TabItem> TabItems = new Arry<TabItem>();
         public readonly Simple<int> FloatLeft = new Simple<int>();
         public readonly Simple<int> FloatTop = new Simple<int>();

         public Point FloatLocation
         {
            get => new Point(FloatLeft.Value, FloatTop.Value);
            set
            {
               FloatLeft.Value = value.X;
               FloatTop.Value = value.Y;
            }
         }

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

                  var pg = rec.SubParams.OfType<Scalar>().FirstOrDefault(p1 => p1.ParamName == CTRL_GUID_FIELD) ?? throw new Crash();

                  if (pg.ObjValue is string s2 && !s2.IsBlank())
                  {
                     foreach (var or in SubRecords.Except(new[] { rec }))
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

      public class WidgetGroupRecord : AppParam.Record
      {
         public WidgetGroupRecord() : base("WidgetGroup") { }

         public Arry<WidgetRecord> Widgets = new Arry<WidgetRecord>();

         public void WriteToRepo(GateDockWidgetGroupCtrl widgetGroup)
         {
            WriteProperties(widgetGroup, false);

            foreach (var wdg in widgetGroup.PpWidgets)
            {
               var wdg_rec = new WidgetRecord();

               wdg_rec.WriteToRecord(wdg);
               Widgets.AddParam(wdg_rec);
            }
         }

         public WidgetRecord[] WidgetWrappers => SubItems.OfType<WidgetRecord>().ToArray();

         public readonly Simple<int> Width = new Simple<int>();

         public readonly Simple<int> Height = new Simple<int>();

         public readonly Simple<DockableAreaCtrlSlotAnchorModeEnum> AnchorMode = new Simple<DockableAreaCtrlSlotAnchorModeEnum>(DockableAreaCtrlSlotAnchorModeEnum.none);

         public readonly Simple<int> PpSelectedIndex = new Simple<int>();

         public Size Size => new Size(Width.Value, Height.Value);
      }

      public class WidgetRecord : AppParam.Record
      {
         public WidgetRecord() : base("Widget") { }

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

            WriteProperties(widget, false);
            WriteProperties(widget.PpFactory.NnOrCrash(), false);

            if (widget.ParentForm?.WindowState == FormWindowState.Minimized)
            {
               Width.Value = widget.PpRestoreSize.Width;
               Height.Value = widget.PpRestoreSize.Height;
            }
         }

         /// <summary>
         /// 
         /// </summary>
         public readonly Simple<string> CtrlGuid = new Simple<string>();

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

         public readonly Simple<int> FloatLeft = new Simple<int>();
         public readonly Simple<int> FloatTop = new Simple<int>();
         public readonly Simple<int> Width = new Simple<int>();

         public readonly Simple<int> Height = new Simple<int>();

         public readonly Simple<GateDockWidgetStateFlags> DockStateParam = new Simple<GateDockWidgetStateFlags>("DockState", null, GateDockWidgetStateFlags.invisible);

         public readonly Simple<string> ContentDescriptor = new Simple<string>();
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