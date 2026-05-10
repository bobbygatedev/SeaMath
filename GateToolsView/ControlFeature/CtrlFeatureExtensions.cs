namespace Gate.ToolsView.ControlFeature.Extensions
{
   public static class CtrlFeatureExtensions
   {
      public static FEA AddFeature<FEA>(this Control control) where FEA : CtrlFeature, new() => CtrlFeature.Add<FEA>(control);

      public static FEA RemoveFeature<FEA>(this Control control) where FEA : CtrlFeature, new() => CtrlFeature.Remove<FEA>(control);

      public static FEA? GetFeature<FEA>(this Control control) where FEA : CtrlFeature, new() => 
         CtrlFeature.GetControlFeatures(control).FirstOrDefault(f => f is FEA) as FEA;
   }
}
