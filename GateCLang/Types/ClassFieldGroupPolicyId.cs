namespace Gate.CLanguage.Types
{
   public enum ClassFieldGroupPolicyId
   {
      /// <summary>
      /// 
      /// </summary>
      none = 0,

      /// <summary>
      /// 
      /// </summary>
      msvs = 1,

      /// <summary>
      /// 
      /// </summary>
      gcc_msys = 2,

      /// <summary>
      /// Bit field always packet together and not keeping into account of pack
      /// </summary>
      gcc_cygwin = 3,

      /// <summary>
      /// 
      /// </summary>
      custom = 4,
   }
}