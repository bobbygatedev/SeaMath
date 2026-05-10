namespace Gate.Tools
{
   /// <summary>
   /// 
   /// </summary>
   public enum BinaryType : uint
   {
      /// <summary>
      /// A 32-bit Windows-based application
      /// </summary>
      SCS_32BIT_BINARY = 0,

      /// <summary>
      /// A 64-bit Windows-based application.
      /// </summary>
      SCS_64BIT_BINARY = 6,

      /// <summary>
      /// An MS-DOS – based application
      /// </summary>
      SCS_DOS_BINARY = 1,

      /// <summary>
      /// A 16-bit OS/2-based application
      /// </summary>
      SCS_OS216_BINARY = 5, 

      /// <summary>
      /// A PIF file that executes an MS-DOS – based application
      /// </summary>
      SCS_PIF_BINARY = 3, 

      /// <summary>
      /// A POSIX – based application
      /// </summary>
      SCS_POSIX_BINARY = 4, 

      /// <summary>
      /// A 16-bit Windows-based application
      /// </summary>
      SCS_WOW_BINARY = 2, 
   }
}
