namespace Gate.Tools.Text.Elab
{
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="DATA"></typeparam>
   /// <typeparam name="OUTPUT"></typeparam>
   public abstract class TxtElabNoOutput<INPUT_MARKER, DATA> : TxtElab<INPUT_MARKER, DATA, TxtElabNotAnOut>
      where INPUT_MARKER : class , ITxtElabInput
      where DATA : TxtElabInData
   {

   }
}
