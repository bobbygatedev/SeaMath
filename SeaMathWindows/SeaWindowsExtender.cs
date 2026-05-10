using Gate.CLanguage;
using Gate.CLanguage.Decl;
using Gate.CLanguage.PrePx.Directives.Macro;
using Gate.CLanguage.Types;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.ConIO;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// Extensions for SeaMath
   /// </summary>
   public static class SeaWindowsExtender
   {

      public static ConsoleCmdHint[] GetHints(this CItem cItem)
      {
         if (cItem is CPrePxDirectiveMacro mac)
         {
            var hnt = new ConsoleCmdHint();

            hnt.ImageId = ConsoleCmdHint.ImageType.define;
            hnt.HintId = hnt.HintText = mac.Identifier;
            hnt.HintFormat = mac.ContentToken?.Content.ExtTrim();
            hnt.HintText = $"#define {mac.Identifier} {mac.ContentToken?.Content.ExtTrim()}";
            return new[] { hnt };
         }
         else if (cItem is CDeclTypedef tdf)
         {
            var hnt = new ConsoleCmdHint();

            hnt.ImageId = ConsoleCmdHint.ImageType.typedef;
            hnt.HintId = hnt.HintText = hnt.HintFormat = tdf.Identifier;

            return new[] { hnt };
         }
         else if (cItem is CTypeStruct str)
         {
            var hnt = new ConsoleCmdHint();

            hnt.ImageId = ConsoleCmdHint.ImageType.type;
            hnt.HintId = hnt.HintText = hnt.HintFormat = str.Identifier;

            return new[] { hnt };
         }
         else if (cItem is CTypeEnum enm)
         {
            var lbs = enm.Labels;

            return lbs.Select(lb =>
            {
               var hnt = new ConsoleCmdHint();
               hnt.ImageId = ConsoleCmdHint.ImageType.enum_label;

               hnt.HintId = hnt.HintText = enm.Identifier + ":" + lb.Identifier;
               hnt.HintFormat = lb.Identifier;

               return hnt;
            }).ToArray();
         }
         else
         {
            return new ConsoleCmdHint[0];
         }
      }

      public static ConsoleCmdHint? GetHint(this RtmObj rtmObj)
      {
         if (!rtmObj.VarName.ExtTrim().IsVarName())
         {
            return null;
         }
         else if (rtmObj is IRtmObjFunction fnc_rmt)
         {
            var hnt = new ConsoleCmdHint();
            var fnc_dcl = fnc_rmt.DeclFunction as CDeclFunction ?? throw new Crash();
            var prs = (fnc_rmt.DeclFunction?.Parameters).NnOrCrash().Select(p => p.Identifier.ExtTrim()).ToArray();

            prs = Enumerable.Range(1, prs.Length).Select(i => prs[i - 1].IsBlank() ? $"[P{i}]" : $"[{prs[i - 1]}]").ToArray();
            hnt.HintFormat = $"{rtmObj.VarName}({string.Join(",", prs)})";
            hnt.ImageId = ConsoleCmdHint.ImageType.func;
            hnt.HintId = rtmObj.VarName;
            hnt.HintText = fnc_dcl.Descriptor;

            return hnt;
         }
         else
         {
            var hnt = new ConsoleCmdHint();

            hnt.ImageId = ConsoleCmdHint.ImageType.var;
            hnt.HintId = hnt.HintFormat = hnt.HintText = rtmObj.VarName;

            return hnt;
         }
      }

   }
}
