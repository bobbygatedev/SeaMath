using Gate.LangBase.Runtime.Object;

namespace Gate.LangBase.Runtime
{
   public abstract class RtmFormat
   {
      public delegate string FormatHandler(object csharpPrimtiveObj);

      public abstract string Format(RtmObj runTimeObj);
   }
}
