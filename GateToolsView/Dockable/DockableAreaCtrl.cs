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

      private readonly InnerSlotRow.Root myRowRoot;
      private Control? myControlUnderWork = null;

      /// <summary>
      /// Constructor.
      /// </summary>
      public DockableAreaCtrl()
      {
         myRowRoot = new InnerSlotRow.Root(this, DockableCtrlRowDirectionEnum.left_2_right);

         InitializeComponent();

         myRowRoot.FlushChanges();
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
      /// Orientation of the center docked controls. Setting has no effect when more than a center control is docked.
      /// </summary>
      public DockableCtrlRowDirectionEnum PpCenterDirectionToSet { get; set; } = DockableCtrlRowDirectionEnum.left_2_right;

      /// <summary>
      /// Effective direction, property has value if more than one center slot has created.
      /// </summary>
      public DockableCtrlRowDirectionEnum? PpCenterDirectionEffective =>
         PpRowRoot.AllRows.Last().SlotsAnyCenter.Length >= 2 ? PpRowRoot.AllRows.Last().Direction : null;

      /// <summary>
      /// Array with all docked controls.
      /// </summary>
      public Control[] PpControlsDocked =>
         PpRowRoot.SlotsAllControlUser.Select(s => s.FramedControl).Nn().ToArray() ?? [];

      /// <summary>
      /// Left-docked controls.
      /// </summary>
      public Control[] PpControlsDockedLeft =>
         PpRowRoot.SlotsAllControlUser.
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.left).
         Select(s => s.FramedControl).
         Nn().
         ToArray();

      /// <summary>
      ///  right-docked controls.
      /// </summary>
      public Control[] PpControlsDockedRight =>
         PpRowRoot.SlotsAllControlUser.
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.right).
         Select(s => s.FramedControl).
         Nn().
         ToArray();

      /// <summary>
      ///  up-docked controls.
      /// </summary>
      public Control[] PpControlsDockedUp =>
         PpRowRoot.SlotsAllControlUser.
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.up).
         Select(s => s.FramedControl).
         Nn().
         ToArray();

      /// <summary>
      /// Down-docked controls.
      /// </summary>
      public Control[] PpControlsDockedDown =>
         PpRowRoot.SlotsAllControlUser.
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.down).
         Select(s => s.FramedControl).
         Nn().
         ToArray();

      /// <summary>
      /// Center-docked controls.
      /// </summary>
      public Control[] PpControlsDockedCenter =>
         PpRowRoot.
         SlotsAllControlUser.
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center).
         Select(s => s.FramedControl).
         Nn().
         ToArray();

      /// <summary>
      /// Minimum size of the contol, which has been st to a fixed value.
      /// </summary>
      public override Size MinimumSize
      {
         get => new Size(MIN_WIDTH, MIN_HEIGHT);
         set => Console.WriteLine("MinimumSize has fixed value, setting has not effect!");
      }

      /// <summary>
      /// 
      /// </summary>
      public Rectangle PpCenterRectangleScreen
      {
         get
         {
            var lst_slt = PpRowRoot.AllRows.LastOrDefault();
            var cnt_sls = lst_slt?.SlotsAnyCenter;
            var frs_sp1 = cnt_sls?.FirstOrDefault()?.SplitContainer.Panel1;
            var lst_sp1 = cnt_sls?.LastOrDefault()?.SplitContainer.Panel1;
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

      public InnerSlotRow.Root PpRowRoot => myRowRoot;

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
      /// <param name="anchorMode">Anchor mode(up,down,left,right,center)</param>
      /// <returns>False if number of already docked control >= PpNumCreatable</returns>
      /// <exception cref="Gate.ToolsView.GateDockException">Control is null or already contained.</exception>
      public bool MthControlDock(Control control, DockableAreaCtrlSlotAnchorModeEnum anchorMode)
      {
         if (control == null) { return false; }

         myControlUnderWork = control;

         if (PpControlsDocked.Contains(control)) { throw new Gate.Tools.ToolsException("Control is already contained, remove before!"); }
         else if (PpRowRoot.SlotsAllControlUser.Length + 1 < PpMaxNumDocked)
         {
            var par_frm = control.MthGetParentForm();
            var all_sls = PpRowRoot.SlotsAllControlUser;
            var top_row = PpRowRoot.AllRows.Last();

            if (par_frm != null)
            {
               par_frm.Controls.Remove(control);//removes the control from parent form
               par_frm.Close();
            }

            PpRowRoot.UpdateCache();

            if (anchorMode == DockableAreaCtrlSlotAnchorModeEnum.center)
            {
               if (top_row.CenterSlot != null) { top_row.ReplaceRowSubWithUser(control); }
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
                  if (PpRowRoot == top_row && PpRowRoot.SlotsUser.Length == 0)
                  {
                     PpRowRoot.SetDirection(
                        new_slt.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.left ||
                        new_slt.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.right ?
                           DockableCtrlRowDirectionEnum.left_2_right : DockableCtrlRowDirectionEnum.up_2_down);
                  }

                  top_row.InsertSlotOnly(new_slt);
               }
            }

            PpRowRoot.FlushChanges();
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
            var slt = PpRowRoot.SlotsAllControlUser.FirstOrDefault(s => s.FramedControl == control);

            myControlUnderWork = control;

            if (slt != null)
            {
               var row = slt.ParentRow;

               if (row?.SlotsAnyCenter.Length == 1 && slt.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center)
               {
                  row.ReplaceCenterWithSubRowSlot(slt);
               }
               else
               {
                  PpRowRoot.UpdateCache();
                  slt?.ParentRow?.RemoveSlot(slt);
                  PpRowRoot.FlushChanges();
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
         var slt = PpRowRoot.SlotsAllControlUser.FirstOrDefault(s => s.FramedControl == control);

         if (slt != null) { return slt.AnchorMode; }
         else { throw new Gate.Tools.ToolsException($"Control '{control.Name}' not belongs to this '{GetType().Name}'"); }
      }

      public void MthControlReplace(Control oldControl, Control? newControl)
      {
         var slo =
            PpRowRoot.
            SlotsAllControlUser.
            FirstOrDefault(
               s => ReferenceEquals(s.FramedControl, oldControl)) ??
               throw new Gate.Tools.ToolsException(
                  $"Control {oldControl.Name}({oldControl.GetType().Name}) not existing!");

         myControlUnderWork = oldControl;//avoid slot remove from hierarchy
         slo.FramedControl = null;
         myControlUnderWork = newControl;//avoid slot remove from hierarchy
         slo.FramedControl = newControl;
         PpRowRoot.FlushChanges();
      }

      private void Control_VisibleChanged(object? sender, EventArgs e)
      {
         var ctr = sender.ConvertOrCrash<Control>();

         //avoid spurious deletion
         if (!ctr.Visible && ctr != myControlUnderWork) { MthControlUndock(ctr); }
      }
   }
}

