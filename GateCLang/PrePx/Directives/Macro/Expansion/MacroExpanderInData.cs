using Gate.CLanguage.PrePx.Directives.Macro.Predefined;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.Directives.Macro.Expansion
{
   /// <summary>
   /// Macor expander input data.
   /// </summary>
   public class MacroExpanderInData : TxtElabInData
   {
      private CPrePxDirectiveMacro[]? myMacroSet;

      /// <summary>
      /// Constructor for root expander.
      /// </summary>
      /// <param name="predefMacroData"></param>
      public MacroExpanderInData(CPrePxInData prePxData) : base(prePxData.Messages)
      {
         PrePxData = prePxData;
         IsRoot = true;
      }

      /// <summary>
      /// Constructor for recursive(of macro content) call contains macro set to apply.
      /// </summary>
      /// <param name="predefMacroData"></param>
      /// <param name="macroSet"></param>
      public MacroExpanderInData(CPrePxInData prePxData, CPrePxDirectiveMacro[] macroSet) : base(prePxData.Messages)
      {
         PrePxData = prePxData;
         MacroInputSet = macroSet;
         IsRoot = false;
      }

      /// <summary>
      /// <br>  array of macro to be for (recursive) expansion </br>
      /// </summary>
      public CPrePxDirectiveMacro[] MacroInputSet
      {
         get => PrePxData.PrePx.PredefMacros.Concat(myMacroSet ?? []).ToArray();
         
         set => myMacroSet = (value ?? []).Where(m => !(m is CPredefMacro)).ToArray();
      }

      /// <summary>
      /// 
      /// </summary>
      public CPrePxInData PrePxData { get; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsRoot { get; }
   }
}
