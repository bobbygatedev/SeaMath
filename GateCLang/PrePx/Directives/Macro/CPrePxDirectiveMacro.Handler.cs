using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using static Gate.CLanguage.PrePx.Directives.Macro.CPrePxDirectiveMacro;

namespace Gate.CLanguage.PrePx.Directives.Macro
{
   /// <summary>
   /// 
   /// </summary>
   public class CPrePxDirectiveMacroHandler : CPrePxDirectiveHandler
   {

      private class InnerActionCheckSysMacroOverload : CPrePxDirectiveAction
      {
         public override TxtElabResult Action(TxtStore prepxStore, TxtStore.Sector currLineSector, CPrePxInData data)
         {
            var mcr = currLineSector.Tag.ConvertOrCrash<CPrePxDirectiveMacro>();
            var sys_mcs = data.PrePx.PredefMacros;

            if (sys_mcs.Any(m => m.Identifier == mcr.Identifier))
            {
               var col = currLineSector.Content.IndexOf(mcr?.Identifier ?? "") + 1;
               var tok = new TxtTokenConst(
                  prepxStore,
                  Interval.FromFromLen(
                     new TxtPos(
                        currLineSector.From?.Line ?? throw new Crash(), col, prepxStore).StoreIdx,
                        mcr?.Identifier?.Length ?? 0));

               data.Messages.Add(CPrePxMessages.M034_TryToRedefineSysMacroWarning(tok));
            }

            return TxtElabResult.success;
         }
      }



      public override string TokenName => DIRECTIVE_NAME;

      public override CPrePxDirectiveAction Action => new InnerActionCheckSysMacroOverload();

      public override CPrePxParserStep LineParser => new CPrePxDirectiveMacroParserStep();
   }

   /// <summary>
   /// 
   /// </summary>
   public partial class CPrePxDirectiveMacro : CPrePxDirective, IWithIdentifierSettable
   {
      /// <summary>
      ///  the identifier.
      /// </summary>
      public virtual string? Identifier { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsAnonimous => (Identifier ?? "").Trim() == "";
   }
}
