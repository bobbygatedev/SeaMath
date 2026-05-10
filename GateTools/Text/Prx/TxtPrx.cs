using Gate.Tools.Text.Elab;

namespace Gate.Tools.Text.Prx
{
   /// <summary>
   /// Generic class for test processor. 
   /// </summary>
   /// <typeparam name="DATA"></typeparam>
   /// <typeparam name="OUTPUT"></typeparam>
   public abstract class TxtPrx<DATA, OUTPUT>
      where DATA : TxtElabInData
      where OUTPUT : TxtPrxOutput, new()
   {
      public interface IStage : ITxtPxStage
      {
         TxtElabResult Start(DATA data, TxtStore store2Edit, ref OUTPUT output);
      }

      public abstract class SingleStage : TxtPrx<DATA, OUTPUT>
      {
         private readonly IStage[] myStages;

         public SingleStage() => myStages = [new InnerStage(this)];

         private class InnerStage : IStage
         {
            public InnerStage(SingleStage singleStage) => SingleStage = singleStage;

            public SingleStage SingleStage { get; }

            public TxtElabResult Start(DATA data, TxtStore store2Edit, ref OUTPUT output) => SingleStage.myStart(data, store2Edit);
         }

         public override IStage[] Stages => myStages;

         protected abstract TxtElabResult myStart(DATA data, TxtStore store2Edit);
      }


      public abstract IStage[] Stages { get; }

      public TxtElabResult Start(TxtStore inStore, DATA data, ref OUTPUT output)
      {
         var sto = inStore;

         if (output == null) { output = new OUTPUT(); }

         foreach (var sta in Stages)
         {           
            var res = sta.Start(data, sto = sto.GetCopy(), ref output);

            output.AddResult(sta, sto);

            switch (res)
            {
               case TxtElabResult.success: break;

               case TxtElabResult.failure:
               case TxtElabResult.failure_unrecoverable: return res;

               case TxtElabResult.continue_searching:
               default:
                  throw new Crash();
            }
         }

         return TxtElabResult.success;
      }
   }
}
