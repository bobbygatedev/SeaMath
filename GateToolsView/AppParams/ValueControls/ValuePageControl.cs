using System.Windows.Forms.Layout;
using static Gate.ToolsView.AppParams.ValueControls.ValuesFrameControl;

namespace Gate.ToolsView.AppParams.ValueControls
{
   /// <summary>
   /// Collection of frames containing 'Value' Controls. 
   /// </summary>
   public partial class ValuePageControl : UserControl
   {
      public event OnAddedValueControlHandler? OnAddedValueControl;

      private const int DEFAULT_FIXED_WIDTH = 70;
      private const int MARGIN = 5;

      private int myFixedWidth = DEFAULT_FIXED_WIDTH;

      public ValuePageControl()
      {
         InitializeComponent();
      }

      private class InnerLayout : LayoutEngine
      {
         public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
         {
            var ctr = (ValuePageControl)container;
            var y = 0;
            var pan = ctr.CtrlExtPanel.PpPanel;

            ctr.CtrlExtPanel.Location = new Point(0, 0);
            ctr.CtrlExtPanel.Size = ctr.Size;

            if (ctr.Parent != null)
            {
               ctr.Size = ctr.Parent.Size;
            }

            var prf_siz = ctr.PpPreferredSize;

            foreach (var frm_ctr in ctr.PpAllFrameControls)
            {
               frm_ctr.Left = 0;
               frm_ctr.Top = y;
               frm_ctr.Size = new Size(prf_siz.Width, frm_ctr.PreferredSize.Height);
               y += frm_ctr.Height + MARGIN;
            }

            pan.Size = prf_siz;
          
            return false;
         }
      }

      public override Size GetPreferredSize(Size proposedSize) => PpPreferredSize;

      public Size PpPreferredSize
      {
         get
         {
            var num = PpAllFrameControls.Length;
            var tot_w = num > 0 ? Math.Max(myFixedWidth, PpAllFrameControls.Max(c => c.PreferredSize.Width)) : myFixedWidth;
            var tot_h = num > 0 ? PpAllFrameControls.Sum(c => c.PreferredSize.Height) + MARGIN * (num - 1) : 10;

            return new Size(Math.Max(tot_w, Width), tot_h);
         }
      }

      public override LayoutEngine LayoutEngine => new InnerLayout();

      public ValuesFrameControl[] PpAllFrameControls => CtrlExtPanel.PpPanel.Controls.OfType<ValuesFrameControl>().ToArray();

      public IValueControl[] PpAllValueControls => PpAllFrameControls.SelectMany(c => c.PpAllControls).ToArray();

      public int PpFixedWidth
      {
         get => myFixedWidth;

         set
         {
            myFixedWidth = value;
            PerformLayout();
         }
      }

      public ValuesFrameControl MthAddFrame()
      {
         var ctr = new ValuesFrameControl();

         CtrlExtPanel.PpPanel.Controls.Add(ctr);
         ctr.OnAddedValueControl += Ctr_OnAddedValueControl;
         PerformLayout();

         return ctr;
      }

      public void MthClear()
      {
         CtrlExtPanel.PpPanel.Controls.Clear();
         PerformLayout();
      }

      private void Ctr_OnAddedValueControl(object? sender, IValueControl valueControl) => OnAddedValueControl?.Invoke(sender, valueControl);
   }
}
