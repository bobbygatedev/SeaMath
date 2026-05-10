using Gate.Tools.Text.Elab;
using System;

namespace Gate.LangBase.Expressions.Nodes
{
   /// <summary>
   /// OUTPUT
   /// </summary>
   /// <typeparam name="IN_DATA"></typeparam>
   public abstract class ExprNodeInterpret<IN_DATA, INT_OUT> : TokenInterpreter<IN_DATA, ExprNodeOutput<INT_OUT>> 
      where IN_DATA : TxtElabInData
      where INT_OUT : class, ICloneable, new()
   {
   }
}
