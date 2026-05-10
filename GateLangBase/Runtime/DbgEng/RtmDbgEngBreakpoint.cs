namespace Gate.LangBase.Runtime.DbgEng
{
   /// <summary>
   /// 
   /// </summary>
   public class RtmDbgEngBreakpoint : IRtmDbgEngBreakpoint
   {
      public RtmDbgEngBreakpoint(string docPath, int docLine)
      {
         DocPath = docPath;
         DocLine = docLine;
         IsToBeReapply = true;
      }

      public string DocPath { get; }

      public int DocLine { get; }

      public bool IsToBeReapply { get; set; }

      public override string ToString() => $"Ln:{DocLine}({DocPath})({(IsToBeReapply ? "Reapply" : "")})";
   }
}
