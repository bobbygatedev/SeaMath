using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.ToolsView.Dockable
{
   /// <summary>
   /// 
   /// </summary>
   public partial class DockAreaRowCtrl : UserControl
   {
      private readonly List<Splitter> myListSplitters = new List<Splitter>();
      private readonly List<SlotType> myListSlots = new List<SlotType>();

      private int myPanelMinSize = 25;
      private int myPanelCenterMinSize = 25;

      /// <summary>
      /// Default splitter color.
      /// </summary>
      private Color mySplitBackColor = new Splitter().BackColor;
      private DockableCtrlRowDirectionEnum myDirection = DockableCtrlRowDirectionEnum.left_2_right;

      public DockAreaRowCtrl()
      {
         InitializeComponent();
      }

      public class SlotType
      {
         public SlotType(Control? control, DockableAreaCtrlSlotAnchorModeEnum anchorMode, Size initialSize)
         {
            if (control != null)
            {
               control.Dock = DockStyle.Fill;
               Panel.Controls.Add(control);
            }

            AnchorMode = anchorMode;
            InitialSize = initialSize;
         }

         public Control? Control
         {
            get => Panel.Controls.Cast<Control>().FirstOrDefault();

            set
            {
               Panel.Controls.Clear();

               if (value != null)
               {
                  value.Dock = DockStyle.Fill;
                  Panel.Controls.Add(value);
               }
            }
         }

         public Panel Panel { get; } = new Panel
         {
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = SystemColors.Control
         };

         public DockableAreaCtrlSlotAnchorModeEnum AnchorMode { get; }

         public Size InitialSize { get; set; }
      };

      /// <summary>
      /// <see cref="DockableCtrlRowDirectionEnum"/> of the control.
      /// </summary> 
      public DockableCtrlRowDirectionEnum PpDirection
      {
         get => myDirection;

         set
         {
            myDirection = value;
            myDoRebuild();
         }
      }

      public Control[] PpControlsAll => myListSlots.Select(c => c.Control).Nn().ToArray();

      public SlotType[] PpSlots => myListSlots.ToArray();

      public SlotType[] PpSlotsCenter =>
         myListSlots.
         Where(x => x.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center).
         ToArray();

      public void MthRemove(int index)
      {
         if (myListSlots.Count > 1 && index >= 0 && index < myListSlots.Count)
         {
            var p = myListSlots[index];

            myListSlots.RemoveAt(index);
            myDoRebuild();
            p.Panel.Dispose();
         }
      }

      public void MthRemove(SlotType slot)
      {
         int i = myListSlots.IndexOf(slot);

         if (i >= 0) { MthRemove(i); }
      }

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

      public Control? MthControlDock(
         Control? control,
         DockableAreaCtrlSlotAnchorModeEnum anchorMode,
         Size? initialSize = null,
         int? atIndex = null)
      {
         initialSize = initialSize ?? control?.Size ?? new Size(15, 15);
         myListSlots.Insert(
            myDoReviewIndex(atIndex, anchorMode), new SlotType(control, anchorMode, initialSize.NnOrCrash()));
         myDoRebuild();

         return control;
      }

      private static bool myIsFar(SlotType slot) =>
         slot.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.right ||
         slot.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.down;

      public int PpPanelMinSize
      {
         get => myPanelMinSize;

         set
         {
            myPanelMinSize = Math.Max(15, value);
            myDoRebuild();
         }
      }

      public int PpPanelCenterMinSize
      {
         get => myPanelCenterMinSize;

         set
         {
            myPanelCenterMinSize = Math.Max(10, value);
            myDoRebuild();
         }
      }

      public Color PpSplitBackColor
      {
         get => mySplitBackColor;

         set
         {
            mySplitBackColor = value;
            myDoRebuild();
         }
      }

      private Splitter myDoMakeSplitter() => new Splitter
      {
         BackColor = PpSplitBackColor,
         MinSize = PpPanelMinSize,
         MinExtra = PpPanelCenterMinSize
      };

      private void myDoRebuild()
      {
         SuspendLayout();

         // 1) Detach splitters (disposed) and panels (kept)
         foreach (var spl in myListSplitters)
         {
            Controls.Remove(spl);
            spl.Dispose();
         }

         myListSplitters.Clear();

         foreach (var slo in myListSlots) { Controls.Remove(slo.Panel); }

         var n = myListSlots.Count;

         if (n != 0)
         {
            var nea = PpDirection == DockableCtrlRowDirectionEnum.left_2_right ?
               DockStyle.Left : DockStyle.Top;
            var far = PpDirection == DockableCtrlRowDirectionEnum.left_2_right ?
               DockStyle.Right : DockStyle.Bottom;

            // 2) Fill slot = last slot that is not "far" (if all are far, the first one)
            var fil = 0;

            for (var i = 0; i < n; i++)
            {
               if (!myIsFar(myListSlots[i])) { fil = i; }
            }

            // 3) Splitters: the one before a "far" slot is docked far (resizes that slot),
            //    the others are docked near (resize the slot on their left/top)
            for (int i = 0; i < n - 1; i++)
            {
               var spl = myDoMakeSplitter();

               spl.Name = $"Splitter{i}";
               spl.Dock = myIsFar(myListSlots[i + 1]) ? far : nea;

               if (PpDirection == DockableCtrlRowDirectionEnum.left_2_right)
               {
                  spl.Width = 5;
               }
               else
               {
                  spl.Height = 5;
               }

               myListSplitters.Add(spl);
            }

            // 4) Dock and size of the panels
            for (var i = 0; i < n; i++)
            {
               var slo = myListSlots[i];
               var p = myListSlots[i].Panel;

               p.Name = $"Panel#{i}";

               if (i == fil)
               {
                  p.Dock = DockStyle.Fill;
               }
               else
               {
                  p.Dock = myIsFar(myListSlots[i]) ? far : nea;

                  // keep current size if already set
                  switch (PpDirection)
                  {
                     case DockableCtrlRowDirectionEnum.left_2_right:
                        p.Width = slo.InitialSize.Width;
                        break;

                     case DockableCtrlRowDirectionEnum.up_2_down:
                        p.Height = slo.InitialSize.Height;
                        break;

                     default: throw new Crash();
                  }
               }
            }

            // 5) Dock order, outer first, for each side: panel, then its splitter
            var lst_new_ord = new List<Control>();//near order
            var lst_far_ord = new List<Control>();//far order

            for (int i = 0; i < fil; i++)
            {
               lst_new_ord.Add(myListSlots[i].Panel);
               lst_new_ord.Add(myListSplitters[i]);
            }

            for (int i = n - 1; i > fil; i--)
            {
               lst_far_ord.Add(myListSlots[i].Panel);
               lst_far_ord.Add(myListSplitters[i - 1]);
            }

            // Controls.Add in reverse dock order: fill first, outermost last
            Controls.Add(myListSlots[fil].Panel);

            for (int i = lst_far_ord.Count - 1; i >= 0; i--) { Controls.Add(lst_far_ord[i]); }
            for (int i = lst_new_ord.Count - 1; i >= 0; i--) { Controls.Add(lst_new_ord[i]); }

            ResumeLayout(true);
         }
         else
         {
            ResumeLayout(true);
         }
      }

      public bool MthRemove(Control control)
      {
         var slo = PpSlots.FirstOrDefault(x => ReferenceEquals(x.Control, control));

         if (slo != null)
         {
            MthRemove(slo);

            return true;
         }
         else
         {
            return false;
         }
      }
   }
}
