using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text.Elab;
using System.Text;

namespace Gate.CLanguageTest
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class TestBase : HierarchicalItem
   {
      public static MsgCollection mgs = new MsgCollection();

      private static readonly List<(TestBase, TxtElabResult)> myListRootTestExecutions = new List<(TestBase, TxtElabResult)>();
      private string? myDescription;
      private bool myIsVerbose = false;

      public TestBase()
      {

      }

      /// <summary>
      /// 
      /// </summary>
      public class Group : TestBase
      {
         public Group(string description, params TestBase[] subTests) => AddSubTests(subTests);

         public Group(params TestBase[] subTests) => AddSubTests(subTests);

         protected override TxtElabResult myExecution() => TxtElabResult.success;

         protected override void myTestPreSet() { }

         /// <summary>
         /// Read all subclasses and create subtest
         /// </summary>
         /// <exception cref="NotImplementedException"></exception>
         protected void myDetectSubClassTests()
         {
            var sub_cls =
               GetType().GetNestedTypes().
                  Where(t => t.IsSubclassOf(typeof(TestBase)) && !t.IsAbstract && t.GetConstructor([]) != null).
                  Select(t => t.InstanciateOrCrash()).
                  Cast<TestBase>().ToArray();

            AddSubTests(sub_cls);
         }
      }

      public class Immediate : TestBase
      {
         private TxtElabResult myResult;

         public Immediate(TxtElabResult result, string description)
         {
            myResult = result;
            Description = description;
         }

         public Immediate(TxtElabResult result) => myResult = result;

         protected override TxtElabResult myExecution() => myResult;

         protected override void myTestPreSet() { }
      }

      protected abstract TxtElabResult myExecution();

      protected abstract void myTestPreSet();

      /// <summary>
      /// 
      /// </summary>
      public static (TestBase, TxtElabResult)[] RootTestExecutions => myListRootTestExecutions.ToArray();

      /// <summary>
      /// 
      /// </summary>
      public string Description { get => myDescription ?? GetType().Name; set => myDescription = value; }

      /// <summary>
      /// 
      /// </summary>
      public TestBase[] SubTests => SubItems.Cast<TestBase>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public TestBase[] AllTestOrdered => GetDescendantWalk(DescendantOrderEnum.pre_order).Cast<TestBase>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public TestBase? ParentTest => ParentItem as TestBase;

      /// <summary>
      /// 
      /// </summary>
      public string ReportString
      {
         get
         {
            var sb = new StringBuilder();
            var all_tst = AllTestOrdered;

            foreach (var tst in all_tst)
            {
               sb.AppendLine(new string(' ', ParenthoodDepth * 2) + $"{tst.GlobalIdx}: {tst.PassFail} {tst.Description}");
            }

            return sb.ToString();
         }
      }

      public virtual TxtElabResult Go()
      {
         myTestPreSet();

         if (IsVerbose && !(this is Group)) { Console.WriteLine($"{GlobalIdx} {Description} Executing"); }

         var res = myExecution();

         if (IsVerbose && !(this is Group)) { Console.WriteLine($"{GlobalIdx} {Description} {res.ToString().ToUpper()}\n"); }

         if (res != TxtElabResult.failure_unrecoverable)
         {
            foreach (var sub in SubTests)
            {
               var sub_res = sub.Go();

               switch (sub_res)
               {
                  case TxtElabResult.success: break;

                  case TxtElabResult.failure:
                     res = TxtElabResult.failure;
                     break;

                  case TxtElabResult.failure_unrecoverable: return res;

                  default: throw new Crash($"{res} not allowed!");
               }
            }
         }

         LastResult = res;

         if (ParentItem == null)
         {
            myListRootTestExecutions.Add((this, res));
         }

         myOnTestFinished(LastResult);

         return res;
      }

      protected virtual void myOnTestFinished(TxtElabResult? lastResult) { }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="testBase"></param>
      public void AddSubTests(params TestBase[] testBase) => myAddSubItemRange(testBase);

      /// <summary>
      /// 
      /// </summary>
      public TxtElabResult? LastResult { get; private set; }

      public string GlobalIdx
      {
         get
         {
            if (ParenthoodDepth == 0) { return "1"; }
            else
            {
               var idx_2_par = 1 + 
                  Enumerable.Range(0, ParentTest?.SubTests.Length ?? 0).
                  FirstOrDefault(i => ParentTest?.SubTests[i] == this);

               return $"{ParentTest?.GlobalIdx}.{idx_2_par}";
            }
         }
      }

      public string PassFail => LastResult == TxtElabResult.success ? "PASS" : "FAIL";

      public bool IsVerbose
      {
         get => ParentItemChain.OfType<TestBase>().Any(p => p.myIsVerbose);

         set => myIsVerbose = value;
      }

      public static TestBase[] GetTestFromSubtypes(Type classBase, Func<Type, bool>? filter = null) =>
         classBase.
         GetNestedTypesRecursively().
         Where(t => (filter?.Invoke(t) ?? false) && t.GetConstructor([]) != null).
         Select(t => t.GetConstructor([])?.Invoke([]) as TestBase ?? throw new Crash()).
         ToArray();

      public static TestBase[] GetTestFromSubtypes(Type classBase, Type filterByClassBase) =>
         GetTestFromSubtypes(classBase, t => t.IsMeOrSubClass(filterByClassBase));

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="TO"></typeparam>
      /// <typeparam name="TB"></typeparam>
      /// <returns></returns>
      public static TO[] GetTestFromSubtypes<TO, TB>() where TO : TestBase =>
         GetTestFromSubtypes(typeof(TB), t => t.IsMeOrSubClass(typeof(TO))).Cast<TO>().ToArray();
   }
}
