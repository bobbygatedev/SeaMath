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

      /// <summary>
      /// Constructor.
      /// </summary>
      public DockableAreaCtrl()
      {
         InitializeComponent();
         CtrlAreaRowCtrl.PpDirection = DockableCtrlRowDirectionEnum.left_2_right;
         CtrlAreaRowCtrl.MthControlDock(null, DockableAreaCtrlSlotAnchorModeEnum.center);
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
      public DockableCtrlRowDirectionEnum? PpCenterDirectionEffective => myGetAllRowCtrl().Last().PpDirection;

      /// <summary>
      /// Array with all docked controls.
      /// </summary>
      public Control[] PpControlsDocked =>
         [.. myGetAllRowCtrl().SelectMany(c => c.PpSlots).Select(s => s.Control).Nn()];

      /// <summary>
      /// Left-docked controls.
      /// </summary>
      public Control[] PpControlsDockedLeft =>
         [.. myGetAllRowCtrl().SelectMany(c => c.PpSlots).
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.left).
         Select(s => s.Control).Nn()];

      /// <summary>
      ///  right-docked controls.
      /// </summary>
      public Control[] PpControlsDockedRight =>
         [.. myGetAllRowCtrl().SelectMany(c => c.PpSlots).
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.right).
         Select(s => s.Control).Nn()];

      /// <summary>
      ///  up-docked controls.
      /// </summary>
      public Control[] PpControlsDockedUp =>
         [.. myGetAllRowCtrl().SelectMany(c => c.PpSlots).
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.up).
         Select(s => s.Control).Nn()];

      /// <summary>
      /// Down-docked controls.
      /// </summary>
      public Control[] PpControlsDockedDown =>
         [.. myGetAllRowCtrl().SelectMany(c => c.PpSlots).
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.down).
         Select(s => s.Control).Nn()];

      /// <summary>
      /// Center-docked controls.
      /// </summary>
      public Control[] PpControlsDockedCenter =>
         [.. myGetAllRowCtrl().SelectMany(c => c.PpSlots).
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center).
         Select(s => s.Control).Nn()];

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
            var cur_row_ctr = myGetCurrentRowCtrl();
            var cnt_sls = cur_row_ctr.PpSlotsCenter;
            var frs_sp1 = cnt_sls?.FirstOrDefault()?.Panel;
            var lst_sp1 = cnt_sls?.LastOrDefault()?.Panel;
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
      /// 
      /// </summary>
      public DockAreaRowCtrl.SlotType[] PpSlotsAll =>
         [.. this.MthGetNephews().OfType<DockAreaRowCtrl>().SelectMany(c => c.PpSlots)];

      private DockAreaRowCtrl[] myGetAllRowCtrl() =>
         this.MthGetNephews().OfType<DockAreaRowCtrl>().ToArray();

      private DockAreaRowCtrl myGetCurrentRowCtrl() => this.MthGetNephews().OfType<DockAreaRowCtrl>().LastOrCrash();

      /// <summary>
      /// Adds a docked control to me. 
      /// </summary>
      /// <param name="control">Control to dock.</param>
      /// <param name="anchorMode">Anchor mode(up,down,left,right,center)</param>
      /// <returns>False if number of already docked control >= PpNumCreatable</returns>
      /// <exception cref="Gate.ToolsView.GateDockException">Control is null or already contained.</exception>
      public bool MthControlDock(Control control, DockableAreaCtrlSlotAnchorModeEnum anchorMode)
      {
         //tododo                                         
         if (!PpControlsDocked.ContainsRef(control))
         {
            /// current dock row <see cref="DockAreaRowCtrl"/>  
            var cur_dck_row = myGetCurrentRowCtrl();

            if (PpControlsDocked.Length < PpMaxNumDocked)
            {
               if (PpControlsDocked.Length == 0)
               {
                  switch (anchorMode)
                  {
                     case DockableAreaCtrlSlotAnchorModeEnum.left:
                     case DockableAreaCtrlSlotAnchorModeEnum.right:
                     case DockableAreaCtrlSlotAnchorModeEnum.center:
                        cur_dck_row.PpDirection = DockableCtrlRowDirectionEnum.left_2_right;
                        break;

                     case DockableAreaCtrlSlotAnchorModeEnum.up:
                     case DockableAreaCtrlSlotAnchorModeEnum.down:
                        cur_dck_row.PpDirection = DockableCtrlRowDirectionEnum.up_2_down;
                        break;
            
                     case DockableAreaCtrlSlotAnchorModeEnum.none:
                     default:
                        throw new Crash();
                  }
               }
               else if (anchorMode == DockableAreaCtrlSlotAnchorModeEnum.center)
               {
                  /// if there one only center control and <see cref="PpCenterDirectionToSet"/>
                  /// is different from current row direction a subcontrol is created
                  if (
                     cur_dck_row.PpSlotsCenter.Length == 1 &&
                     cur_dck_row.PpSlotsCenter[0].Control != null &&
                     cur_dck_row.PpDirection != PpCenterDirectionToSet)
                  {
                     //adds a subrow
                     var slt = cur_dck_row.PpSlotsCenter.FirstOrCrash();
                     var old_ctr = slt.Control.NnOrCrash();
                     var sub_dck = new DockAreaRowCtrl();

                     sub_dck.PpDirection = PpCenterDirectionToSet;
                     sub_dck.Dock = DockStyle.Fill;
                     slt.Control = sub_dck;
                     sub_dck.MthControlDock(old_ctr, anchorMode);
                     cur_dck_row = sub_dck;
                  }
               }

               if (cur_dck_row.PpSlots.Length == 1 && cur_dck_row.PpSlots[0].Control == null)
               {
                  if (cur_dck_row.PpSlots[0].AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center)
                  {
                     cur_dck_row.PpSlots[0].Control = control;
                  }
                  else
                  {
                     throw new Crash();
                  }
               }
               else
               {
                  cur_dck_row.MthControlDock(control, anchorMode);
               }

               return true;
            }
            else
            {
               return false;
            }
         }
         else
         {
            throw new Gate.Tools.ToolsException(
               "Control is already contained, remove before!");
         }
      }

      public bool MthControlUndock(Control control)
      {
         if (PpControlsDocked.ContainsRef(control))
         {
            //current row control 
            var cur_row_ctr = myGetCurrentRowCtrl();

            cur_row_ctr.MthRemove(control);

            if (
               cur_row_ctr.PpControlsAll.ContainsRef(control) &&
               !ReferenceEquals(cur_row_ctr, CtrlAreaRowCtrl) &&
               cur_row_ctr.PpSlots.Length == 1 &&
               cur_row_ctr.PpSlotsCenter.Length == 1)
            {
               var cts = myGetAllRowCtrl();
               var ctr = cur_row_ctr.PpSlotsCenter.FirstOrCrash().Control;
               var par_ctr = cts[cts.Length - 2];
               var slo = par_ctr.PpSlots.FirstOrCrash(s => ReferenceEquals(s, cur_row_ctr));

               if (ctr != null)
               {
                  cur_row_ctr.MthRemove(ctr);
               }

               slo.Control = control;
            }


            return true;
         }

         return false;
      }

      public DockableAreaCtrlSlotAnchorModeEnum MthGetAnchorFromControl(Control control)
      {
         var slo = PpSlotsAll.FirstOrDefault(s => ReferenceEquals(s, control));

         if (slo != null) { return slo.AnchorMode; }
         else { throw new Gate.Tools.ToolsException($""); }
      }

      public void MthControlReplace(Control oldControl, Control? newControl)
      {
         var slo = PpSlotsAll.FirstOrDefault(s => ReferenceEquals(s, oldControl));

         if (slo != null) { slo.Control = newControl; }
         else { throw new Gate.Tools.ToolsException($""); }
      }
   }
}

