using Gate.ToolsView.Extensions;
using Gate.ToolsView.MenuCommand;

namespace Gate.ToolsView.MenuExtended
{
   /// <summary>
   /// 
   /// </summary>
   public partial class ExtendedMenuDropDown : ContextMenuStrip, IExtendedMenuAspect
   {
      private Button? myMenuButton = null;
      private CmdMenu.Ref? myCmdMenuRef = null;
      private Color myColorSave = Color.Empty;
      private Color myBackColorSelected = Color.Empty;
      private Color myBorder = Color.Empty;
      private Color myItemBorder = Color.Empty;
      private Color myBackColorMargin = Color.Empty;
      private Color myCheckBoxBackground = Color.Empty;
      private bool myIsVisible = true;
      private bool myIsEnabled = true;

      public ExtendedMenuDropDown()
      {
         Opening += DockMenuStrip_Opening;
         Closed += DockMenuStrip_Closed;
         Renderer = new InnerRenderer(new InnerColorTable(this));
      }

      public class ForMainMenu : ExtendedMenuDropDown
      {
         public ForMainMenu(ExtendedMainMenu parent) => ParentMainMenu = parent;

         public ExtendedMainMenu ParentMainMenu { get; }
      }

      public CmdMenu.Ref? PpCmdMenuRef
      {
         get => myCmdMenuRef;

         set
         {
            if (myCmdMenuRef != null)
            {
               myCmdMenuRef.CmdMenu.RemoveAssociation(myCmdMenuRef.CmdMenu.Associations.OfType<InnerCmdMenuControlAssociation>().First(a => a.MenuDropDown == this));
            }

            if ((myCmdMenuRef = value) != null) { myCmdMenuRef.CmdMenu.AddAssociation(new InnerCmdMenuControlAssociation(this, myCmdMenuRef)); }
         }
      }

      public new bool Visible
      {
         get => myIsVisible;

         set
         {
            myIsVisible = value;

            if (myMenuButton != null)
            {
               myMenuButton.MthInvoke(() => myMenuButton.Visible = value);
            }
         }
      }

      public new bool Enabled
      {
         get => myIsEnabled;

         set
         {
            myIsEnabled = value;

            if (myMenuButton != null)
            {
               myMenuButton.MthInvoke(() => myMenuButton.Enabled = value);
            }
         }
      }

      public Color PpBackColorSelected
      {
         get => myBackColorSelected;
         set
         {
            myBackColorSelected = value;
            Renderer = new InnerRenderer(new InnerColorTable(this));
         }
      }

      public Color PpBorderColor
      {
         get => myBorder;
         set
         {
            myBorder = value;
            Renderer = new InnerRenderer(new InnerColorTable(this));
         }
      }

      public Color PpItemBorderColor
      {
         get => myItemBorder;
         set
         {
            myItemBorder = value;
            Renderer = new InnerRenderer(new InnerColorTable(this));
         }
      }

      public Color PpBackColorMargin
      {
         get => myBackColorMargin;

         set
         {
            myBackColorMargin = value;
            Renderer = new InnerRenderer(new InnerColorTable(this));
         }
      }

      public Color PpCheckBoxBackground
      {
         get => myCheckBoxBackground;

         set
         {
            myCheckBoxBackground = value;
            Renderer = new InnerRenderer(new InnerColorTable(this));
         }
      }

      public Button? PpMenuButton
      {
         get => myMenuButton;
         set
         {
            if (myMenuButton != null) { myMenuButton.Click -= MyMenuButton_Click; }

            (myMenuButton = value)?.MthInvoke(() =>
            {
               myMenuButton.Click += MyMenuButton_Click;
               myMenuButton.Visible = Visible;
               myMenuButton.Text = Text;
            });
         }
      }

      public void MthShow()
      {
         if (myMenuButton?.Parent != null)
         {
            myMenuButton.MthInvoke(() => Show(myMenuButton.Parent, new Point(myMenuButton.Left, myMenuButton.Bottom)));
         }
      }

      protected override void OnForeColorChanged(EventArgs e)
      {
         Renderer = new InnerRenderer(new InnerColorTable(this));

         base.OnForeColorChanged(e);
      }

      protected override void OnBackColorChanged(EventArgs e)
      {
         Renderer = new InnerRenderer(new InnerColorTable(this));

         base.OnBackColorChanged(e);
      }

      protected override void OnTextChanged(EventArgs e)
      {
         base.OnTextChanged(e);

         PpMenuButton?.MthInvoke(() => PpMenuButton.Text = Text);
      }

      private void MyMenuButton_Click(object? sender, EventArgs e) => MthShow();

      private void DockMenuStrip_Closed(object? sender, ToolStripDropDownClosedEventArgs e) => myMenuButton?.MthInvoke(() => myMenuButton.BackColor = myColorSave);

      private void DockMenuStrip_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
      {
         foreach (var itm in myGetItemsRecursively(Items.OfType<ToolStripMenuItem>().ToArray())) { itm.ForeColor = ForeColor; }

         if (PpBackColorSelected != Color.Empty && myMenuButton != null)
         {
            myMenuButton.MthInvoke(() =>
            {
               myColorSave = myMenuButton.BackColor;
               myMenuButton.BackColor = PpBackColorSelected;
            });
         }
      }

      private ToolStripMenuItem[] myGetItemsRecursively(ToolStripMenuItem[] items) =>
         items.Concat(items.SelectMany(i => myGetItemsRecursively(i.DropDownItems.Cast<ToolStripMenuItem>().ToArray()))).ToArray();
   }
}
