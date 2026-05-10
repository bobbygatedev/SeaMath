using Gate.Tools.Text;

namespace Gate.CLanguage.PrePx.Directives.Macro.Predefined
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class CPredefMacro : CPrePxDirectiveMacro
   {
      public CPredefMacro() => ContentMap = new ContentMapType(new TxtStore(FixedId));

      /// <summary>
      /// 
      /// </summary>
      public sealed override string? Identifier { get => FixedId; set { } }

      /// <summary>
      /// 
      /// </summary>
      public abstract string FixedId { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract bool IsForCppOnly { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract bool IsForCOnly { get; }

      /// <summary>
      /// Used for disabling macros that are not active in current compilation mode (eg _WIN64 is not actve in 32bit mode)
      /// </summary>
      public abstract bool IsActive { get; } 

      /// <summary>
      /// 
      /// </summary>
      /// <param name="preDefData"></param>
      /// <returns></returns>
      public abstract string GetArgumentString(CPredefMacroData preDefData);
   }
}
