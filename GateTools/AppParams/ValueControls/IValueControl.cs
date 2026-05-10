using Gate.Tools.AppParams;

namespace Gate.ToolsView.AppParams.ValueControls
{
   public delegate void OnValueChangeHandler(IValueControl sender, object? newValue);

   public interface IValueControl
   {
      event OnValueChangeHandler? OnValueChange;
     
      object? Tag { get; set; }

      object? ParamValue { get; set; }

      string ParamName { get; set; }

      void ActionOnAppParamAssociationAction(AppParam param);
   }
}
