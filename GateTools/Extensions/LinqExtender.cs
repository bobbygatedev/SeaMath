namespace Gate.Tools.Extensions
{
   public static class LinqExtender
   {
      public static TSource? MinOrDefault<TSource>(this IEnumerable<TSource> source, TSource? defaultValue = default) =>
         source.GetEnumerator().MoveNext() ? source.Min() : defaultValue;

      public static TSource? MaxOrDefault<TSource>(this IEnumerable<TSource> source, TSource? defaultValue = default) => source.GetEnumerator().MoveNext() ? source.Max() : defaultValue;

      public static TResult? MinOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, TResult? defaultValue = default) =>
         source.GetEnumerator().MoveNext() ? source.Min(selector) : defaultValue;

      public static TResult? MaxOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, TResult? defaultValue = default) =>
         source.GetEnumerator().MoveNext() ? source.Max(selector) : defaultValue;

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="TSource"></typeparam>
      /// <param name="source"></param>
      /// <param name="separator"></param>
      /// <param name="toStringFunction"></param>
      /// <returns></returns>
      public static string ToStringExt<TSource>(
         this IEnumerable<TSource> source, string separator = ",", Func<TSource?, string?>? toStringFunction = null)
      {
         Func<TSource?, string> to_s_alt = s => s?.ToString() ?? string.Empty;
         var to_s = toStringFunction ?? to_s_alt;

         return string.Join(separator, source.Select(i => to_s(i)));
      }

      public static List<TSource[]> GroupByNumber<TSource>(this IEnumerable<TSource> source, int number)
      {
         var enr = source.GetEnumerator();
         var lst = new List<TSource>();
         var lst_res = new List<TSource[]>();

         while (enr.MoveNext())
         {
            lst.Add(enr.Current);

            if (lst.Count == number)
            {
               lst_res.Add(lst.ToArray());
               lst.Clear();
            }
         }

         return lst_res;
      }


      /// <summary>
      /// <br>Returns maximum one result from <paramref name="source"/>: </br>
      /// <br>if <paramref name="source"/>/<paramref name="predicate"/> pair has more than 1 item throws <see cref="Gate.Tools.ToolsException"/></br>
      /// <br>If source is empty returns null</br>
      /// <br>If source has one item returns it</br>
      /// </summary>
      /// <typeparam name="TSource"></typeparam>
      /// <param name="source"></param>
      /// <param name="predicate"></param>
      /// <returns></returns>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public static TSource? FirstOrDefaultUnique<TSource>(this IEnumerable<TSource> source, Func<TSource, bool>? predicate = null)
      {
         Func<TSource, bool> def_pre = (_) => true;

         var col = source.Where(predicate ?? def_pre);
         var res = col.FirstOrDefault();

         if (col != null)
         {
            return !col.Skip(1).GetEnumerator().MoveNext() ?
               res :
               throw new Gate.Tools.ToolsException("Just a value is allowed for FirstUnique");
         }
         else
         {
            return default(TSource);
         }
      }

      public static TSource FirstUnique<TSource>(this IEnumerable<TSource> source, Func<TSource, bool>? predicate = null)
      {
         Func<TSource, bool> def_pre = (_) => true;

         var col = source.Where(predicate ?? def_pre);
         var res = col.First();

         return !col.Skip(1).GetEnumerator().MoveNext() ?
            res :
            throw new Gate.Tools.ToolsException("Just a value is allowed for FirstUnique");
      }

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="TSource"></typeparam>
      /// <param name="source"></param>
      /// <returns></returns>
      public static IEnumerable<TSource> Nn<TSource>(this IEnumerable<TSource?> source) where TSource : class => 
         source.Where(t => t != null).Cast<TSource>().ToArray();

      public static IEnumerable<TResult> SelectNotNull<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) =>
         source.Select(selector).Where(t => t != null);
   }
}
