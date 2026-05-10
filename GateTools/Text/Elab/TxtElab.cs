namespace Gate.Tools.Text.Elab
{
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="INPUT">Input text streamer must implement int R/W property 'Position'.</typeparam>
   /// <typeparam name="IN_DATA"></typeparam>
   /// <typeparam name="OUTPUT"></typeparam>
   public abstract class TxtElab<INPUT, IN_DATA, OUTPUT>
      where INPUT : class, ITxtElabInput
      where OUTPUT : class, ICloneable, new()
      where IN_DATA : TxtElabInData
   {
      /// <summary>
      /// Look forward for a set conditions: the condition which is verified for the lowest index is perfomed. 
      /// </summary>
      public abstract class LookForward : TxtElab<INPUT, IN_DATA, OUTPUT>
      {
         public class Combine : LookForward
         {
            public Combine(params LookForward[] subLookForwards) => SubLookForwards = subLookForwards.ToArray();

            public LookForward[] SubLookForwards { get; }

            public override TxtElabResult Perform(INPUT input, IN_DATA inData, ref OUTPUT output)
            {
               var cur_idx = input.CurrIdx;
               var min_idx = int.MaxValue;
               var min_loo_fw = null as LookForward;

               foreach (var sub_loo_fw in SubLookForwards)
               {
                  input.CurrIdx = cur_idx; //restores idx position

                  var beg_idx = sub_loo_fw.GetLookFwIdx(input, inData);

                  beg_idx = beg_idx < 0 ? int.MaxValue : beg_idx;

                  if (beg_idx < min_idx)
                  {
                     min_idx = beg_idx;
                     min_loo_fw = sub_loo_fw;
                  }
               }

               if (min_loo_fw != null)
               {
                  input.CurrIdx = min_idx;

                  var sta_res = min_loo_fw.PerformWhenLookFw(input, inData, ref output);

                  return sta_res;
               }
               else { return TxtElabResult.continue_searching; }
            }

            /// <summary>
            /// Not used in this implememtation.
            /// </summary>
            public override int GetLookFwIdx(INPUT input, IN_DATA inData) { throw new NotImplementedException(); }

            /// <summary>
            /// Not used in this implememtation.
            /// </summary>
            /// <param name="input"></param>
            /// <param name="inData"></param>
            /// <param name="output"></param>
            /// <returns></returns>
            public override TxtElabResult PerformWhenLookFw(INPUT input, IN_DATA inData, ref OUTPUT output) { throw new NotImplementedException(); }
         }

         public abstract int GetLookFwIdx(INPUT input, IN_DATA inData);

         public override TxtElabResult Perform(INPUT input, IN_DATA inData, ref OUTPUT output)
         {
            var loo_fw_idx = GetLookFwIdx(input, inData);

            return loo_fw_idx >= 0 && loo_fw_idx < int.MaxValue ?
               PerformWhenLookFw(input, inData, ref output) : TxtElabResult.continue_searching;
         }

         public abstract TxtElabResult PerformWhenLookFw(INPUT input, IN_DATA inData, ref OUTPUT output);
      }

      public class Success : TxtElab<INPUT, IN_DATA, OUTPUT>
      {
         public override TxtElabResult Perform(INPUT input, IN_DATA inData, ref OUTPUT output) => TxtElabResult.success;
      }

      public class May : TxtElab<INPUT, IN_DATA, OUTPUT>
      {
         public May(TxtElab<INPUT, IN_DATA, OUTPUT> textElab) => TextElab = textElab;

         public TxtElab<INPUT, IN_DATA, OUTPUT> TextElab { get; }

         public override TxtElabResult Perform(INPUT input, IN_DATA inData, ref OUTPUT output)
         {
            var res = TextElab.Perform(input, inData, ref output);

            switch (res)
            {
               case TxtElabResult.success:
               case TxtElabResult.continue_searching:
                  return TxtElabResult.success;

               case TxtElabResult.failure:
               case TxtElabResult.failure_unrecoverable:
                  return res;

               default: throw new Crash();
            }
         }

         public override string ToString() => $"May({TextElab})";
      }

      public abstract class Composed : TxtElab<INPUT, IN_DATA, OUTPUT>
      {
         public Composed(params TxtElab<INPUT, IN_DATA, OUTPUT>[] subElabs)
         {
            if (subElabs.All(s => s != null)) { SubElabs = subElabs.ToArray(); }
            else { throw new Crash(); }
         }

         public TxtElab<INPUT, IN_DATA, OUTPUT>[] SubElabs { get; private set; }

         public override string ToString() => $"{GetType().Name}({string.Join(",", SubElabs.Select(s => s.ToString()))})";
      }

      public static Or operator |(TxtElab<INPUT, IN_DATA, OUTPUT> t1, TxtElab<INPUT, IN_DATA, OUTPUT> t2) => new Or(t1, t2);
      public static And operator &(TxtElab<INPUT, IN_DATA, OUTPUT> t1, TxtElab<INPUT, IN_DATA, OUTPUT> t2) => new And(t1, t2);

      public class Or : Composed
      {
         public Or(params TxtElab<INPUT, IN_DATA, OUTPUT>[] subElabs) : base(myMergeOrs(subElabs)) { }

         private static TxtElab<INPUT, IN_DATA, OUTPUT>[] myMergeOrs(TxtElab<INPUT, IN_DATA, OUTPUT>[] inElabs) =>
            inElabs.SelectMany(e => e is Or or ? myMergeOrs(or.SubElabs) : [e]).ToArray();

         public override TxtElabResult Perform(INPUT input, IN_DATA inData, ref OUTPUT output)
         {
            try
            {
               var cur_str_idx = input.CurrIdx;
               var cln_out = output.Clone() as OUTPUT ?? throw new Crash();

               foreach (var ela in SubElabs)
               {
                  var res = ela.Perform(input, inData, ref output);

                  if (res == TxtElabResult.continue_searching)
                  {
                     output = cln_out ?? throw new Crash();
                     cln_out = output.Clone() as OUTPUT ?? throw new Crash();//recreate a clone
                     input.CurrIdx = cur_str_idx;
                  }
                  else { return res; }
               }

               return TxtElabResult.continue_searching;
            }
            catch (ThreadInterruptedException) { throw; }
            catch (Exception exc) { throw new Crash("Exception not allowed inside TextElab.Perform method!", exc); }
         }
      }

      /// <summary>
      /// <br>Iterate <see cref="SubTextElab"/> while it returns <see cref="TxtElabResult.success"/> </br> 
      /// <br>- returns <see cref="TxtElabResult.success"/> if at least iteration is successfully or <see cref="IsAtLeastAnItemRequired"/> is false </br>
      /// <br>- in case of failure iteration stops and <see cref="TxtElabResult.failure"/> is returned</br>
      /// </summary>
      public class IterateWhileSuccess : TxtElab<INPUT, IN_DATA, OUTPUT>
      {
         /// <summary>
         /// Constructor
         /// </summary>
         /// <param name="subTextElab"></param>
         /// <param name="isAtLeastAnItemRequired"></param>
         /// <exception cref="Crash"></exception>
         public IterateWhileSuccess(TxtElab<INPUT, IN_DATA, OUTPUT> subTextElab, bool isAtLeastAnItemRequired = false)
         {
            SubTextElab = subTextElab ?? throw new Crash();
            IsAtLeastAnItemRequired = isAtLeastAnItemRequired;
         }

         public override TxtElabResult Perform(INPUT input, IN_DATA inData, ref OUTPUT output)
         {
            var cmp_res = IsAtLeastAnItemRequired ? TxtElabResult.continue_searching : TxtElabResult.success;
            var sub_ela = SubTextElab;

            while (true)
            {
               var res = sub_ela.Perform(input, inData, ref output);

               switch (res)
               {
                  case TxtElabResult.success:
                     ///this is the case when <see cref="IsAtLeastAnItemRequired"/> is true
                     if (cmp_res == TxtElabResult.continue_searching) { cmp_res = TxtElabResult.success; }

                     continue;

                  case TxtElabResult.continue_searching: return cmp_res;

                  case TxtElabResult.failure:
                  case TxtElabResult.failure_unrecoverable:
                     return res;

                  default: throw new Crash();
               }
            }
         }

         /// <summary>
         /// <see cref="TxtElab{INPUT, IN_DATA, OUTPUT}"/> to iterate.
         /// </summary>
         public TxtElab<INPUT, IN_DATA, OUTPUT> SubTextElab { get; private set; }

         /// <summary>
         /// When true, <see cref="SubTextElab"/> shall return <see cref="TxtElabResult.success"/> at least once
         /// <br>otherwise returns <see cref="TxtElabResult.continue_searching"/> </br>
         /// </summary>
         public bool IsAtLeastAnItemRequired { get; }

         public override string ToString() => $"{GetType().Name}({SubTextElab})";
      }

      /// <summary>
      /// 
      /// </summary>
      public class And : Composed
      {
         public And(params TxtElab<INPUT, IN_DATA, OUTPUT>[] subElabs) : base(myMergeAnds(subElabs)) { }

         private static TxtElab<INPUT, IN_DATA, OUTPUT>[] myMergeAnds(TxtElab<INPUT, IN_DATA, OUTPUT>[] inElabs) =>
            inElabs.SelectMany(e => e is And and ? myMergeAnds(and.SubElabs) : [e]).ToArray();

         public override TxtElabResult Perform(INPUT input, IN_DATA inData, ref OUTPUT output)
         {
            var cur_str_idx = input.CurrIdx;
            var cln_out = output.Clone() as OUTPUT ?? throw new Crash();
            var cmp_res = TxtElabResult.success;

            foreach (var sub_elb in SubElabs)
            {
               var res = sub_elb.Perform(input, inData, ref output);

               switch (res)
               {
                  case TxtElabResult.success: continue;

                  case TxtElabResult.failure:
                  case TxtElabResult.failure_unrecoverable:
                     return res;

                  case TxtElabResult.continue_searching:
                     input.CurrIdx = cur_str_idx;
                     output = cln_out;

                     //in case of failure in any previous sub-elab failure shall be returned
                     return cmp_res == TxtElabResult.success ? TxtElabResult.continue_searching : cmp_res;

                  default: throw new Crash();
               }
            }

            return cmp_res;
         }
      }

      public class DoCrash : TxtElab<INPUT, IN_DATA, OUTPUT>
      {
         public DoCrash() => CrashMessage = "";

         public DoCrash(string crashMessage) => CrashMessage = crashMessage;

         public string CrashMessage { get; private set; }

         public override TxtElabResult Perform(INPUT input, IN_DATA inData, ref OUTPUT output) => throw new Crash(CrashMessage);
      }

      /// <summary>
      /// <br> Evaluates a condition on input optionally move ahead (CurrIdx++).</br>
      /// <br> Evaluation occurs just when input.IsIn at the moment of performing.</br>
      /// <br> Returns success(ok)/continue_searching(false) </br>
      /// </summary>
      public class Condition : TxtElab<INPUT, IN_DATA, OUTPUT>
      {
         /// <summary>
         /// Constructor, if isMove is true increments CurrIdx if condition matches.
         /// </summary>
         /// <param name="conditionEvaluator">Condition delegate</param>
         /// <param name="isMove">If true CurrIdx is incremented of 1.</param>
         public Condition(Func<INPUT, bool> conditionEvaluator, bool isMove)
         {
            CoditionEvaluator = conditionEvaluator;
            IsMove = isMove;
         }

         /// <summary>
         /// 
         /// </summary>
         public bool IsMove { get; private set; }

         /// <summary>
         /// 
         /// </summary>
         public Func<INPUT, bool> CoditionEvaluator { get; private set; }

         public override TxtElabResult Perform(INPUT input, IN_DATA inData, ref OUTPUT output)
         {
            if (input.IsIn)
            {
               var res = CoditionEvaluator.Invoke(input);

               if (IsMove) { input.CurrIdx++; }

               return res ? TxtElabResult.success : TxtElabResult.continue_searching;
            }

            return TxtElabResult.continue_searching;
         }
      }

      public abstract TxtElabResult Perform(INPUT input, IN_DATA inData, ref OUTPUT output);

      public TxtElabResult PerformNoOutput(INPUT input, IN_DATA inData)
      {
         var output = new OUTPUT();

         return Perform(input, inData, ref output);
      }

      public OUTPUT? PerformSuccessOrFail(INPUT input, IN_DATA inData)
      {
         var output = new OUTPUT();

         var res = Perform(input, inData, ref output);

         switch (res)
         {
            case TxtElabResult.success: return output;

            case TxtElabResult.failure:
            case TxtElabResult.failure_unrecoverable:
               return null;

            case TxtElabResult.continue_searching:
            default:
               throw new Crash($"{res} not allowed!");
         }
      }

      public override string ToString() => GetType().Name;
   }
}
