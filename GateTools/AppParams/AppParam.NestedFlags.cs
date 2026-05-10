using System;

namespace Gate.Tools.AppParams
{
   public abstract partial class AppParam
   {
      /// <summary>
      /// 
      /// </summary>
      [Flags]
      public enum NestedFlags
      {
         /// <summary>
         /// 
         /// </summary>
         none = 0x0,

         /// <summary>
         /// In case of failure error is signalled.
         /// </summary>
         shall_exist = 0x1,

         /// <summary>
         /// Tries to load.
         /// </summary>
         load = 0x4,

         /// <summary>
         /// When container is saved file is saved.
         /// </summary>
         autosave = 0x8,

         /// <summary>
         /// In case file not exists default instance is created (effective just whether <see cref="shall_exist"/> is defined).
         /// </summary>
         default_if_not_exists = 0x10,

         /// <summary>
         /// In case file load failes default instance is created.
         /// </summary>
         default_if_load_file_fails = 0x20,

         /// <summary>
         /// An excepotion is raised in this case instead of raise <see cref="OnError"/> event.
         /// </summary>
         exception = 0x40,

         /// <summary>
         /// Can be not specified or empty
         /// </summary>
         required = 0x80,

         /// <summary>
         /// When selected a static dictionary[path] is used for mapping possibly instance of container inside application
         /// </summary>
         dictionary = 0x100 ,
      }
   }
}
