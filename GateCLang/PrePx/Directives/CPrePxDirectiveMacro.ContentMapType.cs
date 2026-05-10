using Gate.CLanguage.PrePx.LookForwards;
using Gate.Tools;
using Gate.Tools.Message;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using static Gate.Tools.Text.TxtStore;

namespace Gate.CLanguage.PrePx.Directives.Macro
{
   /// <summary>
   /// 
   /// </summary>
   public partial class CPrePxDirectiveMacro
   {
      public class ContentMapType
      {
         /// <summary>
         /// 
         /// </summary>
         public enum TokenType
         {
            /// <summary>
            /// <br>Macro argument which is not involded neither in a string merge nor in a stringfy</br>
            /// <br>eg '#define M(a) a'</br>
            /// </summary>
            arg_to_prescan = 0,

            /// <summary>
            /// eg '#define M(a) a ## Error' 'M(is)' => 'isError'
            /// </summary>
            arg_to_replace,

            /// <summary>
            /// 
            /// </summary>
            arg_to_stringfy,

            /// <summary>
            /// token to ## (string merge operator)
            /// </summary>
            string_merge,

            /// <summary>
            /// __VA_ARGS__ occurence
            /// </summary>
            var_args_to_prescan,

            /// <summary>
            /// __VA_ARGS__ occurence with stringfy (# __VA_ARGS__)
            /// </summary>
            var_args_stringfy,

            /// <summary>
            /// __VA_ARGS__ is to replace only, ie is involved in a string merge like #define M(s) s ## __VA_ARGS__
            /// </summary>
            var_args_to_replace,
         }

         public ContentMapType(TxtStore store) => Store = store;

         /// <summary>
         /// 
         /// </summary>
         public class Item
         {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="sectors"></param>
            /// <param name="tokenType"></param>
            /// <param name="id"></param>
            public Item(Sector[] sectors, TokenType tokenType, int id)
            {
               Sectors = sectors;
               TokenType = tokenType;
               Id = id;
            }

            /// <summary>
            /// 
            /// </summary>
            public Sector[] Sectors { get; }

            /// <summary>
            /// 
            /// </summary>
            public TokenType TokenType { get; }

            /// <summary>
            /// 
            /// </summary>
            public int Id { get; }

            public override int GetHashCode()
            {
               var hsh_cod = -1617199642;

               hsh_cod = hsh_cod * -1521134295 + EqualityComparer<Sector[]>.Default.GetHashCode(Sectors);
               hsh_cod = hsh_cod * -1521134295 + TokenType.GetHashCode();
               hsh_cod = hsh_cod * -1521134295 + Id.GetHashCode();

               return hsh_cod;
            }

            public override string ToString() => $"##{Id}: {TokenType} '{string.Join("+", Sectors.Select(s => s.Content))}'";
         }

         /// <summary>
         /// 
         /// </summary>
         public class Parser : CPrePxDirectiveIdParserStep<CPrePxDirectiveMacro>
         {
            private readonly InnerLookFw[] myLookForward;

            public Parser(params LookFwToken[] stringCharLf) : base(true)
            {
               if (stringCharLf.Length == 0) { stringCharLf = new LookFwToken[] { new LookFwCharToken(), new LookFwStringToken() }; }

               myLookForward =
                  new InnerLookFw[] { new InnerLookFw.Argument(), new InnerLookFw.StrMerge(), new InnerLookFw.Stringfy(), new InnerLookFw.Variadic() }.
                  Concat(stringCharLf.Select(lf => new InnerLookFw.StringCharWrapper(lf))).ToArray();
            }

            /// <summary>
            /// 
            /// </summary>
            public class InData : TxtElabInData
            {
               public InData(CPrePxDirectiveMacro macro, MsgCollection messages) : base(messages) => Macro = macro;

               public CPrePxDirectiveMacro Macro { get; }
            }

            /// <summary>
            /// 
            /// </summary>
            private abstract class InnerLookFw : ParserStep<InData, TxtElabSingleOutput<ContentMapType>>.LookForward
            {
               public class Stringfy : InnerLookFw
               {
                  public Stringfy() { }

                  public override int GetLookFwIdx(TxtMarker input, InData inData)
                  {
                     if (input.LookForAnySign("#"))
                     {
                        var str_idx = input.CurrIdx;

                        input.MoveOf(1);//move over #

                        //if youre at end(this shall cause an error) or there is different of another # (that is '##':concat).
                        if (!input.IsIn || input.MarkedChar != '#')
                        {
                           return str_idx;
                        }
                     }

                     return -1;
                  }

                  public override TxtElabResult PerformWhenLookFw(TxtMarker input, InData inData, ref TxtElabSingleOutput<ContentMapType> output)
                  {
                     var mcr = inData.Macro;

                     if (!mcr.HasArguments)
                     {
                        inData.Messages.Add(CPrePxMessages.M004_AshInMacro(input.CurrPos));

                        return TxtElabResult.failure_unrecoverable;
                     }
                     else
                     {
                        var arg_nam = null as string;
                        var sta_rpl_idx = input.CurrIdx;

                        //next word must be a var_name and included in macro arguments(like a #define M(a,b))
                        var cnd =
                           input.MoveOf(1) == 1 &&
                           (arg_nam = input.GetMarkingVarNameMoveOver()) != null &&
                           (arg_nam == VA_ARGS || (mcr?.Args?.ToList().IndexOf(arg_nam) ?? -1) >= 0);

                        if (cnd)
                        {
                           myAdd(input, (sta_rpl_idx, input.CurrIdx - 1), arg_nam == VA_ARGS ?
                              TokenType.var_args_stringfy : TokenType.arg_to_stringfy, output);

                           return TxtElabResult.success;
                        }
                        else
                        {
                           input.CurrIdx = sta_rpl_idx;
                           inData.Messages.Add(CPrePxMessages.M004_AshInMacro(input.CurrPos));

                           return TxtElabResult.failure_unrecoverable;
                        }
                     }
                  }
               }

               public class Variadic : InnerLookFw
               {
                  public Variadic()
                  {

                  }

                  public override int GetLookFwIdx(TxtMarker input, InData inData)
                  {
                     var par_nam = null as string;

                     while ((par_nam = input.LookForVarName()) != null)
                     {
                        if (par_nam == VA_ARGS)
                        {
                           return input.CurrIdx;
                        }
                        else
                        {
                           input.CurrIdx += par_nam.Length;
                        }
                     }

                     return -1;
                  }

                  public override TxtElabResult PerformWhenLookFw(
                     TxtMarker input, InData inData, ref TxtElabSingleOutput<ContentMapType> output)
                  {
                     myAdd(input, Interval.FromFromLen(input.CurrIdx, VA_ARGS.Length), TokenType.var_args_to_prescan, output);
                     input.CurrIdx += VA_ARGS.Length;

                     return TxtElabResult.success;
                  }
               }

               /// <summary>
               /// 
               /// </summary>
               public class Argument : InnerLookFw
               {
                  public Argument() { }

                  public override int GetLookFwIdx(TxtMarker input, InData inData)
                  {
                     if (!inData.Macro.HasArguments) { return -1; }
                     else
                     {
                        var par_nam = null as string;

                        while ((par_nam = input.LookForVarName()) != null)
                        {
                           var arg_idx = inData.Macro?.Args?.ToList().IndexOf(par_nam) ?? -1;

                           if (arg_idx != -1) { return input.CurrIdx; }
                           else { input.CurrIdx += par_nam.Length; }
                        }

                        return -1;
                     }
                  }

                  public override TxtElabResult PerformWhenLookFw(
                     TxtMarker input, InData inData, ref TxtElabSingleOutput<ContentMapType> output)
                  {
                     //candidate argument name
                     var cnd_arg_nam = null as string;
                     var arg_idx = -1;

                     if (
                        (cnd_arg_nam = input.LookForVarNameMoveOver()) != null &&
                        (arg_idx = inData.Macro?.Args?.ToList().IndexOf(cnd_arg_nam) ?? -1) != -1)
                     {
                        //notice store is cloned
                        var sto = output.Product?.Store;

                        var scs = sto?.SplitInterval((input.CurrIdx - cnd_arg_nam.Length, input.CurrIdx - 1));

                        if (scs?.Length != 1) { throw new Crash(); }

                        myAdd(input, (input.CurrIdx - cnd_arg_nam.Length, input.CurrIdx - 1), TokenType.arg_to_prescan, output);

                        return TxtElabResult.success;
                     }
                     else { throw new Crash(); }
                  }
               }

               /// <summary>
               /// 
               /// </summary>
               public class StrMerge : InnerLookFw
               {
                  public StrMerge() { }

                  public override int GetLookFwIdx(TxtMarker input, InData inData) =>
                     input.LookForAnySign("##") ? input.CurrIdx : -1;

                  public override TxtElabResult PerformWhenLookFw(TxtMarker input, InData inData, ref TxtElabSingleOutput<ContentMapType> output)
                  {
                     if (input.IsMarkingAnySign("##"))
                     {
                        var sta_idx = input.CurrIdx;

                        if (input.MoveToNextNoSpaceBackward())
                        {
                           var merge_token_start_idx = input.CurrIdx + 1;

                           input.CurrIdx = sta_idx + 2;

                           if (input.MoveToNextNoSpace())
                           {
                              var mrg_tok_len = input.CurrIdx - merge_token_start_idx;

                              myAdd(input, Interval.FromFromLen(merge_token_start_idx, mrg_tok_len), TokenType.string_merge, output);

                              return TxtElabResult.success;
                           }
                           else
                           {
                              input.CurrIdx = sta_idx;
                              inData.Messages.Add(CPrePxMessages.M030_InvalidStringMerge(input.CurrPos));

                              return TxtElabResult.failure_unrecoverable;
                           }
                        }
                        else
                        {
                           input.CurrIdx = sta_idx;
                           inData.Messages.Add(CPrePxMessages.M030_InvalidStringMerge(input.CurrPos));

                           return TxtElabResult.failure_unrecoverable;
                        }
                     }
                     else { throw new Crash(); }
                  }
               }

               public class StringCharWrapper : InnerLookFw
               {
                  public StringCharWrapper(LookFwToken stringCharLf2Wrap) => StringCharLf2Wrap = stringCharLf2Wrap;

                  public LookFwToken StringCharLf2Wrap { get; }

                  public override int GetLookFwIdx(TxtMarker input, InData inData) => StringCharLf2Wrap.GetLookFwIdx(input, inData);

                  public override TxtElabResult PerformWhenLookFw(TxtMarker input, InData inData, ref TxtElabSingleOutput<ContentMapType> output)
                  {
                     var dum_out = new TxtElabOutputList<TxtToken>();

                     return StringCharLf2Wrap.PerformWhenLookFw(input, inData, ref dum_out);
                  }
               }

               protected static void myAdd(
                  TxtMarker inMarker, Interval interval, TokenType tokenType, TxtElabSingleOutput<ContentMapType> output)
               {
                  var sto = output.Product?.Store ?? throw new Crash();

                  var scs = sto.SplitInterval(interval);

                  var itm = new Item(scs, tokenType, output.Product.ListItems.Count + 1);

                  foreach (var sec in scs) { sec.Tag = itm; }

                  output.Product.ListItems.Add(itm);
               }
            }

            public override TxtElabResult Perform(TxtMarker lineMarker, CPrePxInData inData, ref CPrePxOutput output)
            {
               var mcr = output.ListProduct.LastOrDefault() as CPrePxDirectiveMacro ?? throw new Crash();
               var ln_idx = inData.CurrLineIdxStage32Tokenisation;
               var sto = output.StageResults.LastOrDefault() ?? throw new Crash();

               //adjust content token (eg #define(a) #a content_token was '(a) #a' now '#a'
               mcr.ContentToken = lineMarker.MoveToNextNoSpace() ?
                  TxtTokenConst.FromFromTo(sto, ln_idx, lineMarker.CurrIdx + 1, ln_idx, lineMarker.Store.Content.Length) :
                  TxtTokenConst.EmptyString;

               if (!lineMarker.IsAtEnd)
               {
                  var loo_fw_cmb = new InnerLookFw.IterateWhileSuccess(
                     new TxtElab<TxtMarker, InData, TxtElabSingleOutput<ContentMapType>>.LookForward.Combine(myLookForward));
                  var loo_fw_dat = new InData(mcr, inData.Messages);
                  var loo_fw_out = new TxtElabSingleOutput<ContentMapType>(new ContentMapType(FromTokens(mcr.ContentToken)));
                  var mrk = TxtMarker.FromTokens(mcr.ContentToken);

                  var res = loo_fw_cmb.Perform(mrk, loo_fw_dat, ref loo_fw_out);

                  mcr.ContentMap = res == TxtElabResult.success ? loo_fw_out?.Product?.GetPreScanAdjusted() : null;

                  return res;
               }
               else
               {
                  mcr.ContentMap = new ContentMapType(new TxtStore(""));//empty content map

                  return TxtElabResult.success;
               }
            }
         }

         /// <summary>
         /// 
         /// </summary>
         public TxtStore Store { get; }

         /// <summary>
         /// 
         /// </summary>
         public List<Item> ListItems { get; private set; } = new List<Item>();

         /// <summary>
         /// Returns a content map where prescan are adjusted based on position of <seealso cref="TokenType.string_merge"/> operator (##)
         /// </summary>
         /// <returns></returns>
         public ContentMapType GetPreScanAdjusted()
         {
            var res = new ContentMapType(Store);
            var itn = new Interval(0, ListItems.Count - 1);

            res.ListItems.AddRange(Enumerable.Range(0, ListItems.Count).Select(i =>
            {
               ///if type is <seealso cref="TokenType.arg_to_prescan"/> or <seealso cref="TokenType.var_arg_to_prescan"/>
               ///and previous or next are contigous and of type <seealso cref="TokenType.string_merge"/>
               switch (ListItems[i].TokenType)
               {
                  case TokenType.arg_to_prescan:
                  case TokenType.var_args_to_prescan:
                     var prv = itn.Contains(i - 1) ? ListItems[i - 1] : null;//previous item
                     var is_prv_ctg = prv != null && prv.Sectors.Last().IsContigous(ListItems[i].Sectors[0]);//is previous item's last sector is contigous to my first sector?
                     var nxt = itn.Contains(i + 1) ? ListItems[i + 1] : null;//next item
                     var is_nxt_ctg = nxt != null && nxt.Sectors[0].IsContigous(ListItems[i].Sectors.Last());//is next item's first sector is contigous to my last sector?

                     if (is_prv_ctg && prv?.TokenType == TokenType.string_merge || is_nxt_ctg && nxt?.TokenType == TokenType.string_merge)
                     {
                        var itm_typ = ListItems[i].TokenType == TokenType.arg_to_prescan ? TokenType.arg_to_replace : TokenType.var_args_to_replace;

                        return new Item(ListItems[i].Sectors, itm_typ, ListItems[i].Id);
                     }
                     else { return ListItems[i]; }

                  default: return ListItems[i];
               }
            }));

            return res;
         }
      }
   }
}
