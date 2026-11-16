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

      private sealed record InnerEntry(Control? Ctrl, DockableAreaCtrlSlotAnchorModeEnum Anchor, Size Space);

      /// <summary>
      /// Constructor.
      /// </summary>
      public DockableAreaCtrl()
      {
         InitializeComponent();

         var roo_row = new DockableAreaRowCtrl();

         roo_row.Dock = DockStyle.Fill;
         Controls.Add(roo_row);
         roo_row.PpDirection = DockableRowDirection.left_2_right;
         roo_row.MthControlDock(null, DockableAreaCtrlSlotAnchorModeEnum.center);
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
      /// Orientation of the center docked controls. 
      /// Setting has no effect when more than a center control is docked.
      /// </summary>
      public DockableRowDirection PpCenterDirectionToSet { get; set; } = DockableRowDirection.left_2_right;

      /// <summary>
      /// Effective direction, property has value if more than one center slot has created.
      /// </summary>
      public DockableRowDirection? PpCenterDirectionEffective =>
         PpAllRows.Last().PpSlotsCenter.Length > 1 ? PpAllRows.Last().PpDirection : null;

      /// <summary>
      /// Array with all docked controls.
      /// </summary>
      public Control[] PpControlsDocked =>
         [.. PpSlotsAll.Select(s => s.Control).Nn()];

      /// <summary>
      /// Left-docked controls.
      /// </summary>
      public Control[] PpControlsDockedLeft =>
         [.. PpSlotsAll.
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.left).
         Select(s => s.Control).Nn()];

      /// <summary>
      ///  right-docked controls.
      /// </summary>
      public Control[] PpControlsDockedRight =>
         [.. PpSlotsAll.
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.right).
         Select(s => s.Control).Nn()];

      /// <summary>
      ///  up-docked controls.
      /// </summary>
      public Control[] PpControlsDockedUp =>
         [.. PpAllRows.SelectMany(c => c.PpSlots).
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.up).
         Select(s => s.Control).Nn()];

      /// <summary>
      /// Down-docked controls.
      /// </summary>
      public Control[] PpControlsDockedDown =>
         [.. PpSlotsAll.
         Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.down).
         Select(s => s.Control).Nn()];

      /// <summary>
      /// Center-docked controls.
      /// </summary>
      public Control[] PpControlsDockedCenter =>
         [.. PpSlotsAll.
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
            var cur_row_ctr = PpAllRows.Last();
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
      public int MthControlsDock(DockableAreaCtrlSlotAnchorModeEnum anchorMode, params Control[] controls) =>
         controls.Count(c => MthControlDock(c, anchorMode));

      /// <summary>
      /// 
      /// </summary>
      public DockableAreaRowCtrl.SlotType[] PpSlotsAll =>
         [.. this.MthGetNephews().
            OfType<DockableAreaRowCtrl>().SelectMany(c => c.PpSlots).
            Where(s => !(s.Control is DockableAreaRowCtrl))];

      internal DockableAreaRowCtrl[] PpAllRows =>
         this.MthGetNephews().OfType<DockableAreaRowCtrl>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="anchorMode"></param>
      /// <returns></returns>
      /// <exception cref="Crash">if different from up,down,right,left</exception>
      private DockableRowDirection? myGetDirection(DockableAreaCtrlSlotAnchorModeEnum anchorMode)
      {
         switch (anchorMode)
         {
            case DockableAreaCtrlSlotAnchorModeEnum.left:
            case DockableAreaCtrlSlotAnchorModeEnum.right:
               return DockableRowDirection.left_2_right;

            case DockableAreaCtrlSlotAnchorModeEnum.up:
            case DockableAreaCtrlSlotAnchorModeEnum.down:
               return DockableRowDirection.up_2_down;

            case DockableAreaCtrlSlotAnchorModeEnum.center:
            case DockableAreaCtrlSlotAnchorModeEnum.none:
            default:
               return null;
         }
      }

      private void myDoAddToCenterRow(Control[] controls)
      {
         //last row
         var row_lst = PpAllRows.LastOrCrash();

         if (row_lst.PpSlotsCenter.FirstOrCrash().Control == null)
         {
            row_lst.PpSlotsCenter[0].Control = controls.FirstOrCrash();
            controls = controls.Skip(1).ToArray();
         }

         row_lst.MthControlsDock(controls, DockableAreaCtrlSlotAnchorModeEnum.center);
      }

      private static DockableRowDirection myGetOpposite(DockableRowDirection direction) =>
         direction == DockableRowDirection.left_2_right ?
            DockableRowDirection.up_2_down : DockableRowDirection.left_2_right;

      /// <summary>
      /// Chain of rows: row 0, then the row inside its center slot, and so on.
      /// Doesn't depend on the order of Controls.
      /// </summary>
      private DockableAreaRowCtrl[] myGetRowChain()
      {
         var lst = new List<DockableAreaRowCtrl> { PpAllRows.FirstOrCrash() };

         while (lst.Last().PpSlotsCenter.FirstOrDefault()?.Control is DockableAreaRowCtrl sub)
         {
            lst.Add(sub);
         }

         return lst.ToArray();
      }

      /// <summary>
      /// Adds a docked control to me. 
      /// </summary>
      /// <param name="control">Control to dock.</param>
      /// <param name="anchorMode">Anchor mode(up,down,left,right,center)</param>
      /// <returns>False if number of already docked control >= PpNumCreatable</returns>
      /// <exception cref="Gate.ToolsView.GateDockException">Control is null or already contained.</exception>
      public bool MthControlDock(Control control, DockableAreaCtrlSlotAnchorModeEnum anchorMode)
      {
         if (anchorMode == DockableAreaCtrlSlotAnchorModeEnum.none)
         {
            throw new Gate.Tools.ToolsException("Anchor 'none' is not dockable");
         }

         if (PpControlsDocked.ContainsRef(control))
         {
            throw new Gate.Tools.ToolsException("Control is already contained, remove before!");
         }

         if (PpControlsDocked.Length >= PpMaxNumDocked) { return false; }

         var chain = myGetRowChain();

         // 1) model: entries grouped by anchor, in slot order
         var by = 
            Enum.GetValues<DockableAreaCtrlSlotAnchorModeEnum>().
            ToDictionary(a => a, a => new List<InnerEntry>());

         foreach (var row in chain)
         {
            foreach (var slo in row.PpSlots)
            {
               if (slo.Control is DockableAreaRowCtrl) { continue; }               // structure
               if (slo.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center && slo.Control == null) { continue; }

               by[slo.AnchorMode].Add(new InnerEntry(slo.Control, slo.AnchorMode, slo.NoCenterSpaces));
            }
         }

         // direction of row 0 is fixed only if it already has non-center slots
         DockableRowDirection? row0_dir =
            chain[0].PpSlotsNotCenter.Length > 0 ? chain[0].PpDirection : null;

         // center direction: effective if 2+ centers already exist, otherwise the one to set
         var cnt_dir = by[DockableAreaCtrlSlotAnchorModeEnum.center].Count >= 2 ?
            chain.Last().PpDirection : PpCenterDirectionToSet;

         // 2) new entry (left/up go to the front: same semantic as myDoReviewIndex)
         var ent = new InnerEntry(control, anchorMode, control.Size);

         if (anchorMode == DockableAreaCtrlSlotAnchorModeEnum.left ||
             anchorMode == DockableAreaCtrlSlotAnchorModeEnum.up)
         {
            by[anchorMode].Insert(0, ent);
         }
         else { by[anchorMode].Add(ent); }

         // 3) rebuild
         myDoBuildRows(chain, by, row0_dir, cnt_dir, anchorMode);

         return true;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="control"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      public bool MthControlUndock(Control control)
      {
         if (PpControlsDocked.ContainsRef(control))
         {
            /// <see cref="DockableAreaRowCtrl"/> associated control to undock
            var slo_2_rem = PpSlotsAll.FirstOrCrash(s => ReferenceEquals(s.Control, control));
            var row = slo_2_rem.Parent.NnOrCrash();
            var rws = PpAllRows;
            var n_rws = rws.Length;

            if (n_rws > 3) { throw new Crash(); }

            var row_emp = rws.FirstOrDefaultUnique(r => r.PpControlsAll.Length == 0);

            if (
               slo_2_rem.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center &&
               PpAllRows.LastOrCrash().PpSlotsCenter.Length == 1)
            {
               //in first row i'm removing last remaining control
               slo_2_rem.Control = null;

               return true;
            }
            else if (row.MthRemoveControl(control))
            {
               var row_lst = PpAllRows.LastOrCrash();
               var nc = row_lst.PpSlotsCenter.Length;

               if (n_rws > 1 && nc == row_lst.PpSlots.Length && nc <= 1)
               {
                  var cns = row_lst.PpControlsAll;

                  foreach (var cnt in cns) { cnt.Parent = null; }

                  row_lst.Parent = null;
                  row_lst = PpAllRows.LastOrCrash();
                  myDoAddToCenterRow(cns);

                  return true;
               }

               //if there are more than one row and there is 
               else if (n_rws > 1 && row_emp != null)
               {
                  //empty is last line just remove
                  if (ReferenceEquals(rws.LastOrCrash(), row_emp))
                  {
                     row_emp.Parent = null;//remove parent only

                     return true;
                  }
                  else if (ReferenceEquals(rws.FirstOrCrash(), row_emp))
                  {
                     //first row removed
                     var sls_lst = rws.Last().PpSlots.Where(s => s.Control != null).ToArray();

                     rws[0].Parent = null;
                     rws[1].Parent = this;

                     return true;
                  }
                  else
                  {
                     //center line center controls moved to first line
                     if (rws.Last().PpSlotsCenter.Length == rws.Last().PpSlots.Length)
                     {
                        var sls = rws.Last().PpSlotsCenter;

                        foreach (var ctr in sls) { ctr.Control = null; }

                        rws.FirstOrCrash().PpSlotsCenter.FirstOrDefaultUnique().NnOrCrash().Control = null;
                        rws.FirstOrCrash().MthAddSlotsFromOther(sls);

                        return true;
                     }
                     else { throw new Crash(); }
                  }
               }
               else
               {
                  return true;
               }
            }
         }

         return false;
      }

      public void MthControlReplace(Control oldControl, Control? newControl)
      {
         if (ReferenceEquals(oldControl, newControl))
         {
            throw new Gate.Tools.ToolsException($"Old and new are same control");
         }
         else
         {
            var slo = PpSlotsAll.FirstOrDefault(s => ReferenceEquals(s.Control, oldControl));

            if (slo != null)
            {
               slo.Control = newControl;
            }
            else
            {
               throw new Gate.Tools.ToolsException(
                  $"Control to replace not included in ${GetType().Name}");
            }
         }
      }

      public DockableAreaCtrlSlotAnchorModeEnum MthGetAnchorFromControl(Control control)
      {
         var slo = PpSlotsAll.FirstOrDefault(s => ReferenceEquals(s.Control, control));

         if (slo != null) { return slo.AnchorMode; }
         else { throw new Gate.Tools.ToolsException($""); }
      }

      private void myDoBuildRows(
         DockableAreaRowCtrl[] chain,
         Dictionary<DockableAreaCtrlSlotAnchorModeEnum, List<InnerEntry>> by,
         DockableRowDirection? row0Direction,
         DockableRowDirection centerDirection,
         DockableAreaCtrlSlotAnchorModeEnum newAnchor)
      {
         var lft = by[DockableAreaCtrlSlotAnchorModeEnum.left];
         var rgt = by[DockableAreaCtrlSlotAnchorModeEnum.right];
         var upp = by[DockableAreaCtrlSlotAnchorModeEnum.up];
         var dwn = by[DockableAreaCtrlSlotAnchorModeEnum.down];
         var cnt = by[DockableAreaCtrlSlotAnchorModeEnum.center];

         var has_h = lft.Count + rgt.Count > 0;
         var has_v = upp.Count + dwn.Count > 0;

         // 1) directions of the rows (same rules as GetRequiredRows)
         var d0 =
            has_h && has_v ? row0Direction ?? myGetDirection(newAnchor) ?? DockableRowDirection.left_2_right :
            has_h ? DockableRowDirection.left_2_right :
            has_v ? DockableRowDirection.up_2_down :
            cnt.Count >= 2 ? centerDirection : chain[0].PpDirection;

         var dirs = new List<DockableRowDirection> { d0 };
         var nc_rows = (has_h ? 1 : 0) + (has_v ? 1 : 0);

         if (nc_rows == 2) { dirs.Add(myGetOpposite(d0)); }

         if (nc_rows > 0 && cnt.Count >= 2 && centerDirection != dirs.Last()) { dirs.Add(centerDirection); }

         // 2) tear down: detach user controls (NOT disposed), dispose the structure
         foreach (var row in chain)
         {
            row.PpIsAutoRebuild = false;

            foreach (var slo in row.PpSlots)
            {
               if (slo.Control is not DockableAreaRowCtrl) { slo.Control = null; }
            }
         }

         var old_sub = chain[0].PpSlotsCenter.FirstOrDefault()?.Control as DockableAreaRowCtrl;

         chain[0].MthRemoveSlots(chain[0].PpSlots);
         old_sub?.Dispose();

         // 3) new chain
         var lst_rws = new List<DockableAreaRowCtrl> { PpAllRows.FirstOrCrash() };

         for (var i = 1; i < dirs.Count; i++)
         {
            lst_rws.Add(new DockableAreaRowCtrl { Dock = DockStyle.Fill, PpIsAutoRebuild = false });
         }

         for (var i = 0; i < lst_rws.Count; i++)
         {
            var row = lst_rws[i];
            var is_hor = dirs[i] == DockableRowDirection.left_2_right;

            row.PpDirection = dirs[i];

            // non-center slots only in the first nc_rows rows
            if (i < nc_rows)
            {
               var near = is_hor ? lft : upp;
               var far = is_hor ? rgt : dwn;

               // near anchors are inserted at the front by myDoReviewIndex -> add in reverse
               foreach (var e in Enumerable.Reverse(near)) { row.MthControlDock(e.Ctrl, e.Anchor, e.Space); }
               foreach (var e in far) { row.MthControlDock(e.Ctrl, e.Anchor, e.Space); }
            }

            if (i < lst_rws.Count - 1)
            {
               // the next row lives in the (only) center slot
               row.MthControlDock(lst_rws[i + 1], DockableAreaCtrlSlotAnchorModeEnum.center);
            }
            else if (cnt.Count == 0)
            {
               row.MthControlDock(null, DockableAreaCtrlSlotAnchorModeEnum.center);
            }
            else
            {
               foreach (var e in cnt) { row.MthControlDock(e.Ctrl, DockableAreaCtrlSlotAnchorModeEnum.center); }
            }
         }

         // 4) layout from inside to outside, then re-enable auto rebuild
         for (var i = lst_rws.Count - 1; i >= 0; i--) { lst_rws[i].MthRebuild(); }

         foreach (var row in lst_rws) { row.PpIsAutoRebuild = true; }
      }
   }
}


