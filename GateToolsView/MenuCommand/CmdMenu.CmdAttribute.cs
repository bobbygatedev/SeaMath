using System.Resources;

namespace Gate.ToolsView.MenuCommand
{
   public partial class CmdMenu
   {
      /// <summary>
      /// 
      /// </summary>
      public class CmdDefAttribute : BaseAttribute
      {
         /// <summary>
         /// 
         /// </summary>
         public CmdDefAttribute() { }

         /// <summary>
         /// Shortcut combination.
         /// </summary>
         public virtual Keys ShortCut { get; set; } = Keys.None;

         /// <summary>
         /// If != None implements double shortcut (ie CTRL+K,CTRL+D)
         /// </summary>
         public virtual Keys ShortCut2 { get; set; } = Keys.None;

         /// <summary>
         /// 
         /// </summary>
         public virtual string? Caption { get; set; } = null;

         /// <summary>
         /// 
         /// </summary>
         public virtual string? Id { get; set; } = null;

         /// <summary>
         /// 
         /// </summary>
         public virtual string? ImageResourceString { get; set; }

         /// <summary>
         /// 
         /// </summary>
         public Image? Image
         {
            get
            {
               if (ImageResourceString != null)
               {
                  var tps =
                     AppDomain.CurrentDomain.GetAssemblies().
                        SelectMany(a => a.GetTypes().Where(t => t.Name == "Resources")).ToArray();

                  var rsr_ims = tps.Select(t =>
                  {
                     var res_man = new ResourceManager(t.Namespace + ".Resources", t.Assembly);

                     return res_man.GetObject(ImageResourceString) as Image;
                  }).OfType<Image>().ToArray();

                  return rsr_ims.Length == 1 ?
                     rsr_ims[0] : throw new Gate.Tools.ToolsException($"Not found single instance for resource {ImageResourceString}");
               }

               return null;
            }
         }
      }
   }
}