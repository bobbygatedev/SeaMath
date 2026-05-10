using System.Text;

namespace Gate.Tools.Text.Encode
{
   public static class TxtExtraEncodings
   {
      public class Utf8BomType : UTF8Encoding
      {
         public const string NAME = "Utf8-BOM";

         public Utf8BomType() : base(true) { }

         public static byte[] Bom => new byte[] { 0xEF, 0xBB, 0xBF };

         public override string BodyName => NAME;

         public override string EncodingName => BodyName;

         public override string HeaderName => BodyName;

         public override string WebName => BodyName;
      }

      public class Utf16BomBEType : UnicodeEncoding
      {
         public const string NAME = "Utf16-BOM-BE";

         public Utf16BomBEType() : base(true, true) { }

         public static byte[] Bom => new byte[] { 0xFE, 0xFF };

         public override string BodyName => NAME;

         public override string EncodingName => BodyName;

         public override string HeaderName => BodyName;

         public override string WebName => BodyName;
      }


      public class Utf16BomType : UnicodeEncoding
      {
         private const string NAME = "Utf16-BOM";

         public Utf16BomType() : base(false, true) { }

         public static byte[] Bom => new byte[] { 0xFF, 0xFE };

         public override string BodyName => NAME;

         public override string EncodingName => BodyName;

         public override string HeaderName => BodyName;

         public override string WebName => BodyName;
      }

      public class Utf16NoBomType : UnicodeEncoding
      {
         public Utf16NoBomType() : base(false, false) { }
      }

      public class Utf16NoBomBEType : UnicodeEncoding
      {
         public const string NAME = "Utf16-BE";

         public Utf16NoBomBEType() : base(true, false) { }

         public override string BodyName => NAME;
      }

      public class Utf8NoBomType : UTF8Encoding
      {
         public Utf8NoBomType() : base(false) { }
      }

      public static readonly Utf16BomType Utf16Bom = new Utf16BomType();
      public static readonly Utf16NoBomType Utf16NoBom = new Utf16NoBomType();

      public static readonly Utf8BomType Utf8Bom = new Utf8BomType();
      public static readonly Utf8NoBomType Utf8NoBom = new Utf8NoBomType();

      public static readonly Utf16BomBEType Utf16BomBE = new Utf16BomBEType();
      public static readonly Utf16NoBomBEType Utf16NoBomBE = new Utf16NoBomBEType();

      public static Encoding[] AllEncodings = new Encoding[] {
         Utf16Bom,
         Utf16NoBom,
         Utf16BomBE,
         Utf16NoBomBE,
         Utf8Bom,
         Utf8NoBom
      };
   }
}
