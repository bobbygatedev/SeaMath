using Gate.Tools.Text.Elab;

namespace Gate.LangBase.Expressions.Nodes
{
   /// <summary>
   /// 
   /// </summary>
   public class ExprNodeOutput<INT_OUT> : TxtElabOutputList<ExprNode>, ICloneable where INT_OUT : class, ICloneable, new()
   {
      /// <summary>
      /// 
      /// </summary>
      public ExprNodeOutput() { }

      public INT_OUT? InterpreterOutput { get; set; }

      object ICloneable.Clone()
      {
         var clo = new ExprNodeOutput<INT_OUT>();

         clo.ListProduct = ListProduct.ToList();
         clo.InterpreterOutput = InterpreterOutput?.Clone() as INT_OUT;

         return clo;
      }
   }
}
