using System.Text;

namespace Gate.Tools.Binary
{
   public class BinComparer
   {
      public int SectorLen = 0x1000;

      public class DescriptorType
      {
         public DescriptorType(BinComparer comparer, bool[] comparition)
         {
            Comparer = comparer;
            Comparition = comparition;

            EqualityIntervals = myGetEqualityIntervals();
            NotEqualityIntervals = myGetNotEqualityIntervals();
         }

         public struct Interval
         {
            public Interval(UInt64 from, UInt64 to)
            {
               From = from;
               To = to;
            }

            public static Interval FromFromLen(UInt64 from, UInt64 len) => new Interval(from, from + len);

            /// <summary>
            /// 
            /// </summary>
            public UInt64 Length => To - From;

            /// <summary>
            /// First Address
            /// </summary>
            public UInt64 From { get; }

            /// <summary>
            /// First invalid address
            /// </summary>
            public UInt64 To { get; }

            public string FromHex => $"0x{From:x}";

            public string ToHex => $"0x{To:x}";

            public string LenString => $"0x{Length:x}({myGetLen(Length)})";

            private object myGetLen(ulong length)
            {
               if (length < 1024) { return $"{length}B"; }
               else if (length < 1024 * 1024) { return $"{length / 1024}KB"; }
               else if (length < 1024 * 1024 * 1024) { return $"{length / (1024 * 1024)}MB"; }
               else { return $"{length / (1024 * 1024 * 1024)}GB"; }
            }

            public string Descriptor { get { return $"{FromHex}-{ToHex}({LenString})"; } }

            public override string ToString() => Descriptor;
         }

         public Interval[] EqualityIntervals { get; }
         public Interval[] NotEqualityIntervals { get; }

         public bool EqualFull => Comparition.All(c => c);

         public string Report
         {
            get
            {
               var sb = new StringBuilder();

               sb.AppendLine("Equals:");

               foreach (var eq in EqualityIntervals)
               {
                  sb.AppendLine(eq.Descriptor);
               }

               sb.AppendLine("NOT Equals:");

               foreach (var eq in NotEqualityIntervals)
               {
                  sb.AppendLine(eq.Descriptor);
               }

               return sb.ToString();
            }
         }

         public BinComparer Comparer { get; }

         public bool[] Comparition { get; }

         public override string ToString() => Report;

         private static (int from, int to)[] myGetSequence(bool[] vector)
         {
            if (vector == null || vector.Length == 0)
               return Array.Empty<(int from, int to)>();

            var lst = new List<(int from, int to)>();
            var sta = -1;

            for (int i = 0; i < vector.Length; i++)
            {
               if (vector[i])
               {
                  // Inizio di una nuova sequenza
                  if (sta == -1)
                  {
                     sta = i;
                  }
               }
               else
               {
                  // Fine di una sequenza
                  if (sta != -1)
                  {
                     lst.Add((sta, i - 1));
                     sta = -1;
                  }
               }
            }

            // Se finisce con una sequenza aperta
            if (sta != -1)
            {
               lst.Add((sta, vector.Length - 1));
            }

            return lst.ToArray();
         }

         private Interval[] myGetNotEqualityIntervals()
         {
            var seq = myGetSequence(Comparition.Select(c => !c).ToArray());

            return seq.Select(
               i => new Interval(
                  (ulong)(i.from * Comparer.SectorLen),
                  (ulong)((i.to + 1) * Comparer.SectorLen))).ToArray();
         }

         private Interval[] myGetEqualityIntervals()
         {
            var seq = myGetSequence(Comparition);

            return seq.Select(
               i => new Interval(
                  (ulong)(i.from * Comparer.SectorLen),
                  (ulong)((i.to + 1) * Comparer.SectorLen))).ToArray();
         }
      }

      public byte[]? Data1 { get; private set; }
      public byte[]? Data2 { get; private set; }

      public DescriptorType? LastDescriptor { get; private set; }
      public string? LastPath1 { get; private set; }
      public string? LastPath2 { get; private set; }
      public int NSectors { get; private set; }

      public DescriptorType Compare(string path1, string path2) =>
         Compare(File.ReadAllBytes(LastPath1 = path1), File.ReadAllBytes(LastPath2 = path2));

      public DescriptorType Compare(byte[] data1, byte[] data2)
      {
         Data1 = data1;
         Data2 = data2;

         if (Data1.Length == Data2.Length)
         {
            if (Data1.Length % (int)SectorLen == 0)
            {
               NSectors = Data1.Length / (int)SectorLen;

               var lst = new List<bool>();

               for (int i = 0; i < NSectors; i++)
               {
                  lst.Add(myCompareSector(i));
               }

               return LastDescriptor = new DescriptorType(this, lst.ToArray());
            }
            else
            {
               throw new Exception($"Not multiple of {SectorLen}!");
            }
         }
         else
         {
            throw new Exception("Not of equal size!");
         }
      }

      public static int[] Search(byte[] file, byte[] dataToSearch)
      {
         var lst = new List<int>();

         var fileSpan = new ReadOnlySpan<byte>(file);
         var dataToSearchSpan = new ReadOnlySpan<byte>(dataToSearch);

         var off = 0;

         while (true)
         {
            var pos = fileSpan.IndexOf(dataToSearchSpan);

            if (pos < 0)
            {
               break;
            }

            pos += off;

            lst.Add(pos);

            pos += dataToSearchSpan.Length;

            off = pos;

            if (off >= file.Length)
            {
               break;
            }

            fileSpan = file.AsSpan(off);
         }

         return lst.ToArray();
      }

      private bool myCompareSector(int sectorIdx) =>
         (Data1 ?? []).Skip(sectorIdx * SectorLen).
         Take(SectorLen).
         SequenceEqual((Data2 ?? []).Skip(sectorIdx * SectorLen).Take(SectorLen));

   }
}
