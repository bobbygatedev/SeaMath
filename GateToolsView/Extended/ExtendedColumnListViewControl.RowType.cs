using Gate.ToolsView.Extensions;
using System.ComponentModel;

namespace Gate.ToolsView.Extended
{
   public partial class ExtendedColumnListViewControl
   {
      public class RowType : BaseType
      {
         private ListViewItem? myListViewItem;
         private ExtendedColumnListViewControl? myListViewControl;

         public RowType() { }

         [Browsable(false)]
         public ExtendedColumnListViewControl ListViewControl { get => myListViewControl ?? throw new NullReferenceException(); }

         public CellType[] Cells
         {
            get => myGetSubItems().Cast<CellType>().ToArray();
            set => myResetContainer(value);
         }

         /// <summary>
         /// Index of row.
         /// </summary>
         [Browsable(false)]
         public int Idx => myListViewControl?.myItemContainer.Rows.ToList().IndexOf(this) ?? -1;

         [Browsable(false)]
         public ListViewItem ListViewItem
         {
            get
            {
               if (ListViewControl != null && Idx >= 0)
               {
                  return ListViewControl.CtrlListView.Items[Idx];
               }
               else
               {
                  throw new NullReferenceException();
               }
            }
         }

         [Browsable(false)]
         public object? Tag { get; set; }

         [Browsable(false)]
         public Rectangle Bounds => ListViewItem.Bounds;

         public Color? BackColor { get; set; }

         /// <summary>
         /// 
         /// </summary>
         public int Height => ListViewItem?.Font?.Height ?? -1;

         public override string ToString() => $"ExtendedListView_Row[{Idx}]({string.Join(",", Cells.Select(c => $"\"{c.Text}\""))})";

         protected override void myActionOnListViewReset(ExtendedColumnListViewControl listViewControl)
         {
            if (myListViewItem != null)
            {
               myListViewControl?.MthInvoke(() => myListViewControl.CtrlListView.Items.Remove(myListViewItem));
               myListViewItem.Tag = null;
               myListViewItem = null;
            }

            myListViewControl = null;
         }

         protected override void myActionOnListViewSet(ExtendedColumnListViewControl listViewControl)
         {
            myListViewControl = listViewControl;
            myListViewItem = ListViewControl.CtrlListView.Items.Insert(Idx, new ListViewItem(new string[ListViewControl.PpColumns.Length]));
            myListViewItem.Tag = this;

            var n_cel = Cells.Length;

            for (var i = 0; i < ListViewControl.PpColumns.Length - n_cel; i++) { myAddToContainer(new CellType()); }
         }
      }
   }
}
