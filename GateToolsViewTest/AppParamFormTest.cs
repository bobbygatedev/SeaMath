using Gate.ToolsView.AppParams;
using static Gate.Tools.AppParams.AppParam;

namespace Gate.ToolsViewTest
{
   internal class AppParamFormTest
   {
      class InnerOptions : Record
      {
         public InnerOptions() : base("Options")
         {

         }

         public readonly Frame1Type Frame1 = new Frame1Type();
         public readonly Frame2Type Frame2 = new Frame2Type();

         public class Frame1Type : Record
         {
            public readonly Simple<int> Option1 = new Simple<int>();
            public readonly Simple<int> Option2 = new Simple<int>();
         }

         public class Frame2Type : Record
         {
            public readonly Simple<int> Option1 = new Simple<int>();
            public readonly Simple<int> Option2 = new Simple<int>();

         }

      }

      static void Main()
      {
         var frm = new AppParamContainerForm();

         frm.PpRootRecord = new InnerOptions();
         frm.ShowDialog();
      }
   }
}
