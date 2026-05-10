namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// Interface for pseudo source ie the compiled output 
   /// </summary>
   public interface IRtmDbgEngVirtCpuPseudoSource : IRtmDbgEngVirtPseudoExeItem
   {
      /// <summary>
      /// Source file content has changed with respect compiled source.
      /// </summary>
      bool IsDirty { get; }
   }
}
