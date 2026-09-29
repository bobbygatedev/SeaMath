 namespace GateToolsViewTest
{
   public partial class CustomCaptionTestForm : Form
   {
      public CustomCaptionTestForm()
      {
         InitializeComponent();
      }

      static void Main(string[] args)
      {
         var frm = new CustomCaptionTestForm();

         frm.ShowDialog();
      }
   }
}
