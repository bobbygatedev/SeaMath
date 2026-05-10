using Gate.CLanguage.Compiler;
using Gate.CLanguage.DeclInterpreter;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.Tools;
using Gate.Tools.Message;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Statement
{
   /// <summary>
   /// 
   /// </summary>
   public class CCycleDoWhile : CCycle
   {
      /// <summary>
      /// 
      /// </summary>
      public CCycleDoWhile() => myAddSubItem(new BodyType());

      /// <summary>
      /// Body for <see cref="CCycleWhile"/>
      /// </summary>
      public class BodyType : CCycleBody
      {
         public BodyType() { }

         public override string Descriptor => Rebuilt;

         public override string Rebuilt => $"do .. while({Condition})";

         public override bool AddToScopeSpace(CItem item, CScopeHelperBase? scopeHelper, MsgCollection messages) =>
            item is CStatement ? true : throw new Gate.Tools.ToolsException($"{item.GetType().Name} not allowed!");
      }

      public class TokenInterpret : TokenInterpretBase
      {
         private readonly And myAnd;

         public TokenInterpret(
            CDeclInterpretFactory declInterpretFactory, 
            CAttributesInterpret attributesInterpret, 
            CExprStatementInterpreter exprInterpret) :
            base(declInterpretFactory, attributesInterpret, exprInterpret) =>
               myAnd =
                  new Is("do", true) &
                  new ContentInterpret(this) &
                  new Expect("while", true) &
                  new ConditionInterpreter(this) &
                  new Expect(";", true);

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var cyc_do = new CCycleDoWhile();
            var itm_sco = output.ScopeSpaceItem ?? throw new Crash();

            if (!itm_sco.AddToScopeSpace(cyc_do, inData.ScopeHelper, inData.Messages)) { throw new Crash(); }

            var res = myNested(cyc_do.Body, input, inData, ref output, myAnd, NestedMode.once_continue);

            if (res == TxtElabResult.success) { }
            else { itm_sco.RemoveFromScopeSpace(cyc_do); }

            return res;
         }
      }

      public override string Rebuilt => throw new NotImplementedException();

      public CExprStatement? StayExpression { get; set; }

      public override string? Descriptor => $"do{{..}}while({StayExpression})";

      public new BodyType Body => SubItems.OfType<BodyType>().First();
   }
}
