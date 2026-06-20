using Gate.CLanguage.PrePx;
using Gate.CLanguage.PrePx.Directives.Macro.Predefined;
using Gate.CLanguage.PrePx.Stages;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   ///  
   /// </summary>
   public class SeaPrePx : CPrePx
   {
      public SeaPrePx(SeaCCompiler seaCCompiler) => SeaCCompiler = seaCCompiler;

      protected class PreStage : ICPrePxStage
      {
         public PreStage(SeaPrePx prePx) => PrePx = prePx;

         public bool IsForSourceOnly => true;

         public SeaPrePx PrePx { get; }

         public TxtElabResult Start(CPrePxInData data, TxtStore store2Edit, ref CPrePxOutput output)
         {
            //all predefined headers inserted at top of file (with full path)
            var hds = PrePx.SeaCCompiler.DbgIde.Workspace.Predefineds.AllFiles;
            var hds_txt = string.Join(store2Edit.Settings.NewLine, hds.Select(f => $"#include \"{f.FullName}\""));

            //todo an option (+pragma) should disable insert at of predefined headers at top of file 

            store2Edit.InsertLines(1, hds_txt);

            return TxtElabResult.success;
         }

         public override string ToString() => $"Sea PrePx";
      }

      public SeaCCompiler SeaCCompiler { get; }

      public override CPredefMacro[] PredefMacros => [.. base.PredefMacros, new SeaPreDefMacro()];

      protected override ICPrePxStage[] myMakeStages() => new[] { new PreStage(this) }.Concat(base.myMakeStages()).ToArray();
   }
}
