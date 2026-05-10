using Gate.Tools.Message;

namespace Gate.Tools.Text.Elab
{
   /// <summary>
   ///
   /// </summary>
   public abstract class TokenInterpreter<IN_DATA, OUTPUT> : TxtElab<TxtTokenList, IN_DATA, OUTPUT>
      where IN_DATA : TxtElabInData
      where OUTPUT : class, ICloneable, new()
   {
      /// <summary>
      /// Seek for a list with at least a member.
      /// </summary>
      public class Lst : TokenInterpreter<IN_DATA, OUTPUT>
      {
         private And myCompose;

         /// <summary>
         /// Constructor with separator.
         /// </summary>
         /// <param name="separator"></param>
         /// <param name="subInterprers"></param>
         /// <param name="isSeparatorAtEndValid">If true an expression like 'a,b,c,' is valid</param>
         public Lst(string separator, TxtElab<TxtTokenList, IN_DATA, OUTPUT> subInterprer, bool isSeparatorAtEndValid)
         {
            SubInterpreter = subInterprer;
            //success if at least in item.
            myCompose = subInterprer & new IterateWhileSuccess(new Is(separator, true) & subInterprer);

            if (isSeparatorAtEndValid)
            {
               myCompose &= new May(new Is(separator, true));
            }

            Separator = separator;
         }

         /// <summary>
         /// Constructor without separator.
         /// </summary>
         /// <param name="subInterprer"></param>
         public Lst(TxtElab<TxtTokenList, IN_DATA, OUTPUT> subInterprer)
         {
            SubInterpreter = subInterprer;
            //success if at least in item.
            myCompose = subInterprer & new IterateWhileSuccess(new Or(subInterprer));
         }

         /// <summary>
         /// 
         /// </summary>
         public TxtElab<TxtTokenList, IN_DATA, OUTPUT> SubInterpreter { get; }

         /// <summary>
         /// 
         /// </summary>
         public string Separator { get; } = "";

         /// <summary>
         /// 
         /// </summary>
         /// <param name="input"></param>
         /// <param name="inData"></param>
         /// <param name="output"></param>
         /// <returns></returns>
         public override TxtElabResult Perform(TxtTokenList input, IN_DATA inData, ref OUTPUT output) => myCompose.Perform(input, inData, ref output);

         public override string ToString() => $"{GetType().Name}({SubInterpreter}) sep({Separator})";

      }

      /// <summary>
      /// Returns always <see cref="TxtElabResult.failure"/> and add an error message based on <see cref="ErrorMsgRetriever"/>
      /// </summary>
      public class Failure : TokenInterpreter<IN_DATA, OUTPUT>
      {
         public Failure(Func<TxtToken?, Msg[]> errorMsgRetriever) => ErrorMsgRetriever = errorMsgRetriever;

         public Func<TxtToken?, Msg[]> ErrorMsgRetriever { get; }

         public override TxtElabResult Perform(TxtTokenList input, IN_DATA inData, ref OUTPUT output)
         {
            if (!input.IsIn) { throw new Crash("Failure valid when IsIn is true only!"); }
            else
            {
               var ers = ErrorMsgRetriever.Invoke(input.Peek());

               inData.Messages.Add(ers);

               return TxtElabResult.failure;
            }
         }
      }

      /// <summary>
      /// Success if <see cref="TxtTokenList.MarkedText"/> match <see cref="What"/> otw <see cref="TxtElabResult.continue_searching"/>.
      /// </summary>
      public class Is : TokenInterpreter<IN_DATA, OUTPUT>
      {
         /// <summary>
         /// Constructor, if isMove is true increments CurrIdx if token.Content == what.
         /// </summary>
         /// <param name="what">Text <see cref="TxtTokenList.MarkedText"/> shall match.</param>
         /// <param name="isMove"><see cref="TxtTokenList.CurrIdx"/> shall be incremented?</param>
         public Is(string what, bool isMove)
         {
            What = what;
            IsMove = isMove;
         }

         /// <summary>
         /// <see cref="TxtTokenList.MarkedText"/> shall match.
         /// </summary>
         public string What { get; }

         /// <summary>
         /// Whether <see cref="TxtTokenList.CurrIdx"/> shall be incremented or not.
         /// </summary>
         public bool IsMove { get; }

         public override TxtElabResult Perform(TxtTokenList input, IN_DATA inData, ref OUTPUT output)
         {
            if (input.IsMarking(What))
            {
               if (IsMove) { input.CurrIdx++; }

               return TxtElabResult.success;
            }
            else { return TxtElabResult.continue_searching; }
         }

         public override string ToString() => $"Is({What}),{(IsMove ? "MOVE" : "NOMOVE")}";
      }
   }
}
