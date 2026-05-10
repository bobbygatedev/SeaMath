namespace Gate.CLanguage.Runtime.Object
{
   /// <summary>
   /// 
   /// </summary>
   public interface ICRtmObjArray : ICRtmObjPointer
   {
      int[] Sizes { get; }
   }
}
