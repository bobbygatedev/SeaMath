using Gate.CLanguage.Compiler;
using Gate.CLanguage.DeclInterpreter;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Types;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using static Gate.CLanguage.TypeSpecifierInterpret.CTypeSpecifierInterpret;

namespace Gate.CLanguage.TypeSpecifierInterpret
{
   /// <summary>
   /// 
   /// </summary>
   public class CTypeSpecifierInterpreterStructUnion : CTokenInterpreter
   {
      private And myAnd;

      public CTypeSpecifierInterpreterStructUnion(
         CTypeUserTag structOrUnion,
         CAttributesInterpret attributesInterpret,
         CExprStatementInterpreter exprInterpret,
         UsageId usage,
         CDeclInterpretFactory declInterpretFactory)
      {
         StructOrUnion = structOrUnion;
         AttributesInterpret = attributesInterpret;
         ExprInterpret = exprInterpret;
         Usage = usage;
         DeclInterpretFactory = declInterpretFactory;
         myAnd = new And(
            new IterateWhileSuccess(AttributesInterpret),
            new CTypeSpecifierInterpretUserTypeCheck(structOrUnion),
            new IterateWhileSuccess(AttributesInterpret),
            new Or(
               new InnerClassCompleterInterpret(this, usage),
               new CTypeSpecifierIncompleteInterpreter(structOrUnion, usage),
               ///this is for trigger error in case previous or return <see cref="TxtElabResult.continue_searching"/>
               new IdSetInterpreter(IdSetInterpreter.ModeType.required)));
      }

      private class InnerClassCompleterInterpret : CTokenInterpreter
      {
         private And myAnd;

         public InnerClassCompleterInterpret(CTypeSpecifierInterpreterStructUnion parent, UsageId usage)
         {
            myAnd = new And(
               new IdSetInterpreter(IdSetInterpreter.ModeType.optional_success),
               new Is("{", true),
               new May(new BodyInterpret(usage, parent, parent.AttributesInterpret)),
               new Expect("}", true));
            Parent = parent;
            Usage = usage;
         }

         private class BodyInterpret : CTokenInterpreter
         {
            private readonly CDeclInterpret myStructItemInterpreter;

            public BodyInterpret(UsageId usage, CTypeSpecifierInterpreterStructUnion parent, CAttributesInterpret attributesInterpret)
            {
               Usage = usage;
               Parent = parent;
               AttributeInterpret = attributesInterpret;
               myStructItemInterpreter = new CDeclInterpret(
                  CDeclInterpretContext.cclass_field,
                  parent.DeclInterpretFactory,
                  parent.ExprInterpret,
                  attributesInterpret);
            }

            public CAttributesInterpret AttributeInterpret { get; }

            public UsageId Usage { get; }

            public CTypeSpecifierInterpreterStructUnion Parent { get; }

            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
            {
               var cls = output.PeekOrCrash<CTypeStruct>();

               var res = myNestedIterate(cls.StructBody, input, inData, ref output, myStructItemInterpreter, inp => inp.MarkedText == "}");

               if (res == TxtElabResult.success && cls.Members.Length == 0 && !inData.Settings.AreEmptyStructAccepted)
               {
                  inData.Messages.Add(CCompilerMsgs.EmptyStruct(cls?.TxtToken ?? throw new Crash()));

                  return TxtElabResult.failure;
               }

               return res;
            }
         }

         /// <summary>
         /// 
         /// </summary>
         public CTypeSpecifierInterpreterStructUnion Parent { get; }

         /// <summary>
         /// 
         /// </summary>
         public UsageId Usage { get; }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="input"></param>
         /// <param name="inData"></param>
         /// <param name="output"></param>
         /// <returns></returns>
         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var cls = new CTypeStruct(Parent.StructOrUnion);

            SetOutputTypeBase(Usage, output ?? throw new Crash(), cls, 0);

            var res = myNested(cls, input, inData, ref output, myAnd, NestedMode.once_continue);

            if (res != TxtElabResult.success) { SetOutputTypeBase(Usage, output ?? throw new Crash(), null, 0); }

            return res;
         }
      }

      public CTypeUserTag StructOrUnion { get; }

      public CAttributesInterpret AttributesInterpret { get; }
      public CExprStatementInterpreter ExprInterpret { get; }
      public UsageId Usage { get; }
      public CDeclInterpretFactory DeclInterpretFactory { get; }

      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         var beg_idx = input.CurrIdx;

         var res = myAnd.Perform(input, inData, ref output);

         if (res == TxtElabResult.success)
         {
            var typ = GetOutputTypeBase(Usage, output) ?? throw new Crash();

            typ.TxtToken =
               typ is CTypeStruct ||
                  (typ is CTypeIncomplete inc && inc.IncompleteKind == StructOrUnion) ?
               input.GetTokenFrom(beg_idx) as TxtToken : throw new Crash();
         }

         return res;
      }

      public override string ToString() => $"{GetType().Name}({Usage})";
   }
}
