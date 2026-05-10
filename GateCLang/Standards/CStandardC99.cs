using Gate.CLanguage.Compiler;
using Gate.CLanguage.Expressions.COperators;
using Gate.CLanguage.Linker;
using Gate.CLanguage.PrePx;
using Gate.CLanguage.Runtime;
using Gate.CLanguage.Types;

namespace Gate.CLanguage.Standards
{
   public class CStandardC99 : CStandard
   {
      public CStandardC99()
      {
         
      }

      public class C99Compiler : CCompiler
      {
         public C99Compiler(CPrePxOptions prePxOptions, CCompilerSettings settings) : base(prePxOptions, settings) { }

         protected override CScopeHelper myMakeScopeHelper() => new CScopeHelper(CScopeSettings.Gcc);

         protected override CRtmObjStrategy myMakeRtmStrategy() => new CRtmObjStrategy();
      }

      /// <summary>
      /// GCC Standard C99 default settings.
      /// </summary>
      public class C99DefSettings : CCompilerSettings
      {
         /// <summary>
         /// Order to p.length descending (in order to avoid equivocation)
         /// </summary>
         public static readonly string[] PUNCTUATORS = new string[] {
            "[","]","(",")","{","}",".","->",
            "++","--","&","*","+","-","~","!",
            "/","%","<<",">>","<",">","<=",">=","==","!=","^","|","&&","||",
            "?",":",";","...",
            "=","*=","/=","%=","+=","-=","<<=",">>=","&=","^=","|=",
            ",","#","##",
            "<:",":>","<%","%>","%:","%:%:",}.OrderByDescending(p => p.Length).ToArray();

         public static readonly string[] KEYWORDS_NO_BUILT_IN = {
            "auto", "break", "case", "char", "const","continue", "default", "do", "double", "else", "enum", "extern", "for", "goto", "if", "inline", "register",
            "restrict", "return", "signed","sizeof",  "static", "struct", "switch", "typedef", "union", "volatile",
            "while", CAttribute.WinDeclSpec.TAG,CAttribute.GccAttribute.Tags[0],CAttribute.GccAttribute.Tags[1]};

         public C99DefSettings() : base(CLangFlags.gcc, CTypeBuiltInSet.Settings.MakeForGcc(false))
         {
            Punctuators = PUNCTUATORS;
            StringFlags = CharStandardStringFlags.gcc;
            AreDuplicatedUnsignedAllowed = false;
            AreInlineSupported = true;
            AreNestedFunctionDefSupported = true;
            IsRestrictAllowed = true;
            KeyWordsBasic = KEYWORDS_NO_BUILT_IN;
            AreEmptyEnumAccepted = false;
            AreEmptyStructAccepted = true;
            AreEmptyDeclSpecifierInStructAccepted = true;
            AreAttributeInDeclSApecifierOnly = true;
         }
      }

      public override string Id => "C99";

      public override CCompilerSettings DefaultCompilerSettings => new C99DefSettings();

      public override CLinkerSettings DefaultLinkerSettings => new CLinkerSettings();

      public override CPrePxOptions DefaultPrePxOptions
      {
         get
         {
            var opt = new CPrePxOptions();

            opt.WarningToErrors = new[] { CPrePxMsgId.cprepx018_not_enough_args_to_macro, CPrePxMsgId.cprepx019_too_many_args_to_macro };

            return base.DefaultPrePxOptions;
         }
      }

      protected override CCompiler myMakeCompiler() => new C99Compiler(PrePxOptions, DefaultCompilerSettings);

      protected override CLinker myMakeLinker() => new CLinker(DefaultLinkerSettings);
   }
}