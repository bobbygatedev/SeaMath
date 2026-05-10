using Gate.Tools.Extensions;

namespace Gate.CLanguage.PrePx.Directives.IfDefElif
{
   /// <summary>
   /// 
   /// </summary>
   public class CPrePxDirectiveIfndef : CPrePxDirectiveIfDefElif, IWithIdentifierSettable
   {
      public const string NAME = "ifndef";

      /// <summary>
      /// 
      /// </summary>
      public override string DirectiveName => NAME;

      /// <summary>
      /// 
      /// </summary>
      public string? Identifier { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsAnonimous => Identifier.ExtTrim() == "";
   }
}
