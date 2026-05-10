using Gate.ToolsView.TextSearch;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Gate.ToolsView.TextSearch
{
   public partial class TextSearchControl
   {
      private class InnerSearchFlagHelper
      {
         private readonly Dictionary<CheckBox, TextSearchFlags> myDictionaryFlagsByCheck = new Dictionary<CheckBox, TextSearchFlags>();

         public InnerSearchFlagHelper(TextSearchControl parent)
         {
            Parent = parent;
            myDictionaryFlagsByCheck[Parent.CtrlCheckBackward] = TextSearchFlags.Backward;
            myDictionaryFlagsByCheck[Parent.CtrlCheckMatchCase] = TextSearchFlags.MatchCase;
            myDictionaryFlagsByCheck[Parent.CtrlCheckMatchWholeWord] = TextSearchFlags.WholeWord;
            myDictionaryFlagsByCheck[Parent.CtrlCheckWrapAround] = TextSearchFlags.WrapAround;
            myDictionaryFlagsByCheck[Parent.CtrlCheckRegularExpression] = TextSearchFlags.Regex;
            myDictionaryFlagsByCheck[Parent.CtrlCheckButtonOpenFileWhenReplace] = TextSearchFlags.OpenWhenReplace;
            myDictionaryFlagsByCheck[Parent.CtrlCheckButtonUseExtendedChar] = TextSearchFlags.UseExtendedChars;
         }

         public TextSearchControl Parent { get; }

         public void Load()
         {
            if (Parent.PpSearchParamRecord != null)
            {
               var src_flg = Parent.PpSearchParamRecord.SearchFlags.Value;

               foreach (var chk in myDictionaryFlagsByCheck.Keys)
               {
                  chk.Checked = (src_flg & myDictionaryFlagsByCheck[chk]) != 0x0;
               }
            }
         }

         public void CheckChange(CheckBox checkBox)
         {
            if (Parent.PpSearchParamRecord != null)
            {
               var flg = myDictionaryFlagsByCheck[checkBox];

               if (checkBox.Checked)
               {
                  Parent.PpSearchParamRecord.SearchFlags.Value |= flg;
               }
               else
               {
                  Parent.PpSearchParamRecord.SearchFlags.Value &= ~flg;
               }
            }
         }
      }
   }
}
