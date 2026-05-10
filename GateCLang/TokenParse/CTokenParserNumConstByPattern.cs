using Gate.CLanguage.Compiler;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System.Text.RegularExpressions;

namespace Gate.CLanguage.TokenParse
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class CTokenParserNumConstByPattern : CTokenParserStep
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="pattern"></param>
      /// <param name="isCaseInSensitive"></param>
      public CTokenParserNumConstByPattern(string pattern, bool isCaseInSensitive)
      {
         Pattern = pattern;
         Regex = new Regex(pattern, RegexOptions.Compiled | (isCaseInSensitive ? RegexOptions.IgnoreCase : 0));
      }

      /// <summary>
      /// Perform a precheck of constant this in order to improve parse time(avoid to repeadet call of <see cref="Regex"/>)
      /// </summary>
      /// <param name="inputMarker"></param>
      /// <param name="inData"></param>
      /// <returns></returns>
      protected abstract TxtElabResult myPrecondition(TxtMarker inputMarker, CCompilerInData inData);

      /// <summary>
      /// 
      /// </summary>
      public string Pattern { get; }

      /// <summary>
      /// 
      /// </summary>
      public string? BuiltInTypeName { get; }

      /// <summary>
      /// 
      /// </summary>
      public Regex Regex { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="inputMarker"></param>
      /// <param name="inData"></param>
      /// <param name="output"></param>
      /// <returns></returns>
      public override TxtElabResult Perform(TxtMarker inputMarker, CCompilerInData inData, ref CTokenParserOutput output)
      {
         //precondition check avoid call regex evaluation improving speed
         var pre_res = myPrecondition(inputMarker, inData);

         if (pre_res == TxtElabResult.success)
         {
            if (inputMarker.IsMarkingRegex(Regex, out var mat))
            {
               var tok = TxtTokenConst.FromFromLen(inputMarker.Store, inputMarker.CurrIdx, mat?.Length ?? 0);

               //moving to end of regex
               inputMarker.CurrIdx += mat?.Length ?? 0;

               var res = myOnMatch(inData, mat ?? throw new Crash(), tok, out var rtm_val);

               if (res == TxtElabResult.success)
               {
                  var c_tok = CToken.MakeConstant(tok, rtm_val ?? throw new Crash());

                  output.ListProduct.Add(c_tok);
               }

               return res;
            }
         }

         return pre_res;
      }


      protected abstract TxtElabResult myGetItems(
         TxtToken regexMatchToken, Match regexMatch, CCompilerInData inData, out string? builtiNTypeName, out ValueType? csharpObject);

      protected virtual TxtElabResult myOnMatch(CCompilerInData inData, Match regexMatch, TxtToken matchToken, out RtmObj? runtimeObject)
      {
         var csh_obj = null as ValueType;
         var bin_nam = null as string;

         var res = myGetItems(matchToken, regexMatch, inData, out bin_nam, out csh_obj);

         if (res == TxtElabResult.success)
         {
            if (inData.Settings.BuiltInSet?.Any(B => B.TypeSpecifier == bin_nam) ?? false)
            {
               runtimeObject = myMakeRunTimeObj(csh_obj, bin_nam, inData);
            }
            else
            {
               inData.Messages.Add(CCompilerMsgs.BuiltInTypeNotDefined(bin_nam ?? "", matchToken));
               runtimeObject = null;

               return TxtElabResult.failure;
            }
         }
         else { runtimeObject = null; }

         return res;
      }

      protected virtual RtmObj myMakeRunTimeObj(ValueType? csharpValue, string? typeBuiltInName, CCompilerInData inData) =>
         inData.RtmStrategy.MakeConstant(
            csharpValue ?? throw new Crash(),
            inData.Settings.BuiltInSet?.FirstOrDefault(t => t.TypeSpecifier == typeBuiltInName) ??
            throw new Crash($"{csharpValue.GetType().Name} not a built-in type!"));
   }
}
