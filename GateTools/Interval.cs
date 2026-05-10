namespace Gate.Tools
{
   /// <summary>
   /// 
   /// </summary>
   public struct Interval : IEquatable<Interval>
   {
      public static Interval Empty = new Interval(0, -2);

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="from"></param>
      /// <param name="to"></param>
      public Interval(int from, int to)
      {
         From = from;
         To = to;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="from"></param>
      /// <param name="length"></param>
      /// <returns></returns>
      public static Interval FromFromLen(int from, int length) => new Interval(from, from + length - 1);

      /// <summary>
      /// Returns a Interval from certain idx of length 0 (remark not <seealso cref="Empty"/>) ie [idx,idx-1]
      /// </summary>
      /// <param name="atIdx"></param>
      /// <returns></returns>
      public static Interval MakeAsMarker(int atIdx) => new Interval(atIdx, atIdx - 1);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="string"></param>
      public static implicit operator Interval(string @string) => new Interval(0, (@string ?? "").Length - 1);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="fromTo"></param>
      public static implicit operator Interval((int, int) fromTo) => new Interval(fromTo.Item1, fromTo.Item2);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="interval"></param>
      public static implicit operator IntervalGroup(Interval interval) => IntervalGroup.Make(interval);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="i1"></param>
      /// <param name="i2"></param>
      /// <returns></returns>
      public static IntervalGroup operator |(Interval i1, Interval i2) => IntervalGroup.Make(i1, i2);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="i1"></param>
      /// <param name="i2"></param>
      /// <returns></returns>
      public static bool operator ==(Interval i1, Interval i2) => i1.Equals(i2);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="i1"></param>
      /// <param name="i2"></param>
      /// <returns></returns>
      public static bool operator !=(Interval i1, Interval i2) => !i1.Equals(i2);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="i1"></param>
      /// <param name="i2"></param>
      /// <returns></returns>
      public static Interval? operator &(Interval? i1, Interval? i2) => i1.HasValue && i2.HasValue ? i1.Value.GetIntersection(i2.Value) : null;

      /// <summary>
      /// Starting idx.
      /// </summary>
      public int From { get; }

      /// <summary>
      /// End idx (included).
      /// </summary>
      public int To { get; }

      /// <summary>
      /// Interval length (To - From + 1).
      /// </summary>
      public int Length => To - From + 1;

      /// <summary>
      /// Int array = {From,To}
      /// </summary>
      public int[] Items => new int[] { From, To };

      /// <summary>
      /// Range array {From, From + 1 , .. , To -1 , To }
      /// </summary>
      public int[] Range => Length > 0 ? Enumerable.Range(From, Length).ToArray() : (new int[0]);

      /// <summary>
      /// Whether empty <seealso cref="Length"/>less than 0
      /// </summary>
      public bool IsEmpty => Length < 0;

      /// <summary>
      /// Hex representation of the interval in the form "From-To" where From and To are represented in hexadecimal format.
      /// </summary>
      public string HexRepresentation => $"{From:X8}-{To:X8} Len:{((UniversalInt)Length).LenRepresentation}";

      /// <summary>
      /// 
      /// </summary>
      /// <param name="other"></param>
      /// <returns></returns>
      public Interval? GetIntersection(Interval other)
      {
         var max_fro = Math.Max(From, other.From);
         var min_to = Math.Min(To, other.To);

         return max_fro <= min_to ? new Interval(max_fro, min_to) : (Interval?)null;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="other"></param>
      /// <returns></returns>
      public Interval? GetUnion(Interval other)
      {
         if (IsEmpty && !other.IsEmpty) { return other; }
         else if (other.IsEmpty && !IsEmpty) { return this; }
         else
         {
            return GetIntersection(other).HasValue || IsContigous(other) ? new Interval(Math.Min(From, other.From), Math.Max(To, other.To)) : (Interval?)null;
         }
      }

      /// <summary>
      /// Splits into lef/rigth intervals l=(From,Idx-1) r=(Idx,To)
      /// </summary>
      /// <param name="atIndex"></param>
      /// <returns></returns>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public (Interval left, Interval right) Split(int atIndex) =>
         Contains(atIndex) || atIndex == To + 1 ?
            ((Interval left, Interval right))(new Interval(From, atIndex - 1), new Interval(atIndex, To)) :
            throw new Gate.Tools.ToolsException($"{atIndex} outside({this})");

      public bool Contains(int idx) => idx >= From && idx <= To;

      public bool Equals(Interval other) => From == other.From && To == other.To;

      public override bool Equals(object? obj) => obj is Interval i && i.Equals(this);

      public override int GetHashCode()
      {
         var hashCode = -1781160927;

         hashCode = hashCode * -1521134295 + From.GetHashCode();
         hashCode = hashCode * -1521134295 + To.GetHashCode();

         return hashCode;
      }

      public static bool AreContigous(bool isLeft2RightToCheck, IEnumerable<Interval> intervals) => AreContigous(isLeft2RightToCheck, intervals.ToArray());

      public string GetString(string @string) => IsEmpty ? "" : @string.Substring(From, Length);

      public static bool AreContigous(bool isLeft2RightToCheck, params Interval[] intervals)
      {
         var arr = intervals.ToArray();

         for (int i = 0; i < arr.Length - 1; i++)
         {
            if (isLeft2RightToCheck && i == 0 && arr[0].From >= arr[1].From)
            {
               return false;
            }

            if (!arr[i].IsContigous(arr[i + 1])) { return false; }
         }

         return true;
      }

      public bool IsContigous(Interval other) => To == other.From - 1 || other.To == From - 1;

      public bool IsContainedIn(Interval other) => From >= other.From && To <= other.To;

      public bool Contains(Interval other) => other.IsContainedIn(this);

      /// <summary>
      /// <br> Joins an array of intervals in this way </br>
      /// <br> - if all intervals have interval each other ie interals are not contigous (eg (0-3) (5-3)) return null </br>
      /// <br> - otherwise returns (min(intervals.From),max(intervals.To). </br>
      /// <br> - empty intervals are discarded </br>
      /// </summary>
      /// <param name="intervals"></param>
      /// <returns></returns>
      public static Interval? Union(params Interval[] intervals)
      {
         if (intervals.Length > 0)
         {
            var int_ord = intervals.OrderBy(i => i.From).ToArray();
            var res = (Interval?)intervals[0];

            foreach (var ins in intervals.Skip(1))
            {
               res = ins.GetUnion(intervals[0]);

               if (!res.HasValue) { return null; }
            }

            return res;
         }
         else { return null; }
      }

      public static Interval? Intersection(params Interval[] intervals)
      {
         if (intervals.Length > 0)
         {
            var int_ord = intervals.OrderBy(i => i.From).ToArray();
            var res = (Interval?)int_ord[0];

            foreach (var ins in intervals.Skip(1))
            {
               res = ins.GetIntersection(ins);

               if (!res.HasValue) { return null; }
            }

            return res;
         }
         else { return null; }
      }

      public override string ToString() => IsEmpty ? "Empty" : $"{From}-{To}";
   }
}
