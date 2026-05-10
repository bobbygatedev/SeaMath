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
   /// Check presence of function specifier inside a function declaration definition (not used in typedef, cast)
   /// </summary>
   public class CDeclSpecifiersInterpretFunction : CTokenInterpreter
   {
      private static string[] myFunctionSpecifierLabels = Enum.GetNames(typeof(CFunctionSpecifier)).ToArray();

      private And myComposed = new And(
         new IsCTokenType(CTokenType.keyword, false),
         new Condition(tl => myFunctionSpecifierLabels.Contains(tl.Peek()?.Content), false),
         new InnerWriteFlag());

      private class InnerWriteFlag : CTokenInterpreter
      {
         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var cdc = output.PeekOrCrash<CDeclSpecifiers>();
            var fnc_spc = (CFunctionSpecifier)Enum.Parse(typeof(CFunctionSpecifier), input.Dequeue()?.Content ?? throw new Crash());

            cdc.FunctionSpecifier |= fnc_spc;

            return TxtElabResult.success;
         }
      }

      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output) =>
         myComposed.Perform(input, inData, ref output);
   }
}
