namespace Gate.Tools
{
   /// <summary>
   /// 
   /// </summary>
   public struct IntervalGroup
   {
      private IntervalGroup(Interval[] intervals) => Intervals = intervals;

      public static IntervalGroup Make(params Interval[] intervals)
      {
         var ord = intervals.OrderBy(i => i.From).ToArray();

         if (ord.Length > 0)
         {
            var cur = (Interval?)ord.First();
            var lst_itn = new List<Interval>();

            foreach (var itn in ord.Skip(1))
            {
               if (cur.HasValue)
               {
                  var ins = itn.GetUnion(cur.Value);

                  if (ins.HasValue) { cur = ins; }
                  else
                  {
                     lst_itn.Add(cur.Value);
                     cur = itn;
                  }
               }
               else { cur = itn; }
            }

            if (cur.HasValue) { lst_itn.Add(cur.Value); }

            return new IntervalGroup(lst_itn.ToArray());
         }
         else { return new IntervalGroup(); }
      }

      public static IntervalGroup operator |(IntervalGroup g1, IntervalGroup g2) => Make(g1.Intervals.Concat(g2.Intervals).ToArray());

      public Interval[] Intervals { get; }

      public override string ToString() => "[" + string.Join(",", Intervals.Select(i => $"({i})")) + "]";
   }
}
