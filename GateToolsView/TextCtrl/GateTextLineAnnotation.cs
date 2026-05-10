using Gate.Tools;
using Gate.Tools.Extensions;
using ScintillaNET.Gate;
using System.Data;

namespace Gate.ToolsView.TextCtrl
{
   public class GateTextLineAnnotation
   {
      private readonly ScintillaExtension myScintilla;
      private readonly GateTextControl myScintillaControl;
      private GateTextLineAnnotationRow[]? myRows;

      internal GateTextLineAnnotation(GateTextControl scintillaControl, int lineIdx)
      {
         myScintilla = scintillaControl.PpScintilla;
         myScintillaControl = scintillaControl;
         LineIdx = lineIdx;
         IsOn = true;
      }

      public bool IsOn { get; private set; }

      public int LineIdx { get; private set; }

      public GateTextLineAnnotationRow[]? Rows
      {
         get => myRows;
         set
         {
            myRows = value;
            UpdateRows();
         }
      }

      public string? Text
      {
         get => myScintilla.Lines[LineIdx - 1].AnnotationText;
         set
         {
            var lns = value.Nn().Split(["\r\n", "\n\r", "\n"], StringSplitOptions.None);

#pragma warning disable CS8604 // Possible null reference argument.
            Rows = lns.Select(l => new GateTextLineAnnotationRow(this, l, StyleUnique)).ToArray();
#pragma warning restore CS8604 // Possible null reference argument.
            UpdateRows();
         }
      }

      public GateTextStyle? StyleUnique
      {
         get => myScintillaControl.PpScintillaStyles.FirstOrDefault(s => s.Index == (GateTextStyleEnum)myScintilla.Lines[LineIdx - 1].AnnotationStyle);
         set => myScintilla.Lines[LineIdx - 1].AnnotationStyle = (int)(value ?? throw new Crash()).Index;
      }

      public void SwitchOff()
      {
         if (IsOn)
         {
            myScintilla.ClearAnnotationOnLine(LineIdx);
            IsOn = false;
            myScintillaControl.MthAnnotationOff(this);
         }
      }

      internal void UpdateRows()
      {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
         if (myRows == null) { myScintilla.Lines[LineIdx - 1].AnnotationText = null; }
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
         else
         {
            var max_len = myRows.Max(r => r.Text.Length);
            var row_sts = myRows.Select(r => r.Style != null ? r.Style.Index : GateTextStyleEnum.Default).ToArray();
            var ann_bys = new byte[row_sts.Length * max_len];
            var k = 0;

            foreach (var row_sty in row_sts.Select(rs => (byte)rs))
            {
               for (int j = 0; j < max_len; j++)
               {
                  ann_bys[k++] = row_sty;
               }
            }

            myScintilla.Lines[LineIdx - 1].AnnotationText = String.Join("\n", myRows.Select(r => r.Text));
            myScintilla.Lines[LineIdx - 1].AnnotationStyles = ann_bys;
         }
      }
   }
}
