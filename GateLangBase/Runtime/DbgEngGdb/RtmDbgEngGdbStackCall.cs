using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;

namespace Gate.LangBase.Runtime.DbgEngGdb
{
   public class RtmDbgEngGdbStackCall : IRtmDbgEngStackCall
   {
      public IRtmDbgEngPoint? DbgPointCurrent => throw new System.NotImplementedException();

      public RtmObj[] ObjLocalsAll => throw new System.NotImplementedException();

      public string? FunctionName => throw new System.NotImplementedException();

      public string? FunctionInfo => throw new System.NotImplementedException();

      public RtmObj[] ObjAll => throw new System.NotImplementedException();

      public RtmObj[] ObjInStackOnly => throw new System.NotImplementedException();
   }
}
