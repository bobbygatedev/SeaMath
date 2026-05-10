using Gate.Tools;

namespace Gate.ToolsView.ControlObserve
{
   /// <summary>
   /// 
   /// </summary>
   public class ControlObservableChildStyle : ControlObservableChild
   {
      public ControlObservableChildStyle(Control control) => RootControl = control;

      protected override void myAddObservedControl(Control control)
      {
         base.myAddObservedControl(control);

         if (control == RootControl)
         {
            control.FontChanged += MyParentControl_FontChanged;
            control.ForeColorChanged += MyParentControl_ForeColorChanged;
            control.BackColorChanged += MyParentControl_BackColorChanged;
         }
         else { myCopyStyle(control); }
      }

      protected override void myRemoveObservedControl(Control? control)
      {
         if (control != null)
         {
            base.myRemoveObservedControl(control);

            if (control == RootControl)
            {
               control.FontChanged -= MyParentControl_FontChanged;
               control.ForeColorChanged -= MyParentControl_ForeColorChanged;
               control.BackColorChanged -= MyParentControl_BackColorChanged;
               control.StyleChanged += MyParentControl_StyleChanged;
            }
         }
      }

      protected void myCopyStyle(Control control)
      {
         control.Font = (RootControl ?? throw new Crash()).Font;
         control.ForeColor = RootControl.ForeColor;
         control.BackColor = RootControl.BackColor;

         var dst_pro = control.GetType().GetProperty("BorderStyle");

         if (dst_pro != null) { dst_pro.SetValue(control, BorderStyle.None, new object[0]); }
      }

      private void myCopyStyleToAll()
      {
         foreach (var ctr in ObservedControls.Where(c => c != RootControl)) { myCopyStyle(ctr); }
      }

      private void MyParentControl_StyleChanged(object? sender, EventArgs e) => myCopyStyleToAll();

      private void MyParentControl_BackColorChanged(object? sender, EventArgs e) => myCopyStyleToAll();

      private void MyParentControl_ForeColorChanged(object? sender, EventArgs e) => myCopyStyleToAll();

      private void MyParentControl_FontChanged(object? sender, EventArgs e) => myCopyStyleToAll();
   }
}
