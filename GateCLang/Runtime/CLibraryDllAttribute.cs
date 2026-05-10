namespace Gate.CLanguage.Runtime
{
   /// <summary>
   /// Specifies the name of the native C library associated with the attributed element.
   /// </summary>
   /// <remarks>Apply this attribute to indicate which native library should be used for interop or platform
   /// invocation scenarios. This is typically used to assist with locating and loading the correct native DLL at
   /// runtime.</remarks>
   public class CLibraryDllAttribute : Attribute
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="CLibraryDllAttribute"/> class with the specified library name.
      /// </summary>
      public string? Name { get; set; }
   }
}
