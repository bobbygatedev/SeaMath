using Gate.Tools.Extensions;
using Gate.ToolsView.Dockable;
using Gate.ToolsView.Extensions;

namespace GateToolsViewTest
{
   /// <summary>
   /// tododo
   /// </summary>
   public partial class DockAreaRowCtrl : UserControl
   {
      private List<RowItem> myListItems = new List<RowItem>();

      public DockAreaRowCtrl()
      {
         InitializeComponent();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="control"></param>
      /// <returns></returns>

      public class RowItem
      {
         public RowItem(
            DockAreaRowCtrl parent,
            Control? control,
            DockableAreaCtrlSlotAnchorModeEnum anchorMode)
         {
            Parent = parent;
            Control = control;
            AnchorMode = anchorMode;
         }

         public override string ToString() => $"{AnchorMode} ({Control?.GetType().Name},{Control?.Text})";

         public DockAreaRowCtrl Parent { get; }
         public Control? Control { get; }
         public DockableAreaCtrlSlotAnchorModeEnum AnchorMode { get; }

         public FixedPanel FixedPanel
         {
            get
            {
               switch (AnchorMode)
               {
                  case DockableAreaCtrlSlotAnchorModeEnum.center:
                     var cid = Parent.PpItemsCenter.ToList().IndexOf(this);

                     return cid == 0 ? FixedPanel.Panel2 : FixedPanel.None;

                  case DockableAreaCtrlSlotAnchorModeEnum.left: return FixedPanel.Panel1;
                  case DockableAreaCtrlSlotAnchorModeEnum.right:
                     var rid = Parent.PpItemsRight.ToList().IndexOf(this);

                     return rid == 0 ? FixedPanel.Panel2 : FixedPanel.Panel1;

                  default: throw new Gate.Tools.ToolsException("");
               }
            }
         }

         public FixedPanel FixedPanelOpposite
         {
            get
            {
               switch (FixedPanel)
               {
                  case FixedPanel.Panel1: return FixedPanel.Panel2;
                  case FixedPanel.Panel2: return FixedPanel.Panel1;
                  default: return FixedPanel.None;
               }
            }
         }
      }

      public RowItem[] PpItemsAll => myListItems.ToArray();
      public RowItem[] PpItemsLeft =>
         myListItems.
         Where(i => i.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.left).ToArray();
      public RowItem[] PpItemsRight =>
         myListItems.Where(i => i.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.right).ToArray();

      public RowItem[] PpItemsCenter =>
         myListItems.Where(i => i.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center).ToArray();

      private SplitContainer myGetSplitContainer()
      {
         var sc = new SplitContainer();

         sc.Dock = DockStyle.Fill;
         sc.SplitterMoved += Sc_SplitterMoved;
         sc.SplitterMoving += Sc_SplitterMoving;

         return sc;
      }


      private (int idx, RowItem? row) myGetRowItem(SplitContainer splitContainer)
      {
         var idx = myGetSplitContainers().ToList().IndexOf(splitContainer);

         if (idx < PpItemsLeft.Length) { return (idx, myListItems[idx]); }
         else if (idx < PpItemsLeft.Length + PpItemsCenter.Length - 1) { return (idx, myListItems[idx]); }
         else { return (idx, myListItems.ElementAtOrDefault(idx + 1)); }
      }


      private void Sc_SplitterMoving(object? sender, SplitterCancelEventArgs e)
      {
         var sc = sender.ConvertOrCrash<SplitContainer>();
         var itm = myGetRowItem(sc);

         sc.FixedPanel = itm.row.FixedPanelOpposite;
      }

      private void Sc_SplitterMoved(object? sender, SplitterEventArgs e)
      {
         var sc = sender.ConvertOrCrash<SplitContainer>();
         var itm = myGetRowItem(sc);

         sc.FixedPanel = itm.row?.FixedPanel ?? FixedPanel.None;
      }

      public void MthControlDock(Control? control, DockableAreaCtrlSlotAnchorModeEnum anchorMode)
      {
         var scs = myGetSplitContainers();
         var sc = myGetSplitContainer();

         myListItems.Add(new RowItem(this, control, anchorMode));

         if (scs.Length == 0)
         {
            Controls.Add(sc);

            if (control != null)
            {
               sc.Panel1.Controls.Add(control);
               control.Dock = DockStyle.Fill;
            }
         }
         else if (scs.LastOrDefault().NnOrCrash().Panel2.Controls.Count == 0)
         {
            if (control != null)
            {
               scs.LastOrDefault().NnOrCrash().Panel2.Controls.Add(control);
               control.Dock = DockStyle.Fill;
            }
         }
         else
         {
            var old_ctr = scs.LastOrDefault().NnOrCrash().Panel2.Controls[0];

            scs.LastOrDefault().NnOrCrash().Panel2.Controls.Clear();
            scs.LastOrDefault().NnOrCrash().Panel2.Controls.Add(sc);
            sc.Panel1.Controls.Add(old_ctr);
            sc.Panel2.Controls.Add(control);

            if (control != null)
            {
               control.Dock = DockStyle.Fill;
            }
         }

         foreach (var ss in myGetSplitContainers())
         {
            var itm = myGetRowItem(ss);

            ss.FixedPanel = itm.row?.FixedPanel ?? FixedPanel.None;
         }
      }

      private SplitContainer[] myGetSplitContainers() => this.MthGetNephews().OfType<SplitContainer>().ToArray();
   }
}
