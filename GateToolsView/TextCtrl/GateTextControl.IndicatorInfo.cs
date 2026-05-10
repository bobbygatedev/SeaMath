using ScintillaNET;
using ScintillaNET.Gate;

namespace Gate.ToolsView.TextCtrl
{
   public partial class GateTextControl
   {
      public struct IndicatorInfo
      {
         public IndicatorInfo(Indicator indicator, int startPos, int endPos, GateTextControl gateTextControl, GateTextIndicatorScintillaIdEnum id)
         {
            Indicator = indicator;
            StartPos = startPos;
            EndPos = endPos;
            GateTextControl = gateTextControl;
            Id = id;
         }

         public class Collection
         {
            public Collection(GateTextControl gateTextControl) => GateTextControl = gateTextControl;

            public GateTextControl GateTextControl { get; }
            public ScintillaExtension PpScintilla => GateTextControl.PpScintilla;

            public IndicatorInfo[] this[GateTextIndicatorScintillaIdEnum indicatorId]
            {
               get
               {
                  var lst_ind = new List<IndicatorInfo>();
                  var txt_len = PpScintilla.TextLength;
                  var ind = PpScintilla.Indicators[(int)indicatorId];
                  var bmp_flg = (1 << ind.Index);
                  var end_pos = 0;

                  do
                  {
                     var sta_pos = ind.Start(end_pos);

                     end_pos = ind.End(sta_pos);

                     // Is this range filled with our indicator id?
                     var bit_map = PpScintilla.IndicatorAllOnFor(sta_pos);
                     var is_fil = ((bmp_flg & bit_map) == bmp_flg);

                     if (is_fil)
                     {
                        // Do stuff with indicator range
                        lst_ind.Add(new IndicatorInfo(ind, sta_pos, end_pos - 1, GateTextControl, indicatorId));
                     }

                  } while (end_pos != 0 && end_pos < txt_len);

                  return lst_ind.ToArray();
               }
            }
         }

         public override bool Equals(object? obj)
         {
            if (obj is IndicatorInfo oth)
            {
               return oth.StartPos == StartPos &&
                  oth.Id == Id &&
                  oth.EndPos == EndPos &&
                  oth.GateTextControl == GateTextControl;
            }
            else { return false; }
         }

         public override int GetHashCode() => StartPos;

         public Indicator Indicator { get; }
         public int StartPos { get; }
         public int EndPos { get; }
         public int LineStart => GateTextControl.MthGetLine(StartPos);
         public int LineEnd => GateTextControl.MthGetLine(EndPos);
         public int ColStart => GateTextControl.MthGetCol(StartPos);
         public int ColEnd => GateTextControl.MthGetCol(EndPos);
         public GateTextControl GateTextControl { get; }
         public GateTextIndicatorScintillaIdEnum Id { get; }

         public int Len => EndPos - StartPos + 1;

         /// <summary>
         /// Whether caret position is inside interval 
         /// </summary>
         public bool IsIn => GateTextControl.PpCurrIdx >= StartPos && GateTextControl.PpCurrIdx <= EndPos;
      }
   }
}


