using Gate.CLanguage.PrePx.Directives.Macro.Predefined;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaPreDefMacro : CPredefMacro
   {
      public const string ID = "__SEA__";

      public SeaPreDefMacro() { }
      
      public override string FixedId => ID;

      public override bool IsForCppOnly => false;

      public override bool IsForCOnly => false;

      public override bool IsActive => true;

      public override string GetArgumentString(CPredefMacroData preDefData) => "1";
   }
}
