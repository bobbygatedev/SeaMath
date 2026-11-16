using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.ToolsView.Dockable
{
   /// <summary>
   /// A 'row' (horizontal or vertical) of controls separated by a splitter 
   /// </summary>
   public partial class DockableAreaRowCtrl : UserControl
   {
      /// <summary>
      /// Minimum space (width or height) of a center slot.
      /// </summary>
      public const int MIN_CENTER_SLOT_SPACE = 50;

      private readonly List<SlotType> myListSlots = new List<SlotType>();

      /// <summary>
      /// Splitter back color.
      /// </summary>
      private Color? mySplitBackColor = null;
      private DockableRowDirection myDirection = DockableRowDirection.left_2_right;
      private int mySplitWidth = 0;

      public DockableAreaRowCtrl()
      {
         InitializeComponent();
      }

      /// <summary>
      /// 
      /// </summary>
      public class SlotType : IDisposable
      {
         /// <summary>
         /// 
         /// </summary>
         /// <param name="parent"></param>
         /// <param name="control"></param>
         /// <param name="anchorMode"></param>
         /// <param name="initialSize"></param>
         internal SlotType(
            DockableAreaRowCtrl parent,
            Control? control,
            DockableAreaCtrlSlotAnchorModeEnum anchorMode,
            Size initialSize)
         {
            if (control != null)
            {
               control.Dock = DockStyle.Fill;
               Panel.Size = control.Size;
               Panel.Controls.Add(control);
            }

            AnchorMode = anchorMode;
            NoCenterSpaces = initialSize;
            Parent = parent;
         }

         public DockableAreaRowCtrl Parent { get; }

         public Control? Control
         {
            get => Panel.Controls.Count > 0 ? Panel.Controls[0] : null;
            set
            {
               Panel.SuspendLayout();

               // detach, NOT dispose
               foreach (var c in Panel.Controls.Cast<Control>().ToArray()) { Panel.Controls.Remove(c); }

               if (value != null)
               {
                  value.Dock = DockStyle.Fill;
                  Panel.Controls.Add(value);
               }

               Panel.ResumeLayout(true);
            }
         }

         public Panel Panel { get; } = new Panel
         {
            BorderStyle = BorderStyle.None,
            BackColor = SystemColors.Control
         };

         public DockableAreaCtrlSlotAnchorModeEnum AnchorMode { get; }

         /// <summary>
         /// No-center width,height
         /// </summary>
         public Size NoCenterSpaces { get; set; }

         internal int NoCenterSpace
         {
            get =>
               Parent?.PpDirection == DockableRowDirection.left_2_right ?
                  NoCenterSpaces.Width : NoCenterSpaces.Height;

            set => NoCenterSpaces =
               Parent?.PpDirection == DockableRowDirection.left_2_right ?
                  new Size(value, NoCenterSpaces.Height) : new Size(NoCenterSpaces.Width, value);
         }

         internal int PanelSpaceInternal
         {
            get => Parent?.PpDirection == DockableRowDirection.left_2_right ?
               Panel.Width : Panel.Height;

            set
            {
               if (Parent.IsDisposed) { throw new Gate.Tools.ToolsException($"{GetType().Name} was disposed"); }

               if (Parent.PpDirection == DockableRowDirection.left_2_right)
               {
                  Panel.Width = value;
               }
               else
               {
                  Panel.Height = value;
               }
            }
         }

         /// <summary>
         /// <br> Significant dimension of Panel: </br> 
         /// <br> Width for <see cref="DockableRowDirection.left_2_right"/> </br>
         /// </summary>
         public int PanelSpace
         {
            get => PanelSpaceInternal;

            set
            {
               PanelSpaceInternal = value;
               Parent.MthRebuild();
            }
         }

         public bool IsDisposed { get; private set; } = false;

         public void Dispose()
         {
            if (!IsDisposed)
            {
               IsDisposed = true;

               if (Control != null)
               {
                  Control.Parent = null;
               }
            }
         }

         /// <summary>
         /// Near/Far respect to splitter order left-right top-down 
         /// </summary>
         internal bool IsFar =>
            AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.right ||
            AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.down;

         /// <summary>
         /// 
         /// </summary>
         internal DockStyle DockToSet
         {
            get
            {
               var dir = Parent?.PpDirection ?? throw new Gate.Tools.ToolsException();
               var is_lft = dir == DockableRowDirection.left_2_right;

               if (IsFar)
               {
                  return is_lft ? DockStyle.Right : DockStyle.Bottom;
               }
               else
               {
                  return is_lft ? DockStyle.Left : DockStyle.Top;
               }
            }
         }

         public override string ToString() =>
            $"{AnchorMode} {NoCenterSpace} {NoCenterSpaces}" +
            $"(c={Control?.GetType().Name},n={Control?.Name},t={Control?.Text})";
      };

      /// <summary>
      /// 
      /// </summary>
      public int PpSplitWidth
      {
         get => mySplitWidth;

         set
         {
            mySplitWidth = value;
            MthRebuild();
         }
      }

      /// <summary>
      /// Direction is not effective when only center slot are present and number of them is <= 1s
      /// </summary>
      public DockableRowDirection? PpDirectionEffective => 
         PpSlotsCenter.Length == PpSlots.Length && 
         PpSlotsCenterNotNull.Length <= 1 ? null : PpDirection;

      /// <summary>
      /// <see cref="DockableRowDirection"/> of the control.
      /// </summary> 
      public DockableRowDirection PpDirection
      {
         get => myDirection;

         set
         {
            myDirection = value;

            if (PpIsAutoRebuild) { MthRebuild(); }
         }
      }
      public Control[] PpControlsAll => myListSlots.Select(c => c.Control).Nn().ToArray();

      public SlotType[] PpSlots => myListSlots.ToArray();

      public SlotType[] PpSlotsNotCenter=> PpSlots.Except(PpSlotsCenter).ToArray();

      public SlotType[] PpSlotsNotNull => myListSlots.Where(s=>s.Control != null).ToArray();

      public SlotType[] PpSlotsCenter =>
         myListSlots.
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center).
         ToArray();

      public SlotType[] PpSlotsCenterNotNull =>
         myListSlots.
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center && s.Control != null).
         ToArray();

      /// <summary>
      /// 
      /// </summary>
      public bool PpIsAutoRebuild { get; set; } = true;

      /// <summary>
      /// Splitter back color.
      /// </summary>
      public Color? PpSplitBackColor
      {
         get => mySplitBackColor;

         set
         {
            mySplitBackColor = value;

            if (PpIsAutoRebuild) { MthRebuild(); }
         }
      }

      public bool MthRemoveIndex(int index)
      {
         var slo = myListSlots.ElementAtOrDefault(index);

         if (slo != null)
         {
            MthRemoveSlots(slo);

            return true;
         }
         else
         {
            return false;
         }
      }

      public void MthRemoveSlots(params SlotType[] slotsToRemove)
      {
         if (slotsToRemove.All(s => myListSlots.ContainsRef(s)))
         {
            foreach (var slo in slotsToRemove)
            {
               myListSlots.Remove(slo);
               slo.Dispose();
            }

            if (PpIsAutoRebuild) { MthRebuild(); }
         }
         else
         {
            throw new Gate.Tools.ToolsException($"Not all slots belong to this {GetType().Name}");
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="controls"></param>
      /// <param name="anchorMode"></param>
      /// <param name="atIndex"></param>
      /// <returns></returns>
      public SlotType[] MthControlsDock(
         Control[] controls, DockableAreaCtrlSlotAnchorModeEnum anchorMode, int? atIndex = null)
      {
         if (controls.Length == 0) { return []; }

         var lst = new List<SlotType>();
         var idx = atIndex;

         foreach (var ctr in controls)
         {
            var slo = new SlotType(this, ctr, anchorMode, ctr?.Size ?? new Size(15, 15));
            var pos = myDoReviewIndex(idx, anchorMode);

            lst.Add(slo);
            myListSlots.Insert(pos, slo);

            if (idx.HasValue) { idx = pos + 1; }
         }

         if (PpIsAutoRebuild) { MthRebuild(); }

         return lst.ToArray();
      }

      public Control? MthControlDock(
         Control? control,
         DockableAreaCtrlSlotAnchorModeEnum anchorMode,
         Size? noCenterSpaces = null,
         int? atIndex = null)
      {
         noCenterSpaces = noCenterSpaces ?? control?.Size ?? new Size(15, 15);
         myListSlots.Insert(
            myDoReviewIndex(atIndex, anchorMode),
            new SlotType(this, control, anchorMode, noCenterSpaces.NnOrCrash()));

         if (PpIsAutoRebuild) { MthRebuild(); }

         return control;
      }

      public void MthRebuild()
      {
         var n = myListSlots.Count;

         SuspendLayout();

         // 0) Dispose old splitters (and unhook handlers), panels are kept
         foreach (var spl in Controls.OfType<Splitter>().ToArray())
         {
            spl.SplitterMoved -= Spl_SplitterMoved;
            spl.Dispose();
         }

         Controls.Clear();

         if (n != 0)
         {
            // 1) Fill slot = last slot that is not "far" (if all are far, the first one)
            var fil = Enumerable.Range(0, n).LastOrDefault(i => !myListSlots[i].IsFar);

            // 2) Splitters: the one before a "far" slot is docked far (resizes that slot),
            //    the others are docked near (resize the slot on their left/top)
            var sps = Enumerable.Range(0, n - 1).Select(i => myDoMakeSplitter(i)).ToArray();
            var pan_scs = myGetPanelSpaces();

            // 3) Dock and size of the panels
            for (var i = 0; i < n; i++)
            {
               var slo = myListSlots[i];
               var pan = slo.Panel;

               pan.Name = $"Panel#{i}";
               pan.Dock = i == fil ? DockStyle.Fill : slo.DockToSet;

               if (i != fil) { slo.PanelSpaceInternal = pan_scs[i]; }
            }

            foreach (var spl in sps) { spl.SplitterMoved += Spl_SplitterMoved; }

            // 4) Dock order, outer first, for each side: panel, then its splitter
            var lst_nea_ord = new List<Control>();
            var lst_far_ord = new List<Control>();

            for (var i = 0; i < fil; i++)
            {
               lst_nea_ord.Add(myListSlots[i].Panel);
               lst_nea_ord.Add(sps[i]);
            }

            for (var i = n - 1; i > fil; i--)
            {
               lst_far_ord.Add(myListSlots[i].Panel);
               lst_far_ord.Add(sps[i - 1]);
            }

            // Controls.Add in reverse dock order: fill first, outermost last
            lst_far_ord.Reverse();
            lst_nea_ord.Reverse();

            Controls.Add(myListSlots[fil].Panel);
            Controls.AddRange(lst_far_ord.ToArray());
            Controls.AddRange(lst_nea_ord.ToArray());
         }

         ResumeLayout(true);
      }
      public void MthDirectionChange() => PpDirection =
            PpDirection == DockableRowDirection.left_2_right ?
               DockableRowDirection.up_2_down :
               DockableRowDirection.left_2_right;

      public bool MthRemoveControl(Control control)
      {
         var slo = PpSlots.FirstOrDefault(s => ReferenceEquals(s.Control, control));

         if (slo != null)
         {
            MthRemoveSlots(slo);

            return true;
         }
         else
         {
            return false;
         }
      }

      protected override void OnBackColorChanged(EventArgs e)
      {
         foreach (var spl in Controls.OfType<Splitter>())
         {
            spl.BackColor = PpSplitBackColor ?? BackColor;
         }

         base.OnBackColorChanged(e);
      }

      protected override void OnResize(EventArgs e)
      {
         base.OnResize(e);

         if (myListSlots.Count == 0) { return; }

         SuspendLayout();

         var pan_scs = myGetPanelSpaces();

         for (var i = 0; i < myListSlots.Count; i++)
         {
            var slo = myListSlots[i];

            // Fill panel is sized by the dock layout
            if (slo.Panel.Dock != DockStyle.Fill && !slo.IsDisposed)
            {
               slo.PanelSpaceInternal = pan_scs[i];
            }
         }

         ResumeLayout(true);
      }
      private int myGetTotalPanelSpace()
      {
         var spl_tot = Math.Max(0, myListSlots.Count - 1) * PpSplitWidth;

         var tot = PpDirection == DockableRowDirection.left_2_right ?
            ClientSize.Width : ClientSize.Height;

         return Math.Max(0, tot - spl_tot);
      }

      /// <summary>
      /// Distributes <paramref name="total"/> among <paramref name="ids"/> proportionally
      /// to <paramref name="weights"/> (equally if weights sum is 0), last one takes the remainder.
      /// </summary>
      private static void myDoDistribute(int[] szs, int[] ids, Func<int, int> weights, int total)
      {
         if (ids.Length == 0) { return; }

         var sum = ids.Sum(weights);
         var rem = total;

         foreach (var i in ids)
         {
            var s = i == ids.Last() ? rem :
               sum > 0 ? (int)((long)total * weights(i) / sum) : total / ids.Length;

            szs[i] = Math.Max(0, s);
            rem -= szs[i];
         }
      }

      private int[] myGetPanelSpaces()
      {
         var n = myListSlots.Count;
         var szs = new int[n];

         if (n == 0) { return szs; }

         var tot_spa = myGetTotalPanelSpace();

         // id's of center / no-center slots
         var cnt_ids = Enumerable.Range(0, n).
            Where(i => myListSlots[i].AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center).
            ToArray();
         var no_cnt_ids = Enumerable.Range(0, n).Except(cnt_ids).ToArray();

         var no_cnt_sum = no_cnt_ids.Sum(i => myListSlots[i].NoCenterSpace);
         var cnt_spa = tot_spa - no_cnt_sum;
         var cnt_lim = MIN_CENTER_SLOT_SPACE * cnt_ids.Length;

         if (cnt_ids.Length > 0 && cnt_spa < cnt_lim)
         {
            // not enough space for centers: they get their minimum (max 80%),
            // no-center slots share the rest proportionally to their preferred space
            cnt_spa = Math.Min(cnt_lim, (int)(0.8 * tot_spa));

            myDoDistribute(szs, cnt_ids, _ => 1, cnt_spa);
            myDoDistribute(szs, no_cnt_ids, i => myListSlots[i].NoCenterSpace, tot_spa - cnt_spa);
         }
         else
         {
            // no-center slots keep their space, centers share the rest equally
            foreach (var i in no_cnt_ids) { szs[i] = Math.Max(0, myListSlots[i].NoCenterSpace); }

            myDoDistribute(szs, cnt_ids, _ => 1, Math.Max(0, cnt_spa));
         }

         return szs;
      }
      private Splitter myDoMakeSplitter(int index) => new Splitter
      {
         Width = Height = PpSplitWidth,
         BackColor = PpSplitBackColor ?? BackColor,
         Name = $"Splitter{index}",
         Dock = myListSlots[index + 1].DockToSet
      };

      private int myDoReviewIndex(int? atIndex, DockableAreaCtrlSlotAnchorModeEnum anchorMode)
      {
         var acs = null as DockableAreaCtrlSlotAnchorModeEnum[];

         //anchor modes
         acs = PpDirection == DockableRowDirection.left_2_right ?
            [
               DockableAreaCtrlSlotAnchorModeEnum.left,
               DockableAreaCtrlSlotAnchorModeEnum.center,
               DockableAreaCtrlSlotAnchorModeEnum.right] :
            [
               DockableAreaCtrlSlotAnchorModeEnum.up,
               DockableAreaCtrlSlotAnchorModeEnum.center,
               DockableAreaCtrlSlotAnchorModeEnum.down];

         var a_idx = acs.ToList().IndexOf(anchorMode);

         if (a_idx >= 0)
         {
            var sc = acs.Select(a => PpSlots.Count(s => s.AnchorMode == a)).ToArray();

            //anchor mode per slot
            var ars = PpSlots.Select(s => s.AnchorMode).ToArray();
            var idx = 0;

            var lst = new List<(int fro, int to)>();

            foreach (var a in acs)
            {
               if (ars.Any(a1 => a1 == a))
               {
                  var max = Enumerable.Range(0, ars.Length).Where(i => ars[i] == a).Max();

                  lst.Add((idx, max));
               }
               else
               {
                  lst.Add((idx, idx - 1));
               }

               idx = lst.Last().to + 1;
            }

            if (atIndex.HasValue)
            {
               //tododo bug
               return atIndex.Value >= lst[a_idx].fro && atIndex.Value <= lst[a_idx].to + 1 ?
                  atIndex.Value :
                  throw new Gate.Tools.ToolsException(
                     $"Index {atIndex.Value} not valid for anchor {anchorMode}" +
                     $"({lst[a_idx].fro}-{lst[a_idx].to + 1})");
            }
            else
            {
               switch (anchorMode)
               {
                  case DockableAreaCtrlSlotAnchorModeEnum.center:
                  case DockableAreaCtrlSlotAnchorModeEnum.right:
                  case DockableAreaCtrlSlotAnchorModeEnum.down:
                     return lst[a_idx].to + 1;

                  case DockableAreaCtrlSlotAnchorModeEnum.left:
                  case DockableAreaCtrlSlotAnchorModeEnum.up:
                     return lst[a_idx].fro;

                  default: throw new Crash();
               }

            }
         }
         else
         {
            throw new Gate.Tools.ToolsException($"Not valid anchor {anchorMode}");
         }
      }

      private void Spl_SplitterMoved(object? sender, SplitterEventArgs e)
      {
         var spl = sender.ConvertOrCrash<Splitter>();

         /// update <see cref="SlotType.NoCenterSpace"/> of all no center slots with current panel space
         foreach (var slo in
            myListSlots.Where(s => s.AnchorMode != DockableAreaCtrlSlotAnchorModeEnum.center))
         {
            slo.NoCenterSpace = slo.PanelSpace;
         }
      }

      /// <summary>
      /// Adds <paramref name="slots"/> from other <see cref="DockableAreaRowCtrl"/> 
      /// </summary>
      /// <param name="slots"></param>
      public void MthAddSlotsFromOther(SlotType[] slots)
      {
         var old_auto = PpIsAutoRebuild;

         PpIsAutoRebuild = false;

         try
         {
            foreach (var slo in slots)
            {
               var ctr = slo.Control;

               if (ctr != null) { ctr.Parent = null; }

               MthControlDock(ctr, slo.AnchorMode, slo.NoCenterSpaces);
            }
         }
         finally
         {
            PpIsAutoRebuild = old_auto;
         }

         if (PpIsAutoRebuild) { MthRebuild(); }
      }
   }
}
