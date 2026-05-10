using System;

namespace Gate.CLanguage.PrePx.Directives.IfDefElif
{
   public class CPrePxDirectiveElse : CPrePxDirectiveIfDefElif
   {
      /// <summary>
      /// 
      /// </summary>
      public const string NAME = "else";

      /// <summary>
      /// 
      /// </summary>
      public override string Rebuilt => throw new NotImplementedException();

      /// <summary>
      /// 
      /// </summary>
      public override string DirectiveName => NAME;
   }
}
