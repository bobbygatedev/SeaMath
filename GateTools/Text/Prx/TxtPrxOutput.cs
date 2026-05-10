namespace Gate.Tools.Text.Prx
{
   /// <summary>
   /// Base class for generic preprocessor output.
   /// </summary>
   public class TxtPrxOutput : ICloneable
   {
      private readonly Dictionary<ITxtPxStage, TxtStore> myDictionary = new Dictionary<ITxtPxStage, TxtStore>();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="stage"></param>
      /// <param name="txtStore"></param>
      public void AddResult(ITxtPxStage stage, TxtStore txtStore) => myDictionary.Add(stage, txtStore);

      /// <summary>
      /// Ouptut storage of each stages.
      /// </summary>
      public TxtStore[] StageResults => myDictionary.Values.ToArray();

      /// <summary>
      /// 
      /// </summary>
      public ITxtPxStage[] Stages => myDictionary.Keys.ToArray();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="stage"></param>
      /// <returns></returns>
      public TxtStore? this[ITxtPxStage stage] => stage == null ? null : myDictionary.TryGetValue(stage, out var sto) ? sto : null;

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="TYP"></typeparam>
      /// <returns></returns>
      public TxtStore? GetStoreOfStage<TYP>() where TYP : ITxtPxStage
      {
         var stg = Stages.OfType<TYP>().FirstOrDefault();

         return stg != null ? this[stg] : null;
      }

      /// <summary>
      /// 
      /// </summary>
      public (ITxtPxStage stage, TxtStore store)[] StageStorePairs => myDictionary.Select(kp => (kp.Key, kp.Value)).ToArray();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="destObject"></param>
      /// <returns></returns>
      protected TxtPrxOutput myMakeCopyTo(TxtPrxOutput destObject)
      {
         foreach (var kp in myDictionary) { destObject.myDictionary[kp.Key] = kp.Value; }

         return destObject;
      }

      public object Clone() => myMakeCopyTo(new TxtPrxOutput());
   }
}
