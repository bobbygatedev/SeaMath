using System.Text;

namespace Gate.Tools
{
   /// <summary>
   /// 
   /// </summary>
   public static class EncodingHelper
   {
      /// <summary>
      /// 
      /// </summary>
      public static readonly EncodingInfo[] EncodingInfos = Encoding.GetEncodings().ToArray();

      /// <summary>
      /// Returns an array of single-byte encodings, ordered with commonly used encodings first and without
      /// duplicates.
      /// </summary>
      /// <remarks>The returned array includes <see cref="Encoding.UTF8"/>, <see
      /// cref="Encoding.Default"/>, encodings from Windows code page 1250, and all other available single-byte
      /// encodings on the system. Each encoding appears only once, based on its web name. The order prioritizes
      /// widely used encodings to facilitate selection in user interfaces or encoding negotiation
      /// scenarios.</remarks>
      /// <returns>An array of <see cref="Encoding"/> objects representing all available single-byte encodings, ordered with
      /// common encodings first and without duplicates.</returns>
      public static Encoding[] OrderedEncodingsSingleByte
      {
         get
         {
            var lst = new List<Encoding>
               {
                  Encoding.UTF8,
                  Encoding.Default
               };

            lst.AddRange(Windows1250Encoding);
            lst.AddRange(EncodingsSingleByte);

            return lst.GroupBy(i => i.WebName).Select(g => g.First()).ToArray();
         }
      }

      /// <summary>
      /// Gets an array of supported <see cref="Encoding"/> instances, ordered by preference and without duplicates.
      /// </summary>
      /// <remarks>The returned array includes commonly used encodings, such as UTF-8 and the system
      /// default encoding, followed by additional encodings relevant to the application context. Each encoding
      /// appears only once in the array, even if it is available through multiple sources.</remarks>
      public static Encoding[] OrderedEncodings
      {
         get
         {
            var lst = new List<Encoding>
               {
                  Encoding.UTF8,
                  Encoding.Unicode,
                  Encoding.UTF32,
               };

            lst.AddRange(Windows1250Encoding);
            lst.AddRange(EncodingInfos.Select(e => e.GetEncoding()));

            return lst.GroupBy(i => i.WebName).Select(g => g.First()).ToArray();
         }
      }

      /// <summary>
      /// Gets an array of all single-byte character encodings supported by the system, ordered by code page.
      /// </summary>
      /// <remarks>Single-byte encodings represent each character with a single byte and are commonly used
      /// for legacy data or interoperability with older systems. The returned array may vary depending on the encodings
      /// available on the current platform.</remarks>
      public static Encoding[] EncodingsSingleByte =>
         EncodingInfos.Select(e => e.GetEncoding()).
         Where(e => e.IsSingleByte).
         OrderBy(e => e.CodePage).ToArray();


      /// <summary>
      /// Windows 1250 (1250-1258) family encodings only.
      /// </summary>
      /// <returns></returns>
      public static Encoding[] Windows1250Encoding =>
         EncodingInfos.
            Where(e => e.CodePage % 1250 < 10).
            OrderBy(ei => ei.CodePage).
            Select(e => e.GetEncoding()).ToArray();
   }

}
