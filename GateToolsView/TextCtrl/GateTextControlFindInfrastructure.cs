using Gate.Tools.Text;
using Gate.ToolsView.Extensions;
using Gate.ToolsView.TextSearch;

namespace Gate.ToolsView.TextCtrl
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class GateTextControlFindInfrastructure : TextSearchInfrastructure.Standard
   {
      /// <summary>
      /// 
      /// </summary>
      protected class CtrlInteraction : ITextSearchCtrlInteraction
      {
         public string? GetFileContent(Control docTextCtrl) => myInvokeInto(docTextCtrl, c => c.PpContentText);

         public string? GetOpenFile(Control docTextCtrl) => myInvokeInto(docTextCtrl, c => c.PpOpenPath);

         public TxtPos? GetTextPos(Control docTextCtrl) => myInvokeInto(docTextCtrl, c => new TxtPos(c.PpCurrLine, c.PpCurrCol));

         public void SelectToken(TxtToken findToken, Control docTextCtrl) =>
            myInvokeInto<object>(docTextCtrl, c => { c.MthSelectToken(findToken); return null; });

         public void ReplaceSelected(string replaceText, Control docTextCtrl) => myInvokeInto<object>(docTextCtrl, c => { c.MthReplaceSelected(replaceText); return null; });

         public void ResetText(Control docTextCtrl, string content) =>
            myInvokeInto<object>(docTextCtrl, c => { c.PpContentText = content; return null; });

         private T? myInvokeInto<T>(Control docTextCtrl, Func<GateTextControl, T?> action) where T : class
         {
            var res = null as T;

            if (docTextCtrl is GateTextControl ctr) { docTextCtrl.MthInvoke(() => res = action(ctr)); }

            return res;
         }

         public TxtToken? GetSelection(Control docTextCtrl) => myInvokeInto(docTextCtrl, c => c.PpSelection);

         public TxtToken? GetMultilineSelectionToken(Control docTextCtrl) => myInvokeInto(docTextCtrl, txt_ctr =>
            {
               var ins = txt_ctr.PpScintillaIndicators[GateTextIndicatorScintillaIdEnum.indicator_11_text_find_selection];

               if (ins.Length > 0)
               {
                  var ind = ins.First();

                  return TxtTokenConst.FromFromTo(txt_ctr.PpContentText, ind.LineStart, ind.ColStart, ind.LineEnd, ind.ColEnd);
               }
               else { return null; }
            });

         public void SetMultilineSelectionToken(Control docTextCtrl, TxtToken? textToken)
         {
            myInvokeInto<object>(docTextCtrl, txt_ctr =>
            {
               if (textToken != null)
               {
                  var fro = txt_ctr.MthGetPosition(textToken.From?.Line ?? -1, textToken.From?.Col ?? -1);
                  var to = txt_ctr.MthGetPosition(textToken.To?.Line ?? -1, textToken.To?.Col ?? -1);

                  txt_ctr.MthIndicatorOn(GateTextIndicatorScintillaIdEnum.indicator_11_text_find_selection, fro, to, Color.White, 128, 0);
               }
               else
               {
                  txt_ctr.MthIndicatorOffAll(GateTextIndicatorScintillaIdEnum.indicator_11_text_find_selection);
               }

               return null;
            });
         }
      }

      /// <summary>
      ///  
      /// </summary>
      /// <returns></returns>
      protected override ITextSearchCtrlInteraction myMakeTxtCtrlInteraction() => new CtrlInteraction();
   }
}
