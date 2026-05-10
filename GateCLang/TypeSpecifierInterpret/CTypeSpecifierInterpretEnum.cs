using Gate.CLanguage.Compiler;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Types;
using Gate.CLanguage.Types.BuiltIns;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using static Gate.CLanguage.TypeSpecifierInterpret.CTypeSpecifierInterpret;

namespace Gate.CLanguage.TypeSpecifierInterpret
{
   /// <summary>
   /// 
   /// </summary>
   public class CTypeSpecifierInterpretEnum : CTokenInterpreter
   {
      private And myAnd;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="attributesInterpret"></param>
      /// <param name="exprInterpret"></param>
      /// <param name="usage"></param>
      public CTypeSpecifierInterpretEnum(CAttributesInterpret attributesInterpret, CExprStatementInterpreter exprInterpret, UsageId usage)
      {
         Usage = usage;
         ExprInterpret = exprInterpret;
         AttributesInterpret = attributesInterpret;

         myAnd = new And(
            new IterateWhileSuccess(AttributesInterpret),
            new CTypeSpecifierInterpretUserTypeCheck(CTypeUserTag.@enum),
            new IterateWhileSuccess(AttributesInterpret),
            new Or(
               new InnerEnumCompleterInterpreter(exprInterpret, usage),
               new CTypeSpecifierIncompleteInterpreter(CTypeUserTag.@enum, usage),
               ///this is for trigger error in case previous or return <see cref="TxtElabResult.continue_searching"/>
               new IdSetInterpreter(IdSetInterpreter.ModeType.required)));
      }

      private class InnerEnumCompleterInterpreter : CTokenInterpreter
      {
         private And myAnd;
         private IdSetInterpreter myIdSetInterpreter = new IdSetInterpreter(IdSetInterpreter.ModeType.optional_success);

         public InnerEnumCompleterInterpreter(CExprStatementInterpreter exprIntepret, UsageId usage) => myAnd = new And(
            myIdSetInterpreter,
            new Is("{", true),
            new May(new Lst(",", new InnerLabelInterpret(exprIntepret, usage), true)),
            new Expect("}", true));

         public UsageId Usage { get; }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var enm = new CTypeEnum();

            SetOutputTypeBase(Usage, output ?? throw new Crash(), enm, 0);

            output.Push(enm);

            var res = myAnd.Perform(input, inData, ref output);

            if (res == TxtElabResult.success)
            {
               enm.UnderlyingIntType = inData.Settings.BuiltInSet?["int"] as CTypeBinInt ?? throw new Crash();
            }

            return res;
         }
      }

      private class InnerLabelInterpret : CTokenInterpreter
      {
         private And myCompose;

         public InnerLabelInterpret(CExprStatementInterpreter exprInterpret, UsageId usage)
         {
            myCompose = new IdSetInterpreter(IdSetInterpreter.ModeType.optional_continue) & new May(new Is("=", true) & new LabelInit(exprInterpret));
            Usage = usage;
         }

         private class LabelInit : CTokenInterpreter
         {
            private CExprStatementInterpreter myExprInterpret;

            public LabelInit(CExprStatementInterpreter exprInterpret) { myExprInterpret = exprInterpret; }

            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
            {
               myExprInterpret.OutputPreCondition = t => t?.Content == "," || t?.Content == ";";

               var res = myExprInterpret.Perform(input, inData, ref output);

               if (res == TxtElabResult.success)
               {
                  var c_exp = output.PopOrCrash<CExprStatement>();

                  if (c_exp.ConstIntValue != null)
                  {
                     var cnt = output.PeekOrCrash<CTypeEnumLabel>();

                     cnt.LabelExpr = c_exp;

                     return TxtElabResult.success;
                  }
                  else
                  {
                     throw new NotImplementedException();//not an integer constant
                  }
               }

               return res;
            }
         }

         public UsageId Usage { get; }


         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var enm = output.PeekOrCrash<CTypeEnum>();
            var lab = new CTypeEnumLabel(inData.Settings.BuiltInSet?.FirstOrDefault(b => b.TypeSpecifier == "int") ?? throw new Crash());
            var beg_idx = input.CurrIdx;

            output.Push(lab);

            var res = myCompose.Perform(input, inData, ref output);

            output.PopOrCrash<CTypeEnumLabel>();

            if (res == TxtElabResult.success)
            {
               lab.TxtToken = input.GetTokenFrom(beg_idx);

               if (!enm.AddLabel(lab, inData.Settings, inData.Messages)) { return TxtElabResult.failure; }
            }

            return res;
         }
      }

      public CAttributesInterpret AttributesInterpret { get; }

      public CExprStatementInterpreter ExprInterpret { get; }

      public UsageId Usage { get; private set; }

      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         var beg_idx = input.CurrIdx;
         var res = myAnd.Perform(input, inData, ref output);

         if (res == TxtElabResult.success)
         {
            var enm = output.PopOrCrash<CTypeEnum>();
            //set token to type enum type
            enm.TxtToken = input.GetTokenFrom(beg_idx);

            if (enm.Labels.Length == 0 && !inData.Settings.AreEmptyEnumAccepted)
            {
               inData.Messages.Add(CCompilerMsgId.empty_enum_is_invalid.GetError(enm.TxtToken));

               return TxtElabResult.failure;
            }
         }

         return res;
      }
   }
}
