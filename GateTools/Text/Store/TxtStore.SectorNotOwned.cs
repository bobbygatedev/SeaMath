namespace Gate.Tools.Text
{
   public partial class TxtStore
   {
      /// <summary>
      /// Represents sector which is not owned by a <see cref="TxtStore"/>. 
      /// It is used to represent sectors of source which is included by the store.
      /// </summary>
      public class SectorNotOwned : Sector
      {
         private TxtPos? myFrom;
         private TxtPos? myTo;

         public SectorNotOwned(
            TxtTokenConst sourceToken,
            TxtStore store,
            Interval interval) : base(sourceToken, store) => Interval = interval;

         public override Interval Interval { get; }

         /// <summary>
         /// If sector is owned by a <see cref="TxtStore"/> ie this is instance of <see cref="SectorOwned"/> otherwise = 1.
         /// </summary>
         public override int StoreOwnerIdx => -1;

         public override TxtPos? From => myFrom = myFrom ?? Store.GetPos(Interval.From);

         public override TxtPos? To => myTo = myTo ?? Store.GetPos(Interval.To);
      }
   }
}