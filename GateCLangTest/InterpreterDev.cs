using Gate.CLanguage.Standards;
using Gate.Tools.Message;
using Gate.Tools.Text.Elab;
using System;

namespace Gate.CLanguageTest
{
   public class InterpreterDev : TestWithSource
   {
      protected unsafe override TxtElabResult myExecution()
      {
         var txt_fil_sto = myAddResourceFile("interpret_dev.c");
         var mgs = new MsgCollection();
         var std = new CStandardC99();
         var cmp = std.CCompiler;

         var res = cmp.Compile(txt_fil_sto, mgs, out _);

         mgs.PlotOnConsole();

         return res ? TxtElabResult.success : TxtElabResult.failure;
      }

      protected override void myTestPreSet() { }

      public static void Main(string[] args)
      {
         var tst = new InterpreterDev();

         tst.Go();
         Console.WriteLine(tst.ReportString);
      }
   }
}

