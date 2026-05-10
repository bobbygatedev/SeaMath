using Gate.CLanguage.Compiler;
using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Types;
using Gate.CLanguage.Types.BuiltIns;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.DeclInterpreter
{
   /// <summary>
   /// 
   /// </summary>
   public class CDeclInterpreterBitField : CTokenInterpreter
   {
      public CDeclInterpreterBitField(CExprStatementInterpreter exprInterpret) => ExprInterpret = exprInterpret;

      public CExprStatementInterpreter ExprInterpret { get; }

      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         /// if not a ':' pipe line <see cref="TxtElab{TxtTokenList, CCompilerInData, CTokenInterpreterOutput}.And"/>
         /// must go on
         if (input.MarkedText != ":") { return TxtElabResult.success; }
         else
         {
            var dcl_spc = output.PeekOrCrash<CDeclSpecifiers>(1);
            var pri_ali = (new CTypeAlias(dcl_spc.TypeBase)).PrimitiveAlias;

            if (!(pri_ali.TypeBase is CTypeBinInt bin))
            {
               inData.Messages.Add(CCompilerMsgs.BitFieldNotAnInteger(dcl_spc.TxtToken));

               return TxtElabResult.failure;
            }
            else
            {
               var tok = input.Dequeue();

               //exit at end of 'int bf:2;'
               ExprInterpret.OutputPreCondition = i => i?.Content == ";";

               var res = ExprInterpret.Perform(input, inData, ref output);

               if (res == TxtElabResult.success)
               {
                  var exp = output.PopOrCrash<CExprStatement>();
                  var var = output.PeekOrCrash<CDeclClassField>();

                  if (!exp.ConstIntValue.HasValue)
                  {
                     inData.Messages.Add(CCompilerMsgs.BitFieldNotAnInteger(dcl_spc.TxtToken));

                     return TxtElabResult.failure;
                  }
                  else if (exp.ConstIntValue > bin.SizeOf * 8)
                  {
                     inData.Messages.Add(CCompilerMsgs.BitFieldToomManyBits(dcl_spc.TxtToken));

                     return TxtElabResult.failure;
                  }
                  else
                  {
                     var.BitFieldExpr = exp;

                     return TxtElabResult.success;
                  }
               }
               else
               {
                  inData.Messages.Add(CCompilerMsgs.BitFieldNotAnInteger(dcl_spc.TxtToken));

                  return TxtElabResult.failure;
               }
            }
         }
      }
   }
}
