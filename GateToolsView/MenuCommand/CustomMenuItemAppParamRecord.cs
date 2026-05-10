using Gate.Tools;
using Gate.Tools.AppParams;

namespace Gate.ToolsView.MenuCommand
{
   public class CustomMenuItemAppParamRecord : AppParam.Record
   {
      public enum TypeEnum
      {
         /// <summary>
         /// 
         /// </summary>
         none = 0,

         /// <summary>
         /// 
         /// </summary>
         cmd,

         /// <summary>
         /// 
         /// </summary>
         sub_menu,

         /// <summary>
         /// 
         /// </summary>
         separator
      }

      public static CustomMenuItemAppParamRecord MakeFromMenuItem(ICmdMenuItem cmdMenuItem) => InnerMakerFromMenuItem.Create(cmdMenuItem);

      /// <summary>
      /// 
      /// </summary>
      private static class InnerMakerFromMenuItem
      {
         public static CustomMenuItemAppParamRecord Create(ICmdMenuItem itm) => my_Create((dynamic)itm);

         private static CustomMenuItemAppParamRecord my_Create(Cmd.Slot cmdSlot)
         {
            var itm = new CustomMenuItemAppParamRecord();

            itm.Id = cmdSlot.Cmd.Id;
            itm.Type = CustomMenuItemAppParamRecord.TypeEnum.cmd;

            return itm;
         }

         private static CustomMenuItemAppParamRecord my_Create(CmdMenuSeparator cmdMenuSeparator)
         {
            var itm = new CustomMenuItemAppParamRecord();

            itm.Type = CustomMenuItemAppParamRecord.TypeEnum.separator;

            return itm;
         }

         private static CustomMenuItemAppParamRecord my_Create(CmdMenu.Ref subMenu)
         {
            var itm = new CustomMenuItemAppParamRecord();

            itm.Type = CustomMenuItemAppParamRecord.TypeEnum.sub_menu;
            itm.Id = subMenu.Id;
            itm.SubMenuCmdId = subMenu.CmdMenu.Id;
            itm.SubMenuCaption = subMenu?.Caption ?? "";

            return itm;
         }

         private static CustomMenuItemAppParamRecord my_Create(ICmdMenuItem hierarchicalItem) => throw new Crash($"Unexpected type {hierarchicalItem.GetType().Name}");
      }


      /// <summary>
      /// 
      /// </summary>
      public string? Id { get => myId.Value; set => myId.Value = value; }

      /// <summary>
      /// 
      /// </summary>
      public int MenuIdx { get => myMenuIdx.Value; set => myMenuIdx.Value = value; }

      /// <summary>
      /// 
      /// </summary>
      public string SubMenuCaption { get => mySubMenuCaption.Value ?? ""; set => mySubMenuCaption.Value = value; }

      /// <summary>
      /// 
      /// </summary>
      public string SubMenuCmdId { get => mySubMenuCmdId.Value ?? ""; set => mySubMenuCmdId.Value = value; }

      /// <summary>
      /// 
      /// </summary>
      public TypeEnum Type
      {
         get => myType.Value;

         set => myType.Value = value;
      }

      private readonly Simple<TypeEnum> myType = new Simple<TypeEnum>(TypeEnum.none);
      private readonly Simple<string> myId = new Simple<string>();
      private readonly Simple<string> mySubMenuCmdId = new Simple<string>();
      private readonly Simple<string> mySubMenuCaption = new Simple<string>();
      private readonly Simple<int> myMenuIdx = new Simple<int>(-1, false);
   }
}

