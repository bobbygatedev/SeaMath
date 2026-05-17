using Gate.CLanguage.Compiler;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Statement
{
   /// <summary>
   /// tododo
   /// </summary>
   public class CCycleIfElse : CCycle
   {
      /// <summary>
      /// 
      /// </summary>
      public CCycleIfElse() { }

      /// <summary>
      /// 
      /// </summary>
      public class TokenInterpret : CTokenInterpreter
      {
         public TokenInterpret(CExprStatementInterpreter exprInterpret) => ExprInterpret = exprInterpret;

         public CExprStatementInterpreter ExprInterpret { get; }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            if (input.MarkedText == "if")
            {
               throw new NotImplementedException();//todo develop
            }
            else
            {
               return TxtElabResult.continue_searching;
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override string Rebuilt => throw new NotImplementedException();

      /// <summary>
      /// 
      /// </summary>
      public CCycleIfElse[] IfElses => SubItems.Where(s => s is CCycleIfElse).OfType<CCycleIfElse>().ToArray();

      /// <summary>
      /// Condition, if null instance represents an else.
      /// </summary>
      public CExprStatement? IfExpression
      {
         get => SubItems.OfType<CExprStatement>().FirstOrDefault();
         set
         {
            myRemoveSubItem(IfExpression);
            myAddSubItem(value);
         }
      }

      public bool IsPureElse => IfExpression == null;

      public override string? Descriptor
      {
         get
         {
            if (IsPureElse) { return $"else {myGetBodyStr(base.Body)}"; }
            else if (base.Body is CCycleIfElse) { return $"else {base.Body.Descriptor}"; }
            else { return $"if({IfExpression?.Descriptor}){myGetBodyStr(base.Body)}"; }//compound
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="ifElseExpression"></param>
      public void AddElse(CCycleIfElse ifElseExpression) => myAddSubItem(ifElseExpression);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="ifElseExpression"></param>
      /// <returns></returns>
      public bool RemoveElse(CCycleIfElse ifElseExpression) => myRemoveSubItem(ifElseExpression);
   }
}
