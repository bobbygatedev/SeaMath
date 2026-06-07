using Gate.CLanguage.Runtime;
using Gate.CLanguage.Runtime.Object;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;

namespace Gate.CLanguage.Expressions.Operators
{
   public abstract class COperatorPointer : OperatorUnary
   {
      protected COperatorPointer()
      {

      }

      public override ValueType? CSharpHandler(params dynamic[] ins) => null;

      [BasicOperator(BasicOperatorTypeFlags.c_operator)]
      public class Dereference : COperatorPointer
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(3);

         public override string Punctuator => "*";

         public override bool IsReturningLValue => true;

         public override bool IsPrefix => true;

         public override bool IsPostfix => false;

         public override bool IsFirstOperandLValue => false;

         public override RtmObj? EvalRtmArgs(
            ExprNodeOperator operatorNode, RtmObj?[]? rtmArgs, IRtmObjStrategy? rtmStrategy, RtmDbgEngStackVirtCpu? stack)
         {
            var str = rtmStrategy as CRtmObjStrategy ?? throw new Crash();

            return str.Dereference(
               rtmArgs?.FirstOrDefault() ?? 
               throw new Gate.LangBase.Runtime.RtmException("Required at least an argument"));
         }
      }

      [BasicOperator(BasicOperatorTypeFlags.c_operator)]
      public class AddressOf : COperatorPointer
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(3);

         public override bool IsReturningLValue => false;

         public override string Punctuator => "&";

         public override bool IsPrefix => true;

         public override bool IsPostfix => false;

         public override string Symbol => $"{Punctuator}()";

         public override bool IsFirstOperandLValue => true;

         public override RtmObj? EvalRtmArgs(
            ExprNodeOperator operatorNode, RtmObj?[]? rtmArgs, IRtmObjStrategy? rtmStrategy, RtmDbgEngStackVirtCpu? stack) =>
            (rtmStrategy ?? throw new Gate.LangBase.Runtime.RtmException($"RTm-Strategy required in this context!")).
               MakeConstant((rtmArgs?.FirstOrDefault() as CRtmObj)?.Address ?? 0, operatorNode.DeclType ?? throw new Crash());
      }
   }
}
