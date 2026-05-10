using Gate.Tools;
using Gate.ToolsView.ControlFeature.Extensions;
using Gate.ToolsView.Extensions;
using System.Reflection;

namespace Gate.ToolsView.ControlFeature
{
   /// <summary>
   ///
   /// </summary>
   public abstract class CtrlFeature
   {
      public delegate void OnFeatureAddedHandler(CtrlFeature feature, Control? boundControl);
      public delegate void OnFeatureRemovedHandler(CtrlFeature feature, Control? boundControl);

      private static readonly MethodInfo mySetStyleMethod =
         typeof(Control).GetMethod("SetStyle", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new Crash();
      private static readonly List<InnerFeatureTray> myListInnerFeatureTray = [];

      protected Control? myBoundControl = null;

      public static event OnFeatureAddedHandler? OnFeatureAdded;
      public static event OnFeatureRemovedHandler? OnFeatureRemoved;

      public CtrlFeature()
      {
         if (SpecificControlType != null && !SpecificControlType.IsSubclassOf(typeof(Control)))
         {
            throw new Crash($"{SpecificControlType} not a subclass of {typeof(Control).Name}");
         }
      }

      public abstract class Specialized<CTRL> : CtrlFeature where CTRL : Control
      {
         public override Type SpecificControlType => typeof(CTRL);

         public new CTRL BoundControl => (CTRL)base.BoundControl;
      }

      private class InnerFeatureTray
      {
         public InnerFeatureTray(Control control) => Control = control;

         public Control Control { get; }

         public List<CtrlFeature> ListFeature { get; private set; } = new List<CtrlFeature>();
      }

      public abstract Type? SpecificControlType { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <exception cref="NullReferenceException">Feature not initialized yet</exception>"
      public virtual Control BoundControl => myBoundControl ?? throw new NullReferenceException("Feature not initialized yet");

      protected virtual void myDoSetBoundControl(Control? control)
      {
         if (myBoundControl != control)
         {
            if (myBoundControl != null)
            {
               myBoundControl.HandleDestroyed -= Control_HandleDestroyed;
               myOnControlDeassociate(myBoundControl);
            }

            if (SpecificControlType != null && control != null && !(control.GetType() == SpecificControlType || control.GetType().IsSubclassOf(SpecificControlType)))
            {
               throw new Gate.Tools.ToolsException($"Feature '{GetType().Name}' is for form only!");
            }

            myBoundControl = control;

            if (myBoundControl != null) { myOnControlAssociate(myBoundControl); }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <exception cref="NullReferenceException">Feature not initialized yet</exception>""
      public Form BoundControlParentForm => BoundControl.MthGetParentForm() ?? throw new NullReferenceException("Feature not initialized yet");

      public static CtrlFeature[] GetControlFeatures(Control control)
      {
         var tra = myListInnerFeatureTray.FirstOrDefault(t => t.Control == control);

         return tra != null ? tra.ListFeature.ToArray() : new CtrlFeature[0];
      }

      internal static FEA Add<FEA>(Control control) where FEA : CtrlFeature, new()
      {
         var fea = control.GetFeature<FEA>();

         if (fea != null) { return fea; }
         else
         {
            var tra = myListInnerFeatureTray.FirstOrDefault(t => t.Control == control);

            if (tra == null) { myListInnerFeatureTray.Add(tra = new InnerFeatureTray(control)); }

            fea = new FEA();
            fea.myDoSetBoundControl(control);
            tra.ListFeature.Add(fea);
            OnFeatureAdded?.Invoke(fea, control);

            return fea;
         }
      }

      internal static FEA Remove<FEA>(Control control) where FEA : CtrlFeature
      {
         var tra = myListInnerFeatureTray.FirstOrDefault(t => t.Control == control);
         var fea = tra != null ? tra.ListFeature.FirstOrDefault(f => f is FEA) as FEA : null;

         if (fea == null)
         {
            throw new Gate.Tools.ToolsException($"Control of type 'control.GetType().Name' not containing a feature of type '{typeof(FEA).Name}'");
         }
         else
         {
            fea.myDoSetBoundControl(null);
            tra?.ListFeature.Remove(fea);
            OnFeatureRemoved?.Invoke(fea, control);

            return fea;
         }
      }

      protected void mySetStyle(Control control, ControlStyles controlStyle, bool value) => mySetStyleMethod.Invoke(control, new object[] { controlStyle, value });

      protected abstract void myOnControlAssociate(Control boundControl);

      protected abstract void myOnControlDeassociate(Control boundControl);

      private static void Control_HandleDestroyed(object? sender, EventArgs e)
      {
         var tra = myListInnerFeatureTray.FirstOrDefault(f => f.Control == sender);

         if (tra != null)
         {
            foreach (var fea in tra.ListFeature) { fea.myDoSetBoundControl(null); }
         }
      }
   }
}
