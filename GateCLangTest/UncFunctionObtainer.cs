using Gate.CLanguage.Standards;
using Gate.Tools;
using Gate.Tools.Extensions;
using System.Globalization;

namespace Gate.CLanguageTest
{
   /// <summary>
   /// 
   /// </summary>
   public class UncFunctionObtainer
   {
      private delegate IntPtr FuncDelegate();
      private delegate uint DelTyp();

      private readonly Dictionary<uint, uint> myDictionary = new Dictionary<uint, uint>();

      public UncFunctionObtainer() { }

      private List<byte[]>? myHexDigits;
      private List<(byte val, int len)[]>? myHexDigitsSequences;

      private uint[] mySelect(uint from, uint to, uint step)
      {
         var lst = new List<uint>();
         var fro = from;

         if (from < CharStandard.Unc.MIN_VALUE_UCS)
         {
            for (; fro <= Math.Min(to, CharStandard.Unc.MIN_VALUE_UCS - 1); fro += step)
            {
               if (CharStandard.Unc.ValidLowValues.Contains(fro)) { lst.Add(fro); }
            }
         }

         var toa = Math.Min(CharStandard.Unc.MAX_VALUE_UCS, to);

         for (var i = fro; i <= toa; i += step)
         {
            if (!CharStandard.Unc.ExceptRange.Contains((int)i)) { lst.Add(i); }
         }

         return lst.ToArray();
      }

      public List<byte[]> HexDigits
      {
         get
         {
            if (myHexDigits == null)
            {
               myHexDigits = new List<byte[]>();
               var len = myDictionary.Count;

               for (int i = 0; i < 8; i++)
               {
                  myHexDigits.Add(new byte[len]);
               }

               var k = 0;

               foreach (var val in myDictionary.Values)
               {
                  for (int i = 0; i < 8; i++)
                  {
                     myHexDigits[i][k] = (byte)((val & (0xf << (i * 4))) >> (i * 4));
                  }

                  k++;
               }
            }

            return myHexDigits;
         }
      }

      private StreamWriter myGetStreamWriter()
      {
         return myGetStreamWriter(TestSource.FullName);
      }

      public void Clear()
      {
         myDictionary.Clear();
         myNullateVars();
      }

      private static StreamWriter myGetStreamWriter(string fileName)
      {
         for (var i = 0; i < 5; i++)
         {
            try { return new StreamWriter(fileName); }
            catch (Exception)
            {
               if (i >= 4) { throw; }
               else { Thread.Sleep(100); }
            }
         }

         throw new Crash();
      }
      public unsafe static uint GetCompositionVcc(string charBody)
      {
         var cmp = new CompilerVcc();
         var fil_nam = @"c:\temp\tstvcc.c";

         using (var fil = myGetStreamWriter(fil_nam))
         {
            fil.WriteLine("#include <stdio.h>");
            fil.WriteLine("");

            fil.WriteLine($"unsigned int val = '{charBody}';");

            fil.WriteLine("__declspec(dllexport) unsigned int get_val(void)");
            fil.WriteLine("{");

            fil.WriteLine("   return val;");

            fil.WriteLine("}");
            fil.WriteLine("");
         }

         cmp.AreWarningEnabled = false;
         cmp.IsDll = true;
         cmp.OutputName = @"c:\temp\tstvcc.dll";

         if (cmp.Compile(fil_nam))
         {
            using (var dll_ist = new DllInstance(cmp.OutputName))
            {
               var prc_hnd = DllInstance.GetProcAddress(dll_ist.Handle, "get_val");

               var res = dll_ist.GetDelegate<DelTyp>("get_val")();

               return res;
            }
         }
         else
         {
            throw new Gate.Tools.ToolsException($"Failed compile");
         }
      }


      public unsafe static uint GetCompositionGcc(string charBody)
      {
         var cmp = new CompilerGcc();
         var fil_nam = @"c:\temp\tstgcc.c";

         using (var fil = myGetStreamWriter(fil_nam))
         {
            fil.WriteLine("#include <stdio.h>");
            fil.WriteLine("#pragma GCC diagnostic push");
            fil.WriteLine("#pragma GCC diagnostic ignored \"-Wmultichar\"");
            fil.WriteLine("");

            fil.WriteLine($"unsigned int val = '{charBody}';");

            fil.WriteLine("__declspec(dllexport) unsigned int get_val(void)");
            fil.WriteLine("{");

            fil.WriteLine("   return val;");

            fil.WriteLine("}");
            fil.WriteLine("");
         }

         cmp.AreWarningEnabled = false;
         cmp.IsDll = true;
         cmp.OutputName = @"c:\temp\tstgcc.dll";

         if (cmp.Compile(fil_nam))
         {
            using (var dll_ist = new DllInstance(cmp.OutputName))
            {
               var prc_hnd = DllInstance.GetProcAddress(dll_ist.Handle, "get_val");

               var res = dll_ist.GetDelegate<DelTyp>("get_val")();

               return res;
            }
         }
         else
         {
            throw new Gate.Tools.ToolsException($"Failed compile");
         }
      }


      public unsafe void CalculateDll(uint from, uint to, uint step)
      {
         var cmp = new CompilerGcc();
         var rng = mySelect(from, to, step);

         myNullateVars();

         using (var fil = myGetStreamWriter())
         {
            fil.WriteLine("#include <stdio.h>");
            fil.WriteLine("#pragma GCC diagnostic push");
            fil.WriteLine("#pragma GCC diagnostic ignored \"-Wmultichar\"");
            fil.WriteLine("");

            fil.WriteLine("unsigned int arr[] = {");

            foreach (var i in rng)
            {
               var ch = $"\\U{i:x8}";

               fil.WriteLine($"'{ch}',");
            }

            fil.WriteLine("};");


            fil.WriteLine("__declspec(dllexport) unsigned int* get_vector(void)");
            fil.WriteLine("{");

            fil.WriteLine("   return arr;");

            fil.WriteLine("}");
            fil.WriteLine("");
         }

         cmp.AreWarningEnabled = false;
         cmp.IsDll = true;

         if (cmp.Compile(TestSource.FullName))
         {
            using (var lib = DllInstance.LoadLibrary(TestDll.FullName))
            {
               var del = DllInstance.GetDelegate<FuncDelegate>("get_vector", lib);

               var res = del();

               var ptr = (IntPtr)res;
               var u32p = (UInt32*)ptr;

               for (var i = 0u; i < rng.Length; i++)
               {
                  myDictionary[rng[i]] = u32p[i];
               }
            }
         }
         else
         {
            throw new Gate.Tools.ToolsException($"Failed compile for 0x{from:x8},0x{to:x8},{step}");
         }
      }


      public void Calculate(uint from, uint to, uint step)
      {
         var cmp = new CompilerGcc();
         var rng = mySelect(from, to, step);

         myNullateVars();

         using (var fil = myGetStreamWriter())
         {
            fil.WriteLine("#include <stdio.h>");
            fil.WriteLine("#pragma GCC diagnostic push");
            fil.WriteLine("#pragma GCC diagnostic ignored \"-Wmultichar\"");
            fil.WriteLine("");
            fil.WriteLine("int main(void)");
            fil.WriteLine("{");

            foreach (var i in rng)
            {
               var ch = $"\\U{i:x8}";

               fil.WriteLine($"printf(\"%8.8x\\n\" , (unsigned int)'{ch}' );");
            }

            fil.WriteLine("}");
            fil.WriteLine("");
         }

         cmp.AreWarningEnabled = false;

         if (cmp.Compile(TestSource.FullName))
         {
            var rdb = cmp.ExecuteAndReadAuto(TestExe.FullName);

            using (var sr = new StringReader(rdb))
            {
               for (var i = 0u; i < rng.Length; i++)
               {
                  var ln = sr.ReadLine();

                  myDictionary[rng[i]] = uint.Parse(ln??"", NumberStyles.HexNumber);
               }
            }
         }
         else
         {
            throw new Gate.Tools.ToolsException($"Failed compile for 0x{from:x8},0x{to:x8},{step}");
         }
      }

      private void myNullateVars()
      {
         myHexDigits = null;
         myHexDigitsSequences = null;
      }

      public DirectoryInfo TestDir { get; set; } = new DirectoryInfo(@"c:\test");

      public FileInfo TestSource => TestDir.GetCombinedToFile("tst.c");
      public FileInfo TestExe => TestDir.GetCombinedToFile("a.exe");

      public FileInfo TestDll => TestDir.GetCombinedToFile("a.dll");

      public uint this[uint index] => myDictionary[index];

      public static (byte val, int len)[] GetSequences(byte[] digits)
      {
         var v = digits[0];
         var sl = 1;
         var lst = new List<(byte val, int len)>();

         foreach (var itm in digits.Skip(1))
         {
            if (itm == v)
            {
               sl++;
            }
            else
            {
               lst.Add((v, sl));
               v = itm;
               sl = 1;
            }
         }

         lst.Add((v, sl));

         return lst.ToArray();
      }

      public List<(byte val, int len)[]> HexDigitsSequences
      {
         get
         {
            if (myHexDigitsSequences == null)
            {
               myHexDigitsSequences = new List<(byte val, int len)[]>();

               foreach (var hex_dig in HexDigits)
               {
                  myHexDigitsSequences.Add(GetSequences(hex_dig));
               }
            }

            return myHexDigitsSequences;
         }
      }

      public static bool IsRollingDigit(byte[] digits)
      {
         var x0 = digits[0];

         foreach (var itm in digits.Skip(1))
         {
            if (itm != (0xf & (x0 + 1)))
            {
               return false;
            }
            else
            {
               x0 = itm;
            }
         }

         return true;
      }

      public static string GetBits(uint value, int groupSize, int numBits = -1)
      {
         var nb = numBits == -1 ? 32 : numBits;
         var bts = Enumerable.Range(0, numBits).Select(i => ((0x1 << i) & value) != 0).ToArray();
         var rb = numBits;
         var lst = new List<string>();

         for (var i = 0; i < nb; i += groupSize, rb -= groupSize)
         {
            var n = Math.Min(groupSize, rb);
            var str = bts.Skip(i).Take(n).Reverse().Select(b => b ? "1" : "0").Aggregate((s1, s2) => s1 + s2);

            lst.Add(str);
         }

         return string.Join(" ", lst.ToArray().Reverse());
      }

      unsafe static void Main(string[] args)
      {
         var clc = new UncFunctionObtainer();

         var dlt = 0x200000u;
         var sta = 0x0u;
         var end = CharStandard.Unc.MAX_VALUE_UCS;
         var ste = 64u;

         //sta = 0x10000;
         //end = 0xfffff;
         //dlt = 0x40000;
         ste = 1;

         for (var off = sta; off <= end; off += dlt)
         {
            var to = off + dlt - 1;

            Console.Write($"Test 0x{off:x8}-0x{to:x8} start ..");

            clc.Clear();
            clc.CalculateDll(off, to, ste);

            var all_ins = clc.myDictionary.Keys.ToArray();
            var all_ous = clc.myDictionary.Values.ToArray();

            var vls_frm = all_ins.Select(i => CharStandard.Unc.GccMultibyteRepresentation(i)).ToArray();

            //var pai = Enumerable.Range(0, all_ins.Length).Select(i => (all_ins[i], all_ous[i], vls_frm[i], all_ous[i] == vls_frm[i])).ToArray();
            var dfs = Enumerable.Range(0, all_ins.Length).Where(i => all_ous[i] != vls_frm[i]).Select(i => (all_ins[i], all_ous[i], vls_frm[i])).ToArray();

            if (dfs.Length > 0)
            {
               Console.WriteLine($"{dfs.Length} errors:");

               var xx = clc.HexDigits[1];

               foreach (var itm in dfs)
               {
                  Console.WriteLine($"Input: 0x{itm.Item1:x8} exp:  0x{itm.Item2:x8} found: 0x{itm.Item3:x8}");
               }
            }
            else
            {
               Console.WriteLine($"ok");
            }
         }
      }
   }
}
