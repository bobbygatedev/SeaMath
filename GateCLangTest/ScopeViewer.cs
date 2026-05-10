using Gate.CLanguage;
using Gate.CLanguage.Standards;
using Gate.CLanguage.Types;
using Gate.CLanguageTest.Properties;
using Gate.Tools.Message;
using Gate.Tools.Text;

namespace Gate.CLanguageTest
{
   public unsafe class ScopeViewer
   {
      static unsafe void Main(string[] args)
      {
         var std = new CStandardC99();
         var fil = new TxtStore(Resources.scope_test_c);

         var mgs = new MsgCollection();
         var res = std.CCompiler.Compile(fil, mgs, out var src);

         foreach (var msg in mgs) { Console.WriteLine(msg.FullMessage); }

         Console.WriteLine(mgs.Resume);

         if (res)
         {
            var chs = src?.AllDescendant.OfType<CItemWithScopeSpace>().ToArray();

            foreach (var ch in chs ?? [])
            {
               Console.WriteLine($"Item with scope of type {ch.GetType()}: '{ch.Descriptor}'");

               Console.WriteLine("\tScope declarations");
               Console.WriteLine(string.Join("\n", ch.ScopeDecls.Select(d => $"\t\t{d.Descriptor}")));

               Console.WriteLine("\tFunction visible declarations");
               Console.WriteLine(string.Join("\n", ch.Scope.VisibleDeclarations.Select(d => $"\t\t{d.Descriptor}")));

               Console.WriteLine("\tVisible typedefs");
               Console.WriteLine(string.Join("\n", ch.Scope.TypedefsFunctionVisible.Select(d => $"\t\t{d.Descriptor}")));

               Console.WriteLine("\tVisible User Types");
               Console.WriteLine(string.Join("\n", ch.Scope.TypesUsersFunctionVisible.Select(d => $"\t\t{d.Descriptor}")));

               Console.WriteLine();
            }

            Console.WriteLine("List of structs");

            var css = src?.AllDescendant.OfType<CTypeUserDefined>().ToArray();

            foreach (var cls in css ?? [])
            {
               Console.WriteLine(cls.Rebuilt);
               Console.WriteLine(cls.SizeOf);
               //Console.WriteLine("\t{");

               //foreach (var mmb in cls.Members) { Console.WriteLine($"\t{mmb};"); }

               //Console.WriteLine("\t}");
               //Console.WriteLine();
            }
         }
      }
   }
}