using Gate.LangBase;
using Gate.Tools;
using Gate.Tools.Message;
using Gate.Tools.Programming;

namespace Gate.CLanguageTest
{
   public static class TestObjects
   {
      private static NumericConverter? myNumericConverter;
      private static CompileEnvGcc? myGccCompiler;

      public static CompileEnvGcc GccCompiler
      {
         get
         {
            if (myGccCompiler == null)
            {
               var mgs = new MsgCollection();

               mgs.Is2PlotOnConsole = true;
               myGccCompiler = new CompileEnvGcc();

               if (myGccCompiler.Register(mgs))
               {
                  LangBase.ExtraTypes.LongDouble.ForceInit(myGccCompiler);
               }
               else
               {
                  throw new Crash($"Failed to register gcc environment!");
               }
            }

            return myGccCompiler;
         }
      }

      public static NumericConverter NumericConverter
      {
         get
         {
            if (myNumericConverter == null)
            {
               myNumericConverter = NumericConverter.CImplemented.Make(GccCompiler);
               NumericConverter.StdImpl = myNumericConverter;
            }

            return myNumericConverter;
         }
      }
   }
}
