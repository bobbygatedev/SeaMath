using Gate.CLanguage.PrePx.Directives;
using Gate.CLanguage.PrePx.Directives.Macro.Expansion;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.Stages
{
   /// <summary>
   /// 
   /// </summary>
   public class CPrePxStage5MacroExpansion : ICPrePxStage
   {
      private readonly MacroExpanderStep.IterateWhileSuccess myMacroExpanderAllFile;

      public CPrePxStage5MacroExpansion(MacroExpanderStep? macroExpanderStep = null)
      {
         MacroExpanderStep = macroExpanderStep ?? new MacroExpanderStep();
         myMacroExpanderAllFile = new MacroExpanderStep.IterateWhileSuccess(MacroExpanderStep);
      }

      public bool IsForSourceOnly => true;

      public MacroExpanderStep MacroExpanderStep { get; }

      public TxtElabResult Start(CPrePxInData data, TxtStore store2Edit, ref CPrePxOutput output)
      {
         myBlankDirectiveLines(store2Edit);

         var in_mrk = new TxtMarker(store2Edit);
         var in_dat = new MacroExpanderInData(data);

         return myMacroExpanderAllFile.PerformNoOutput(in_mrk, in_dat);
      }

      /// <summary>
      /// Blanks out lines containing preprocessor directives in the given text store.
      /// </summary>
      /// <param name="store2Edit">The text store to edit.</param>
      private static void myBlankDirectiveLines(TxtStore store2Edit)
      {
         // all files sector associated to a directive
         var scs = store2Edit.OwnedSectors.Where(s => s.Tag is CPrePxDirective).ToArray();

         //retrieves the lins associated to directives and fills them in the store
         var lns = scs.Select(s => store2Edit[s.From.NnOrCrash().Line]).ToArray();

         //and replace directive lines with blank spaces (eg #define ... => "          ")
         foreach (var ln in lns)
         {
            store2Edit.Fill(ln.Interval);
         }
      }

      public override string ToString() => $"PrePx Stage5: Macro Expansion";
   }
}
