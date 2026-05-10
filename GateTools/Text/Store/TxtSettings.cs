using Gate.Tools.Text.Encode;
using System.Text;

namespace Gate.Tools.Text
{
   /// <summary>
   /// 
   /// </summary>
   public class TxtSettings
   {
      private string myNewLine = "\r\n";

      /// <summary>
      /// Num tab chars to use
      /// </summary>
      private int myTabNumChars = 3;


      /// <summary>
      /// 
      /// </summary>
      private bool myIsTabUseSpace = true;

      /// <summary>
      /// UTF8 WITHOUT BOM
      /// </summary>
      private Encoding myEncoding = new TxtExtraEncodings.Utf8NoBomType();

      /// <summary>
      /// 
      /// </summary>
      private static TxtSettings myCurrent = new TxtSettings();

      /// <summary>
      /// Constructor.
      /// </summary>
      public TxtSettings() { }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="numTabChars"></param>
      /// <param name="isSpaceToUse"></param>
      /// <param name="encoding"></param>
      public TxtSettings(int numTabChars, bool isSpaceToUse, Encoding encoding)
      {
         TabNumChars = numTabChars;
         IsTabUseSpace = IsTabUseSpace;
         myEncoding = encoding;
      }

      public TxtSettings GetCopy()
      {
         var cpy = new TxtSettings();

         cpy.Encoding = Encoding;
         cpy.TabNumChars = TabNumChars;
         cpy.IsTabUseSpace = IsTabUseSpace;
         cpy.NewLine = NewLine;
      
         return cpy;
      }


      /// <summary>
      /// 
      /// </summary>
      public static TxtSettings Current
      {
         get => myCurrent;

         set => myCurrent = value ?? throw new Gate.Tools.ToolsException("Tab settings not to be null!");
      }

      /// <summary>
      /// 
      /// </summary>
      public Encoding Encoding
      {
         get => myEncoding;
         set => myEncoding = value != null ? value : throw new Gate.Tools.ToolsException("Encoding not to be null!");
      }

      /// <summary>
      /// 
      /// </summary>
      public bool IsTabUseSpace { get => myIsTabUseSpace; set => myIsTabUseSpace = value; }

      /// <summary>
      /// 
      /// </summary>
      public int TabNumChars
      {
         get => myTabNumChars;
         set => myTabNumChars = value > 0 ? value : throw new Gate.Tools.ToolsException("Tab chars to be > 0!");
      }

      public string NewLine
      {
         get => myNewLine;

         set => myNewLine = value ?? "\r\n";
      }
   }
}
