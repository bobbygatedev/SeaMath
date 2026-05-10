using Gate.CLanguage.Linker;
using Gate.CLanguage.Source;
using Gate.CLanguage.Standards;
using Gate.LangBase.Runtime;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;

namespace Gate.CLanguage.Runtime
{
   /// <summary>
   /// 
   /// </summary>
   public class CRtmVirtCpuBuilder : RtmDbgEngVirtCpuBuilder
   {
      private CStandard myCStandard;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="dbgIde"></param>
      public CRtmVirtCpuBuilder(IRtmDbgEngIde dbgIde, CStandard? cStandard = null) : base(dbgIde) => myCStandard = cStandard ?? new CStandardC99();

      /// <summary>
      /// 
      /// </summary>
      public new CLibrary[]? Libraries { get => base.Libraries as CLibrary[]; set => base.Libraries = value; }

      /// <summary>
      /// 
      /// </summary>
      public CSource[] CSources
      {
         get
         {
            var csr = PseudoSources.OfType<CSource>().ToArray();

            return csr.Length == SourceFiles.Length ? csr : throw new Gate.LangBase.Runtime.RtmException("Not input files " + GetType().Name);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override IRtmDbgEngVirtCpuLinker VirtCpuLinker => new CRtmLinker(CStandard);

      /// <summary>
      /// 
      /// </summary>
      public override IRtmObjStrategy? RtmStrategy => myCStandard?.CCompiler.RtmStrategy;

      /// <summary>
      /// 
      /// </summary>
      public CStandard CStandard
      {
         get => myCStandard;
         set => myCStandard = value ?? new CStandardC99();
      }

      /// <summary>
      /// 
      /// </summary>
      public CLinkerResult? LinkerResult { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public override IRtmDbgEngVirtCpuCompiler VirtCpuCompiler => new CRtmCompiler(CStandard ?? throw new RtmException());
   }
}
