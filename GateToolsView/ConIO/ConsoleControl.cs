using Gate.Tools;
using Gate.Tools.Text;
using Gate.ToolsView.ControlFeature;
using Gate.ToolsView.ControlFeature.Extensions;
using Gate.ToolsView.Extensions;
using Gate.ToolsView.MenuCommand;
using Gate.ToolsView.TextCtrl;
using ScintillaNET;
using ScintillaNET.Gate;

namespace Gate.ToolsView.ConIO
{
   /// <summary>
   /// Console control to be used in conjunction with <see cref="ConsoleController"/>.
   /// </summary>
   public partial class ConsoleControl : UserControl, IConsoleControl
   {
      private const int TIME_FOR_SELECTION_MILLI = 1000;

      private ConsoleController? myConsoleController;
      private OnCmdStateUpdateHandler? myOnCmdStateUpdate;
      private CmdManagedByControlImpl[] myCommands;
      private readonly ScintillaHelper myScintillaHelper;
      private readonly ConsoleInputKeyEventStroke myConsoleInputKeyEventStroke;
      private readonly TxtStore myTxtStore = new TxtStore();
      private TxtPos myCurrentPos = new TxtPos(1, 1);
      private int? myTime;

      event OnCmdStateUpdateHandler IControlWithManagedCmds.OnCmdStateUpdate
      {
         add => myOnCmdStateUpdate += value;

         remove => myOnCmdStateUpdate -= value;
      }

      private ScintillaExtension myScintilla;

      public ConsoleControl()
      {
         myScintilla = new ScintillaExtension();
         myScintilla.Dock = DockStyle.Fill;
         myScintilla.BorderStyle = BorderStyle.None;

         InitializeComponent();

         myCommands = [new InnerCmd.Cut(this), new InnerCmd.Copy(this), new InnerCmd.Paste(this)];
         myScintillaHelper = new ScintillaHelper(myTxtStore, myScintilla);

         var fea = this.AddFeature<CtrlFeatureDispatchFocusToDescendants>();

         CtrlTableLayout.Controls.Add(myScintilla, 0, 0);

         fea.DispatchToControl = myScintilla;
         myScintilla.WrapMode = WrapMode.Word;
         myScintilla.KeyDown += MyScintilla_KeyDown;
         myScintilla.KeyPress += (s, e) => e.Handled = true;
         myScintilla.VScrollBar = myScintilla.HScrollBar = false;
         myConsoleInputKeyEventStroke = new ConsoleInputKeyEventStroke(myScintilla);
         myTxtStore.Settings.NewLine = "\n";
         myScintilla.UpdateUI += Scintilla_UpdateUI;

         for (int i = 0; i < myScintilla.Margins.Count; i++) { myScintilla.Margins[i].Width = 0; }
      }

      /// <summary>
      /// If true direct commands action are used (cut, copy, paste), otherwise the commands are synthetized as key events.
      /// </summary>
      public bool PpIsUseDirectCommandsAction { get; set; } = false;

      private abstract class InnerCmd : CmdManagedByControlImpl
      {
         public InnerCmd(ConsoleControl parent, string id) : base(parent, id) { }

         public new ConsoleControl? Parent => base.Parent as ConsoleControl;

         public override bool IsEnabled => Parent?.myConsoleController != null && Parent.myConsoleController.IsInputEnabled;

         public override bool IsVisible => true;

         public class Cut : InnerCmd
         {
            public Cut(ConsoleControl parent) : base(parent, CmdCommonlyUsedIds.CUT) { }

            public override void ActionImpl() => Parent?.MthCut();
         }
         public class Copy : InnerCmd
         {
            public Copy(ConsoleControl parent) : base(parent, CmdCommonlyUsedIds.COPY) { }

            public override void ActionImpl() => Parent?.MthCopy();
         }

         public class Paste : InnerCmd
         {
            public Paste(ConsoleControl parent) : base(parent, CmdCommonlyUsedIds.PASTE) { }

            public override void ActionImpl() => Parent?.MthPaste();
         }
      }

      public string[]? PpScreenBuffer
      {
         get
         {
            var res = null as string[];

            myScintilla.MthInvoke(() =>
            {
               res = myTxtStore.Lines.Select(l => l.Content).ToArray();
            });

            return res;
         }
      }

      public TxtPos PpCurrentPos
      {
         get => myCurrentPos;

         set
         {
            if (value != null)
            {
               var sci = myScintilla;

               sci.MthInvoke(() =>
               {
                  //check if position is different
                  if (value.CompareTo(myCurrentPos) != 0)
                  {
                     // Simply calculate how many additional lines we need
                     var nnl = value.Line - myTxtStore.LineCount;

                     if (nnl > 0)
                     {
                        //adding nnl empty lines to scintilla
                        myScintillaHelper.InsertText(
                           myTxtStore.Content.Length, Enumerable.Range(0, nnl).Select(_ => "\n").Aggregate((s1, s2) => s1 + s2));

                        //adding nnl empty lines to store
                        myTxtStore.AddLines(Enumerable.Range(0, nnl).Select(_ => "").ToArray());
                     }

                     //effectively set position
                     myDoSetPos(value);
                  }
               });

            }
         }
      }

      public Point PpScreenPos
      {
         get
         {
            var pp = new Point(0, 0);

            myScintilla.MthInvoke(() =>
            {
               var pos = PpIdx;
               var line = myScintillaHelper.LineFromCharPosition(pos);
               var linePos = myScintillaHelper.CharPositionFromLine(line);
               var column = pos - linePos;

               // Get character position in pixels
               var x = myScintillaHelper.PointXFromPosition(pos);
               var y = myScintillaHelper.PointYFromPosition(pos);

               // Add character width to get right edge
               x += myScintilla.TextWidth(Style.Default, "M"); // Use "M" as representative character width

               pp = new Point(x, y);
            });


            return pp;
         }
      }

      public int PpIdx
      {
         get
         {
            if (myCurrentPos.Line == 1 && myCurrentPos.Col == 1 || myTxtStore.Content.Length == 0) { return 0; }
            else if (
               myCurrentPos.Line > myTxtStore.LineCount ||
               myCurrentPos.Line == myTxtStore.LineCount && myCurrentPos.Col > myTxtStore.Lines.LastLine.Content.Length)
            {
               return myTxtStore.Content.Length;
            }
            else { return myTxtStore.GetIdx(myCurrentPos); }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public (int start, int end) PpSelectionIndices
      {
         get
         {
            (int, int) res = (0, 0);

            this.MthInvoke(() => { res = (myScintillaHelper.SelectionStart, myScintillaHelper.SelectionEnd); });

            return res;
         }
         set
         {
            this.MthInvoke(() =>
            {
               myScintillaHelper.SelectionStart = value.start;
               myScintillaHelper.SelectionEnd = value.end;
            });
         }
      }

      public (TxtPos? start, TxtPos? end) PpSelectionPositions
      {
         get
         {
            var ids = PpSelectionIndices;

            if (ids.end > ids.start)
            {
               return (myGetTxtPos(ids.start), myGetTxtPos(ids.end));
            }
            else
            {
               return (null, null);
            }
         }

         set
         {
            if (value.start != null && value.end != null)
            {
               PpSelectionIndices = (myGetIdx(value.start), myGetIdx(value.end));
            }
            else
            {
               PpSelectionIndices = (PpIdx, PpIdx);
            }
         }
      }

      public void MthCancelChar(bool isBackward)
      {
         myScintilla.MthInvoke(() =>
         {
            var cur_idx = PpIdx;
            var idx = 0;

            if (isBackward)
            {
               if (cur_idx > 0) { idx = cur_idx - 1; }
               else { return; }
            }
            else
            {
               if (cur_idx < myTxtStore.Content.Length) { idx = cur_idx; }
               else { return; }
            }

            myScintillaHelper.DeleteRange(idx, 1);
            myTxtStore.RemoveIntervals(Interval.FromFromLen(idx, 1));
            myDoSetPos(myGetTxtPos(idx));
         });
      }

      public void MthClearScreen()
      {
         myScintilla.MthInvoke(() =>
         {
            myScintilla.Text = "";
            myTxtStore.Content = "";
            myDoSetPos(new TxtPos(1, 1));
         });
      }

      public void MthCopy() => myScintilla.MthInvoke(() => myScintilla.Copy());

      public void MthPaste() => myConsoleInputKeyEventStroke.PasteString(Clipboard.GetText());

      public void MthCut()
      {
         var sel_sta = myScintillaHelper.SelectionStart;
         var sel_end = myScintillaHelper.SelectionEnd;

         if (sel_end > sel_sta)
         {
            var sel_sta_pos = myGetTxtPos(sel_sta);
            var sel_end_pos = myGetTxtPos(sel_end);

            if (sel_sta_pos?.Line == myCurrentPos.Line && sel_end_pos?.Line == myCurrentPos.Line)
            {
               myTxtStore.Replace(new TxtStoreReplacement(new Interval(sel_sta, sel_end - 1)));
               myScintilla.Cut();
            }
         }
      }

      public void MthInsert2CurrentPos(string @string)
      {
         myScintilla.MthInvoke(() =>
         {
            var idx = PpIdx;
            var len = myTxtStore.Content.Length;
            var new_idx = idx + @string.Length;

            myScintillaHelper.InsertText(idx, @string);
            myTxtStore.InsertText(idx, @string);
            myDoSetPos(myGetTxtPos(new_idx));
         });
      }

      public string MthGetLine(int line1 = 0)
      {
         var res = "";

         myScintilla.MthInvoke(() =>
         {
            if (line1 == 0)
            {
               line1 = myCurrentPos.Line;
            }

            if (line1 > myTxtStore.LineCount) { res = ""; }
            else { res = myTxtStore.Lines[line1 - 1].Content; }
         });

         return res;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="lineText"></param>
      /// <param name="line1"></param>
      /// <returns></returns>
      public string MthSetLine(string lineText, int line1 = 0)
      {
         var old_ln = "";

         myScintilla.MthInvoke(() =>
         {
            if (line1 == 0) { line1 = myCurrentPos.Line; }

            var ln = null as TxtStore.LineToken;

            if (line1 > myTxtStore.LineCount)
            {
               myTxtStore.AddLines(Enumerable.Range(0, line1-myTxtStore.LineCount).Select(_=>"").ToArray());   
               ln = myTxtStore.Lines[line1 - 1];
            }
            else
            {
               ln = myTxtStore.Lines[line1 - 1];

               var sta = ln.Interval.From;
               var end = ln.Interval.To;

               myTxtStore.ReplaceLine(line1, lineText);
            }

            myScintilla.Text = myTxtStore.Content;

            if (myTxtStore.Content != "")
            {
               ln = myTxtStore.Lines[line1 - 1];
               myDoSetPos(new TxtPos(ln.LineIdx, ln.Content.Length + 1));
            }
            else
            {
               myDoSetPos(new TxtPos(1, 1));
            }
         });

         return old_ln;
      }

      public void MthReplaceSelection(string newText)
      {
         this.MthInvoke(() =>
         {
            var si = PpSelectionIndices;

            if (si.end > si.start)
            {
               myScintillaHelper.ReplaceSelection(newText);
               myTxtStore.Replace(new TxtStoreReplacement(new Interval(si.start, si.end - 1), new TxtTokenConst(newText)));
               myScintilla.ScrollCaret();
            }
            else if (si.end == si.start)
            {
               MthInsert2CurrentPos(newText);
            }
         });
      }

      /// <summary>
      /// Updates the current text position if it differs from the specified position and adjusts the view to ensure the
      /// caret is visible.
      /// </summary>
      /// <remarks>If the specified position differs from the current position, this method updates the
      /// current position, moves the caret to the new position, and scrolls the view to ensure the caret is
      /// visible.</remarks>
      /// <param name="newSetPos">The new text position to set. Must not be equal to the current position to trigger an update.</param>
      private void myDoSetPos(TxtPos? newSetPos)
      {
         if (newSetPos != null)
         {
            if (newSetPos.CompareTo(myCurrentPos) != 0)
            {
               myCurrentPos = newSetPos;
               myScintillaHelper.GotoPosition(PpIdx);
               myScintilla.ScrollCaret();
            }

            CtrlScrollBar.Value = newSetPos.Line;
            CtrlScrollBar.Minimum = 1;
            CtrlScrollBar.Maximum = myTxtStore.LineCount;
            CtrlScrollBar.Enabled = myTxtStore.LineCount > myScintilla.LinesOnScreen;
         }
      }

      protected override void OnLoad(EventArgs e)
      {
         base.OnLoad(e);
         myScintilla.Focus();
      }

      protected override void OnHandleDestroyed(EventArgs e)
      {
         base.OnHandleDestroyed(e);
         myConsoleInputKeyEventStroke.Dispose();
      }

      protected override void OnFontChanged(EventArgs e)
      {
         myScintilla.Styles[Style.Default].Font = Font.Name;
         myScintilla.Styles[Style.Default].Size = (int)Font.Size;
         myScintilla.StyleClearAll(); // Applica il nuovo font a tutto il testo         base.OnFontChanged(e);
      }

      protected override void OnBackColorChanged(EventArgs e)
      {
         base.OnBackColorChanged(e);

         myScintilla.CaretForeColor = ForeColor;
         myScintilla.Styles[Style.Default].BackColor = BackColor;
         myScintilla.Styles[Style.Default].ForeColor = ForeColor;
         myScintilla.StyleClearAll();
      }

      TxtPos IConsoleControl.CurrentPos { get => PpCurrentPos; set => PpCurrentPos = value; }

      ConsoleInputKeyEventStroke IConsoleControl.ConsoleInputKeyEventStroke => myConsoleInputKeyEventStroke;

      CmdManagedByControlImpl[] IControlWithManagedCmds.CmdsImpl => myCommands;

      ConsoleController? IConsoleControl.ConsoleController
      {
         get => myConsoleController;

         set
         {
            if (myConsoleController != value)
            {
               if (myConsoleController != null)
               {
                  myConsoleController.OnTaskTakeControl -= Value_OnConsoleControllerStackChange;
               }

               if (value != null)
               {
                  value.OnTaskTakeControl += Value_OnConsoleControllerStackChange;
               }

               myConsoleController = value;
            }
         }
      }

      private void myActionOnCmdUpdateState(ConsoleIo consoleIo, string inputLine) => myOnCmdStateUpdate?.Invoke(this);

      private void Value_OnConsoleControllerStackChange(ConsoleTask consoleTask)
      {
         //event are added and removed for avoiding double invoke
         foreach (var tsk in consoleTask.ConsoleController?.ConsoleTasks ?? [])
         {
            tsk.Conio.OnInputLineEnded -= myActionOnCmdUpdateState;
         }

         foreach (var tsk in consoleTask.ConsoleController?.ConsoleTasks ?? [])
         {
            tsk.Conio.OnInputLineEnded += myActionOnCmdUpdateState;
         }
      }

      string[]? IConsoleControl.ScreenBuffer { get => PpScreenBuffer; }

      Point IConsoleControl.CurrentScreenPos { get => PpScreenPos; }

      (TxtPos? start, TxtPos? end) IConsoleControl.Selection { get => PpSelectionPositions; set => PpSelectionPositions = value; }

      CmdManagedByControlImpl? IControlWithManagedCmds.this[string id] => myCommands.FirstOrDefault(c => c.Id == id);

      void IConsoleControl.Insert2CurrentPos(string @string) => MthInsert2CurrentPos(@string);

      void IConsoleControl.CancelChar(bool isBackward) => MthCancelChar(isBackward);

      void IConsoleControl.ClearScreen() => MthClearScreen();

      string IConsoleControl.GetLine(int line1) => MthGetLine(line1);

      string IConsoleControl.SetLine(string lineText, int line1) => MthSetLine(lineText, line1);

      void IConsoleControl.ReplaceSelection(string newText) => MthReplaceSelection(newText);

      private TxtPos? myGetTxtPos(int newIdx)
      {
         if (newIdx == 0 || myTxtStore.Content.Length == 0) { return new TxtPos(1, 1); }
         else if (newIdx >= myTxtStore.Content.Length) { return new TxtPos(myTxtStore.LineCount, myTxtStore.Lines.LastLine.Content.Length + 1); }
         else { return myTxtStore.GetPos(newIdx); }
      }

      private int myGetIdx(TxtPos txtPos)
      {
         if (txtPos.Line == 1 && txtPos.Col == 1) { return 0; }
         else if (txtPos.Line == myTxtStore.Lines.Count && txtPos.Col > myTxtStore.Lines.Last().Length)
         {
            return myTxtStore.Content.Length;
         }
         else
         {
            return myTxtStore.GetIdx(txtPos);
         }
      }

      private void CtrlTimer_Tick(object? sender, EventArgs e)
      {
         var sel_sta = myScintillaHelper.SelectionStart;
         var sel_end = myScintillaHelper.SelectionEnd;

         if (myTime.HasValue && Environment.TickCount - myTime > TIME_FOR_SELECTION_MILLI && sel_end - sel_sta <= 0)
         {
            myScintillaHelper.GotoPosition(PpIdx);
            myScintilla.CaretStyle = CaretStyle.Line; // Hide caret after timeout
            myTime = null;
         }
      }

      private void Scintilla_UpdateUI(object? sender, UpdateUIEventArgs e)
      {
         //caret position 
         var cp = myScintillaHelper.CurrentPosition;

         if (cp != PpIdx && !myTime.HasValue)
         {
            myTime = Environment.TickCount;
            myScintilla.CaretStyle = CaretStyle.Invisible; // Show caret initially
         }
      }

      private void CtrlScrollBar_Scroll(object? sender, ScrollEventArgs e) => myScintilla.FirstVisibleLine = e.NewValue;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void MyScintilla_KeyDown(object? sender, KeyEventArgs e)
      {
         if (PpIsUseDirectCommandsAction)
         {
            if (!e.Shift && !e.Alt && e.Control && e.KeyCode == Keys.X)
            {
               var cmd = myCommands.FirstOrDefault(c => c.Id == CmdCommonlyUsedIds.CUT);

               cmd?.ActionImpl();
               e.SuppressKeyPress = true;
            }
            if (!e.Shift && !e.Alt && e.Control && e.KeyCode == Keys.C)
            {
               var cmd = myCommands.FirstOrDefault(c => c.Id == CmdCommonlyUsedIds.COPY);

               cmd?.ActionImpl();
               e.SuppressKeyPress = true;
            }
            if (!e.Shift && !e.Alt && e.Control && e.KeyCode == Keys.V)
            {
               var cmd = myCommands.FirstOrDefault(c => c.Id == CmdCommonlyUsedIds.PASTE);

               cmd?.ActionImpl();
               e.SuppressKeyPress = true;
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="action"></param>
      void IConsoleControl.InQueueInvoke(Action action) => this.MthInvoke(action);
   }
}
