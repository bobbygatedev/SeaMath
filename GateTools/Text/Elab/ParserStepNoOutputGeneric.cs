namespace Gate.Tools.Text.Elab
{
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="DATA"></typeparam>
   /// <typeparam name="OUTPUT"></typeparam>
   public abstract class ParserStepNoOutputGeneric<DATA> : ParserStep<DATA, TxtElabNotAnOut>
      where DATA : TxtElabInData
   {

   }
}
