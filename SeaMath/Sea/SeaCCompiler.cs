using Gate.CLanguage;
using Gate.CLanguage.Compiler;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.PrePx;
using Gate.CLanguage.Runtime;
using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Source;
using Gate.CLanguage.Standards;
using Gate.Tools;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// Represents a Sea C-Compiler that extends the C99 standard compiler with additional functionality specific to the
   /// Sea language.
   /// </summary>
   /// <remarks>This class provides specialized behavior for compiling and interpreting Sea language code,
   /// including custom runtime strategies, source handling, and scope management. It integrates with the SeaMathDbgIde
   /// debugging environment and ensures compatibility with Sea-specific language features.</remarks>
   public class SeaCCompiler : CStandardC99.C99Compiler
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="prePxOptions"></param>
      /// <param name="settings"></param>
      public SeaCCompiler(SeaMathDbgIde dbgIde, CPrePxOptions prePxOptions, CCompilerSettings settings) : base(prePxOptions, settings)
      {
         DbgIde = dbgIde ?? throw new Crash();
         settings.KeyWordsBasic = (settings.KeyWordsBasic ?? []).Append(SeaType.NAME).ToArray();
      }

      public new SeaInterpret? Interpreter => base.Interpreter as SeaInterpret;

      public SeaMathDbgIde DbgIde { get; }

      public CRtmObjAllocatorByPrivateHeap? Allocator => RtmStrategy.Allocator as CRtmObjAllocatorByPrivateHeap;

      protected override CPrePx myMakePrePx() => new SeaPrePx(this);

      protected override CInterpreter myMakeInterpreter() => new SeaInterpret(Settings.LangFlags, this);

      protected override CRtmObjStrategy myMakeRtmStrategy() => new SeaRtmStrategy(Settings as SeaDefSettings ?? throw new Crash());

      protected override CSource myMakeSource() => new SeaSource();

      protected override CScopeHelper myMakeScopeHelper() => new SeaScopeHelper(DbgIde, false);
   }
}
