using Gate.CLanguage.Compiler;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Interpreter;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.DeclInterpreter
{
   /// <summary>
   /// <br> Check termination of a <see cref="CDeclSpecifiers"/>: it shall be a semicolon (';') or an empty <see cref="CDeclSpecifiers"/>.</br>
   /// <br> If <see cref="CDeclSpecifiers"/> is empty </br>
   /// </summary>
   public class CDeclInterpreterSemicolonCheck : CTokenInterpreter
   {
      private readonly Expect myExpect = new Expect(";", true);
      private readonly Is myMay = new Is(";", true);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="input"></param>
      /// <param name="inData"></param>
      /// <param name="output"></param>
      /// <returns></returns>
      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         var dcl_spc = output.PeekOrCrash<CDeclSpecifiers>();

         ///<see cref="CDeclSpecifiers"/> is empty input can point other than semicolon(;)
         if (dcl_spc.IsEmpty) { return myMay.Perform(input,inData,ref output); }
         else { return myExpect.Perform(input, inData, ref output); }///this will cause an error <see cref="Gate.CLanguage.Compiler.CCompilerMsgs.CCompilerMsgId.c018_expected_token"/>
      }
   }
}
