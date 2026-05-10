using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using static Gate.Tools.Text.TxtStore;

namespace Gate.CLanguage.PrePx
{
   /// <summary>
   /// During <seealso cref="Gate.CLanguage.PrePx.Stages.CPrePxStage4DoDirectiveAction"/> provides abstract generic class for directive action.
   /// </summary>
   public abstract class CPrePxDirectiveAction
   {
      /// <summary>
      /// Do nothing (eg in #undef) <see cref="Gate.CLanguage.PrePx.Directives.CPrePxDirective"/> NOT causes changes in source content.
      /// </summary>
      public class NotAnAction : CPrePxDirectiveAction
      {
         public override TxtElabResult Action(TxtStore prepxStore, Sector currLineSector, CPrePxInData data) => TxtElabResult.success;
      }

      /// <summary>
      /// <br>Action entry point, an action produces changes in content of <paramref name="prepxStore"/> ie source code under preprocessing.</br>
      /// <br> Line.Tag shall be populated with directive instance.</br>
      /// </summary>
      /// <brm name="prepxStore"></brm>
      /// <brm name="currLineSector">Line whose Tag is populated with <see cref="Gate.CLanguage.PrePx.Directives.CPrePxDirective"/>/></brm>
      /// <brm name="data"></brm>
      /// <returns></returns>
      public abstract TxtElabResult Action(TxtStore prepxStore, Sector currLineSector, CPrePxInData data);
   }
}
