using Gate.Tools.Message;

namespace Gate.SeaMath
{
   /// <summary>
   /// 
   /// </summary>
   public interface ISeaMathMessageDisplayer
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="msgs"></param>
      void AddMsg(params Msg[] msgs);
      
      /// <summary>
      /// 
      /// </summary>
      void Clear();
   }
}