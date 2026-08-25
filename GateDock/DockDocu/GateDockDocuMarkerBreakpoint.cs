using Gate.Tools.AppParams;
using Gate.Tools.Text;

namespace Gate.Dock.DockDocu
{
   /// <summary>
   /// 
   /// </summary>
   public class GateDockDocuMarkerBreakpoint : AppParam.Record
   {
      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="path"></param>
      /// <param name="line"></param>
      /// <param name="col"></param>
      /// <param name="length"></param>
      public GateDockDocuMarkerBreakpoint(string? path, int line, int col, int length)
      {
         BreakpointPath = path;
         Line = line;
         Column = col;
         TextLen = length;
      }

      /// <summary>
      /// Constructor.
      /// </summary>
      public GateDockDocuMarkerBreakpoint() { }

      /// <summary>
      /// Set the position off brekpoint
      /// </summary>
      public abstract class Positioner
      {
         /// <summary>
         /// Move to first avalaible line
         /// </summary>
         public class Standard : Positioner
         {
            public override TxtToken? GetPosition(TxtStore store, int line, int col)
            {
               if (store[line].Content.Trim() != "") { return TxtTokenConst.FromToken(store[line]); }
               else
               {
                  var mrk = new TxtMarker(store);

                  mrk.CurrPos = new TxtPos(line, col);
                  mrk.MoveToNextNoSpace();

                  return mrk.IsIn ? TxtTokenConst.FromToken(store[mrk.CurrPos.Line]) : null;
               }
            }
         }

         public abstract TxtToken? GetPosition(TxtStore fileContentTextStore, int line, int col);
      }

      /// <summary>
      /// Column (1- a value < 1 means all line).
      /// </summary>
      public int Column { get => myColumn.Value; set => myColumn.Value = value; }

      /// <summary>
      /// 
      /// </summary>
      public int TextLen { get => myTextLen.Value; set => myTextLen.Value = value; }

      /// <summary>
      /// 
      /// </summary>
      public string? BreakpointPath { get => myBreakpointPath.Value; set => myBreakpointPath.Value = value; }

      /// <summary>
      /// 
      /// </summary>
      public int Line { get => myLine.Value; set => myLine.Value = value; }

      public override string ToString() => $"Pth:{BreakpointPath} Ln{Line},Col{Column} Len={TextLen}";

      private readonly Simple<int> myColumn = new Simple<int>();
      private readonly Simple<int> myTextLen = new Simple<int>();
      private readonly Simple<string> myGuid = new Simple<string>();
      private readonly Simple<int> myLine = new Simple<int>();
      private readonly Simple<string> myBreakpointPath = new Simple<string>();
   }
}
