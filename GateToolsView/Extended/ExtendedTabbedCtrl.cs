using Gate.Tools;
using Gate.ToolsView.Dockable;
using System.Data;
using System.Windows.Forms.Layout;

namespace Gate.ToolsView.Extended
{
   /// <summary>
   /// Tabbed control user can make visible one control a time, allows multiselection (by pressing CTRL).
   /// </summary>
   public partial class ExtendedTabbedCtrl : UserControl
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="newTabIdx"></param>
      public delegate void OnSelectedTabChangedHandler(object? sender, int newTabIdx, Control? newTabbedControl);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="tabIdx"></param>
      /// <param name="controlInTab"></param>
      public delegate void OnAskForDraggingHandler(object? sender, int tabIdx, Control controlInTab);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="tabIdx"></param>
      /// <param name="controlInTab"></param>
      public delegate void OnAskForTabCloseHandler(object? sender, int tabIdx, Control controlInTab);

      /// <summary>
      /// 
      /// </summary>
      public event OnSelectedTabChangedHandler? OnSelectedTabChanged;

      /// <summary>
      /// 
      /// </summary>
      public event OnAskForDraggingHandler? OnAskForDragging;

      /// <summary>
      /// 
      /// </summary>
      public event OnAskForTabCloseHandler? OnAskForTabClose;

      private readonly List<InnerControlHolder> myListControlHolder = new List<InnerControlHolder>();
      private int myTabVisibleIdx = -1;
      private ButtonsPosEnum myButtonsPos = ButtonsPosEnum.down;
      private bool myHasCloseButton = false;
      private Color myButtonBackColorSelected = Color.FromArgb(0, 30, 170);
      private Color myButtonBackColorVisible = Color.FromArgb(0, 50, 255);
      private Color myButtonForeColorSelected = Color.FromArgb(255, 255, 255);
      private Color myButtonForeColorVisible = Color.FromArgb(255, 255, 255);

      /// <summary>
      /// 
      /// </summary>
      public enum ButtonsPosEnum
      {
         /// <summary>
         /// 
         /// </summary>
         up = 0,

         /// <summary>
         /// 
         /// </summary>
         down
      }

      /// <summary>
      /// Constructor.
      /// </summary>
      public ExtendedTabbedCtrl()
      {
         InitializeComponent();
         SetStyle(ControlStyles.ResizeRedraw, true);
      }

      private class InnerLayoutEngine : LayoutEngine
      {
         public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
         {
            var par = (ExtendedTabbedCtrl)container;

            //paint tab's (dock button)
            foreach (var cth in par.myListControlHolder)
            {
               var but = cth.Button;

               cth.Control.Visible = par.myListControlHolder.IndexOf(cth) == par.PpTabVisibleIdx;

               if (par.myListControlHolder.IndexOf(cth) == par.PpTabVisibleIdx)
               {
                  but.ForeColor = par.PpButtonForeColorVisible;
                  but.BackColor = par.PpButtonBackColorVisible;
               }
               else if (par.PpTabsSelected.Contains(cth.Control))
               {
                  but.ForeColor = par.PpButtonForeColorSelected;
                  but.BackColor = par.PpButtonBackColorSelected;
               }
               else
               {
                  but.ForeColor = par.ForeColor;
                  but.BackColor = par.BackColor;
               }

               but.Margin = new Padding();

               if (par.PpIsFixedTabHeightToUse)
               {
                  but.PerformLayout();
                  but.Width = but.PreferredSize.Width;
                  but.Height = par.PpFixedTabHeight;
               }
               else { but.Size = but.PreferredSize; }
            }

            var dr = par.DisplayRectangle;

            par.CtrlFlowLayoutPanel.Width = dr.Width;
            par.CtrlFlowLayoutPanel.PerformLayout();
            par.CtrlFlowLayoutPanel.Height = par.CtrlFlowLayoutPanel.Controls.Count > 0 ?
                  par.CtrlFlowLayoutPanel.Controls.Cast<Control>().Max(c => c.Bottom) :
                  Math.Min(20, par.PpFixedTabHeight);
            par.MinimumSize = new Size(0, par.CtrlFlowLayoutPanel.Height);
            par.CtrlPanel.Size = new Size(par.Width, par.Height - par.CtrlFlowLayoutPanel.Height);

            switch (par.PpButtonsPos)
            {
               case ButtonsPosEnum.up:
                  par.CtrlPanel.Location = new Point(0, par.CtrlFlowLayoutPanel.Height);
                  par.CtrlFlowLayoutPanel.Location = new Point(0, 0);
                  break;

               case ButtonsPosEnum.down:
                  par.CtrlPanel.Location = new Point(0, 0);
                  par.CtrlFlowLayoutPanel.Location = new Point(0, par.CtrlPanel.Height);
                  break;

               default: throw new Crash();
            }

            return false;
         }
      }

      private class InnerControlHolder
      {
         private readonly ExtendedTabbedCtrl myTabbedControl;

         public InnerControlHolder(Control control, ExtendedTabbedCtrl tabbedControl)
         {
            SaveDockState = control.Dock;
            control.Dock = DockStyle.Fill;
            Control = control;
            Control.Visible = false;
            myTabbedControl = tabbedControl;
            myTabbedControl.CtrlPanel.Controls.Add(Control);
            Button = new DockableTabbedCtrlSelectButtonCtrl();
            Button.PpText = "NewTab";
            Button.AutoSize = false;
            Button.PpHasCloseButton = tabbedControl.PpHasCloseButton;
         }

         public Control Control { get; private set; }

         public DockableTabbedCtrlSelectButtonCtrl Button { get; private set; }

         public DockStyle SaveDockState { get; }
      }

      /// <summary>
      /// 
      /// </summary>
      public ButtonsPosEnum PpButtonsPos
      {
         get => myButtonsPos;
         set
         {
            myButtonsPos = value;
            PerformLayout();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public bool PpHasCloseButton
      {
         get => myHasCloseButton;
         set
         {
            myHasCloseButton = value;

            foreach (var hld in myListControlHolder) { hld.Button.PpHasCloseButton = value; }
         }
      }

      /// <summary>
      ///  the only visible tab. 
      /// </summary>
      public Control? PpTabVisible
      {
         get => PpTabVisibleIdx >= 0 && PpTabVisibleIdx < myListControlHolder.Count ? myListControlHolder[PpTabVisibleIdx].Control : null;

         set
         {
            var ctr_h = myListControlHolder.FirstOrDefault(h => h.Control == value);

            if (ctr_h != null)
            {
               PpTabVisibleIdx = myListControlHolder.IndexOf(ctr_h);
            }
         }
      }

      /// <summary>
      /// <br>Array of selected tabs</br>
      /// <br>while user keep CTRL pressed, tabs becoming visible are enqueued to selected tab array.</br>
      /// <br>otherwise array includes visible tab only.</br> 
      /// </summary>
      public Control[] PpTabsSelected { get; private set; } = new Control[0];

      /// <summary>
      /// Array of control tabs.
      /// </summary>
      public Control[] PpTabs => myListControlHolder.Select(h => h.Control).ToArray();

      /// <summary>
      /// Array of tab buttons.
      /// </summary>
      public DockableTabbedCtrlSelectButtonCtrl[] PpTabButtons => myListControlHolder.Select(h => h.Button).ToArray();

      /// <summary>
      ///  button back color when visible.
      /// </summary>
      public Color PpButtonBackColorVisible
      {
         get => myButtonBackColorVisible;
         set
         {
            myButtonBackColorVisible = value;
            PerformLayout();
         }
      }

      /// <summary>
      ///  button foreground color when visible.
      /// </summary>
      public Color PpButtonForeColorVisible
      {
         get => myButtonForeColorVisible;
         set
         {
            myButtonForeColorVisible = value;
            PerformLayout();
         }
      }

      /// <summary>
      ///  button back color when selected.
      /// </summary>
      public Color PpButtonBackColorSelected
      {
         get => myButtonBackColorSelected;
         set 
         {
            myButtonBackColorSelected = value;
            PerformLayout();
         }
      }

      /// <summary>
      ///  button foreground color when selected.
      /// </summary>
      public Color PpButtonForeColorSelected
      {
         get => myButtonForeColorSelected;
         set
         {
            myButtonForeColorSelected = value;
            PerformLayout();
         }
      }

      /// <summary>
      ///  if is true PpFixedTabHeight becomes tab height, otherwise it's tab height equal to button.PreferredSize
      /// </summary>
      public bool PpIsFixedTabHeightToUse { get; set; } = false;

      /// <summary>
      /// 
      /// </summary>
      public int PpFixedTabHeight { get; set; } = 20;

      /// <summary>
      /// 
      /// </summary>
      public int PpTabVisibleIdx
      {
         get => myTabVisibleIdx;

         set => myDoSetTabVisibleIdx(value, false);
      }

      /// <summary>
      /// Clears multiselcetion ie set selected tabs to {visible tab}.
      /// </summary>
      public void MthClearMultiSelection()
      {
         PpTabsSelected = PpTabVisible != null ? new Control[] { PpTabVisible } : new Control[0];
         PerformLayout();
      }

      /// <summary>
      /// Does set value visible index 
      /// </summary>
      /// <param name="visibleIdx"></param>
      /// <param name="isKeepSelected"></param>
      private void myDoSetTabVisibleIdx(int visibleIdx, bool isKeepSelected)
      {
         if (visibleIdx < 0)
         {
            myTabVisibleIdx = -1;
         }
         else
         {
            myTabVisibleIdx = PpTabs.Length == 0 ? -1 : Math.Max(0, Math.Min(visibleIdx, PpTabs.Length - 1));
            myListControlHolder[myTabVisibleIdx].Control.Focus();
         }

         if (isKeepSelected)
         {
            if (PpTabVisible != null && !PpTabsSelected.Contains(PpTabVisible))
            {
               PpTabsSelected = PpTabsSelected.Append(PpTabVisible).ToArray();
            }
         }
         else
         {
            PpTabsSelected = PpTabVisible != null ? new Control[] { PpTabVisible } : new Control[0];
         }

         PerformLayout();
         myActionSelectedTabChanged(this, myTabVisibleIdx);
      }

      /// <summary>
      /// 
      /// </summary>
      public override LayoutEngine LayoutEngine => new InnerLayoutEngine();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="control"></param>
      public void MthControlAdd(Control control)
      {
         if (!myListControlHolder.Any(h => h.Control == control))
         {
            var new_h = new InnerControlHolder(control, this);

            new_h.Button.Height = PpFixedTabHeight;
            new_h.Button.OnAskForSelect += Button_OnAskForSelect;
            new_h.Button.OnAskForClose += Button_OnAskForClose;
            new_h.Button.OnAskForDragging += Button_OnAskForDragging;
            CtrlFlowLayoutPanel.Controls.Add(new_h.Button);
            myListControlHolder.Add(new_h);
            myDoSetTabVisibleIdx(myListControlHolder.Count - 1, false);
         }
      }

      /// <summary>
      /// Removes a control from tabbed control.
      /// </summary>
      /// <param name="control"></param>
      public void MthControlRemove(Control control)
      {
         if (myListControlHolder.Any(h => h.Control == control))
         {
            var h2r = myListControlHolder.First(h => h.Control == control);
            var idx_2_rem = myListControlHolder.IndexOf(h2r);
            var cnt = myListControlHolder.Count;
            var new_vis_idx = -1;

            if (myListControlHolder.Count > 1)
            {
               new_vis_idx = idx_2_rem != PpTabVisibleIdx ?
                  (idx_2_rem < PpTabVisibleIdx ? PpTabVisibleIdx - 1 : PpTabVisibleIdx) :
                  (idx_2_rem == cnt - 1 ? cnt - 2 : idx_2_rem);
            }

            CtrlFlowLayoutPanel.Controls.Remove(h2r.Button);
            h2r.Button.OnAskForSelect -= Button_OnAskForSelect;
            h2r.Button.OnAskForClose -= Button_OnAskForClose;
            h2r.Button.OnAskForDragging -= Button_OnAskForDragging;
            CtrlPanel.Controls.Remove(control);
            control.Dock = h2r.SaveDockState;
            control.Size = Size;
            control.Parent = null;
            control.Visible = true;
            myListControlHolder.Remove(h2r);
            myDoSetTabVisibleIdx(new_vis_idx, false);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="control"></param>
      /// <returns></returns>
      /// <exception cref="System.IndexOutOfRangeException"></exception>
      public DockableTabbedCtrlSelectButtonCtrl MthGetButton(Control control) => PpTabButtons[PpTabs.ToList().IndexOf(control)];

      protected virtual void myActionSelectedTabChanged(object? sender, int newTabIdx) => 
         OnSelectedTabChanged?.Invoke(sender, newTabIdx, newTabIdx != -1 ? myListControlHolder[newTabIdx].Control : null);

      private void Button_OnAskForDragging(object? sender)
      {
         var but = sender as DockableTabbedCtrlSelectButtonCtrl ?? throw new Crash();
         var tab_idx = myListControlHolder.Select(h => h.Button).ToList().IndexOf(but);//not necessarily selected one

         OnAskForDragging?.Invoke(this, tab_idx, myListControlHolder[tab_idx].Control);
      }

      private void Button_OnAskForClose(object? sender)
      {
         if (OnAskForTabClose != null)
         {
            var but = sender as DockableTabbedCtrlSelectButtonCtrl ?? throw new Crash();
            var tab_idx = myListControlHolder.Select(h => h.Button).ToList().IndexOf(but);//not necessarily selected one

            OnAskForTabClose.Invoke(this, tab_idx, PpTabs[tab_idx]);
         }
      }

      private void Button_OnAskForSelect(object? sender) => 
         myDoSetTabVisibleIdx(
            myListControlHolder.IndexOf(myListControlHolder.First(h => h.Button == sender)),
            ModifierKeys == Keys.Control);
   }
}
