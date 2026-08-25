using Gate.Dock.DockFactories;
using Gate.Dock.DockFactories.Text;
using Gate.Dock.DockSkin;
using Gate.Dock.DockTab;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.ToolsView.ControlFeature.Extensions;
using Gate.ToolsView.Extended;
using Gate.ToolsView.Extensions;
using Gate.ToolsView.FileChange;
using Gate.ToolsView.MenuCommand;
using Gate.ToolsView.MenuExtended;
using Gate.ToolsView.TextCtrl;
using ScintillaNET.Gate;
using static Gate.ToolsView.TextCtrl.GateTextControl;

namespace Gate.Dock.DockDocu
{
   public delegate void OnBreakpointsChangedHandler(object? sender, GateDockDocuMarkerBreakpoint[]? breakpoints);
   public delegate void OnBoomarksChangedHandler(object? sender, GateDockDocuMarkerBookmark[]? bookmarks);
   public delegate void OnSaveHandler(object? sender, string path);

   /// <summary>
   /// 
   /// </summary>
   public partial class GateDockDocuTextCtrl : GateDockTabPageCtrl, IGateDockDocuText
   {
      public event OnBreakpointsChangedHandler? OnBreakpointsChanged;
      public event OnBoomarksChangedHandler? OnBoomarksChanged;
      public event OnSaveHandler? OnSave;

      private readonly Dictionary<ScintillaMarkerWrapper, GateDockDocuMarkerBookmark>
         myDictionaryBookmarkByScintillaMarker = new Dictionary<ScintillaMarkerWrapper, GateDockDocuMarkerBookmark>();
      private readonly FileChangeObserver myFileChangeObserver = new FileChangeObserver();
      private string? myDocuName = "";
      private TxtToken? myExecutionToken;

      public event EventHandler OnDocuPathChange
      {
         add { CtrlText.OnOpenPathChange += value; }

         remove { CtrlText.OnOpenPathChange -= value; }
      }

      public GateDockDocuTextCtrl()
      {
         InitializeComponent();

         PpSkinChildCtrlDispacther = new SkinChildCtrlDispactherForText(this);
         CtrlText.PpOpenPath = "";
         CtrlText.PpScintilla.ContextMenuStrip = new ExtendedMenuDropDown();
         myFileChangeObserver.OnFileEvent += FileChangeObserver_OnFileEvent;
         CtrlText.PpScintillaMarkers[GateTextMarkerScintillaIdEnum.marker_0_bookmark].SetBackColor(Color.Blue);
         CtrlText.PpToolWinGoto.AddFeature<GateDockToolWinFeature>();
      }

      public class SkinChildCtrlDispactherForText : GateDockTabPageCtrlSkinDispacther
      {
         private readonly UpdateVisitor3 myUpdateVisitor = new UpdateVisitor3();
         private readonly Control[] myTextControlHierarchy;

         public SkinChildCtrlDispactherForText(GateDockDocuTextCtrl parent) : base(parent)
         {
            var txt_ctr = parent.MthGetNephew<GateTextControl>();

            myTextControlHierarchy = [.. txt_ctr?.MthGetNephews() ?? [], .. new Control?[] { txt_ctr }.Nn()];
         }

         protected class UpdateVisitor3 : UpdateVisitor2
         {
            public override void Visit(GateDockSkin skin, Control control)
            {
               control.BackColor = skin.Params.BackFrameColor.Value;
               control.BackColor = skin.Params.BackContentColor.Value;
               control.Font = skin.Params.ControlsFont.Value;
            }

            public virtual void Visit(GateDockSkin skin, GateTextControl textControl)
            {
               textControl.ForeColor = skin.Params.ForeColor.Value;
               textControl.BackColor = skin.Params.BackContentColor.Value;
               textControl.PpLexer = textControl.PpLexer;//cause style refresh
            }

            public virtual void Visit(GateDockSkin skin, ExtendedScrollBar scrollBar)
            {
               scrollBar.ForeColor = skin.Params.ForeColor.Value;
               scrollBar.BackColor = skin.Params.ScrollBarBackColor.Value;
               scrollBar.PpArrowColor = skin.Params.ScrollBarArrowColor.Value;
               scrollBar.PpGripColor = skin.Params.ScrollBarGripColor.Value;
               scrollBar.PpGripActiveColor = skin.Params.ScrollBarpGripActiveColor.Value;

               if (scrollBar.Orientation == ExtendedScrollBarOrientationEnum.Horizontal)
               {
                  scrollBar.Height = GateDockSkin.DefaultValues.ScrollBarDocuSize;
               }
               else
               {
                  scrollBar.Width = GateDockSkin.DefaultValues.ScrollBarDocuSize;
               }
            }
         }

         protected override void myUpdate(GateDockSkin skin, Control control)
         {
            if (myTextControlHierarchy.Contains(control)) { myUpdateVisitor.Visit(Skin, (dynamic)control); }
            else { base.myUpdate(skin, control); }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public bool PpIsReadOnly { get => CtrlText.PpIsReadOnly; set => CtrlText.PpIsReadOnly = value; }

      /// <summary>
      /// 
      /// </summary>
      public bool PpAreLineNumberActive { get => CtrlText.PpAreLineNumberActive; set => CtrlText.PpAreLineNumberActive = value; }

      /// <summary>
      /// 
      /// </summary>
      public string? PpDocuPath
      {
         get => CtrlText.PpOpenPath;

         set
         {
            if ((value ?? "").Trim() != "") { PpDocuName = Path.GetFileName(value); }

            myFileChangeObserver.FilePath = CtrlText.PpOpenPath = value ?? "";
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override GateDockMainForm? PpMainFrm
      {
         get => base.PpMainFrm;

         internal set
         {
            base.PpMainFrm = value;

            if (
               PpFactory is GateDockCtrlFactoryDocuText fac &&
               PpMainFrm?.PpCmdMainMenu != null &&
               PpMainFrm.PpCmdMainMenu.CmdContainer != null)
            {
               var ctx_men = PpMainFrm.PpCmdMainMenu.CmdContainer.AllMenus.FirstOrDefault(c => c.Id == fac.MenuContextId);

               if (ctx_men != null)
               {
                  var men = CtrlText.PpScintilla.ContextMenuStrip as ExtendedMenuDropDown;

                  if (men != null)
                  {
                     men.PpCmdMenuRef = new CmdMenu.Ref(ctx_men, true);
                  }
               }
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public bool PpIsModified { get => CtrlText.PpIsModified; set => CtrlText.PpIsModified = value; }

      /// <summary>
      /// 
      /// </summary>
      public bool PpIsDocuNotEmpty => CtrlText.PpContentText != "";

      /// <summary>
      /// 
      /// </summary>
      public string? PpDocuName
      {
         get => myDocuName;
         set
         {
            myDocuName = value;
            PpTitle = CtrlText.PpIsModified ? $"{PpDocuName} *" : PpDocuName;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public GateTextControlFindInfrastructure PpFindInfrastructure { get => CtrlText.PpFindInfrastructure; set => CtrlText.PpFindInfrastructure = value; }

      /// <summary>
      /// 
      /// </summary>
      public int PpCurrLine
      {
         get => CtrlText.PpCurrLine;

         set => CtrlText.PpCurrLine = value;
      }

      /// <summary>
      /// 
      /// </summary>
      public int PpCurrCol
      {
         get => CtrlText.PpCurrCol;

         set => CtrlText.PpCurrCol = value;
      }

      /// <summary>
      /// 
      /// </summary>
      public GateDockDocuMarkerBreakpoint[]? PpBreakpoints
      {
         get =>
            CtrlText.PpScintillaIndicators[GateTextIndicatorScintillaIdEnum.indicator_9_breakpoints].
            Select(
               bkp_ind => new GateDockDocuMarkerBreakpoint(
                  GateDockDocuMarkerHandler.GetMarkerPath(this).NnOrCrash(),
                  bkp_ind.LineStart, bkp_ind.ColStart, bkp_ind.Len)).ToArray();

         set
         {
            CtrlText.MthIndicatorOffAll(GateTextIndicatorScintillaIdEnum.indicator_9_breakpoints);

            foreach (var brk in value ?? []) { myDoMakeBreakpoint(brk.Line, brk.Column, brk.TextLen); }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public GateDockDocuMarkerBookmark[]? PpBookmarks
      {
         get => CtrlText.PpAllMarkers.Where(m => m.ScintillaId == GateTextMarkerScintillaIdEnum.marker_0_bookmark).
            Select(m1 => myDictionaryBookmarkByScintillaMarker[m1]).ToArray();

         set
         {
            foreach (var mrk in CtrlText.PpAllMarkers.Where(m => m.ScintillaId == GateTextMarkerScintillaIdEnum.marker_0_bookmark)) { CtrlText.MthScintillaMarkerDelete(mrk); }

            foreach (var bok in value ?? []) { myDoBookmarkAdd(bok); }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public GateDockDocuMarkerBreakpoint.Positioner? PpBreakpointPositioner { get; set; } = new GateDockDocuMarkerBreakpoint.Positioner.Standard();

      /// <summary>
      /// 
      /// </summary>
      public FoldZoneGetter? PpFoldZoneGetter { get => CtrlText.PpFoldZoneGetter; set => CtrlText.PpFoldZoneGetter = value; }

      /// <summary>
      /// 
      /// </summary>
      public int PpTabSpaces { get => CtrlText.PpTabSpaces; set => CtrlText.PpTabSpaces = value; }

      /// <summary>
      /// 
      /// </summary>
      public bool PpIsUseTab { get => CtrlText.PpIsUseTab; set => CtrlText.PpIsUseTab = value; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="path"></param>
      public void MthSaveFile(string path) => MthSaveFileCopy(PpDocuPath = path);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="path"></param>
      public void MthSaveFileCopy(string path)
      {
         CtrlText.MthSaveFile(path);
         OnSave?.Invoke(this, path);
      }

      public void MthOpenFile(string path)
      {
         if (File.Exists(path))
         {
            PpDocuPath = path;
            CtrlText.MthOpenFile(path);
            myFileChangeObserver.FilePath = path;
            PpIsReadOnly = myFileChangeObserver.IsReadOnly;
         }
      }

      public void MthUndo() => CtrlText.MthUndo();

      public void MthRedo() => CtrlText.MthRedo();

      public void MthSelectTokenFromOtherControl(TxtToken token) => CtrlText.MthSelectTokenFromOtherControl(token);

      public void MthGoToWindow() => CtrlText.MthGoToWindow();

      public void MthSelectAll() => CtrlText.PpScintilla.SelectAll();

      public bool MthReopen()
      {
         if (File.Exists(PpDocuPath))
         {
            try
            {
               //line-2-line comparition
               var old_cnt = CtrlText.PpContentText;
               var new_cnt = File.ReadAllText(PpDocuPath);

               if (old_cnt != new_cnt)
               {
                  var txt_cmp = new TxtLineComparer();

                  txt_cmp.Compare(old_cnt, new_cnt);

                  var pos = (PpCurrLine, PpCurrCol);

                  //tododo restore bookmark
                  var old_bks = PpBookmarks?.ToArray() ?? [];
                  var old_bps = PpBreakpoints?.ToArray() ?? [];

                  CtrlText.PpContentText = new_cnt;
                  PpIsModified = false;

                  if (txt_cmp.AreIdentical)
                  {
                     PpCurrLine = pos.PpCurrLine;
                     PpCurrCol = pos.PpCurrCol;
                     PpBookmarks = old_bks;
                  }
                  else
                  {
                     var eq_sqs = txt_cmp.Sections.
                        Where(s => s.Type == TxtLineComparer.SectionType.TypeEnum.equal).ToArray();

                     //tododo
                     var lst_bok = new List<GateDockDocuMarkerBookmark>();
                     var lst_bkp = new List<GateDockDocuMarkerBreakpoint>();

                     foreach (var bok in old_bks)
                     {
                        var eq_sec = eq_sqs.FirstOrDefault(s => s.LineIntervalNew1.Contains(bok.Line));

                        if (eq_sec != null)
                        {
                           lst_bok.Add(new GateDockDocuMarkerBookmark(
                              bok.BookmarkPath, bok.Line + eq_sec.LineIntervalNew1.From - eq_sec.LineIntervalOld1.From));
                        }
                     }

                     foreach (var bok in old_bps)
                     {
                        var eq_sec = eq_sqs.FirstOrDefault(s => s.LineIntervalNew1.Contains(bok.Line));

                        if (eq_sec != null)
                        {
                           lst_bkp.Add(new GateDockDocuMarkerBreakpoint(
                              bok.BreakpointPath, 
                              bok.Line + eq_sec.LineIntervalNew1.From - eq_sec.LineIntervalOld1.From ,
                              bok.Column,
                              bok.TextLen));
                        }
                     }

                     PpBookmarks = lst_bok.ToArray();
                     PpBreakpoints = lst_bkp.ToArray();

                     var sec_pos = txt_cmp.Sections.FirstOrDefault(
                        s => s.LineIntervalOld1.Contains(pos.PpCurrLine)).NnOrCrash();

                     if (sec_pos.Type == TxtLineComparer.SectionType.TypeEnum.equal)
                     {
                        var new_lin = pos.PpCurrLine - sec_pos.LineIntervalOld1.From + sec_pos.LineIntervalNew1.From;

                        PpCurrLine = new_lin;
                        PpCurrCol = pos.PpCurrCol;
                     }
                     else
                     {
                        PpCurrLine = sec_pos.LineIntervalNew1.From;
                        PpCurrCol = 1;
                     }
                  }
               }

               //raises save event
               OnSave?.Invoke(this, PpDocuPath);

               return true;
            }
            catch { return false; }
         }
         return false;
      }

      /// <summary>
      /// Running position
      /// </summary>
      public TxtToken? PpDbgPointCurrent
      {
         get => myExecutionToken;

         set
         {
            //check
            if (value != null && (value?.Store?.FileInfo == null || value.Store.FileInfo.FullName != PpDocuPath))
            {
               throw new Gate.Dock.GateDockException($"Token shall be associated to same file!");
            }

            CtrlText.MthIndicatorOffAll(GateTextIndicatorScintillaIdEnum.indicator_10_run_markers);

            if (myExecutionToken != null)
            {
               //delete marker
               foreach (var run_mrk in CtrlText.PpAllMarkers.Where(m => m.ScintillaId == GateTextMarkerScintillaIdEnum.marker_1_run_marker))
               {
                  CtrlText.MthScintillaMarkerDelete(run_mrk);
               }

               //delete indicator
               var ifs = CtrlText.PpScintillaIndicators[GateTextIndicatorScintillaIdEnum.indicator_10_run_markers].Where(i => i.IsIn).ToArray();

               if (ifs.Length > 0) { CtrlText.MthIndicatorOff(ifs[0]); }
            }

            if ((myExecutionToken = value) != null)
            {
               var fro = myExecutionToken.From.NnOrCrash();

               //sets bookmark
               CtrlText.MthScintilaMarkerAdd(fro.Line, GateTextMarkerScintillaIdEnum.marker_1_run_marker);

               //sets indicator
               myDoMakeRunMarkerIndicator(fro.Line, fro.Col, myExecutionToken.Length);
            }
         }
      }

      public void MthToggleBookmark()
      {
         var bok_mrk = CtrlText.PpAllMarkers.Where(m => m.ScintillaId == GateTextMarkerScintillaIdEnum.marker_0_bookmark).FirstOrDefault(m => m.Line == PpCurrLine);

         if (bok_mrk != null)
         {
            myDictionaryBookmarkByScintillaMarker.Remove(bok_mrk);
            CtrlText.MthScintillaMarkerDelete(bok_mrk);
         }
         else { myDoBookmarkAdd(new GateDockDocuMarkerBookmark(GateDockDocuMarkerHandler.GetMarkerPath(this).NnOrCrash(), PpCurrLine)); }

         OnBoomarksChanged?.Invoke(this, PpBookmarks ?? []);
      }

      public void MthToggleBreakpoint()
      {
         var ifs = CtrlText.PpScintillaIndicators[GateTextIndicatorScintillaIdEnum.indicator_9_breakpoints].
            Where(i => i.IsIn).
            ToArray();

         if (ifs.Length > 0)
         {
            CtrlText.MthIndicatorOff(ifs[0]);
            OnBreakpointsChanged?.Invoke(this, PpBreakpoints);
         }
         else
         {
            var sto = new TxtStore(CtrlText.PpContentText);
            var tok = PpBreakpointPositioner?.GetPosition(sto, PpCurrLine, PpCurrCol);

            if (tok != null)
            {
               var fro = tok.From.NnOrCrash();

               myDoMakeBreakpoint(fro.Line, fro.Col, tok.Length);
               OnBreakpointsChanged?.Invoke(this, PpBreakpoints);
            }
         }
      }

      public void MthOutlineToggle(int line = -1) => CtrlText.MthOutlineToggle(line);

      public void MthOutlineToggleAll() => CtrlText.MthOutlineToggleAll();

      public void MthOutlineCollapseToFunction() => CtrlText.MthOutlineCollapseToFunction();

      public override string ToString() => $"{((PpDocuPath ?? "") != "" ? PpDocuPath : PpDocuName)}";

      protected override void OnLoad(EventArgs e)
      {
         base.OnLoad(e);

         PpSkin = PpSkin;
      }

      protected override void myActionOnGotFocus(object? sender)
      {
         this.MthGetNephew<ScintillaExtension>()?.Focus();
         base.myActionOnGotFocus(sender);
      }

      protected override void myActionOnFactoryChange(GateDockTabPageFactory? factoryDocu)
      {
         var fac = factoryDocu as GateDockCtrlFactoryDocuText;

         PpBreakpointPositioner = fac?.BreakpointPositioner;
         PpFoldZoneGetter = fac?.FoldZoneGetter;
         CtrlText.PpLexer = fac?.Lexer;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="bookmark"></param>
      private void myDoBookmarkAdd(GateDockDocuMarkerBookmark bookmark) => myDictionaryBookmarkByScintillaMarker[CtrlText.MthScintilaMarkerAdd(bookmark.Line, GateTextMarkerScintillaIdEnum.marker_0_bookmark)] = bookmark;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="line"></param>
      /// <param name="col"></param>
      /// <param name="length"></param>
      private void myDoMakeBreakpoint(int line, int col, int length) => CtrlText.MthIndicatorOn(GateTextIndicatorScintillaIdEnum.indicator_9_breakpoints, line, col, length, Color.Red, 50, 255);


      /// <summary>
      /// 
      /// </summary>
      /// <param name="line"></param>
      /// <param name="col"></param>
      /// <param name="length"></param>
      private void myDoMakeRunMarkerIndicator(int line, int col, int length)
      {
         CtrlText.MthIndicatorOn(GateTextIndicatorScintillaIdEnum.indicator_10_run_markers, line, col, length, Color.Yellow, 50, 255);
         CtrlText.PpCurrLine = line;
         CtrlText.PpCurrCol = col;
      }

      /// <summary>
      /// Causes update of PpTitle. 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void CtrlText_OnSavePointLeft(object? sender, EventArgs e) => PpDocuName = PpDocuName;

      /// <summary>
      /// Causes update of PpTitle. 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void CtrlText_OnSavePointReached(object? sender, EventArgs e) => PpDocuName = PpDocuName;

      private void CtrlText_DragDrop(object? sender, DragEventArgs e)
      {
         var fls = e.Data?.GetData(DataFormats.FileDrop) as string[];

         if (fls?.Length == 1) { PpMainFrm?.PpDocuHandler.OpenPath(PpMainFrm, fls[0], PpParentTab); }
      }

      private void CtrlText_DragEnter(object? sender, DragEventArgs e) =>
         e.Effect = e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;

      private void FileChangeObserver_OnFileEvent(object? sender, FileChangeEventArgs eventArgs)
      {
         PpIsReadOnly = myFileChangeObserver.IsReadOnly;

         switch (eventArgs.FileChangeType)
         {
            case FileChangeType.removed:
            case FileChangeType.read_only_changed:
               break;

            case FileChangeType.replaced:
            case FileChangeType.modified:
               MthReopen();
               break;

            default: throw new Crash();
         }
      }
   }
}
