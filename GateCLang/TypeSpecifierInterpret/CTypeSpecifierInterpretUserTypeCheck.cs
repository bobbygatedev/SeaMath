using Gate.CLanguage.Compiler;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.TokenParse;
using Gate.CLanguage.Types;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.TypeSpecifierInterpret
{
   /// <summary>
   /// 
   /// </summary>
   public class CTypeSpecifierInterpretUserTypeCheck : CTokenInterpreter
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="userType"></param>
      public CTypeSpecifierInterpretUserTypeCheck(CTypeUserTag userType) => UserType = userType;

      /// <summary>
      /// 
      /// </summary>
      public CTypeUserTag UserType { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="input"></param>
      /// <param name="inData"></param>
      /// <param name="output"></param>
      /// <returns></returns>
      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         if (input.Peek() is CToken ctk && ctk.Content == UserType.ToString())
         {
            var dcl_spf = output.PeekOrCrash<CDeclSpecifiers>();

            if (dcl_spf.TypeBase != null)
            {
               inData.Messages.Add(CCompilerMsgId.two_or_more_types_in_declation.GetError(ctk));

               return TxtElabResult.failure;
            }
            else
            {
               input.Dequeue();

               return TxtElabResult.success;
            }
         }
         else { return TxtElabResult.continue_searching; }
      }
   }
}
