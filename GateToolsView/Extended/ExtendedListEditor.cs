using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.Extended;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using static Gate.ToolsView.Extended.ExtendedColumnListViewControl;

namespace Gate.ToolsView.BaseControls
{
   /// <summary>
   /// 
   /// </summary>
   public partial class ExtendedListEditor : UserControl
   {
      public delegate void OnCellHandler(object? sender, CellType? cell);
      public delegate void OnItemHandler(object? sender, IExtendedListEditorItem item, CellType? cell);
      public delegate void OnAskForDeleteHandler(object? sender, AskForDeleteArgs args);

      public event OnItemHandler? OnItemSelected;
      public event OnAskForDeleteHandler? OnAskForDelete;
      public event OnCellHandler? OnAddNew;

      /// <summary>
      /// In case user double clicks on valid item cell or press enter.
      /// </summary>
      public event OnCellHandler? OnUserDblClickOrEnter;

      private const string ADD_NEW_TXT = "(Add new ..)";
      private Type? myItemType;
      private PropertyInfo[]? myPropertiesWithAttribute;
      private bool myIsAddNew;

      public ExtendedListEditor()
      {
         InitializeComponent();

         CtrlList.PpRowReorderMethod = myReorderMethod;
      }

      public class AskForDeleteArgs
      {
         public AskForDeleteArgs(IExtendedListEditorItem ecoaModelItem, CellType cell)
         {
            EcoaModelItem = ecoaModelItem;
            Cell = cell;
         }

         public bool IsToDelete { get; set; } = false;

         public IExtendedListEditorItem EcoaModelItem { get; }

         public CellType Cell { get; }
      }

      public bool PpIsAddNewPresent => CtrlList.PpRows.Any(r => r.Tag == null);

      public bool PpIsAddNew
      {
         get => myIsAddNew;

         set
         {
            if (value != myIsAddNew)
            {
               myIsAddNew = value;
               myDoAddNewCheck();
            }
         }
      }

      public IExtendedListEditorItem[]? PpItems
      {
         get => CtrlList.PpRows.Select(r => r.Tag).OfType<IExtendedListEditorItem>().ToArray();

         set
         {
            if (myItemType != null)
            {
               CtrlList.MthClear(false);

               if (value != null) { myDoAddNewArrayItems(value); }

               MthRefresh();
            }
            else if ((value ?? []).Length > 0)
            {
               throw new Gate.Tools.ToolsException($"Not array type set!");
            }
         }
      }

      [Browsable(false)]
      public IExtendedListEditorItem? PpSelectedItem
      {
         get => CtrlList.PpSelectedCell?.Row?.Tag as IExtendedListEditorItem;

         set
         {
            var row = CtrlList.PpRows.FirstOrDefault(r => r.Tag == value);

            CtrlList.PpSelectedCell = row?.Cells?.FirstOrDefault();
         }
      }

      public Type? PpItemType
      {
         get => myItemType;

         set
         {
            if (myItemType != value)
            {
               myItemType = value;

               if (myItemType == null)
               {
                  CtrlList.MthClear(true);
               }
               else
               {
                  CtrlList.MthClear(true);

                  var prs_all = PpItemType?.GetProperties().ToArray();

                  myPropertiesWithAttribute = prs_all?.Where(p => p.GetCustomAttribute<ExtendedListEditorAttribute>() != null).ToArray();

                  var alw_tps = new[] { typeof(string), typeof(bool), typeof(RelativePath) };

                  //check 
                  var prs_no_str = myPropertiesWithAttribute?.Where(p => !alw_tps.Contains(p.PropertyType)).ToArray();

                  if (prs_no_str?.Length > 0)
                  {
                     throw new Gate.Tools.ToolsException(
                        $"Properties ({string.Join(",", prs_no_str.Select(p => p.Name))}) not valid (not returning string)");
                  }

                  var idx = 0;

                  foreach (var pro in myPropertiesWithAttribute ?? [])
                  {
                     var atr = pro.GetCustomAttribute<ExtendedListEditorAttribute>();

                     var col = CtrlList.MthColAdd(atr?.ShownName ?? pro.Name);
                     var typ = pro.PropertyType;
                     var alt_pro = null as PropertyInfo;
                     var edi_cbb = null as EditorType.ByComboBox;

                     if (atr?.FixedWidth > 0)
                     {
                        col.FixedWidth = atr.FixedWidth;
                     }
                     else if (atr?.MinimumWidth > 0)
                     {
                        col.Width = Math.Max(col.Width, atr.MinimumWidth);
                     }

                     if (pro.PropertyType != typeof(bool))
                     {
                        if (!(atr?.AlternativeProperty.IsBlank() ?? true))
                        {
                           alt_pro = prs_all?.FirstOrDefault(p =>
                             p.Name == atr.AlternativeProperty) ??
                             throw new Crash($"{atr.AlternativeProperty} not found in type {myItemType.FullName}");

                           Func<string[]> alt_get = () =>
                              PpSelectedItem != null ? alt_pro?.GetGetMethod()?.Invoke(PpSelectedItem, null) as string[] ?? [] : [];

                           col.ColumnEditor = edi_cbb = new EditorType.ByComboBox(atr.IsAlternativeStrict, alt_get);
                           col.AutoEdit = CellAutoEditMode.at_dblclick;

                           if (!alt_pro.PropertyType.GetInterfaces().Any(i => i == typeof(IEnumerable))) { throw new Crash(); }
                        }
                        else
                        {
                           if (pro.PropertyType == typeof(string))
                           {
                              col.ColumnEditor = new EditorType.Default();
                           }
                           else if (pro.PropertyType == typeof(RelativePath))
                           {
                              col.ColumnEditor = new FileControlEditor();
                           }
                           else { throw new Crash(); }
                        }
                     }

                     idx++;
                  }

                  myDoAddNewCheck();
               }
            }
         }
      }

      public ExtendedColumnListViewControl PpUnderlyingControl => CtrlList;

      public void MthRefresh()
      {
         if (PpItemType != null)
         {
            var prs_all = PpItemType.GetProperties().ToArray();
            var prs_atr = prs_all.
               Where(p => p.GetCustomAttribute<ExtendedListEditorAttribute>() != null).ToArray();
            var ats = prs_atr.Select(p => p.GetCustomAttribute<ExtendedListEditorAttribute>()).ToArray();

            foreach (var row in CtrlList.PpRows.Where(r => r.Tag is IExtendedListEditorItem))
            {
               var arr_val = row.Tag as IExtendedListEditorItem ?? throw new Crash();

               for (var i = 0; i < prs_atr.Length; i++)
               {
                  var pro = prs_atr[i];

                  if (pro.PropertyType == typeof(string))
                  {
                     var pro_val_str = myGetPropertyString(pro?.GetValue(arr_val) ?? throw new Crash());

                     if (pro_val_str != row.Cells[i].Text)
                     {
                        row.Cells[i].Text = pro_val_str;
                     }
                  }
                  else if (pro.PropertyType == typeof(RelativePath))
                  {
                     var rel_pth = pro.GetValue(arr_val) as RelativePath;

                     row.Cells[i].Tag = rel_pth;

                     if (rel_pth != null)
                     {
                        row.Cells[i].Text = rel_pth.RelativePathLinux;
                     }
                     else
                     {
                        row.Cells[i].Text = "(empty)";
                     }
                  }
                  else if (pro.PropertyType == typeof(bool))
                  {
                     var cb = row.Cells[i].EmbeddedControlPermanent as CheckBox ?? throw new Crash();

                     cb.Checked = pro.GetValue(arr_val).ConvertOrCrash<bool>();
                  }
                  else
                  {
                     throw new Crash();
                  }
               }
            }

            for (int i = 0; i < prs_atr.Length; i++)
            {
               if (ats?[i]?.IsAutoSize ?? false)
               {
                  CtrlList.PpColumns[i].TextWidthFit();
               }
            }
         }

         Refresh();
      }

      public void MthStartEdit(IExtendedListEditorItem item, int colIndex)
      {
         var itm_idx = PpItems?.ToList().IndexOf(item) ?? -1;

         if (itm_idx != -1) { MthStartEdit(itm_idx, colIndex); }
      }

      public void MthStartEdit(int rowIndex, int colIndex)
      {
         if (rowIndex >= 0 && rowIndex < PpItems?.Length && colIndex >= 0 && colIndex < CtrlList.PpColumns.Length)
         {
            CtrlList.PpSelectedCell = CtrlList.PpRows[rowIndex].Cells[colIndex];
            CtrlList.PpRows[rowIndex].Cells[colIndex].Edit();
         }
      }

      public void MthAddNewArrayItems(params IExtendedListEditorItem[] newArrayItems)
      {
         myDoAddNewArrayItems(newArrayItems);
         MthRefresh();
      }

      public bool MthRemoveItem(IExtendedListEditorItem item)
      {
         var row = CtrlList.PpRows.FirstOrDefault(r => r.Tag == item);

         if (row != null)
         {
            CtrlList.MthRowsRemove(row);
            myDoAddNewCheck();

            return true;
         }
         else
         {
            return false;
         }
      }

      protected override void OnHandleCreated(EventArgs e)
      {
         base.OnHandleCreated(e);

         myDoAddNewCheck();
      }

      protected virtual void myActionOnAddNew(object? sender, CellType? cell) => OnAddNew?.Invoke(sender, cell);

      protected virtual void myActionOnAskForDelete(object? sender, AskForDeleteArgs args)
      {
         OnAskForDelete?.Invoke(this, args);

         if (args.IsToDelete)
         {
            CtrlList.MthRowsRemove(args.Cell?.Row);
         }
      }

      private bool myIsCellDropDown(CellType? cell) => cell?.Column?.ColumnEditor is EditorType.ByComboBox;

      /// <summary>
      /// <br> The method `myActionOnItemAction` handles actions on items within the list editor. </br>
      /// <br> It checks if the item is editable and performs the following: </br>
      /// <br> - If the property type is not boolean, it configures a combo box editor with alternative values if applicable. </br>
      /// <br> - If the property type is boolean, it toggles the checkbox state. </br>
      /// <br> This method ensures proper editing behavior based on the item's property type. </br>
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="item"></param>
      /// <param name="cell"></param>
      protected virtual void myActionOnItemAction(object? sender, IExtendedListEditorItem item, CellType? cell)
      {
         if (item != null && item.IsEditable && cell != null)
         {
            if (myPropertiesWithAttribute?[cell.ColIdx].PropertyType != typeof(bool))
            {
               var edi_cbb = cell.Column.ColumnEditor as EditorType.ByComboBox;

               cell.Edit();
            }
            else
            {
               var ch_box = cell.EmbeddedControlPermanent as CheckBox ?? throw new Crash();

               ch_box.Checked = !ch_box.Checked;
            }
         }
      }

      public override Size GetPreferredSize(Size proposedSize) => CtrlList.GetPreferredSize(proposedSize);

      private RowType[] myDoAddNewRows(IExtendedListEditorItem[] newArrayItem)
      {
         var rws = null as RowType[];

         rws = PpIsAddNewPresent ?
            CtrlList.MthRowsInsert((CtrlList.PpRows.Length) - 1, newArrayItem.Length) :
            CtrlList.MthRowsAdd(newArrayItem.Length);

         for (int i = 0; i < newArrayItem.Length; i++)
         {
            rws[i].Tag = newArrayItem[i];
         }

         return rws;
      }

      private void myDoAddNewCheck()
      {
         if (IsHandleCreated && CtrlList.PpColumns.Length > 0 && PpIsAddNew && !PpIsAddNewPresent)
         {
            var new_row = CtrlList.MthRowAdd();

            new_row.Cells[0].Text = ADD_NEW_TXT;
         }
         else if (!PpIsAddNew && PpIsAddNewPresent)
         {
            var rws = CtrlList.PpRows.Where(r => r.Tag == null).ToArray();

            if (rws.Length == 1)
            {
               CtrlList.MthRowsRemove(rws[0]);
            }
            else
            {
               throw new Crash();
            }
         }
      }

      private void myDoAddNewArrayItems(IExtendedListEditorItem[] newArrayItems)
      {
         var rws = myDoAddNewRows(newArrayItems);

         var prs_all = PpItemType?.GetProperties().ToArray();
         var prs_atr = prs_all?.
            Where(p => p.GetCustomAttribute<ExtendedListEditorAttribute>() != null).ToArray();

         for (int i = 0; i < newArrayItems.Length; i++)
         {
            var new_itm = newArrayItems[i];
            var row = rws[i];

            for (var j = 0; j < prs_atr?.Length; j++)
            {
               var pro = prs_atr[j];
               var pro_val = pro.GetValue(new_itm);

               row.Cells[j].Tag = pro_val;

               if (pro_val is bool boo)
               {
                  var ch_box = new CheckBox();

                  ch_box.Checked = boo;
                  ch_box.CheckedChanged += (s, e) => pro.SetValue(new_itm, ch_box.Checked);
                  row.Cells[j].EmbeddedControlPermanent = ch_box;
               }
               else if (pro.PropertyType == typeof(string))
               {
                  row.Cells[j].Text = myGetPropertyString(pro_val);
               }
            }
         }
      }

      private static string? myGetPropertyString(object? pro_val) => pro_val != null ? pro_val?.ToString() : "";

      private RowType[] myReorderMethod(RowType[] rows, ColumnType byColumn, bool isPhaseDown)
      {
         var add_new = rows.FirstOrDefault(r => r.Tag == null);

         if (add_new != null)
         {
            var rws = rows.Except(new[] { add_new }).ToArray();

            return MthReorderDefault(rws, byColumn, isPhaseDown).Append(add_new).ToArray();
         }
         else
         {
            return MthReorderDefault(rows, byColumn, isPhaseDown);
         }
      }

      private void CtrlList_OnCellTextUpdating(CellType cell, UpdateCellTextArgs args)
      {
         var pro = myPropertiesWithAttribute?[cell.ColIdx];

         if (pro?.PropertyType == typeof(RelativePath))
         {
            var rel_pth = cell.Tag as RelativePath ?? throw new Crash();

            pro.SetValue(cell?.Row?.Tag, rel_pth);
         }
         else if (pro?.PropertyType == typeof(string))
         {
            pro.SetValue(cell?.Row?.Tag, args.Text2Set);
            args.Text2Set = pro.GetValue(cell?.Row?.Tag) as string ?? "";
         }
         else
         {
            throw new Crash();
         }
      }

      private void CtrlList_OnCellDoubleClick(CellType cell)
      {
         //this is the case is add_new celll
         if (!(cell.Row.Tag is IExtendedListEditorItem) && cell.Row == cell.ParentListView?.PpRows.LastOrDefault() && PpIsAddNew)
         {
            myActionOnAddNew(this, cell);
         }
         else if (cell.Row.Tag is IExtendedListEditorItem)
         {
            OnUserDblClickOrEnter?.Invoke(this, cell);
         }
      }

      private void CtrlList_OnCellTextUpdated(CellType cell, string newText) => MthRefresh();

      private void CtrlList_OnCellKeyUp(object? sender, CellType cell, KeyEventArgs e)
      {
         if (!e.Alt && !e.Shift && !e.Control)
         {
            var emi = cell.Row.Tag as IExtendedListEditorItem;

            switch (e.KeyData)
            {
               case Keys.F2:
                  if (!myIsCellDropDown(cell) && emi != null)
                  {
                     myActionOnItemAction(this, emi, cell);
                  }
                  break;

               case Keys.Enter:
                  if (emi != null)
                  {
                     myActionOnItemAction(this, emi, cell);
                     OnUserDblClickOrEnter?.Invoke(this, cell);
                  }
                  else
                  {
                     myActionOnAddNew(this, cell);
                  }

                  break;

               case Keys.Delete:
                  var ars = new AskForDeleteArgs(cell.Row.Tag as IExtendedListEditorItem ?? throw new Crash(), cell);

                  myActionOnAskForDelete(this, ars);
                  break;
            }
         }
      }

      private void CtrlList_OnSelectedCellChanged(object? sender, CellType selectedCell) =>
         OnItemSelected?.Invoke(sender, selectedCell.Row?.Tag as IExtendedListEditorItem ?? throw new Crash(), selectedCell);
   }
}
