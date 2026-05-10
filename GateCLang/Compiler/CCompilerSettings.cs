using Gate.CLanguage.Expressions.COperators;
using Gate.CLanguage.Standards;
using Gate.CLanguage.Types;
using Gate.Tools;
using System.Text;

namespace Gate.CLanguage.Compiler
{
   /// <summary>
   /// 
   /// </summary>
   public class CCompilerSettings
   {
      private Lazy<CTypeBuiltInSet>? myLazyBuiltInSet;
      private CTypeBuiltInSet.Settings? myBuiltInSetSettings;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="language"></param>
      public CCompilerSettings(CLangFlags langFlags, CTypeBuiltInSet.Settings builtInSetSettings)
      {
         Language = myGetLanguage(LangFlags = langFlags);
         BuiltInSetSettings = builtInSetSettings;
      }

      /// <summary>
      /// True if 'unsigned unsigned' or 'signed signed' is allowed.
      /// </summary>
      public bool AreDuplicatedUnsignedAllowed { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public bool AreNestedFunctionDefSupported { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public bool AreInlineSupported { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsRestrictAllowed { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public bool AreEmptyStructAccepted { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public bool AreEmptyDeclSpecifierInStructAccepted { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public bool AreEmptyEnumAccepted { get; set; }

      /// <summary>
      /// <br>True: attributes only in declarator specifier only(MSVC) int __declspec(dllexport) var;(ok) int var __declspec(dllexport);</br> 
      /// <br>False: attributes in declarator specifier, array subscript, type name (eg (int _atr_)2u;</br> 
      /// </summary>
      public bool AreAttributeInDeclSApecifierOnly { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public CLanguage Language { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public CLangFlags LangFlags { get; }

      /// <summary>
      /// 
      /// </summary>
      public string[]? Punctuators { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public string[]? KeyWordsBasic { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public string[]? KeyWords => KeyWordsBasic?.Concat(BuiltInSet?.AllKeywords ?? []).ToArray();

      /// <summary>
      /// 
      /// </summary>
      public CharStandardStringFlags StringFlags { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public CTypeBuiltInSet? BuiltInSet => myLazyBuiltInSet?.Value;

      /// <summary>
      /// 
      /// </summary>
      public CTypeBuiltInSet.Settings? BuiltInSetSettings
      {
         get => myBuiltInSetSettings;
         set
         {
            myBuiltInSetSettings = value;
            myLazyBuiltInSet = new Lazy<CTypeBuiltInSet>(myMakeBuiltInSet);
         }
      }

      /// <summary>
      /// Whether a global var init can contains variables (eg 'int c = a + b;', true in C false in C++)
      /// </summary>
      public bool IsGlobalInitToBeConstant { get; set; } = true;

      /// <summary>
      /// In this case expressions such as 'int a; int v[a];' are valid in local context.
      /// </summary>
      public bool AreVarArrayValid { get; set; } = true;

      /// <summary>
      /// 
      /// </summary>
      public bool IsImplicitFunctionCallAccepted { get; set; } = true;

      /// <summary>
      /// <br>If selected function definition paramters can be anonimous (eg 'void f(int){ printf("Hallo world"); }' </br>
      /// <br>True for MSVS, false for GCC</br>
      /// </summary>
      public bool CanFunctionDefinitionParamsBeAnonimous { get; set; } = true;

      /// <summary>
      /// When true variable size array eg 'int s; int v[s]' are allowed in local scope only.
      /// </summary>
      public bool IsVariableArraySizeAllowed { get; set; } = true;

      /// <summary>
      /// Pack in bytes (like in #pragma pack)
      /// </summary>
      public int Pack { get; set; } = 4;

      /// <summary>
      /// 
      /// </summary>
      public ClassFieldGroupPolicyId ClassFieldGroupPolicyId { get; set; } = ClassFieldGroupPolicyId.gcc_msys;

      /// <summary>
      /// 
      /// </summary>
      public string ClassFieldGroupCustomPolicyName { get; set; } = "";

      /// <summary>
      /// 
      /// </summary>
      public bool AreExtraInitAllowed { get; set; } = true;

      /// <summary>
      /// If true a statement like L"\u00" is encoded into "u00" (MSVS) otherwise it's an not a valid universal character error.
      /// </summary>
      public bool MayUniversalCharacterNameBeEmtpy { get; set; } = false;

      /// <summary>
      /// If true a hex constant can exceed capacity(eg 0xff for char) otherwise an error is signalled (eg char s[] = "\xabcd" is valid when flag is hi)
      /// </summary>
      public bool MayConstExceedBitsNum { get; set; } = true;

      /// <summary>
      /// eg in case 
      /// </summary>
      public bool IsLastCharToSelect { get; set; } = true;

      /// <summary>
      /// 
      /// </summary>
      public CCharEncodingEnv CharEncodingEnv { get; set; } = CCharEncodingEnv.gcc;

      /// <summary>
      /// 
      /// </summary>
      public bool IsDefaultIntAcceptable { get; set; } = false;

      /// <summary>
      /// 
      /// </summary>
      public Encoding NarrowCharEncoding { get; set; } = Encoding.UTF8;

      /// <summary>
      /// 
      /// </summary>
      public Encoding WideCharEncoding { get; set; } = Encoding.UTF32;

      private CLanguage myGetLanguage(CLangFlags langFlags)
      {
         if ((langFlags & CLangFlags.cpp) != 0) { return CLanguage.cpp; }
         else if ((langFlags & CLangFlags.c) != 0) { return CLanguage.c; }
         else { throw new Crash(); }
      }

      private CTypeBuiltInSet myMakeBuiltInSet() => new CTypeBuiltInSet(BuiltInSetSettings ?? throw new Crash());
   }
}
