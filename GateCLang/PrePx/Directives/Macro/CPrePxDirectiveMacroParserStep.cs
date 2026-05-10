using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using static Gate.CLanguage.PrePx.Directives.Macro.CPrePxDirectiveMacro;

namespace Gate.CLanguage.PrePx.Directives.Macro
{
   /// <summary>
   /// 
   /// </summary>
   public class CPrePxDirectiveMacroParserStep : CPrePxDirectiveIdParserStep<CPrePxDirectiveMacro>
   {
      /// <summary>
      /// ()
      /// (par)
      /// (...)
      /// (par1,par2,...parn)
      /// (par1,par2,...parn,...)
      /// </summary>
      private readonly Composed myMacroWithArgsParser = new And(
         new Or(
            new And(new InnerZeroParams(), new ContentMapType.Parser()),
            new And(
               new Or(new IsSign("("), new DoCrash()),
                  new And(
                     new Or(new InnerParam(), new InnerVariadic(), new InnerError())),
                     new IterateWhileSuccess(
                        new And(
                           new IsSign(","),
                           new Or(new InnerParam(), new InnerVariadic(), new InnerError()))),
               new Or(new IsSign(")"), new Failure(p => CPrePxMessages.M005_Expected(p, ")", MsgType.fail))))),
         new ContentMapType.Parser());

      public CPrePxDirectiveMacroParserStep() : base(true) { }

      /// <summary>
      /// Parser macro with 0 argument '#define M() content' (different from no args macro eg '#define PI 3.14').
      /// </summary>
      private class InnerZeroParams : CPrePxParserStep
      {
         /// <summary>
         /// Check if #define macro has not arguments (eg #define PI 3.14 #define INCLUDE_H) 
         /// or is a macro #define sum(a,b) ((a)+(b)).
         /// Analyzes the specified text marker and determines whether processing should continue or succeed based on
         /// the marker's content and context.
         /// </summary>
         /// <param name="lineMarker">The <see cref="TxtMarker"/> representing the current position and content in the text to be analyzed. Must
         /// not be <c>null</c>.</param>
         /// <param name="data">The input data for the pre-processing operation. Must not be <c>null</c>.</param>
         /// <param name="output">When this method returns, contains the output data generated during processing.</param>
         /// <returns><see cref="TxtElabResult.success"/> if the marker is not an opening parenthesis, or if it is an opening
         /// parenthesis preceded by a whitespace character;  otherwise, <see cref="TxtElabResult.continue_searching"/>.</returns>
         public override TxtElabResult Perform(TxtMarker lineMarker, CPrePxInData data, ref CPrePxOutput output)
         {
            if (lineMarker.MarkedString == "(")
            {
               var is_spa = char.IsWhiteSpace(lineMarker.Store.Content[lineMarker.CurrIdx - 1]);

               return is_spa ? TxtElabResult.success : TxtElabResult.continue_searching;
            }
            else
            {
               return TxtElabResult.success;
            }
         }
      }

      private class InnerError : CPrePxParserStep
      {
         public override TxtElabResult Perform(TxtMarker lineMarker, CPrePxInData data, ref CPrePxOutput output)
         {
            if (lineMarker.MoveToNextNoSpace())
            {
               data.Messages.Add(CPrePxMessages.M003_UnexpectedText(lineMarker.CurrPos, $"{lineMarker.MarkedChar}", MsgType.fail));
            }
            else
            {
               lineMarker.CurrIdx--;
               data.Messages.Add(CPrePxMessages.M008_UnexpectedEndOfLine(lineMarker.CurrPos));
            }

            return TxtElabResult.failure;
         }
      }

      private class InnerVariadic : CPrePxParserStep
      {
         public override TxtElabResult Perform(TxtMarker lineMarker, CPrePxInData data, ref CPrePxOutput output)
         {
            var mcr = output.ListProduct.LastOrDefault().ConvertOrCrash<CPrePxDirectiveMacro>();
            var is_vdc = lineMarker.IsMarkingAnySign("...");

            if (is_vdc)
            {
               lineMarker.CurrIdx += 3;//move over ...

               if (lineMarker.MoveToNextNoSpace() && lineMarker.MarkedChar == ')')
               {
                  var new_prs = mcr?.Args == null ? ["..."] : mcr.Args.Concat(["..."]).ToArray();

                  (mcr ?? throw new Crash()).Args = new_prs;

                  return TxtElabResult.success;
               }
               else
               {
                  var fai = new Failure(p => CPrePxMessages.M005_Expected(p, ")", MsgType.fail));

                  return fai.Perform(lineMarker, data, ref output);
               }
            }
            else { return TxtElabResult.continue_searching; }
         }
      }

      private class InnerParam : CPrePxParserStep
      {
         public override TxtElabResult Perform(TxtMarker lineMarker, CPrePxInData data, ref CPrePxOutput output)
         {
            var mcr = output.ListProduct.LastOrDefault() as CPrePxDirectiveMacro ?? throw new Crash();
            var arg_nam = lineMarker.GetMarkingVarNameMoveOver();

            if (arg_nam != null)
            {
               var lst_prs = new List<string>(mcr.Args != null ? mcr.Args : []);

               if (lst_prs.Contains(arg_nam))
               {
                  lineMarker.MoveOf(-arg_nam.Length);
                  data.Messages.Add(CPrePxMessages.M026_DuplicatedMacroArg(lineMarker.CurrPos, arg_nam));

                  return TxtElabResult.failure_unrecoverable;
               }
               else
               {
                  lst_prs.Add(arg_nam);
                  mcr.Args = lst_prs.ToArray();

                  return TxtElabResult.success;
               }

            }
            else { return TxtElabResult.continue_searching; }
         }
      }

      /// <summary>
      /// Entry point for macro(#define) parser.
      /// </summary>
      /// <param name="inMarker"></param>
      /// <param name="data"></param>
      /// <param name="output"></param>
      /// <returns></returns>
      public override TxtElabResult Perform(TxtMarker inMarker, CPrePxInData data, ref CPrePxOutput output)
      {
         // calls the base which found directiveName (define) and Identfier (macro name) and 
         // the content which include parenthesis '#define A(a,b) #a #b' where content is '(a,b) #a #b'
         var res = base.Perform(inMarker, data, ref output);

         return res == TxtElabResult.success ? myMacroWithArgsParser.Perform(inMarker, data, ref output) : res;
      }
   }
}
