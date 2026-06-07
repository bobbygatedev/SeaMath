using Gate.CLanguage.Runtime;
using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.Object;
using Gate.Tools.Message;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Compiler
{
   /// <summary>
   /// 
   /// </summary>
   public class CCompilerInData : TxtElabInData, IExprSolverInData
   {
      private readonly CScopeHelper? myScopeHelper;
      private readonly CFunctionInstructionTranslator? myFunctionInstructionTranslator;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="messages"></param>
      /// <param name="settings"></param>
      /// <param name="functionInstructionTranslator"></param>
      /// <param name="rtmStrategy"></param>
      /// <param name="scopeHelper"></param>
      public CCompilerInData(
         MsgCollection messages, 
         CCompilerSettings settings,
         CFunctionInstructionTranslator? functionInstructionTranslator ,
         CRtmObjStrategy rtmStrategy, 
         CScopeHelper? scopeHelper)
         : base(messages)
      {
         Settings = settings;
         myFunctionInstructionTranslator = functionInstructionTranslator;
         RtmStrategy = rtmStrategy;
         myScopeHelper = scopeHelper;
      }

      /// <summary>
      /// 
      /// </summary>
      public CCompilerSettings Settings { get; }

      /// <summary>
      /// 
      /// </summary>
      public CFunctionInstructionTranslator FunctionInstructionTranslator => 
         myFunctionInstructionTranslator?? throw new NullReferenceException("FunctionInstructionTranslator not defined!");
      /// <summary>
      /// 
      /// </summary>
      public CScopeHelper ScopeHelper => myScopeHelper ?? throw new NullReferenceException("Scope Helper Not defined");
      /// <summary>
      /// 
      /// </summary>
      public CRtmObjStrategy RtmStrategy { get; }

      /// <summary>
      /// 
      /// </summary>
      public Dictionary<object,object?> AppData { get; private set; } = new Dictionary<object,object?>();

      /// <summary>
      /// 
      /// </summary>
      IRtmObjStrategy IExprSolverInData.RtmStrategy => RtmStrategy;
   }
}
