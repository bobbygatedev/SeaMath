namespace Gate.Tools
{
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="ITM"></typeparam>
   public class EqualityComparerByRef<ITM> : IEqualityComparer<ITM>
   {
      public bool Equals(ITM? x, ITM? y) => ReferenceEquals(x, y);

      public int GetHashCode(ITM obj) => obj?.GetHashCode() ?? -1;
   }
}
