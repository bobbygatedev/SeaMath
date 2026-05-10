using Gate.CLanguage;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Standards;
using Gate.LangBase.ExtraTypes;
using Gate.Tools;
using Gate.Tools.Message;
using Gate.Tools.Text;

namespace Gate.CLanguageTest
{
   /// <summary>
   /// 
   /// </summary>
   public class ComplexTest
   {
      private static Type? myGetBaseType(Type complexType) => complexType.GetField("Re")?.FieldType;

      private static unsafe void myParseTest(string inputExpr, bool hasComplex)
      {
         var std = new CStandardC99();
         var mgs = new MsgCollection();

         (std.CCompiler.Settings.BuiltInSetSettings ?? throw new Crash()).HasComplex = hasComplex;
         std.CCompiler.Settings.BuiltInSetSettings = std.CCompiler.Settings.BuiltInSetSettings;//refresh

         Console.WriteLine($"input: '{inputExpr}'");

         var res = std.CCompiler.Compile(new TxtStore(inputExpr), mgs, out var src);

         if (res)
         {
            var dcl_spc = src?.AllDescendant.OfType<CDeclSpecifiers>().FirstOrDefault() ?? throw new Crash();

            Console.WriteLine(dcl_spc);
         }
         else { mgs.PlotOnConsole(); }
      }

      static unsafe void Main(string[] args)
      {
         Console.WriteLine("RtmNumericConverterStandard.DoConvert test..");

         var rtm_cvt = new CLangNumericConverterStandard();

         var iss = new object[]{
               new ComplexUint8(11, 2),
               new ComplexUint16(11, 2),
               new ComplexUint32(11, 2),
               new ComplexUint32(11, 2),
               new ComplexInt8(11, 2),
               new ComplexInt16(11, 2),
               new ComplexInt32(11, 2),
               new ComplexInt32(11, 2),};

         var oss = iss.Select(i => rtm_cvt.DoConvertCsharpValue(
            myGetBaseType(i.GetType()) ?? throw new Crash(),
            i as ValueType ?? throw new Crash()) ?? throw new Crash()).ToArray();

         for (int i = 0; i < iss.Length; i++)
         {
            Console.WriteLine($"{iss[i]}({iss[i].GetType().Name})=>{oss[i]}({oss[i].GetType().Name})");
         }


         Console.WriteLine("Parse test..");

         myParseTest("long _Complex x=555ulli;", true);
         myParseTest("long _Complex x=555ulli;", false);
         myParseTest("double _Complex x=5.0e-3i;", true);
         myParseTest("float _Complex x=5.0e-3fi;", true);
         //todo long double still not working
      }
   }
}
