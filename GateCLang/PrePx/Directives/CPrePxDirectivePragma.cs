using Gate.CLanguage.PrePx.Directives.PragmaKinds;

namespace Gate.CLanguage.PrePx.Directives
{
   /// <summary>
   /// 
   /// </summary>
   public class CPrePxDirectivePragma : CPrePxDirective
   {
      /// <summary>
      /// 
      /// </summary>
      public CPrePxDirectivePragma()
      {

      }

      /// <summary>
      /// 
      /// </summary>
      public override string DirectiveName => "pragma";

      /// <summary>
      /// 
      /// </summary>
      public CPragmaKind? PragmaKind
      {
         get => SubItems.OfType<CPragmaKind>().FirstOrDefault();

         set
         {
            myRemoveSubItemRange(SubItems.OfType<CPragmaKind>());
            myAddSubItem(value);
         }
      }

      public override string ToString() => $"#pragma {(ContentToken != null ? $" {ContentToken.Content.Trim()}" : "")}";
   }
}