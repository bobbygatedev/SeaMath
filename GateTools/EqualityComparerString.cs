using Gate.Tools.Extensions;

namespace Gate.Tools
{
   /// <summary>
   /// 
   /// </summary>
   public class EqualityComparerString : IEqualityComparer<string>
   {
      public enum ModeType
      {
         /// <summary>
         /// 
         /// </summary>
         case_sensitive ,
         
         /// <summary>
         /// 
         /// </summary>
         case_insensitive,
         
         /// <summary>
         /// using <see cref="Gate.Tools.Extensions.StringExtender.IsEqualNoContent(string, string, bool)"/>
         /// </summary>
         is_equal_no_content
      }

      public EqualityComparerString(ModeType mode = ModeType.case_sensitive) => Mode = mode;

      public ModeType Mode { get; }


      public bool Equals(string? x, string? y)
      {
         switch (Mode)
         {
            case ModeType.case_sensitive: return x == y;              
            case ModeType.case_insensitive: return string.Compare(x,y,true)==0;
            case ModeType.is_equal_no_content: return x.IsEqualNoContent(y);
            default: throw new Crash();
         }
      }

      public int GetHashCode(string obj) => obj.GetHashCode();
   }
}
