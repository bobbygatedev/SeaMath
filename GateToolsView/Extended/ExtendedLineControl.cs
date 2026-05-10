using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.ToolsView.Extensions;
using Gate.ToolsView.MenuCommand;
using System.Collections;

namespace Gate.ToolsView.Extended
{
   /// <summary>
   /// 
   /// </summary>
   public partial class ExtendedLineControl : UserControl, IControlWithManagedCmds
   {
      private readonly InnerLineContainer myLineContainer;
      private OnCmdStateUpdateHandler? myOnCmdStateUpdate;

      event OnCmdStateUpdateHandler IControlWithManagedCmds.OnCmdStateUpdate
      {
         add => myOnCmdStateUpdate += value;

         remove => myOnCmdStateUpdate -= value;
      }

      private static readonly TextFormatFlags myTextFormatFlags = TextFormatFlags.NoPadding | TextFormatFlags.NoClipping;

      private CmdManagedByControlImpl[] myCommands;
      private Color myCurrentIdBackColor = Color.Violet;
      private (TxtPos from, TxtPos to)? mySelectionInterval;
      private bool myIsLeftMouseDown;
      private Color mySelectionBackColor = Color.Aquamarine;
      private uint myLineSpacing = 3;
      private uint myMarginPixels = 5;

      public ExtendedLineControl()
      {
         InitializeComponent();

         CtrlInnerText.Font = Font;
         myLineContainer = new InnerLineContainer(this);
         CtrlInnerText.Paint += CtrlInnerText_Paint;
         CtrlInnerText.MouseDown += CtrlInnerText_MouseDown;
         CtrlInnerText.MouseUp += CtrlInnerText_MouseUp;
         CtrlInnerText.MouseMove += CtrlInnerText_MouseMove;
         CtrlInnerText.MouseDoubleClick += (s, e) => OnMouseDoubleClick(e);
         CtrlInnerText.KeyDown += CtrlInnerText_KeyDown;
         myCommands = [new InnerCmd.Copy(this)];
      }

      private class InnerLineContainer : HierarchicalItem
      {
         private bool myIsExtLineIdToRecreate = true;

         /// <summary>
         /// First extended line on screen
         /// </summary>
         private int myFirstLineExtId = 1;
         private int myCurrentLineId = -1;

         public InnerLineContainer(ExtendedLineControl parent)
         {
            ParentTextControl = parent.CtrlInnerText;
            Parent = parent;
            Lines = new LineHolder(this);
            OnChildAdded += myActionLineChanged;
            OnChildRemoved += myActionLineChanged;
         }

         public class LineType : HierarchicalItem
         {
            private string myLine = "";

            public LineType() { }

            public bool Is2Repaint { get; set; } = true;

            public void ClearLineExt()
            {
               myRemoveSubItemRange(SubItems.OfType<FoldedLineType>());
               Is2Repaint = true;
               (ParentLineContainer ?? throw new Crash()).myIsExtLineIdToRecreate = true;
            }

            public InnerLineContainer? ParentLineContainer => ParentItem as InnerLineContainer;

            public ExtendedLineControl? Parent => ParentLineContainer?.Parent;

            public UserControl? ParentTextControl => ParentLineContainer?.ParentTextControl;

            public int Idx => ParentLineContainer != null ? ParentLineContainer.Lines.ToList().IndexOf(this) + 1 : 0;

            public string Text
            {
               get => myLine;

               set
               {
                  value = value ?? "";

                  if (value != myLine)
                  {
                     ClearLineExt();
                     myLine = value;
                  }
               }
            }

            public Size TextSize => ParentLineContainer?.GetTextSize(Text) ?? Size.Empty;

            public FoldedLineType[] FoldedLines
            {
               get
               {
                  var ext_lns = SubItems.OfType<FoldedLineType>().ToArray();

                  if (ext_lns.Length == 0 && ParentTextControl != null)
                  {
                     var mw = ParentTextControl.Width - 2 * (Parent?.PpMarginPixels ?? 1);
                     var off = 0;
                     var ext_lns_num = (int)(1.05 * TextSize.Width / mw) + 1;
                     var ext_chs = Math.Max(24, Text.Length / ext_lns_num);

                     while (off < Text.Length)
                     {
                        var ex_ln = new FoldedLineType();

                        myAddSubItem(ex_ln);
                        ex_ln.Text = Text.Substring(off, Math.Min(ext_chs, Text.Length - off));
                        off += ex_ln.Text.Length;
                     }

                     ext_lns = SubItems.OfType<FoldedLineType>().ToArray();

                     //empty line is composed by an emtpy ext line
                     if (ext_lns.Length == 0)
                     {
                        myAddSubItem(new FoldedLineType());
                        ext_lns = SubItems.OfType<FoldedLineType>().ToArray();
                     }
                  }

                  return ext_lns;
               }
            }

            public Color? Color { get; internal set; }

            public void Invalidate()
            {
               foreach (var ex_ln in FoldedLines) { ex_ln.Invalidate(); }
            }

            public override string ToString() => $"Ln{Idx}: '{Text}' [{string.Join(",", FoldedLines.ToList())}]";
         }

         public class FoldedLineType : HierarchicalItem
         {
            private int myIdx = -1;

            public FoldedLineType() { }

            public LineType? ParentLine => ParentItem as LineType;

            public ExtendedLineControl? Parent => (Anchestor as InnerLineContainer)?.Parent;

            public string Text { get; set; } = "";

            /// <summary>
            /// <br> Index 1-offset of folded line inside all <see cref="InnerLineContainer"/> </br> 
            /// <br> eg <see cref="InnerLineContainer"/> has 2 lines:</br> 
            /// <br> Line1 {folded1.1(idx=1),folded 1.2(idx=2) } </br>
            /// <br> Line2 {folded2.1(idx=3),folded 2.2(idx=4) } </br>
            /// </summary>
            public int FoldedGeneralIdx1
            {
               get
               {
                  (ParentLine?.ParentLineContainer ?? throw new ToolsException()).myLineExtIdxRecreate(myIdx == -1);

                  return myIdx;
               }

               set { myIdx = value; }
            }

            /// <summary>
            /// Interval 0-offset of the extended line 
            /// </summary>
            public Interval LineRelativeInterval
            {
               get
               {
                  var prs = ParentLine?.FoldedLines.TakeWhile(el => !ReferenceEquals(el, this)).ToArray();
                  var lft = (prs ?? []).Sum(p => p.Text.Length);

                  return Interval.FromFromLen(lft, Text.Length);
               }
            }

            public Rectangle? Rect
            {
               get
               {
                  if (
                     FoldedGeneralIdx1 >= (LineContainer?.myFirstLineExtId ?? 0) &&
                     FoldedGeneralIdx1 < (LineContainer?.myFirstLineExtId ?? 0) + (LineContainer?.NumLinesDisplayable ?? 0))
                  {
                     var plc = ParentLine?.ParentLineContainer ?? throw new ToolsException();
                     var scr_pos = plc.GetScreenMargin(FoldedGeneralIdx1);

                     return new Rectangle(
                        scr_pos,
                        new Size(
                           (ParentTextControl?.Width ?? 0) - 2 * (int)(Parent?.PpMarginPixels ?? 1),
                           plc.FontHeight));
                  }
                  else { return null; }
               }
            }

            public InnerLineContainer? LineContainer => ParentLine?.ParentLineContainer;

            public UserControl? ParentTextControl => ParentLine?.ParentLineContainer?.ParentTextControl;

            public Color? Color { get; set; }

            public bool IsVisible =>
               ParentLine?.ParentLineContainer != null &&
               Enumerable.Range(
                  ParentLine.ParentLineContainer.FoldedLinesFirstId,
                  ParentLine.ParentLineContainer.NumLinesDisplayable).Contains(FoldedGeneralIdx1);

            public void Invalidate()
            {
               if (Rect.HasValue) { ParentTextControl?.Invalidate(Rect.Value); }

               if (ParentLine != null)
               {
                  ParentLine.Is2Repaint = true;
               }
            }

            public override string ToString() => $"ExtLn#{FoldedGeneralIdx1} '{Text}'";
         }

         public class LineHolder : IEnumerable<LineType>
         {
            private readonly InnerLineContainer myParentLineContainer;

            public LineHolder(InnerLineContainer lineContainer) => myParentLineContainer = lineContainer;

            public LineType this[int index] =>
               Enumerable.Range(1, Length).Contains(index) ?
                  myParentLineContainer.SubItems.OfType<LineType>().ElementAt(index - 1) :
                  throw new Gate.Tools.ToolsException($"Index outside range");

            public int Length => myParentLineContainer.SubItems.OfType<LineType>().Count();

            public IEnumerator<LineType> GetEnumerator() => myParentLineContainer.SubItems.OfType<LineType>().GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => myParentLineContainer.SubItems.OfType<LineType>().GetEnumerator();
         }

         public UserControl ParentTextControl { get; }

         public ExtendedLineControl Parent { get; }

         public LineHolder Lines { get; }

         public FoldedLineType[] FoldedLines => Lines.SelectMany(l => l.FoldedLines).ToArray();

         public int FoldedLinesFirstId
         {
            get => myFirstLineExtId;

            set
            {
               if (myFirstLineExtId != value)
               {
                  myFirstLineExtId = value;
                  RepaintAll();
               }
            }
         }

         public FoldedLineType[] FoldedLinesVisible => FoldedLines.Where(le => le.IsVisible).ToArray();

         public LineType[] LinesVisible => FoldedLinesVisible.Select(le => le.ParentLine).Nn().Distinct().ToArray() ?? [];

         public int FontHeight => GetTextSize("").Height;

         public int NumLinesDisplayable => ParentTextControl.Height / (FontHeight + (int)Parent.PpLineSpacing);

         public int CurrentLineId
         {
            get => myCurrentLineId;

            set
            {
               if (value >= 1 && value <= Parent.PpLineCount && value != myCurrentLineId)
               {
                  var cur_ln = Lines[value];

                  if (!cur_ln.FoldedLines.All(el => Enumerable.Range(FoldedLinesFirstId, NumLinesDisplayable).Contains(el.FoldedGeneralIdx1)))
                  {
                     myUpdateFirstLineExtId(cur_ln);
                  }
                  else
                  {
                     //invalidate old and new value
                     if (myCurrentLineId >= 1 && myCurrentLineId <= Lines.Length)
                     {
                        Lines[myCurrentLineId].Invalidate();
                     }

                     Lines[value].Invalidate();
                  }

                  myCurrentLineId = value;
                  Parent.PpSelectionInterval = null;
               }
            }
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="textPos">Includes folded lines</param>
         /// <returns></returns>
         public Point GetScreenMargin(int extLineId)
         {
            var off_y = (extLineId - FoldedLinesFirstId) * (FontHeight + (int)Parent.PpLineSpacing);
            var off_x = (int)Parent.PpMarginPixels;

            return new Point(off_x, off_y);
         }

         /// <summary>
         /// Returns (Ln,Col) based on text pos
         /// </summary>
         /// <param name="screenPos"></param>
         /// <returns></returns>
         public TxtPos? GetTextPos(Point screenPos)
         {
            var fld_lin_id0 = FoldedLinesFirstId - 1 + (screenPos.Y / (FontHeight + (int)Parent.PpLineSpacing));

            if (fld_lin_id0 >= 0 && fld_lin_id0 < FoldedLines.Length)
            {
               var ex_ln = FoldedLines[fld_lin_id0] ?? throw new Crash();

               //approximated extended column id
               var apr_ex_col_id = myGetTextCol(ex_ln, screenPos.X);

               return new TxtPos(ex_ln.ParentLine?.Idx ?? -1, apr_ex_col_id);
            }
            else { return null; }
         }

         public void PaintLineSet(Graphics graphics)
         {
            var ln_2_upd = FoldedLinesVisible.ToArray();

            foreach (var ln_ext in ln_2_upd ?? [])
            {
               var sel_int = myGetLineSelection(ln_ext.ParentLine);

               using (var sf = new StringFormat())
               {
                  sf.Alignment = StringAlignment.Near;
                  sf.LineAlignment = StringAlignment.Center;

                  var le_r_int = ln_ext.LineRelativeInterval;
                  var ise = sel_int.HasValue ? sel_int.Value.GetIntersection(le_r_int) : null;

                  var lst_chr = new List<CharacterRange>();
                  var sel_idx = 0;

                  if (ise.HasValue)
                  {
                     var fro = 0;

                     if (ise.Value.From > le_r_int.From)
                     {
                        lst_chr.Add(new CharacterRange(fro, ise.Value.From - le_r_int.From));
                        fro += lst_chr.Last().Length;
                        sel_idx++;
                     }

                     lst_chr.Add(new CharacterRange(fro, ise.Value.Length));
                     fro += lst_chr.Last().Length;

                     if (ise.Value.To < le_r_int.To)
                     {
                        lst_chr.Add(new CharacterRange(fro, le_r_int.To - ise.Value.To));
                        fro += lst_chr.Last().Length;
                     }
                  }
                  else
                  {
                     lst_chr.Add(new CharacterRange(0, ln_ext.LineRelativeInterval.Length));
                     sel_idx = -1;
                  }

                  sf.SetMeasurableCharacterRanges(lst_chr.ToArray());

                  var rgs = graphics.MeasureCharacterRanges(ln_ext.Text, ParentTextControl.Font, ln_ext.Rect ?? Rectangle.Empty, sf);

                  for (int reg_idx = 0; reg_idx < rgs.Length; reg_idx++)
                  {
                     var reg = rgs[reg_idx];
                     var bns = Rectangle.Round(reg.GetBounds(graphics));
                     var wrd = ln_ext?.Text.Substring(lst_chr[reg_idx].First, lst_chr[reg_idx].Length);
                     var bck_col = ln_ext?.ParentLine?.Idx == CurrentLineId ? Parent.PpCurrentIdBackColor : Parent.BackColor;

                     if (reg_idx == sel_idx)
                     {
                        bck_col = Parent.PpSelectionBackColor;
                     }

                     TextRenderer.DrawText(
                        graphics,
                        wrd,
                        ParentTextControl.Font,
                        bns,
                        ln_ext?.ParentLine?.Color ?? ParentTextControl.ForeColor,
                        bck_col,
                        myTextFormatFlags);
                  }
               }

               if (ln_ext?.ParentLine != null)
               {
                  ln_ext.ParentLine.Is2Repaint = false;
               }
            }
         }

         public void ClearScreen()
         {
            myRemoveSubItemRange(Lines);
            myCurrentLineId = myFirstLineExtId = 1;
            RepaintAll();
         }

         public void RepaintAll()
         {
            foreach (var ln in Lines)
            {
               ln.Is2Repaint = true;
            }

            ParentTextControl.Invalidate();
         }

         public Size GetTextSize(string text)
         {
            var a2 = TextRenderer.MeasureText(
               "aa", ParentTextControl.Font, new Size(int.MaxValue, int.MaxValue), myTextFormatFlags);
            var a1 = TextRenderer.MeasureText(
               "a", ParentTextControl.Font, new Size(int.MaxValue, int.MaxValue), myTextFormatFlags);

            if (text == "") { return new Size(0, a1.Height); }
            else
            {
               var ext_off = a2.Width - a1.Width;
               var tmp = TextRenderer.MeasureText(
                  text, ParentTextControl.Font, new Size(int.MaxValue, int.MaxValue), myTextFormatFlags);

               return new Size(tmp.Width - ext_off, tmp.Height);
            }
         }

         public void InsertRange(IEnumerable<int> idx1Range)
         {
            if (idx1Range.Count() > 0)
            {
               // eg idx1Range = {2,3,4} line is are inserted starting from index 2 { 1(old) , 2,3,4 , 2(old),.. 
               myInsertSubItemRange(idx1Range.Select(_ => new LineType()), idx1Range.First() - 1);
            }
         }

         public void RemoveLineRange(int[] idx1Range)
         {
            var lns = Lines.Where(l => idx1Range.Contains(l.Idx)).ToArray();

            myRemoveSubItemRange(lns);
            FoldedLinesFirstId = Math.Min(FoldedLinesFirstId, FoldedLines.Length);
            myLineExtIdxRecreate(true);
         }

         public void OnResize()
         {
            foreach (var ln in Lines) { ln.ClearLineExt(); }

            if (Lines.Length > 0)
            {
               if (CurrentLineId >= 1 && CurrentLineId < Parent.PpLineCount)
               {
                  myUpdateFirstLineExtId(Lines[CurrentLineId]);
               }
               else
               {
                  myUpdateFirstLineExtId(Lines.First());
               }
            }

            RepaintAll();
         }

         private Interval? myGetLineSelection(LineType? line)
         {
            if (line != null && Parent.PpSelectionInterval.HasValue)
            {
               var sel_tok = Parent.PpSelectionInterval.Value;

               if (sel_tok.from != null && sel_tok.to != null)
               {
                  var ln_rng = new Interval(sel_tok.from.Line, sel_tok.to.Line);

                  if (ln_rng.Contains(line.Idx))
                  {
                     var fro = 1;
                     var to = line.Text.Length;

                     var is_frs = line.Idx == sel_tok.from.Line;
                     var is_lst = line.Idx == sel_tok.to.Line;

                     //if is first line of selection selection starts from token left
                     if (is_frs)
                     {
                        fro = sel_tok.from.Col;
                     }

                     //if is last line of selection selection ends at token right
                     if (is_lst)
                     {
                        to = sel_tok.to.Col;
                     }

                     //scaled to 0 offset
                     return new Interval(fro - 1, to - 1);
                  }
               }
            }
            return null;
         }

         private void myUpdateFirstLineExtId(LineType currentLine)
         {
            if (NumLinesDisplayable >= currentLine.FoldedLines.Length)
            {
               //current line as last (full visible) 
               FoldedLinesFirstId = Math.Max(1, currentLine.FoldedLines.Last().FoldedGeneralIdx1 - NumLinesDisplayable + 1);
            }
            else
            {
               //if less lines are displayable than extended lines first lines are displayable
               //eg ln has 3 extended lines and 2 lines are displayable then first 2 ext-lines are displayed
               FoldedLinesFirstId = currentLine.FoldedLines.First().FoldedGeneralIdx1;
            }
         }

         private void myLineExtIdxRecreate(bool isForce)
         {
            if (myIsExtLineIdToRecreate || isForce)
            {
               myIsExtLineIdToRecreate = false;

               var id = 1;

               foreach (var ln_ex in FoldedLines) { ln_ex.FoldedGeneralIdx1 = id++; }
            }
         }

         private void myActionLineChanged(HierarchicalItem sender, HierarchicalItem childAdded) => myRefreshScrollBar();

         private void myRefreshScrollBar()
         {
            Parent.CtrlScrollBar.Enabled = FoldedLines.Length > NumLinesDisplayable;
            Parent.CtrlScrollBar.Minimum = 1;
            Parent.CtrlScrollBar.Maximum = Parent.CtrlScrollBar.Enabled ? Lines.Length : 0;

            if (Parent.CtrlScrollBar.Value > Parent.CtrlScrollBar.Maximum)
            {
               Parent.CtrlScrollBar.Value = Parent.CtrlScrollBar.Maximum;
            }
         }

         /// <summary>
         /// Returns column relative to a certain screen x
         /// </summary>
         /// <param name="extLine"></param>
         /// <param name="screenX"></param>
         /// <returns></returns>
         /// <exception cref="NotImplementedException"></exception>
         private int myGetTextCol(FoldedLineType extLine, int screenX)
         {
            if (screenX <= (int)Parent.PpMarginPixels) { return 0; }
            else
            {
               var txt = extLine.Text;

               for (int i = 0; i < txt.Length; i++)
               {
                  var tl =
                     (int)Parent.PpMarginPixels +
                     TextRenderer.MeasureText(
                        txt.Substring(0, i + 1), ParentTextControl.Font, new Size(int.MaxValue, int.MaxValue), myTextFormatFlags).Width;

                  if (tl >= screenX) { return i + 1; }
               }

               return txt.Length;
            }
         }
      }

      private abstract class InnerCmd : CmdManagedByControlImpl
      {
         public InnerCmd(ExtendedLineControl parent, string id) : base(parent, id) { }

         public new ExtendedLineControl? Parent => base.Parent as ExtendedLineControl;

         public override bool IsEnabled => true;

         public override bool IsVisible => true;

         public class Copy : InnerCmd
         {
            public Copy(ExtendedLineControl parent) : base(parent, CmdCommonlyUsedIds.COPY) { }

            public override void ActionImpl() => Parent?.MthCopy();
         }
      }

      public int PpLineCurrentId
      {
         get
         {
            var res = -1;

            this.MthInvoke(() => res = myLineContainer.CurrentLineId);

            return res;
         }

         set => this.MthInvoke(() => myLineContainer.CurrentLineId = value);
      }

      public (TxtPos from, TxtPos to)? PpSelectionInterval
      {
         get
         {
            (TxtPos from, TxtPos to)? res = null;

            this.MthInvoke(() =>
            {
               res = mySelectionInterval;
            });

            return res;
         }

         set
         {
            this.MthInvoke(() =>
            {
               if (mySelectionInterval.HasValue)
               {
                  myDoInvalideSelection();
               }

               if ((mySelectionInterval = value).HasValue)
               {
                  myDoInvalideSelection();
               }
            });
         }
      }

      public string PpText
      {
         get
         {
            var res = "";

            this.MthInvoke(() => { res = myGetText(); });

            return res;
         }
      }

      public string PpSelectionText
      {
         get
         {
            var res = "";

            this.MthInvoke(() =>
            {
               if (mySelectionInterval != null)
               {
                  var tok = TxtTokenConst.FromFromTo(myGetText(), mySelectionInterval.Value.from, mySelectionInterval.Value.to);
               }
            });

            return res;
         }
      }

      public int PpLineCount
      {
         get
         {
            var res = 0;

            this.MthInvoke(() => res = myLineContainer.Lines.Length);

            return res;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public Color PpCurrentIdBackColor
      {
         get
         {
            var res = Color.Empty;

            this.MthInvoke(() => { res = myCurrentIdBackColor; });

            return res;
         }

         set
         {
            this.MthInvoke(() =>
            {
               myCurrentIdBackColor = value;
               myLineContainer.RepaintAll();
            });
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public Color PpSelectionBackColor
      {
         get
         {
            var res = Color.Empty;

            this.MthInvoke(() => { res = mySelectionBackColor; });

            return res;
         }

         set
         {
            this.MthInvoke(() =>
            {
               mySelectionBackColor = value;
               myLineContainer.RepaintAll();
            });
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public uint PpLineSpacing
      {
         get
         {
            var res = 0u;

            this.MthInvoke(() => res = myLineSpacing);

            return res;
         }

         set
         {
            this.MthInvoke(() =>
            {
               myLineSpacing = value;
               myLineContainer.RepaintAll();
            });
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public uint PpMarginPixels
      {
         get
         {
            var res = 0u;

            this.MthInvoke(() => res = myMarginPixels);

            return res;
         }

         set
         {
            this.MthInvoke(() =>
            {
               myMarginPixels = value;
               myLineContainer.RepaintAll();
            });
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="line1"></param>
      /// <returns></returns>
      public string this[int line1]
      {
         get
         {
            var res = "";

            this.MthInvoke(() => res = myLineContainer.Lines[line1].Text);

            return res;
         }

         set
         {
            this.MthInvoke(() =>
            {
               var ln = myLineContainer.Lines[line1];

               ln.Text = value;
               myLineContainer.RepaintAll();
            });
         }
      }

      public void MthLineRemove(int line1Idx) => MthLineRemoveRange(new[] { line1Idx });

      public void MthLineRemoveRange(IEnumerable<int> line1IdxRange)
      {
         this.MthInvoke(() =>
         {
            myLineContainer.RemoveLineRange(line1IdxRange.ToArray());
            myLineContainer.RepaintAll();
         });
      }

      public void MthCopy()
      {
         if (PpSelectionInterval.HasValue)
         {
            Clipboard.SetText(PpSelectionText);
         }
      }

      public void MthClear() => this.MthInvoke(() => myLineContainer.ClearScreen());

      public void MthInsertLines(int lineIdx1, params string[] lines) =>
         MthInsertLinesColor(lineIdx1, lines.Select(l => (l, null as Color?)).ToArray());

      public void MthInsertLinesColor(int lineIdx1, params (string, Color?)[] linesColor)
      {
         this.MthInvoke(() =>
         {
            if (linesColor == null || linesColor.Length == 0) { return; }
            else if (Enumerable.Range(1, PpLineCount + 1).Contains(lineIdx1))
            {
               var rng = Enumerable.Range(lineIdx1, linesColor.Length);
               var li = 0;

               myLineContainer.InsertRange(rng);

               foreach (var idx in rng)
               {
                  myLineContainer.Lines[idx].Text = linesColor[li].Item1;
                  myLineContainer.Lines[idx].Color = linesColor[li++].Item2;
               }

               myLineContainer.RepaintAll();
            }
            else
            {
               throw new Gate.Tools.ToolsException($"{lineIdx1} outside range 1-lineCoint");
            }
         });
      }

      public void MthLinesAdd(params string[] lines) => MthInsertLines(PpLineCount + 1, lines);

      public void MthLinesAddcolor(params (string, Color?)[] linesColor) => MthInsertLinesColor(PpLineCount + 1, linesColor);

      protected override void OnFontChanged(EventArgs e)
      {
         base.OnFontChanged(e);
         CtrlInnerText.Font = Font;
      }

      private string myGetText() => string.Join("\r\n", myLineContainer.Lines.Select(l => l.Text));

      private void myDoInvalideSelection()
      {
         if (mySelectionInterval.HasValue)
         {
            var rng = new Interval(mySelectionInterval.Value.from.Line, mySelectionInterval.Value.to.Line).Range;

            foreach (var idx in rng.Where(i => Interval.FromFromLen(0, myLineContainer.Lines.Length).Contains(i)))
            {
               myLineContainer.Lines[idx].Invalidate();
            }
         }
      }

      CmdManagedByControlImpl[] IControlWithManagedCmds.CmdsImpl => myCommands;

      CmdManagedByControlImpl? IControlWithManagedCmds.this[string id] => myCommands?.FirstOrDefault(c => c.Id == id);

      protected override void OnLoad(EventArgs e)
      {
         base.OnLoad(e);
         CtrlInnerText.Focus();
      }

      private void CtrlInnerText_Paint(object? sender, PaintEventArgs e)
      {
         var gr = e.Graphics;

         myLineContainer.PaintLineSet(gr);
      }

      private void CtrlInnerText_KeyDown(object? sender, KeyEventArgs e)
      {
         if (!e.Shift && !e.Control && !e.Alt)
         {
            switch (e.KeyCode)
            {
               case Keys.Up:
                  PpLineCurrentId--;
                  break;

               case Keys.Down:
                  PpLineCurrentId++;
                  break;

               case Keys.Escape:
                  PpSelectionInterval = null;
                  break;
            }
         }
      }

      private void CtrlInnerText_MouseDown(object? sender, MouseEventArgs e)
      {
         if (e.Button == MouseButtons.Left)
         {
            myIsLeftMouseDown = true;
            PpSelectionInterval = null;
         }
      }

      private void CtrlInnerText_MouseUp(object? sender, MouseEventArgs e)
      {
         if (myIsLeftMouseDown)
         {
            myIsLeftMouseDown = false;

            if (PpSelectionInterval == null)
            {
               foreach (var ln_ext in myLineContainer.FoldedLinesVisible)
               {
                  var rc = ln_ext.Rect;

                  if (rc.HasValue && rc.Value.Contains(e.Location))
                  {
                     PpLineCurrentId = ln_ext?.ParentLine?.Idx ?? -1;
                     return;
                  }
               }
            }
         }
      }

      private void CtrlInnerText_MouseMove(object? sender, MouseEventArgs e)
      {
         if (e.Button == MouseButtons.Left)
         {
            var txt_pos = myLineContainer.GetTextPos(e.Location);

            PpSelectionInterval = txt_pos != null ?
               ((TxtPos from, TxtPos to)?)(PpSelectionInterval.HasValue ? PpSelectionInterval.Value.from : txt_pos, txt_pos) :
               null;
         }
      }

      private void CtrlInnerText_Resize(object? sender, EventArgs e) => myLineContainer?.OnResize();

      private void CtrlScrollBar_Scroll(object? sender, ScrollEventArgs e)
      {
         if (myLineContainer.Lines.Length > 0)
         {
            var new_val = e.NewValue;

            if (new_val < 0)
            {
               new_val = 0;
            }
            else if (e.NewValue >= myLineContainer.Lines.Length)
            {
               new_val = myLineContainer.Lines.Length - 1;
            }

            var ln = myLineContainer.Lines[new_val];

            myLineContainer.FoldedLinesFirstId = ln.FoldedLines[0].FoldedGeneralIdx1;
         }
      }
   }
}
