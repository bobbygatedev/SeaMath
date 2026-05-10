namespace Gate.CLanguage.PrePx.Directives
{
   /// <summary>
   /// Interface for macro define/undef classes (<seealso cref="CPrePxDirectiveMacro"/> ,<seealso cref="CPrePxDirectiveUndef"/>).
   /// </summary>
   public interface IPrePxDirectiveDefUndef
   {
      /// <summary>
      /// Id (alias for <seealso cref="CPrePxDirectiveMacro.Identifier"/>
      /// </summary>
      string? Id { get; }

      /// <summary>
      /// True for <seealso cref="CPrePxDirectiveMacro"/> false for <seealso cref="CPrePxDirectiveUndef"/>
      /// </summary>
      bool IsDefine { get; }
   }
}
