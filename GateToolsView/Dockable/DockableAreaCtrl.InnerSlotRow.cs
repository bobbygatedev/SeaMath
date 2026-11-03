using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.ControlObserve;
using System.Data;
using static Gate.ToolsView.Dockable.DockableAreaCtrl.InnerSlot;

namespace Gate.ToolsView.Dockable
{
   public partial class DockableAreaCtrl
   {
      /// <summary>
      /// 
      /// </summary>
      public abstract class InnerSlotRow : HierarchicalItem
      {
         public const int MIN_FLOAT_SIZE = 10;

         /// <summary>
         /// 
         /// </summary>
         protected InnerSlotRow() { }

         /// <summary>
         /// 
         /// </summary>
         public class Root : InnerSlotRow
         {
            private readonly Cache myCache;
            private DockableAreaCtrl? myParentDockCtrl;
            private DockableCtrlRowDirectionEnum myDirection;
            private readonly ControlObservableParent myControlObservableParent;
            private readonly ControlObservableChild myControlObservableChild;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentDockCtrl"></param>
            public Root(DockableAreaCtrl parentDockCtrl, DockableCtrlRowDirectionEnum direction)
            {
               myDirection = direction;
               myParentDockCtrl = parentDockCtrl;
               myCache = new Cache(this);
               mySlotAddInCollection(new CenterSlot());
               myControlObservableParent = new ControlObservableParent(parentDockCtrl);
               myControlObservableParent.OnControlAdded += MyControlObservableParent_OnControlAdded;
               myControlObservableParent.OnControlRemoved += MyControlObservableParent_OnControlRemoved;
               myControlObservableChild = new ControlObservableChild(parentDockCtrl);
               myControlObservableChild.OnControlAdded += MyControlObservableChild_OnControlAdded;
               myControlObservableChild.OnControlRemoved += MyControlObservableChild_OnControlRemoved;
            }

            private void MyControlObservableChild_OnControlAdded(Control control)
            {
               if (control is SplitContainer sc)
               {
                  sc.SplitterMoving += myOnSplitterMoving;
                  sc.SplitterMoved += myOnSplitterMoved;
               }
            }

            private void myOnSplitterMoved(object? sender, SplitterEventArgs e) => myReset(sender);

            private void myReset(object? sender)
            {
               var sc = sender.ConvertOrCrash<SplitContainer>();
               var slo =
                  AllDescendant.OfType<InnerSlot>().
                  FirstOrCrash(s => ReferenceEquals(s.SplitContainer, sc));

               var row = slo.ParentRow;

               foreach (var ss in row?.SlotsLeftUp ?? []) { ss.SplitContainer.FixedPanel = FixedPanel.Panel1; }

               var cns = row?.SlotsAny.Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center) ?? [];

               cns.FirstOrCrash().SplitContainer.FixedPanel = FixedPanel.Panel2;

               foreach (var ss in cns.Skip(1)) { ss.SplitContainer.FixedPanel = FixedPanel.None; }

               var srs = row?.SlotsRightDown ?? [];

               foreach (var ss in srs.Skip(1)) { ss.SplitContainer.FixedPanel = FixedPanel.None; }

               if (srs.Length > 0)
               {
                  srs.First().SplitContainer.FixedPanel = FixedPanel.Panel2;
                  //srs.Last().SplitContainer.FixedPanel = FixedPanel.Panel2;//tododo
               }
            }

            private void MyControlObservableChild_OnControlRemoved(Control control)
            {
               if (control is SplitContainer sc)
               {
                  sc.SplitterMoving -= myOnSplitterMoving;
                  sc.SplitterMoved -= myOnSplitterMoved;
               }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void myOnSplitterMoving(object? sender, SplitterCancelEventArgs e)
            {
               var sc = sender.ConvertOrCrash<SplitContainer>();
               var slo =
                  AllDescendant.OfType<InnerSlot>().
                  FirstOrCrash(s => ReferenceEquals(s.SplitContainer, sc));
               var row = slo.ParentRow.NnOrCrash();

               myReset(sender);

               if (
                  slo.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.down ||
                  slo.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.right)
               {
                  slo = row.SlotsAny[row.SlotsAny.ToList().IndexOf(slo) + 1];
               }

               switch (slo.SplitContainer.FixedPanel)
               {
                  case FixedPanel.None: break;
                  case FixedPanel.Panel1:
                     slo.SplitContainer.FixedPanel = FixedPanel.Panel2;
                     break;

                  case FixedPanel.Panel2:
                     slo.SplitContainer.FixedPanel = FixedPanel.Panel1;
                     break;

                  default: throw new Crash();
               }
            }

            private void MyControlObservableParent_OnControlRemoved(Control control)
            {
               if (control is Form frm)
               {
                  frm.ResizeBegin -= ParentForm_ResizeBegin;
                  frm.ResizeEnd -= ParentForm_ResizeEnd;
               }
            }

            private void MyControlObservableParent_OnControlAdded(Control control)
            {
               if (control is Form frm)
               {
                  frm.ResizeBegin += ParentForm_ResizeBegin;
                  frm.ResizeEnd += ParentForm_ResizeEnd;
               }
            }

            private void ParentForm_ResizeEnd(object? sender, EventArgs e)
            {
               foreach (var ss in AllDescendant.OfType<InnerSlot>())
               {
                  ss.SplitContainer.FixedPanel = FixedPanel.None;
               }
            }

            private void ParentForm_ResizeBegin(object? sender, EventArgs e)
            {
               foreach (var ss in AllDescendant.OfType<CenterSlot>())
               {
                  ss.SplitContainer.FixedPanel = FixedPanel.Panel2;
               }

               foreach (var ss in AllDescendant.OfType<User>())
               {
                  ss.SplitContainer.FixedPanel = FixedPanel.Panel1;
               }
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

                  if (lst_row.CenterSlot != null && lst_row.CenterSlot.SplitContainer.Panel1.Controls.Count > 0)
                  {
                     lst_row.CenterSlot.SplitContainer.Panel1.Controls.Clear();
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
            public User[] SlotsAllControlUser => AllRows.SelectMany(r => r.SlotsUser).ToArray();

            public void SetDirection(DockableCtrlRowDirectionEnum direction) => myDirection = direction;

            public void FlushChanges()
            {
               myCache.Flush();
               myCache.Recreate();
            }

            public void UpdateCache() => myCache.Update();

            public override string ToString() => $"{base.ToString()}(root)";
         }

         /// <summary>
         /// 
         /// </summary>
         public class NotRoot : InnerSlotRow
         {
            public NotRoot(DockableCtrlRowDirectionEnum direction) => Direction = direction;

            public override DockableAreaCtrl? ParentDockCtrl => ParentItemChain.OfType<Root>().FirstOrDefault()?.ParentDockCtrl;

            public override Control? ParentCtrl => ParentCenterSlot?.SplitContainer.Panel1;

            public override DockableCtrlRowDirectionEnum Direction { get; }
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
         /// Depth(info) value, where 0 is row direct bound to DockCtrl and Max is top value (which contains center controls).
         /// </summary>
         public int Depth => ParentDockCtrl == ParentCtrl ? 0 : (ParentCenterSlot?.ParentRow?.Depth ?? 0) + 1;

         public CenterSlot? ParentCenterSlot => ParentItem as CenterSlot;

         public User[] SlotsLeftUp =>
            SlotsAny.Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.left || s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.up).
               Cast<User>().ToArray();

         public User[] SlotsUser => SlotsAny.OfType<User>().ToArray();

         /// <summary>
         /// 
         /// </summary>
         public InnerSlot[] SlotsAny => SubItems.OfType<InnerSlot>().ToArray();

         public InnerSlot[] SlotsAnyCenter => SlotsAny.Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center).ToArray();

         public User[] SlotsCenterUser => SlotsUser.Where(s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center).ToArray();

         public User[] SlotsRightDown =>
            SlotsUser.Where(
               s => s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.right || s.AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.down).
               Cast<User>().ToArray();

         /// <summary>
         /// 
         /// </summary>
         public CenterSlot? CenterSlot => SlotsAny.FirstOrDefault(s => s is CenterSlot) as CenterSlot;

         /// <summary>
         /// Programmed center size, ie float size minus float size of all right/down/left/up slots
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
               var sls_all_ctr = ParentDockCtrl?.PpRowRoot.SlotsAllControlUser;//all control slots            
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
            if (CenterSlot != null && control != null)
            {
               var rep_slt = new InnerSlot.User(control, CenterSlot.SplitContainer);

               myRemoveSubItem(CenterSlot);
               mySlotAddInCollection(rep_slt);
            }
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="slot"></param>
         public void ReplaceCenterWithSubRowSlot(InnerSlot.User slot)
         {
            var rep_slt = new InnerSlot.CenterSlot(slot.SplitContainer);

            myRemoveSubItem(slot);
            mySlotAddInCollection(rep_slt);
            ParentDockCtrl.NnOrCrash().PpRowRoot.FlushChanges();
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
            var roo_row = ParentItemChain.OfType<Root>().FirstOrCrash();
            var all_rws = roo_row.AllRows;
            var row = slot.ParentRow.NnOrCrash();
            var row_idx = all_rws.ToList().IndexOf(row);
            var row_pls_1 = all_rws.ElementAtOrDefault(row_idx + 1);
            var row_pls_2 = all_rws.ElementAtOrDefault(row_idx + 2);

            row.myRemoveSubItem(slot);

            if (row is Root roo)
            {
               if (row.SlotsUser.Length == 0 && row_pls_1 != null)
               {
                  var sls = row_pls_1.SlotsUser;

                  row_pls_1.myRemoveSubItemRange(sls);
                  row.mySlotAddInCollection(sls);

                  if (CenterSlot != null && row.SlotsCenterUser.Length > 0)
                  {
                     row.myRemoveSubItem(CenterSlot);
                  }

                  if (row_pls_2 != null) { row_pls_2.myReplaceParent(row.CenterSlot); }

                  roo.SetDirection(row_pls_1.Direction);
               }
            }
            else if (row.SlotsUser.Length == 0)
            {
               var row_min_1 = all_rws[row_idx - 1];

               if (row_pls_1 != null)
               {
                  var row_p1_sls = row_pls_1.SlotsUser;//slots of row plus 1

                  row_pls_1.myRemoveSubItemRange(row_p1_sls);//empties row slot 1 
                  row_min_1.mySlotAddInCollection(row_p1_sls);

                  if (row_pls_2 != null) { row_pls_2.myReplaceParent(row_min_1.CenterSlot); }
                  else { row.myReplaceParent(null); }
               }
            }
            else if (
               row.SlotsUser.Length == 1 &&
               row.SlotsUser[0].AnchorMode == DockableAreaCtrlSlotAnchorModeEnum.center)
            {
               if (all_rws.Last() != row) { throw new Crash("Center rows shall be the last one!"); }//check 

               var oth_cnt_slt = row.SlotsUser[0];//other center slot (not removed)

               row.myRemoveSubItem(oth_cnt_slt);//empties row
               row.ParentItem = null;//remove row as child center-slot of lower-level row
               all_rws = roo_row.AllRows;
               row = all_rws.Last();

               if (row.CenterSlot != null) { row.ReplaceRowSubWithUser(oth_cnt_slt?.FramedControl); }
               else { throw new Crash(); }//row lower than top should have center-slot
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
               //new slot has differential new level(top row) is needed.
               var new_top_row = new NotRoot(myGetInvertedDirection(Direction));
               var cnt_sls = SlotsCenterUser;

               mySlotRemoveFromCollection(cnt_sls);
               CenterSlot?.AddSubRow(new_top_row ?? throw new Crash());

               if (num_new_row >= 2)
               {
                  //eg top row has lft-cnt-cnt new_slot is up new stack becomes
                  // cnt-cnt
                  // up-(row_slot)
                  // lft-(row_slot)
                  var new_top_top_row = new NotRoot(Direction);

                  // in this case a new slot with direction different from top 
                  // and center slots are > 2 so they have put to new 
                  new_top_row.myCreateRowSubSlotIfNecessary();
                  new_top_row.CenterSlot?.AddSubRow(new_top_top_row);
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
               if (slt.SplitContainer.Orientation != ori) { slt.SplitContainer.Orientation = ori; }
            }

            if (sls.First().SplitContainer.Parent != ParentCtrl && ParentCtrl != null)
            {
               ParentCtrl.Controls.Clear();
               ParentCtrl.Controls.Add(sls.First().SplitContainer);
            }

            for (int i = 0; i < sls.Length - 1; i++)
            {
               sls[i].SplitContainer.Panel2Collapsed = false;

               if (sls[i].SplitContainer.Panel2.Controls.Count == 0 || sls[i].SplitContainer.Panel2.Controls[0] != sls[i + 1].SplitContainer)
               {
                  sls[i + 1].SplitContainer.Parent = null;
                  sls[i].SplitContainer.Panel2.Controls.Clear();
                  sls[i].SplitContainer.Panel2.Controls.Add(sls[i + 1].SplitContainer);
               }
            }

            sls.Last().SplitContainer.Panel2Collapsed = true;
            sls.Last().SplitContainer.Panel2.Controls.Clear();
         }

         private void mySetFloatSizeToProgram()
         {
            var sls = SlotsAny;
            //not center slot 
            var sls_new = sls.Where(s => s.AnchorMode != DockableAreaCtrlSlotAnchorModeEnum.center && s.FloatSizeToProgram == -1).ToArray();

            //program all sizes (includes center and center-slot)
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
            var cnt_szs_pro = myMakeProportional(cnt_szs, CenterSizeToProgram);

            for (int i = 0; i < slt_cnt.Length; i++) { slt_cnt[i].FloatSizeToProgram = cnt_szs_pro[i]; }
         }

         /// <summary>
         /// 
         /// </summary>
         private void myCreateRowSubSlotIfNecessary()
         {
            if (SlotsAnyCenter.Length == 0) { myInsertSubItem(new InnerSlot.CenterSlot(), SlotsLeftUp.Length); }
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="inSizes"></param>
         /// <param name="totalSize"></param>
         /// <returns></returns>
         private static int[] myMakeProportional(int[] inSizes, int totalSize)
         {
            if (inSizes.Length == 0) { return []; }
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
         private static DockableCtrlRowDirectionEnum myGetInvertedDirection(DockableCtrlRowDirectionEnum direction)
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
