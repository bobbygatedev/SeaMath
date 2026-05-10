using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.Extensions;

namespace Gate.ToolsView.Dockable
{
   /// <summary>
   /// 
   /// </summary>
   public partial class DockableAreaCtrl : UserControl
   {
      private const int SPLITTER_WIDTH = 6;
      private const int MIN_WIDTH = 100;
      private const int MIN_HEIGHT = 100;
      private const int MIN_CENTER_WIDTH = 20;
      private const int MIN_CENTER_HEIGHT = 20;

      private readonly InnerSlotRow.Root mySlotRowRoot;
      private Control? myControlUnderWork = null;

      /// <summary>
      /// Constructor.
      /// </summary>
      public DockableAreaCtrl()
      {
         mySlotRowRoot = new InnerSlotRow.Root(this, DockableCtrlRowDirectionEnum.left_2_right);

         InitializeComponent();

         mySlotRowRoot.FlushChanges();
      }

      /// <summary>
      /// Number of max dockable controls.
      /// </summary>
      public int PpMaxNumDocked
      {
         get
         {
            var max_w = (MIN_WIDTH - MIN_CENTER_WIDTH) / SPLITTER_WIDTH;
            var max_h = (MIN_HEIGHT - MIN_CENTER_HEIGHT) / SPLITTER_WIDTH;

            return Math.Min(max_w, max_h);
         }
      }

      /// <summary>
      ///  Orientation of the center docked controls. Setting has no effect when more than a center control is docked.
      /// </summary>
      public DockableCtrlRowDirectionEnum PpCenterDirectionToSet { get; set; } = DockableCtrlRowDirectionEnum.left_2_right;

      /// <summary>
      /// Effective direction, property has value if more than one center slot has created.
      /// </summary>
      public DockableCtrlRowDirectionEnum? PpCenterDirectionEffective => mySlotRowRoot.AllRows.Last().SlotsAnyCenter.Length >= 2 ? (DockableCtrlRowDirectionEnum?)mySlotRowRoot.AllRows.Last().Direction : null;

      /// <summary>
      /// Array with all docked controls.
      /// </summary>
      public Control[] PpControlsDocked => mySlotRowRoot.SlotsAllControlUser.Select(s => s.UserControl).Nn().ToArray() ?? [];

      /// <summary>
      /// Left-docked controls.
      /// </summary>
      public Control[] PpControlsDockedLeft =>
         mySlotRowRoot.SlotsAllControlUser.
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.left).
         Select(s => s.UserControl).
         Nn().
         ToArray();

      /// <summary>
      ///  right-docked controls.
      /// </summary>
      public Control[] PpControlsDockedRight =>
         mySlotRowRoot.SlotsAllControlUser.
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.right).
         Select(s => s.UserControl).
         Nn().
         ToArray();

      /// <summary>
      ///  up-docked controls.
      /// </summary>
      public Control[] PpControlsDockedUp =>
         mySlotRowRoot.SlotsAllControlUser.
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.up).
         Select(s => s.UserControl).
         Nn().
         ToArray();

      /// <summary>
      /// Down-docked controls.
      /// </summary>
      public Control[] PpControlsDockedDown =>
         mySlotRowRoot.SlotsAllControlUser.
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.down).
         Select(s => s.UserControl).
         Nn().
         ToArray();

      /// <summary>
      /// Center-docked controls.
      /// </summary>
      public Control[] PpControlsDockedCenter =>
         mySlotRowRoot.
         SlotsAllControlUser.
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center).
         Select(s => s.UserControl).
         Nn().
         ToArray();

      /// <summary>
      /// Minimum size of the contol, which has been st to a fixed value.
      /// </summary>
      public override Size MinimumSize { get => new Size(MIN_WIDTH, MIN_HEIGHT); set => Console.WriteLine("MinimumSize has fixed value, setting has not effect!"); }

      /// <summary>
      /// 
      /// </summary>
      public Rectangle PpCenterRectangleScreen
      {
         get
         {
            var lst_slt = mySlotRowRoot.AllRows.LastOrDefault();
            var cnt_sls = lst_slt?.SlotsAnyCenter;
            var frs_sp1 = cnt_sls?.FirstOrDefault()?.SplitCont.Panel1;
            var lst_sp1 = cnt_sls?.LastOrDefault()?.SplitCont.Panel1;
            var frs_rc_scr = frs_sp1?.Parent?.RectangleToScreen(frs_sp1.ClientRectangle);
            var lst_rc_scr = lst_sp1?.Parent?.RectangleToScreen(lst_sp1.ClientRectangle);

            if (frs_rc_scr.HasValue && lst_rc_scr.HasValue)
            {
               return Rectangle.FromLTRB(
                  frs_rc_scr.Value.Left, frs_rc_scr.Value.Top, lst_rc_scr.Value.Right, lst_rc_scr.Value.Bottom);
            }
            else
            {
               return Rectangle.Empty;
            }
         }
      }

      /// <summary>
      /// Adds an array of docked control to me. 
      /// </summary>
      /// <param name="anchorMode"></param>
      /// <param name="controls"></param>
      /// <returns>Number of added controls.</returns>
      public int MthControlsDock(DockableAreaCtrlSlotAnchorModeEnum anchorMode, params Control[] controls) => controls.Count(c => MthControlDock(c, anchorMode));

      /// <summary>
      /// Adds a docked control to me. 
      /// </summary>
      /// <param name="control">Control to dock.</param>
      /// <param name="anchorMode">Anchor mode(up,down,left,roght,center)</param>
      /// <returns>False if number of already docked control >= PpNumCreatable</returns>
      /// <exception cref="Gate.ToolsView.GateDockException">Control is null or already contained.</exception>
      public bool MthControlDock(Control control, DockableAreaCtrlSlotAnchorModeEnum anchorMode)
      {
         if (control == null) { return false; }

         myControlUnderWork = control;

         if (PpControlsDocked.Contains(control)) { throw new Gate.Tools.ToolsException("Control is already contained, remove before!"); }
         else if (mySlotRowRoot.SlotsAllControlUser.Length + 1 < PpMaxNumDocked)
         {
            var par_frm = control.MthGetParentForm();
            var all_sls = mySlotRowRoot.SlotsAllControlUser;
            var top_row = mySlotRowRoot.AllRows.Last();

            if (par_frm != null)
            {
               par_frm.Controls.Remove(control);//removes the control from parent form
               par_frm.Close();
            }

            mySlotRowRoot.UpdateCache();

            if (anchorMode == DockableAreaCtrlSlotAnchorModeEnum.center)
            {
               if (top_row.RowSubSlot != null) { top_row.ReplaceRowSubWithUser(control); }
               else { top_row.InsertSlotOnly(new InnerSlot.User(control, anchorMode, control.Size)); }
            }
            else
            {
               var new_slt = new InnerSlot.User(control, anchorMode, control.Size);

               //if any slot has same anchor of an left/right/up/down new slot it is inserted at interior of that row
               if (all_sls.Any(s => s.AnchorMode == anchorMode))
               {
                  all_sls.LastOrDefault(s => s.AnchorMode == anchorMode)?.ParentRow?.InsertSlotOnly(new_slt);
               }
               else
               {
                  if (mySlotRowRoot == top_row && mySlotRowRoot.SlotsUser.Length == 0)
                  {
                     mySlotRowRoot.SetDirection(
                        new_slt.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.left ||
                        new_slt.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.right ?
                           DockableCtrlRowDirectionEnum.left_2_right : DockableCtrlRowDirectionEnum.up_2_down);
                  }

                  top_row.InsertSlotOnly(new_slt);
               }
            }

            mySlotRowRoot.FlushChanges();
            control.VisibleChanged += Control_VisibleChanged;
            myControlUnderWork = null;

            return true;
         }

         return false;
      }

      public bool MthControlUndock(Control control)
      {
         if (control != null)
         {
            myControlUnderWork = control;
            var slt = mySlotRowRoot.SlotsAllControlUser.FirstOrDefault(s => s.FramedControl == control);

            if (slt != null)
            {
               var row = slt.ParentRow;

               if (row?.SlotsAnyCenter.Length == 1 && slt.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center)
               {
                  row.ReplaceCenterWithSubRowSlot(slt);
               }
               else
               {
                  mySlotRowRoot.UpdateCache();
                  slt?.ParentRow?.RemoveSlot(slt);
                  mySlotRowRoot.FlushChanges();
               }

               control.VisibleChanged -= Control_VisibleChanged;
               myControlUnderWork = null;
               return true;
            }
         }

         return false;
      }

      public DockableAreaCtrlSlotAnchorModeEnum MthGetAnchorFromControl(Control control)
      {
         var slt = mySlotRowRoot.SlotsAllControlUser.FirstOrDefault(s => s.UserControl == control);

         if (slt != null) { return slt.AnchorMode; }
         else { throw new Gate.Tools.ToolsException($"Control '{control.Name}' not belongs to this '{GetType().Name}'"); }
      }

      private void Control_VisibleChanged(object? sender, EventArgs e)
      {
         var ctr = sender as Control ?? throw new Crash();

         //avoid spurious deletion
         if (!ctr.Visible && ctr != myControlUnderWork) { MthControlUndock(ctr); }
      }
   }
}

