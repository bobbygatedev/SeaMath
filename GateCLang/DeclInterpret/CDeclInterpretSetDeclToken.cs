using Gate.CLanguage.Compiler;
using Gate.CLanguage.Decl;
using Gate.CLanguage.Interpreter;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.DeclInterpreter
{
   /// <summary>
   /// Set declaration <see cref="TxtToken"/>, doesn't perform parsing (always return <see cref="TxtElabResult.success"/>.
   /// </summary>
   public class CDeclInterpretSetDeclToken : CTokenInterpreter
   {
      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         var dcl = output.PeekOrCrash<CDecl>();

         if (inData.AppData[CDeclInterpret.BeginningTokenIdData] is int tok_id)
         {
            dcl.TxtToken = input.GetTokenFrom(tok_id);

            return TxtElabResult.success;
         }
         else
         {
            throw new Crash();
         }
      }
   }
}
