using Gate.Tools;
using Gate.Tools.DesignPattern;
using Gate.Tools.Extensions;
using System.Reflection;
using System.Text;
using static Gate.ToolsView.MenuCommand.CmdMenu;

namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// Subclass it and add methods with 
   /// </summary>
   public abstract class CmdMenuBuilder : Builder<CmdMenu>
   {
      private MethodInfo[]? myMethods;
      private Dictionary<MethodInfo, Cmd>? myDictCmdByMethod;
      private CmdMenuBuilder[]? myMeAndAllSubMenuBuilders;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="id"></param>
      /// <param name="refCaption"></param>
      public CmdMenuBuilder(string id, string? refCaption = null)
      {
         Id = Cmd.GetSanitizedString(id);

         if (Id == null) { throw new Crash("Id must be specified!"); }

         RefCaption = Cmd.GetSanitizedString(refCaption);
      }

      /// <summary>
      /// 
      /// </summary>
      public abstract class Part : IPartBuilder
      {
         public Part(CmdMenuBuilder menuBuilder) => MenuBuilder = menuBuilder;

         public CmdMenuBuilder MenuBuilder { get; }

         public abstract bool Build(CmdMenu? cmdMenu);
      }

      /// <summary>
      /// 
      /// </summary>
      public class PartCheckMethods : Part
      {
         public PartCheckMethods(CmdMenuBuilder menuBuilder) : base(menuBuilder) { }

         private class CheckMethodVisitor
         {
            public StringBuilder Errors { get; private set; } = new StringBuilder();

            public void Check(MethodInfo method, CmdMenuBuilder menuBuilder)
            {
               var ats = method.GetCustomAttributes<BaseAttribute>().ToArray();

               if (ats.Length > 1)
               {
                  Errors.AppendLine(
                     $"Method {method?.DeclaringType?.Name}.{method?.Name} has too many attributes ({ats.Length})!");
               }
               else
               {
                  myCheck(method, menuBuilder, (dynamic)ats[0]);
               }
            }

            private void myCheck(MethodInfo method, CmdMenuBuilder menuBuilder, BaseAttribute attribute) =>
               Errors.AppendLine($"Attribute of type {attribute.GetType().Name} not valid!");

            private void myCheck(MethodInfo method, CmdMenuBuilder menuBuilder, CmdRefAttribute cmdRefAttribute)
            {
               if ((cmdRefAttribute.Ids ?? []).Length == 0)
               {
                  Errors.AppendLine($"Method '{method?.DeclaringType?.Name}.{method?.Name}' has empty 'Ids' array!");
               }
               else if (menuBuilder.CmdContainerBuilder != null)
               {
                  foreach (var id in cmdRefAttribute.Ids ?? [])
                  {
                     var idb = menuBuilder.CmdContainerBuilder.AllMenuBuilders.SelectMany(b => b.CmdIds).FirstOrDefault(b => b == id);

                     if (idb == null)
                     {
                        Errors.AppendLine(
                           $"Method '{method?.DeclaringType?.Name}.{method?.Name}' cmd id {id} not exist in container builder!");
                     }
                  }
               }
               else
               {
                  Errors.AppendLine(
                     $"Method '{method?.DeclaringType?.Name}.{method?.Name}' has a {typeof(CmdRefAttribute).Name} " +
                     $"but {typeof(CmdContainerBuilder).Name} is not specified!");
               }
            }

            private void myCheck(MethodInfo method, CmdMenuBuilder menuBuilder, CmdDefAttribute cmdAttribute)
            {
               var prs = method.GetParameters();
               var par_0 = prs.Length >= 1 ? prs[0] : null;

               if (prs.Length > 1)
               {
                  Errors.AppendLine(
                     $"Method '{method?.DeclaringType?.Name}.{method?.Name}' associated to cmd={cmdAttribute.Caption} has too many parameters({prs.Length})!");
               }
               else if (prs.Length == 1 && par_0?.ParameterType != typeof(Cmd))
               {
                  Errors.AppendLine(
                     $"Method '{method?.DeclaringType?.Name}.{method?.Name}' associated to cmd={cmdAttribute.Caption} has too wrong single parameter (shall be '{typeof(Cmd).Name}')!");
               }
            }

            /// <summary>
            /// No constraint for SeparatorAttribute
            /// </summary>
            /// <param name="method"></param>
            /// <param name="separatorAttribute"></param>
            private void myCheck(MethodInfo method, CmdMenuBuilder menuBuilder, SeparatorAttribute separatorAttribute) { }

            private void myCheck(MethodInfo method, CmdMenuBuilder menuBuilder, SubmenuAttribute subMenuAttribute)
            {
               if (subMenuAttribute.MenuIdSanitized != null)
               {
                  if (menuBuilder.CmdContainerBuilder == null)
                  {
                     Errors.AppendLine(
                        $"Method '{method?.DeclaringType?.Name}.{method?.Name}' has {typeof(SubmenuAttribute).Name} with 'IdRef' with {typeof(CmdContainerBuilder).Name} not specified!");
                  }
                  else
                  {
                     var men = menuBuilder.CmdContainerBuilder.AllMenuBuilders.FirstOrDefault(b => b.Id == subMenuAttribute.MenuIdSanitized);

                     if (men == null)
                     {
                        Errors.AppendLine(
                           $"Method '{method?.DeclaringType?.Name}.{method?.Name}' method with id = {subMenuAttribute.MenuIdSanitized} not found in!!");
                     }
                  }
               }
               else
               {
                  var prs = method.GetParameters();

                  if (prs.Length > 1)
                  {
                     Errors.AppendLine(
                        $"Method '{method?.DeclaringType?.Name}.{method?.Name}' associated to subMenu='{subMenuAttribute.Caption}' has too many parameters ({prs.Length})!");
                  }
                  else if (!myIsTypeOrSubclass(method?.ReturnType, typeof(CmdMenuBuilder)))
                  {
                     Errors.AppendLine(
                        $"Method '{method?.DeclaringType?.Name}.{method?.Name}' associated to subMenu '{subMenuAttribute.Caption}' " +
                        $"doesn't return {typeof(CmdMenuBuilder).Name}|{typeof(CmdMenu).Name}|{typeof(CmdMenu).Name} array or any its subclass!");
                  }
               }
            }

            private static bool myIsTypeOrSubclass(Type? type, Type baseType) => type == baseType || type?.IsSubclassOf(baseType) == true;
         }

         public override bool Build(CmdMenu? cmdMenu)
         {
            var vis = new CheckMethodVisitor();

            foreach (var mth in MenuBuilder.Methods) { vis.Check(mth, MenuBuilder); }

            if (vis.Errors.ToString().Length > 0) { throw new Crash($"Error using builder {GetType().Name}\n{vis.Errors}"); }

            return true;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public class PartPopulateMenu : Part
      {
         public PartPopulateMenu(CmdMenuBuilder menuBuilder) : base(menuBuilder) { }

         private class PopulateItemVisitor
         {
            public void Populate(MethodInfo method, CmdMenuBuilder cmdMenuBuilder, Attribute attribute, CmdMenu cmdMenu)
            {
               try { myPopulate(method, cmdMenuBuilder, cmdMenu, (dynamic)attribute); }
               catch (Exception exc) { throw new Crash(exc.InnerException ?? exc); }
            }

            private void myPopulate(MethodInfo method, CmdMenuBuilder cmdMenuBuilder, CmdMenu cmdMenu, Attribute attribute) =>
               throw new Crash($"Attribute of type {attribute.GetType().Name} not valid!");

            private void myPopulate(MethodInfo method, CmdMenuBuilder cmdMenuBuilder, CmdMenu cmdMenu, CmdDefAttribute cmdAttribute) => cmdMenu.AddCommand(cmdMenuBuilder.DictCmdByMethod[method], true);

            private void myPopulate(MethodInfo method, CmdMenuBuilder cmdMenuBuilder, CmdMenu cmdMenu, CmdRefAttribute cmdRefAttribute)
            {
               var all_cms = cmdMenuBuilder.CmdContainerBuilder?.AllMenuBuilders.SelectMany(b => b.DictCmdByMethod.Values).ToArray();

               foreach (var id in cmdRefAttribute.Ids)
               {
                  var cmd = all_cms?.FirstOrDefault(c => c.Id == id) ?? throw new Crash();

                  cmdMenu.AddCommand(cmd, true);
               }
            }

            private void myPopulate(MethodInfo method, CmdMenuBuilder cmdMenuBuilder, CmdMenu cmdMenu, SeparatorAttribute separatorAttribute) => cmdMenu.AddSeparator(true);

            private void myPopulate(MethodInfo method, CmdMenuBuilder cmdMenuBuilder, CmdMenu cmdMenu, SubmenuAttribute subMenuAttribute)
            {
               //in this case there is a ref to a menu declared somewhere in container builder
               if (subMenuAttribute.MenuIdSanitized != null)
               {
                  var sub_men_bui =
                     cmdMenuBuilder.CmdContainerBuilder?.AllMenuBuilders.FirstOrDefault(b => b.Id == subMenuAttribute.MenuIdSanitized);
                  var cap = subMenuAttribute.CaptionSanitized ?? sub_men_bui?.RefCaption;

                  cmdMenu.AddSubMenu(sub_men_bui?.CompleteBuilding() ?? throw new Crash(), true, subMenuAttribute.SubMenuIdSanitized, cap);
               }
               else
               {
                  //retrieve singleton builder instance by id and complete object binding 
                  var sub_men_bui = method.Invoke(cmdMenuBuilder, []) as CmdMenuBuilder;
                  var sub_men_bui_1 = cmdMenuBuilder.MeAndAllSubMenuBuilders.FirstOrDefault(b => b.Id == sub_men_bui?.Id) ?? throw new Crash();
                  var cmd_men = sub_men_bui_1?.CompleteBuilding() ?? throw new Crash();

                  var cap = subMenuAttribute.CaptionSanitized ?? sub_men_bui_1.RefCaption;

                  if (subMenuAttribute.IsAddCommandOnly)
                  {
                     //
                     if (subMenuAttribute.IsSeparatorsToAddForCommandOnly) { cmdMenu.AddSeparator(true); }

                     cmdMenu.AddCommandRange(true, cmd_men.Commands.Items);

                     if (subMenuAttribute.IsSeparatorsToAddForCommandOnly) { cmdMenu.AddSeparator(true); }
                  }
                  else
                  {
                     cmdMenu.AddSubMenu(cmd_men, true, subMenuAttribute.SubMenuIdSanitized, cap);
                  }
               }
            }
         }

         public override bool Build(CmdMenu? cmdMenu)
         {
            var vis = new PopulateItemVisitor();

            foreach (var mth in MenuBuilder.Methods)
            {
               vis.Populate(mth, MenuBuilder, mth.GetCustomAttributes().First(), cmdMenu ?? throw new Crash());
            }

            return true;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public class PartCustomInit : Part
      {
         public PartCustomInit(CmdMenuBuilder menuBuilder) : base(menuBuilder) { }

         public override bool Build(CmdMenu? cmdMenu)
         {
            MenuBuilder.myCustomInit(cmdMenu ?? throw new Crash());
            
            return true;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="cmd"></param>
      /// <returns></returns>
      protected abstract Image? myGetCmdImage(Cmd cmd);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="cmdMenu"></param>
      protected abstract void myCustomInit(CmdMenu cmdMenu);

      public CmdContainerBuilder? CmdContainerBuilder { get; set; } = null;

      public CmdMenuBuilder[] MeAndAllSubMenuBuilders
      {
         get
         {
            if (myMeAndAllSubMenuBuilders == null)
            {
               var res = new CmdMenuBuilder[] { this };

               var mts = Methods.Where(m =>
                  m.GetCustomAttributes<SubmenuAttribute>().Count() > 0
                  && (m.ReturnType == typeof(CmdMenuBuilder) || m.ReturnType.IsSubclassOf(typeof(CmdMenuBuilder)))).ToArray();

               //just not anonimous builder are selected
               var bls = mts.Select(m => m.Invoke(this, []) as CmdMenuBuilder).Where(b => b?.Id != null).ToArray();

               //recursively
               myMeAndAllSubMenuBuilders = 
                  [.. res.Concat(bls.Concat(bls.SelectMany(b => b?.MeAndAllSubMenuBuilders ?? []))).
                  Nn().Distinct()];
            }

            return myMeAndAllSubMenuBuilders ?? [];
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public string? Id { get; }

      /// <summary>
      /// Caption (significant only if menu is used inside a main menu). 
      /// </summary>
      public string? RefCaption { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public Dictionary<MethodInfo, Cmd> DictCmdByMethod
      {
         get
         {
            if (myDictCmdByMethod == null)
            {
               var dct = new Dictionary<MethodInfo, Cmd>();

               foreach (var mth in CmdMethods) { dct[mth] = GetCmd(mth); }

               myDictCmdByMethod = dct;

            }

            return myDictCmdByMethod;
         }
      }

      public override IPartBuilder[] PartBuilders => new IPartBuilder[] {
         new PartPopulateMenu(this), new PartCheckMethods(this) , new PartCustomInit(this) };

      public MethodInfo[] CmdMethods => Methods.Where(m => m.GetCustomAttribute<CmdDefAttribute>() != null).ToArray();

      public string[] CmdIds => CmdMethods.
         Select(m => m.GetCustomAttribute<CmdDefAttribute>()?.Id).
         Nn().
         Where(i => !i.IsBlank()).ToArray();

      public MethodInfo[] Methods
      {
         get
         {
            if (myMethods == null)
            {
               myMethods =
                  GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).
                  Where(m => m.GetCustomAttributes() != null && m?.GetCustomAttributes<BaseAttribute>().Count() > 0).ToArray();
            }

            return myMethods;
         }
      }

      public override CmdMenu Reset()
      {
         myDictCmdByMethod = null;

         return base.Reset();
      }

      public override CmdMenu GetBlankObject() => new CmdMenu(true, Id);

      public Cmd GetCmd(MethodInfo method)
      {
         var cmd_atr = method.GetCustomAttribute<CmdDefAttribute>();

         if (method.GetCustomAttributes().Count() == 1 && cmd_atr != null)
         {
            var cmd = new Cmd(cmd_atr.Id, cmd_atr.Caption);
            var prs = method.GetParameters();

            cmd.ShortCut = cmd_atr.ShortCut;
            cmd.ShortCut2 = cmd_atr.ShortCut2;
            cmd.Image = myGetCmdImage(cmd) ?? cmd_atr.Image;

            if (prs.Length == 0 || prs.Length == 1 && prs[0].ParameterType == typeof(Cmd)) { cmd.Action = c => myInvoke(method, c); }
            else { throw new Crash($"{cmd.Id} Cmd.Action not of type Action() or Action(Cmd)"); }

            return cmd;
         }
         else { throw new Crash($"'{method?.DeclaringType?.Name}.{method?.Name}'"); }
      }

      private void myInvoke(MethodInfo method, Cmd cmd)
      {
         try
         {
            var ars = method.GetParameters().Length > 0 ? [cmd] : new object[] { };

            method.Invoke(this, ars);
         }
         catch (TargetInvocationException exc) { throw new Crash(exc.InnerException); }
      }
   }
}
