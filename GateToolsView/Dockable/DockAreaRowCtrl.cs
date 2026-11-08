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

         public Control? Control => Panel.Controls.Cast<Control>().FirstOrDefault();

         public Panel Panel { get; } = new Panel
         {
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = SystemColors.Control
         };

         public DockableAreaCtrlSlotAnchorModeEnum AnchorMode { get; }

         public Size InitialSize { get; set; }
      };

      public bool PpIsHorizontal { get; set; } = true;

      public SlotType[] PpSlots => myListSlots.ToArray();

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

      public Control? MthControlDock(
         Control? control,
         DockableAreaCtrlSlotAnchorModeEnum anchorMode,
         Size? initialSize = null,
         int? atIndex = null)
      {
         initialSize = initialSize ?? control?.Size ?? new Size(15, 15);
         atIndex = atIndex ?? myListSlots.Count;
         myListSlots.Insert(atIndex.Value, new SlotType(control, anchorMode, initialSize.NnOrCrash()));
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

            foreach (var spl in myListSplitters) { spl.MinSize = myPanelMinSize; }
         }
      }

      public int PpPanelCenterMinSize
      {
         get => myPanelCenterMinSize;

         set
         {
            myPanelCenterMinSize = Math.Max(10, value);

            foreach (var spl in myListSplitters) { spl.MinSize = myPanelCenterMinSize; }
         }
      }

      public Color PpSplitBackColor
      {
         get => mySplitBackColor;

         set
         {
            mySplitBackColor = value;

            foreach (var spl in myListSplitters) { spl.BackColor = mySplitBackColor; }
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
            var nea = PpIsHorizontal ? DockStyle.Left : DockStyle.Top;
            var far = PpIsHorizontal ? DockStyle.Right : DockStyle.Bottom;

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

               spl.Name = "splitter" + i;
               spl.Dock = myIsFar(myListSlots[i + 1]) ? far : nea;

               if (PpIsHorizontal) { spl.Width = 5; }
               else { spl.Height = 5; }

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
                  if (PpIsHorizontal) { p.Width = slo.InitialSize.Width; }
                  if (!PpIsHorizontal) { p.Height = slo.InitialSize.Height; }
               }
            }

            // 5) Dock order, outer first, for each side: panel, then its splitter
            var new_ord = new List<Control>();//near order
            var far_ord = new List<Control>();//far order

            for (int i = 0; i < fil; i++)
            {
               new_ord.Add(myListSlots[i].Panel);
               new_ord.Add(myListSplitters[i]);
            }

            for (int i = n - 1; i > fil; i--)
            {
               far_ord.Add(myListSlots[i].Panel);
               far_ord.Add(myListSplitters[i - 1]);
            }

            // Controls.Add in reverse dock order: fill first, outermost last
            Controls.Add(myListSlots[fil].Panel);

            for (int i = far_ord.Count - 1; i >= 0; i--) { Controls.Add(far_ord[i]); }
            for (int i = new_ord.Count - 1; i >= 0; i--) { Controls.Add(new_ord[i]); }

            ResumeLayout(true);
         }
         else
         {
            ResumeLayout(true);
         }
      }
   }
}
