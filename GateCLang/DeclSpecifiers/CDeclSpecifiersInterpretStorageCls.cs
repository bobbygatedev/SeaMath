using Gate.CLanguage.Compiler;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.TokenParse;
using Gate.CLanguage.Types;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.DeclSpecifiers
{
   /// <summary>
   /// 
   /// </summary>
   public class CDeclSpecifiersInterpretStorageCls : CTokenInterpreter
   {
      private static string[] myStorageClassLabels = Enum.GetNames(typeof(CTypeStorageClass)).ToArray();

      private And myComposed = new And(
         new IsCTokenType(CTokenType.keyword, false),
         new Condition(tl => myStorageClassLabels.Contains(tl?.Peek()?.Content), false),
         new Inner());

      private class Inner : CTokenInterpreter
      {
         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var dcl = output.PeekOrCrash<CDeclSpecifiers>();
            var stg_cls = (CTypeStorageClass)Enum.Parse(typeof(CTypeStorageClass), input.Dequeue()?.Content ?? throw new Crash());

            if (dcl.StorageClass == 0)
            {
               dcl.StorageClass = stg_cls;

               return TxtElabResult.success;
            }
            else
            {
               input.CurrIdx--;
               inData.Messages.Add(CCompilerMsgs.DuplicatedStorageClass(input.Peek()));

               return TxtElabResult.failure;
            }
         }
      }

      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output) => myComposed.Perform(input, inData, ref output);
   }
}
