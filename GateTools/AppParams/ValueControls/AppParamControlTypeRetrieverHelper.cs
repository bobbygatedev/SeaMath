using Gate.Tools.Extensions;
using Gate.ToolsView.AppParams.ValueControls;
using System.Data;
using System.Reflection;
using static Gate.Tools.AppParams.AppParam;

namespace Gate.Tools.AppParams.ValueControls
{
   public static class AppParamControlTypeRetrieverHelper
   {
      private static readonly List<Type> myListValueControlTypes = new List<Type>();
      private static readonly List<Assembly> myListAssembly = new List<Assembly>();

      public static IValueControl GetControlInstance(Scalar scalarParams, ValueControlAssociationAttribute attribute)
      {
         var typ = null as Type;

         if (attribute.Type != null)
         {
            return myGetInstanceFromType(attribute.Type);
         }
         else if (attribute.Id != null && (typ = myGetValueControlTypesById(attribute.Id)) != null)
         {
            return myGetInstanceFromType(typ);
         }
         else
         {
            throw new Crash($"Not a valid attribute for scalar param {scalarParams.ParamName}");
         }
      }

      private static Type? myGetValueControlTypesById(object id)
      {
         var typ = myListValueControlTypes.FirstOrDefault(t => id.Equals(t.GetCustomAttribute<ValueControlAssociationAttribute>()?.Id));

         if (typ != null) { return typ; }
         else
         {
            var ass = AppDomain.CurrentDomain.GetAssemblies();
            var ass_2_elb = ass.Except(myListAssembly).ToArray();

            if (ass_2_elb.Length == 0) { return null; }
            else
            {
               myListAssembly.AddRange(ass_2_elb);

               foreach (var asm in ass_2_elb)
               {
                  var tps_vc = asm.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IValueControl))).ToArray();
                  var tps_vc_atr = tps_vc.Where(t => t.GetCustomAttribute<ValueControlAssociationAttribute>()?.Id != null).ToArray();

                  myListValueControlTypes.AddRange(tps_vc_atr.Except(myListValueControlTypes));
               }

               return myGetValueControlTypesById(id);
            }
         }
      }

      private static IValueControl myGetInstanceFromType(Type type)
      {
         try
         {
            return type.InstanciateOrCrash() as IValueControl ?? throw new Crash($"Not a IValueControl for scalar param {type.Name}");
         }
         catch (Exception exc)
         {
            throw new Crash(exc);
         }
      }
   }
}
