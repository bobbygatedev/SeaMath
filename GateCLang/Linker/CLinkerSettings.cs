namespace Gate.CLanguage.Linker
{
   public class CLinkerSettings
   {
      /// <summary>
      /// A not initialised global var can be linked to another even if is not marked as 'extern'
      /// </summary>
      public bool IsExternCompulsoryForVars { get; set; } = false;

      /// <summary>
      /// Linker policy to use
      /// </summary>
      public CLinkerPolicy LinkerPolicy { get; set; } = CLinkerPolicy.strict;
   }
}
