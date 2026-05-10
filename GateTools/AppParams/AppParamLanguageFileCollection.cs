using Gate.Tools.Message;
using Gate.Tools.Text;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using static Gate.Tools.AppParams.AppParamLanguageFile;

namespace Gate.Tools.AppParams
{
   public abstract class AppParamLanguageFileCollection
   {
      public const string DEFAULT_DIR = "(default)";
      public const string CONTEXT_REGEX_GROUP = "ctx";

      private static Regex my_RegexContextFile = new Regex(@"Context\.(?<ctx>\w+)\.xml", RegexOptions.Compiled | RegexOptions.IgnoreCase);

      private readonly List<AppParamLanguageFile> myListFile = new List<AppParamLanguageFile>();
      private TxtStringConverter? myStringConverter;
      private AppParamLoadSaver? myLoadSaver;

      public AppParamLanguageFileCollection() => Items = new ItemCollection(this);

      public class ItemCollection : IEnumerable<AppParamLanguageFile>
      {
         public ItemCollection(AppParamLanguageFileCollection parent) => Parent = parent;

         public AppParamLanguageFile this[int index] => Parent.myListFile[index];

         public AppParamLanguageFileCollection Parent { get; }

         public AppParamLanguageFile[] Array => Parent.myListFile.ToArray();

         public IEnumerator<AppParamLanguageFile> GetEnumerator() => Parent.myListFile.GetEnumerator();

         IEnumerator IEnumerable.GetEnumerator() => Parent.myListFile.GetEnumerator();
      }

      protected abstract TxtStringConverter myMakeStringConverter();

      public ItemCollection Items { get; }

      public abstract string BaseDir { get; }

      public TxtStringConverter StringConverter
      {
         get
         {
            if (myStringConverter == null)
            {
               myStringConverter = myMakeStringConverter();
            }

            return myStringConverter;
         }
      }

      public AppParamLoadSaver LoadSaver
      {
         get
         {
            if (myLoadSaver == null)
            {
               myLoadSaver = myMakeLoadSaver();
            }

            return myLoadSaver;
         }
      }

      protected virtual AppParamLoadSaver myMakeLoadSaver() => new AppParamLoadSaver.ByXDoc();

      public Replacement[] GetReplacements(string context, CultureInfo language, bool areDefaultToInclude)
      {
         var no_def_its = Items.Where(i => i.Language?.Name == language.Name && i.Context.ToLower() == context.ToLower()).ToArray();
         var no_def_rps = no_def_its.SelectMany(i => i.Params.Replacements.Items).ToArray();

         if (areDefaultToInclude)
         {
            var def_its = Items.Where(i => i.IsDefault && i.Context.ToLower() == context.ToLower()).ToArray();
            var def_rps = def_its.SelectMany(i => i.Params.Replacements.Items).ToArray();

            return Replacement.MergeAndSelect(def_rps.Concat(no_def_rps).Where(r => (r.ParamCaption ?? "") != "").ToArray(), true);
         }
         else
         {
            return Replacement.MergeAndSelect(no_def_rps, true);
         }
      }

      public virtual string GetOptionRelativeDir(CultureInfo? language) => $"{(language == null ? DEFAULT_DIR : language.Name)}";

      public virtual string GetOptionAbsoluteDir(CultureInfo? language) => Path.Combine(BaseDir, GetOptionRelativeDir(language));

      public virtual string? GetOptionRelativePath(AppParamLanguageFile languageFile) => 
         $"{GetOptionRelativeDir(languageFile?.Language)}\\Context.{languageFile?.Context}.xml";

      public string? GetContextFromFilePath(string filePath)
      {
         var nam = Path.GetFileName(filePath);
         var mat = my_RegexContextFile.Match(nam);

         return mat.Success && mat.Length == nam.Length ? mat.Groups[CONTEXT_REGEX_GROUP].Value : null;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="language"></param>
      /// <param name="v"></param>
      /// <returns></returns>
      /// <exception cref="NotImplementedException"></exception>
      public AppParamLanguageFile GetLanguageFile(CultureInfo? language, string context, bool createIfNotExist)
      {
         var fil = myListFile.FirstOrDefault(f => f.Context.ToLower() == context.ToLower() && (language == null ?
            f.Language == null :
            f.Language?.Name == language.Name));

         if (fil == null)
         {
            var new_fil = new AppParamLanguageFile(this, context, language);

            myListFile.Add(fil = new_fil);
         }

         return fil;
      }

      public void ReadAll(MsgCollection msgs)
      {
         if (Directory.Exists(BaseDir))
         {
            myListFile.Clear();

            foreach (var dir in Directory.EnumerateDirectories(BaseDir))
            {
               var dir_nam = new DirectoryInfo(dir).Name;
               var lan = null as CultureInfo;

               if (dir_nam == DEFAULT_DIR || (lan = myGetCultureInfo(dir_nam)) != null)
               {
                  myReadDir(dir, lan, msgs);
               }
            }
         }
      }

      public void SaveAll()
      {
         if (!Directory.Exists(BaseDir)) { Directory.CreateDirectory(BaseDir); }

         foreach (var itm in Items)
         {
            itm.Save();
         }
      }

      public override string ToString() => $"Language file collection from '{BaseDir}'";

      private void myReadDir(string directory, CultureInfo? language, MsgCollection msgs)
      {
         var fls = Directory.EnumerateFiles(directory, "*");

         foreach (var fil in fls)
         {
            var ctx = GetContextFromFilePath(fil);

            if (ctx != null)
            {
               var app_par_fil = new AppParamLanguageFile(this, ctx, language);

               app_par_fil.Load(msgs);
               myListFile.Add(app_par_fil);
            }
         }
      }

      private CultureInfo? myGetCultureInfo(string dirName)
      {
         try { return CultureInfo.GetCultureInfo(dirName); }
         catch (System.Globalization.CultureNotFoundException) { return null; }
         catch (Exception exc) { throw new Crash(exc); }
      }
   }
}

