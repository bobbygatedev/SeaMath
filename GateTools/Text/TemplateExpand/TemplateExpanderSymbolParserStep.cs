namespace Gate.Tools.Text.TemplateExpand
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class TemplateExpanderSymbolParserStep : TemplateExpanderSymbolParserBase
   {
      protected TemplateExpanderSymbolParserStep(TemplateExpanderSymbolParserMain parserMain) => ParserMain = parserMain;

      public TemplateExpanderSymbolParserMain ParserMain { get; }

      protected bool myLookForwardIniBorderMoveOver(TxtMarker txtMarker, out int startIdx)
      {
         if (txtMarker.LookForAnySignMoveOver(ParserMain.TokenBorders.ini))
         {
            //check if double ini 
            var idx = txtMarker.CurrIdx;

            startIdx = txtMarker.CurrIdx - ParserMain.TokenBorders.ini.Length;

            //eg tokenBorderStart = '!' if marking !! is considered as !
            return !txtMarker.IsMarkingAnySign(ParserMain.TokenBorders.ini) || idx != txtMarker.CurrIdx;
         }
         else { startIdx = txtMarker.CurrIdx; }

         return false;
      }

      protected bool myIsMarkingEndBorderMoveOver(TxtMarker txtMarker) => txtMarker.IsMarkingAnySignMoveOver(ParserMain.TokenBorders.end);

      protected string? myIsMarkingIniBorderMoveOverName(TxtMarker txtMarker, out int startIdx) =>
         myLookForwardIniBorderMoveOver(txtMarker, out startIdx) ? txtMarker.GetMarkingVarNameMoveOver() : null;

   }
}
