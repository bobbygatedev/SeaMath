using Gate.Tools;
using Gate.ToolsView.ControlObserve;

namespace Gate.ToolsView.Dockable
{
   public partial class DockableAreaCtrl
   {
      private abstract class InnerSlot : HierarchicalItem
      {
         private int myFloatSize;

         public InnerSlot(Control? control, DockableAreaCtrlSlotAnchorModeEnum anchorMode, SplitContainer? splitContainer = null)
         {
            AnchorMode = anchorMode;
            SplitCont = splitContainer ?? myMakeSplitContainer(anchorMode);
            ParentForm = SplitCont?.ParentForm;
            FramedControl = control;

            if (SplitCont == null) { throw new Crash(); }

            switch (anchorMode)
            {
               case DockableAreaCtrlSlotAnchorModeEnum.left:
               case DockableAreaCtrlSlotAnchorModeEnum.right:
               case DockableAreaCtrlSlotAnchorModeEnum.center:
                  SplitCont.Orientation = System.Windows.Forms.Orientation.Vertical;
                  break;

               case DockableAreaCtrlSlotAnchorModeEnum.up:
               case DockableAreaCtrlSlotAnchorModeEnum.down:
                  SplitCont.Orientation = System.Windows.Forms.Orientation.Horizontal;
                  break;

               default: throw new Crash();
            }
         }

         public class RowSubSlot : InnerSlot
         {
            public RowSubSlot() : base(null, DockableAreaCtrlSlotAnchorModeEnum.center) { }

            public RowSubSlot(SplitContainer splitContainer) : base(null, DockableAreaCtrlSlotAnchorModeEnum.center, splitContainer) => 
               SplitCont.Panel1.Controls.Clear();

            public void AddSubRow(InnerSlotRow.SubRow subRow) => myAddSubItem(subRow);

            public override string ToString() => ParentRow != null ?
               $"row-sub-slot to row {ParentRow}: FloSize: {FloatSize}(Prog:{FloatSizeToProgram})" :
               $"row-sub-slot unbound : FloSize: {FloatSize}(Prog:{FloatSizeToProgram})";
         }

         public class User : InnerSlot
         {
            private ControlObservableParent? myControlObservableParent;
            private FormWindowState? myWindowState;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="userControl"></param>
            /// <param name="anchorMode"></param>
            /// <param name="initSize"></param>
            public User(Control userControl, DockableAreaCtrlSlotAnchorModeEnum anchorMode, Size initSize) : base(userControl, anchorMode)
            {
               InitSize = initSize;
               myInit();
            }

            /// <summary>
            /// Constructor for converting from RowSubSlot to UserCenterSlot
            /// </summary>
            /// <param name="userControl"></param>
            /// <param name="splitContainer"></param>
            public User(Control userControl, SplitContainer splitContainer) : base(userControl, DockableAreaCtrlSlotAnchorModeEnum.center, splitContainer)
            {
               InitSize = userControl.Size;
               FramedControl = userControl;
               myInit();
            }

            /// <summary>
            /// 
            /// </summary>
            public Control? UserControl => FramedControl;

            public override string ToString() => string.Format(
                  "{0} slot of {1}({2}) bound to {3}: FloSize: {4}(Prog:{5})",
                  AnchorMode,
                  FramedControl?.Name,
                  FramedControl?.GetType().Name,
                  ParentRow != null ? ParentRow.ToString() : "",
                  FloatSize,
                  FloatSizeToProgram);

            private void myInit()
            {
               SplitCont.SplitterMoved += SplitCont_SplitterMoved;
               myWindowState = ParentForm?.WindowState;
               myControlObservableParent = new ControlObservableParent(SplitCont);
               myControlObservableParent.OnControlAdded += MyControlObservableParent_OnControlChange;
               myControlObservableParent.OnControlRemoved += MyControlObservableParent_OnControlChange;
            }

            private void MyControlObservableParent_OnControlChange(Control control)
            {
               var old_par_frm = ParentForm;

               ParentForm = SplitCont?.ParentForm;

               if (old_par_frm != null) { old_par_frm.Resize -= ParentForm_Resize; }

               if (ParentForm != null)
               {
                  ParentForm.Resize += ParentForm_Resize;
                  myWindowState = ParentForm.WindowState;
               }
               else { myWindowState = null; }
            }

            private void ParentForm_Resize(object? sender, EventArgs e)
            {
               if (SplitCont != null && myWindowState == FormWindowState.Minimized && ParentForm?.WindowState != FormWindowState.Minimized)
               {
                  SplitCont.SplitterDistance = myFloatSize;
               }

               myWindowState = ParentForm?.WindowState;
            }

            private void SplitCont_SplitterMoved(object? sender, SplitterEventArgs e)
            {
               if (ParentItem == null) { return; }
               else if (myWindowState != FormWindowState.Minimized && ParentForm?.WindowState != FormWindowState.Minimized)
               {
                  myFloatSize = Direction == DockableCtrlRowDirectionEnum.up_2_down ? SplitCont.Panel1.Height : SplitCont.Panel1.Width;
               }
            }
         }

         public Form? ParentForm { get; private set; }


         /// <summary>
         /// 
         /// </summary>
         public Size InitSize { get; protected set; } = new Size(50, 50);

         /// <summary>
         /// Predefined direction which is defined for all NOT center slots.
         /// </summary>
         public DockableCtrlRowDirectionEnum DirectionPredefined
         {
            get
            {
               switch (AnchorMode)
               {
                  case DockableAreaCtrlSlotAnchorModeEnum.left:
                  case DockableAreaCtrlSlotAnchorModeEnum.right:
                     return DockableCtrlRowDirectionEnum.left_2_right;

                  case DockableAreaCtrlSlotAnchorModeEnum.up:
                  case DockableAreaCtrlSlotAnchorModeEnum.down:
                     return DockableCtrlRowDirectionEnum.up_2_down;

                  case DockableAreaCtrlSlotAnchorModeEnum.center:
                  default: throw new NotImplementedException("Not valid for for center");
               }
            }
         }

         /// <summary>
         /// 
         /// </summary>
         public DockableCtrlRowDirectionEnum? Direction => ParentRow?.Direction;

         /// <summary>
         /// 
         /// </summary>
         public int FloatSizeToProgram { get; set; } = -1;

         /// <summary>
         /// 
         /// </summary>
         public int FloatSize
         {
            get => myFloatSize;

            set
            {
               try { myFloatSize = SplitCont.SplitterDistance = value; }
               catch { myFloatSize = SplitCont.SplitterDistance = 0; }
            }
         }

         public int FloatSizeInit => Direction == DockableCtrlRowDirectionEnum.up_2_down ? InitSize.Height : InitSize.Width;

         /// <summary>
         /// 
         /// </summary>
         public bool IsSubRowSlot => FramedControl == null || FramedControl is SplitContainer;

         /// <summary>
         /// <br> Associated control ie: </br>
         /// <br> - split container of a first slot of a sub row </br>
         /// <br> - control associated with the slot. </br>
         /// </summary>
         public Control? FramedControl
         {
            get => SplitCont.Panel1.Controls.Count > 0 ? SplitCont.Panel1.Controls[0] : null;

            set
            {
               SplitCont.Panel1.Controls.Clear();

               if (value != null)
               {
                  value.Dock = DockStyle.Fill;
                  SplitCont.Panel1.Controls.Add(value);
               }
            }
         }

         public SplitContainer SplitCont { get; }

         public Size? Size => SplitCont?.Panel1.ClientSize;

         public DockableAreaCtrlSlotAnchorModeEnum AnchorMode { get; }

         public Orientation? Orientation => SplitCont?.Orientation;

         public InnerSlotRow? ParentRow => ParentItem as InnerSlotRow;

         public InnerSlotRow? ChildRow => SubItems.FirstOrDefault(s => s is InnerSlotRow) as InnerSlotRow;

         private static SplitContainer myMakeSplitContainer(DockableAreaCtrlSlotAnchorModeEnum anchorMode)
         {
            var spl = new SplitContainer();

            spl.Dock = DockStyle.Fill;
            spl.Panel1MinSize = 0;
            spl.Panel2MinSize = 0;
            spl.SplitterWidth = SPLITTER_WIDTH;
            spl.FixedPanel = anchorMode == DockableAreaCtrlSlotAnchorModeEnum.center ? FixedPanel.None : FixedPanel.Panel1;

            switch (anchorMode)
            {
               case DockableAreaCtrlSlotAnchorModeEnum.center:
               case DockableAreaCtrlSlotAnchorModeEnum.left:
               case DockableAreaCtrlSlotAnchorModeEnum.right:
                  spl.Orientation = System.Windows.Forms.Orientation.Vertical;
                  break;

               case DockableAreaCtrlSlotAnchorModeEnum.up:
               case DockableAreaCtrlSlotAnchorModeEnum.down:
                  spl.Orientation = System.Windows.Forms.Orientation.Horizontal;
                  break;

               default: throw new Crash();
            }

            return spl;
         }
      }
   }
}
