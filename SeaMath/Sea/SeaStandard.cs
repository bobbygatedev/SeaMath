using Gate.CLanguage.Compiler;
using Gate.CLanguage.Linker;
using Gate.CLanguage.Standards;
using Gate.Tools;
using Gate.Tools.AppParams;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaStandard : CStandardC99
   {
      public SeaStandard(SeaMathDbgIde? dbgIde) => DbgIde = dbgIde;

      private void CompileLinkSettings_OnAnyChange(AppParam changedParamField) => myReadIncludeDirs();

      private void myReadIncludeDirs()
      {
         var drs = (DbgIde?.OptionPage ?? throw new Crash()).CompileLinkSettings.LibDirs;

         drs = drs.Concat((DbgIde?.Workspace ?? throw new Crash()).Sources.IncludeDirs).ToArray();
         PrePxOptions.IncludeDirs = drs;
      }

      public new SeaCCompiler CCompiler => (SeaCCompiler)base.CCompiler;

      public override CCompilerSettings DefaultCompilerSettings => new SeaDefSettings();

      public SeaMathDbgIde? DbgIde { get; }

      protected override CCompiler myMakeCompiler()
      {
         ((DbgIde?.OptionPage?.CompileLinkSettings ?? throw new Crash()).OnAnyChange) += CompileLinkSettings_OnAnyChange;
         myReadIncludeDirs();

         return new SeaCCompiler(DbgIde, PrePxOptions, DefaultCompilerSettings);
      }

      protected override CLinker myMakeLinker() => new SeaCLinker(DefaultLinkerSettings);
   }
}
