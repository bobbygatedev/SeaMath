namespace Gate.Tools
{
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="ITM"></typeparam>
   public interface IHierachicalItemCollection<ITM> : IHierachicalItemCollectionRO<ITM> where ITM : HierarchicalItem
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="itemArray"></param>
      void Add(params ITM[] itemArray);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="atIndex"></param>
      /// <param name="itemArray"></param>
      void Insert(int atIndex, params ITM[] itemArray);
      
      /// <summary>
      /// 
      /// </summary>
      /// <param name="itemArray"></param>
      void Remove(params ITM[] itemArray);
   
      /// <summary>
      /// 
      /// </summary>
      void Clear();
   }
}