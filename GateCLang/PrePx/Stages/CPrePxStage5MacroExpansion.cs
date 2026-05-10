using Gate.CLanguage.PrePx.Directives;
using Gate.CLanguage.PrePx.Directives.Macro.Expansion;
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
         foreach (var drs_sec in store2Edit.OwnedSectors.Where(d => d.Tag is CPrePxDirective))
         {
            store2Edit.Fill(drs_sec.Interval);
         }

         var in_mrk = new TxtMarker(store2Edit);
         var in_dat = new MacroExpanderInData(data);

         return myMacroExpanderAllFile.PerformNoOutput(in_mrk, in_dat);
      }

      public override string ToString() => $"PrePx Stage5: Macro Expansion";
   }
}
