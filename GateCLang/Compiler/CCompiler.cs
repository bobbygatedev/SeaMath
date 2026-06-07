using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.PrePx;
using Gate.CLanguage.Runtime;
using Gate.CLanguage.Source;
using Gate.CLanguage.TokenParse;
using Gate.Tools;
using Gate.Tools.Message;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Compiler
{
   /// <summary>
   /// Using Factory method.
   /// </summary>
   public abstract class CCompiler : HierarchicalItemWithFinalizer
   {
      private Lazy<CInterpreter> myLazyInterpreter;
      private Lazy<CScopeHelper> myLazyScopeHelper;
      private Lazy<CRtmObjStrategy> myLazyRtmStrategy;
      private Lazy<CTokenParser> myLazyTokenParser;
      private CFunctionInstructionTranslator? myFunctionInstructorTranslator;

      public CCompiler(CPrePxOptions prePxOptions, CCompilerSettings settings)
      {
         PrePx = myMakePrePx();
         PrePx.Options = prePxOptions;
         Settings = settings;
         myLazyInterpreter = new Lazy<CInterpreter>(myMakeInterpreter);
         myLazyScopeHelper = new Lazy<CScopeHelper>(myMakeScopeHelper);
         myLazyRtmStrategy = new Lazy<CRtmObjStrategy>(myMakeRtmStrategy);
         myLazyTokenParser = new Lazy<CTokenParser>(myMakeTokenParserParser);
      }

      public CCompilerSettings Settings { get; }

      public CInterpreter Interpreter => myLazyInterpreter.Value;

      public CScopeHelper ScopeHelper => myLazyScopeHelper.Value;

      public CRtmObjStrategy RtmStrategy => myLazyRtmStrategy.Value;

      public CTokenParser TokenParser => myLazyTokenParser.Value;

      public CPrePx PrePx { get; }

      public CFunctionInstructionTranslator FunctionInstructorTranslator
      {
         get
         {
            if (myFunctionInstructorTranslator == null)
            {
               myFunctionInstructorTranslator = myMakeFunctionInstructionTranslator();
            }

            return myFunctionInstructorTranslator;
         }
      }

      protected abstract CScopeHelper myMakeScopeHelper();

      protected abstract CRtmObjStrategy myMakeRtmStrategy();

      protected virtual CInterpreter myMakeInterpreter() => new CInterpreter(Settings.LangFlags);

      protected virtual CTokenParser myMakeTokenParserParser() => new CTokenParser();

      protected virtual CPrePx myMakePrePx() => new CPrePx();

      protected virtual CFunctionInstructionTranslator myMakeFunctionInstructionTranslator() => new CFunctionInstructionTranslator(RtmStrategy);

      public virtual CCompilerInData GetInData(MsgCollection messages) =>
         new CCompilerInData(
            messages, Settings, FunctionInstructorTranslator, RtmStrategy, ScopeHelper);


      public bool Compile(TxtStore file, MsgCollection messages, out CSource? source)
      {
         var pre_px_dat = new CPrePxInData(PrePx, messages, Settings, RtmStrategy);

         var res = PrePx.Start(file, CPrePxFileOptions.is_source, pre_px_dat, out var pre_px_src);

         source = myMakeSource();

         switch (res)
         {
            case TxtElabResult.success: break;

            case TxtElabResult.failure:
            case TxtElabResult.failure_unrecoverable:
               return false;

            case TxtElabResult.continue_searching:
            default:
               throw new Crash();
         }

         //interpreter         
         var tok_prs_out = new CTokenParserOutput();
         var in_dat = GetInData(messages);
         var pre_px_txt = pre_px_src?.ToCompileStore ?? throw new Crash();

         res = TokenParser.Perform(new TxtMarker(pre_px_txt), in_dat, ref tok_prs_out);

         if (res == TxtElabResult.success)
         {
            var tok_lst = tok_prs_out?.GetTextTokenList() ?? throw new Crash();

            source.PrePxSource = pre_px_src;

            if (res == TxtElabResult.success)
            {
               var inr = myMakeInterpreter();

               res = inr.Start(tok_lst, in_dat, ref source);
            }
         }
         else { source = null; }

         if (res == TxtElabResult.success) { return true; }
         else
         {
            messages.Add(new Msg(MsgType.fail, $"Compile of {file.FileInfo?.Name} failed!"));

            return false;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="content"></param>
      /// <returns></returns>
      /// <exception cref="Gate.CLanguage.CLangException"></exception> 
      public CSource Parse(string content)
      {
         var mgs = new MsgCollection();
         var res = Compile(new TxtStore(content), mgs, out var src);

         if (res)
         {
            return src ?? throw new Crash();
         }
         else
         {
            throw new Gate.CLanguage.CLangException(mgs);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="constExpression"></param>
      /// <returns></returns>
      /// <exception cref="Gate.CLanguage.CLangException"></exception> 
      public CExprStatement ParseConstExpression(string constExpression)
      {
         var src = Parse($"void main(void){{{constExpression};}}");

         var exp = src.AllDescendant.OfType<CExprStatement>().FirstOrDefault();

         return exp != null ? (CExprStatement)exp.Clone() : throw new Gate.CLanguage.CLangException("Can't get a constant expression");
      }

      protected virtual CSource myMakeSource() => new CSource();

      protected override void myFreeManaged()
      {
         if (myLazyRtmStrategy.IsValueCreated)
         {
            myLazyRtmStrategy.Value.Dispose();
         }
      }

      protected override void myFreeUnmanaged() { }
   }
}
