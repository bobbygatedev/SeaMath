namespace Gate.ToolsViewTest
{
   public partial class ExtendedLineControlTest : Form
   {
      private double myElapsed = 0;

      private readonly List<InnerAction> myListActions = new List<InnerAction>();

      public ExtendedLineControlTest()
      {
         InitializeComponent();
      }

      private class InnerAction
      {
         public InnerAction(Action<InnerAction> action, double interval, bool isPeriodic)
         {
            Action = action;
            Interval = interval;
            IsPeriodic = isPeriodic;
            NextInvoke = Interval;
         }

         public bool IsDone { get; private set; } = false;

         public Action<InnerAction> Action { get; }
         public double Interval { get; }
         public bool IsPeriodic { get; }
         public double NextInvoke { get; private set; }

         public void Perform(double elapsed)
         {
            if (myIsToDo(elapsed))
            {
               Action(this);
            }
         }

         private bool myIsToDo(double elapsed)
         {
            if (!IsDone && elapsed >= NextInvoke)
            {
               if (IsPeriodic)
               {
                  NextInvoke += Interval;
               }
               else
               {
                  IsDone = true;
               }

               return true;
            }
            else
            {
               return false;
            }
         }
      }

      private void ExtendedLineControlTest_Load(object sender, EventArgs e)
      {
         var nl = 11;
         var ai = 0;

         for (int i = 1; i <= nl; i++)
         {
            var ln = string.Join(" ", Enumerable.Range(1, 10).Select(j => $"Line{i}.{j}"));

            CtrlExtendedLineControl.MthLinesAdd(ln);
         }

         CtrlExtendedLineControl.PpLineCurrentId = 10;
         //CtrlExtendedLineControl.PpSelectionInterval = ( new TxtPos(10,9), new TxtPos(10, 16));// new Interval(8, 15);

         var il = 1;

         myListActions.Add(new InnerAction(a =>
         {
            if (ai == 0 && il <= 11)
            {
               CtrlExtendedLineControl[il++] = $"Suca{il - 1}";
            }
            else
            {
               ai = 1;
               il = 1;

               if (CtrlExtendedLineControl.PpLineCount > 0)
               {
                  CtrlExtendedLineControl.MthLineRemoveRange(new[] { 1 });
               }
            }
         }, 1.0, true));
      }

      static void Main()
      {
         var frm = new ExtendedLineControlTest();

         frm.ShowDialog();
      }

      private void CtrlTimer_Tick(object sender, EventArgs e)
      {
         myElapsed += CtrlTimer.Interval * 1e-3;

         foreach (var act in myListActions)
         {
            act.Perform(myElapsed);
         }
      }
   }
}
