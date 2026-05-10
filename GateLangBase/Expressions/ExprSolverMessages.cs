using Gate.LangBase.Expressions.Nodes;
using Gate.Tools.Message;
using Gate.Tools.Text;

namespace Gate.LangBase.Expressions
{
   /// <summary>
   /// 
   /// </summary>
   public static class ExprSolverMessages
   {
      private static CompilerMessagesTools myMsgTools = new CompilerMessagesTools("EXP");

      public enum Id
      {
         [CompilerMessage(Message = "Not an l-value")]
         not_an_lvalue = 1,
         open_bracket_not_closed = 2,
         operator_without_operand = 3,
         operand_without_operator = 4,
         identifier_not_found = 5,
         no_matching_bracket = 6,
         no_open_bracket = 7,
         end_of_file_reached = 8,
         cant_assign = 9,
         
         [CompilerMessage(Message = "Identifier expected after struct member")]
         identifier_expected_after_member = 10,
         
         not_found_valid_operators_for_operand = 11,

         [CompilerMessage(Message = "Expected an operand")]
         operand_expected = 12,
      }

      public static Msg NotAnLValue(TxtToken? token) => myMsgTools.MakeMsg<Id>(MsgType.error, Id.not_an_lvalue, token);
      public static Msg OpenBracketNotClosed(TxtToken? token) =>
         myMsgTools.MakeMsg2(
            MsgType.error, Id.open_bracket_not_closed, token, $"Open bracket '{token?.Content}' not closed.");

      public static Msg OperatorWithoutOperand(ExprNodeOperator operatorNode) =>
         myMsgTools.MakeMsg2(
            MsgType.error,
            Id.operator_without_operand,
            operatorNode.Token,
            $"Operator '{operatorNode.Token.Content}' without operand.");

      public static Msg ExpectedOperatorAfter(TxtToken afterToken) =>
         myMsgTools.MakeMsg2(
            MsgType.error,
            Id.operand_without_operator,
            afterToken,
            $"Expected an operator after '{afterToken.Content}'.");

      public static Msg IdentifierNotFound(TxtToken? token) =>
         myMsgTools.MakeMsg2(
            MsgType.error,
            Id.identifier_not_found,
            token,
            $"Id '{token?.Content}' undeclared.");

      public static Msg[] NotMatchingBracket(TxtToken? openToken, TxtToken wrongToken, bool isFatal) => [
            myMsgTools.MakeMsg2(
               isFatal ? MsgType.fatal : MsgType.error ,
               Id.no_matching_bracket,
               wrongToken,
               "No matching bracket.") ,
            myMsgTools.MakeMsg2(
               MsgType.info ,
               Id.no_matching_bracket,
               openToken,
               $"      Open token was '{openToken}'.")];

      public static Msg NoOpenBracket(TxtToken? token) => 
         myMsgTools.MakeMsg(MsgType.fatal, Id.no_open_bracket, token);

      public static Msg ExpectedAnIdAfterMemberOperand(TxtToken? token) =>
         myMsgTools.MakeMsg(MsgType.error, Id.identifier_expected_after_member, token);

      public static Msg NotFoundValidOperatorsForOperand(TxtToken? token) =>
         myMsgTools.MakeMsg(MsgType.error, Id.not_found_valid_operators_for_operand, token);

      public static Msg ExpectedAnOperand(TxtToken? token) => myMsgTools.MakeMsg(MsgType.error, Id.operand_expected, token);
   }
}
