using Gate.Tools.Extensions;
using System.Text.RegularExpressions;

namespace Gate.CLanguage.PrePx.Directives.Macro.Predefined
{
   /// <summary>
   /// Provides a collection of predefined macros commonly used in C and C++ programming.
   /// </summary>
   /// <remarks>The <see cref="CPredefMacros"/> class contains nested types and properties that represent
   /// standard predefined macros. These macros are typically used in C and C++ code to provide information about the
   /// compilation environment, such as the current file, line number, or compilation date and time. The class also
   /// includes utilities to retrieve macros specific to C or C++ contexts.</remarks>
   public static class CPredefMacros
   {
      public static class Standard
      {
         private static CPredefMacro[] myAllForCpp = typeof(Standard).GetNestedTypes().Where(
            nt => !nt.IsAbstract && nt.IsSubclassOf(typeof(CPredefMacro))).Select(nt =>
               {
                  var cst = nt.GetConstructor([]);

                  return cst != null ? cst.Invoke([]) as CPredefMacro : null;

               }).Nn().Where(pm => pm.IsActive).ToArray();

         public static CPredefMacro[] AllForC => AllForCpp.Where(pm => !pm.IsForCppOnly).ToArray();

         public static CPredefMacro[] AllForCpp => myAllForCpp;

         /// <summary>
         /// 
         /// </summary>
         public class Line : CPredefMacro
         {
            public const string ID = "__LINE__";

            public override string FixedId => ID;

            public override bool IsForCppOnly => false;

            public override bool IsForCOnly => false;

            public override bool IsActive => true;

            public override string GetArgumentString(CPredefMacroData preDefData) => preDefData.CurrLine.ToString().Nn();

         }

         public class CPlusPlus : CPredefMacro
         {
            public const string ID = "__cplusplus";

            public override string FixedId => ID;

            public CPlusPlus() { }

            public override bool IsForCppOnly => true;

            public override bool IsForCOnly => false;

            public override bool IsActive => true;

            public override string GetArgumentString(CPredefMacroData predefMacroData) => "1";
         }

         /// <summary>
         /// 
         /// </summary>
         public class File : CPredefMacro
         {
            public const string ID = "__FILE__";

            public File() { }

            public override bool IsActive => true;

            public override string FixedId => ID;

            public override bool IsForCppOnly => false;

            public override bool IsForCOnly => false;

            public override string GetArgumentString(CPredefMacroData preDefData) => $"\"{Regex.Escape(preDefData?.CurrFile ?? "")}\"";
         }

         /// <summary>
         /// 
         /// </summary>
         public class Date : CPredefMacro
         {
            public const string ID = "__DATE__";

            public override string FixedId => ID;

            public override bool IsForCppOnly => false;

            public override bool IsForCOnly => false;

            public override bool IsActive => true;

            public override string GetArgumentString(CPredefMacroData preDefData) => preDefData.CompilationDate;
         }

         /// <summary>
         /// 
         /// </summary>
         public class Time : CPredefMacro
         {
            public const string ID = "__TIME__";

            public override string FixedId => ID;

            public override bool IsForCppOnly => false;

            public override bool IsForCOnly => false;

            public override bool IsActive => true;

            public override string GetArgumentString(CPredefMacroData preDefData) => preDefData.CompilationTime;
         }

         /// <summary>
         /// 
         /// </summary>
         public class Stdc : CPredefMacro
         {
            public const string ID = "__STDC__";

            public override string FixedId => ID;

            public override bool IsForCppOnly => false;

            public override bool IsForCOnly => true;

            public override bool IsActive => true;

            public override string GetArgumentString(CPredefMacroData preDefData) => "1";
         }

         /// <summary>
         /// 
         /// </summary>
         public class StdcHosted : CPredefMacro
         {
            public const string ID = "__STDC_HOSTED__";

            public StdcHosted(bool isTrue) => IsTrue = isTrue;

            public override string FixedId => ID;

            public override bool IsForCppOnly => false;

            public override bool IsForCOnly => true;

            public override string GetArgumentString(CPredefMacroData preDefData) => IsTrue ? "1" : "0";

            public override bool IsActive => true;

            public bool IsTrue { get; }
         }

         /// <summary>
         /// 
         /// </summary>
         public class Win64 : CPredefMacro
         {
            public const string ID = "_WIN64";

            public Win64() { }

            public override string FixedId => ID;

            public override bool IsForCppOnly => false;

            public override bool IsForCOnly => false;

            public unsafe override bool IsActive => sizeof(IntPtr) == 8;

            public override string GetArgumentString(CPredefMacroData preDefData) => "";
         }

      }
   }
}
