namespace Gate.CLanguage.PrePx.Directives.IfDefElif
{

   /// <summary>
   /// 
   /// </summary>
   public class CPrePxDirectiveIfDef : CPrePxDirectiveIfDefElif, IWithIdentifierSettable
   {
      public CPrePxDirectiveIfDef() { }


      public const string NAME = "ifdef";

      public override string DirectiveName => NAME;

      public string? Identifier { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsAnonimous => (Identifier ?? "").Trim() == "";
   }
}
