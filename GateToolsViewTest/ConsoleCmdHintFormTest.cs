using Gate.ToolsView.ConIO;

namespace Gate.ToolsViewTest
{
   public class ConsoleCmdHintFormTest
   {
      static void Main()
      {
         //sort test
         {
            var frm = null as ConsoleCmdHintListForm;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var lst = new List<ConsoleCmdHint>();

            for (int i = 0; i < 10; i++)
            {
               {
                  var hnt = new ConsoleCmdHint();

                  hnt.ImageId = ConsoleCmdHint.ImageType.var;
                  hnt.HintId = "var";
                  hnt.HintText = "var";
                  hnt.HintFormat = "var";

                  lst.Add(hnt);
               }

               {
                  var hnt = new ConsoleCmdHint();

                  hnt.ImageId = ConsoleCmdHint.ImageType.type;
                  hnt.HintId = "structPippo";
                  hnt.HintText = "typedef struct {int dummy;} structPippo;";
                  hnt.HintFormat = "var";

                  lst.Add(hnt);
               }
               {
                  var hnt = new ConsoleCmdHint();

                  hnt.ImageId = ConsoleCmdHint.ImageType.func;
                  hnt.HintText = "void func(int p1)";
                  hnt.HintId = "func";
                  hnt.HintFormat = "func([p1])";

                  lst.Add(hnt);
               }
            }

            var f1 = new Form();

            f1.GotFocus += (s, e) =>
            {
               frm = new ConsoleCmdHintListForm();
               frm.MthShow(f1, new System.Drawing.Point(), lst.ToArray(), "");
               frm.BringToFront();
            };

            Application.Run(f1);

            Console.WriteLine($"Selected {frm?.PpHintSelected}");
               
            return;
         }
      }
   }
}
