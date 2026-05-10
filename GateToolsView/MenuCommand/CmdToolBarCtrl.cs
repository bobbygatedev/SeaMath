using Gate.Tools;
using Gate.ToolsView.Extended;
using Gate.ToolsView.Extensions;
using System.Windows.Forms.Layout;
using static Gate.ToolsView.MenuCommand.CmdMenu;

namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// 
   /// </summary>
   public partial class CmdToolBarCtrl : UserControl
   {
      private Ref? myCmdMenuRef = null;
      private Panel? myGripPanel;
      private ButtonSeparatorCtrl[]? mySeparators = null;

      public delegate void OnGripMoveChangeStateHandler(object? sender, MoveChangeStateArgs stateArgs);
      public delegate void OnGripMoveHandler(object? sender, MoveArgs moveArgs);

      public event OnGripMoveChangeStateHandler? OnGripMoveStateChange;
      public event OnGripMoveHandler? OnGripMove;

      public CmdToolBarCtrl()
      {
         InitializeComponent();

         PpGripPanel = new InnerGripPanel();
         Controls.Add(myGripPanel);
         SetStyle(ControlStyles.ResizeRedraw, true);
      }

      public class MoveArgs
      {
         public MoveArgs(CmdToolBarContainerCtrl cmdToolBarContainer, Point cmdToolBarRelativeLocation)
         {
            CmdToolBarContainer = cmdToolBarContainer;
            CmdToolBarRelativeLocation = cmdToolBarRelativeLocation;
         }

         public CmdToolBarContainerCtrl CmdToolBarContainer { get; }
         public Point CmdToolBarRelativeLocation { get; }
      }

      public class MoveChangeStateArgs
      {
         public MoveChangeStateArgs(bool newModeValue) => NewModeValue = newModeValue;

         public bool NewModeValue { get; }
      }

      public class ButtonSeparatorCtrl
      {
         public ButtonSeparatorCtrl() { }
         public ButtonSeparatorCtrl(int idx) => AfterButtonIndex = idx;

         public int AfterButtonIndex { get; set; }
         public ExtendedButtonCtrl? AfterButton { get; set; }
         public CmdMenuSeparator? CmdSeparator { get; set; }
      }

      private class InnerGripPanel : Panel
      {
         private const int FIX_WIDTH = 9;
         private const int FIX_WIDTH_HALF = FIX_WIDTH / 2 + 1;
         private const int SPACING = 2;

         public InnerGripPanel() => BackColor = Color.Transparent;

         protected override void OnPaint(PaintEventArgs pevent)
         {
            var par = Parent as CmdToolBarCtrl ?? throw new Crash();
            var gr = pevent.Graphics;
            var lft = FIX_WIDTH_HALF - SPACING;
            var rgt = FIX_WIDTH_HALF + SPACING;
            var bru = new SolidBrush(par.ForeColor);
            var is_eve = true;

            for (int y = 0; y < Height; y += SPACING, is_eve = !is_eve)
            {
               if (is_eve)
               {
                  gr.FillRectangle(bru, lft, y, 1, 1);
                  gr.FillRectangle(bru, rgt, y, 1, 1);
               }
               else
               {
                  gr.FillRectangle(bru, FIX_WIDTH_HALF, y, 1, 1);
               }
            }
         }

         public override Size GetPreferredSize(Size proposedSize) => new Size(FIX_WIDTH, base.GetPreferredSize(proposedSize).Height);

         private bool myIsMoving = false;

         /// <summary>
         /// 
         /// </summary>
         public CmdToolBarCtrl? PpParent => Parent as CmdToolBarCtrl;

         /// <summary>
         /// 
         /// </summary>
         public CmdToolBarContainerCtrl? PpParentContainer => this.MthGetAnchestor<CmdToolBarContainerCtrl>();

         /// <summary>
         /// 
         /// </summary>
         public bool PpIsMoving
         {
            get => myIsMoving;

            set
            {
               if (myIsMoving != value)
               {
                  myIsMoving = value;

                  (PpParent ?? throw new Crash()).myActionOnMoveChangeState(PpParent, new MoveChangeStateArgs(myIsMoving));
               }
            }
         }

         protected override void OnMouseDown(MouseEventArgs e)
         {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
               PpIsMoving = true;
            }
            else
            {
               PpIsMoving = false;
            }
         }

         protected override void OnMouseUp(MouseEventArgs e) => PpIsMoving = false;

         protected override void OnMouseMove(MouseEventArgs e)
         {
            if (PpIsMoving)
            {
               var rel_pnt = (PpParentContainer ?? throw new Crash()).PointToClient(Cursor.Position);

               PpParent?.myActionOnMove(PpParent, new MoveArgs(PpParentContainer, rel_pnt));
            }
         }
      }

      private class InnerLayoutEngine : LayoutEngine
      {
         public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
         {
            var par = container as CmdToolBarCtrl ?? throw new Crash();
            var gp = par.myGripPanel ?? throw new Crash();

            gp.Left = 0;
            gp.Height = par.Height;
            gp.Width = gp.PreferredSize.Width;
            gp.Refresh();

            var x = gp.Right;

            foreach (var but in par.PpButtons)
            {
               but.Left = x;
               but.Height = par.Height;
               but.Width = but.Text == "" ? but.Height : but.PreferredSize.Width;
               but.Refresh();
               x += but.Width + 1;
            }

            par.Width = x;

            return false;
         }
      }

      private class InnerCmdMenuControlAssociation : ICmdMenuControlAssociation
      {
         public InnerCmdMenuControlAssociation(CmdToolBarCtrl toolBar, Ref cmdMenuRef)
         {
            ToolBar = toolBar;
            CmdMenuRef = cmdMenuRef;
         }

         public CmdToolBarCtrl ToolBar { get; }

         public CmdMenu CmdMenu => CmdMenuRef.CmdMenu;

         public bool IsEnabled { get => ToolBar.Enabled; set => ToolBar.Enabled = value; }

         public bool IsVisible { get => true; set { } }

         public Ref CmdMenuRef { get; }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="command"></param>
         public void InsertCommand(int atIndex, Cmd command)
         {
            var but = new ExtendedButtonCtrl();

            command.AddAssociation(new CmdButtonAssociation(but, command));
            ToolBar.MthButtonInsert(atIndex, but);
         }

         public void RemoveCommand(Cmd command)
         {
            var ass = command.Associations.OfType<CmdButtonAssociation>().FirstOrDefault(a => ToolBar.PpButtons.Contains(a.Button));

            if (ass != null)
            {
               ToolBar.MthButtonRemove((ExtendedButtonCtrl)ass.Button);
               command.RemoveAssociation(ass);
            }
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="atIndex"></param>
         /// <param name="separator"></param>
         public void InsertSeparator(int atIndex, CmdMenuSeparator separator)
         {
            //retrieve association of from command to button
            var but_ass = separator.ParentMenuCmdAfter?.Associations.OfType<CmdButtonAssociation>().FirstOrDefault(a => ToolBar.PpButtons.Contains(a.Button));

            if (but_ass != null)
            {
               var sep_ctr = new ButtonSeparatorCtrl();

               sep_ctr.CmdSeparator = separator;
               sep_ctr.AfterButton = (ExtendedButtonCtrl)but_ass.Button;
               ToolBar.PpSeparators = ToolBar.PpSeparators.Concat(new ButtonSeparatorCtrl[] { sep_ctr }).ToArray();
            }
         }

         public void RemoveSeparator(CmdMenuSeparator separator) => ToolBar.PpSeparators = ToolBar.PpSeparators.Where(s => s.CmdSeparator != separator).ToArray();

         public void InsertSubMenu(int atIndex, Ref subMenu) => throw new Crash("Not implemented!");

         public void RemoveSubMenu(Ref subMenu) => throw new Crash("Not implemented!");

         public void SetCaption(string? caption) { }

         public void Show() { }
      }

      public ExtendedButtonCtrl[] PpButtons
      {
         get => Controls.OfType<ExtendedButtonCtrl>().ToArray();

         set
         {
            foreach (var ctr in PpButtons) { Controls.Remove(ctr); }

            Controls.AddRange(value ?? []);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public ButtonSeparatorCtrl[] PpSeparators
      {
         get => mySeparators ?? new ButtonSeparatorCtrl[0];
         set
         {
            mySeparators = value;
            Refresh();
         }
      }

      public Ref? PpCmdMenuRef
      {
         get => myCmdMenuRef;
         set
         {
            if (myCmdMenuRef != null)
            {
               var ass = myCmdMenuRef.CmdMenu.Associations.OfType<InnerCmdMenuControlAssociation>().First(a => a.ToolBar == this);

               myCmdMenuRef.CmdMenu.RemoveAssociation(ass);
            }

            if ((myCmdMenuRef = value) != null) { myCmdMenuRef.CmdMenu.AddAssociation(new InnerCmdMenuControlAssociation(this, myCmdMenuRef)); }
         }
      }

      public Panel? PpGripPanel
      {
         get => myGripPanel;
         set
         {
            if (value != null)
            {
               if (myGripPanel != null)
               {
                  Controls.Remove(myGripPanel);
               }

               Controls.Add(myGripPanel = value);
               Refresh();
            }
         }
      }

      public int PpPreferredWidth
      {
         get
         {
            var w = myGripPanel?.PreferredSize.Width ?? 0;

            foreach (var but in PpButtons)
            {
               var w_but = but.Text == "" ? but.Height : but.PreferredSize.Width;

               w += w_but + 1;
            }

            if (PpButtons.Length > 0) { w -= 1; }

            return w;
         }
      }

      public override LayoutEngine LayoutEngine => new InnerLayoutEngine();

      public void MthButtonInsert(int atIndex, ExtendedButtonCtrl extendedButton)
      {
         Controls.Add(extendedButton);

         if (atIndex >= 0) { Controls.SetChildIndex(extendedButton, atIndex); }

         Refresh();
      }

      public void MthButtonRemove(ExtendedButtonCtrl extendedButton)
      {
         Controls.Remove(extendedButton);
         Refresh();
      }

      protected override void OnPaint(PaintEventArgs e)
      {
         foreach (var sep in PpSeparators ?? new ButtonSeparatorCtrl[0])
         {
            var aft = sep.AfterButton;

            if (aft == null && sep.AfterButtonIndex >= 0 && sep.AfterButtonIndex < PpButtons.Length - 1) { aft = PpButtons[sep.AfterButtonIndex]; }

            if (aft != null) { e.Graphics.DrawLine(new Pen(ForeColor), new Point(aft.Right, 0), new Point(aft.Right, Height)); }
         }
      }

      public override Size GetPreferredSize(Size proposedSize) => new Size(PpPreferredWidth, Height);

      protected virtual void myActionOnMove(object? sender, MoveArgs moveArgs) => OnGripMove?.Invoke(sender, moveArgs);

      protected virtual void myActionOnMoveChangeState(object? sender, MoveChangeStateArgs stateArgs) => OnGripMoveStateChange?.Invoke(sender, stateArgs);

   }
}

