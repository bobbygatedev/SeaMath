using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System.Collections.Generic;

namespace Gate.CLanguage.PrePx.Stages
{
   /// <summary>
   /// 
   /// </summary>
   public class CPrePxStage1TrigraphReplacement : ICPrePxStage
   {
      public CPrePxStage1TrigraphReplacement()
      {
         
      }

      /// <summary>
      /// List of trigraphs.
      /// </summary>
      public virtual (string trigraph, string replace)[] TriraphPairs
      {
         get
         {
            return new (string, string)[]  {
               ("??=" , "#"),
               ("??)" , "]"),
               ("??!" , "|"),
               ("??(" , "["),
               ("??'" , "^"),
               ("??>" , "}"),
               ("??/" , "\\"),
               ("??<" , "{"),
               ("??-" , "~"),
         };
         }
      }

      public bool IsForSourceOnly => false;

      public TxtElabResult Start(CPrePxInData data, TxtStore store2Edit, ref CPrePxOutput output)
      {
         if (data.Options?.AreTrigraphToReplace ?? false)
         {
            var lst_rep = new List<TxtStoreReplacement>();

            foreach (var tri in TriraphPairs)
            {
               for (int i = 0; i < store2Edit.Content.Length;)
               {
                  var idx = store2Edit.Content.IndexOf(tri.trigraph, i);

                  if (idx == -1) { break; }
                  else { lst_rep.Add(new TxtStoreReplacement(Interval.FromFromLen(idx, tri.trigraph.Length), new TxtTokenConst(tri.replace))); }
               }
            }

            store2Edit.Replace(lst_rep.ToArray());
         }
         
         return TxtElabResult.success;
      }

      public override string ToString() => $"PrePx Stage1: Trigraph replacement";
   }
}
