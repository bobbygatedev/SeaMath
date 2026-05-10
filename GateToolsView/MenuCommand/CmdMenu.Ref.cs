using Gate.Tools;

namespace Gate.ToolsView.MenuCommand
{
   public partial class CmdMenu
   {
      public class Ref : HierarchicalItem, ICmdWithId, ICmdWithCaption, ICmdMenuItem
      {
         private string? myCaption;

         /// <summary>
         /// 
         /// </summary>
         /// <param name="cmdMenu"></param>
         /// <param name="isDefault"></param>
         /// <param name="refId"></param>
         /// <param name="caption"></param>
         public Ref(CmdMenu cmdMenu, bool isDefault, string? refId = null, string? caption = null)
         {
            CmdMenu = cmdMenu;
            Id = Cmd.GetUniqueId(refId);
            IsDefault = isDefault;
            DefaultCaption = Caption = Cmd.GetSanitizedString(caption);
         }

         /// <summary>
         /// 
         /// </summary>
         public bool IsDefault { get; }

         /// <summary>
         /// 
         /// </summary>
         public ICmdMenuControlAssociation? Association => CmdMenu.Associations.FirstOrDefault(a => a.CmdMenuRef == this);

         /// <summary>
         /// 
         /// </summary>
         public string? Caption
         {
            get => myCaption;

            set
            {
               myCaption = value;
               Association?.SetCaption(value);
            }
         }

         /// <summary>
         /// 
         /// </summary>
         public string? DefaultCaption { get; }

         /// <summary>
         ///  id (used just for language file) search is made by cmd id.
         /// </summary>
         public string? Id { get; set; }

         /// <summary>
         /// 
         /// </summary>
         public CmdMenu CmdMenu { get; }

         /// <summary>
         /// 
         /// </summary>
         public CmdMainMenu? CmdMainMenu => ParentItemChain.OfType<CmdMainMenu>().FirstOrDefault();

         /// <summary>
         /// 
         /// </summary>
         public Keys MenuShortCut
         {
            get
            {
               var txt = Caption;
               var idx = txt?.IndexOf('&')??-1;

               if (idx != -1 && idx + 1 < txt?.Length)
               {
                  var ch = txt[idx + 1];
                  var key = (Keys)char.ToUpper(ch);

                  return Keys.Alt | key;
               }

               return Keys.None;
            }
         }

         /// <summary>
         /// 
         /// </summary>
         public void Show() => Association?.Show();

         public override string ToString() => $"Submenu (Id={Id} Caption={Caption}, to {CmdMenu}";
      }
   }
}
