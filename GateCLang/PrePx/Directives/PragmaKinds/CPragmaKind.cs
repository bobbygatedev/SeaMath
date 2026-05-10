using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.Directives.PragmaKinds
{
   public abstract class CPragmaKind : CItem
   {
      public CPragmaKind()
      {

      }

      /// <summary>
      /// 
      /// </summary>
      public override bool HasAssociatedPragma => false;

      public abstract string KindName { get; }

      public new CPrePxDirectivePragma? ParentItem => base.ParentItem as CPrePxDirectivePragma;

      public abstract class Parser : ParserStep<CPrePxInData, TxtElabSingleOutput<CPragmaKind>>
      {
      }
   }
}
