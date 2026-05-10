using Gate.Tools.AppParams;
using Gate.Tools.Extensions;
using System.Collections;

namespace Gate.ToolsView.TextSearch
{
   public class TextSearchParamRecord : AppParam.Record
   {
      public TextSearchParamRecord() : base("TextSearchParams") { }

      public class ComboRecord : Record
      {
         /// <summary>
         /// 
         /// </summary>
         public readonly Simple<string> Text = new Simple<string>();

         /// <summary>
         /// 
         /// </summary>
         public readonly Arry<Simple<string>> DropDowns = new Arry<Simple<string>>();

         public (string? text, string[] dropDowns) Content
         {
            get => (Text.Value, DropDowns.Items.Select(i => i.Value ?? "").ToArray());

            set
            {
               Text.Value = value.text;
               DropDowns.Clear();

               foreach (var dd in value.dropDowns)
               {
                  DropDowns.AddParam(new Simple<string>()).Value = dd;
               }
            }
         }
      }

      public class LookInRecord : Record
      {
         /// <summary>
         /// 
         /// </summary>
         private readonly Arry<Arry<Simple<string>>> myDirItemsDropDowns = new Arry<Arry<Simple<string>>>();

         public class DirItem : IEnumerable<string>
         {
            public static DirItem? FromArray(Arry<Simple<string>> arry) =>
               arry.ItemCount > 0 ? new DirItem(arry.Items.Select(i => i.Value).Nn().ToArray()) : null;

            public DirItem() => Dirs = [];

            public DirItem(string[] dirs) => Dirs = (dirs ?? []).ToArray();

            public string RebuiltValue => string.Join(";", Dirs);

            public string[] Dirs { get; }

            public override string ToString() => RebuiltValue;

            public IEnumerator<string> GetEnumerator() => Dirs.ToList().GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => Dirs.GetEnumerator();

            public Arry<Simple<string>> ToArry()
            {
               var arr = new Arry<Simple<string>>();

               foreach (var dir in Dirs)
               {
                  arr.AddParam(new Simple<string>(dir));
               }

               return arr;
            }
         }

         /// <summary>
         /// Look in parameters for doc mode (F3/Shift+F3)
         /// </summary>
         public readonly Simple<string> TextValueDoc = new Simple<string>();

         /// <summary>
         /// Look in parameters for "Find in files mode" (F4/Shift+F4)
         /// </summary>
         public readonly Simple<string> TextValueFiles = new Simple<string>();

         public DirItem[] DirItems
         {
            get => myDirItemsDropDowns.Items.Select(i => DirItem.FromArray(i)).Nn().ToArray() ?? [];
            set
            {
               value = value ?? [];

               myDirItemsDropDowns.Clear();

               foreach (var dd in value)
               {
                  myDirItemsDropDowns.AddParam(dd.ToArry());
               }
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public readonly Simple<TextSearchFlags> SearchFlags = new Simple<TextSearchFlags>();

      public readonly ComboRecord ComboFind = new ComboRecord();

      public readonly ComboRecord ComboReplace = new ComboRecord();
      public readonly ComboRecord ComboFileTypes = new ComboRecord();

      public readonly LookInRecord LookIn = new LookInRecord();
   }
}
