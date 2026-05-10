using Gate.Tools;
using Gate.Tools.DesignPattern;
using static Gate.ToolsView.MenuCommand.CmdMenu;

namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class CmdContainerBuilder : Builder<CmdContainer>
   {
      private CmdMenuBuilder[]? myContextMenuBuilders;
      private MainMenuBuildDef[]? myMainMenusBuildDefs;
      private CmdMenuBuilder[]? myMainMenuBuilders;

      /// <summary>
      /// 
      /// </summary>
      protected CmdContainerBuilder() { }

      /// <summary>
      /// 
      /// </summary>
      public abstract class Part : IPartBuilder
      {
         public Part(CmdContainerBuilder cmdContainerBuilder) => CmdContainerBuilder = cmdContainerBuilder;

         public CmdContainerBuilder CmdContainerBuilder { get; }

         public abstract bool Build(CmdContainer? cmdContainerBuilder);
      }

      /// <summary>
      /// 
      /// </summary>
      public class PartInit : Part
      {
         public PartInit(CmdContainerBuilder cmdContainerBuilder) : base(cmdContainerBuilder) { }

         public override bool Build(CmdContainer? cmdContainerBuilder)
         {
            foreach (var bui in CmdContainerBuilder?.AllMenuBuilders ?? [])
            {
               bui.CmdContainerBuilder = CmdContainerBuilder;
               bui.Reset();
            }

            return true;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public class PartFillMenus : Part
      {
         public PartFillMenus(CmdContainerBuilder cmdContainerBuilder) : base(cmdContainerBuilder) { }

         public override bool Build(CmdContainer? cmdContainer)
         {
            //force creation of all menus 
            var mns = CmdContainerBuilder.AllMenuBuilders.Select(b => b.CompleteBuilding()).ToArray();

            //adds menu into container
            foreach (var men in mns.Cast<CmdMenu>())
            {
               //sometimes are added from parents menus
               if (men.CmdContainer == null)
               {
                  cmdContainer?.AllMenus.Add(men);
               }
            }

            return true;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public class PartFillMainMenus : Part
      {
         public PartFillMainMenus(CmdContainerBuilder cmdContainerBuilder) : base(cmdContainerBuilder) { }

         public override bool Build(CmdContainer? cmdContainer)
         {
            foreach (var def in CmdContainerBuilder.MainMenusBuildDefs ?? [])
            {
               var mai_men = new CmdMainMenu(def.Id, def.DefaultCaption);

               foreach (var men_def in def.MenuDefs)
               {
                  mai_men.AddMenuRef(
                     new Ref(
                        men_def.MenuBuilder.Object ?? throw new Crash(),
                        true,
                        men_def.IdRef,
                        men_def.IdRefCaption));
               }

               cmdContainer?.CmdMainMenus.Add(mai_men);
            }

            return true;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public class PartFinalize : Part
      {
         public PartFinalize(CmdContainerBuilder cmdContainerBuilder) : base(cmdContainerBuilder) { }

         public override bool Build(CmdContainer? cmdContainer)
         {
            foreach (var bui in CmdContainerBuilder.AllMenuBuilders) { bui.CmdContainerBuilder = null; }

            return true;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public class MainMenuBuildDef
      {
         public MainMenuBuildDef(string id, string defaultCaption, params CmdMenuBuilder[] menuBuilders)
         {
            Id = id;
            DefaultCaption = defaultCaption;
            MenuDefs = menuBuilders.Select(b => new MenuDef(b)).ToArray();
         }

         public class MenuDef
         {
            public MenuDef(CmdMenuBuilder menuBuilder, string? idRef = null, string? idRefCaption = null)
            {
               MenuBuilder = menuBuilder;
               IdRef = idRef ?? menuBuilder.Id;
               IdRefCaption = idRefCaption ?? menuBuilder.RefCaption;
            }

            public string? IdRef { get; }

            public string? IdRefCaption { get; }

            public CmdMenuBuilder MenuBuilder { get; }
         }

         /// <summary>
         /// Id (used ie for language file)
         /// </summary>
         public string Id { get; }

         /// <summary>
         /// Name appearing in menu customisation form.
         /// </summary>
         public string DefaultCaption { get; }

         /// <summary>
         /// 
         /// </summary>
         public MenuDef[] MenuDefs { get; }
      }

      protected abstract CmdMenuBuilder[] myMakeContextMenuBuilders();

      protected abstract MainMenuBuildDef[] myMakeMainMenusBuildDefs();

      public override IPartBuilder[] PartBuilders => 
         [new PartInit(this) , new PartFillMenus(this) , new PartFillMainMenus(this) , new PartFinalize(this)];

      public CmdMenuBuilder[] AllMenuBuilders => 
         ContextMenuBuilders.
         Concat(MainMenuBuilders ?? []).
         SelectMany(mb => mb.MeAndAllSubMenuBuilders).
         ToArray();

      public CmdMenuBuilder[] ContextMenuBuilders
      {
         get
         {
            if (myContextMenuBuilders == null) { 
               myContextMenuBuilders = 
                  myMakeContextMenuBuilders().
                  SelectMany(b => b.MeAndAllSubMenuBuilders).
                  Distinct().
                  ToArray(); }

            return myContextMenuBuilders;
         }
      }

      public CmdMenuBuilder[]? MainMenuBuilders
      {
         get
         {
            if (myMainMenuBuilders == null) { myBuildMainMenuBuilders(); }

            return myMainMenuBuilders;
         }
      }

      public MainMenuBuildDef[]? MainMenusBuildDefs
      {
         get
         {
            if (myMainMenusBuildDefs == null) { myBuildMainMenuBuilders(); }

            return myMainMenusBuildDefs;
         }
      }

      public override CmdContainer GetBlankObject() => new CmdContainer();

      private void myBuildMainMenuBuilders()
      {
         myMainMenusBuildDefs = myMakeMainMenusBuildDefs();
         myMainMenuBuilders = myMainMenusBuildDefs.SelectMany(d => d.MenuDefs.Select(d1 => d1.MenuBuilder)).ToArray();
      }
   }
}
