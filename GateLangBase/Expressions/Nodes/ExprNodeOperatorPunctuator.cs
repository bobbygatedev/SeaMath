using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.LangBase.Expressions.Nodes
{
   public static class ExprNodeOperatorPunctuator<IN_DATA> where IN_DATA : TxtElabInData
   {
      public interface ICommon
      {
         string[] Punctuators { get; }
      }

      public class Interpreter<INT_OUT> : ExprNodeInterpret<IN_DATA, INT_OUT>, ICommon
         where INT_OUT : class, ICloneable, new()
      {
         public Interpreter(string[] punctuators) => Punctuators = punctuators.Distinct().OrderByDescending(p => p.Length).ToArray();

         public string[] Punctuators { get; }

         public override TxtElabResult Perform(TxtTokenList input, IN_DATA inData, ref ExprNodeOutput<INT_OUT> output)
         {
            if (input.IsIn && Punctuators.Contains(input.Peek()?.Content))
            {
               output.ListProduct.Add(new ExprNodeOperator(input.Dequeue() ?? throw new Crash()));

               return TxtElabResult.success;
            }

            return TxtElabResult.continue_searching;
         }
      }

      public class Parser<INT_OUT> : ExprNodeParser<IN_DATA, INT_OUT>, ICommon
         where INT_OUT : class, ICloneable, new()
      {
         public Parser(string[] punctuators) => Punctuators = punctuators.Distinct().OrderByDescending(p => p.Length).ToArray();

         public string[] Punctuators { get; }

         public override TxtElabResult Perform(TxtMarker input, IN_DATA inData, ref ExprNodeOutput<INT_OUT> output)
         {
            var sgn = input.GetMarkingSign(Punctuators);

            if (sgn != null)
            {
               output.ListProduct.Add(new ExprNodeOperator(input.GetMarkingToken(sgn.Length)));

               return TxtElabResult.success;
            }

            return TxtElabResult.continue_searching;
         }
      }
   }
}
