using Gate.CLanguage.Compiler;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.TokenParse;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage
{
   /// <summary>
   /// 
   /// </summary>
   public class CAttributesInterpret : CTokenInterpreter
   {
      private Or myOr;

      public CAttributesInterpret() => myOr = new Or(MakeAttributeInterprets());

      public class WinDeclSpec : CTokenInterpreter
      {
         private And myComposed = new And(
            new IsCTokenType(CTokenType.keyword, false),
            new Is(CAttribute.WinDeclSpec.TAG, true) | new Is(CAttribute.WinDeclSpec.TAG2, true),
            new Expect("(", true),
            new Expect(CTokenType.identifier, false),
            new InnerSimple() | new InnerWithArg(),
            new Expect(")", true));

         public static readonly string[] Simple = [
               "allocator","appdomain", "deprecated","dllimport","dllexport","jitintrinsic",
            "naked","noalias","noinline","noreturn","nothrow","novtable","process","restrict","safebuffers","selectany","thread"];
         public static readonly string[] WithArg = [
               "align","property","code_seg","allocate","uuid","spectre",];

         private class InnerSimple : CTokenInterpreter
         {
            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
            {
               if (Simple.Contains(input.MarkedText))
               {
                  var atr = new CAttribute.WinDeclSpec();
                  var itm = output.Peek() ?? throw new Crash();

                  atr.Content = input?.Dequeue()?.Content;
                  output.Push(atr);

                  return TxtElabResult.success;
               }
               else { return TxtElabResult.continue_searching; }
            }
         }

         private class InnerWithArg : CTokenInterpreter
         {
            public And myPreCond =
               new And(
                  new Condition(tl => WithArg.Contains(tl.MarkedText), true) | new Expect("valid __declspec value", false),
                  new Expect("(", true));

            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
            {
               var res = myPreCond.PerformNoOutput(input, inData);

               if (res == TxtElabResult.success)
               {
                  var atr = new CAttribute.WinDeclSpec();
                  var sta_tok = input.Peek(-1)?.NnOrCrash();
                  var cur_idx = input.CurrIdx - 1;

                  atr.Content = input.Peek(-2)?.Content.NnOrCrash();

                  var is_ok = false;

                  while (input.IsIn)
                  {
                     if (input.MarkedText == ")")
                     {
                        is_ok = true;
                        atr.Content += string.Join(" ", Enumerable.Range(cur_idx, input.CurrIdx - cur_idx + 1).Select(i => input[i].Content));
                        input.CurrIdx++;
                        break;
                     }

                     input.CurrIdx++;
                  }

                  if (!is_ok)
                  {
                     inData.Messages.Add(CCompilerMsgs.EndOfFileReachedOpenBracket(sta_tok, true));

                     return TxtElabResult.failure_unrecoverable;
                  }
                  else
                  {
                     output.Push(atr);

                     return TxtElabResult.success;
                  }
               }
               else if (res == TxtElabResult.continue_searching) { throw new Crash(); }//no reach here
               else { return res; }
            }
         }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output) => myComposed.Perform(input, inData, ref output);
      }

      public class GccAttribute : CTokenInterpreter
      {
         private And myComposed = new And(
            new IsCTokenType(CTokenType.keyword, false),
            new Condition(tl => CAttribute.GccAttribute.Tags.Contains(tl.MarkedText), true),
            new Expect("(", true), new Expect("(", true),
            new InnerInterpret(),
            new Expect(")", true), new Expect(")", true));

         private class InnerInterpret : CTokenInterpreter
         {
            //weak interpreter valid expression 
            public InnerInterpret() { }

            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
            {
               var tt = (input.MarkedToken as CToken)?.TokenType;

               if (input.IsIn && (tt == CTokenType.identifier || tt == CTokenType.keyword))
               {
                  var atr = new CAttribute.GccAttribute();
                  var itm = output.PeekOrCrash<CItem>();

                  atr.TagId = (CAttribute.GccAttribute.TagEnum)
                     Enum.Parse(typeof(CAttribute.GccAttribute.TagEnum), input[input.CurrIdx - 3].Content);

                  var idx_sta = input.CurrIdx;

                  while (true)
                  {
                     var ct = input.Peek<CToken>().NnOrCrash();

                     if (ct.TokenType == CTokenType.identifier)
                     {
                        input.CurrIdx++;

                        if (input.MarkedText == "(")
                        {
                           var is_ok = false;
                           var sta_tok = input.MarkedToken.NnOrCrash();

                           while (input.IsIn)
                           {
                              if (input.MarkedText == ")")
                              {
                                 input.CurrIdx++;
                                 is_ok = true;
                                 break;
                              }

                              input.CurrIdx++;
                           }

                           if (!is_ok)
                           {
                              inData.Messages.Add(CCompilerMsgs.EndOfFileReachedOpenBracket(sta_tok, true));

                              return TxtElabResult.failure_unrecoverable;
                           }
                        }

                        if (input.MarkedText == ",")
                        {
                           input.CurrIdx++;
                           continue;
                        }
                        else
                        {
                           break;
                        }
                     }
                     else
                     {
                        inData.Messages.Add(CCompilerMsgId.expected_identifier.GetError(ct));

                        return TxtElabResult.failure;
                     }
                  }

                  atr.Content = TxtTokenConst.FromTokenInterval(input[idx_sta], input[input.CurrIdx - 1]).Content;
                  output.Push(atr);

                  return TxtElabResult.success;
               }
               else
               {
                  inData.Messages.Add(CCompilerMsgId.expected_identifier.GetError(input.MarkedToken));

                  return TxtElabResult.failure;
               }
            }
         }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output) =>
            myComposed.Perform(input, inData, ref output);
      }

      public virtual CTokenInterpreter[] MakeAttributeInterprets() => [new WinDeclSpec(), new GccAttribute()];

      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         var sta_idx = input.CurrIdx;
         var res = myOr.Perform(input, inData, ref output);

         if (res == TxtElabResult.success)
         {
            var atr = output.PopOrCrash<CAttribute>() ?? throw new Crash();
            var tok = TxtTokenConst.FromTokenInterval(input[sta_idx], input[input.CurrIdx - 1]);
            var itm = output.Peek() ?? throw new Crash();

            if (!itm.Attributes.Any(a => a.TxtToken != null && a.TxtToken.IsEquivalent(tok)))
            {
               atr.TxtToken = tok;
               itm.Attributes.Add(atr);
            }
         }

         return res;
      }
   }
}

