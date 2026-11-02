using Gate.Tools;
using Gate.Tools.Extensions;
using System.Management;

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
      private DockableCtrlRowDirectionEnum myDirection = DockableCtrlRowDirectionEnum.left_2_right;
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
               Parent?.PpDirection == DockableCtrlRowDirectionEnum.left_2_right ?
                  NoCenterSpaces.Width : NoCenterSpaces.Height;

            set => NoCenterSpaces =
               Parent?.PpDirection == DockableCtrlRowDirectionEnum.left_2_right ?
                  new Size(value, NoCenterSpaces.Height) : new Size(NoCenterSpaces.Width, value);
         }

         internal int PanelSpaceInternal
         {
            get => Parent?.PpDirection == DockableCtrlRowDirectionEnum.left_2_right ?
               Panel.Width : Panel.Height;

            set
            {
               if (Parent.IsDisposed) { throw new Gate.Tools.ToolsException($"{GetType().Name} was disposed"); }

               if (Parent.PpDirection == DockableCtrlRowDirectionEnum.left_2_right)
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
         /// <br> Width for <see cref="DockableCtrlRowDirectionEnum.left_2_right"/> </br>
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
               var is_lft = dir == DockableCtrlRowDirectionEnum.left_2_right;

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
      /// <see cref="DockableCtrlRowDirectionEnum"/> of the control.
      /// </summary> 
      public DockableCtrlRowDirectionEnum PpDirection
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

      public SlotType[] PpSlotsCenter =>
         myListSlots.
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center).
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

      public Control? MthControlDock(
         Control? control,
         DockableAreaCtrlSlotAnchorModeEnum anchorMode,
         Size? initialSize = null,
         int? atIndex = null)
      {
         initialSize = initialSize ?? control?.Size ?? new Size(15, 15);
         myListSlots.Insert(
            myDoReviewIndex(atIndex, anchorMode),
            new SlotType(this, control, anchorMode, initialSize.NnOrCrash()));

         if (PpIsAutoRebuild) { MthRebuild(); }

         return control;
      }

      public void MthRebuild()
      {
         var n = myListSlots.Count;

         SuspendLayout();
         Controls.Clear();

         if (n != 0)
         {
            // 1) Fill slot = last slot that is not "far" (if all are far, the first one)
            var fil = Enumerable.Range(0, n).LastOrDefault(i => !myListSlots[i].IsFar);

            // 2) Splitters: the one before a "far" slot is docked far (resizes that slot),
            //    the others are docked near (resize the slot on their left/top)
            var sps = Enumerable.Range(0, n - 1).Select(i => myDoMakeSplitter(i)).ToArray();
            var pan_scs = myGetPanelSpaces(sps);

            // 3) Dock and size of the panels
            for (var i = 0; i < n; i++)
            {
               var slo = myListSlots[i];
               var pan = myListSlots[i].Panel;

               pan.Name = $"Panel#{i}";
               pan.Dock = i == fil ? DockStyle.Fill : myListSlots[i].DockToSet;
               slo.PanelSpaceInternal = pan_scs[i];
            }

            foreach (var spl in sps)
            {
               spl.SplitterMoved += Spl_SplitterMoved;
            }

            // 4) Dock order, outer first, for each side: panel, then its splitter
            var lst_nea_ord = new List<Control>();//near order
            var lst_far_ord = new List<Control>();//far order

            for (int i = 0; i < fil; i++)
            {
               lst_nea_ord.Add(myListSlots[i].Panel);
               lst_nea_ord.Add(sps[i]);
            }

            for (int i = n - 1; i > fil; i--)
            {
               lst_far_ord.Add(myListSlots[i].Panel);
               lst_far_ord.Add(sps[i - 1]);
            }

            // Controls.Add in reverse dock order: fill first, outermost last
            Controls.Add(myListSlots[fil].Panel);
            Controls.AddRange(lst_far_ord.ToArray().Reverse().ToArray());
            Controls.AddRange(lst_nea_ord.ToArray().Reverse().ToArray());
         }

         ResumeLayout(true);
      }

      private void Spl_SplitterMoved(object? sender, SplitterEventArgs e)
      {
         var spl = sender.ConvertOrCrash<Splitter>();

         /// update <see cref="SlotType.NoCenterSpace"/> of all no center slots with current panel space
         foreach (var slo in 
            myListSlots.Where(s=>s.AnchorMode != DockableAreaCtrlSlotAnchorModeEnum.center))
         {
            slo.NoCenterSpace = slo.PanelSpace;
         }
      }

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
         SuspendLayout();

         // spaces(width/height according direction)
         var sps = myGetPanelSpaces(Controls.OfType<Splitter>().ToArray());

         // keep current (left,top) 
         var x = 0;

         // space index
         var sp_i = 0;

         /// order by position (left/top) since <see cref="MthRebuild"/>
         /// has its special ordering
         var cts = PpDirection == DockableCtrlRowDirectionEnum.left_2_right ?
            Controls.Cast<Control>().OrderBy(c => c.Left).ToArray() :
            Controls.Cast<Control>().OrderBy(c => c.Top).ToArray();

         // place controls:
         // - preserve left/right/top/bottom spaces(width/height according direction)
         // - divide equally remaining space among remaining center spaces
         foreach (var c in cts)
         {
            if (PpDirection == DockableCtrlRowDirectionEnum.left_2_right)
            {
               c.Left = x;
               c.Height = Height;

               if (c is not Splitter)
               {
                  c.Width = sps[sp_i++];
               }

               x = c.Right;
            }
            else
            {
               c.Top = x;
               c.Width = Width;

               if (c is not Splitter)
               {
                  c.Height = sps[sp_i++];
               }

               x = c.Bottom;
            }
         }

         ResumeLayout(true);
      }

      private int[] myGetPanelSpaces(Splitter[] splitter)
      {
         //total space (Width/Height)
         var tot_spa = myGetTotalPanelSpace(splitter);
         var sls_no_cnt = myListSlots.Except(PpSlotsCenter).ToArray();

         // space for center Panel's 
         var cnt_spa = tot_spa - sls_no_cnt.Sum(s => s.NoCenterSpace);
         var szs = new int[myListSlots.Count];

         // id's [0-(nslot-1)] of center slots 
         var cnt_ids =
            Enumerable.Range(0, szs.Length).
            Where(i => myListSlots[i].AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center).
            ToArray();

         // id's [0-(nslot-1)] of no-center slots
         var no_cnt_ids = Enumerable.Range(0, szs.Length).Except(cnt_ids).ToArray();

         // if a single slot   
         var cnt_lim = MIN_CENTER_SLOT_SPACE * PpSlots.Length;

         if (cnt_spa < cnt_lim)
         {
            cnt_spa = Math.Min(cnt_lim, (int)(0.8 * Width));

            if (cnt_ids.Length > 0)
            {
               foreach (var i in cnt_ids)
               {
                  szs[i] = cnt_spa / cnt_ids.Length;
                  tot_spa -= szs[i];
               }
            }

            var w = (double)tot_spa / no_cnt_ids.Sum(i => myListSlots[i].NoCenterSpace); //weight

            foreach (var i in no_cnt_ids)
            {
               szs[i] = i == no_cnt_ids.Last() ? tot_spa : (int)(myListSlots[i].NoCenterSpace * w);
               tot_spa -= szs[i];
            }
         }
         else
         {
            foreach (var i in no_cnt_ids)
            {
               szs[i] = myListSlots[i].NoCenterSpace;
               tot_spa -= szs[i];
            }

            var ini_to_siz = tot_spa;

            foreach (var i in cnt_ids)
            {
               szs[i] = i == cnt_ids.Last() ? tot_spa : ini_to_siz / myListSlots.Count;
               tot_spa -= szs[i];
            }
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
         acs = PpDirection == DockableCtrlRowDirectionEnum.left_2_right ?
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

      private int myGetTotalPanelSpace(Splitter[] splitters)
      {
         switch (PpDirection)
         {
            case DockableCtrlRowDirectionEnum.left_2_right:
               return Width - splitters.Sum(s => s.Width);

            case DockableCtrlRowDirectionEnum.up_2_down:
               return Height - splitters.Sum(s => s.Height);

            default: throw new Crash();
         }
      }
   }
}
