using Gate.CLanguage.Compiler;
using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Types;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.DeclInterpreter
{
   /// <summary>
   /// 
   /// </summary>
   public class CDeclInterpretTryToAddSpecifier : CTokenInterpreter
   {
      /// <summary>
      /// 
      /// </summary>
      public CDeclInterpretTryToAddSpecifier() { }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="input"></param>
      /// <param name="inData"></param>
      /// <param name="output"></param>
      /// <returns></returns>
      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         var dcl = output.PeekOrCrash<CDecl>();
         var dcl_spc = output.PeekOrCrash<CDeclSpecifiers>(1);

         //set the token
         dcl.TxtToken = input.GetTokenFrom(inData.AppData[CDeclInterpret.BeginningTokenIdData].ConvertOrCrash<int>());
      
         if (dcl_spc.AddDecl(dcl, inData.Messages, inData.ScopeHelper))
         {
            if (!dcl.IsTypedef)
            {
               //variable(not typedef) can't be incomplete
               var pri_ali = dcl.TypeAlias.PrimitiveAlias;

               if (pri_ali.TypeSubscriptSet.SubscriptCount == 0 && pri_ali.TypeBase is CTypeIncomplete inc && inc.CompleteType == null)
               {
                  inData.Messages.Add(CCompilerMsgs.IncompleteTypeNotAllowed(inc));

                  return TxtElabResult.failure;
               }
            }

            return TxtElabResult.success;
         }

         return TxtElabResult.failure;
      }
   }
}
