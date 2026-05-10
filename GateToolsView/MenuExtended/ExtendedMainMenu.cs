using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.Extended;
using Gate.ToolsView.MenuCommand;
using System.Data;

namespace Gate.ToolsView.MenuExtended
{
   /// <summary>
   /// 
   /// </summary>
   public partial class ExtendedMainMenu : UserControl, IExtendedMenuAspect
   {
      private Color myBackColorMargin = Color.Empty;
      private Color myBackColorSelected = Color.Empty;
      private Color myBorderColor = Color.Empty;
      private Color myItemBorderColor = Color.Empty;
      private Color myBackColorDropDown = Color.Empty;
      private Color myCheckBoxBackground = Color.Empty;
      private CmdMainMenu? myCmdMainMenu = null;
      private readonly List<ExtendedMenuDropDown.ForMainMenu> myListMenuDropDowns = new List<ExtendedMenuDropDown.ForMainMenu>();

      public ExtendedMainMenu()
      {
         //needed for correct working of 
         Application.EnableVisualStyles();

         InitializeComponent();
      }

      /// <summary>
      /// 
      /// </summary>
      public ExtendedMenuDropDown.ForMainMenu[] PpMenuDropDowns => myListMenuDropDowns.ToArray();

      /// <summary>
      /// 
      /// </summary>
      public ExtendedButtonCtrl[] PpMenuButtons => myListMenuDropDowns.Select(m => m.PpMenuButton as ExtendedButtonCtrl).Nn().ToArray();

      /// <summary>
      ///  always true (menu size calculated by system).
      /// </summary>
      public override bool AutoSize { get => true; set => base.AutoSize = true; }

      /// <summary>
      /// 
      /// </summary>
      public CmdMainMenu? PpCmdMainMenu
      {
         get => myCmdMainMenu;
         set
         {
            if (myCmdMainMenu != null) { myCmdMainMenu.ControlAssociation = null; }

            if ((myCmdMainMenu = value) != null)
            {
               myCmdMainMenu.ControlAssociation = new InnerCmdMainMenuControlAssociation(this, myCmdMainMenu);
            }
         }
      }

      /// <summary>
      ///  the currently open dropdown 
      /// </summary>
      public ExtendedMenuDropDown.ForMainMenu? PpMenuDropDownOpened { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public Button[] PpVisibleButtons => PpMenuButtons.Where(b => b.Visible).ToArray();

      public Size PpPreferredSize => PpVisibleButtons.Length > 0 ?
         new Size(PpVisibleButtons.Last().Right + PpButtonMargin, PpVisibleButtons.Last().Height) :
         new Size(20, 20);

      public int PpMinButtonSize { get; set; } = 10;

      /// <summary>
      ///  button margin distance (both left and right) from button text to margin. 
      /// </summary>
      public int PpButtonMargin { get; set; } = 5;

      /// <summary>
      /// 
      /// </summary>
      public Color PpBackColorMargin
      {
         get => myBackColorMargin;
         set
         {
            myBackColorMargin = value;
            PerformLayout();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public Color PpBackColorSelected
      {
         get => myBackColorSelected;
         set
         {
            myBackColorSelected = value;
            PerformLayout();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public Color PpBorderColor
      {
         get => myBorderColor;
         set
         {
            myBorderColor = value;
            PerformLayout();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public Color PpBackColorDropDown
      {
         get => myBackColorDropDown;
         set
         {
            myBackColorDropDown = value;
            PerformLayout();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public Color PpItemBorderColor
      {
         get => myItemBorderColor;
         set
         {
            myItemBorderColor = value;
            PerformLayout();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public Color PpCheckBoxBackground
      {
         get => myCheckBoxBackground;
         set
         {
            myCheckBoxBackground = value;
            PerformLayout();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="menuStrip"></param>
      /// <param name="atIndex"></param>
      /// <exception cref="NotImplementedException"></exception>
      public ExtendedMenuDropDown MthInsertMenuStrip(int atIndex)
      {
         var ddm = new ExtendedMenuDropDown.ForMainMenu(this);

         ddm.PpMenuButton = new ExtendedButtonCtrl();
         ddm.PreviewKeyDown += MenuStrip_PreviewKeyDown;
         ddm.Opened += MenuStrip_Opened;
         ddm.PpMenuButton.TextChanged += MenuButton_TextChanged;
         ddm.PpMenuButton.VisibleChanged += PpMenuButton_VisibleChanged;
         myListMenuDropDowns.Insert(atIndex < 0 ? myListMenuDropDowns.Count : atIndex, ddm);
         PerformLayout();

         return ddm;
      }

      private void PpMenuButton_VisibleChanged(object? sender, EventArgs e) => PerformLayout();


      private void MenuButton_TextChanged(object? sender, EventArgs e) => PerformLayout();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="menuStrip"></param>
      public ExtendedMenuDropDown MthAddMenuStrip() => MthInsertMenuStrip(myListMenuDropDowns.Count);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="menuStrip"></param>
      /// <returns></returns>
      public bool MthRemoveMenuStrip(ExtendedMenuDropDown.ForMainMenu menuStrip)
      {
         if (myListMenuDropDowns.Contains(menuStrip))
         {
            menuStrip.PreviewKeyDown -= MenuStrip_PreviewKeyDown;

            if (menuStrip.PpMenuButton != null)
            {
               menuStrip.PpMenuButton.TextChanged -= MenuButton_TextChanged;
               menuStrip.PpMenuButton.VisibleChanged += PpMenuButton_VisibleChanged;
            }

            myListMenuDropDowns.Remove(menuStrip);
            PerformLayout();

            return true;
         }

         return false;
      }

      public override Size GetPreferredSize(Size proposedSize) =>
        new Size(PpPreferredSize.Width, proposedSize.Height > 0 ? proposedSize.Height : PpPreferredSize.Height);

      protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
      {
         //  Only when the size is affected...
         if (AutoSize && (specified & BoundsSpecified.Size) != 0)
         {
            var lef = 0;

            foreach (var but in PpVisibleButtons)
            {
               var sz = TextRenderer.MeasureText(but.Text, Font);
               var w = Math.Max(PpMinButtonSize, sz.Width + 2 * PpButtonMargin);

               but.Left = lef;
               but.Top = 0;
               but.Width = (int)w;
               but.Height = height = Math.Max(sz.Height + 2 * PpButtonMargin, height);
               lef += but.Width;
            }

            width = lef;
         }

         base.SetBoundsCore(x, y, width, height, specified);
      }

      protected override void OnForeColorChanged(EventArgs e)
      {
         base.OnForeColorChanged(e);
         PerformLayout();
      }

      protected override void OnFontChanged(EventArgs e)
      {
         base.OnFontChanged(e);
         PerformLayout();
      }

      protected override void OnBackColorChanged(EventArgs e)
      {
         base.OnBackColorChanged(e);
         PerformLayout();
      }

      protected override void OnVisibleChanged(EventArgs e)
      {
         base.OnVisibleChanged(e);
         PerformLayout();
      }

      protected override void OnTextChanged(EventArgs e)
      {
         base.OnTextChanged(e);
         PerformLayout();
      }

      protected override void OnLayout(LayoutEventArgs e)
      {
         var idx = 0;

         foreach (var but in PpMenuButtons.Except(PpVisibleButtons)) { Controls.Remove(but); }

         foreach (var but in PpVisibleButtons)
         {
            if (but.Visible)
            {
               if (but.Parent != this) { Controls.Add(but); }

               but.Refresh();
               Controls.SetChildIndex(but, idx++);
            }
            else { Controls.Remove(but); }
         }

         myDoRefreshStyle();
         base.OnLayout(e);
      }

      private void myDoRefreshStyle()
      {
         for (var i = 0; i < PpMenuDropDowns.Length; i++)
         {
            var but = PpMenuButtons[i];

            if (but != null && but.Visible)
            {
               var ddm = PpMenuDropDowns[i];

               foreach (var pro in typeof(IExtendedMenuAspect).GetProperties())
               {
                  pro.SetValue(ddm, pro.GetValue(this, new object[0]), new object[0]);
               }

               ddm.Font = but.Font = Font;
               ddm.ForeColor = ForeColor;
               ddm.BackColor = PpBackColorDropDown != Color.Empty ? PpBackColorDropDown : BackColor;
               but.FlatAppearance.BorderSize = 0;
               but.ForeColor = ForeColor;
               but.BackColor = BackColor;

               ddm.Opening += (s, e) => but.BackColor = ddm.BackColor;
               ddm.Closed += (s, e) => but.BackColor = BackColor;
            }
         }

         Size = PreferredSize;
         PerformLayout();
      }

      private void myDoShowDropDownIdx(int dropDownIdx)
      {
         PpMenuDropDownOpened?.Close();
         myListMenuDropDowns[dropDownIdx].MthShow();

         if (myListMenuDropDowns[dropDownIdx].Items.Count > 0)
         {
            myListMenuDropDowns[dropDownIdx].Items[0].Select();
         }
      }

      private void MenuStrip_Opened(object? sender, EventArgs e) => PpMenuDropDownOpened = sender as ExtendedMenuDropDown.ForMainMenu;

      private void MenuStrip_PreviewKeyDown(object? sender, PreviewKeyDownEventArgs e)
      {
         if (!e.Alt && !e.Control && !e.Shift)
         {
            int idx = myListMenuDropDowns.IndexOf(sender as ExtendedMenuDropDown.ForMainMenu ?? throw new Crash());
            int inc;

            if (e.KeyData == Keys.Right || e.KeyData == Keys.Tab) { inc = +1; }
            else if (e.KeyData == Keys.Left) { inc = -1; }
            else { return; }

            var nx_idx = idx + inc;
            var sel_itm = myListMenuDropDowns[idx].Items.OfType<ToolStripMenuItem>().FirstOrDefault(i => i.Selected);

            //sub menu item don't cause move, but sub menu opening
            if (inc > 0 && sel_itm?.DropDownItems.Count > 0)
            {
               //retrieve the (hidden) dropdown container which is the sub-menu and add
               //preview-key-down-handler
               var drp_dwn = sel_itm.DropDownItems[0].GetCurrentParent() as ToolStripDropDownMenu;

               if (drp_dwn != null)
               {
                  drp_dwn.Closing += (s, e1) => drp_dwn.PreviewKeyDown -= Drp_dwn_PreviewKeyDown;
                  drp_dwn.PreviewKeyDown += Drp_dwn_PreviewKeyDown;
               }

               return;
            }

            if (nx_idx < 0) { nx_idx += myListMenuDropDowns.Count; }
            else if (nx_idx >= myListMenuDropDowns.Count) { nx_idx = 0; }

            myDoShowDropDownIdx(nx_idx);
         }
      }

      private void Drp_dwn_PreviewKeyDown(object? sender, PreviewKeyDownEventArgs e)
      {
         if (!e.Alt && !e.Control && !e.Shift && e.KeyData == Keys.Right)
         {
            var itm = sender as ToolStripDropDownMenu ?? throw new Crash();
            var sel_itm = itm.Items.Cast<ToolStripMenuItem>().FirstOrDefault(i => i.Selected);

            //sub menu item don't cause move, but sub menu opening
            if (sel_itm?.DropDownItems.Count > 0)
            {
               //retrieve the (hidden) dropdown container which is the sub-menu and add
               //preview-key-down-handler
               var drp_dwn = sel_itm.DropDownItems[0].GetCurrentParent() as ToolStripDropDownMenu;

               if (drp_dwn != null)
               {
                  drp_dwn.Closing += (s, e1) => drp_dwn.PreviewKeyDown -= Drp_dwn_PreviewKeyDown;
                  drp_dwn.PreviewKeyDown += Drp_dwn_PreviewKeyDown;
               }

               return;
            }
            else if (PpMenuDropDownOpened != null)
            {
               var idx = myListMenuDropDowns.IndexOf(PpMenuDropDownOpened) + 1;

               idx = idx >= myListMenuDropDowns.Count ? 0 : myListMenuDropDowns.Count;
               myDoShowDropDownIdx(idx);
            }
         }
      }
   }
}
