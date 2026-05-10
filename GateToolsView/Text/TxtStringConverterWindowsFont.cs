using Gate.Tools;
using Gate.Tools.Message;
using Gate.Tools.Text;

namespace Gate.ToolsView.Text
{
   /// <summary>
   /// Simple <see cref="TxtStringConverter"/> for <see cref="System.Drawing.Font"/> suitbale just in Windows.
   /// </summary>
   public class TxtStringConverterWindowsFont : TxtStringConverter.Simple<System.Drawing.Font>
   {
#pragma warning disable CA1416 // Validate platform compatibility
      private readonly FontConverter myFontConverter = new();
#pragma warning restore CA1416 // Validate platform compatibility

      public TxtStringConverterWindowsFont()
      {

      }

      public override bool TryParse(string stringValue, Type stringType, out object? result, MsgCollection? msgs)
      {
         try
         {
            result = myFontConverter.ConvertFromString(stringValue);

            return true;
         }
         catch (NotSupportedException exc)
         {
            msgs?.Add(new Msg(MsgType.fail, exc.Message));
            result = null;

            return false;
         }
         catch (Exception exc) { throw new Crash(exc); }
      }

      public override string? ToStr(object? objValue) =>
         objValue is Font ? myFontConverter.ConvertTo(objValue, typeof(string)) as string : throw new Crash($"Not a Font!");
   }
}
