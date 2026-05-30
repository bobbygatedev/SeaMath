using Gate.CLanguage.Compiler;
using Gate.CLanguage.DeclInterpreter;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.LangBase.Runtime;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System.Text;

namespace Gate.CLanguage.Statement
{
   /// <summary>
   /// Encapsulates a if() else if() else cycle.
   /// </summary>
   public class CStatementIfElse : CStatementConditional
   {
      private CStatement? myElseBody;

      /// <summary>
      /// 
      /// </summary>
      public CStatementIfElse() { }

      /// <summary>
      /// 
      /// </summary>
      public class TokenInterpret : TokenInterpretBase
      {
         private readonly And myAnd;
         private readonly ContentInterpret myContentInterpret;

         public TokenInterpret(
            CDeclInterpretFactory declInterpretFactory,
            CAttributesInterpret attributesInterpret,
            CExprStatementInterpreter exprInterpret) :
            base(declInterpretFactory, attributesInterpret, exprInterpret)
         {
            myAnd =
               new Is("if", true) &
               new ConditionInterpreter(this) &
               (myContentInterpret = new ContentInterpret(this));
         }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            if (input.MarkedText != "if") { return TxtElabResult.continue_searching; }

            var top_itm = output.TopItem;
            var c_sco = top_itm as CStatementCompound;
            var cyc = top_itm as CStatementConditional;

            var cyc_if = new CStatementIfElse();

            if (c_sco != null) { c_sco.AddStatements(cyc_if); }
            else if (cyc != null) { cyc.SetBody(cyc_if); }
            else { throw new Crash(); }

            while (true)
            {
               var res = myNested(cyc_if, input, inData, ref output, myAnd, NestedMode.once_continue);

               if (res != TxtElabResult.success)
               {
                  if (c_sco != null) { c_sco.RemoveFromScopeSpace(cyc_if); }
                  else if (cyc != null) { cyc.SetBody(null); }
                  else { throw new Crash(); }

                  return res;
               }
               else if (input.MarkedText == "else")
               {
                  input.CurrIdx++;

                  if (input.MarkedText == "if")
                  {
                     cyc_if.SetBody(cyc_if = new CStatementIfElse());
                     continue;
                  }
                  else
                  {
                     //else body
                     res = myContentInterpret.Perform(input, inData, ref output);

                     if (c_sco != null)
                     {
                        var sta = 
                           c_sco.NnOrCrash().Content.LastOrDefault() as CStatement ?? 
                           throw new RtmException("Expected a statement ");

                        c_sco.NnOrCrash().RemoveFromScopeSpace(sta.NnOrCrash());
                        cyc_if.ElseBody = sta;
                     }
                     else 
                     {
                        cyc.NnOrCrash().SetBody(cyc_if);
                     }
                  }

                  return res;
               }
               else { return TxtElabResult.success; }
            }

         }
      }

      /// <summary>
      /// 
      /// </summary>
      public CStatement? ElseBody
      {
         get => myElseBody;

         private set
         {
            myRemoveSubItem(myElseBody);
            myAddSubItem(myElseBody = value);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public CStatementIfElse[] IfElses
      {
         get
         {
            var lst = new List<CStatementIfElse>();

            if (myElseBody is CStatementIfElse eli)
            {
               lst.AddRange(new[] { eli }.Concat(eli.IfElses));
            }

            return lst.ToArray();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override string Rebuilt
      {
         get
         {
            var sb = new StringBuilder();

            sb.Append($"if({StayConditionExpr?.Rebuilt})");

            if (Body is CStatementCompound c1)
            {
               sb.AppendLine();
               sb.AppendLine(c1.Rebuilt);
            }
            else
            {
               sb.AppendLine(Body?.Rebuilt);
            }

            if (IfElses.Length > 0)
            {
               foreach (var eli in IfElses)
               {
                  sb.Append("else ");
                  sb.Append(eli.Rebuilt);
               }
            }
            else if (ElseBody is CStatementCompound c2)
            {
               sb.AppendLine("else");
               sb.AppendLine(c2.Rebuilt);
            }
            else if (ElseBody != null)
            {
               sb.AppendLine("else" + ElseBody.Rebuilt);
            }

            return sb.ToString();
         }
      }

      public override string? Descriptor
      {
         get
         {
            var sb = new StringBuilder();

            sb.Append($"if({StayConditionExpr?.Descriptor})");

            if (Body is CStatementCompound c1)
            {
               sb.AppendLine();
               sb.AppendLine(c1.Descriptor);
            }
            else
            {
               sb.AppendLine(Body?.Descriptor);
            }


            if (IfElses.Length > 0)
            {
               foreach (var eli in IfElses)
               {
                  sb.Append("else ");
                  sb.Append(eli.Descriptor);
               }
            }
            else if (ElseBody is CStatementCompound cmp)
            {
               sb.AppendLine("else");
               sb.AppendLine(cmp.Descriptor);
            }
            else if (ElseBody != null)
            {
               sb.AppendLine("else" + ElseBody.Descriptor);
            }

            return sb.ToString();
         }
      }

      /// <summary>
      /// If body is null 
      /// </summary>
      /// <param name="body"></param>
      public override void SetBody(CStatement? body)
      {
         if (body == null)
         {
            //empties both else and if condition
            ElseBody = null;
            base.SetBody(body);
         }
         else if (Body == null)
         {
            base.SetBody(body);
         }
         else
         {
            ElseBody = body;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="ifElseExpression"></param>
      /// <returns></returns>
      public bool RemoveElse(CStatementIfElse ifElseExpression) => myRemoveSubItem(ifElseExpression);
   }
}
