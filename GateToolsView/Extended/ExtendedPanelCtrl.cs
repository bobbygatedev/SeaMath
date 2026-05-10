using Gate.Tools.Extensions;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms.Design;
using System.Windows.Forms.Layout;

namespace Gate.ToolsView.Extended
{
   public delegate void OnScrollBarMovedHandler(object? sender, ExtendedPanelScrollBarMovedArgs scrollBarMovedArgs);

   [Designer(typeof(Designer))]
   public partial class ExtendedPanelCtrl : UserControl
   {
      /// <summary>
      /// Raised after panel has been scrolled.
      /// </summary>
      public event ScrollEventHandler? OnScrolled;

      /// </summary>
      public event OnScrollBarMovedHandler? OnScrollBarMoved;

      private int myBorderPixels = 2;
      private Color myBorderColor = Color.DarkGray;
      private bool myIsScrollVBarActive = false;
      private bool myIsScrollHBarActive = false;
      private bool myIsAutoScrollActive = false;
      private int myScrollBarsSize = 20;
      private Color myBackColor = Color.Gray;
      private Control? mySingleControlBased = null;
      private Func<Control, Size>? mySingleControlContentSizeCalculator = null;
      private int myScrollHValue = 0;
      private int myScrollVValue = 0;

      public ExtendedPanelCtrl()
      {
         InitializeComponent();
         SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
         SetStyle(ControlStyles.ResizeRedraw, true);
         CtrlPanelFather.BackColor = CtrlPanelContent.BackColor = myBackColor = base.BackColor;
         CtrlScrollBarH.Value = myScrollHValue;
         CtrlScrollBarV.Value = myScrollVValue;
         base.BackColor = PpBorderColor;
         PerformLayout();
      }

      internal class Designer : ParentControlDesigner
      {
         public override void Initialize(IComponent component)
         {
            base.Initialize(component);

            if (Control is ExtendedPanelCtrl)
            {
               var par = (ExtendedPanelCtrl)Control;

               EnableDesignMode(par.PpPanel, "PpPanel");
            }
         }
      }

      public bool IsHScrollPossible => PpContentPreferredSize.Width > Width - 2 * PpBorderWidth - PpScrollBarsSize;

      public bool IsVScrollPossible => PpContentPreferredSize.Height > Height - 2 * PpBorderWidth - PpScrollBarsSize;

      /// <summary>
      /// Gets the width of the screen, adjusted for the border width.
      /// </summary>
      public int PpScreenWidth => Width - 2 * PpBorderWidth;

      /// <summary>
      /// Gets the height of the screen, adjusted to account for the border width.
      /// </summary>
      public int PpScreenHeight => Height - 2 * PpBorderWidth;

      public bool PpIsScrollVBarVisible => PpIsAutoScrollActive ? IsVScrollPossible : PpIsScrollVBarActive;

      public bool PpIsScrollHBarVisible => PpIsAutoScrollActive ? IsHScrollPossible : PpIsScrollHBarActive;

      public int PpContentWidth => PpIsScrollVBarVisible ? PpScreenWidth - PpScrollBarsSize : PpScreenWidth;

      public int PpContentHeight => PpIsScrollHBarVisible ? PpScreenHeight - PpScrollBarsSize : PpScreenHeight;

      public int PpMaxScrollHValue => PpContentPreferredSize.Width - PpContentWidth;

      public int PpMaxScrollVValue => PpContentPreferredSize.Height - PpContentHeight;

      private class InnerLayoutEngine : LayoutEngine
      {
         public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
         {
            //parent control
            var ctr = (ExtendedPanelCtrl)container;
            var cnt_prf_siz = ctr.PpContentPreferredSize;
            var cnt_w = ctr.PpContentWidth;
            var cnt_h = ctr.PpContentHeight;

            ctr.CtrlPanelFather.Location = new Point(ctr.PpBorderWidth, ctr.PpBorderWidth);
            ctr.CtrlPanelFather.Size = new Size(cnt_w, cnt_h);
            ctr.CtrlPanelContent.Size = new Size(Math.Max(cnt_prf_siz.Width, cnt_w), Math.Max(cnt_prf_siz.Height, cnt_h));
            ctr.CtrlPanelContent.Location = new Point(-ctr.myScrollHValue, -ctr.myScrollVValue);

            if (ctr.PpSingleControlBased != null) { ctr.PpSingleControlBased.Size = ctr.CtrlPanelContent.Size; }

            if (ctr.CtrlScrollBarH.Visible = ctr.PpIsScrollHBarVisible)
            {
               var d = ctr.PpMaxScrollHValue;

               ctr.CtrlScrollBarH.Left = ctr.PpBorderWidth;
               ctr.CtrlScrollBarH.Top = ctr.Height - ctr.PpScrollBarsSize - ctr.PpBorderWidth;
               ctr.CtrlScrollBarH.Width = ctr.PpScreenWidth;
               ctr.CtrlScrollBarH.Minimum = 0;
               ctr.CtrlScrollBarH.Maximum = d > 0 ? d : 100;
               ctr.CtrlScrollBarH.Enabled = d > 0;
            }

            if (ctr.CtrlScrollBarV.Visible = ctr.PpIsScrollVBarVisible)
            {
               var d = ctr.PpMaxScrollVValue;

               ctr.CtrlScrollBarV.Left = ctr.Width - ctr.PpScrollBarsSize - ctr.PpBorderWidth;
               ctr.CtrlScrollBarV.Top = ctr.PpBorderWidth;
               ctr.CtrlScrollBarV.Height = ctr.PpScreenHeight;
               ctr.CtrlScrollBarV.Minimum = 0;
               ctr.CtrlScrollBarV.Maximum = d > 0 ? d : 100;
               ctr.CtrlScrollBarV.Enabled = d > 0;
            }

            ctr.BackColor = ctr.PpBorderColor;
            ctr.Refresh();

            return false;
         }
      }

      /// <summary>
      /// Returns the content size ie:
      /// - PpSingleControlBased is not null
      ///   - PpSingleControlBased.PreferredSize if PpSingleControlContentSizeCalculator is null
      ///   - PpSingleControlContentSizeCalculator(PpSingleControlBased)
      /// - CtrlPanelContent.PreferredSize PpSingleControlBased is null
      /// </summary>
      /// <param name="ctr"></param>
      /// <returns></returns>
      [Browsable(false)]
      public Size PpContentPreferredSize => PpSingleControlBased != null ?
               PpSingleControlContentSizeCalculator != null ?
                  PpSingleControlContentSizeCalculator.Invoke(this) : PpSingleControlBased.PreferredSize :
               CtrlPanelContent.PreferredSize;

      /// <summary>
      /// 
      /// </summary>
      [Browsable(false)]
      public int PpScrollHValue
      {
         get => myScrollHValue;

         set
         {
            if (IsHScrollPossible)
            {
               myScrollHValue = value;
            }
            else
            {
               myScrollHValue = 0;
            }

            PerformLayout();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      [Browsable(false)]
      public int PpScrollVValue
      {
         get => myScrollVValue;

         set
         {
            if (IsVScrollPossible)
            {
               myScrollVValue = value;
            }
            else
            {
               myScrollVValue = 0;
            }

            PerformLayout();
         }
      }
      /// <summary>
      /// 
      /// </summary>
      [Category("Appearance")]
      public int PpBorderWidth
      {
         get => myBorderPixels;
         set
         {
            myBorderPixels = value;
            PerformLayout();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      [Category("Appearance")]
      public Color PpBorderColor
      {
         get => myBorderColor;

         set
         {
            myBorderColor = value;
            PerformLayout();
         }
      }

      [Category("Appearance")]
      public bool PpIsAutoScrollActive
      {
         get => myIsAutoScrollActive;

         set
         {
            myIsAutoScrollActive = value;
            PerformLayout();
         }
      }

      [Category("Appearance")]
      public bool PpIsScrollVBarActive
      {
         get => myIsScrollVBarActive;

         set
         {
            myIsScrollVBarActive = value;
            PerformLayout();
         }
      }

      [Category("Appearance")]
      public bool PpIsScrollHBarActive
      {
         get => myIsScrollHBarActive;

         set
         {
            CtrlScrollBarH.Visible = myIsScrollHBarActive = value;
            PerformLayout();
            Refresh();
         }
      }

      [Category("Appearance")]
      public int PpScrollBarsSize
      {
         get => myScrollBarsSize;

         set
         {
            CtrlScrollBarV.Width = CtrlScrollBarH.Height = myScrollBarsSize = value;
            PerformLayout();
         }
      }

      public Panel PpPanel => CtrlPanelContent;

      [Browsable(false)]
      public override LayoutEngine LayoutEngine => new InnerLayoutEngine();

      /// <summary>
      /// Always false.
      /// </summary>
      [Browsable(false)]
      public override bool AutoSize { get => false; set { } }

      /// <summary>
      /// Always false.
      /// </summary>
      [Browsable(false)]
      public override bool AutoScroll { get => false; set { } }

      [Category("Appearance")]
      [Description("Back color of horizontal scroll bar.")]
      [DefaultValue(typeof(Color), "Black")]
      public Color PpScrollHBackColor
      {
         get => CtrlScrollBarH.BackColor;
         set => CtrlScrollBarH.BackColor = value;
      }

      [Category("Appearance")]
      [Description("Border color in disabled state.")]
      [DefaultValue(typeof(Color), "Black")]
      public Color PpScrollHArrowColor
      {
         get => CtrlScrollBarH.PpArrowColor;
         set => CtrlScrollBarH.PpArrowColor = value;
      }

      /// <summary>
      /// Border color.
      /// </summary>
      [Category("Appearance")]
      [Description("Border color.")]
      [DefaultValue(typeof(Color), "93, 140, 201")]
      public Color PpScrollHBorderColor
      {
         get => CtrlScrollBarH.PpBorderColor;
         set => CtrlScrollBarH.PpBorderColor = value;
      }

      /// <summary>
      /// Border color in disabled state.
      /// </summary>
      [Category("Appearance")]
      [Description("Border color in disabled state.")]
      [DefaultValue(typeof(Color), "Gray")]
      public Color PpScrollHDisabledBorderColor
      {
         get => CtrlScrollBarH.PpDisabledBorderColor;
         set => CtrlScrollBarH.PpDisabledBorderColor = value;
      }

      [Category("Appearance")]
      [Description("Solid grip back color when active (pressed or selected).")]
      [DefaultValue(typeof(Color), "DarkGray")]
      public Color PpScrollHGripActiveColor
      {
         get => CtrlScrollBarH.PpGripActiveColor;
         set => CtrlScrollBarH.PpGripActiveColor = value;
      }

      [Category("Appearance")]
      [Description("Solid grip back color.")]
      [DefaultValue(typeof(Color), "DarkGray")]
      public Color PpScrollHGripColor
      {
         get => CtrlScrollBarH.PpGripColor;
         set => CtrlScrollBarH.PpGripColor = value;
      }

      /// <summary>
      /// Border color is to draw.
      /// </summary>
      [Category("Appearance")]
      [Description("Border color in disabled state.")]
      [DefaultValue(false)]
      public bool PpScrollHIsBorderToDraw
      {
         get => CtrlScrollBarH.PpIsBorderToDraw;
         set => CtrlScrollBarH.PpIsBorderToDraw = value;
      }

      [Category("Appearance")]
      [Description("Back color of vertical scroll bar.")]
      [DefaultValue(typeof(Color), "Black")]
      public Color PpScrollVBackColor
      {
         get => CtrlScrollBarV.BackColor;
         set => CtrlScrollBarV.BackColor = value;
      }

      [Category("Appearance")]
      [Description("Border color in disabled state.")]
      [DefaultValue(typeof(Color), "Black")]
      public Color PpScrollVArrowColor
      {
         get => CtrlScrollBarV.PpArrowColor;
         set => CtrlScrollBarV.PpArrowColor = value;
      }

      /// <summary>
      /// Border color.
      /// </summary>
      [Category("Appearance")]
      [Description("Border color.")]
      [DefaultValue(typeof(Color), "93, 140, 201")]
      public Color PpScrollVBorderColor
      {
         get => CtrlScrollBarV.PpBorderColor;
         set => CtrlScrollBarV.PpBorderColor = value;
      }

      /// <summary>
      /// Border color in disabled state.
      /// </summary>
      [Category("Appearance")]
      [Description("Border color in disabled state.")]
      [DefaultValue(typeof(Color), "Gray")]
      public Color PpScrollVDisabledBorderColor
      {
         get => CtrlScrollBarV.PpDisabledBorderColor;

         set => CtrlScrollBarV.PpDisabledBorderColor = value;
      }

      [Category("Appearance")]
      [Description(" the solid grip back color when active (pressed or selected).")]
      [DefaultValue(typeof(Color), "DarkGray")]
      public Color PpScrollVGripActiveColor
      {
         get => CtrlScrollBarV.PpGripActiveColor;
         set => CtrlScrollBarV.PpGripActiveColor = value;
      }

      [Category("Appearance")]
      [Description(" the solid grip back color.")]
      [DefaultValue(typeof(Color), "DarkGray")]
      public Color PpScrollVGripColor
      {
         get => CtrlScrollBarV.PpGripColor;
         set => CtrlScrollBarV.PpGripColor = value;
      }

      /// <summary>
      /// Border color is to draw.
      /// </summary>
      [Category("Appearance")]
      [Description("Border color in disabled state.")]
      [DefaultValue(false)]
      public bool PpScrollVIsBorderToDraw
      {
         get => CtrlScrollBarV.PpIsBorderToDraw;
         set => CtrlScrollBarV.PpIsBorderToDraw = value;
      }

      /// <summary>
      /// 
      /// </summary>
      public Control? PpSingleControlBased
      {
         get => mySingleControlBased;
         set
         {
            foreach (var ctr in PpPanel.Controls.Cast<Control>()) { ctr.Parent = null; }

            if ((mySingleControlBased = value) != null)
            {
               PpPanel.Controls.Add(mySingleControlBased);
               mySingleControlBased.Location = new Point();//origin
            }

            PerformLayout();
         }
      }

      /// <summary>
      /// <br>  client size calculator, </br>
      /// <br> if single control based is not null client size is calculated using calculator </br>
      /// <br> otherwise PpSingleControlBased.PreferredSize is get. </br>
      /// </summary>
      [Browsable(false)]
      public Func<Control, Size>? PpSingleControlContentSizeCalculator
      {
         get => mySingleControlContentSizeCalculator;
         set
         {
            mySingleControlContentSizeCalculator = value;
            PerformLayout();
         }
      }
      /// <summary>
      /// 
      /// </summary>
      public override Color BackColor
      {
         get => myBackColor;
         set => CtrlPanelFather.BackColor = CtrlPanelContent.BackColor = myBackColor = value;
      }

      private void CtrlScrollBarAny_Scroll(object? sender, ScrollEventArgs e)
      {
         var h_val = CtrlScrollBarH.Value;
         var v_val = CtrlScrollBarV.Value;

         var ars = new ExtendedPanelScrollBarMovedArgs(e, h_val, v_val);

         myActionOnScrollBarMoved(sender, ars);
      }

      protected virtual void myActionOnScrollBarMoved(object? sender, ExtendedPanelScrollBarMovedArgs e)
      {
         OnScrollBarMoved?.Invoke(sender, e);

         if (!e.IsCancel)
         {
            myScrollHValue = e.ScrollHValue;
            myScrollVValue = e.ScrollVValue;
            myActionOnScroll(sender, e.ScrollEventArgs);
         }

         myActionOnScroll(sender, e.ScrollEventArgs);
      }

      protected virtual void myActionOnScroll(object? sender, ScrollEventArgs e)
      {
         PerformLayout();
         OnScrolled?.Invoke(sender, e);
      }

      protected override void OnForeColorChanged(EventArgs e)
      {
         CtrlPanelContent.ForeColor = ForeColor;

         base.OnForeColorChanged(e);
      }

      private void myActionOnControlAddedRemoved()
      {
         myScrollHValue = myScrollVValue = 0;
         CtrlScrollBarH.Value = CtrlScrollBarV.Value = 0;
         CtrlPanelContent.Location = new Point();
         PerformLayout();
      }

      private void CtrlPanel_ControlAdded(object? sender, ControlEventArgs e) => myActionOnControlAddedRemoved();

      private void CtrlPanel_ControlRemoved(object? sender, ControlEventArgs e) => myActionOnControlAddedRemoved();

      private void ExtendedPanelCtrl_ControlAdded(object? sender, ControlEventArgs e) => e.Control.NnOrCrash().Parent = CtrlPanelContent;

      private void CtrlPanelContent_Layout(object? sender, LayoutEventArgs e) => PerformLayout();

      private void CtrlPanelContent_Resize(object? sender, EventArgs e) => PerformLayout();
   }
}

