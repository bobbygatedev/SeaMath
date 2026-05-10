using Gate.Tools;
using Gate.Tools.Extensions;
using static Gate.ToolsView.Extended.ExtendedColumnListViewControl;

namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// 
   /// </summary>
   public partial class CmdManagerMenuListControl : UserControl
   {
      public delegate void OnMenuChangedHandler(object? sender, CmdMenu? cmdMenu, CmdMenu.Ref? cmdMenuRef);

      public event OnMenuChangedHandler? OnMenuChanged;

      public CmdManagerMenuListControl()
      {
         InitializeComponent();
      }

      public CmdMainMenu? PpCmdMainMenu { get; private set; }

      public CmdContainer? PpCmdContainer { get; private set; }

      public CmdMenu? PpSelectedMenu { get; private set; }

      public CmdMenu.Ref? PpSelectedMenuRef { get; private set; }

      public int PpSelectedIndex => CtrlListView.PpSelectedRowIdx;

      public void MthUseForMainMenu(CmdMainMenu cmdMainMenu)
      {
         CtrlListView.PpRows = [];
         PpCmdContainer = cmdMainMenu.CmdContainer;
         PpCmdMainMenu = cmdMainMenu;

         foreach (var men_ref in PpCmdMainMenu.MenuRefs) { myDoInsertRef(men_ref); }

         //selects first item
         CtrlListView.PpSelectedRowIdx = Math.Max(0, CtrlListView.PpRows.Length - 1);
      }

      public void MthUseForContextMenu(CmdContainer? cmdContainer)
      {
         CtrlListView.PpRows = [];
         PpCmdContainer = cmdContainer;
         PpCmdMainMenu = null;
         PpSelectedMenuRef = null;

         foreach (var men in PpCmdContainer?.AllMenus ?? []) { myDoInsertMenu(men); }

         if (PpCmdContainer?.AllMenus.Count > 0) { CtrlListView.PpSelectedRowIdx = 0; }
      }

      public void MthDoClear()
      {
         CtrlListView.PpRows = [];
         PpCmdMainMenu = null;
         PpCmdMainMenu = null;
         PpCmdContainer = null;
      }

      protected override void OnResize(EventArgs e)
      {
         base.OnResize(e);

         CtrlListView.PpColumns[0].Width = CtrlListView.Width;
      }

      private void myDoInsertMenu(CmdMenu menu, int atIndex = -1)
      {
         var cap = $"{menu.Id}";
         var row = CtrlListView.MthRowInsert(atIndex);

         row.Cells[0].Text = cap;
         row.Tag = menu;
      }

      private void myDoInsertRef(CmdMenu.Ref menuRef, int atIndex = -1)
      {
         var row = CtrlListView.MthRowInsert(atIndex);

         row.Cells[0].Text = myDoGetMenuRefText(menuRef);
         row.Tag = menuRef;
      }

      private static string myDoGetMenuRefText(CmdMenu.Ref menuRef) => $"{menuRef.Caption}({menuRef.CmdMenu.Id})";

      private void myDoAddNewMenu()
      {
         var new_dum_nam = myGetNewDummyId(PpCmdContainer, out string men_cap);
         var sel_idx = PpSelectedIndex;

         if (PpCmdMainMenu != null)
         {
            var cmd_men = new CmdMenu(false, new_dum_nam);
            var cmd_men_ref = new CmdMenu.Ref(cmd_men, false);

            cmd_men_ref.Caption = men_cap;
            myDoInsertRef(cmd_men_ref, sel_idx);
            PpCmdMainMenu.InsertMenuRef(PpSelectedIndex >= 0 ? PpSelectedIndex : PpCmdMainMenu?.MenuRefs.Length ?? 0, cmd_men_ref);
            PpCmdContainer?.AllMenus.Add(cmd_men);
         }
         else
         {
            var cmd_men = new CmdMenu(false, new_dum_nam);

            myDoInsertMenu(cmd_men);
            PpCmdContainer?.AllMenus.Add(cmd_men);
            PpCmdContainer?.AllMenus.Add(cmd_men);
         }
      }

      private string myGetNewDummyId(CmdContainer? cmdContainer, out string refCaption)
      {
         int idx;
         var men_ids = cmdContainer?.AllMenus.Select(m => m.Id).ToArray() ?? [];

         for (idx = 1; men_ids.Any(m => m == $"NewMenu.Id.{idx}"); idx++) { }

         refCaption = $"NewMenu{idx}";

         return $"NewMenu.Id.{idx}";
      }

      private void myDoDelete()
      {
         if (PpCmdMainMenu != null)
         {
            var sel_men_ref = PpSelectedMenuRef;

            if (sel_men_ref != null && !sel_men_ref.IsDefault)
            {
               var row = CtrlListView.PpRows.FirstOrDefault(i => i.Tag == sel_men_ref);

               if (row != null)
               {
                  PpCmdMainMenu.RemoveMenuRef(sel_men_ref);
                  CtrlListView?.MthRowsRemove(row);
               }
               else { throw new Crash(); }
            }
         }
         else
         {
            var men = PpSelectedMenu;

            if (PpCmdContainer != null)
            {
               var rfs = PpCmdContainer.AllDescendant.OfType<CmdMenu.Ref>().Where(r => r.CmdMenu == men).ToArray();

               if (rfs.Length > 0)
               {
                  var row = CtrlListView.PpRows.FirstOrDefault(i => i.Tag == men);

                  if (row != null)
                  {
                     PpCmdContainer.AllMenus.Remove(row.Tag as CmdMenu ?? throw new Crash());
                     CtrlListView?.MthRowsRemove(row);
                  }
                  else { throw new Crash(); }
               }
               else { MessageBox.Show($"Can't remove menu id={men?.Id} because has {rfs.Length} reference!"); }
            }
            else { throw new Crash(); }
         }
      }

      private void myDoMove(bool isDown)
      {
         var sel_idx = PpSelectedIndex;
         var sel_men_ref = PpSelectedMenuRef;

         if (sel_idx >= 0 && PpCmdMainMenu != null && !(sel_men_ref?.IsDefault ?? false))//main menu mode only
         {
            var is_mov = isDown ? sel_idx < CtrlListView.PpRows.Length - 1 : sel_idx > 0;

            if (is_mov)
            {
               var row = CtrlListView.PpRows.ElementAtOrDefault(sel_idx) ?? throw new Crash();
               var new_idx = isDown ? sel_idx + 1 : sel_idx - 1;
               var men_ref = PpCmdMainMenu.MenuRefs[sel_idx];

               CtrlListView.MthRowsRemove(row);
               CtrlListView.MthRowInsert(row, new_idx);
               PpCmdMainMenu.RemoveMenuRef(men_ref);
               PpCmdMainMenu.InsertMenuRef(new_idx, men_ref);
            }
         }
      }

      private void CtrlCmdBar_OnCmd(Button sender, CmdManagerCmdBar.Cmd cmd)
      {
         switch (cmd)
         {
            case CmdManagerCmdBar.Cmd.up:
               myDoMove(false);
               break;

            case CmdManagerCmdBar.Cmd.down:
               myDoMove(true);
               break;

            case CmdManagerCmdBar.Cmd.delete:
               myDoDelete();
               break;

            case CmdManagerCmdBar.Cmd.add:
               myDoAddNewMenu();
               break;

            default: throw new Crash();
         }
      }

      private void CtrlListView_OnRowSelected(object? sender, RowType row)
      {
         if (row == null) { OnMenuChanged?.Invoke(this, null, null); }
         else if (PpCmdMainMenu != null)
         {
            PpSelectedMenuRef = row.Tag as CmdMenu.Ref ?? throw new Crash();
            PpSelectedMenu = PpSelectedMenuRef.CmdMenu;
            OnMenuChanged?.Invoke(this, PpSelectedMenu, PpSelectedMenuRef);
         }
         else
         {
            PpSelectedMenu = row.Tag as CmdMenu ?? throw new Crash();
            OnMenuChanged?.Invoke(this, PpSelectedMenu, null);
         }
      }

      private void CtrlListView_OnCellDoubleClick(CellType cell)
      {
         var cmd_men_ref = PpSelectedMenuRef;

         if (cmd_men_ref != null)
         {
            cell.Edit(cmd_men_ref.Caption);
         }
      }

      private void CtrlListView_OnCellUpdateText(CellType cell, UpdateCellTextArgs args)
      {
         if (args.Text2Set.IsBlank())
         {
            MessageBox.Show($"Empty caption not acceptable!");

            args.IsAccept = false;
         }
         else
         {
            var cmd_men_ref = PpSelectedMenuRef ?? throw new Crash();

            cmd_men_ref.Caption = args.Text2Set;
            args.Text2Set = myDoGetMenuRefText(cmd_men_ref);
         }
      }
   }
}
