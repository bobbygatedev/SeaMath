using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;

namespace Gate.LangBase.Expressions.Operators
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class OperatorPunctuator : Operator
   {
      /// <summary>
      /// 
      /// </summary>
      protected OperatorPunctuator() { }

      /// <summary>
      /// 
      /// </summary>
      public abstract string Punctuator { get; }

      /// <summary>
      /// 
      /// </summary>
      public override string Symbol => Punctuator;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="operatorNode"></param>
      /// <param name="rtmArgs"></param>
      /// <param name="rtmStrategy"></param>
      /// <param name="stack"></param>
      /// <returns></returns>
      public override RtmObj? EvalRtmArgs(ExprNodeOperator operatorNode,
         RtmObj?[]? rtmArgs, IRtmObjStrategy? rtmStrategy, IRtmDbgEngStackExecutable? stack)
      {
         var res_val = base.EvalRtmArgs(operatorNode, rtmArgs, rtmStrategy, stack);
         var ar0 = rtmArgs?.ElementAtOrDefault(0) ?? throw new Crash();

         if (IsFirstOperandLValue)
         {
            //then is a operator of type '+=', '*=', ...
            rtmStrategy?.Assign(ar0, res_val ?? throw new Crash());
         }

         //return a l-value ie a var ref like in a[5] operator subscript 
         return IsReturningLValue ? rtmStrategy?.MakeRefValue(ar0, operatorNode.DeclType ?? throw new Crash()) : res_val;
      }
   }
}
