using Gate.Tools;
using Gate.Tools.Extensions;
using System.ComponentModel;

namespace Gate.ToolsView.Extended
{
   public partial class ExtendedColumnListViewControl
   {
      public class CellType : BaseType
      {
         public delegate void OnUnembedHandler(CellType cell);

         public event OnUnembedHandler? OnUnembed;

         private string? myText;
         private Image? myImage;
         private EditorType? myEditor;
         private PaintMethodHandler? myPaintMethod;
         private ImageModeFlags myImageMode = ImageModeFlags.none;
         private Color? myBackColor;
         private Control? myEmbeddedControlPermanent;
         private RowType? myRow;
         private ListViewItem.ListViewSubItem? myListViewSubItem;

         public CellType() { }

         [Browsable(false)]
         public ListViewItem.ListViewSubItem ListViewSubItem { get => myListViewSubItem ?? throw new NullReferenceException(); }

         [Browsable(false)]
         public ListViewItem ListViewItem => Row.ListViewItem;

         [Browsable(false)]
         public RowType Row { get => myRow ?? throw new NullReferenceException(); }

         [Browsable(false)]
         public ExtendedColumnListViewControl? ParentListView { get; private set; }

         public Rectangle Bounds => myGetSubItemBounds(ListViewItem, ListViewSubItem ?? throw new Gate.Tools.ToolsException());

         public string? Text
         {
            get => myText;

            set
            {
               myText = value;

               if (ParentListView != null)
               {
                  ParentListView.CtrlListView.Items[RowIdx].Text = value;//this causes refresh of line only
               }
            }
         }

         public PaintMethodHandler? PaintMethod
         {
            get => myPaintMethod;

            set
            {
               myPaintMethod = value;
               ParentListView?.Refresh();
            }
         }

         public ImageModeFlags ImageMode
         {
            get => myImageMode;

            set
            {
               myImageMode = value;
               ParentListView?.Refresh();
            }
         }

         public Image? Image
         {
            get => myImage;

            set
            {
               myImage = value;
               ParentListView?.Refresh();
            }
         }

         public Color? GridColor { get; set; }

         [Browsable(false)]
         public Color? GridColorEffective => GridColor ?? ParentListView?.PpGridColor;

         public float? GridWidth { get; set; }

         [Browsable(false)]
         public float? GridWithEffective => GridWidth ?? ParentListView?.PpGridWidth;

         [Browsable(false)]
         public PaintMethodHandler? EffectivePaintMethod => PaintMethod ?? Column?.ColumnPaintMethod;

         [Browsable(false)]
         public ImageModeFlags EffectiveImageMode => ImageMode == ImageModeFlags.none ?
            Column?.ColumnImageMode != ImageModeFlags.none ? Column?.ColumnImageMode ?? ImageModeFlags.none : ImageModeFlags.center :
            ImageMode;

         [Browsable(false)]
         public Image? EffectiveImage => Image ?? Column?.ColumnImage;

         /// <summary>
         /// Back color, if not set equal to <see cref="Column"/>.Backcolor <see cref="Row"/>.Backcolor in this order.
         /// </summary>
         public Color? BackColor
         {
            get => myBackColor ?? Column?.BackColor ?? Row?.BackColor;

            set => myBackColor = value;
         }

         /// <summary>
         /// 
         /// </summary>
         /// <exception cref="IndexOutOfRangeException"></exception>
         public ColumnType Column => ColIdx >= 0 && ColIdx < ParentListView?.PpColumns.Length ?
            ParentListView.PpColumns[ColIdx] : throw new IndexOutOfRangeException();

         /// <summary>
         /// 
         /// </summary>
         public CellAutoEditMode CellAutoEdit { get; set; } = CellAutoEditMode.none;

         /// <summary>
         /// Effective auto edit mode (which compose column and cell auto edit).
         /// </summary>
         [Browsable(false)]
         public CellAutoEditMode EffectiveAutoEdit => CellAutoEdit == CellAutoEditMode.none && Column != null ? Column.AutoEdit : CellAutoEdit;

         [Browsable(false)]
         public int RowIdx => myRow != null ? myRow.Idx : -1;

         [Browsable(false)]
         public int ColIdx => myRow != null ? myRow.Cells.ToList().IndexOf(this) : -1;

         [Browsable(false)]
         public object? Tag { get; set; }

         public override string ToString() => $"ExtendedListView_Cell[{RowIdx}][{ColIdx}]({Text})";

         protected override void myActionOnListViewSet(ExtendedColumnListViewControl listViewControl)
         {
            myRow = myGetParentItem() as RowType;
            ParentListView = listViewControl;
            ParentListView.CtrlListView.ColumnWidthChanged += CtrlListView_ColumnWidthChanged;

            if (ColIdx < listViewControl.PpColumns.Length)
            {
               myListViewSubItem = listViewControl.CtrlListView.Items[RowIdx].SubItems[ColIdx];
               ListViewSubItem.Tag = this;
            }

            if (myEmbeddedControlPermanent != null) { myEmbeddedControlPermanent.Parent = listViewControl.CtrlListView; }
         }

         protected override void myActionOnListViewReset(ExtendedColumnListViewControl listViewControl)
         {
            (ParentListView ?? throw new Crash()).CtrlListView.ColumnWidthChanged -= CtrlListView_ColumnWidthChanged;

            if (myEmbeddedControlPermanent != null) { myEmbeddedControlPermanent.Parent = null; }

            if (ListViewSubItem != null)
            {
               ListViewSubItem.Text = "";
               ListViewSubItem.Tag = null;
               myListViewSubItem = null;
            }

            myRow = null;
            ParentListView = null;
         }

         [Browsable(false)]
         public EditorType Editor { get => myEditor ?? Column?.ColumnEditor ?? new EditorType.Default(); set => myEditor = value; }

         public void Edit(string? text2Edit = null) => Editor.Edit(this, text2Edit);

         /// <summary>
         /// Temporary adds a control as sub-control in cell client area.
         /// </summary>
         /// <param name="control"></param>
         public void EmbedTempControl(Control control)
         {
            EmbeddedControlTemp = control;
            ParentListView?.Controls.Add(control);
            control.Visible = true;
            control.Bounds = myGetSubItemBounds(ListViewItem, ListViewSubItem ?? throw new Crash());
            control.Leave += Control_Leave;
            control.BringToFront();
            control.Focus();
            control.KeyDown += Control_KeyDown;
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="sender"></param>
         /// <param name="e"></param>
         private void Control_Leave(object? sender, System.EventArgs e) => UnembedTempControl();

         /// <summary>
         /// Unembed temp control remove from sub-controls and remove all event callbacks.
         /// </summary>
         public void UnembedTempControl()
         {
            var emb_cnt = EmbeddedControlTemp;

            if (emb_cnt != null)
            {
               EmbeddedControlTemp = null;
               emb_cnt.Visible = false;
               emb_cnt.Controls.Remove(EmbeddedControlTemp);
               emb_cnt.KeyDown -= Control_KeyDown;
               emb_cnt.Leave -= Control_Leave;
               OnUnembed?.Invoke(this);
            }
         }

         public Control? EmbeddedControlPermanent
         {
            get => myEmbeddedControlPermanent;

            set
            {
               if (myEmbeddedControlPermanent != value)
               {
                  if (myEmbeddedControlPermanent != null)
                  {
                     (ParentListView ?? throw new Crash()).Controls.Remove(myEmbeddedControlPermanent);
                     myEmbeddedControlPermanent.GotFocus -= Value_GotFocus;
                     myEmbeddedControlPermanent.LostFocus -= Value_LostFocus;
                  }

                  if (value != null)
                  {
                     (ParentListView ?? throw new Crash()).Controls.Add(value);
                     value.BringToFront();
                     value.Bounds = myGetSubItemBounds(ListViewItem, ListViewSubItem.NnOrCrash());
                     value.BackColor = BackColor ?? ParentListView.BackColor;
                     value.ForeColor = ParentListView.ForeColor;
                     value.GotFocus += Value_GotFocus;
                     value.LostFocus += Value_LostFocus;
                  }

                  myEmbeddedControlPermanent = value;
               }
            }
         }

         /// <summary>
         /// 
         /// </summary>
         public Control? EmbeddedControlTemp { get; private set; }

         private void Value_LostFocus(object? sender, System.EventArgs e) =>
            (sender as Control ?? throw new Crash()).BackColor = BackColor ?? ParentListView?.BackColor ?? Color.Empty;

         private void Value_GotFocus(object? sender, System.EventArgs e)
         {
            (ParentListView ?? throw new Crash()).PpSelectedCell = this;
            (sender as Control ?? throw new Crash()).BackColor = ParentListView.PpSelectionColor;
         }

         private void Control_KeyDown(object? sender, KeyEventArgs e)
         {
            if (e.KeyCode == Keys.Escape) { UnembedTempControl(); }
         }

         private void CtrlListView_ColumnWidthChanged(object? sender, ColumnWidthChangedEventArgs e)
         {
            if (myEmbeddedControlPermanent != null && ListViewItem != null)
            {
               myEmbeddedControlPermanent.Bounds = myGetSubItemBounds(ListViewItem, ListViewSubItem ?? throw new Crash());
            }
         }
      }
   }
}
