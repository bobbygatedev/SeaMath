using Gate.Tools.AppParams;
using Gate.Tools.Text;
using System.IO;

namespace Gate.Dock.DockApp
{
   public class GateDockAppLanguageFileCollection : AppParamLanguageFileCollection
   {
      public GateDockAppLanguageFileCollection(GateDockApp app) => App = app;

      public GateDockApp App { get; }

      public override string BaseDir => Path.Combine(App.AppFolder, "LanguageFiles");

      protected override TxtStringConverter myMakeStringConverter() => new TxtStringConverter.Default();
   }
}

