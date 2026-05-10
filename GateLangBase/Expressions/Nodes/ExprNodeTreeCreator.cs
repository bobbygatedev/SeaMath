using Gate.LangBase.Expressions.Operators;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Message;
using Gate.Tools.Text.Elab;

namespace Gate.LangBase.Expressions.Nodes
{
   /// <summary>
   /// 
   /// </summary>
   internal class ExprNodeTreeCreator
   {
      public ExprNodeTreeCreator(Operator[] operators) => OperatorsOrdered = operators.OrderBy(o => o.PrecedenceClass.PrecedenceGroup).ToArray();

      public Operator[] OperatorsOrdered { get; }

      public bool Go(ExprNode[] exprNodesIn, MsgCollection messages, IRtmObjStrategy rtmStrategy, out ExprNode[]? exprNodesOut)
      {
         var lst_nds = new List<ExprNode>(exprNodesIn);

         ////array of subexpression
         var sub_exs = exprNodesIn.OfType<SubExpr>().SelectMany(se => se.AllDescendant.OfType<SubExpr>()).ToArray();

         exprNodesOut = null;

         if (!myCreateNodeTree(lst_nds, messages, rtmStrategy)) { return false; }

         //sub-expr with 0 nodes (eg ())
         foreach (var sub_exp in sub_exs.Where(se => se.SubExprNodes.Length > 0))
         {
            var lst_sub_nds = sub_exp.SubExprNodes.ToList();

            sub_exp.ClearSubNodes();

            var res = myCreateNodeTree(lst_sub_nds, messages, rtmStrategy);

            //sub-expr nodes are re-added in case of error also for debug purposes
            sub_exp.AddSubNodes(lst_sub_nds.ToArray());

            if (!res) { return false; }
         }

         exprNodesOut = lst_nds.ToArray();

         return true;
      }

      private static int myUpdateIndex(IGrouping<PrecedenceClass, Operator> operatorGroup, int oldExpressionNodeIndex) =>
         operatorGroup.Key.Associativity == Associativity.left2right ? oldExpressionNodeIndex + 1 : oldExpressionNodeIndex - 1;

      private bool myCreateNodeTreeStepPrecedenceClass(
         List<ExprNode> listExprNode, MsgCollection messages, IRtmObjStrategy rtmStrategy, IGrouping<PrecedenceClass, Operator> operatorGroup)
      {
         var dir = operatorGroup.Key.Associativity;
         var ops = operatorGroup.ToArray();
         var exp_nod_idx = dir == Associativity.left2right ? 0 : listExprNode.Count - 1;

         //walk on expression forward(associativity left-2-right) or backward(associativity right-2-left)
         while (exp_nod_idx >= 0 && exp_nod_idx < listExprNode.Count)
         {
            var fnd_opr = new FindOperatorInData(listExprNode, exp_nod_idx, messages, rtmStrategy);
            var opr_nod = null as ExprNodeOperator;
            var opn_nds = null as ExprNode[];
            var res = TxtElabResult.continue_searching;

            //is already assigned
            if ((listExprNode[exp_nod_idx] as ExprNodeOperator)?.Operator != null) { exp_nod_idx = myUpdateIndex(operatorGroup, exp_nod_idx); }
            else
            {
               var opr = ops.FirstOrDefault(o => (res = o.FindOperatorNode(fnd_opr, out opr_nod, out opn_nds)) != TxtElabResult.continue_searching);

               switch (res)
               {
                  case TxtElabResult.failure:
                  case TxtElabResult.failure_unrecoverable:
                     return false;

                  case TxtElabResult.continue_searching:
                     //update expression idx 
                     exp_nod_idx = myUpdateIndex(operatorGroup, exp_nod_idx);
                     break;

                  case TxtElabResult.success:
                     //if operator (node) is successfully found then ..

                     // .. operator is inserted in the node list if is not contained:
                     // this is the case of eg operator call(f()) and all operators which are syntactically identified and not by punctuator (such as '+')
                     if (!listExprNode.Contains(opr_nod ?? throw new Crash())) { listExprNode.Insert(exp_nod_idx, opr_nod); }

                     // .. its operands are removed from list
                     var num_nod_rem = listExprNode.RemoveAll(n => (opn_nds ?? []).Contains(n));

                     //check if operand nodes already belonging to node list
                     if (num_nod_rem != (opn_nds ?? []).Length) { throw new Crash("Nodes not belonging to expression nodes collection!"); }

                     //adding operands to operator node
                     opr_nod.AddOperands((opn_nds ?? []));
                     opr_nod.@Operator = opr;
                     exp_nod_idx = myUpdateIndex(operatorGroup, listExprNode.IndexOf(opr_nod));
                     break;

                  default: throw new Crash();
               }
            }
         }

         return true;
      }


      private bool myCreateNodeTree(List<ExprNode> listExprNode, MsgCollection messages, IRtmObjStrategy rtmStrategy)
      {
         if (OperatorsOrdered.GroupBy(o => o.PrecedenceClass).All(og => myCreateNodeTreeStepPrecedenceClass(listExprNode, messages, rtmStrategy, og)))
         {
            switch (listExprNode.Count)
            {
               case 0: throw new Crash();//there is a bug

               case 1:
                  if (listExprNode[0] is ExprNodeOperand || listExprNode[0] is ExprNodeOperator nod_opr && nod_opr.Operator != null)
                  {
                     return true;
                  }
                  else
                  {
                     messages.Add(ExprSolverMessages.OperatorWithoutOperand((ExprNodeOperator)listExprNode[0]));

                     return false;
                  }
               default:
                  var aft_tok =
                     listExprNode[0].AllDescendant.
                     OfType<ExprNode>().
                     Select(n => n.Token).
                     OrderBy(t => t?.To?.StoreIdx ?? 0).
                     LastOrDefault() ??
                     throw new Crash();

                  messages.Add(ExprSolverMessages.ExpectedOperatorAfter(aft_tok));

                  return false;
            }
         }
         else
         {
            return false;
         }
      }
   }
}
