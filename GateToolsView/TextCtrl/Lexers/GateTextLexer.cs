using ScintillaNET;
using System.Collections.Generic;
using System.Drawing;

namespace Gate.ToolsView.TextCtrl.Lexers
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class GateTextLexer
   {
      private readonly List<int> myListExtraLexers = new List<int>();

      public const int EXTRA_LEXER_MIN = 40;
      public const int EXTRA_LEXER_MAX = 254;

      /// <summary>
      /// 
      /// </summary>
      public abstract string[] Extensions { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract string Name { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract GateTextLexerEnum Id { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract string KeyWords0 { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract string KeyWords1 { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="docTextCtrl"></param>
      public abstract void Configure(GateTextControl docTextCtrl);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="docTextCtrl"></param>
      protected virtual void myConfigureCommon(GateTextControl docTextCtrl)
      {
         var bck_mar = docTextCtrl.PpMarginColor.IsEmpty ? docTextCtrl.BackColor : docTextCtrl.PpMarginColor;

         docTextCtrl.PpScintilla.StyleResetDefault();

         docTextCtrl.PpScintilla.Styles[Style.Default].ForeColor = docTextCtrl.ForeColor;
         docTextCtrl.PpScintilla.Styles[Style.Default].BackColor = docTextCtrl.BackColor;
         docTextCtrl.PpScintilla.Styles[Style.Default].Font = docTextCtrl.Font.Name;
         docTextCtrl.PpScintilla.Styles[Style.Default].SizeF = docTextCtrl.Font.Size;

         docTextCtrl.PpScintilla.StyleClearAll();

         docTextCtrl.PpScintilla.Styles[Style.LineNumber].BackColor = bck_mar;
         docTextCtrl.PpScintilla.SetFoldMarginColor(true, bck_mar);
         docTextCtrl.PpScintilla.SetFoldMarginHighlightColor(true, bck_mar);

         docTextCtrl.PpScintilla.Margins[GateTextControl.FOLD_MARGIN_IDX].Type = MarginType.Symbol;
         docTextCtrl.PpScintilla.Margins[GateTextControl.FOLD_MARGIN_IDX].Mask = Marker.MaskFolders;
         docTextCtrl.PpScintilla.Margins[GateTextControl.FOLD_MARGIN_IDX].Sensitive = true;
         docTextCtrl.PpScintilla.Margins[GateTextControl.FOLD_MARGIN_IDX].Width = docTextCtrl.PpFoldZoneGetter != null ? 20 : 0;

         for (var i = Marker.FolderEnd; i <= Marker.FolderOpen; i++)
         {
            var for_col = docTextCtrl.PpFoldBoxBackColor == Color.Empty ? docTextCtrl.BackColor : docTextCtrl.PpFoldBoxBackColor;
            var bak_col = docTextCtrl.PpFoldLineColor == Color.Empty ? docTextCtrl.ForeColor : docTextCtrl.PpFoldLineColor;

            docTextCtrl.PpScintilla.Markers[i].SetForeColor(for_col);
            docTextCtrl.PpScintilla.Markers[i].SetBackColor(bak_col);
         }

         docTextCtrl.PpScintilla.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
         docTextCtrl.PpScintilla.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
         docTextCtrl.PpScintilla.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
         docTextCtrl.PpScintilla.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;
         docTextCtrl.PpScintilla.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
         docTextCtrl.PpScintilla.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
         docTextCtrl.PpScintilla.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
      }

      protected virtual void myConfigureCommonEnd(GateTextControl docTextCtrl)
      {
         docTextCtrl.PpScintilla.Lexer = (Lexer)Id;
         docTextCtrl.PpScintilla.SetKeywords(0, KeyWords0);
         docTextCtrl.PpScintilla.SetKeywords(1, KeyWords1);
         docTextCtrl.PpScintilla.CaretForeColor = docTextCtrl.ForeColor;
      }
   }
}
