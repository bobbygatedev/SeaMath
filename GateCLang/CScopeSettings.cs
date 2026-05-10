namespace Gate.CLanguage
{
   /// <summary>
   /// 
   /// </summary>
   public class CScopeSettings
   {
      public static CScopeSettings Default => new CScopeSettings();

      public static CScopeSettings Msvc
      {
         get
         {
            var msc = Default;

            msc.AreIncompleteEnumValid = true;
            msc.AreTypeSpecifierRedeclarable = true;
            msc.IsInternalLinkWeak = true;

            return msc;
         }
      }

      public static CScopeSettings Gcc => Default;

      /// <summary>
      /// 
      /// </summary>
      public bool AreIncompleteEnumValid { get; set; } = false;

      /// <summary>
      /// When true a declaration like 'int a; float a;' is valid (MSVC), typebase is the one of first instance (in this case 'int').
      /// </summary>
      public bool AreTypeSpecifierRedeclarable { get; set; } = false;

      /// <summary>
      ///<br> Check is of internal link is made by name only. </br>
      ///<br> eg 'int a; float a;' 'int f(void); int f(float);' are ok with weak internal link ( but 'int a; const int a' no => change type modifier)'  </br>
      /// <br> Otherwise strict internal link is made</br>
      /// <br>True for MSVS, false for GCC</br>
      /// </summary>
      public bool IsInternalLinkWeak { get; set; } = false;
   }
}
