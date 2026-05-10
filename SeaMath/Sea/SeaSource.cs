using Gate.CLanguage.Source;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// Represents a source of sea expressions, providing functionality to manage global scope expressions.
   /// </summary>
   /// <remarks>The <see cref="SeaSource"/> class extends <see cref="CSource"/> and provides methods and
   /// properties for working with global scope expressions, which are represented by <see cref="SeaExprStatement"/>
   /// objects.</remarks>
   public class SeaSource : CSource
   {
      public SeaSource() { }

      /// <summary>
      /// Gets an array of <see cref="SeaExprStatement"/> objects representing the global scope expressions.
      /// </summary>
      public SeaExprStatement[] GlobalScopeExpr => SubItems.OfType<SeaExprStatement>().ToArray();

      /// <summary>
      /// Adds a global scope expression to the current context.
      /// </summary>
      /// <param name="expr">The global scope expression to add. Cannot be <see langword="null"/>.</param>
      public void AddGlobalScopeExpression(SeaExprStatement expr) => myAddSubItem(expr);
   }
}
