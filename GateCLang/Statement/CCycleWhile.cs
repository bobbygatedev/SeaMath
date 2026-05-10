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
   public class CCycleWhile : CCycle
   {
      /// <summary>
      /// 
      /// </summary>
      public CCycleWhile() => myAddSubItem(new BodyType());

      /// <summary>
      /// 
      /// </summary>
      public class BodyType : CCycleBody
      {
         public override string Descriptor => throw new NotImplementedException();

         public override string Rebuilt => throw new NotImplementedException();

         public override bool AddToScopeSpace(CItem item, CScopeHelperBase? scopeHelper, MsgCollection messages) =>
            throw new Gate.Tools.ToolsException($"{item.GetType().Name} not allowed!");
      }

      /// <summary>
      /// 
      /// </summary>
      public class TokenInterpret : TokenInterpretBase
      {
         private readonly And myAnd;

         public TokenInterpret(CDeclInterpretFactory declInterpretFactory, CAttributesInterpret attributesInterpret, CExprStatementInterpreter exprInterpret) :
            base(declInterpretFactory, attributesInterpret, exprInterpret) =>
            myAnd =
               new Is("while", true) &
               new ConditionInterpreter(this) &
               new ContentInterpret(this);

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var cyc_whi = new CCycleWhile();
            var itm_sco = output.ScopeSpaceItem ?? throw new Crash();

            if (!itm_sco.AddToScopeSpace(cyc_whi, inData.ScopeHelper, inData.Messages)) { throw new Crash(); }

            var res = myNested(cyc_whi.Body, input, inData, ref output, myAnd, NestedMode.once_continue);

            if (res == TxtElabResult.success) { }
            else { itm_sco.RemoveFromScopeSpace(cyc_whi); }

            return res;
         }
      }

      public override string? Rebuilt => throw new NotImplementedException();

      public CExprStatement? StayExpression
      {
         get => SubItems.OfType<CExprStatement>().FirstOrDefault();
         set
         {
            myRemoveSubItem(StayExpression);
            myAddSubItem(value);
         }
      }

      public new BodyType Body => SubItems.OfType<BodyType>().First();

      public override string Descriptor => $"while({StayExpression}){myGetBodyStr(Content)}";
   }
}
