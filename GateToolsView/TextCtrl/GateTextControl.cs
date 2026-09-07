using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Multithread;
using Gate.Tools.Text;
using Gate.Tools.Text.Encode;
using Gate.ToolsView.Extensions;
using Gate.ToolsView.MenuCommand;
using Gate.ToolsView.TextCtrl.Lexers;
using Gate.ToolsView.TextSearch;
using ScintillaNET;
using ScintillaNET.Gate;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms.Layout;

namespace Gate.ToolsView.TextCtrl
{
   public delegate void OnGateTextControlSelectionChangeHandler(GateTextControl sender);
   public delegate void OnDocumentInsertHandler(object? sender, int documentPos, string insertedText);
   public delegate void OnDocumentDeleteHandler(object? sender, int documentPos, int numCharDeleted);

   /// <summary>
   /// Remember compile as X86 otherwise scintilla goes wrong in find.
   /// </summary>
   public partial class GateTextControl : UserControl, IControlWithManagedCmds
   {
      public const int LINE_NUMBER_MARGIN_IDX = 0;
      public const int BOOKMARK_MARGIN_IDX = 1;
      public const int FOLD_MARGIN_IDX = 2;

      private event OnCmdStateUpdateHandler? myOnCmdStateUpdate;

      public event OnDocumentInsertHandler OnDocumentInsert;
      public event OnDocumentDeleteHandler OnDocumentDelete;

      public event EventHandler<EventArgs> OnSavePointLeft
      {
         add { PpScintilla.SavePointLeft += value; }
         remove { PpScintilla.SavePointLeft -= value; }
      }

      public event EventHandler<EventArgs> OnSavePointReached
      {
         add { PpScintilla.SavePointReached += value; }
         remove { PpScintilla.SavePointReached -= value; }
      }

      event OnCmdStateUpdateHandler IControlWithManagedCmds.OnCmdStateUpdate
      {
         add => myOnCmdStateUpdate += value;

         remove => myOnCmdStateUpdate -= value;
      }

      public event EventHandler? OnOpenPathChange;
      public event EventHandler? OnTextChange;

      private readonly List<GateTextLineAnnotation> myListAnotation = new List<GateTextLineAnnotation>();
      private readonly List<ScintillaMarkerWrapper> myListMarkerWrappers = new List<ScintillaMarkerWrapper>();
      private readonly CmdManagedByControlImpl[] myCommands;
      private bool myIsReadOnly = false;
      private GateTextLexer? myLexer = null;
      private Color myMarginColor = Color.Empty;
      private bool myIsLineNumberActive;
      private Color myScrollBarsColor = Color.Empty;
      private Encoding? myEncoding = null;

      public static Encoding DefaultEncoding = TxtExtraEncodings.Utf8NoBom;
      public static ITxtEncodingRetriever DefaultEncodingRetriever = new InternalRetriever();
      private string myOpenPath = "";
      private bool myIsBookmarkMarginEnabled = true;
      private FoldZoneGetter? myFoldZoneGetter = null;
      private Color myFoldBoxBackColor = Color.Gray;
      private Color myFoldLineColor = Color.DarkGray;
      private bool myToggleFoldDone = false;
      private GateTextLexerPlainText myDefaultLexer = new GateTextLexerPlainText();
      private Color mySelectionBackColor = Color.Green;
      private Color mySelectionForeColor = Color.Empty;
      private ITxtEncodingRetriever? myEncodingRetriever = null;
      private InnerUpdateThread myUpdateThread;

      public GateTextControl()
      {
         InitializeComponent();

         Controls.Add(PpScintilla);
         PpScintilla.ReadOnly = myIsReadOnly;
         PpScintilla.Dock = DockStyle.Fill;
         PpScintilla.StyleResetDefault();
         PpScintilla.StyleClearAll();
         PpScintilla.VScrollBar = false;
         PpScintilla.HScrollBar = false;
         PpScintilla.BorderStyle = BorderStyle.None;
         PpScintilla.UpdateUI += Scintilla_UpdateUI;
         PpScintilla.Insert += PpScintilla_Insert;
         PpScintilla.Delete += PpScintilla_Delete;

         PpScintillaStyles = new GateTextStyle.Collection(PpScintilla);
         PpAnnotationMode = GateTextAnnotationMode.boxed;
         PpMarginColor = BackColor;
         PpScrollBarsColor = PpScrollBarsColor;
         PpScrollBarSize = 25;
         Font = new Font("Consolas", 10);
         PpScintilla.Update();
         SetStyle(ControlStyles.ResizeRedraw, true);
         DoubleBuffered = true;
         PpFindInfrastructure = new InnerFindStrategy(this);
         PpIsBookmarkMarginEnabled = PpIsBookmarkMarginEnabled;
         PpScintillaIndicators = new IndicatorInfo.Collection(this);
         PpScintillaMarkers = new ScintillaMarkerCollection(this);

         PpScintilla.MarginClick += PpScintilla_MarginClick;
         PpScintilla.TextChanged += PpScintilla_TextChanged;

         PpScintilla.SetFoldFlags(FoldFlags.LineAfterContracted);
         PpLexer = PpLexer;
         PpSelectionBackColor = PpSelectionBackColor;
         PpSelectionForeColor = PpSelectionForeColor;
         myUpdateThread = new InnerUpdateThread(this);
         myCommands = [new InnerCommands.Cut(this), new InnerCommands.Copy(this), new InnerCommands.Paste(this)];
         CtrlScrollBarV.PpWheelSensitivityMultiplier = 4;
      }

      private void PpScintilla_Delete(object? sender, ModificationEventArgs e)
      {
         OnDocumentDelete?.Invoke(sender, e.Position, e.Text.Length);

         int a = 2;//throw new NotImplementedException();//tododo 
      }

      private void PpScintilla_Insert(object? sender, ModificationEventArgs e)
      {
         OnDocumentInsert?.Invoke(sender,e.Position,e.Text);

         int a = 2;//throw new NotImplementedException();//tododo
      }

      private class InnerUpdateThread : IDisposable
      {
         private Size? myTxtSize;
         private Thread myInternalThread;
         private Graphics? myGraphics = null;
         private Interval[]? myLineIntervals;
         private readonly ConcurrentQueue<FlagsType> myQueue = new ConcurrentQueue<FlagsType>();
         private readonly Semaphore mySemaphoreRequest = new Semaphore(0, int.MaxValue);
         private readonly Semaphore mySemaphoreReqParams = new Semaphore(0, int.MaxValue);

         private readonly ConcurrentQueue<(Interval[]? lineIntervals, Size?)> myQueueReqParams = new ConcurrentQueue<(Interval[]? lineIntervals, Size?)>();
         private readonly HighlighTask myHighlighTask;
         private readonly UpdateFoldZoneTask myUpdateFoldZoneTask;

         [Flags]
         public enum FlagsType
         {
            none = 0x0,
            text = 0x1,
            content = 0x2,
            resize = 0x4,
            selection = 0x8,

            fold_zone = 0x10,

            req_params = 0x20,
         }

         public InnerUpdateThread(GateTextControl textControl)
         {
            TextControl = textControl ?? throw new Crash();
            myInternalThread = new Thread(myThreadBody);
            myInternalThread.Priority = ThreadPriority.Lowest;
            myInternalThread.IsBackground = true;
            myHighlighTask = new HighlighTask(this);
            myUpdateFoldZoneTask = new UpdateFoldZoneTask(this);
         }

         private class HighlighTask : RestartableBackgroundTask
         {
            private readonly InnerUpdateThread myParent;

            private readonly Stopwatch myStopwatch = new Stopwatch();
            public HighlighTask(InnerUpdateThread parent) => myParent = parent;

            protected override void myTaskBody()
            {
               var txt = "";
               var sel_txt = "";

               if (!IsRunning) { return; }

               myParent.TextControl.MthInvoke(() =>
               {
                  txt = myParent.TextControl.PpContentText;
                  sel_txt = myParent.TextControl.PpSelectedText;
                  myParent.TextControl.MthIndicatorOffAll(GateTextIndicatorScintillaIdEnum.indicator_8);
               });

               if (!IsRunning) { return; }

               var tks = null as TxtToken[];

               if (sel_txt != "")
               {
                  tks = myParent.TextControl.PpFindInfrastructure.
                     FindStrategy.FindAll(sel_txt, TextSearchFlags.WrapAround | TextSearchFlags.WholeWord, txt, null);
               }

               if (tks == null || !IsRunning) { return; }

               for (var idx = 0; IsRunning && idx < tks.Length;)
               {
                  myStopwatch.Restart();

                  myParent.TextControl.MthInvoke(() =>
                  {
                     for (; IsRunning && idx < tks.Length; idx++)
                     {
                        if (myStopwatch.ElapsedMilliseconds > myParent.TextControl.PpBackgroundBatchMaxTimeMs) { break; }

                        var tok = tks[idx];

                        myParent.TextControl.MthIndicatorOn(
                          GateTextIndicatorScintillaIdEnum.indicator_8,
                          (tok.From ?? throw new Crash()).Line,
                          tok.From.Col,
                          tok.Length,
                          myParent.TextControl.ForeColor, 0, 70);
                     }
                  });
               }
            }
         }

         private class UpdateFoldZoneTask : RestartableBackgroundTask
         {
            private readonly InnerUpdateThread myParent;
            private readonly Stopwatch myStopwatch = new Stopwatch();
            private readonly ScintillaExtension myScintilla;

            public UpdateFoldZoneTask(InnerUpdateThread parent)
            {
               myParent = parent;
               myScintilla = myParent.TextControl.PpScintilla;
            }

            protected override void myTaskBody()
            {
               var nl = myScintilla.Lines.Count;
               var cnt_txt = "";

               if (!IsRunning) { return; }

               myParent.TextControl.MthInvoke(() =>
               {
                  cnt_txt = myParent.TextControl.PpContentText;
               });

               if (!IsRunning || myParent.TextControl.PpFoldZoneGetter == null) { return; }

               var roo_zon = myParent.TextControl.PpFoldZoneGetter.GetRootFoldZone(cnt_txt);

               //valid zones: not-root and two or more line long
               var val_zns = roo_zon.AllZones.Skip(1).Where(z => z.NumLines > 1).ToArray();

               var fn = new FileInfo(myParent.TextControl.PpOpenPath ?? "");
               var ln_idx = 0;

               //line index(0-) not beloning to any valid zone (ie not-root and two or more line long).
               var roo_vls = Enumerable.Range(0, nl).Where(i => !val_zns.Any(z => z.Interval.Contains(i))).ToArray();
               var n_roo = roo_vls.Length;

               for (; IsRunning && ln_idx < n_roo;)
               {
                  myStopwatch.Restart();

                  myParent.TextControl.MthInvoke(() =>
                  {
                     for (; IsRunning && ln_idx < n_roo; ln_idx++)
                     {
                        if (myStopwatch.ElapsedMilliseconds > myParent.TextControl.PpBackgroundBatchMaxTimeMs) { break; }

                        var idx = roo_vls[ln_idx];

                        myScintilla.Lines[idx].FoldLevelFlags = 0;
                        myScintilla.Lines[idx].FoldLevel = 1024;
                     }
                  });
               }

               var zns_tst = Enumerable.Range(0, val_zns.Length).Select(i => new bool[val_zns[i].NumLines - 1]).ToArray();

               ln_idx = 1;

               for (var zon_idx = 0; IsRunning && zon_idx < val_zns.Length;)
               {
                  var zon = val_zns[zon_idx];

                  if (IsRunning)
                  {
                     myStopwatch.Restart();

                     myParent.TextControl.MthInvoke(() =>
                     {
                        while (IsRunning && zon_idx < val_zns.Length)
                        {
                           zon = val_zns[zon_idx];
                           var zon_lin = val_zns[zon_idx]?.FromPos?.Line ?? throw new Crash();

                           if (IsRunning && ln_idx == 1)
                           {
                              myScintilla.Lines[zon_lin - 1].FoldLevel = zon.Level + 1023;
                              myScintilla.Lines[zon_lin - 1].FoldLevelFlags = FoldLevelFlags.Header;
                           }

                           for (; IsRunning && ln_idx < zon.NumLines; ln_idx++)
                           {
                              if (myStopwatch.ElapsedMilliseconds > myParent.TextControl.PpBackgroundBatchMaxTimeMs) { return; }

                              zns_tst[zon_idx][ln_idx - 1] = true;
                              myScintilla.Lines[zon_lin - 1 + ln_idx].FoldLevel = zon.Level + 1024;
                           }

                           ln_idx = 1;
                           zon_idx++;
                        }
                     });
                  }
               }
            }
         }

         public void Start(Thread currentThread)
         {
            MsgQueueThread = currentThread;
            myGraphics = Graphics.FromHwnd(TextControl.PpScintilla.Handle);
            myInternalThread.Start();
         }

         /// <summary>
         /// Starts a task is not blocking.
         /// </summary>
         /// <param name="flags"></param>
         /// <exception cref="Crash"></exception>
         /// <exception cref="NotImplementedException"></exception>
         public void Enqueue(FlagsType flags)
         {
            if (MsgQueueThread != null && Thread.CurrentThread != MsgQueueThread) { throw new Crash(); }
            else
            {
               if ((flags & (FlagsType.fold_zone | FlagsType.text)) != 0) { myUpdateFoldZoneTask.Stop(); }

               if ((flags & (FlagsType.selection | FlagsType.text)) != 0) { myHighlighTask.Stop(); }

               myQueue.Enqueue(flags);
               mySemaphoreRequest.Release();
            }
         }

         private bool myDoCalculateTxtSize()
         {
            var txt = null as string;

            if ((myPeekFlags() & FlagsType.text) != 0) { return false; }

            TextControl.MthInvoke(() => txt = TextControl.PpContentText);

            var ln_its = txt?.GetLineIntervals();

            if ((myPeekFlags() & FlagsType.text) != 0) { return false; }

            if (myGraphics != null && ln_its?.Length > 0)
            {
               var max_ln = ln_its.FirstOrDefault();
               var max_l = max_ln.Length;

               foreach (var ln in ln_its.Skip(1))
               {
                  if (ln.Length > max_l)
                  {
                     max_ln = ln;
                     max_l = ln.Length;
                  }
               }

               if ((myPeekFlags() & FlagsType.text) != 0) { return false; }

               var max_cnt = txt?.Substring(max_ln.From, max_ln.Length);

               myTxtSize = myGraphics.MeasureString(max_cnt, TextControl.Font).ToSize();
               myTxtSize = new Size(myTxtSize.Value.Width, myTxtSize.Value.Height * ln_its.Length);
            }
            else { myTxtSize = new Size(0, 0); }

            myLineIntervals = ln_its;

            return true;
         }

         private FlagsType myPeekFlags()
         {
            var que = myQueue.ToArray();
            var flg = FlagsType.none;

            foreach (var itm in que) { flg |= itm; }

            return flg;
         }

         private void myThreadBody()
         {
            try
            {
               while (true)
               {
                  var flg = myTaskWaitFor();

                  if ((flg & FlagsType.text) != 0)
                  {
                     myTxtSize = null;
                     myLineIntervals = null;

                     if (!myDoCalculateTxtSize()) { continue; }
                  }

                  TextControl.MthInvoke(() =>
                  {
                     if ((flg & FlagsType.selection) != 0) { myDoUpdateLabelInfo(); }
                     if ((flg & FlagsType.text) != 0)
                     {
                        TextControl.myDoUpdateLineNumber();
                        TextControl.OnTextChange?.Invoke(TextControl, new EventArgs());
                     }

                     TextControl.CtrlScrollBarV.Maximum = TextControl.PpScintilla.Lines.Count;
                     TextControl.CtrlScrollBarV.LargeChange = TextControl.PpScintilla.LinesOnScreen;
                     TextControl.CtrlScrollBarV.Value = TextControl.PpScintilla.FirstVisibleLine;
                     TextControl.CtrlScrollBarV.Enabled = myTxtSize.HasValue && myTxtSize.Value.Height > TextControl.Height;

                     TextControl.CtrlScrollBarH.Maximum = myTxtSize.HasValue ? myTxtSize.Value.Width - TextControl.Width : 1;
                     TextControl.CtrlScrollBarH.Value = TextControl.PpScintilla.XOffset;
                     TextControl.CtrlScrollBarH.LargeChange = TextControl.Width;
                     TextControl.CtrlScrollBarH.Enabled = myTxtSize.HasValue && myTxtSize.Value.Width > TextControl.Width;
                  });

                  if ((flg & FlagsType.fold_zone) != 0) { myUpdateFoldZoneTask.Restart(); }

                  if ((flg & FlagsType.selection) != 0) { myHighlighTask.Restart(); }

                  if ((flg & FlagsType.req_params) != 0)
                  {
                     myQueueReqParams.Enqueue((myLineIntervals, myTxtSize));
                     mySemaphoreReqParams.Release();
                  }
               }
            }
            catch (ThreadInterruptedException) { }
            catch (Exception exc) { throw new Crash(exc); }
         }

         private FlagsType myTaskWaitFor()
         {
            if (Thread.CurrentThread != myInternalThread) { throw new Crash(); }
            else
            {
               mySemaphoreRequest.WaitOne();

               var flg = FlagsType.none;

               if (myQueue.TryDequeue(out var f2))
               {
                  flg |= f2;

                  while (myQueue.TryDequeue(out var f3))
                  {
                     if (!mySemaphoreRequest.WaitOne(1000)) { throw new Crash(); }
                     else { flg |= f3; }
                  }

                  return flg;
               }
               else { throw new Crash(); }
            }
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="lineIntervals"></param>
         /// <param name="txtSize"></param>
         private void myRequireParams(out Interval[] lineIntervals, out Size? txtSize)
         {
            if (Thread.CurrentThread == myInternalThread) { throw new Crash(); }
            else
            {
               myQueue.Enqueue(FlagsType.req_params);
               mySemaphoreRequest.Release();
               mySemaphoreReqParams.WaitOne();
               myQueueReqParams.TryDequeue(out var tup);
               lineIntervals = tup.Item1 ?? throw new Crash();
               txtSize = tup.Item2;
            }
         }

         private void myDoUpdateLabelInfo() => TextControl.CtrlLabelInfo.Text = $"{TextControl.PpEncoding.EncodingName} Ln:{TextControl.PpCurrLine} Cl:{TextControl.PpCurrCol}";

         public GateTextControl TextControl { get; }

         /// <summary>
         /// Shall be message queue thread of <see cref="Gate.ToolsView.Extensions.ControlExtensions.MthInvoke(Control, Func{object})"/>
         /// </summary>
         public Thread? MsgQueueThread { get; private set; }

         public Interval[] LineIntervals
         {
            get
            {
               if (Thread.CurrentThread != MsgQueueThread && MsgQueueThread != null)
               {
                  myRequireParams(out var ln_its, out Size? _);

                  return ln_its;
               }
               else { return myLineIntervals ?? TextControl.PpContentText.GetLineIntervals(); ; }
            }
         }

         public Size? TxtSize
         {
            get
            {
               if (Thread.CurrentThread != MsgQueueThread && MsgQueueThread != null)
               {
                  myRequireParams(out var _, out Size? siz);

                  return siz;
               }
               else { return myTxtSize; }
            }
         }

         public void Dispose()
         {
            myInternalThread.Interrupt();
            myHighlighTask.Dispose();
            myUpdateFoldZoneTask.Dispose();
            myGraphics?.Dispose();
            myGraphics = null;
         }

         public void OnScintillaUpdateUi(UpdateChange change)
         {
            var flg = FlagsType.none;

            if ((change & UpdateChange.Content) != 0) { flg |= FlagsType.content; }

            if ((change & UpdateChange.Selection) != 0) { flg |= FlagsType.selection; }

            if ((change & UpdateChange.VScroll) != 0)
            {
               //Vscroll is managed by scintilla
               TextControl.CtrlScrollBarV.MthUpdateValueWithoutFireEvents(TextControl.PpFirstLineVisible);
            }

            if (flg != FlagsType.none) { Enqueue(flg); }
         }
      }

      private class InnerFindStrategy : GateTextControlFindInfrastructure
      {
         private readonly TextSearchParamRecord myGateTextFindParamRepo = new TextSearchParamRecord();

         public InnerFindStrategy(GateTextControl gateTextControl) => GateTextControl = gateTextControl;

         private class TxtAppInteract : ITextSearchAppInteraction
         {
            public TxtAppInteract(GateTextControl gateTextControl) => GateTextControl = gateTextControl;

            public Control[] AllOpenTextControls => [GateTextControl];

            public Control? SelectedTextControl { get => GateTextControl; set { } }

            public GateTextControl GateTextControl { get; }

            public Form? GetParentForm() => GateTextControl.ParentForm;

            public void OnAddFindTokens(TextSearchToken[] findTokens) { }

            public void OnEndFindInFiles() { }

            public void OnStartFindInFiles(string text, TextSearchFlags searchFlags, TextSearchMode searchMode, string[] directories, string pattern) { }

            public Control? OpenFile(string filePath) => throw new NotImplementedException();
         }

         public override TextSearchParamRecord SearchParamRecord => myGateTextFindParamRepo;

         public GateTextControl GateTextControl { get; }

         protected override ITextSearchAppInteraction myMakeTxtAppInteraction() => new TxtAppInteract(GateTextControl);
      }

      private class InnerLayoutEngine : LayoutEngine
      {
         public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
         {
            var ctr = (GateTextControl)container;

            var w = ctr.CtrlScrollBarV.Width;
            var h = ctr.PpIsScrollBarHVisible ? ctr.CtrlScrollBarH.Height : 0;
            var lab_w = ctr.CtrlLabelInfo.Visible ? ctr.CtrlLabelInfo.PreferredWidth + 5 : 0;

            ctr.CtrlScrollBarV.Height = ctr.Height - h;
            ctr.CtrlScrollBarV.Left = ctr.Width - w;
            ctr.CtrlScrollBarV.Top = 0;
            ctr.CtrlScrollBarV.BringToFront();

            ctr.CtrlScrollBarH.Left = 0;
            ctr.CtrlScrollBarH.Top = ctr.Height - h;
            ctr.CtrlScrollBarH.Width = ctr.Width - lab_w;

            ctr.CtrlLabelInfo.Left = ctr.CtrlScrollBarH.Width;
            ctr.CtrlLabelInfo.Top = ctr.CtrlScrollBarH.Top;
            ctr.CtrlLabelInfo.Width = lab_w;
            ctr.CtrlLabelInfo.Height = h;

            ctr.PpScintilla.Left = 0;
            ctr.PpScintilla.Top = 0;
            ctr.PpScintilla.Width = ctr.Width - w;
            ctr.PpScintilla.Height = ctr.Height - h;

            return false;
         }
      }

      private static class InnerCommands
      {
         public class Copy : CmdManagedByControlImpl
         {
            public Copy(IControlWithManagedCmds parent) : base(parent, CmdCommonlyUsedIds.COPY) { }

            public new GateTextControl Parent => (GateTextControl)base.Parent;

            public override bool IsEnabled => true;

            public override bool IsVisible => true;

            public override void ActionImpl() => Parent.MthCopy();
         }

         public class Cut : CmdManagedByControlImpl
         {
            public Cut(IControlWithManagedCmds parent) : base(parent, CmdCommonlyUsedIds.CUT) { }

            public new GateTextControl Parent => (GateTextControl)base.Parent;

            public override bool IsEnabled => !Parent.PpIsReadOnly;

            public override bool IsVisible => true;

            public override void ActionImpl() => Parent.MthCut();
         }

         public class Paste : CmdManagedByControlImpl
         {
            public Paste(IControlWithManagedCmds parent) : base(parent, CmdCommonlyUsedIds.PASTE) { }

            public new GateTextControl Parent => (GateTextControl)base.Parent;

            public override bool IsEnabled => !Parent.PpIsReadOnly;

            public override bool IsVisible => true;

            public override void ActionImpl() => Parent.MthPaste();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override LayoutEngine LayoutEngine => new InnerLayoutEngine();

      /// <summary>
      /// 
      /// </summary>
      public FoldZoneGetter? PpFoldZoneGetter
      {
         get => myFoldZoneGetter;
         set
         {
            myFoldZoneGetter = value;
            myUpdateThread.Enqueue(InnerUpdateThread.FlagsType.fold_zone);
         }
      }

      public virtual ScintillaExtension PpScintilla { get; private set; } = new ScintillaExtension();

      /// <summary>
      /// 
      /// </summary>
      public int PpTabSpaces { get => PpScintilla.TabWidth; set => PpScintilla.TabWidth = value; }

      /// <summary>
      /// 
      /// </summary>
      public bool PpIsUseTab { get => PpScintilla.UseTabs; set => PpScintilla.UseTabs = value; }

      [DefaultValue(typeof(Font), "Consolas; 10pt")]
      public override Font Font
      {
         get => base.Font;
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
         set
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
         {
            if (value != null)
            {
               base.Font = value;
               PpLexer = PpLexer;//refresh style
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public bool PpIsLabelInfoVisible
      {
         get => CtrlLabelInfo.Visible;
         set
         {
            CtrlLabelInfo.Visible = value;
            PerformLayout();
         }
      }

      public bool PpIsReadOnly
      {
         get
         {
            var res = false;

            PpScintilla.MthInvoke(() => res = PpScintilla.ReadOnly);

            return res;
         }

         set
         {
            PpScintilla.ReadOnly = myIsReadOnly = value;
            myOnCmdStateUpdate?.Invoke(this);
         }
      }

      public bool PpAreLineNumberActive
      {
         get => myIsLineNumberActive;
         set
         {
            myIsLineNumberActive = value;
            PpLexer = PpLexer;
            this.MthInvoke(() => myDoUpdateLineNumber());
         }
      }

      public Color PpMarginColor
      {
         get => myMarginColor;

         set
         {
            myMarginColor = value;
            PpLexer = PpLexer;//forces update
         }
      }

      [Browsable(false)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public Encoding PpEncoding
      {
         get => myEncoding ?? DefaultEncoding;
         set => myEncoding = value;
      }

      [Browsable(false)]
      public GateTextLexer? PpLexer
      {
         get => myLexer;
         set
         {
            myLexer = value ?? PpLexerSet.Default;
            myLexer.Configure(this);
         }
      }

      [Browsable(false)]
      public GateTextLexerSet PpLexerSet { get; set; } = new GateTextLexerSet();

      [Browsable(false)]
      public string PpOpenPath
      {
         get => myOpenPath;
         set
         {
            myOpenPath = value.ExtTrim();
            OnOpenPathChange?.Invoke(this, new EventArgs());
         }
      }

      [Browsable(false)]
      public int PpLineCount => PpScintilla.Lines.Count;

      [Browsable(false)]
      public int PpCurrLine
      {
         get => PpScintilla.CurrentLine + 1;

         set
         {
            PpScintilla.GotoPosition(PpScintilla.Lines[value - 1].Position);
            PpScintilla.ScrollCaret();//ensure cursor is visible
         }
      }

      [Browsable(false)]
      public int PpCurrCol
      {
         get => PpScintilla.CurrentLine >= PpScintilla.Lines.Count ?
            1 : PpScintilla.CurrentPosition - PpScintilla.Lines[PpScintilla.CurrentLine].Position + 1;

         set
         {
            PpScintilla.GotoPosition(PpScintilla.Lines[PpScintilla.CurrentLine].Position + value - 1);
            PpScintilla.ScrollCaret();//ensure cursor is visible
         }
      }

      [Browsable(false)]
      public int PpCurrIdx
      {
         get => PpScintilla.CurrentPosition;
         set
         {
            PpScintilla.GotoPosition(value);
            PpScintilla.ScrollCaret();//ensure cursor is visible
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public GateTextAnnotationMode PpAnnotationMode { get => (GateTextAnnotationMode)PpScintilla.AnnotationVisible; set => PpScintilla.AnnotationVisible = (Annotation)value; }

      /// <summary>
      /// 
      /// </summary>
      public bool PpAttachToMainMenu { get; set; } = true;

      /// <summary>
      /// 
      /// </summary>
      [Browsable(false)]
      public GateTextStyle.Collection PpScintillaStyles { get; }

      /// <summary>
      /// 
      /// </summary>
      [Browsable(false)]
      public GateTextLineAnnotation[] PpAnnotations => myListAnotation.ToArray();

      /// <summary>
      /// 
      /// </summary>
      [Browsable(false)]
      public string PpSelectedText => PpScintilla.SelectedText;

      /// <summary>
      /// Selection token or null when no selection.
      /// </summary>
      [Browsable(false)]
      public TxtToken? PpSelection
      {
         get
         {
            var sel_txt = PpScintilla.SelectedText;

            if (sel_txt.Length > 0)
            {
               var cnt_txt = PpContentText;
               var sci_txt = new TxtSettings { Encoding = PpScintilla.Encoding };

               var sto = new TxtStore(cnt_txt, sci_txt);
               var shi_hlp = new ScintillaHelper(sto, PpScintilla);

               return new TxtTokenConst(sto, Interval.FromFromLen(shi_hlp.SelectionStart, sel_txt.Length));
            }
            else
            {
               return null;
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public Color PpScrollBarsColor
      {
         get => myScrollBarsColor;
         set => CtrlScrollBarH.BackColor = (myScrollBarsColor = value).IsEmpty ? (CtrlScrollBarV.BackColor = BackColor) : (CtrlScrollBarV.BackColor = value);
      }

      /// <summary>
      /// Back color of text box.
      /// </summary>
      public override Color BackColor
      {
         get => base.BackColor;

         set
         {
            base.BackColor = value;
            PpScrollBarsColor = PpScrollBarsColor;//forces possible updates
            PpLexer = PpLexer;//refresh style
            PpSelectionBackColor = PpSelectionBackColor;
         }
      }

      /// <summary>
      /// Back color of fold box(either [+] or [-]);
      /// </summary>
      public Color PpFoldBoxBackColor
      {
         get => myFoldBoxBackColor;
         set
         {
            myFoldBoxBackColor = value;
            PpLexer = PpLexer;
         }
      }

      /// <summary>
      /// Color of fold lines.
      /// </summary>
      public Color PpFoldLineColor
      {
         get => myFoldLineColor;
         set
         {
            myFoldLineColor = value;
            PpLexer = PpLexer;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override Color ForeColor
      {
         get => base.ForeColor;

         set
         {
            base.ForeColor = value;
            PpLexer = PpLexer;//refresh style
            PpSelectionForeColor = PpSelectionForeColor;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public Color PpScrollBarBackColor { get => CtrlScrollBarH.BackColor; set => CtrlScrollBarH.BackColor = CtrlScrollBarV.BackColor = value; }

      /// <summary>
      /// 
      /// </summary>
      public Color PpScrollBarArrowColor { get => CtrlScrollBarH.PpArrowColor; set => CtrlScrollBarH.PpArrowColor = CtrlScrollBarV.PpArrowColor = value; }

      /// <summary>
      /// 
      /// </summary>
      public Color PpScrollBarThumbColor { get => CtrlScrollBarH.PpGripColor; set => CtrlScrollBarH.PpGripColor = CtrlScrollBarV.PpGripColor = value; }

      /// <summary>
      /// 
      /// </summary>
      public Color PpScrollBarThumbColorActive { get => CtrlScrollBarH.PpGripActiveColor; set => CtrlScrollBarH.PpGripActiveColor = CtrlScrollBarV.PpGripActiveColor = value; }

      /// <summary>
      /// Scrollbar size
      /// </summary>
      [Category("Appearance")]
      [Description("Border color in disabled state.")]
      [DefaultValue(25)]
      public int PpScrollBarSize { get => CtrlScrollBarH.Height; set => CtrlScrollBarH.Height = CtrlScrollBarV.Width = value; }

      /// <summary>
      /// 
      /// </summary>
      [Browsable(false)]
      public bool PpIsModified
      {
         get => PpScintilla.Modified;
         set
         {
            if (!value) { PpScintilla.SetSavePoint(); }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      [Browsable(false)]
      public GateTextToolWinGoto PpToolWinGoto { get; set; } = new GateTextToolWinGoto();

      /// <summary>
      /// 
      /// </summary>
      [Browsable(false)]
      public GateTextControlFindInfrastructure PpFindInfrastructure { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public string PpContentText
      {
         get => PpScintilla.Text;

         set => PpScintilla.Text = (value ?? "").Replace("\r\n", "\n");
      }

      /// <summary>
      /// 
      /// </summary>
      [Browsable(false)]
      public Interval[] PpLineIntervals => myUpdateThread.LineIntervals;

      /// <summary>
      /// 
      /// </summary>
      public bool PpIsHighlightSelectedWordActive { get; set; } = true;

      /// <summary>
      /// 
      /// </summary>
      [Browsable(false)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public ITxtEncodingRetriever PpEncodingRetriever
      {
         get => myEncodingRetriever ?? DefaultEncodingRetriever;
         set => myEncodingRetriever = value;
      }

      /// <summary>
      /// Enable state of margin for breakpoint/bookmark
      /// </summary>
      public bool PpIsBookmarkMarginEnabled
      {
         get => myIsBookmarkMarginEnabled;
         set
         {
            myIsBookmarkMarginEnabled = value;
            PpScintilla.Margins[BOOKMARK_MARGIN_IDX].Width = (value) ? 20 : 0;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public ScintillaMarkerCollection PpScintillaMarkers { get; }

      /// <summary>
      /// 
      /// </summary>
      public ScintillaMarkerWrapper[] PpAllMarkers
      {
         get
         {
            //check for deleted marker
            var new_lst = myListMarkerWrappers.Where(m => PpScintilla.MarkerLineFromHandle(m.Handle) != -1).ToArray();

            myListMarkerWrappers.Clear();
            myListMarkerWrappers.AddRange(new_lst);

            return myListMarkerWrappers.ToArray();
         }
      }

      /// <summary>
      /// Max duration of a background control thread batch
      /// </summary>
      public int PpBackgroundBatchMaxTimeMs { get; set; } = 1;

      /// <summary>
      /// 
      /// </summary>
      public Color PpSelectionBackColor
      {
         get => mySelectionBackColor;

         set
         {
            var col = mySelectionBackColor = value;

            if (col == Color.Empty) { col = myDoGetAdjustBackColor(BackColor); }

            PpScintilla.SetSelectionBackColor(true, col);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public Color PpSelectionForeColor
      {
         get => mySelectionForeColor;

         set
         {
            var col = mySelectionForeColor = value;

            if (col == Color.Empty) { col = ForeColor; }

            PpScintilla.SetSelectionForeColor(true, col);
         }
      }

      /// <summary>
      /// Text size expressed in control DPI.
      /// </summary>
      public Size? PpTextSize => myUpdateThread.TxtSize;

      public bool PpIsScrollBarHVisible
      {
         get => CtrlScrollBarH.Visible;

         set
         {
            CtrlScrollBarH.Visible = value;
            PerformLayout();
         }
      }

      public void MthReplaceSelected(string replaceText) => PpScintilla.ReplaceSelection(replaceText);

      public void MthSelectToken(TxtToken findToken)
      {
         if (findToken == null) { PpScintilla.ClearSelections(); }
         else
         {
            var txt_pos = MthGetPosition(findToken.From?.Line ?? throw new Crash(), findToken.From.Col);

            try
            {
               var txt_rd = PpContentText.Substring(txt_pos, findToken.Length);

               if (txt_rd == findToken.Content)
               {
                  PpScintilla.SelectionStart = txt_pos;
                  PpScintilla.SelectionEnd = txt_pos + findToken.Length;
                  PpScintilla.ScrollCaret();
               }
            }
            catch { }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public void MthCopy() => PpScintilla.Copy();

      /// <summary>
      /// 
      /// </summary>
      public void MthPaste() => PpScintilla.Paste();

      /// <summary>
      /// 
      /// </summary>
      public void MthCut() => PpScintilla.Cut();

      /// <summary>
      /// Shows GoTo .. box.
      /// </summary>
      public void MthGoToWindow()
      {
         PpToolWinGoto.PpLine = PpCurrLine;

         if (PpToolWinGoto.ShowDialog(ParentForm) == DialogResult.OK) { PpCurrLine = PpToolWinGoto.PpLine; }
      }

      public void MthFindToolWinLaunch() => PpFindInfrastructure.LaunchFindWinTool(TextSearchFindReplaceToolWinModeFlags.find_doc);

      public void MthReplaceToolWinLaunch() => PpFindInfrastructure.LaunchFindWinTool(TextSearchFindReplaceToolWinModeFlags.replace_doc);

      public GateTextLineAnnotation? MthAnnotationAdd(int line)
      {
         if (line >= 1 && line <= PpLineCount)
         {
            if (!myListAnotation.Select(a => a.LineIdx).Contains(line))
            {
               var ann = new GateTextLineAnnotation(this, line);

               myListAnotation.Add(ann);

               return ann;
            }
         }

         return null;
      }

      public void MthAnnotationOff(GateTextLineAnnotation annotation)
      {
         annotation.SwitchOff();
         myListAnotation.Remove(annotation);
      }

      public void MthOpenFile(string path)
      {
         var ext = Path.GetExtension(PpOpenPath = path).ToLower();
         var lex = PpLexerSet.GetLexer(ext);

         PpIsReadOnly = false;
         PpLexer = lex;
         PpScintilla.OpenFile(path, PpEncoding = PpEncodingRetriever.FromPath(path));
         PpScintilla.SetSavePoint();
         PpScintilla.EmptyUndoBuffer();
      }

      public void MthSaveFile(string path)
      {
         try
         {
            MthSaveFileCopy(path);
            PpOpenPath = path;
            PpScintilla.SetSavePoint();
         }
         catch (System.IO.IOException) { MessageBox.Show("Can't save " + path); }
         catch (Exception exc) { throw new Crash(exc); }
      }

      /// <summary>
      /// Saves a copy of text content neither changing open path nor marking save point (may raise exceptions).
      /// </summary>
      /// <param name="path">Path where save content.</param>
      /// <exception cref="System.IO.IOException"></exception>
      public void MthSaveFileCopy(string path) => File.WriteAllText(path, PpScintilla.Text, PpEncoding);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="id"></param>
      /// <param name="line"></param>
      /// <param name="col"></param>
      /// <param name="length"></param>
      /// <param name="backColor">Fill color of Box.</param>
      /// <param name="outlineAlpha">Box Border alpha (0 invisible, 255 = full) value is auto clamped in [0,255]</param>
      /// <param name="alpha">Box color alpha (0 invisible, 255 = full) value is auto clamped in [0,255]</param>
      /// <returns></returns>
      public IndicatorInfo? MthIndicatorOn(GateTextIndicatorScintillaIdEnum id, int line, int col, int length, Color backColor, int outlineAlpha = 50, int alpha = 30)
      {
         var sta_idx = MthGetPosition(line, col);

         if (sta_idx == -1) { return null; }
         else
         {
            var end_idx = sta_idx + length - 1;

            return MthIndicatorOn(id, sta_idx, end_idx, backColor, outlineAlpha, alpha);
         }
      }

      public IndicatorInfo.Collection PpScintillaIndicators { get; private set; }

      /// <summary>
      /// Zero based first line visible idx;
      /// </summary>
      public int PpFirstLineVisible
      {
         get => PpScintilla.FirstVisibleLine;

         set => PpScintilla.FirstVisibleLine = value;
      }

      CmdManagedByControlImpl[] IControlWithManagedCmds.CmdsImpl => myCommands;

      CmdManagedByControlImpl? IControlWithManagedCmds.this[string id] => myCommands.FirstOrDefault(c => c.Id == id);

      public IndicatorInfo? MthIndicatorOn(
         GateTextIndicatorScintillaIdEnum id,
         int fromIdx,
         int toIdx,
         Color backColor,
         int outlineAlpha = 50,
         int alpha = 30,
         IndicatorStyle style = IndicatorStyle.StraightBox)
      {
         var int_id = (int)id;

         PpScintilla.Indicators[int_id].Style = style;
         PpScintilla.Indicators[int_id].Under = true;
         PpScintilla.Indicators[int_id].ForeColor = backColor;
         PpScintilla.Indicators[int_id].OutlineAlpha = outlineAlpha;
         PpScintilla.Indicators[int_id].Alpha = alpha;
         PpScintilla.IndicatorCurrent = int_id;
         PpScintilla.IndicatorFillRange(fromIdx, toIdx - fromIdx + 1);

         return new IndicatorInfo(PpScintilla.Indicators[int_id], fromIdx, toIdx, this, id);
      }

      public void MthIndicatorOffAll(GateTextIndicatorScintillaIdEnum id)
      {
         var int_id = (int)id;

         PpScintilla.IndicatorCurrent = int_id;
         PpScintilla.IndicatorClearRange(0, PpScintilla.TextLength);
      }

      public bool MthIndicatorOff(IndicatorInfo indicator)
      {
         if (PpScintillaIndicators[GateTextIndicatorScintillaIdEnum.indicator_9_breakpoints].Contains(indicator))
         {
            PpScintilla.IndicatorCurrent = (int)indicator.Id;
            PpScintilla.IndicatorClearRange(indicator.StartPos, indicator.Len);

            return true;
         }
         else
         {
            return false;
         }
      }

      /// <summary>
      /// Returns line index (1-) from cursor position.
      /// </summary>
      /// <param name="index">Offset with respect file beginning in char number(0-)</param>
      /// <returns></returns>
      public int MthGetLine(int index)
      {
         if (index < 0 || index >= PpContentText.Length) { return -1; }
         else
         {
            var ln_its = PpLineIntervals;

            for (var i = 0; i < ln_its.Length - 1; i++)
            {
               if (index >= ln_its[i].From && index < ln_its[i + 1].From) { return i + 1; }
            }

            var lst_int = ln_its.LastOrDefault();

            return lst_int.Contains(index) ? ln_its.Length : throw new Crash();
         }
      }

      /// <summary>
      /// Returns column index (1-) from cursor position.
      /// </summary>
      /// <param name="index">Offset with respect file beginning in char number(0-len-1)</param>
      /// <returns></returns>
      public int MthGetCol(int index)
      {
         if (index <= 0 || index >= PpContentText.Length) { return -1; }
         else
         {
            var ln_its = PpLineIntervals;

            for (var i = 0; i < ln_its.Length - 1; i++)
            {
               if (index >= ln_its[i].From && index < ln_its[i + 1].From)
               {
                  return index - ln_its[i].From + 1;
               }
            }

            var lst_int = ln_its.Last();

            return lst_int.Contains(index) ? index - lst_int.From + 1 : throw new Crash();
         }
      }

      /// <summary>
      /// Returns cursor offset from file beginning (0-len-1)
      /// </summary>
      /// <param name="line"></param>
      /// <param name="col"></param>
      /// <returns></returns>
      public int MthGetPosition(int line, int col)
      {
         var ln_its = PpLineIntervals;

         if (line < 1 || line > ln_its.Length) { return -1; }
         else { return ln_its[line - 1].From + col - 1; }
      }

      /// <summary>
      /// 
      /// </summary>
      public void MthUndo() => PpScintilla.Undo();

      /// <summary>
      /// 
      /// </summary>
      public void MthRedo() => PpScintilla.Redo();

      /// <summary>
      /// Selects a token, highlighting of all selection is suspended, until you re-enter text control.
      /// </summary>
      /// <param name="token"></param>
      public void MthSelectTokenFromOtherControl(TxtToken token)
      {
         var pri_tok = token.From?.Primitive ?? throw new Gate.Tools.ToolsException($"Not found primitive for {token}!");
         var txt_pos = MthGetPosition(pri_tok.Line, pri_tok.Col);

         try
         {
            var txt_rd = PpContentText.Substring(txt_pos, token.Length);

            if (txt_rd == token.Content)
            {
               PpScintilla.ClearSelections();
               PpScintilla.CurrentPosition = txt_pos;
               PpScintilla.ScrollCaret();
               PpScintilla.SelectionStart = txt_pos;
               PpScintilla.SelectionEnd = txt_pos + token.Length;
            }
         }
         catch { }
      }

      /// <summary>
      /// Adds a scintilla marker with a certain style.
      /// </summary>
      /// <param name="line">Line idx (1-)</param>
      /// <param name="avalaibleMarkerStyle">Style choosen from 'PpAvalaibleScintilaMarkerStyles'</param>
      public ScintillaMarkerWrapper MthScintilaMarkerAdd(int line, GateTextMarkerScintillaIdEnum scintillaId)
      {
         var oth = myListMarkerWrappers.FirstOrDefault(w => w.Line == line && w.ScintillaId == scintillaId);

         if (oth != null)
         {
            return oth;
         }
         else
         {
            var mrk = new ScintillaMarkerWrapper(this, PpScintilla.Lines[line - 1].MarkerAdd((int)scintillaId), scintillaId);

            myListMarkerWrappers.Add(mrk);

            return mrk;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="scintillaMarkerWrapper"></param>
      /// <returns></returns>
      public bool MthScintillaMarkerDelete(ScintillaMarkerWrapper scintillaMarkerWrapper)
      {
         if (PpAllMarkers.Contains(scintillaMarkerWrapper))
         {
            PpScintilla.MarkerDeleteHandle(scintillaMarkerWrapper.Handle);
            myListMarkerWrappers.Remove(scintillaMarkerWrapper);

            return true;
         }
         else { return false; }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="line"></param>
      public void MthOutlineToggle(int line = -1)
      {
         if (PpFoldZoneGetter != null) { PpScintilla.Lines[line == -1 ? PpCurrLine : line].ToggleFold(); }
      }

      /// <summary>
      /// 
      /// </summary>
      public void MthOutlineToggleAll()
      {
         if (PpFoldZoneGetter != null)
         {
            var roo_zon = PpFoldZoneGetter.GetRootFoldZone(PpContentText);
            var ln_ids = roo_zon.AllZones.
               Where(z => z.Type != FoldZoneTypeEnum.text_root).
               Select(z => z.FromPos?.Line ?? throw new Crash()).
               Distinct().ToArray();

            foreach (var ln_idx in ln_ids.Reverse())
            {
               PpScintilla.Lines[ln_idx - 1].FoldLine(myToggleFoldDone ? FoldAction.Expand : FoldAction.Contract);
            }

            myToggleFoldDone = !myToggleFoldDone;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public void MthOutlineCollapseToFunction()
      {
         if (PpFoldZoneGetter != null)
         {
            var roo_zon = PpFoldZoneGetter.GetRootFoldZone(PpContentText);
            var ln_ids = roo_zon.AllZones.
               Where(z => z.Type == FoldZoneTypeEnum.function).
               Select(z => z.FromPos?.Line ?? throw new Crash()).
               Distinct().
               ToArray();

            foreach (var ln_idx in ln_ids.Reverse()) { PpScintilla.Lines[ln_idx - 1].FoldLine(FoldAction.Contract); }
         }
      }

      protected override void OnResize(EventArgs e)
      {
         myUpdateThread?.Enqueue(InnerUpdateThread.FlagsType.resize);
         base.OnResize(e);
      }

      protected override void OnHandleCreated(EventArgs e)
      {
         base.OnHandleCreated(e);

         myUpdateThread.Start(Thread.CurrentThread);
      }

      protected override void OnHandleDestroyed(EventArgs e)
      {
         base.OnHandleDestroyed(e);

         myUpdateThread.Dispose();
      }

      private Color myDoGetAdjustBackColor(Color col)
      {
         var b = col.GetBrightness();
         var f = 2.5;

         return b == 0 ?
            Color.FromArgb(10, 10, 10) :
            b <= 0.5 ?
               Color.FromArgb((int)(col.R * f), (int)(col.G * f), (int)(col.B * f)) :
               Color.FromArgb((int)(col.R / f), (int)(col.G / f), (int)(col.B / f));
      }

      private void myDoUpdateLineNumber()
      {
         if (PpAreLineNumberActive)
         {
            var pad = 2;

            // Did the number of characters in the line number display change?
            // i.e. nnn VS nn, or nnnn VS nn, etc...
            var max_ln_chs = PpScintilla.Lines.Count.ToString().Length;

            // Calculate the width required to display the last line number
            // and include some padding for good measure.
            PpScintilla.Margins[LINE_NUMBER_MARGIN_IDX].Width = PpScintilla.TextWidth(Style.LineNumber, new string('9', max_ln_chs + 1)) + pad;

            PerformLayout();
         }
         else { PpScintilla.Margins[LINE_NUMBER_MARGIN_IDX].Width = 0; }
      }

      private void PpScintilla_TextChanged(object? sender, EventArgs e) => 
         myUpdateThread.Enqueue(
            InnerUpdateThread.FlagsType.text | InnerUpdateThread.FlagsType.fold_zone | InnerUpdateThread.FlagsType.selection);

      private void CtrlScrollBarV_Scroll(object? sender, ScrollEventArgs e) => PpFirstLineVisible = CtrlScrollBarV.Value;

      private void CtrlScrollBarH_Scroll(object? sender, ScrollEventArgs e) => PpScintilla.XOffset = CtrlScrollBarH.Value;

      private void CtrlLabelPosition_TextChanged(object? sender, EventArgs e) => PerformLayout();

      private void Scintilla_UpdateUI(object? sender, UpdateUIEventArgs e) => myUpdateThread?.OnScintillaUpdateUi(e.Change);

      private void PpScintilla_MarginClick(object? sender, MarginClickEventArgs e) => MthOutlineToggle(MthGetLine(e.Position) - 1);
   }
}



