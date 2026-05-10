using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.LangBase.Expressions.Operators
{
   [BasicOperator(BasicOperatorTypeFlags.c_operator)]
   public class OperatorCall : Operator
   {
      public override string Symbol => "f()";

      public override bool IsFirstOperandLValue => false;

      public override bool IsReturningLValue => false;

      public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(1);

      public override TxtElabResult FindOperatorNode(
         FindOperatorInData findOperatorData, out ExprNodeOperator? operatorNode, out ExprNode[]? operandNodes)
      {
         operatorNode = null;
         operandNodes = null;

         //eg shall be here '2 + ->f(a,b)'
         if (
            findOperatorData.CurrExprNode is ExprNodeOperand &&
            findOperatorData.NextExprNode is SubExpr &&
            (findOperatorData.NextExprNode as SubExpr)?.BracketOpen.Content == "(")
         {
            //creates a new operator node associated to operator call (ie this) and between
            operatorNode = new ExprNodeOperator(
               TxtTokenConst.FromTokenInterval(
                  findOperatorData.CurrExprNode?.Token ?? throw new Crash(),
                  findOperatorData.NextExprNode?.Token ?? throw new Crash()));
            operandNodes = [findOperatorData.CurrExprNode, findOperatorData.NextExprNode];

            return TxtElabResult.success;
         }

         return TxtElabResult.continue_searching;
      }

      /// <summary>
      /// Different from <see cref="Operator.Eval(ExprNodeOperator, IRtmDbgEngStackExecutable, IRtmObjStrategy)"/> 
      /// this method evaluates rtm-values before calling <see cref="EvalRtmArgs"/>.
      /// </summary>
      /// <param name="operatorNode"></param>
      /// <param name="stack"></param>
      /// <param name="rtmStrategy"></param>
      /// <returns></returns>
      public override RtmObj? Eval(ExprNodeOperator operatorNode, IRtmDbgEngStackExecutable? stack, IRtmObjStrategy? rtmStrategy)
      {
         var op1 = operatorNode.OperandNodes[0];
         var op2 = operatorNode.OperandNodes[1] as SubExpr ?? throw new Crash();

         var fnc = op1.Eval(stack, rtmStrategy) ?? throw new Gate.LangBase.Runtime.RtmException("Expected a function as operand #1");
         var cal_ars = op2.EvalForFunctionCall(stack, rtmStrategy);
         var rtm_ars = new[] { fnc }.Concat(cal_ars).OfType<RtmObj>().ToArray();

         var res = EvalRtmArgs(operatorNode, rtm_ars, rtmStrategy, stack);

         return res;
      }

      /// <summary>
      /// Take the first argument as function to call and the others as arguments to pass to function
      /// </summary>
      /// <param name="operatorNode"></param>
      /// <param name="rtmArgs">[function arg0 arg1 ..]</param>
      /// <param name="rtmStrategy"></param>
      /// <param name="stack"></param>
      /// <returns></returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
      public override RtmObj? EvalRtmArgs(
         ExprNodeOperator operatorNode, RtmObj?[]? rtmArgs, IRtmObjStrategy? rtmStrategy, IRtmDbgEngStackExecutable? stack)
      {
         var sub_exp =
            operatorNode.OperandNodes[1] as SubExpr ??
            throw new Gate.LangBase.Runtime.RtmException($"Not found subexpression for operator {Symbol}");

         var var_nam = rtmArgs?.ElementAtOrDefault(0)?.VarName;

         var fnc = (rtmStrategy ?? throw new Gate.LangBase.Runtime.RtmException($"RTM Strategy can't be null in this context!")).
            GetFunction(rtmArgs?.FirstOrDefault()) ??
            throw new Gate.LangBase.Runtime.RtmException($"Arg#0 {var_nam} not a function!");

         var dcl_fun = fnc.DeclFunction ?? throw new Gate.LangBase.Runtime.RtmException($"Not a function associated to {var_nam}");

         //value of function input parameters (skipping first argument which is the function itself)
         var arg_rtm_vls = rtmArgs?.Skip(1).ToArray();

         //copies of arguments to function input parameters
         var arg_rtm_cps = Enumerable.Range(0, arg_rtm_vls?.Length ?? 0).Select(i =>
         {
            //declaration parameters
            var dcl_par = i < fnc.DeclFunction.Parameters.Length ? fnc.DeclFunction.Parameters[i] : null;
            var cpy_par_val = null as RtmObj;
            var par_val = arg_rtm_vls?.ElementAtOrDefault(i) ?? throw new Crash();

            if (dcl_par != null)
            {
               cpy_par_val = (dcl_par.DeclType ?? throw new Crash()).IsReference ?
                  rtmStrategy.CopyFunctionParamByRef(par_val, dcl_par) :
                  rtmStrategy.CopyFunctionParamByValue(par_val, dcl_par);
            }
            else //optional parameters ( eg printf(const char*,...); )
            {
               cpy_par_val = rtmStrategy.CopyFunctionOptionalParamByValue(par_val);
            }

            return cpy_par_val;
         }).ToArray();

         //notice parameter are passed as are they are handling of by value/by ref is demanded to function Invoke 
         return fnc.Exec(stack, rtmStrategy, arg_rtm_cps);
      }

      public override ValueType? CSharpHandler(params dynamic[] ins) => null;
   }
}
