namespace Gate.ToolsView.MenuCommand
{
   public partial class CmdMenu
   {
      /// <summary>
      /// Use for include in a <see cref="Gate.ToolsView.MenuCommand.CmdMenuBuilder"/> method for adding <see cref="Gate.ToolsView.MenuCommand.Cmd"/> definined in other menus.
      /// </summary>
      public class CmdRefAttribute : BaseAttribute
      {
         /// <summary>
         ///
         /// </summary>
         /// <param name="ids"></param>
         public CmdRefAttribute(params string[] ids) => Ids = ids;

         /// <summary>
         /// 
         /// </summary>
         public string[] Ids { get; }
      }
   }
}