using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.Object;

namespace Gate.LangBase.Runtime.DbgEng
{
   /// <summary>
   /// Instruction interface for <see cref="IRtmDbgEngStackExecutable"/>.
   /// </summary>
   public interface IRtmDbgEngInstruction : IRtmDbgEngPoint
   {
      IDeclFunction? DeclFunction { get; }

      void Run(IRtmDbgEngStackExecutable? stack, IRtmObjStrategy? rtmStrategy);
      
      string ToString();
   }
}