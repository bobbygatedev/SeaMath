using Gate.Tools;
using System.Data;

namespace Gate.ToolsView.Dockable
{
   public partial class DockableAreaCtrl
   {
      private abstract class InnerSlotRow : HierarchicalItem
      {
         public const int MIN_FLOAT_SIZE = 10;

         /// <summary>
         /// 
         /// </summary>
         public class Root : InnerSlotRow
         {
            private readonly Cache myCache;
            private DockableAreaCtrl? myParentDockCtrl;
            private DockableCtrlRowDirectionEnum myDirection;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentDockCtrl"></param>
            public Root(DockableAreaCtrl parentDockCtrl, DockableCtrlRowDirectionEnum direction)
            {
               myDirection = direction;
               myParentDockCtrl = parentDockCtrl;
               myCache = new Cache(this);
               mySlotAddInCollection(new InnerSlot.RowSubSlot());
            }

            /// <summary>
            /// 
            /// </summary>
            public class Cache
            {
               private Root mySlotRowRoot;
               private readonly List<RowMirror> myListRowMirror;

               public Cache(Root slotRowRoot)
               {
                  mySlotRowRoot = slotRowRoot;
                  myListRowMirror = slotRowRoot.AllRows.Select(r => new RowMirror(r)).ToList();
               }

               private class RowMirror
               {
                  private readonly List<InnerSlot> myListSlots;

                  public RowMirror(InnerSlotRow row) => myListSlots = (Row = row).SlotsAny.ToList();

                  public bool HasChanged => myListSlots.Count != Row.SlotsAny.Length || Enumerable.Range(0, myListSlots.Count).Any(i => myListSlots[i] != Row.SlotsAny[i]);

                  public InnerSlotRow Row { get; }
               }

               public void Recreate()
               {
                  myListRowMirror.Clear();
                  myListRowMirror.AddRange(mySlotRowRoot.AllRows.Select(r => new RowMirror(r)));
               }

               public void Flush()
               {
                  var rws_chg = myGetChangedRows();

                  foreach (var row in rws_chg) { row.Flush(); }

                  //removes any child controls to last row
                  var lst_row = mySlotRowRoot.AllRows.Last();

                  if (lst_row.RowSubSlot != null && lst_row.RowSubSlot.SplitCont.Panel1.Controls.Count > 0)
                  {
                     lst_row.RowSubSlot.SplitCont.Panel1.Controls.Clear();
                  }
               }

               public void Update()
               {
                  foreach (var slt in mySlotRowRoot.SlotsAllControlUser) { slt.FloatSizeToProgram = slt.FloatSize; }
               }

               private InnerSlotRow[] myGetChangedRows()
               {
                  var rws = mySlotRowRoot.AllRows;
                  var cyc_len = Math.Min(rws.Length, myListRowMirror.Count);

                  if (cyc_len == 0) { throw new Crash(); }

                  var frs_row_chg = 0;//first row changed

                  for (; frs_row_chg < cyc_len; frs_row_chg++)
                  {
                     if (
                        rws[frs_row_chg] != myListRowMirror[frs_row_chg].Row ||
                        myListRowMirror[frs_row_chg].HasChanged)
                     {
                        break;
                     }
                  }

                  return rws.Skip(frs_row_chg).ToArray();
               }
            }

            public override DockableCtrlRowDirectionEnum Direction => myDirection;

            /// <summary>
            /// 
            /// </summary>
            public override DockableAreaCtrl? ParentDockCtrl => myParentDockCtrl;

            /// <summary>
            /// 
            /// </summary>
            public override Control? ParentCtrl => myParentDockCtrl;

            /// <summary>
            /// 
            /// </summary>
            public InnerSlotRow[] AllRows => AllDescendant.OfType<InnerSlotRow>().ToArray();

            /// <summary>
            /// Array of all slots (not sub row slot) contained in dock control.
            /// </summary>
            public InnerSlot.User[] SlotsAllControlUser => AllRows.SelectMany(r => r.SlotsUser).ToArray();

            public void SetDirection(DockableCtrlRowDirectionEnum direction) => myDirection = direction;

            public void FlushChanges()
            {
               myCache.Flush();
               myCache.Recreate();
            }

            public void UpdateCache() => myCache.Update();
         }

         public class SubRow : InnerSlotRow
         {
            private DockableCtrlRowDirectionEnum myDirection;

            public SubRow(DockableCtrlRowDirectionEnum direction) => myDirection = direction;

            public override DockableAreaCtrl? ParentDockCtrl => ParentItemChain.OfType<Root>().FirstOrDefault()?.ParentDockCtrl;

            public override Control? ParentCtrl => SlotParentSubRow?.SplitCont.Panel1;

            public override DockableCtrlRowDirectionEnum Direction => myDirection;
         }

         /// <summary>
         /// 
         /// </summary>
         public abstract DockableAreaCtrl? ParentDockCtrl { get; }

         /// <summary>
         /// Parent control of row: may be either the anchestor DockCtrl or RowSubSlot Panel1. 
         /// </summary>
         public abstract Control? ParentCtrl { get; }

         /// <summary>
         /// 
         /// </summary>
         public abstract DockableCtrlRowDirectionEnum Direction { get; }

         /// <summary>
         ///  depth(info) value, where 0 is row direct bound to DockCtrl and Max is top value (which contains center controls).
         /// </summary>
         public int Depth => ParentDockCtrl == ParentCtrl ? 0 : (SlotParentSubRow?.ParentRow?.Depth ?? 0) + 1;

         public InnerSlot.RowSubSlot? SlotParentSubRow => ParentItem as InnerSlot.RowSubSlot;

         public InnerSlot.User[] SlotsLeftUp =>
            SlotsAny.Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.left || s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.up).
               Cast<InnerSlot.User>().ToArray();

         public InnerSlot.User[] SlotsUser => SlotsAny.OfType<InnerSlot.User>().ToArray();

         /// <summary>
         /// 
         /// </summary>
         public InnerSlot[] SlotsAny => SubItems.OfType<InnerSlot>().ToArray();

         public InnerSlot[] SlotsAnyCenter => SlotsAny.Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center).ToArray();

         public InnerSlot.User[] SlotsCenterUser => SlotsUser.Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center).ToArray();

         public InnerSlot.User[] SlotsRightDown =>
            SlotsUser.Where(
               s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.right || s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.down).
               Cast<InnerSlot.User>().ToArray();

         /// <summary>
         /// 
         /// </summary>
         public InnerSlot.RowSubSlot? RowSubSlot => SlotsAny.FirstOrDefault(s => s is InnerSlot.RowSubSlot) as InnerSlot.RowSubSlot;

         /// <summary>
         ///  programmed center size, ie float size minus float size of all right/down/left/up slots
         /// </summary>
         public int CenterSizeToProgram
         {
            get
            {
               var sls_sel = DirectionSlotAll;
               var flt_ctr_siz = (Direction == DockableCtrlRowDirectionEnum.up_2_down ?
                  ParentDockCtrl?.Height : ParentDockCtrl?.Width) ?? 0;//control float size

               //(total) center size 
               return flt_ctr_siz - sls_sel.Sum(s => s.FloatSizeToProgram) - SPLITTER_WIDTH * sls_sel.Length;
            }
         }

         public InnerSlot[] DirectionSlotAll
         {
            get
            {
               var sls_all_ctr = ParentDockCtrl?.mySlotRowRoot.SlotsAllControlUser;//all control slots            
               var sls_sel = null as InnerSlot[];

               if (Direction == DockableCtrlRowDirectionEnum.up_2_down)
               {
                  //selected either up or down slots
                  sls_sel = sls_all_ctr?.
                     Where(s =>
                        s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.down ||
                        s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.up).ToArray();
               }
               else
               {
                  //selected either left or down right
                  sls_sel = sls_all_ctr?.
                     Where(s =>
                        s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.left ||
                        s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.right).ToArray();
               }

               return sls_sel ?? [];
            }
         }

         /// <summary>
         /// <br>  the minimum size of center block(sum of all center blocks of a row) ie:</br>
         /// <br> -  </br>
         /// </summary>
         public int MinCenterSize
         {
            get
            {
               var sls_sel = DirectionSlotAll;
               var min_cnt_siz = Direction == DockableCtrlRowDirectionEnum.left_2_right ? MIN_CENTER_WIDTH : MIN_CENTER_HEIGHT;
               var min_siz = Direction == DockableCtrlRowDirectionEnum.left_2_right ? MIN_WIDTH : MIN_HEIGHT;
               var num_anc = sls_sel.Select(s => s.AnchorMode).Distinct().Count();

               switch (num_anc)
               {
                  case 0: return min_siz; //not left/right(up/down dependently on direction) slots in row minimum size = minimum control width(height)
                  case 1: return min_cnt_siz + (min_siz - min_cnt_siz) / 2;//one block in direction
                  case 2: return min_cnt_siz;//blocks in both direction 
                  default: throw new Crash();
               }
            }
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="control"></param>
         public void ReplaceRowSubWithUser(Control? control)
         {
            if (RowSubSlot != null && control != null)
            {
               var rep_slt = new InnerSlot.User(control, RowSubSlot.SplitCont);

               myRemoveSubItem(RowSubSlot);
               mySlotAddInCollection(rep_slt);
            }
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="slot"></param>
         public void ReplaceCenterWithSubRowSlot(InnerSlot.User slot)
         {
            var rep_slt = new InnerSlot.RowSubSlot(slot.SplitCont);

            myRemoveSubItem(slot);
            mySlotAddInCollection(rep_slt);
         }

         /// <summary>
         /// 
         /// </summary>
         public void Flush()
         {
            myRebuildConnections();
            mySetFloatSizeToProgram();
            myApplyFloatSizeToProgram();
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="slot"></param>
         public void RemoveSlot(InnerSlot.User slot)
         {
            var roo_row = (Root)ParentItemChain.First(p => p is InnerSlotRow.Root);
            var all_rws = roo_row.AllRows;
            var row = slot.ParentRow;
            var row_idx = row == null ? -1 : all_rws.ToList().IndexOf(row);
            var row_pls_1 = row_idx + 1 < all_rws.Length ? all_rws[row_idx + 1] : null;
            var row_pls_2 = row_idx + 2 < all_rws.Length ? all_rws[row_idx + 2] : null;

            row?.myRemoveSubItem(slot);

            if (row_idx == 0)
            {
               if (row?.SlotsUser.Length == 0)
               {
                  if (row_pls_1 != null)
                  {
                     var sls = row_pls_1.SlotsUser;

                     row_pls_1.myRemoveSubItemRange(sls);
                     row?.mySlotAddInCollection(sls);

                     if (RowSubSlot != null && row?.SlotsCenterUser.Length > 0) { row?.myRemoveSubItem(RowSubSlot); }

                     if (row_pls_2 != null) { row_pls_2.myReplaceParent(row?.RowSubSlot); }
                  }
               }
            }
            else
            {
               if (row?.SlotsUser.Length == 0)
               {
                  var row_min_1 = all_rws[row_idx - 1];

                  if (row_pls_1 != null)
                  {
                     var row_p1_sls = row_pls_1.SlotsUser;//slots of row plus 1

                     row_pls_1.myRemoveSubItemRange(row_p1_sls);//empties row slot 1 
                     row_min_1.mySlotAddInCollection(row_p1_sls);

                     if (row_pls_2 != null) { row_pls_2.myReplaceParent(row_min_1.RowSubSlot); }
                     else { row.myReplaceParent(null); }
                  }
               }
               else if (row?.SlotsUser.Length == 1 && row.SlotsUser[0].AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center)
               {
                  if (all_rws.Last() != row) { throw new Crash("Center rows shall be the last one!"); }//check 

                  var oth_cnt_slt = row.SlotsUser[0];//other center slot (not removed)

                  row.myRemoveSubItem(oth_cnt_slt);//empties row
                  row.ParentItem = null;//remove row as child row-sub-slot of lower-level row
                  all_rws = roo_row.AllRows;
                  row = all_rws.Last();

                  if (row.RowSubSlot != null) { row.ReplaceRowSubWithUser(oth_cnt_slt?.UserControl); }
                  else { throw new Crash(); }//row lower than top should have row-sub-slot
               }
            }

            if (slot.FramedControl != null) { slot.FramedControl.Parent = null; }
         }

         /// <summary>
         /// Inserts slot in slot hierarchy without change controls (this is made by FlushChanges()). 
         /// </summary>
         /// <param name="newSlot"></param>
         public void InsertSlotOnly(InnerSlot.User newSlot)
         {
            var num_new_row = myGetNumberOfRowToCreate(newSlot);

            if (num_new_row >= 1)
            {
               var new_top_row = new SubRow(my_GetInvertedDirection(Direction));//new slot has differenta new level(top row) is needed.

               var cnt_sls = SlotsCenterUser;

               mySlotRemoveFromCollection(cnt_sls);
               RowSubSlot?.AddSubRow(new_top_row ?? throw new Crash());

               if (num_new_row >= 2)
               {
                  //eg top row has lft-cnt-cnt new_slot is up new stack becomes
                  // cnt-cnt
                  // up-(row_slot)
                  // lft-(row_slot)
                  var new_top_top_row = new SubRow(Direction);

                  // in this case a new slot with direction different from top 
                  // and center slots are > 2 so they have put to new 
                  new_top_row.myCreateRowSubSlotIfNecessary();
                  new_top_row.RowSubSlot?.AddSubRow(new_top_top_row);
                  new_top_row.mySlotAddInCollection(newSlot);
                  new_top_top_row.mySlotAddInCollection(cnt_sls);//sub-sub(top) row receives center
               }
               else { new_top_row.mySlotAddInCollection(cnt_sls.Concat(new InnerSlot[] { newSlot }).ToArray()); }
            }
            else { mySlotAddInCollection(newSlot); }
         }

         /// <summary>
         /// 
         /// </summary>
         /// <returns></returns>
         public override string ToString() => $"{Direction} row depth {Depth}";

         /// <summary>
         /// Returns the number of new row to create:
         /// If new slot is center:
         /// -1  when row contains exactly 1 center and center-direction is different from row 
         /// -0  otherwise
         /// 
         /// If new slot is NOT center:
         /// -2: when control has a different predefined-direction from top-row and top-row has 2 or more center slots
         ///     in this case center slots are placed into new-top-row-2 and new slot in new-top-row-1
         /// -1: when control has a different predefined-direction from top-row, but center slot <= 1
         /// -0: otherwise
         /// </summary>
         /// <param name="newSlot"></param>
         /// <returns></returns>
         private int myGetNumberOfRowToCreate(InnerSlot.User newSlot)
         {
            if (newSlot.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center)
            {
               return SlotsCenterUser.Length == 1 && ParentDockCtrl?.PpCenterDirectionToSet != Direction ? 1 : 0;
            }
            else
            {
               if (SlotsUser.Length == 0 && newSlot.DirectionPredefined != Direction && this is InnerSlotRow.Root)
               {
                  throw new Crash();//constraint: in case of a not-center-slot added to empty slot(root) row direction shall be adjusted
               }

               return newSlot.DirectionPredefined != Direction ? (SlotsCenterUser.Length >= 2 ? 2 : 1) : 0;
            }
         }

         private void mySlotRemoveFromCollection(params InnerSlot.User[] slots)
         {
            myRemoveSubItemRange(slots);
            myCreateRowSubSlotIfNecessary();
         }

         private void mySlotAddInCollection(params InnerSlot[] slots)
         {
            foreach (var slt in slots)
            {
               switch (slt.AnchorMode)
               {
                  case DockableAreaCtrlSlotAnchorModeEnum.center:
                  case DockableAreaCtrlSlotAnchorModeEnum.right:
                  case DockableAreaCtrlSlotAnchorModeEnum.down:
                     myInsertSubItem(slt, SlotsLeftUp.Length + SlotsAnyCenter.Length);
                     break;

                  case DockableAreaCtrlSlotAnchorModeEnum.left:
                  case DockableAreaCtrlSlotAnchorModeEnum.up:
                     myInsertSubItem(slt, SlotsLeftUp.Length);
                     break;

                  default: throw new Crash();
               }
            }

            myCreateRowSubSlotIfNecessary();
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="floatSizes"></param>
         private void myApplyFloatSizeToProgram() { foreach (var slt in SlotsAny) { slt.FloatSize = slt.FloatSizeToProgram; } }

         private void myRebuildConnections()
         {
            var sls = SlotsAny;
            var ori = Direction == DockableCtrlRowDirectionEnum.left_2_right ? Orientation.Vertical : Orientation.Horizontal;

            foreach (var slt in sls)
            {
               if (slt.SplitCont.Orientation != ori) { slt.SplitCont.Orientation = ori; }
            }

            if (sls.First().SplitCont.Parent != ParentCtrl && ParentCtrl != null)
            {
               ParentCtrl.Controls.Clear();
               ParentCtrl.Controls.Add(sls.First().SplitCont);
            }

            for (int i = 0; i < sls.Length - 1; i++)
            {
               sls[i].SplitCont.Panel2Collapsed = false;

               if (sls[i].SplitCont.Panel2.Controls.Count == 0 || sls[i].SplitCont.Panel2.Controls[0] != sls[i + 1].SplitCont)
               {
                  sls[i + 1].SplitCont.Parent = null;
                  sls[i].SplitCont.Panel2.Controls.Clear();
                  sls[i].SplitCont.Panel2.Controls.Add(sls[i + 1].SplitCont);
               }
            }

            sls.Last().SplitCont.Panel2Collapsed = true;
            sls.Last().SplitCont.Panel2.Controls.Clear();

            var cnt_sls = SlotsAnyCenter;

            foreach (var slt in cnt_sls.Take(cnt_sls.Length - 1)) { slt.SplitCont.FixedPanel = FixedPanel.None; }

            cnt_sls.Last().SplitCont.FixedPanel = FixedPanel.Panel2;
         }

         private void mySetFloatSizeToProgram()
         {
            var sls = SlotsAny;
            //not center slot 
            var sls_new = sls.Where(s => s.AnchorMode != DockableAreaCtrlSlotAnchorModeEnum.center && s.FloatSizeToProgram == -1).ToArray();

            //program all sizes (includes center and row-sub-slot)
            foreach (var slt in sls.Where(s => s.FloatSizeToProgram == -1))
            {
               slt.FloatSizeToProgram = Math.Max(MIN_FLOAT_SIZE, slt.FloatSizeInit);
            }

            if (sls_new.Length == 1)
            {
               var slt_new = sls_new[0];
               int min_cnt_wdt = MinCenterSize;

               if (CenterSizeToProgram < MinCenterSize)
               {
                  //all slot having same anchor of new slot, except the new slot
                  var oth_slt_sd = sls.Where(s => s != slt_new && s.AnchorMode == slt_new.AnchorMode).ToArray();

                  foreach (var slt in oth_slt_sd) { slt.FloatSizeToProgram = (int)(slt.FloatSizeToProgram * 0.7); }

                  //if this attempt fails                  
                  while (CenterSizeToProgram < MinCenterSize)
                  {
                     foreach (var slt in sls.Where(s => s.AnchorMode == slt_new.AnchorMode))
                     {
                        slt.FloatSizeToProgram = (int)(slt.FloatSizeToProgram * 0.7);
                     }
                  }
               }
            }
            else if (sls_new.Length > 1) { throw new Crash(); }

            // resizes center slots to new calculated center size
            var slt_cnt = SlotsAnyCenter;
            var cnt_szs = slt_cnt.Select(s => s.FloatSizeToProgram).ToArray();
            var cnt_szs_pro = my_MakeProportional(cnt_szs, CenterSizeToProgram);

            for (int i = 0; i < slt_cnt.Length; i++) { slt_cnt[i].FloatSizeToProgram = cnt_szs_pro[i]; }
         }

         /// <summary>
         /// 
         /// </summary>
         private void myCreateRowSubSlotIfNecessary()
         {
            if (SlotsAnyCenter.Length == 0) { myInsertSubItem(new InnerSlot.RowSubSlot(), SlotsLeftUp.Length); }
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="inSizes"></param>
         /// <param name="totalSize"></param>
         /// <returns></returns>
         private static int[] my_MakeProportional(int[] inSizes, int totalSize)
         {
            if (inSizes.Length == 0) { return new int[0]; }
            else
            {
               var fac = (double)totalSize / inSizes.Sum(s => s);
               var out_siz = inSizes.Select(s => (int)(s * fac)).ToArray();

               //in order the sum = totalSize
               out_siz[out_siz.Length - 1] = totalSize - out_siz.Take(out_siz.Length - 1).Sum();

               return out_siz;
            }
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="direction"></param>
         /// <returns></returns>
         private static DockableCtrlRowDirectionEnum my_GetInvertedDirection(DockableCtrlRowDirectionEnum direction)
         {
            switch (direction)
            {
               case DockableCtrlRowDirectionEnum.left_2_right: return DockableCtrlRowDirectionEnum.up_2_down;
               case DockableCtrlRowDirectionEnum.up_2_down: return DockableCtrlRowDirectionEnum.left_2_right;
               default: throw new Crash();
            }
         }
      }
   }
}
