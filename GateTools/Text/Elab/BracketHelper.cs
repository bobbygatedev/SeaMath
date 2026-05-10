namespace Gate.Tools.Text.Elab
{
   /// <summary>
   /// 
   /// </summary>
   /// <param name="input"></param>
   /// <param name="stack"></param>
   /// <returns></returns>
   public delegate bool BracketOutputHandler(TxtTokenList input, Stack<TxtToken> stack);

   /// <summary>
   /// <br> Check bracket helper returns: </br>  
   /// <br> <see cref="TxtElabResult.success"/>             if output condition is reached </br>
   /// <br> <see cref="TxtElabResult.continue_searching"/>  end of text was reached </br>
   /// <br> <see cref="TxtElabResult.failure"/>             an bracket error condition is reached eg '(','[',')' </br>
   /// <br> At end of process input points to token where output condition is verified.</br>
   /// </summary>
   public class BracketHelper
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="bracketPairs"></param>
      /// <param name="outputPostCondition">When returning true exit from <see cref="Perform(TxtTokenList, IN_DATA, ref OUTPUT)"/></param>
      public BracketHelper(
         (string, string, TxtElabResult missReturn)[] bracketPairs,
         bool successIfAtEnd,
         BracketOutputHandler? outputPreCondition = null,
         BracketOutputHandler? outputPostCondition = null)
      {
         BracketPairs = bracketPairs;
         SuccessIfAtEnd = successIfAtEnd;
         OutputPreCondition = outputPreCondition;
         OutputPostCondition = outputPostCondition;
      }

      /// <summary>
      /// 
      /// </summary>
      public (string, string, TxtElabResult missReturn)[] BracketPairs { get; }

      /// <summary>
      /// If true <see cref="Check(TxtTokenList, out Stack{TxtToken})"/> returns <see cref="TxtElabResult.success"/> if <see cref="TxtTokenList"/> goes to end otw returns <see cref="TxtElabResult.continue_searching"/>.
      /// </summary>
      public bool SuccessIfAtEnd { get; }

      /// <summary>
      /// 
      /// </summary>
      public BracketOutputHandler? OutputPreCondition { get; }

      /// <summary>
      /// When returning true exit from <see cref="Perform(TxtTokenList, IN_DATA, ref OUTPUT)"/>.  
      /// </summary>
      public BracketOutputHandler? OutputPostCondition { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="input"></param>
      /// <param name="inData"></param>
      /// <param name="output"></param>
      /// <returns></returns>
      public TxtElabResult Check(TxtTokenList input, out Stack<TxtToken> outStackToken)
      {
         outStackToken = new Stack<TxtToken>();

         while (input.IsIn)
         {
            if (
               outStackToken.Count == 0 &&
               OutputPreCondition != null &&
               OutputPreCondition.Invoke(input, outStackToken))
            {
               return TxtElabResult.success;
            }

            var ope = myOpenBracket(input);

            if (ope != null) { outStackToken.Push(ope); }
            else
            {
               var clo = myCloseBracket(input, outStackToken, out var err_act);

               if (clo != null) { outStackToken.Pop(); }
               else if (err_act.HasValue)
               {
                  return err_act.Value;
               }
            }

            if (
               outStackToken.Count == 0 &&
               OutputPostCondition != null &&
               OutputPostCondition(input, outStackToken))
            {
               return TxtElabResult.success;
            }

            input.CurrIdx++;
         }

         return SuccessIfAtEnd ? TxtElabResult.success : TxtElabResult.continue_searching;
      }

      private TxtToken? myCloseBracket(TxtTokenList input, Stack<TxtToken> stack, out TxtElabResult? result)
      {
         foreach (var pai in BracketPairs)
         {
            //if a closed bracket is matched (eg ')')..
            if (input.MarkedText == pai.Item2)
            {
               //if stack top is corresponding open brace (ie ')') 
               if (stack.Count > 0 && stack.Peek().Content == pai.Item1)
               {
                  result = null;

                  return input.MarkedToken;
               }
               else //otherwise an error condition is raised
               {
                  result = pai.missReturn;

                  return null;
               }
            }
         }

         result = null;

         return null;
      }

      private TxtToken? myOpenBracket(TxtTokenList input)
      {
         foreach (var pai in BracketPairs)
         {
            if (input.MarkedText == pai.Item1) { return input.MarkedToken; }
         }

         return null;
      }
   }
}
