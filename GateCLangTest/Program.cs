using Gate.CLanguage.Runtime.Object;
using Gate.Tools;
using System.Diagnostics;
using System.Reflection;

namespace Gate.CLanguageTest
{
   public class Program
   {
      private static bool myIsRun = false;

      private static void ExecuteAllMains()
      {
         if (!myIsRun)
         {
            myIsRun = true;
            var css = Assembly.GetExecutingAssembly().GetTypes().
               Where(t => t.IsSubclassOf(typeof(TestBase)) && t.GetConstructor([]) != null).ToArray();


            var lst_tst = new List<TestBase>();

            foreach (var cls in css.Where(c => !c.IsNested))
            {
               var cst = cls.GetConstructor([]);
               var tc = cst?.Invoke([]) as TestBase ?? throw new Crash();

               lst_tst.Add(tc);
            }

            Console.WriteLine("All Test Executions:");

            foreach (var tst in lst_tst)
            {
               Console.WriteLine($"Executing {tst.Description}({tst.GetType().Name})");
               tst.Go();
            }

            Console.WriteLine("All Test Executions: FINISHED");

            foreach (var pai in TestBase.RootTestExecutions)
            {
               Console.WriteLine($"{pai.Item2}: {pai.Item1.Description}");
            }
         }
      }

      public static void ConfigureGlobalExceptionHandling()
      {
         AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
         {
            //int a = 2;
            //throw new Crash(e.Exception);
         };

         TaskScheduler.UnobservedTaskException += (s, e) =>
         {
            e.SetObserved();
            //throw new Crash(e.Exception);
         };

         AppDomain.CurrentDomain.UnhandledException += (s, e) =>
         {
            //int a = 2;
            //throw new Crash((Exception)e.ExceptionObject);
         };
      }

      static unsafe void Main(string[] args)
      {
         ConfigureGlobalExceptionHandling();

         CRtmObjAllocatorByPrivateHeap.GlobalOptions.IsDebugMode = true;

         ExecuteAllMains();

         Process.GetCurrentProcess().Kill();
      }
   }
}
