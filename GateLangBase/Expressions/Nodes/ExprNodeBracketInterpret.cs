using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.LangBase.Expressions.Nodes
{
   /// <summary>
   ///
   /// </summary>
   /// <typeparam name="IN_DATA"></typeparam>
   public static class ExprNodeBracket<IN_DATA> where IN_DATA : TxtElabInData
   {
      public interface ICommon
      {
         (string open, string close)[] BracketPairs { get; }

         string[] Brackets { get; }
      }

      public class Interpreter<INT_OUT> : ExprNodeInterpret<IN_DATA, INT_OUT>, ICommon
         where INT_OUT : class, ICloneable, new()
      {
         public Interpreter((string open, string close)[] bracketPairs)
         {
            BracketPairs = bracketPairs;
            Brackets = BracketPairs.SelectMany(p => new[] { p.open, p.close }).ToArray();
         }

         public (string open, string close)[] BracketPairs { get; }

         public string[] Brackets { get; }

         public override TxtElabResult Perform(TxtTokenList input, IN_DATA inData, ref ExprNodeOutput<INT_OUT> output)
         {
            var mrk_txt = input.MarkedText;

            if (BracketPairs.Any(p => p.open == mrk_txt))
            {
               //add open 
               output.ListProduct.Add(new ExprNodeBracket(input.Dequeue() ?? throw new Crash()));

               return TxtElabResult.success;
            }
            else if (BracketPairs.Any(p => p.close == mrk_txt))
            {
               var cor_ope_par = BracketPairs.First(p => p.close == mrk_txt).open;
               var ope_par_nod =
                  output.ListProduct.LastOrDefault(p => p is ExprNodeBracket && p.Content == cor_ope_par) as ExprNodeBracket;

               if (ope_par_nod == null)
               {
                  //this in the case bracket are used as terminator
                  //eg 'char a[->128];'
                  // epxr will mark at ]' at end
                  return TxtElabResult.continue_searching;
               }
               else
               {
                  var clo_par_nod = new ExprNodeBracket(input.Dequeue().NnOrCrash());

                  myExtractSubExpression(output, ope_par_nod, clo_par_nod);

                  return TxtElabResult.success;
               }
            }
            else { return TxtElabResult.continue_searching; }
         }
      }

      public class Parser<INT_OUT> : ExprNodeParser<IN_DATA, INT_OUT>, ICommon
         where INT_OUT : class, ICloneable, new()
      {
         public Parser((string open, string close)[] bracketPairs)
         {
            BracketPairs = bracketPairs;
            Brackets = BracketPairs.SelectMany(p => new[] { p.open, p.close }).OrderBy(b => b.Length).ToArray();
         }

         public (string open, string close)[] BracketPairs { get; }

         public string[] Brackets { get; }

         public override TxtElabResult Perform(TxtMarker input, IN_DATA inData, ref ExprNodeOutput<INT_OUT> output)
         {
            var bra = input.GetMarkingSign(Brackets);

            if (bra == null) { return TxtElabResult.continue_searching; }
            else if (BracketPairs.Any(b => b.open == bra))
            {
               output.ListProduct.Add(new ExprNodeBracket(input.GetMarkingToken(bra.Length)));
               input.CurrIdx += bra.Length;

               return TxtElabResult.success;
            }
            else if (BracketPairs.Any(p => p.close == bra))
            {
               var cor_ope_par = BracketPairs.First(p => p.close == bra).close;
               var ope_par_nod = output.ListProduct.FirstOrDefault(p => p is ExprNodeBracket && p.Content == cor_ope_par) as ExprNodeBracket;

               if (ope_par_nod == null)
               {
                  //this in the case bracket are used as terminator
                  //eg 'char a[->128];'
                  // epxr will mark at ]' at end
                  return TxtElabResult.continue_searching;
               }
               else
               {
                  var clo_par_nod = new ExprNodeBracket(input.GetMarkingToken(bra.Length));

                  myExtractSubExpression(output, ope_par_nod, clo_par_nod);
                  input.CurrIdx += bra.Length;

                  return TxtElabResult.success;
               }
            }

            return TxtElabResult.continue_searching;
         }
      }


      /// <summary>
      /// <br> Removes all output nodes from bracket open, then creates a sub expression and add it to output list</br>
      /// <br>eg output = 3 [ a + 2  <paramref name="nodeOpen"/>= '[' <paramref name="nodeClose"/>=']' becomes 3 subexpr( [ a + 2 ] ) </br>
      /// </summary>
      /// <param name="output"></param>
      /// <param name="nodeOpen"></param>
      /// <param name="nodeClose"></param>
      private static void myExtractSubExpression<INT_OUT>(ExprNodeOutput<INT_OUT> output, ExprNodeBracket nodeOpen, ExprNodeBracket nodeClose)
         where INT_OUT : class, ICloneable, new()
      {
         var ope_idx = output.ListProduct.IndexOf(nodeOpen);
         var sub_nds = output.ListProduct.Skip(ope_idx + 1).ToArray();

         output.ListProduct.RemoveRange(ope_idx, output.ListProduct.Count - ope_idx);

         var sub_exp = new SubExpr(nodeOpen, nodeClose, sub_nds ?? []);

         output.ListProduct.Add(sub_exp);
      }
   }
}
