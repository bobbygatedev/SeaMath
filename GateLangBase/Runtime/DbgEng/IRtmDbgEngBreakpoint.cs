namespace Gate.LangBase.Runtime.DbgEng
{
   public interface IRtmDbgEngBreakpoint
   {
      int DocLine { get; }
      string DocPath { get; }
      bool IsToBeReapply { get; set; }
   }
}