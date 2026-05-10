using Gate.Tools.Extensions;
using Gate.Tools.Text;
using System.Drawing;

namespace Gate.Tools.Message
{
   /// <summary>
   /// Text messsage with position.
   /// </summary>
   public class Msg : HierarchicalItem
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="msgType"></param>
      /// <param name="msgTxt"></param>
      /// <param name="msgId"></param>
      /// <param name="token"></param>
      /// <param name="msgParams"></param>
      public Msg(MsgType msgType, string? msgTxt, object? msgId = null, TxtToken? token = null)
      {
         MsgTxt = msgTxt;
         Token = token;
         MsgType = msgType;
         MsgId = msgId;
         myAddSubItem(new MsgCollection());// sub-message collection
      }

      /// <summary>
      /// 
      /// </summary>
      public Msg() { }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="msgType"></param>
      /// <param name="msgTxt"></param>
      /// <param name="msgId"></param>
      /// <param name="token"></param>
      /// <returns></returns>
      public static Msg FromToken(MsgType msgType, string msgTxt, object msgId, TxtToken? token) =>
         new Msg(msgType, msgTxt, msgId, token);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="msgType"></param>
      /// <param name="msgTxt"></param>
      /// <param name="msgId"></param>
      /// <param name="txtPos"></param>
      /// <returns></returns>
      public static Msg FromTxtPos(MsgType msgType, string msgTxt, object msgId, TxtPos? txtPos)
      {
         if (txtPos?.Store != null)
         {
            var tok = TxtTokenConst.FromFromLen(txtPos.Store, txtPos.Line, txtPos.Col, 1);

            return new Msg(msgType, msgTxt, msgId, tok);
         }
         else { throw new Crash(); }
      }

      public MsgCollection SubMessages => SubItems.OfType<MsgCollection>().First();

      /// <summary>
      /// Source code fragment binded to the message
      /// </summary>
      public TxtToken? Token { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public MsgType MsgType { get; set; }

      /// <summary>
      /// Full message descriptor of the error (contains <see cref="FilePos"/> info is not null).
      /// </summary>
      public string FullMessage
      {
         get
         {
            var pos = FilePos != null ? $"({FilePos.Line},{FilePos.ColumnDisplayed}): " : "";

            return $"{FilePath ?? ""}{pos,10}{MsgTxt}";
         }
      }

      public bool IsError
      {
         get
         {
            switch (MsgType)
            {
               case MsgType.fatal:
               case MsgType.error:
               case MsgType.fail:
                  return true;

               default: return false;
            }
         }
      }

      /// <summary>
      /// Message text.
      /// </summary>
      public string? MsgTxt { get; private set; }

      /// <summary>
      /// Message identifier.
      /// </summary>
      public object? MsgId { get; private set; }

      /// <summary>
      /// Message file path (or null if Fragment is null) ie the file path which Fragemnt.From refers to. 
      /// </summary>
      public string? FilePath => FilePos?.Store?.FileInfo?.FullName;

      /// <summary>
      /// Message file pos (or null if Fragment is null) ie Fragemnt.From. 
      /// </summary>
      public TxtPos? FilePos => Token?.From?.Primitive;

      /// <summary>
      /// Containing msg collection.
      /// </summary>
      public MsgCollection? ParentCollection => ParentItem as MsgCollection;

      /// <summary>
      /// Not a <see cref="FullMessage"/> blank.
      /// </summary>
      public bool Is2Display => !FullMessage.IsBlank();

      /// <summary>
      /// Equal to <see cref="FullMessage"/>.
      /// </summary>
      /// <returns></returns>
      public override string ToString() => FullMessage;

      /// <summary>
      /// 
      /// </summary>
      public virtual Color? MsgColor
      {
         get
         {
            switch (MsgType)
            {
               case MsgType.uptodate:
               case MsgType.info: return null;

               case MsgType.warning: return Color.Yellow;

               case MsgType.error:
               case MsgType.fail:
               case MsgType.violation:
                  return Color.Red;

               case MsgType.fatal: return Color.Magenta;

               case MsgType.success: return Color.DarkGreen;

               default: throw new Crash();
            }
         }
      }

      public void PlotOnConsole()
      {
         var cur_col = Console.ForegroundColor;

         switch (MsgType)
         {
            case MsgType.info:
               Console.ForegroundColor = cur_col;
               break;

            case MsgType.warning:
               Console.ForegroundColor = ConsoleColor.Yellow;
               break;

            case MsgType.error:
            case MsgType.fail:
            case MsgType.violation:
               Console.ForegroundColor = ConsoleColor.Red;
               break;

            case MsgType.fatal:
               Console.ForegroundColor = ConsoleColor.Magenta;
               break;

            case MsgType.success:
               Console.ForegroundColor = ConsoleColor.DarkGreen;
               break;

            default: throw new Crash();
         }

         Console.WriteLine(FullMessage);
         Console.ForegroundColor = cur_col;
      }
   }
}