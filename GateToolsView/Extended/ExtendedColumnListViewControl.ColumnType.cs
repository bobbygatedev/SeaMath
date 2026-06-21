using Gate.ToolsView.Extensions;
using System.ComponentModel;

namespace Gate.ToolsView.Extended
{
   public partial class ExtendedColumnListViewControl
   {
      public class ColumnType : BaseType
      {
         public event OnCellTextUpdatingHandler? OnCellTextUpdating;
         public event OnCellTextUpdatedHandler? OnCellTextUpdated;
         public event OnCellEventHandler? OnCellDoubleClick;
         public event OnCellEventHandler? OnCellClick;

         private PaintMethodHandler? myColumnPaintMethod;
         private ImageModeFlags myColumnImageMode = ImageModeFlags.none;
         private Image? myColumnImage;
         private HorizontalAlignment myTextAlingment = HorizontalAlignment.Left;
         private HorizontalAlignment myTextAlignmentHeader;
         private string myText = "";
         private int? myFixedWidth;

         /// <summary>
         /// Default Constructor (not erase it causes error in WinForm Designer if you defines other constructor). 
         /// </summary>
         public ColumnType() => Text = "";

         /// <summary>
         /// Constructor.
         /// </summary>
         /// <param name="text"></param>
         public ColumnType(string? text) => Text = text ?? "";

         /// <summary>
         /// 
         /// </summary>
         [Browsable(false)]
         public ColumnHeader ColumnHeader { get; private set; } = new ColumnHeader();

         /// <summary>
         /// 
         /// </summary>
         public Color? BackColorHeader { get; set; }

         /// <summary>
         /// 
         /// </summary>
         public Color? BackColorHeaderEffective => BackColorHeader ?? ListViewControl?.PpColumnHeaderBackColor;

         /// <summary>
         /// 
         /// </summary>
         public CellAutoEditMode AutoEdit { get; set; } = CellAutoEditMode.none;

         /// <summary>
         /// 
         /// </summary>
         public HorizontalAlignment TextAlignmentHeader
         {
            get => myTextAlignmentHeader;
            set
            {
               myTextAlignmentHeader = value;
               ListViewControl?.Refresh();
            }
         }

         /// <summary>
         /// 
         /// </summary>
         public HorizontalAlignment TextAlignment
         {
            get => myTextAlingment;

            set
            {
               myTextAlingment = value;
               ListViewControl?.CtrlListView.Refresh();
            }
         }

         /// <summary>
         /// 
         /// </summary>
         public string Text
         {
            get => myText;
            set
            {
               myText = value;
               ListViewControl?.Refresh();
            }
         }

         /// <summary>
         /// 
         /// </summary>
         [Browsable(false)]
         public EditorType? ColumnEditor { get; set; }

         /// <summary>
         /// 
         /// </summary>
         [Browsable(false)]
         public ExtendedColumnListViewControl? ListViewControl => (myGetParentItem() as InnerItemContainer)?.Parent;

         /// <summary>
         /// 
         /// </summary>
         [Browsable(false)]
         public PaintMethodHandler? ColumnPaintMethod
         {
            get => myColumnPaintMethod;
            set
            {
               myColumnPaintMethod = value;
               ListViewControl?.Refresh();
            }
         }

         /// <summary>
         /// 
         /// </summary>
         public ImageModeFlags ColumnImageMode
         {
            get => myColumnImageMode;
            set
            {
               myColumnImageMode = value;
               ListViewControl?.Refresh();
            }
         }

         /// <summary>
         /// 
         /// </summary>
         public Image? ColumnImage
         {
            get => myColumnImage;
            set
            {
               myColumnImage = value;
               ListViewControl?.Refresh();
            }
         }

         /// <summary>
         /// 
         /// </summary>
         public int Width { get => ColumnHeader.Width; set => ColumnHeader.Width = value; }

         /// <summary>
         /// 
         /// </summary>
         [Browsable(false)]
         public object? Tag { get; set; }

         /// <summary>
         /// 
         /// </summary>
         public int? FixedWidth
         {
            get => myFixedWidth;
            set
            {
               myFixedWidth = value;

               if (myFixedWidth.HasValue) { myDoSetFixedWidth(); }
            }
         }

         /// <summary>
         /// 
         /// </summary>
         public int Index => ColumnHeader.Index;

         /// <summary>
         /// 
         /// </summary>
         public Color? BackColor { get; set; }

         /// <summary>
         /// 
         /// </summary>
         public Color? HeaderGridColorEffective => HeaderGridColor ?? ListViewControl?.PpGridColor;

         /// <summary>
         /// 
         /// </summary>
         public Color? HeaderGridColor { get; set; }

         /// <summary>
         /// 
         /// </summary>
         public float? GridWithHeaderEffective => GridWithHeader ?? ListViewControl?.PpGridWidth;

         /// <summary>
         /// 
         /// </summary>
         public float? GridWithHeader { get; set; }

         /// <summary>
         /// If true reordering is performed whenever column is double-clicked.
         /// </summary>
         public bool IsRowReorderingActive { get; set; } = true;

         /// <summary>
         /// Reordering phase (true for up) change whenever column is double clicked.
         /// </summary>
         internal bool RowReorderingUpDown { get; set; } = false;

         /// <summary>
         /// Search for biggest text width, then resize in order to view it all.
         /// </summary>
         public void TextWidthFit()
         {
            if (ListViewControl != null)
            {
               var gr = Graphics.FromHwnd(ListViewControl.CtrlListView.Handle);
               var tw = gr.MeasureString(Text, ListViewControl.Font).Width;

               foreach (var row in ListViewControl.PpRows ?? [])
               {
                  var cel = row.Cells[Index];
                  var rtw = gr.MeasureString(cel.Text, ListViewControl.Font).Width;

                  tw = Math.Max(tw, rtw);
               }

               Width = (int)Math.Ceiling(tw) + 2;
            }
         }

         protected override void myActionOnListViewSet(ExtendedColumnListViewControl listViewControl)
         {
            listViewControl.MthInvoke(() =>
            {
               var idx = listViewControl.myItemContainer.Columns.ToList().IndexOf(this);

               listViewControl.CtrlListView.Columns.Insert(idx, ColumnHeader);
               listViewControl.OnCellTextUpdating += ListViewControl_OnCellTextUpdating;
               listViewControl.OnCellTextUpdated += ListViewControl_OnCellTextUpdated;
               listViewControl.OnCellClick += ListViewControl_OnCellClick;
               listViewControl.OnCellDoubleClick += ListViewControl_OnCellDoubleClick;
               listViewControl.CtrlListView.ColumnWidthChanged += CtrlListView_ColumnWidthChanged;
            });
         }

         protected override void myActionOnListViewReset(ExtendedColumnListViewControl listViewControl)
         {

            listViewControl.MthInvoke (() =>
            {
               listViewControl.CtrlListView.Columns.Remove(ColumnHeader);
               listViewControl.OnCellTextUpdating -= ListViewControl_OnCellTextUpdating;
               listViewControl.OnCellTextUpdated -= ListViewControl_OnCellTextUpdated;
               listViewControl.OnCellClick -= ListViewControl_OnCellClick;
               listViewControl.OnCellDoubleClick -= ListViewControl_OnCellDoubleClick;
               listViewControl.CtrlListView.ColumnWidthChanged -= CtrlListView_ColumnWidthChanged;
            });
         }

         private void CtrlListView_ColumnWidthChanged(object? sender, ColumnWidthChangedEventArgs e)
         {
            if (e.ColumnIndex == ColumnHeader.Index) { myDoSetFixedWidth(); }
         }

         private void myDoSetFixedWidth()
         {
            if (ListViewControl != null)
            {
               if (FixedWidth.HasValue && Index >= 0 && FixedWidth != ListViewControl.CtrlListView.Columns[Index].Width)
               {
                  ListViewControl.CtrlListView.Columns[Index].Width = FixedWidth.Value;
               }
            }
         }

         private void ListViewControl_OnCellTextUpdating(CellType cell, UpdateCellTextArgs args)
         {
            if (cell.ColIdx != -1 && cell.Column == this) { OnCellTextUpdating?.Invoke(cell, args); }
         }

         private void ListViewControl_OnCellTextUpdated(CellType cell, string newText)
         {
            if (cell.ColIdx != -1 && cell.Column == this) { OnCellTextUpdated?.Invoke(cell, newText); }
         }

         private void ListViewControl_OnCellDoubleClick(CellType cell)
         {
            if (cell.Column == this) { OnCellDoubleClick?.Invoke(cell); }
         }

         private void ListViewControl_OnCellClick(CellType cell)
         {
            if (cell.Column == this) { OnCellClick?.Invoke(cell); }
         }
      }
   }
}
