using Gate.Tools.Message;

namespace Gate.Tools.Text.Elab
{
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="IN_DATA"></typeparam>
   /// <typeparam name="OUTPUT"></typeparam>
   public abstract class ParserStep<IN_DATA, OUTPUT> : TxtElab<TxtMarker, IN_DATA, OUTPUT>
      where IN_DATA : TxtElabInData
      where OUTPUT : class, ICloneable, new()
   {
      public delegate Msg MessageGetter(TxtPos? fileErrPos);

      /// <summary>
      /// Seek for a list with at least a member.
      /// </summary>
      public class List : ParserStep<IN_DATA, OUTPUT>
      {
         private And myCompose;

         /// <summary>
         /// Constructor without separator.
         /// </summary>
         /// <param name="separator"></param>
         /// <param name="subParsers"></param>
         public List(string separator, params TxtElab<TxtMarker, IN_DATA, OUTPUT>[] subParsers)
         {
            SubParsers = subParsers.All(i => i != null) ? subParsers : throw new Crash();

            myCompose = new Or(subParsers) & new IterateWhileSuccess(new IsSign(separator) & new Or(subParsers));
            Separator = separator;
         }

         /// <summary>
         /// Constructor without separator.
         /// </summary>
         /// <param name="subParsers"></param>
         public List(params TxtElab<TxtMarker, IN_DATA, OUTPUT>[] subParsers)
         {
            SubParsers = subParsers;
            myCompose = new Or(subParsers) & new IterateWhileSuccess(new Or(subParsers));
         }

         /// <summary>
         /// 
         /// </summary>
         public TxtElab<TxtMarker, IN_DATA, OUTPUT>[] SubParsers { get; }

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
         public override TxtElabResult Perform(TxtMarker input, IN_DATA inData, ref OUTPUT output) => myCompose.Perform(input, inData, ref output);

         public override string ToString() => $"{GetType().Name}({string.Join("|", SubParsers.Select(sb => sb.ToString()))}) sep({Separator})";
      }


      public class Failure : ParserStep<IN_DATA, OUTPUT>
      {
         private MessageGetter myOnFailureMsg;

         public Failure(MessageGetter onError) => myOnFailureMsg = onError;

         public override TxtElabResult Perform(TxtMarker marker, IN_DATA inData, ref OUTPUT output)
         {
            if (!marker.MoveToNextNoSpace()) { marker.CurrIdx = marker.Store.Content.Length - 1; }

            inData.Messages.Add(myOnFailureMsg.Invoke(marker?.CurrPos?.Primitive));

            return TxtElabResult.failure;
         }
      }

      public class IsWord : ParserStep<IN_DATA, OUTPUT>
      {
         public IsWord(string word) => Word = word;

         public override TxtElabResult Perform(TxtMarker marker, IN_DATA inData, ref OUTPUT output)
         {
            var sav = marker.CurrIdx;
            var var_nam = marker.GetMarkingWord();

            if (var_nam == Word)
            {
               marker.MoveOf(var_nam.Length);

               return TxtElabResult.success;
            }
            else
            {
               marker.CurrIdx = sav;

               return TxtElabResult.continue_searching;
            }
         }

         public string Word { get; }
      }

      public class IsFinished : ParserStep<IN_DATA, OUTPUT>
      {
         public IsFinished()
         {
            
         }

         public override TxtElabResult Perform(TxtMarker marker, IN_DATA inData, ref OUTPUT output) =>
            marker.MoveToNextNoSpace() ? TxtElabResult.continue_searching : TxtElabResult.success;
      }

      public class IsSign : ParserStep<IN_DATA, OUTPUT>
      {
         public IsSign(string sign) => Sign = sign;

         public override TxtElabResult Perform(TxtMarker marker, IN_DATA inData, ref OUTPUT output)
         {
            var sav = marker.CurrIdx;

            if (marker.IsMarkingAnySignMoveOver(Sign)) { return TxtElabResult.success; }
            else
            {
               marker.CurrIdx = sav;

               return TxtElabResult.continue_searching;
            }
         }

         public string Sign { get; }
      }

      public class IsVarName : ParserStep<IN_DATA, OUTPUT>
      {
         public IsVarName(string varName) => VarName = varName;

         public string VarName { get; private set; }

         public override TxtElabResult Perform(TxtMarker marker, IN_DATA inData, ref OUTPUT output)
         {
            var sav = marker.CurrIdx;
            var var_nam = marker.GetMarkingVarNameMoveOver();

            if (var_nam == VarName) { return TxtElabResult.success; }
            else
            {
               marker.CurrIdx = sav;

               return TxtElabResult.continue_searching;
            }
         }
      }
   }
}
