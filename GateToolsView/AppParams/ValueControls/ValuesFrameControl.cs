using Gate.Tools;
using Gate.Tools.AppParams;
using Gate.Tools.Extensions;
using System.Windows.Forms.Layout;
using static Gate.ToolsView.AppParams.ValueControls.ValueControlFactory;
using static Gate.ToolsView.AppParams.ValueControls.ValueControlFactory.ForString;

namespace Gate.ToolsView.AppParams.ValueControls
{
   /// <summary>
   ///
   /// </summary>
   public partial class ValuesFrameControl : UserControl
   {
      public delegate void OnAddedValueControlHandler(object? sender, IValueControl valueControl);

      public event OnAddedValueControlHandler? OnAddedValueControl;

      private readonly Dictionary<string, IValueControl> myDictionaryControls = new Dictionary<string, IValueControl>();

      public ValuesFrameControl()
      {
         InitializeComponent();
      }

      private class InnerLayoutEngine : LayoutEngine
      {
         public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
         {
            var ctr = (ValuesFrameControl)container;
            var gru = ctr.CtrlGroupBox;

            gru.Size = ctr.Size;

            var y = gru.DisplayRectangle.Y;
            var x = gru.DisplayRectangle.X;
            var w = gru.DisplayRectangle.Width;

            foreach (var ct in ctr.PpAllControls.Cast<Control>())
            {
               ct.Location = new Point(x, y);
               ct.Size = new Size(w, ct.PreferredSize.Height);
               y += ct.Height;
            }

            return false;
         }
      }
      public override LayoutEngine LayoutEngine => new InnerLayoutEngine();

      public string PpTitle { get => CtrlGroupBox.Text; set => CtrlGroupBox.Text = value; }

      public IValueControl[] PpAllControls => myDictionaryControls.Values.ToArray();

      public ValueControlFactory[] PpParamValueControlFactories { get; set; } = [
            new ForString() ,
            new ForBool(),
            new ForCultureInfo(),
            new ForFont(),
            new ForEncoding(),
            new ForEnum(),
            new WithConverter<byte>() ,
            new WithConverter<sbyte>() ,
            new WithConverter<Int16>() ,
            new WithConverter<Int32>() ,
            new WithConverter<Int64>() ,
            new WithConverter<UInt16>() ,
            new WithConverter<UInt32>() ,
            new WithConverter<UInt64>() ,
            new WithConverter<float>() ,
            new WithConverter<double>() ,
         ];

      public IValueControl MthAddParameterType(Type paramType, string paramName, object tag)
      {
         if (paramName.IsBlank()) { throw new Gate.Tools.ToolsException("Param name can't be blank!"); }
         else
         {
            if (!myDictionaryControls.ContainsKey(paramName))
            {
               var val_ctr_fac = PpParamValueControlFactories.FirstOrDefault(p => p.IsTypeValid(paramType));

               if (val_ctr_fac == null)
               {
                  throw new Gate.Tools.ToolsException(
                     $"Not a '{typeof(ValueControlFactory).Name}' associated to option editor type '{paramType}'!");
               }
               else { return myDoAddControl(paramName, val_ctr_fac.MakeValueControl(paramType), tag); }
            }
            else { throw new Gate.Tools.ToolsException($"Frame already contains tag '{paramName}'!"); }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="paramName"></param>
      /// <param name="valueControl"></param>
      /// <returns></returns>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      /// <exception cref="Crash"></exception>
      public IValueControl MthAddParameterType(string paramName, IValueControl valueControl, object? tag)
      {
         if (paramName.IsBlank()) { throw new Gate.Tools.ToolsException("Param name can't be blank!"); }
         else if (!(valueControl.GetType().IsMeOrSubClass(typeof(Control)))) { throw new Gate.Tools.ToolsException("valueControl shall be of type Control"); }
         else
         {
            if (!myDictionaryControls.ContainsKey(paramName)) { return myDoAddControl(paramName, valueControl, tag); }
            else { throw new Gate.Tools.ToolsException($"Frame already contains tag '{paramName}'!"); }
         }
      }

      private IValueControl myDoAddControl(string paramName, IValueControl control, object? tag)
      {
         myDictionaryControls[paramName] = control;
         CtrlGroupBox.Controls.Add((Control)control);
         PerformLayout();
         control.ParamName = paramName;
         control.Tag = tag;
         OnAddedValueControl?.Invoke(this, control);

         if (tag is AppParam par)
         {
            control.ActionOnAppParamAssociationAction(par);
         }

         return control;
      }

      public override Size GetPreferredSize(Size proposedSize)
      {
         if (PpAllControls.Length > 0)
         {
            var mrg_w = CtrlGroupBox.ClientRectangle.Width - CtrlGroupBox.DisplayRectangle.Width;
            var mrg_h = CtrlGroupBox.ClientRectangle.Height - CtrlGroupBox.DisplayRectangle.Height;

            var ct_hei = PpAllControls.Cast<Control>().Sum(c => c.PreferredSize.Height);
            var ct_wdt = PpAllControls.Cast<Control>().Max(c => c.PreferredSize.Width);

            return new Size(ct_wdt + mrg_w, ct_hei + mrg_h);
         }
         else
         {
            return CtrlGroupBox.GetPreferredSize(proposedSize);
         }
      }
   }
}
