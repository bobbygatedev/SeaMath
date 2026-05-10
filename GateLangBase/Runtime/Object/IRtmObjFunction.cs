using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.DbgEng;

namespace Gate.LangBase.Runtime.Object
{
   /// <summary>
   /// Defines a function object that can be executed within an RTM (Run-Time Model) environment and provides access to
   /// its declaration metadata.
   /// </summary>
   /// <remarks>Implementations of this interface represent executable functions that can be invoked with a
   /// stack context, an optional strategy, and a set of parameters. The interface also exposes the function's
   /// declaration, allowing callers to inspect its signature and metadata. Thread safety and execution semantics depend
   /// on the specific implementation.</remarks>
   public interface IRtmObjFunction
   {
      RtmObj? Exec(IRtmDbgEngStackExecutable? stack, IRtmObjStrategy? rtmStrategy, params RtmObj?[] @params);

      IDeclFunction? DeclFunction { get; }
   }
}
