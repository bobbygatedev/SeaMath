using Gate.Tools.Extensions;
using static Gate.Tools.Text.TxtLineComparer.SectionType;

namespace Gate.Tools.Text
{
   /// <summary>
   /// 
   /// </summary>
   public class TxtLineComparer : HierarchicalItem
   {
      public TxtLineComparer(Func<string, string, bool> lineEqualityComparer) => LineEqualityComparer = lineEqualityComparer;

      public TxtLineComparer(bool isCaseSensitive = true) : this((s1, s2) => string.Compare(s1, s2, !isCaseSensitive) == 0) { }

      public class SectionType : HierarchicalItem, IComparable
      {
         public enum TypeEnum
         {
            insert_only = 0,
            delete_only = 1,
            replace = 2,
            equal = 3,
         }

         public SectionType(Interval lineIntervalOld, Interval lineIntervalNew, bool isEquality)
         {
            LineIntervalOld0 = lineIntervalOld;
            LineIntervalNew0 = lineIntervalNew;

            if (isEquality)
            {
               Type = TypeEnum.equal;
            }
            else
            {
               if (lineIntervalOld.Length > 0)
               {
                  Type = lineIntervalNew.Length > 0 ? TypeEnum.replace : TypeEnum.delete_only;
               }
               else
               {
                  Type = lineIntervalNew.Length > 0 ? TypeEnum.insert_only : throw new Crash();
               }
            }
         }

         public TypeEnum Type { get; set; }

         public TxtLineComparer TextComparer => ParentItem as TxtLineComparer ?? throw new NullReferenceException();

         public Interval LineIntervalOld0 { get; }

         public Interval LineIntervalOld1 => LineIntervalOld0.GetPlus1();

         public Interval LineIntervalNew0 { get; }

         public Interval LineIntervalNew1 => LineIntervalNew0.GetPlus1();

         public string DescriptorPlus1 =>
            $"{Type} Section old {LineIntervalOld0.GetPlus1()} new {LineIntervalNew0.GetPlus1()}";

         public string[] LinesNew =>
            (TextComparer.FileLinesNew ?? []).
            Skip(LineIntervalNew0.From).
            Take(LineIntervalNew0.Length).ToArray();

         public string[] LinesOld =>
            (TextComparer.FileLinesOld ?? []).
            Skip(LineIntervalOld0.From).
            Take(LineIntervalOld0.Length).ToArray();

         public int CompareTo(object? obj) =>
            obj is SectionType s ? LineIntervalOld0.From.CompareTo(s.LineIntervalOld0.From) : -1;

         public override string ToString() => DescriptorPlus1;
      }

      public SectionType[] Sections => SubItems.OfType<SectionType>().ToArray();

      /// <summary>
      ///  
      /// </summary>
      public string? OldFile { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public string[]? FileLinesOld { get; private set; }

      /// <summary>
      ///  
      /// </summary>
      public string? NewFile { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public string[]? FileLinesNew { get; private set; }

      /// <summary>
      ///  
      /// </summary>
      public Func<string, string, bool> LineEqualityComparer { get; }

      public bool AreIdentical
      {
         get
         {
            if (OldFile != null && NewFile != null)
            {
               return Sections.Length == 1 && Sections[0].Type == TypeEnum.equal;
            }
            else
            {
               throw new ArgumentNullException("Null Old and New file!");
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="oldText"></param>
      /// <param name="newText"></param>
      public void Compare(string oldText, string newText)
      {
         myRemoveSubItemRange(SubItems);
         OldFile = oldText;
         FileLinesOld = oldText.SplitLines();
         NewFile = newText;
         FileLinesNew = newText.SplitLines();
         myMakeEqualityIntervals();
         myMakeNotEqualSections();

         var lst_scs = Sections.ToList();

         myRemoveSubItemRange(lst_scs);
         myAddSubItemRange(lst_scs.OrderBy(s => s));
      }

      private void myMakeEqualityIntervals()
      {
         var len_old = FileLinesOld?.Length ?? 0;
         var len_new = FileLinesNew?.Length ?? 0;

         // Hash → index list in new text
         var map = new Dictionary<string, List<int>>(StringComparer.Ordinal);
         var nfl = FileLinesNew.NnOrCrash();
         var ofl = FileLinesOld.NnOrCrash();

         for (var i = 0; i < len_new; i++)
         {
            if (!map.TryGetValue(nfl[i], out var lst))
            {
               lst = new List<int>();
               map[nfl[i]] = lst;
            }

            lst.Add(i);
         }

         var old_idx = 0;
         var new_idx = 0;

         while (old_idx < len_old && new_idx < len_new)
         {
            var ln = ofl[old_idx];

            //find new positions
            if (map.TryGetValue(ln, out var pss))
            {
               // Finds nearest position in new text
               var bst = -1;
               var bst_len = 0;

               foreach (int pos in pss)
               {
                  var len = 0;
                  var oi = old_idx;
                  var ni = pos;

                  while (oi < len_old && ni < len_new && LineEqualityComparer(ofl[oi], nfl[ni]))
                  {
                     oi++;
                     ni++;
                     len++;
                  }

                  if (len > bst_len)
                  {
                     bst_len = len;
                     bst = pos;
                  }
               }

               if (bst_len > 0)
               {
                  myAddSubItem(new SectionType(
                     Interval.FromFromLen(old_idx, bst_len),
                     Interval.FromFromLen(bst, bst_len),
                     true));
                  old_idx += bst_len;
                  new_idx = bst + bst_len;
                  continue;
               }
            }

            old_idx++;
            new_idx++;
         }

         var lst_se = SubItems.OfType<SectionType>().ToList();

         for (var i = 1; i < lst_se.Count; i++)
         {
            var se_m_1 = lst_se[i - 1];
            var se = lst_se[i];

            if (
               se.LineIntervalNew0.GetIntersection(se_m_1.LineIntervalNew0).HasValue ||
               se.LineIntervalOld0.GetIntersection(se_m_1.LineIntervalOld0).HasValue)
            {
               //to be remove
               var ses = new[] { se_m_1, se }.
                  OrderBy(s => s.LineIntervalNew0.Length).
                  FirstOrDefault().NnOrCrash();

               lst_se.Remove(ses);
               i--; //nullate effect of i++
            }
         }

         lst_se = lst_se.OrderBy(s => s.LineIntervalOld0.From).ToList();

         var i0 = lst_se.FirstOrDefault().NnOrCrash();
         var idx = 1;

         //delete items not in order by LineIntervalNew
         while (true)
         {
            var i1 = lst_se.ElementAtOrDefault(idx);

            if (i1 == null)
            {
               break;
            }
            else if(i1.LineIntervalNew1.From <= i0.LineIntervalNew1.From)
            {
               lst_se.Remove(i1);
            }
            else
            {
               i0 = i1;
               idx++;
            }
         }

         myRemoveSubItemRange(Sections);
         myAddSubItemRange(lst_se);
      }

      private void myMakeNotEqualSections()
      {
         var equ_scs = Sections;
         var lns_old = FileLinesOld ?? [];
         var lns_new = FileLinesNew ?? [];

         //if file are completely different operation consist in 'delete all old lines' 'insert all new lines'
         if (equ_scs.Length == 0)
         {
            var old_li = Interval.FromFromLen(0, lns_old.Length);
            var new_li = Interval.FromFromLen(0, lns_new.Length);

            if (new_li.Length > 0 || old_li.Length > 0)
            {
               myAddSubItem(new SectionType(old_li, new_li, false));
            }
         }
         else
         {
            var equ_seq_sta = equ_scs.FirstOrDefault().NnOrCrash();
            var equ_seq_end = equ_scs.LastOrDefault().NnOrCrash();

            if (equ_seq_sta.LineIntervalOld0.From > 0 || equ_seq_sta.LineIntervalNew0.From > 0)
            {
               myAddSubItem(new SectionType(
                  Interval.FromFromLen(0, equ_seq_sta.LineIntervalOld0.From),
                  Interval.FromFromLen(0, equ_seq_sta.LineIntervalNew0.From),
                  false));
            }

            for (var i = 0; i < equ_scs.Length - 1; i++)
            {
               var equ_seq = equ_scs[i];
               var equ_seq_nxt = equ_scs[i + 1];

               var i_old = new Interval(equ_seq.LineIntervalOld0.To + 1, equ_seq_nxt.LineIntervalOld0.From - 1);
               var i_new = new Interval(equ_seq.LineIntervalNew0.To + 1, equ_seq_nxt.LineIntervalNew0.From - 1);

               myAddSubItem(new SectionType(i_old, i_new, false));
            }

            //last sector old
            var lst_sec_old = new Interval(equ_seq_end.LineIntervalOld0.To + 1, lns_old.Length - 1);

            //last sector new 
            var lst_sec_new = new Interval(equ_seq_end.LineIntervalNew0.To + 1, lns_new.Length - 1);

            if (lst_sec_old.Length > 0 || lst_sec_new.Length > 0)
            {
               myAddSubItem(new SectionType(lst_sec_old, lst_sec_new, false));
            }
         }
      }

      public static void Test()
      {
         var old = new TxtStore();
         var nef = new TxtStore();

         old.Content = "xx\r\n2\r\n\r\n3\r\n14\r\n\r\n15\r\n6\r\n7\r\n";
         nef.Content = "xx\r\n2\r\nxx\r\n3\r\n14\r\n\r\n15\r\n6\r\n7";

         //old.AddLines("1", "2", "3", "4", "5");
         //nef.AddLines("1", "2", "11", "12", "13", "5");

         //old.AddLines("1", "2", "3", "4", "5");
         //nef.AddLines("11", "12", "2", "13", "3", "15");

         Console.WriteLine("Old:");
         Console.WriteLine(old.ContentWithLnNumber);

         Console.WriteLine("New:");
         Console.WriteLine(nef.ContentWithLnNumber);

         var cmp = new TxtLineComparer((s1, s2) => s1 == s2);

         cmp.Compare(old.Content, nef.Content);

         //check
         var nef_2 = new TxtStore();

         nef_2.AddLines(old.Lines.Select(l => l.Content).ToArray());

         var scs = cmp.Sections;

         foreach (var ope in scs)
         {
            Console.WriteLine(ope.DescriptorPlus1);
         }

         //scs = scs.OrderBy(o => o.LineIdx0).ToArray();

         foreach (var sec in scs.Where(s => s.Type != SectionType.TypeEnum.equal))
         {
            if (sec.LineIntervalOld0.Length > 0)
            {
               nef_2.RemoveLines(sec.LineIntervalOld0);
            }

            if (sec.LineIntervalOld0.Length > 0)
            {
               nef_2.RemoveLines(Interval.FromFromLen(sec.LineIntervalNew0.From, sec.LineIntervalOld0.Length));
            }

            if (sec.LineIntervalNew0.Length > 0)
            {
               nef_2.InsertLines(sec.LineIntervalNew0.From + 1, sec.LinesNew);
            }

            Console.WriteLine($"After operation {sec}\n{nef_2.ContentWithLnNumber}");
         }

         foreach (var eqi in cmp.Sections.Where(s => s.Type == SectionType.TypeEnum.equal))
         {
            Console.WriteLine(eqi.DescriptorPlus1);
         }

         Console.WriteLine("New rebuilt:");
         Console.WriteLine(nef_2.ContentWithLnNumber);

         var res = nef_2.Content == nef.Content;

         Console.WriteLine($"Comparison result: {(res ? "PASS" : "FAIL")}");
      }
   }
}
