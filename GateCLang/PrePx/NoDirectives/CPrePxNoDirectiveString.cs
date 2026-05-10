namespace Gate.CLanguage.PrePx.NoDirectives
{
   public class CPrePxNoDirectiveString : CPrePxNoDirective
   {
      /// <summary>
      /// 
      /// </summary>
      public string StringValue => TxtToken != null ? TxtToken.Content : "";

      public override string ToString() => $"\"{StringValue}\"";
   }
}
