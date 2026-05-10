using Gate.CLanguage.Statement;
using Gate.Tools;

namespace Gate.CLanguage.Interpreter
{
   /// <summary>
   /// 
   /// </summary>
   public class CTokenInterpreterOutput : ICloneable
   {
      private Stack<CItem> myStackItems = new Stack<CItem>();

      /// <summary>
      /// 
      /// </summary>
      public CTokenInterpreterOutput() { }

      /// <summary>
      /// 
      /// </summary>
      public CItem? TopItem => myStackItems.FirstOrDefault();

      /// <summary>
      /// 
      /// </summary>
      public CCycle? CurrentCycle
      {
         get
         {
            var its = myStackItems.ToArray();

            for (var i = 0; i < its.Length; i++)
            {
               var cmp = its[i] as CCompound;

               if (cmp?.ParentItem is CCycle cyc) { return cyc; }
            }

            return null;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public CItem[] ItemsOnStack => myStackItems.ToArray();

      /// <summary>
      /// 
      /// </summary>
      public CItemWithScopeSpace? ScopeSpaceItem => ItemsOnStack.OfType<CItemWithScopeSpace>().FirstOrDefault();

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public CItem? Peek() => myStackItems.Count > 0 ? myStackItems.Peek() : null;

      public ITEM PopOrCrash<ITEM>() where ITEM : CItem =>
         myStackItems.Pop() as ITEM ?? throw new Crash($"Not {typeof(ITEM).Name} on out stack");

      public ITEM PeekOrCrash<ITEM>() where ITEM : CItem =>
            myStackItems.Peek() as ITEM ?? throw new Crash($"Not {typeof(ITEM).Name} on out stack");

      public ITEM PeekOrCrash<ITEM>(int offset) where ITEM : CItem =>
         myStackItems.ToArray()[offset] as ITEM ?? throw new Crash($"Not {typeof(ITEM).Name} at slot {offset} of stack");

      public ITEM? PeekOrDefault<ITEM>(int offset = 0) where ITEM : CItem =>
         offset >= 0 && offset < myStackItems.Count() ? myStackItems.ElementAtOrDefault(offset) as ITEM : null;

      /// <summary>
      /// <br> Returns all item "above" <paramref name="item"/></br> 
      /// <br> eg stack = [ ix iy <paramref name="item"/> i1 i2 13 ] </br>
      /// <br> returns i1 12 i3  </br>
      /// </summary>
      /// <typeparam name="ITEM"></typeparam>
      /// <param name="item"></param>
      /// <param name="isPop">If true items are popped from stack</param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      public ITEM[] GetAllItemsAbove<ITEM>(CItem? item, bool isPop) where ITEM : CItem
      {
         var lst_its = myStackItems.ToList();
         var idx = item != null ? lst_its.IndexOf(item) : -1;

         if (idx == -1) { throw new Crash(); }
         else
         {
            var its = lst_its.Take(idx).Reverse().Cast<ITEM>().ToArray();

            if (isPop)
            {
               for (var i = 0; i < idx; i++) { myStackItems.Pop(); }
            }

            return its;
         }
      }

      public object Clone()
      {
         var cpy = new CTokenInterpreterOutput();

         foreach (var itm in myStackItems.Reverse()) { cpy.myStackItems.Push(itm); }

         return cpy;
      }

      public void Push(CItem item) => myStackItems.Push(item);

      public void RestoreStackTo(CItem currentStackTop)
      {
         if (currentStackTop == null) { myStackItems.Clear(); }
         else if (myStackItems.Contains(currentStackTop))
         {
            while (myStackItems.Peek() != currentStackTop) { myStackItems.Pop(); }
         }
         else { throw new Crash($"{currentStackTop.Descriptor} not contained in interpret out stack!"); }
      }

      public override string ToString() => $"->|{string.Join("|", myStackItems.ToArray().Select(s => s.ToString()))}|";
   }
}
