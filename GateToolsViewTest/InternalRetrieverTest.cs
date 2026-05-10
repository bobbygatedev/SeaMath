using Gate.Tools.Extensions;
using Gate.Tools.Text.Encode;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Gate.ToolsViewTest
{
   internal class InternalRetrieverTest
   {
      public InternalRetrieverTest()
      {
      }

      public InternalRetriever InternalRetriever { get; } = new InternalRetriever();

      public Encoding[] Encodings { get; } = new Encoding[] {
         new TxtExtraEncodings.Utf16BomType(),
         new TxtExtraEncodings.Utf16NoBomType(),
         new TxtExtraEncodings.Utf16BomBEType(),
         new TxtExtraEncodings.Utf16NoBomBEType(),
         new TxtExtraEncodings.Utf8BomType(),
         new TxtExtraEncodings.Utf8NoBomType(),
      };

      public string SampleText => "La Figa è bella!";

      public string TestDirectory => $@"c:\temp\{GetType().Name}";

      public void Go()
      {
         Directory.CreateDirectory(TestDirectory);

         var chs = SampleText.ToCharArray();

         foreach (var enc in Encodings)
         {
            var ecr = enc.GetEncoder();

            ecr.Reset();

            Console.WriteLine($"Encoding {enc.BodyName}");

            var bys = enc.GetPreamble().Concat(enc.GetBytes(chs)).ToArray();

            var fil = new DirectoryInfo(TestDirectory).GetCombinedToFile($"{enc.BodyName}.txt");

            File.WriteAllBytes(fil.FullName, bys);

            var rb = InternalRetriever.FromPath(fil.FullName);

            Console.WriteLine($"Re-Read {rb.BodyName} {(enc.BodyName == rb.BodyName ? "PASSED" : "FAIL")}");
         }
      }

      static void Main()
      {
         var rtr = new InternalRetrieverTest();

         rtr.Go();
      }
   }
}
