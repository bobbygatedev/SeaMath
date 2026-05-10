using Gate.Tools.Text;

namespace Gate.LangBase.Runtime.DbgEng
{
   /// <summary>
   /// Interface for instruction and breakpoint points.
   /// </summary>
   public interface IRtmDbgEngPoint
   {
      /// <summary>
      /// Token of the instruction or breakpoint point. Can be null if point is not associated with a token.
      /// </summary>
      TxtToken? Token { get; }
   }
}
