using Gate.Tools.Extensions;
using Gate.Tools.Text;
using System.Globalization;
using static Gate.Tools.AppParams.AppParam;
using static Gate.Tools.AppParams.AppParamLanguageFile;

namespace Gate.Tools.AppParams
{
   /// <summary>
   /// 
   /// </summary>
   public class AppParamLanguageFile : AppParamContainerSpecialized<ParamsRecord>
   {
      public AppParamLanguageFile(AppParamLanguageFileCollection parentCollection, string context, CultureInfo? language = null)
      {
         ParentCollection = parentCollection;
         Context = context;
         Language = language;
      }

      public class ParamsRecord : Record
      {
         public ParamsRecord() : base(name: "Languages") { }

         public readonly Arry<Replacement> Replacements = new Arry<Replacement>();
      }

      public class Replacement : Record
      {
         /// <summary>
         /// 
         /// </summary>
         public readonly Simple<string> Id = new Simple<string>();

         /// <summary>
         /// 
         /// </summary>
         public readonly Simple<string> ReplacementText = new Simple<string>();

         /// <summary>
         /// Merges replacement deleting duplicated id (replacement with greater list.id overrides smaller ones).
         /// Any replacements with Id.Trim()=="" is discarded.
         /// </summary>
         /// <param name="replacements"></param>
         /// <param name="isIdCaseInsensitive"></param>
         /// <returns></returns>
         public static Replacement[] MergeAndSelect(Replacement[] replacements, bool isIdCaseInsensitive)
         {
            var lst_rep = new List<Replacement>();

            foreach (var rep in replacements.Where(r => !r.Id.Value.IsBlank()))
            {
               lst_rep.RemoveAll(r => string.Compare(r.Id.Value.ExtTrim(), rep.Id.Value.ExtTrim(), isIdCaseInsensitive) == 0);
               lst_rep.Add(rep);
            }

            return lst_rep.ToArray();
         }
      }
      public bool IsDefault => Language == null;

      public override string FixedPath => Path.Combine(
         ParentCollection.BaseDir, ParentCollection.GetOptionRelativePath(this)??throw new Gate.Tools.ToolsException());

      public AppParamLanguageFileCollection ParentCollection { get; }

      public CultureInfo? Language { get; }

      public string Context { get; }

      protected override AppParamLoadSaver myMakeLoadSaver() => ParentCollection.LoadSaver;

      protected override TxtStringConverter myMakeStringConverter() => ParentCollection.StringConverter;

      public override string ToString() => $"Language file context:'{Context}' Language: {Language?.Name}";
   }
}

