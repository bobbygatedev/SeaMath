namespace Gate.Tools.Text
{
   /// <summary>
   /// 
   /// </summary>
   public class TxtLineComparer
   {
      public TxtLineComparer(Func<string, string, bool> lineEqualityComparer) => LineEqualityComparer = lineEqualityComparer;

      public class Operation
      {
         public enum TypeEnum
         {
            insert_line = 0,
            delete_line
         }

         public Operation(TxtLineComparer textComparer, TypeEnum operationType, int rowIdx, string[] lines)
         {
            TextComparer = textComparer;
            Type = operationType;
            LineIdx = rowIdx;
            Lines = lines.ToArray();
         }

         public TypeEnum Type { get; private set; }

         public int LineIdx { get; set; }
         public int LineCount => Lines.Length;

         public TxtLineComparer TextComparer { get; }
         public string[] Lines { get; private set; }

         public override string ToString() => $"Do {Type} from row {LineIdx} num {LineCount} rows:\n{string.Join("\n", Lines)}";
      }

      public class SequenceEquality
      {
         public SequenceEquality(TxtLineComparer textComparer, int sequenceStartNew, int len, int sequenceStartOld)
         {
            TextComparer = textComparer;
            SequenceStartNew = sequenceStartNew;
            SequenceStartOld = sequenceStartOld;
            Len = len;
         }

         /// <summary>
         ///  
         /// </summary>
         public TxtLineComparer TextComparer { get; }

         /// <summary>
         ///  
         /// </summary>
         public int SequenceStartNew { get; private set; }

         /// <summary>
         ///  
         /// </summary>
         public int SequenceEndNew => SequenceStartNew + Len - 1;

         /// <summary>
         ///  
         /// </summary>
         public int Len { get; private set; }

         /// <summary>
         ///  
         /// </summary>
         public int SequenceStartOld { get; private set; }

         /// <summary>
         ///  
         /// </summary>
         public int SequenceEndOld => SequenceStartOld + Len - 1;

         /// <summary>
         ///  
         /// </summary>
         public TxtStore.LineToken[]? NewLines => TextComparer.NewFile?.Lines.Skip(SequenceStartNew - 1).Take(Len).ToArray();

         /// <summary>
         ///  
         /// </summary>
         public TxtStore.LineToken[]? OldLines => TextComparer.OldFile?.Lines.Skip(SequenceStartOld - 1).Take(Len).ToArray();

         public override string ToString() => $"Old({SequenceStartOld}-{SequenceEndOld}) New({SequenceStartNew}-{SequenceEndNew})";

         /// <summary>
         /// Extend the interval to empty lines(on both old,new) downwards. If isUpAsWell upwards too.
         /// </summary>
         /// <param name="isUpAsWell"></param>
         public void ExtendToEmpty(bool isUpAsWell)
         {
            var i_old = SequenceEndOld + 1;
            var i_new = SequenceEndNew + 1;

            for (; i_old <= TextComparer?.OldFile?.LineCount && i_new <= TextComparer?.NewFile?.LineCount; i_old++, i_new++)
            {
               if (TextComparer.OldFile[i_old].Content.Trim() == "" && TextComparer.NewFile[i_new].Content.Trim() == "") { Len++; }
               else { break; }
            }

            if (isUpAsWell)
            {
               i_old = SequenceStartOld - 1;
               i_new = SequenceStartNew - 1;

               for (; i_new >= 1 && i_old >= 1; i_old--, i_new--)
               {
                  if (TextComparer?.OldFile?[i_old].Content.Trim() == "" && TextComparer?.NewFile?[i_new].Content.Trim() == "")
                  {
                     SequenceStartOld = i_old;
                     SequenceStartNew = i_new;
                     Len++;
                  }
                  else { break; }
               }
            }
         }
      }

      /// <summary>
      ///  
      /// </summary>
      public SequenceEquality[]? EqualityIntervals { get; private set; }

      /// <summary>
      ///  
      /// </summary>
      public Operation[]? Operations { get; private set; }

      /// <summary>
      ///  
      /// </summary>
      public TxtStore? OldFile { get; private set; }

      /// <summary>
      ///  
      /// </summary>
      public TxtStore? NewFile { get; private set; }

      /// <summary>
      ///  
      /// </summary>
      public Func<string, string, bool> LineEqualityComparer { get; }

      public void Compare(string oldText, string newText) => Compare(new TxtStore(oldText), new TxtStore(newText));

      public void Compare(TxtStore oldFile, TxtStore newFile)
      {
         OldFile = oldFile;
         NewFile = newFile;
         EqualityIntervals = myGetEqualityIntervals(OldFile, NewFile);
         Operations = myGetOperations(EqualityIntervals);
      }

      protected virtual bool myCompareLines(TxtStore.LineToken textStoreFileLine1, TxtStore.LineToken textStoreFileLine2) => LineEqualityComparer.Invoke(textStoreFileLine1.Content, textStoreFileLine2.Content);

      private SequenceEquality[] myGetEqualityIntervals(TxtStore oldFile, TxtStore newFile)
      {
         var lst_eq = new List<SequenceEquality>();
         var i_old = 1;

         for (int i_new = 1; i_new <= newFile.LineCount; i_new++)
         {
            var ln_new = newFile[i_new];
            var ln_fnd = oldFile.Lines.Skip(i_old - 1).FirstOrDefault(l => myCompareLines(l, ln_new));

            if (ln_new.Content.Trim() != "" && ln_fnd != null)
            {
               i_old = ln_fnd.LineIdx;

               var seq_sta_new = i_new;
               var seq_end_new = -1;
               var seq_sta_old = i_old;

               while (i_new <= newFile.LineCount && i_old <= oldFile.LineCount && myCompareLines(newFile[i_new], oldFile[i_old]))
               {
                  seq_end_new = i_new;
                  i_new++;
                  i_old++;
               }

               i_new--;//rewinds i_new 

               var len = seq_end_new - seq_sta_new + 1;
               var seq = new SequenceEquality(this, seq_sta_new, len, seq_sta_old);

               seq.ExtendToEmpty(lst_eq.Count == 0);
               lst_eq.Add(seq);
            }
         }

         return lst_eq.ToArray();
      }
      private Operation[] myGetOperations(SequenceEquality[] sequenceEqualities)
      {
         var lst_ope = new List<Operation>();

         if (sequenceEqualities.Length == 0)//if file are completely different operation consist in 'delete all old lines' 'insert all new lines'
         {
            if (OldFile?.LineCount > 0)
            {
               lst_ope.Add(new Operation(this, Operation.TypeEnum.delete_line, 1, OldFile.Lines.Select(l => l.Content).ToArray()));
            }

            if (NewFile?.LineCount > 0)
            {
               lst_ope.Add(new Operation(this, Operation.TypeEnum.insert_line, 1, NewFile.Lines.Select(l => l.Content).ToArray()));
            }
         }
         else
         {
            var cur_idx = 1;
            var seq_sta = sequenceEqualities.FirstOrDefault();
            var seq_end = sequenceEqualities.LastOrDefault();

            if (seq_sta != null)
            {
               var seq_len = seq_sta.SequenceStartOld - 1;
               var old_lns = OldFile?.Lines.Take(seq_len).Select(l => l.Content).ToArray();
               var new_lns = NewFile?.Lines.Take(seq_sta.SequenceStartNew - 1).Select(l => l.Content).ToArray();

               //remove lines not in interval from old beginning 
               if (old_lns?.Length > 0) { lst_ope.Add(new Operation(this, Operation.TypeEnum.delete_line, cur_idx, old_lns)); }

               if (new_lns?.Length > 0) { lst_ope.Add(new Operation(this, Operation.TypeEnum.insert_line, cur_idx, new_lns)); }

               cur_idx += new_lns?.Length ?? 0;

               for (int i = 0; i < sequenceEqualities.Length - 1; i++)
               {
                  var seq = sequenceEqualities[i];
                  var seq_nxt = sequenceEqualities[i + 1];

                  old_lns = OldFile?.Lines.
                     Skip(seq.SequenceEndOld).
                     Take(seq_nxt.SequenceStartOld - seq.SequenceEndOld - 1).
                     Select(l => l.Content).ToArray();
                  new_lns = NewFile?.Lines.
                     Skip(seq.SequenceEndNew).
                     Take(seq_nxt.SequenceStartNew - seq.SequenceEndNew - 1).
                     Select(l => l.Content).ToArray();
                  cur_idx += seq.Len;

                  //remove lines not in interval from old beginning 
                  if (old_lns?.Length > 0) { lst_ope.Add(new Operation(this, Operation.TypeEnum.delete_line, cur_idx, old_lns)); }

                  if (new_lns?.Length > 0) { lst_ope.Add(new Operation(this, Operation.TypeEnum.insert_line, cur_idx, new_lns)); }

                  cur_idx += new_lns?.Length??0;
               }

               cur_idx += seq_end?.Len??0;
               old_lns = OldFile?.Lines.Skip(seq_end?.SequenceEndOld??0).Select(l => l.Content).ToArray();
               new_lns = NewFile?.Lines.Skip(seq_end?.SequenceEndNew??0).Select(l => l.Content).ToArray();

               //remove lines not in interval from old beginning 
               if (old_lns?.Length > 0) { lst_ope.Add(new Operation(this, Operation.TypeEnum.delete_line, cur_idx, old_lns)); }

               if (new_lns?.Length > 0) { lst_ope.Add(new Operation(this, Operation.TypeEnum.insert_line, cur_idx, new_lns)); }
            }
         }

         return lst_ope.ToArray();
      }

      public static void Test()
      {
         var old = new TxtStore();
         var nef = new TxtStore();

         old.AddLines("1", "", "2", "", "4", "5", "", "6");
         nef.AddLines("21", "", "2", "4", "5", "", "");

         var cmp = new TxtLineComparer((s1, s2) => s1 == s2);

         cmp.Compare(old.Content, nef.Content);

         //check
         var nef_2 = new TxtStore();

         nef_2.AddLines(old.Lines.Select(l => l.Content).ToArray());

         foreach (var ope in cmp.Operations ?? [])
         {
            switch (ope.Type)
            {
               case Operation.TypeEnum.insert_line:
                  nef_2.InsertLines(ope.LineIdx, ope.Lines);

                  break;

               case Operation.TypeEnum.delete_line:
                  nef_2.RemoveLines(Interval.FromFromLen(ope.LineIdx, ope.Lines.Length));
                  break;
            }
         }

         foreach (var eqi in cmp.EqualityIntervals ?? [])
         {
            Console.WriteLine(eqi);
         }

         Console.WriteLine("Old:");
         Console.WriteLine(old);

         Console.WriteLine("New:");
         Console.WriteLine(nef);

         Console.WriteLine("New rebuilt:");
         Console.WriteLine(nef_2);
      }
   }
}
