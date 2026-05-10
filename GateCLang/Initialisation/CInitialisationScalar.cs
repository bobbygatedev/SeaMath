using Gate.CLanguage.Decl;
using Gate.CLanguage.Expressions;

namespace Gate.CLanguage.Initialisation
{
   /// <summary>
   /// Init implementation for scalar with/without struct field indication eg 5 | .f1=3.2
   /// </summary>
   public class CInitialisationScalar : CInitialisation
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="scalarExpression"></param>
      public CInitialisationScalar(CExprStatement scalarExpression) => myAddSubItem(scalarExpression);

      public static CInitialisationScalar MakeScalar(CExprStatement scalarExpression) => scalarExpression.IsLiteralString ?
         new CInitialisationString(scalarExpression) : new CInitialisationScalar(scalarExpression);

      public override CExprStatement? ScalarExpression => SubItems.OfType<CExprStatement>().FirstOrDefault();

      public override int? IncompleteArraySize => null;

      public override string Descriptor =>
         ((StructFieldName ?? "") != "" ? $".{StructFieldName}=" : "") +
         (ScalarExpression != null ?
            (ScalarExpression.Descriptor + (Indices.HasValue ? $"-->{Indices.Value.StringVal}" : "")) :
            "Empty");

      public CDeclSubscriptIndices? Indices { get; set; }
   }
}
