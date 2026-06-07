using Gate.LangBase.Expressions.Operators;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.LangBase.Expressions.Nodes
{
   /// <summary>
   /// <br>Operand node variable variable and needed to be evaluated.</br> 
   /// <br>If IsPureIdentifier is true the node is just an identifier(like a class/struct/union member) and evaluate returns null. </br>
   /// </summary>
   public class ExprNodeOperandVariable : ExprNodeOperand, IWithIdentifier
   {
      private IDecl? myDecl = null;

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="token"></param>
      public ExprNodeOperandVariable(TxtToken token) : base(token) => Identifier = token.Content;

      public abstract class Parser<IN_DATA, INT_OUT> : ExprNodeParser<IN_DATA, INT_OUT>
         where INT_OUT : class, ICloneable, new()
         where IN_DATA : TxtElabInData
      {
         protected Parser() { }

         public class Standard : Parser<IN_DATA, INT_OUT>
         {
            public Standard() { }

            public override string? GetMarkingIdentifier(TxtMarker txtMarker) => txtMarker.GetMarkingVarName();
         }

         public abstract string? GetMarkingIdentifier(TxtMarker txtMarker);

         public override TxtElabResult Perform(TxtMarker input, IN_DATA inData, ref ExprNodeOutput<INT_OUT> output)
         {
            var var_nam = GetMarkingIdentifier(input);

            if (var_nam != null)
            {
               var var_nod = new ExprNodeOperandVariable(input.GetMarkingToken(var_nam.Length));

               output.ListProduct.Add(var_nod);
               input.CurrIdx += var_nam.Length;

               return TxtElabResult.success;
            }

            return TxtElabResult.continue_searching;
         }
      }

      public abstract class Interpreter<IN_DATA, INT_OUT> : ExprNodeInterpret<IN_DATA, INT_OUT>
         where IN_DATA : TxtElabInData
         where INT_OUT : class, ICloneable, new()
      {
         protected Interpreter() { }

         public class Standard : Interpreter<IN_DATA, INT_OUT>
         {
            public Standard() { }

            public override bool IsTokenIdentifier(TxtToken token, IN_DATA inData, ref ExprNodeOutput<INT_OUT> output)
            {
               var mat = TxtMarker.RegexForVarName.Match(token.Content);

               return mat.Success && mat.Length == token.Length;
            }
         }

         public abstract bool IsTokenIdentifier(TxtToken token, IN_DATA inData, ref ExprNodeOutput<INT_OUT> output);

         public override TxtElabResult Perform(TxtTokenList input, IN_DATA inData, ref ExprNodeOutput<INT_OUT> output)
         {
            var pee_tok = input.Peek();

            if (pee_tok != null && IsTokenIdentifier(pee_tok, inData, ref output))
            {
               var var_nod = new ExprNodeOperandVariable(input.Dequeue() ?? throw new Crash());

               output.ListProduct.Add(var_nod);

               return TxtElabResult.success;
            }

            return TxtElabResult.continue_searching;
         }
      }

      /// <summary>
      /// <see cref="IDecl"/> associated to variable can be null in not typized languages such MATLAB
      /// </summary>
      public IDecl? Decl
      {
         get => myDecl;
         set
         {
            if (value == null) { myDecl = null; }
            else if (value.Identifier == null) { throw new Gate.LangBase.Expressions.ExprSolverException("Decl id can't be null"); }
            else if (value.Identifier == Identifier) { myDecl = value; }
            else { throw new Gate.LangBase.Expressions.ExprSolverException($"Decl id {value.Identifier} different fro m node id {Identifier}."); }
         }
      }

      /// <summary>
      /// Identifier(var name) of operand node.
      /// </summary>
      public string? Identifier { get; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsAnonimous => Identifier.IsBlank();

      /// <summary>
      /// Always true.
      /// </summary>
      public override bool IsRtmValue => !IsClassMember;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsAnOperand => true;

      /// <summary>
      /// Equal to <see cref="Decl"/>.DeclType
      /// </summary>
      public override IDeclType? DeclType => Decl?.DeclType;

      /// <summary>
      /// Always an l-value unless <see cref="IsClassMember"/> is true.
      /// </summary>
      public override bool IsLValue => !IsClassMember;

      /// <summary>
      /// 
      /// </summary>
      public override string? Rebuilt => Identifier;

      /// <summary>
      /// If true variable when variable is member name (eg is second operand of a struct/class/union member operator)
      /// </summary>
      public bool IsClassMember => ParentExprNode is ExprNodeOperator opr_nod && opr_nod.Operator is OperatorMember && opr_nod.OperandNodes[1] == this;

      /// <summary>
      ///  
      /// </summary>
      /// <param name="stack"></param>
      /// <returns></returns>
      /// <exception cref="Gate.LangBase.Expressions.ExprSolverException"></exception>
      public override RtmObj? Eval(RtmDbgEngStackVirtCpu? stack, IRtmObjStrategy? rtmStrategy)
      {
         var vis_ojs = rtmStrategy?.GetFunctionVisibleObject(stack) ?? [];
     
         //associate expression declaration to runtime object
         //if language is 'declared' (vars have explicit)
         var rtm_obj =
            Decl != null ?
               vis_ojs.FirstOrDefault(v => v.Decl == Decl || Decl.Linkage == v.Decl) :
               vis_ojs.FirstOrDefault(v => v.VarName == Identifier);

         if (rtm_obj != null)
         {
            rtm_obj.Module?.InitIfNecessary(stack);

            return rtm_obj;
         }

         throw new Gate.LangBase.Expressions.ExprSolverException($"Can't find a runtime object for '{Identifier}'.");
      }

      public override ExprNode GetCopy() => new ExprNodeOperandVariable(Token?.GetConstCopy() ?? throw new Crash());
   }
}
