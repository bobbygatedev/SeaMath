using System.Collections.Generic;

namespace Gate.Tools
{
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="ITM"></typeparam>
   public interface IHierachicalItemCollectionRO<ITM> : IEnumerable<ITM> where ITM : HierarchicalItem
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="index"></param>
      /// <returns></returns>
      ITM this[int index] { get; }

      /// <summary>
      /// 
      /// </summary>
      int Count { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="itm"></param>
      /// <returns></returns>
      int IndexOf(ITM itm);
   }
}