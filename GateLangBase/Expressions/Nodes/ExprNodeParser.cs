using Gate.Tools.Text.Elab;
using System;

namespace Gate.LangBase.Expressions.Nodes
{
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="IN_DATA"></typeparam>
   /// <typeparam name="PRS_OUT"></typeparam>
   public abstract class ExprNodeParser<IN_DATA, PRS_OUT> : ParserStep<IN_DATA, ExprNodeOutput<PRS_OUT>>
      where IN_DATA : TxtElabInData
      where PRS_OUT : class, ICloneable, new()
   {
   }
}
