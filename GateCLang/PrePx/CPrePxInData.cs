using Gate.CLanguage.Compiler;
using Gate.CLanguage.PrePx.Directives.Macro.Predefined;
using Gate.CLanguage.Runtime;
using Gate.Tools.Message;
using Gate.Tools.Text.Elab;
using System.Collections.Generic;

namespace Gate.CLanguage.PrePx
{
   /// <summary>
   /// 
   /// </summary>
   public class CPrePxInData : TxtElabInData
   {
      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="options"></param>
      /// <param name="messages"></param>
      public CPrePxInData(CPrePx prepPx, MsgCollection messages, CCompilerSettings compileSettings, CRtmObjStrategy rtmStrategy) : base(messages)
      {
         PrePx = prepPx;
         CompilerSettings = compileSettings;
         RtmStrategy = rtmStrategy;     
      }

      /// <summary>
      /// Current <see cref="CPrePxSource"/> instance under pre-compiling.
      /// </summary>
      public CPrePxSource? CurrPrePxSource { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public List<CPrePxSource> ListHeaderSources { get; private set; } = new List<CPrePxSource>();
      
      /// <summary>
      ///  
      /// </summary>
      public CPrePx PrePx { get; }

      /// <summary>
      /// 
      /// </summary>
      public CPrePxOptions? Options => PrePx.Options;

      /// <summary>
      /// 
      /// </summary>
      public CPredefMacroData PredefMacroData { get; set; } = new CPredefMacroData();

      /// <summary>
      /// 
      /// </summary>
      public CCompilerSettings CompilerSettings { get;  }
      
      /// <summary>
      /// 
      /// </summary>
      public CRtmObjStrategy RtmStrategy { get;  }

      /// <summary>
      /// Used by <see cref="Gate.CLanguage.PrePx.Stages.CPrePxStage32Tokenisation"/>
      /// </summary>
      public int CurrLineIdxStage32Tokenisation { get; set; }
   }
}
