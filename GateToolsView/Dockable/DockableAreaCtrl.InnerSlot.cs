using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.ControlObserve;

namespace Gate.ToolsView.Dockable
{
   public partial class DockableAreaCtrl
   {
      /// <summary>
      /// 
      /// </summary>
      public abstract class InnerSlot : HierarchicalItem
      {
         private int myFloatSize;
         private static int myCnt = 0;

         protected InnerSlot(Control? control, DockableAreaCtrlSlotAnchorModeEnum anchorMode, SplitContainer? splitContainer)
         {
            AnchorMode = anchorMode;
            SplitContainer = splitContainer ?? myMakeSplitContainer(anchorMode) ?? throw new Crash();
            SplitContainer.Tag = this;
            ParentForm = SplitContainer.ParentForm;
            FramedControl = control;
            Idx = ++myCnt;
         }

         /// <summary>
         /// 
         /// </summary>
         public class CenterSlot : InnerSlot
         {
            public CenterSlot() : base(null, DockableAreaCtrlSlotAnchorModeEnum.center, null) { }

            public CenterSlot(SplitContainer splitContainer) : base(null, DockableAreaCtrlSlotAnchorModeEnum.center, splitContainer) =>
               SplitContainer.Panel1.Controls.Clear();

            public void AddSubRow(InnerSlotRow.NotRoot subRow) => myAddSubItem(subRow);

            public override string ToString() => ParentRow != null ?
               $"center-slot to row {ParentRow}: FloSize: {FloatSize}(Prog:{FloatSizeToProgram})" :
               $"center-slot unbound : FloSize: {FloatSize}(Prog:{FloatSizeToProgram})";
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
            public User(Control userControl, DockableAreaCtrlSlotAnchorModeEnum anchorMode, Size initSize) :
               base(userControl, anchorMode, null)
            {
               InitSize = initSize;
               myInit();
            }

            /// <summary>
            /// Constructor for converting from RowSubSlot to UserCenterSlot
            /// </summary>
            /// <param name="userControl"></param>
            /// <param name="splitContainer"></param>
            public User(Control userControl, SplitContainer splitContainer) :
               base(userControl, DockableAreaCtrlSlotAnchorModeEnum.center, splitContainer)
            {
               InitSize = userControl.Size;
               FramedControl = userControl;
               myInit();
            }

            /// <summary>
            /// 
            /// </summary>
            public Control? UserControl => FramedControl;

            private void myInit()
            {
               myWindowState = ParentForm?.WindowState;
               myControlObservableParent = new ControlObservableParent(SplitContainer);
               myControlObservableParent.OnControlAdded += MyControlObservableParent_OnControlChange;
               myControlObservableParent.OnControlRemoved += MyControlObservableParent_OnControlChange;
            }

            private void MyControlObservableParent_OnControlChange(Control control)
            {
               var old_par_frm = ParentForm;

               ParentForm = SplitContainer?.ParentForm;

               if (SplitContainer != null) { SplitContainer.SplitterMoved += SplitCont_SplitterMoved; }

               if (old_par_frm != null)
               {
                  old_par_frm.Resize -= ParentForm_Resize;
               }

               if (ParentForm != null)
               {
                  ParentForm.Resize += ParentForm_Resize;
                  myWindowState = ParentForm.WindowState;
               }
               else { myWindowState = null; }
            }

            private void ParentForm_Resize(object? sender, EventArgs e)
            {
               if (
                  SplitContainer != null &&
                  myWindowState == FormWindowState.Minimized &&
                  ParentForm?.WindowState != FormWindowState.Minimized)
               {
                  SplitContainer.NnOrCrash().SplitterDistance = FloatSize;
               }

               myWindowState = ParentForm?.WindowState;
            }

            //tododo
            private void SplitCont_SplitterMoved(object? sender, SplitterEventArgs e)
            {
               var sc = sender.ConvertOrCrash<SplitContainer>();

               if (
                  myWindowState != FormWindowState.Minimized &&
                  ParentForm?.WindowState != FormWindowState.Minimized)
               {
                  myFloatSize =
                     Direction == DockableCtrlRowDirectionEnum.up_2_down ?
                        sc.Panel1.Height : sc.Panel1.Width;
                  Console.WriteLine($"myFloatSize: {myFloatSize}");
               }
            }

            public override string ToString() =>
               $"{AnchorMode} slot of {FramedControl?.Name}({FramedControl?.GetType().Name}) " +
               $"bound to {ParentRow?.ToString()}: FloSize: {FloatSize}(Prog:{FloatSizeToProgram})";

         }

         public int Idx { get; }

         /// <summary>
         /// 
         /// </summary>
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
               try { myFloatSize = SplitContainer.NnOrCrash().SplitterDistance = value; }
               catch { myFloatSize = SplitContainer.NnOrCrash().SplitterDistance = 0; }
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
            get => SplitContainer.Panel1.Controls.Count > 0 ? SplitContainer.Panel1.Controls[0] : null;

            set
            {
               SplitContainer.Panel1.Controls.Clear();

               if (value != null)
               {
                  value.Dock = DockStyle.Fill;
                  SplitContainer.Panel1.Controls.Add(value);
               }
            }
         }

         public SplitContainer SplitContainer { get; }

         public Size? Size => SplitContainer?.Panel1.ClientSize;

         public DockableAreaCtrlSlotAnchorModeEnum AnchorMode { get; }

         public Orientation? Orientation => SplitContainer?.Orientation;

         public InnerSlotRow? ParentRow => ParentItem as InnerSlotRow;

         public InnerSlotRow? ChildRow => SubItems.FirstOrDefault(s => s is InnerSlotRow) as InnerSlotRow;

         private static SplitContainer myMakeSplitContainer(DockableAreaCtrlSlotAnchorModeEnum anchorMode)
         {
            var spl = new SplitContainer();

            spl.Dock = DockStyle.Fill;
            spl.Panel1MinSize = 0;
            spl.Panel2MinSize = 0;
            spl.SplitterWidth = SPLITTER_WIDTH;

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
