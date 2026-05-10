using System.Text;

namespace Gate.Tools.Text.Encode
{
   /// <summary>
   /// 
   /// </summary>
   public interface ITxtEncodingRetriever
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="path"></param>
      /// <param name="fallbackEncoding"></param>
      /// <returns></returns>
      Encoding FromPath(string path, Encoding? fallbackEncoding = null);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="bytes"></param>
      /// <param name="fallbackEncoding"></param>
      /// <returns></returns>
      Encoding FromBytes(byte[] bytes, Encoding? fallbackEncoding = null);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="content"></param>
      /// <param name="fallbackEncoding"></param>
      /// <returns></returns>
      Encoding? FromString(string content, Encoding? fallbackEncoding = null);
   }
}
