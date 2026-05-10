using Gate.CLanguage.Compiler;
using Gate.CLanguage.Linker;
using Gate.CLanguage.PrePx;
using Gate.CLanguage.Source;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.Tools;
using Gate.Tools.DesignPattern;
using Gate.Tools.Message;
using Gate.Tools.Text;
using System;

namespace Gate.CLanguage.Standards
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class CStandard : BaseClassWithFinalizer
   {
      private Lazy<CCompiler> myLazyCompiler;
      private Lazy<CLinker> myLazyLinker;

      public CStandard()
      {
         PrePxOptions = DefaultPrePxOptions;
         myLazyCompiler = new Lazy<CCompiler>(myMakeCompiler);
         myLazyLinker = new Lazy<CLinker>(myMakeLinker);
      }

      /// <summary>
      /// Id of the standard (eg C99).
      /// </summary>
      public abstract string Id { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected abstract CCompiler myMakeCompiler();

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected abstract CLinker myMakeLinker();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="filesOrdered"></param>
      /// <param name="messages"></param>
      /// <param name="pseudoExe"></param>
      /// <param name="name"></param>
      /// <returns></returns>
      public bool Make(
         string[] filesOrdered, MsgCollection messages, out RtmDbgEngVirtCpuPseudoExe pseudoExe, string name)
      {
         var res = true;

         pseudoExe = new RtmDbgEngVirtCpuPseudoExe(name);

         foreach (var fil in filesOrdered)
         {
            res &= CCompiler.Compile(TxtStore.FromPath(fil), messages, out var src);
            pseudoExe.AddSources(src??throw new Crash());
         }

         res &= Linker.Link((CSource[])pseudoExe.Sources, messages).IsSuccess;
         messages.Add(new Msg(MsgType.info, messages.Resume, null, null));

         return res;
      }

      /// <summary>
      /// 
      /// </summary>
      public abstract CCompilerSettings DefaultCompilerSettings { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract CLinkerSettings DefaultLinkerSettings { get; }

      /// <summary>
      /// 
      /// </summary>
      public virtual CPrePxOptions DefaultPrePxOptions => new CPrePxOptions();

      /// <summary>
      /// 
      /// </summary>
      public CLanguage Language => CCompiler.Settings.Language;

      /// <summary>
      /// 
      /// </summary>
      public CPrePxOptions PrePxOptions { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public CCompiler CCompiler => myLazyCompiler.Value;

      /// <summary>
      /// 
      /// </summary>
      public CLinker Linker => myLazyLinker.Value;

      protected override void myFreeManaged() => CCompiler.Dispose();

      protected override void myFreeUnmanaged() { }
   }
}

