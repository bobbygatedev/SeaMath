using Gate.CLanguage.Compiler;
using Gate.CLanguage.Expressions.COperators;
using Gate.CLanguage.Linker;
using Gate.CLanguage.PrePx;
using Gate.CLanguage.Types;
using System.Text;

namespace Gate.CLanguage.Standards
{
   public class CStandardMsvs : CStandardC99
   {
      /// <summary>
      /// Constructor.
      /// </summary>
      public CStandardMsvs() { }

      /// <summary>
      /// MS Visual Studio default settings.
      /// </summary>
      public class MsvsDefSettings : CCompilerSettings
      {
         public MsvsDefSettings() : base(CLangFlags.msvc, CTypeBuiltInSet.Settings.MakeForMsvc(false))
         {
            Punctuators = C99DefSettings.PUNCTUATORS;
            StringFlags = CharStandardStringFlags.msvc;
            AreDuplicatedUnsignedAllowed = false;
            AreInlineSupported = false;
            AreNestedFunctionDefSupported = false;
            IsRestrictAllowed = false;
            AreEmptyEnumAccepted = true;
            AreEmptyStructAccepted = false;
            AreEmptyDeclSpecifierInStructAccepted = false;
            AreAttributeInDeclSApecifierOnly = false;
            KeyWordsBasic = C99DefSettings.KEYWORDS_NO_BUILT_IN.Append(CAttribute.WinDeclSpec.TAG2).ToArray();
            CanFunctionDefinitionParamsBeAnonimous = false;
            IsVariableArraySizeAllowed = false;
            ClassFieldGroupPolicyId = ClassFieldGroupPolicyId.msvs;
            AreExtraInitAllowed = false;
            AreVarArrayValid = false;
            MayUniversalCharacterNameBeEmtpy = true;
            MayConstExceedBitsNum = false;
            CharEncodingEnv = CCharEncodingEnv.msvs;
            NarrowCharEncoding = Encoding.Default;
            WideCharEncoding = Encoding.Unicode;
         }
      }

      public class MsvsCompiler : C99Compiler
      {
         public MsvsCompiler(CPrePxOptions prePxOptions, CCompilerSettings settings) : base(prePxOptions, settings) { }

         protected override CScopeHelper myMakeScopeHelper() => new CScopeHelper(CScopeSettings.Msvc);
      }

      protected override CCompiler myMakeCompiler() => new MsvsCompiler(PrePxOptions, DefaultCompilerSettings);

      public override CCompilerSettings DefaultCompilerSettings => new MsvsDefSettings();

      public override CLinkerSettings DefaultLinkerSettings
      {
         get
         {
            var set = new CLinkerSettings();

            set.IsExternCompulsoryForVars = false;

            return set;
         }
      }

      public override string Id => "MSVS";
   }
}
