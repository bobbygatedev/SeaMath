using Gate.Tools;
using Gate.Tools.Text;

namespace Gate.ToolsView.TextCtrl
{
   public partial class GateTextControl
   {
      public enum FoldZoneTypeEnum
      {
         text_root = 0,
         comment_multi_line,
         comment_single_line,
         brace,
         function,
         @class
      }

      public class FoldZone : HierarchicalItem
      {
         private int myTo = int.MaxValue;

         public FoldZone(int from, string fileBody, FoldZoneTypeEnum type)
         {
            From = from;
            FileBody = fileBody;
            Type = type;
         }

         public int From { get; }

         public string FileBody { get; }

         public Interval Interval => new Interval(From, To);

         public int To { get => Math.Min(myTo, FileBody.Length - 1); set => myTo = value; }

         public TxtPos? FromPos { get; internal set; } = null;

         public TxtPos? ToPos { get; internal set; } = null;

         public FoldZoneTypeEnum Type { get; set; }

         public int Level => ParenthoodDepth;

         public void AddSubZone(FoldZone cppZone) => myAddSubItem(cppZone);

         public void RemoveSubZone(FoldZone cppZone) => myRemoveSubItem(cppZone);

         public FoldZone ParentZone => ParentItem as FoldZone ?? throw new NullReferenceException();

         public FoldZone[] SubZones => SubItems.OfType<FoldZone>().ToArray();

         public int NumLines => (ToPos?.Line - FromPos?.Line + 1) ?? 0;

         public FoldZone[] AllZones => AllDescendant.Cast<FoldZone>().ToArray();

         public override string ToString() => $"{Type} {FromPos}-{ToPos}";
      }
   }
}


