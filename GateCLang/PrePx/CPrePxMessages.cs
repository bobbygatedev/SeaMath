using Gate.CLanguage.PrePx.Directives;
using Gate.CLanguage.PrePx.Directives.IfDefElif;
using Gate.Tools.Message;
using Gate.Tools.Text;

namespace Gate.CLanguage.PrePx
{
   public static class CPrePxMessages
   {
      public static Msg M001_UnexpectedEndFile(TxtPos? txtPos) => Msg.FromTxtPos(
            MsgType.fatal, "[CPrePx001] Unexpected end of file", CPrePxMsgId.cprepx001_unexp_end_of_line, txtPos);

      public static Msg M002_UnknownDirective(TxtPos? txtPos, string directive) => Msg.FromTxtPos(
            MsgType.fail, $"[CPrePx002] Unknown directive '#{directive}'.", CPrePxMsgId.cprepx002_unexp_directive, txtPos);

      public static Msg M003_UnexpectedText(TxtPos? txtPos, string text, MsgType msgType) =>
         Msg.FromTxtPos(msgType, $"[CPrePx003] Unexpected text '{text}'.", CPrePxMsgId.cprepx003_unexp_text, txtPos);

      public static Msg M004_AshInMacro(TxtPos? txtPos) =>
         Msg.FromTxtPos(MsgType.fatal, "[CPrePx005] Invalid '#' in macro content.", CPrePxMsgId.cprepx004_invalid_ash, txtPos);

      public static Msg M005_Expected(TxtPos? txtPos, string what, MsgType msgType) =>
         Msg.FromTxtPos(msgType, $"[CPrePx005] '{what}' was expected here.", CPrePxMsgId.cprepx005_expected_something, txtPos);

      public static Msg M006_ExpectedEndOfLineWarn(TxtPos? txtPos) => Msg.FromTxtPos(MsgType.warning, "[CPrePx006] End of line was expected.", CPrePxMsgId.cprepx006_expected_end_of_line, txtPos);

      public static Msg M007_UnterminatedStringChar(TxtPos? txtPos, bool isStringOrChar) =>
         Msg.FromTxtPos(MsgType.fatal, $"[CPrePx007] Unterminated {(isStringOrChar ? "string" : "char")}.", CPrePxMsgId.cprepx007_unterminated_string, txtPos);

      public static Msg M008_UnexpectedEndOfLine(TxtPos? txtPos) =>
         Msg.FromTxtPos(MsgType.fail, "[CPrePx008] Uexpected end of line.", CPrePxMsgId.cprepx008_unexpected_end_of_line, txtPos);

      public static Msg M009_TooManyClosedPar(TxtPos? txtPos) =>
         Msg.FromTxtPos(MsgType.fatal, "[CPrePx009] Too many ')'.", CPrePxMsgId.cprepx009_too_many_close_parenthesis, txtPos);

      public static Msg M011_IdentifierExpected(TxtPos? txtPos) =>
         Msg.FromTxtPos(MsgType.fatal, "[CPrePx010] Identifier expected.", CPrePxMsgId.cprepx010_identifier_expected, txtPos);

      /// <summary>
      /// For #else #elif #endif
      /// </summary>
      /// <param name="txtPos"></param>
      /// <param name="logicalDirectiveName"></param>
      /// <returns></returns>
      public static Msg M011_WithoutIf(TxtPos? txtPos, string logicalDirectiveName) =>
         Msg.FromTxtPos(MsgType.fatal, $"[CPrePx011] #{logicalDirectiveName} without #if.", CPrePxMsgId.cprepx011_without_if, txtPos);

      /// <summary>
      /// For #if #ifdef #ifndef #else
      /// </summary>
      /// <param name="txtPos"></param>
      /// <param name="logicalDirectiveName"></param>
      /// <returns></returns>
      public static Msg M012_UnterminatedLogicalDirective(TxtPos? txtPos, string logicalDirectiveName) =>
         Msg.FromTxtPos(MsgType.fatal, "[CPrePx012] Unterminated #{logicalDirectiveName}.", CPrePxMsgId.cprepx012_unterminated_logical_directive, txtPos);

      /// <summary>
      /// In case extra word after directive occurs (eg '#endif foo')
      /// </summary>
      /// <param name="txtPos"></param>
      /// <param name="what"></param>
      public static Msg M013_ExtraTokenAfterWarning(TxtPos? txtPos, string what) =>
         Msg.FromTxtPos(MsgType.warning, $"[CPrePx013] Extra token after '{what}'.", CPrePxMsgId.cprepx013_extra_token_after, txtPos);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="txtPos"></param>
      public static Msg M014_ElifAfterElse(TxtPos? txtPos) => Msg.FromTxtPos(MsgType.fatal, "[CPrePx014] #elif after #else.", CPrePxMsgId.cprepx014_elif_after_else, txtPos);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="txtPos"></param>
      public static Msg M015_ElseAfterElse(TxtPos? txtPos) => Msg.FromTxtPos(MsgType.fatal, "[CPrePx015] #else after #else.", CPrePxMsgId.cprepx015_else_after_else, txtPos);

      /// <summary>
      /// For #else,#elif,#endif
      /// </summary>
      /// <param name="txtPos"></param>
      /// <param name="directiveName"></param>
      public static Msg M016_AnyWithoutIf(TxtPos? txtPos, string directiveName) =>
         Msg.FromTxtPos(MsgType.fatal, $"[CPrePx016] #{directiveName} without #if/#ifdef/#ifndef.", CPrePxMsgId.cprepx016_any_without_if, txtPos);

      /// <summary>
      /// For any but #endif.
      /// </summary>
      /// <param name="txtPos"></param>
      /// <param name="directiveName"></param>
      public static Msg M017_AnyWithoutEndif(TxtPos? txtPos, string directiveName) =>
         Msg.FromTxtPos(MsgType.fatal, $"[CPrePx017] #{directiveName} without #endif.", CPrePxMsgId.cprepx017_any_without_endif, txtPos);

      public static Msg M018_NotEnoughMacroArguments(TxtPos? txtPos, string macroName) =>
         Msg.FromTxtPos(MsgType.warning, $"[CPrePx018] Not enough arguments passed to macro {macroName}.", CPrePxMsgId.cprepx018_not_enough_args_to_macro, txtPos);

      public static Msg M019_TooManyMacroArguments(TxtPos? txtPos, string macroName) =>
         Msg.FromTxtPos(MsgType.warning, $"[CPrePx019] Too many arguments passed to macro {macroName}.", CPrePxMsgId.cprepx019_too_many_args_to_macro, txtPos);

      public static Msg M020_AshNotFollowedByMacroArg(TxtPos? txtPos) =>
         Msg.FromTxtPos(MsgType.warning, "[CPrePx020] '#' not followed by macro argument.", CPrePxMsgId.cprepx020_ash_not_followed_by_macro_arg, txtPos);

      public static Msg M021_InvalidPath(TxtPos? txtPos, string path) => Msg.FromTxtPos(
            MsgType.fatal, $"[CPrePx021] Invalid path {path}.", CPrePxMsgId.cprepx021_invalid_path, txtPos);

      public static Msg M022_NoSuchInclude(TxtPos? txtPos, string? includeRelativePath) => Msg.FromTxtPos(
            MsgType.fatal, $"[CPrePx022] Include path not existing '{includeRelativePath}'.", CPrePxMsgId.cprepx022_invalid_path, txtPos);

      /// <summary>
      /// Specific error since MSVC signalize it as an error and gcc not.
      /// </summary>
      /// <param name="txtPos"></param>
      /// <returns></returns>
      public static Msg M023_ExtraTokenAfterPragmaOnce(TxtPos? txtPos) =>
         Msg.FromTxtPos(MsgType.warning, $"[CPrePx023] Extra token after '#pragma once'.", CPrePxMsgId.cprepx023_extra_token_after_pragma_once, txtPos);

      public static Msg M024_PragmaPackTooBig(TxtPos? txtPos, int pragmaPackValue) =>
         Msg.FromTxtPos(MsgType.warning, $"[CPrePx024] Too big #pragma pack value {pragmaPackValue}.", CPrePxMsgId.cprepx024_pragma_pack_too_big, txtPos);

      public static Msg M025_PragmaPackNotAPowerOfTwo(TxtPos? txtPos, int pragmaPackValue) =>
         Msg.FromTxtPos(MsgType.warning, $"[CPrePx025] #pragma pack value {pragmaPackValue} not a power of two.", CPrePxMsgId.cprepx025_pragma_pack_not_a_power_of_2, txtPos);

      public static Msg M026_DuplicatedMacroArg(TxtPos? txtPos, string argName) =>
         Msg.FromTxtPos(MsgType.fatal, $"[CPrePx026] Duplicated macro argument '{argName}'.", CPrePxMsgId.cprepx026_duplictaed_macro_arg, txtPos);

      public static Msg M027_UnterminatedComment(TxtPos? txtPos) =>
         Msg.FromTxtPos(MsgType.fatal, "[CPrePx027] Comment starting from here is unterminated.", CPrePxMsgId.cprepx027_unterminated_comment, txtPos);

      public static Msg M028_UnspecifiedPragmaWarning(TxtPos? txtPos, string pragmaKind) =>
         Msg.FromTxtPos(MsgType.warning, $"[CPrePx028] Unspecified #pragma {pragmaKind}.", CPrePxMsgId.cprepx028_unspecified_pragma, txtPos);

      public static Msg M029_EmptyPragma(TxtPos? txtPos) =>
         Msg.FromTxtPos(MsgType.warning, "[CPrePx029] Empty #pragma.", CPrePxMsgId.cprepx029_empty_pragma, txtPos);

      public static Msg M030_InvalidStringMerge(TxtPos? txtPos) =>
         Msg.FromTxtPos(MsgType.fatal, "[CPrePx030] Invalid string merge '##' shall be followed and preceded by string.", CPrePxMsgId.cprepx030_invalid_string_merge, txtPos);

      public static Msg M031_NotIntConstInIf(TxtPos? txtPos) =>
         Msg.FromTxtPos(MsgType.fatal, "[CPrePx030] Not an integer constant inside #if/#elif.", CPrePxMsgId.cprepx031_not_int_const_in_if, txtPos);

      public static Msg M032_IfElifExpandedToEmptyCondition(IIfElif ifElif)
      {
         var drc = (CPrePxDirective)ifElif;
         var txt_pos = drc.TxtToken?.Trim().To;

         return Msg.FromTxtPos(MsgType.fatal, $"[CPrePx032] #{drc.DirectiveName} expanded to empty condition.", CPrePxMsgId.cprepx032_if_elif_expanded_to_empty_condition, txt_pos);
      }

      public static Msg M033_DefinedNotInCorrectFormat(TxtToken definedToken) => 
         Msg.FromToken(MsgType.fatal, $"[CPrePx033] {IfElifExpr.DEFINED} not in format {IfElifExpr.DEFINED}(ID)", CPrePxMsgId.cprepx033_defined_not_in_correct_format, definedToken);

      public static Msg M034_TryToRedefineSysMacroWarning(TxtToken definedToken) =>
         Msg.FromToken(MsgType.warning, $"[CPrePx034] Trying to re-define sys macro '{definedToken.Content}'.", CPrePxMsgId.cprepx034_try_2_redefine_sys_macro, definedToken);

      public static Msg M035_InvalidPragmaCLink(TxtPos? txtPos) => 
         Msg.FromTxtPos(MsgType.fatal, $"[CPrePx035] Invalid pragma.", CPrePxMsgId.cprepx035_invalid_pragma_clink, txtPos);

      public static Msg M036_IteractionNumberExceeded(TxtToken? txtToken) => 
         Msg.FromToken(
            MsgType.fatal, 
            $"[CPrePx036] Number of iteration on same directive '{txtToken?.Content}' exceeded.",
            CPrePxMsgId.cprepx036_iteraction_number_exceeded, txtToken);
   }
}
