using Gate.CLanguage.Decl;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// Represents a specialized RTM object function that incorporates a lambda expression.
   /// </summary>
   /// <remarks>This class extends <see cref="RtmObjFunction"/> to provide additional functionality for
   /// handling lambda expressions in conjunction with a declared function. It is designed to work with <see
   /// cref="CDeclFunction"/> and exposes the lambda expression as a display value.</remarks>
   public class SeaRtmLambdaObjFunction : RtmObjFunction
   {
      public SeaRtmLambdaObjFunction(CDeclFunction declFunction, string lambda) : base(declFunction) => Lambda = lambda;

      public new CDeclFunction DeclFunction => base.DeclFunction as CDeclFunction ?? throw new Crash();

      public override string DisplayValue => Lambda;

      public string Lambda { get; }
   }
}
