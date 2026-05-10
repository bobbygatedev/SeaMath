namespace Gate.Tools.Text
{
   public partial class TxtStore
   {
      /// <summary>
      /// Sector belonging to TxtStore. 
      /// </summary>
      public class SectorOwned : Sector
      {
         private Interval myInterval;
         private int myStoreOwnerIdx;
         private TxtPos? myFrom;
         private TxtPos? myTo;

         public SectorOwned(TxtTokenConst sourceToken, TxtStore store) : base(sourceToken, store) { }

         public override Interval Interval
         {
            get
            {
               myUpdate();

               return myInterval;
            }
         }

         private void myUpdate()
         {
            if (Store.myIsSectorUpdate)
            {
               Store.myIsSectorUpdate = false;
               myStoreOwnerIdx = -1;

               var off = 0;
               var idx = 0;//sector idx starting from 0

               ///updates all sectors inside <see cref="TxtStore"/>
               foreach (var sec in Store.myListOwnedSectors)
               {
                  sec.myInterval = Interval.FromFromLen(off, sec.Length);
                  off += sec.Length;
                  sec.myStoreOwnerIdx = idx++;
                  sec.myFrom = sec.myTo = null;//are when prop get is called
               }
            }
         }

         /// <summary>
         /// If sector is owned by a <see cref="TxtStore"/> ie this is instance of <see cref="SectorOwned"/> otherwise = 1.
         /// </summary>
         public override int StoreOwnerIdx
         {
            get
            {
               myUpdate();

               return myStoreOwnerIdx;
            }
         }

         public override TxtPos? From
         {
            get
            {
               myUpdate();

               return myFrom = myFrom ?? Store.GetPos(Interval.From);
            }
         }

         public override TxtPos? To
         {
            get
            {
               myUpdate();

               return myTo = myTo ?? Store.GetPos(Interval.To);
            }
         }
      }
   }
}
