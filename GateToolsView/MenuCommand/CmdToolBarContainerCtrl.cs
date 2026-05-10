using Gate.Tools;
using Gate.Tools.Extensions;
using System.Data;
using System.Windows.Forms.Layout;
using static Gate.ToolsView.MenuCommand.CmdToolBarCtrl;

namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// 
   /// </summary>
   public partial class CmdToolBarContainerCtrl : UserControl
   {
      public const int MIN_ROW_HEIGHT = 20;

      private int myRowHeight = MIN_ROW_HEIGHT;
      private CmdMainMenu? myCmdMainMenu = null;
      private readonly List<InnerRow> myListRows = [new InnerRow()];

      public CmdToolBarContainerCtrl()
      {
         InitializeComponent();
      }

      private class InnerCmdMainMenuControlAssociation : ICmdMainMenuControlAssociation
      {
         public InnerCmdMainMenuControlAssociation(
            CmdToolBarContainerCtrl cmdToolBarContainerCtrl, CmdMainMenu cmdMainMenu)
         {
            CmdToolBarContainerCtrl = cmdToolBarContainerCtrl;
            CmdMainMenu = cmdMainMenu;
         }

         public CmdMainMenu CmdMainMenu { get; }

         public CmdToolBarContainerCtrl CmdToolBarContainerCtrl { get; }

         public void InsertMenu(CmdMenu.Ref menuRef, int atIndex)
         {
            var tol_bar = new CmdToolBarCtrl();

            tol_bar.PpCmdMenuRef = menuRef;
            CmdToolBarContainerCtrl.MthToolBarInsert(tol_bar, atIndex);
         }

         public void RemoveMenu(CmdMenu.Ref menuRef)
         {
            var tol_bar = CmdToolBarContainerCtrl.PpToolBars.FirstOrDefault(t => t.PpCmdMenuRef == menuRef);

            if (tol_bar != null) { CmdToolBarContainerCtrl.MthToolBarRemove(tol_bar); }
         }
      }

      public class ToolBarSlot
      {
         public ToolBarSlot() { }

         public ToolBarSlot(CmdToolBarCtrl toolBar) => ToolBar = toolBar;

         public CmdToolBarCtrl? ToolBar { get; set; } = null;
      }

      private class InnerRow
      {
         private readonly List<CmdToolBarCtrl> myListToolBar = new List<CmdToolBarCtrl>();

         public CmdToolBarCtrl[] ToolBars => myListToolBar.ToArray();

         public void AddToolBar(CmdToolBarCtrl toolBar) { myListToolBar.Add(toolBar); }

         public void RemoveToolBar(CmdToolBarCtrl toolBar) { myListToolBar.Remove(toolBar); }

         public void ToolBarNewIdx(CmdToolBarCtrl toolBar, int newIdx)
         {
            if (newIdx >= myListToolBar.Count)
            {
               myListToolBar.Remove(toolBar);
               myListToolBar.Add(toolBar);
            }
            else
            {
               myListToolBar.Remove(toolBar);
               myListToolBar.Insert(newIdx, toolBar);
            }
         }
      }

      private class InnerLayoutEngine : LayoutEngine
      {
         public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
         {
            var par = (CmdToolBarContainerCtrl)container;
            var y = 0;

            par.Height = par.myRowHeight * par.myListRows.Count;

            foreach (var row in par.myListRows)
            {
               var x = 0;

               foreach (var tol in row.ToolBars)
               {
                  tol.Top = y;
                  tol.Left = x;
                  tol.Height = par.myRowHeight;
                  tol.PerformLayout();
                  x = tol.Right;
               }

               y += par.PpRowHeight;
            }

            return false;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public CmdMainMenu? PpCmdMainMenu
      {
         get => myCmdMainMenu;
         set
         {
            if (myCmdMainMenu != null) { myCmdMainMenu.ControlAssociation = null; }

            if ((myCmdMainMenu = value) != null)
            {
               myCmdMainMenu.ControlAssociation = new InnerCmdMainMenuControlAssociation(this, myCmdMainMenu);
            }
         }
      }

      /// <summary>
      ///  tool bar slots (to be used in design) mode
      /// </summary>
      public ToolBarSlot[] PpToolBarSlots
      {
         get => PpToolBars.Select(t => new ToolBarSlot(t)).ToArray();

         set => PpToolBars = (value ?? []).Select(t => t.ToolBar).Nn().ToArray();
      }

      /// <summary>
      /// 
      /// </summary>
      public CmdToolBarCtrl[] PpToolBars
      {
         get => Controls.OfType<CmdToolBarCtrl>().ToArray();

         set
         {
            foreach (var ctr in PpToolBars)
            {
               Controls.Remove(ctr);
               myListRows.Clear();
               myListRows.Add(new InnerRow());
            }

            if (value != null)
            {
               Controls.AddRange(value);

               foreach (var too in value) { myListRows[0].AddToolBar(too); }
            }

            PerformLayout();
            Refresh();
         }
      }

      public int PpNumRows => myListRows.Count;

      public int PpRowHeight
      {
         get => myRowHeight;

         set
         {
            value = Math.Max(MIN_ROW_HEIGHT, value);

            myRowHeight = value;
            Refresh();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override LayoutEngine LayoutEngine => new InnerLayoutEngine();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="toolBar"></param>
      public void MthToolBarAdd(CmdToolBarCtrl toolBar) => MthToolBarInsert(toolBar, -1);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="toolBar"></param>
      /// <param name="atIndex">Index where the toolbar is placed (-1) at end.</param>
      /// <exception cref="NotImplementedException"></exception>
      public void MthToolBarInsert(CmdToolBarCtrl toolBar, int atIndex = -1)
      {
         if (!Controls.Contains(toolBar))
         {
            Controls.Add(toolBar);

            if (atIndex != -1) { Controls.SetChildIndex(toolBar, atIndex); }

            toolBar.OnGripMove += ToolBar_OnGripMove;
            myListRows.Last().AddToolBar(toolBar);
            Refresh();
         }
      }


      private void ToolBar_OnGripMove(object? sender, MoveArgs moveArgs)
      {
         var tol_bar = sender as CmdToolBarCtrl??throw new Crash();
         var sta_row = myListRows.First(r => r.ToolBars.Contains(tol_bar));
         var sta_row_id = myListRows.IndexOf(sta_row);
         var sta_col_idx = sta_row.ToolBars.ToList().IndexOf(tol_bar);
         var end_row_idx = moveArgs.CmdToolBarRelativeLocation.Y / PpRowHeight;

         if (Controls.Count <= 1) { return; }
         else if (end_row_idx == sta_row_id)
         {
            var col_idx = my_GetColIdx(moveArgs, sta_row);

            //in col range and != old_col
            if (col_idx >= 0 && col_idx != sta_col_idx)
            {
               sta_row.ToolBarNewIdx(tol_bar, col_idx);
               PerformLayout();
            }
         }
         else if (end_row_idx == myListRows.Count)//creating new line
         {
            var new_row = new InnerRow();

            new_row.AddToolBar(tol_bar);
            sta_row.RemoveToolBar(tol_bar);
            myListRows.Add(new_row);

            if (sta_row.ToolBars.Length == 0)
            {
               myListRows.Remove(sta_row);
            }

            myDoRemoveEmptyRows();
            PerformLayout();
         }
         else if (end_row_idx >= 0 && end_row_idx < myListRows.Count)
         {
            var end_row = myListRows[end_row_idx];
            var col_idx = my_GetColIdx(moveArgs, end_row);

            if (col_idx >= 0)
            {
               sta_row.RemoveToolBar(tol_bar);
               end_row.ToolBarNewIdx(tol_bar, col_idx);
               myDoRemoveEmptyRows();
               PerformLayout();
            }
         }
      }

      private void myDoRemoveEmptyRows() => myListRows.RemoveAll(r => r.ToolBars.Length == 0);

      private static int my_GetColIdx(MoveArgs moveArgs, InnerRow row)
      {
         var x = moveArgs.CmdToolBarRelativeLocation.X;

         if (x >= 0)
         {
            for (var col_idx = 0; col_idx < row.ToolBars.Length; col_idx++)
            {
               if (x < row.ToolBars[col_idx].Right) { return col_idx; }
            }

            if (x < row.ToolBars.Last().Right + 50) { return row.ToolBars.Length; }
         }

         return -1;
      }

      public void MthToolBarRemove(CmdToolBarCtrl toolBar)
      {
         if (Controls.Contains(toolBar))
         {
            toolBar.OnGripMove -= ToolBar_OnGripMove;
            Controls.Remove(toolBar);

            var row = myListRows.FirstOrDefault(r => r.ToolBars.Contains(toolBar));

            if (row != null)
            {
               row.RemoveToolBar(toolBar);

               if (row.ToolBars.Length == 0 && myListRows.IndexOf(row) > 0)
               {
                  myListRows.Remove(row);
               }
            }

            Refresh();
         }
      }
   }
}
