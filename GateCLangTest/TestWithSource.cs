using Gate.CLanguageTest.Properties;
using Gate.Tools;
using Gate.Tools.Text;
using System.Text;

namespace Gate.CLanguageTest
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class TestWithSource : TestBase
   {

      public virtual TxtSettings TxtSettings => new TxtSettings
      {
         Encoding = Encoding.UTF8,
         IsTabUseSpace = true,
         TabNumChars = 3
      };

      public virtual string WorkDirHeader => Path.GetFullPath(Path.Combine(WorkDirCpp, "sub"));

      public virtual string WorkDirCpp => Path.GetFullPath("./work");

      protected TxtStore myAddResourceFile(string fullPath)
      {
         var fil_nam = Path.GetFileName(fullPath);
         var bdy = Resources.ResourceManager.GetString(fil_nam, Resources.Culture);
         var sto = new TxtStore(bdy);
         var dir = Directory.GetParent(fullPath);

         Directory.CreateDirectory(dir?.FullName ?? throw new Crash());

         sto.FileInfo = new FileInfo(fullPath);
         sto.Save(sto.FileInfo.FullName);

         return sto;
      }
   }

}
