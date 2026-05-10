using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System.Text.RegularExpressions;

namespace Gate.CLanguage.PrePx.Directives.PragmaKinds
{
   /// <summary>
   /// 
   /// </summary>
   public class CPragmaKindPack : CPragmaKind
   {
      public enum TypeId
      {
         /// <summary>
         /// like #pragma pack(4)
         /// </summary>
         simple = 0,

         /// <summary>
         /// like #pragma pack(show)
         /// </summary>
         show,

         /// <summary>
         /// like #pragma pack(push),#pragma pack(push,4)
         /// </summary>
         push,

         /// <summary>
         /// like #pragma pack(pop),#pragma pack(pop,4)
         /// </summary>
         pop,

         /// <summary>
         /// #pragma pack is wrong (eg '#pragma pack(3)')
         /// </summary>
         wrong
      }

      /// <summary>
      /// 
      /// </summary>
      public CPragmaKindPack()
      {

      }

      public new class Parser : CPragmaKind.Parser
      {
         private class InnerShow : CPragmaKind.Parser
         {
            public override TxtElabResult Perform(TxtMarker inputMarker, CPrePxInData data, ref TxtElabSingleOutput<CPragmaKind> output)
            {
               if (inputMarker.MoveToNextNoSpace() && inputMarker.IsMarkingVarNameMoveOver("show"))
               {
                  var pak = output.Product as CPragmaKindPack ?? throw new Crash();

                  pak.Type = TypeId.show;

                  return TxtElabResult.success;
               }
               else
               {
                  return TxtElabResult.continue_searching;
               }
            }
         }

         private class InnerNumeric : CPragmaKind.Parser
         {
            private static Regex myRegexNumeric = new Regex(@"\s*\d+\s*");

            public override TxtElabResult Perform(TxtMarker inputMarker, CPrePxInData data, ref TxtElabSingleOutput<CPragmaKind> output)
            {
               if (inputMarker.MoveToNextNoSpace() && inputMarker.IsMarkingRegexMoveOver(myRegexNumeric, out var mat_val))
               {
                  var pak = output.Product as CPragmaKindPack ?? throw new Crash();
                  var pak_int = int.Parse(mat_val.ExtTrim());

                  if (!myIsPowerOf2(pak_int))
                  {
                     data.Messages.Add(CPrePxMessages.M025_PragmaPackNotAPowerOfTwo(inputMarker.CurrPos, pak_int));
                     pak.Type = TypeId.wrong;
                  }
                  else if (pak_int > data.Options?.MaxPacking)
                  {
                     data.Messages.Add(CPrePxMessages.M024_PragmaPackTooBig(inputMarker.CurrPos, pak_int));
                     pak.Type = TypeId.wrong;
                  }
                  else
                  {
                     pak.Pack = pak_int;
                  }

                  return TxtElabResult.success;
               }

               return TxtElabResult.continue_searching;
            }

            private bool myIsPowerOf2(int value) { return (value & (value - 1)) == 0; }
         }

         private class InnerIdentifier : CPragmaKind.Parser
         {
            public override TxtElabResult Perform(TxtMarker inputMarker, CPrePxInData data, ref TxtElabSingleOutput<CPragmaKind> output)
            {
               if (inputMarker.MoveToNextNoSpace())
               {
                  var var_nam = inputMarker.GetMarkingVarNameMoveOver();

                  if (var_nam != null)
                  {
                     var pak = output.Product as CPragmaKindPack ?? throw new Crash();

                     pak.Identifier = var_nam;

                     return TxtElabResult.success;
                  }
               }

               return TxtElabResult.continue_searching;
            }
         }

         private class InnerPushPop : CPragmaKind.Parser
         {
            private static And myAnd = new And(
               new May(new And(new IsSign(","), new InnerIdentifier())),
               new May(new And(new IsSign(","), new InnerNumeric())));

            public override TxtElabResult Perform(TxtMarker inputMarker, CPrePxInData data, ref TxtElabSingleOutput<CPragmaKind> output)
            {
               if (inputMarker.MoveToNextNoSpace())
               {
                  var pak = output.Product as CPragmaKindPack ?? throw new Crash();

                  if (inputMarker.IsMarkingVarNameMoveOver("push")) { pak.Type = TypeId.push; }
                  else if (inputMarker.IsMarkingVarNameMoveOver("pop")) { pak.Type = TypeId.pop; }
                  else { return TxtElabResult.continue_searching; }

                  return myAnd.Perform(inputMarker, data, ref output);
               }

               return TxtElabResult.continue_searching;
            }
         }

         private class InnerFailure : CPragmaKind.Parser
         {
            private MessageGetter myOnWarningMsg;

            public InnerFailure(MessageGetter onError) => myOnWarningMsg = onError;

            public override TxtElabResult Perform(TxtMarker inputMarker, CPrePxInData data, ref TxtElabSingleOutput<CPragmaKind> output)
            {
               var msg_pos = null as TxtPos;

               if (inputMarker.MoveToNextNoSpace()) { msg_pos = inputMarker.CurrPos; }
               else
               {
                  inputMarker.CurrPos = new TxtPos(1, inputMarker.Store[1].Length, inputMarker.Store);
                  msg_pos = inputMarker.CurrPos;
               }

               data.Messages.Add(myOnWarningMsg.Invoke(msg_pos));

               return TxtElabResult.success;
            }
         }

         public override TxtElabResult Perform(TxtMarker inputMarker, CPrePxInData data, ref TxtElabSingleOutput<CPragmaKind> output)
         {
            var and = new And(
               new IsVarName("pack"),
               new Or(new IsSign("("), new InnerFailure(p => CPrePxMessages.M005_Expected(p, "(", MsgType.warning))),
               new Or(new InnerShow(), new InnerPushPop(), new InnerNumeric()),
               new Or(new IsSign(")"), new InnerFailure(p => CPrePxMessages.M005_Expected(p, ")", MsgType.warning))),
               new Or(new IsFinished(), new InnerFailure(p => CPrePxMessages.M006_ExpectedEndOfLineWarn(p))));

            output.Product = new CPragmaKindPack();

            var res = and.Perform(inputMarker, data, ref output);

            return res;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public string? Identifier { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public TypeId Type { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public override string KindName => "pack";

      /// <summary>
      /// Pack size in bytes (eg #pragma pack(4)).
      /// </summary>
      public int? Pack { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public override TxtToken? TxtToken { get => ParentItem?.TxtToken; set { } }

      /// <summary>
      /// 
      /// </summary>
      public override string Descriptor => Rebuilt;

      public override string Rebuilt
      {
         get
         {
            var n = Pack > 0 ? Pack.ToString() : "";
            var id = Identifier != null && Identifier.Trim() != "" ? Identifier.Trim() : "";
            var typ_str = Type != TypeId.simple ? Type.ToString() : "";
            var prs = new[] { typ_str, n, id };

            return $"pack({string.Join(",", prs.Where(p => p != ""))})";
         }
      }
   }
}
