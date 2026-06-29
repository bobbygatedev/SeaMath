using Gate.CLanguage;
using Gate.LangBase.Runtime.Object;
using Gate.SeaMath.Console;
using Gate.SeaMath.Sea;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text.Elab;
using Gate.ToolsView.ConIO;

namespace Gate.SeaMath.Windows.Console
{
   /// <summary>
   /// Represents a dummy command for the SeaMath console, primarily used for testing or placeholder purposes.
   /// </summary>
   /// <remarks>This command does not perform any supervisory actions and is always considered successful when
   /// parsing input. It interacts with the <see cref="SeaMathConsole"/> instance to execute input and manage console
   /// tasks.</remarks>
   public class SeaMathConsoleDummyCommand : ConsoleCmd
   {
      public SeaMathConsoleDummyCommand(SeaMathConsole console) => Console = console;

      public override bool IsSupervisor => false;

      public SeaMathConsole Console { get; }

      public override ConsoleCmdHint[] Hints
      {
         get
         {
            var c_hns = Console?.
               PreCompiledHeader?.
               AllDescendant.
               OfType<CItem>().
               SelectMany(i => i.GetHints()).
               Nn().
               ToArray();

            var ojs = Console?.ObjVisibleForConsole ?? [];
            var fns = ojs.OfType<IRtmObjFunction>().ToArray();
            var vrs = ojs.Except(fns.Cast<RtmObj>()).ToArray();
            var hns = ojs.Select(o => o.GetHint()).Nn().ToArray();

            return (c_hns ?? []).Concat(hns).ToArray();
         }
      }

      public override void OnFinished() { }

      protected override object? myCmdBody(ConsoleTask consoleTask, params dynamic[] @params)
      {
         var mgs = new MsgCollection();
         var txt = @params[0];

         return Console.ExecuteInput(txt, mgs);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="text"></param>
      /// <param name="inParams"></param>
      /// <returns></returns>
      public override TxtElabResult Parse(string text, out object[] inParams)
      {
         //dummy command always successfull
         inParams = new[] { text }; ///passes text to <see cref="myCmdBody(ConsoleTask, dynamic[])"/>

         return TxtElabResult.success;
      }

      public override void AbortAction(ConsoleTask.AbortActionParams abortParams) => Console.VirtThread?.Kill();
   }
}
