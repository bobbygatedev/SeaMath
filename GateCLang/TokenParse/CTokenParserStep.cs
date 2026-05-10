using Gate.CLanguage.Compiler;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.TokenParse
{
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="DATA"></typeparam>
   public abstract class CTokenParserStep : ParserStep<CCompilerInData, CTokenParserOutput> { }
}
