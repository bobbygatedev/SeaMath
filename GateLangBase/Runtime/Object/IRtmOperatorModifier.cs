using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
using Gate.LangBase.Runtime.DbgEng;

namespace Gate.LangBase.Runtime.Object
{
   /// <summary>
   /// Allows to selectively override normal behaviour of an <see cref="Operator"/>
   /// </summary>
   public interface IRtmOperatorModifier
   {
      /// <summary>
      /// Perform operator if result is not null otherwise normal behaviour is performed.
      /// </summary>
      /// <param name="operatorNode"></param>
      /// <param name="rtmArgs"></param>
      /// <param name="rtmStrategy"></param>
      /// <returns></returns>
      RtmObj? EvalRtmArgsModified(ExprNodeOperator operatorNode, RtmObj?[]? rtmArgs, IRtmObjStrategy? rtmStrategy, IRtmDbgEngStackExecutable? stack);
   }
}

