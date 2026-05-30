using System.Collections;
using System.Runtime.CompilerServices;

namespace Gate.Tools
{
   /// <summary>
   /// Represents an item that can be part of a hierarchical structure, 
   /// allowing for parent-child relationships and providing events for changes in the hierarchy.
   /// </summary>
   public abstract class HierarchicalItem
   {
      public delegate void OnChildAddedHandler(HierarchicalItem sender, HierarchicalItem childAdded);
      public delegate void OnChildRemovedHandler(HierarchicalItem sender, HierarchicalItem childRemoved);
      public delegate void OnParentSetHandler(HierarchicalItem sender, HierarchicalItem newParent);
      public delegate void OnParentResetHandler(HierarchicalItem sender, HierarchicalItem resetParent);
      public delegate void OnObserverHandler(HierarchicalItem observer, HierarchicalItem childItem);

      public event OnChildAddedHandler? OnChildAdded;
      public event OnChildRemovedHandler? OnChildRemoved;
      public event OnParentSetHandler? OnParentSet;
      public event OnParentResetHandler? OnParentReset;

      private readonly List<HierarchicalItem> myListSubItem = new List<HierarchicalItem>();
      private HierarchicalItem? myParentItem = null;

      /// <summary>
      /// 
      /// </summary>
      public enum DescendantOrderEnum
      {
         /// <summary>
         /// 
         /// </summary>
         pre_order = 0,
      }

      /// <summary>
      /// 
      /// </summary>
      protected HierarchicalItem()
      {

      }

      /// <summary>
      /// Read Only collection of child hierarchical items use <see cref="Collection{ITM}"/> for add/remove/clear functionalities (instance belongs to hierarchy).
      /// </summary>
      /// <typeparam name="ITM"></typeparam>
      public class CollectionRO<ITM> : HierarchicalItem, IHierachicalItemCollectionRO<ITM> where ITM : HierarchicalItem
      {
         /// <summary>
         /// 
         /// </summary>
         protected CollectionRO() { }

         /// <summary>
         /// 
         /// </summary>
         public int Count => SubItems.Length;

         /// <summary>
         /// 
         /// </summary>
         /// <param name="itm"></param>
         /// <returns></returns>
         public int IndexOf(ITM itm) => this.ToList().IndexOf(itm);

         /// <summary>
         /// 
         /// </summary>
         /// <param name="index"></param>
         /// <returns></returns>
         public ITM this[int index] => (ITM)SubItems[index];

         /// <summary>
         /// 
         /// </summary>
         /// <returns></returns>
         public IEnumerator<ITM> GetEnumerator() => SubItems.Cast<ITM>().GetEnumerator();

         /// <summary>
         /// 
         /// </summary>
         /// <returns></returns>
         IEnumerator IEnumerable.GetEnumerator() => SubItems.Cast<ITM>().GetEnumerator();
      }

      /// <summary>
      /// Collection of child hierarchical items add/remove/clear functionalities (instance belongs to hierarchy).
      /// </summary>
      /// <typeparam name="ITM"></typeparam>
      public class Collection<ITM> : CollectionRO<ITM>, IHierachicalItemCollection<ITM> where ITM : HierarchicalItem
      {
         /// <summary>
         /// /
         /// </summary>
         public Collection() { }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="itemArray"></param>
         public void Add(params ITM[] itemArray) => myAddSubItemRange(itemArray);

         /// <summary>
         /// 
         /// </summary>
         /// <param name="itemArray"></param>
         public void Remove(params ITM[] itemArray) => myRemoveSubItemRange(itemArray);

         /// <summary>
         /// 
         /// </summary>
         /// <param name="atIndex"></param>
         /// <param name="itemArray"></param>
         public void Insert(int atIndex, params ITM[] itemArray) => myInsertSubItemRange(itemArray, atIndex);

         /// <summary>
         /// 
         /// </summary>
         public void Clear() => myRemoveSubItemRange(SubItems);

         /// <summary>
         /// 
         /// </summary>
         public ITM[] Items => SubItems.Cast<ITM>().ToArray();
      }

      /// <summary>
      /// 
      /// </summary>
      public class ChildObserver
      {
         public event OnObserverHandler? OnChildAdded;
         public event OnObserverHandler? OnChildRemoved;

         public ChildObserver(HierarchicalItem observer)
         {
            foreach (var itm in observer.AllDescendant)
            {
               itm.OnChildAdded += Itm_OnChildAdded;
               itm.OnChildRemoved += Itm_OnChildRemoved;
            }

            ObservingItem = observer;
         }

         public HierarchicalItem ObservingItem { get; }

         private void Itm_OnChildRemoved(HierarchicalItem sender, HierarchicalItem childRemoved)
         {
            var dsc_arr = childRemoved.AllDescendant;

            foreach (var dsc in dsc_arr)
            {
               dsc.OnChildAdded -= Itm_OnChildAdded;
               dsc.OnChildRemoved -= Itm_OnChildRemoved;
            }

            foreach (var dsc in dsc_arr) { myActionOnChildRemoved(dsc); }
         }

         private void Itm_OnChildAdded(HierarchicalItem sender, HierarchicalItem childAdded)
         {
            var dsc_arr = childAdded.AllDescendant;

            foreach (var dsc in dsc_arr.Where(d => d != sender))
            {
               dsc.OnChildAdded += Itm_OnChildAdded;
               dsc.OnChildRemoved += Itm_OnChildRemoved;
            }

            foreach (var dsc in dsc_arr.Where(d => d != sender)) { myActionOnChildAdded(dsc); }
         }

         protected virtual void myActionOnChildAdded(HierarchicalItem child) => OnChildAdded?.Invoke(ObservingItem, child);

         protected virtual void myActionOnChildRemoved(HierarchicalItem child) => OnChildRemoved?.Invoke(ObservingItem, child);
      }

      /// <summary>
      /// 
      /// </summary>
      public class ParentObserver
      {
         public event OnObserverHandler? OnParentAdded;
         public event OnObserverHandler? OnParentRemoved;

         public ParentObserver(HierarchicalItem observer)
         {
            Observer = observer;

            foreach (var itm in observer.ParentItemChain)
            {
               itm.OnParentSet += Itm_OnParentSet;
               itm.OnParentReset += Itm_OnParentReset;
            }

            foreach (var itm in observer.ParentItemChain.Where(i => i != observer)) { myActionOnParentAdded(itm); }
         }

         public HierarchicalItem Observer { get; }

         private void Itm_OnParentReset(HierarchicalItem sender, HierarchicalItem resetParent)
         {
            foreach (var itm in resetParent.ParentItemChain)
            {
               itm.OnParentSet -= Itm_OnParentSet;
               itm.OnParentReset -= Itm_OnParentReset;
               myActionOnParentRemoved(itm);
            }
         }

         private void Itm_OnParentSet(HierarchicalItem sender, HierarchicalItem newParent)
         {
            foreach (var itm in newParent.ParentItemChain)
            {
               itm.OnParentSet += Itm_OnParentSet;
               itm.OnParentReset += Itm_OnParentReset;

               myActionOnParentAdded(itm);
            }
         }

         protected virtual void myActionOnParentRemoved(HierarchicalItem parent) => OnParentRemoved?.Invoke(Observer, parent);

         protected virtual void myActionOnParentAdded(HierarchicalItem parent) => OnParentAdded?.Invoke(Observer, parent);
      }

      /// <summary>
      /// 
      /// </summary>
      public class HierarchyObserver
      {
         public event OnObserverHandler? OnItemAdded;
         public event OnObserverHandler? OnItemRemoved;

         public HierarchyObserver(HierarchicalItem observer)
         {
            Observer = observer;
            AllHierarchy = Root.AllDescendant;

            foreach (var itm in AllHierarchy)
            {
               itm.OnChildAdded += Itm_OnChildAdded;
               itm.OnChildRemoved += Itm_OnChildRemoved;
               itm.OnParentSet += Itm_OnParentSet;
               itm.OnParentReset += Itm_OnParentReset;
            }
         }

         public HierarchicalItem Root => Observer.ParentItemChain.Last();

         public HierarchicalItem Observer { get; }

         public HierarchicalItem[] AllHierarchy { get; private set; }

         protected virtual void myActionOnItemRemoved(HierarchicalItem parent) => OnItemRemoved?.Invoke(Observer, parent);

         protected virtual void myActionOnItemAdded(HierarchicalItem parent) => OnItemAdded?.Invoke(Observer, parent);

         private void Itm_OnParentSet(HierarchicalItem sender, HierarchicalItem newParent) => Itm_OnChildAdded(sender, newParent);

         private void Itm_OnParentReset(HierarchicalItem sender, HierarchicalItem resetParent) => Itm_OnChildRemoved(sender, resetParent);

         private void Itm_OnChildRemoved(HierarchicalItem sender, HierarchicalItem childRemoved)
         {
            if (AllHierarchy.Contains(childRemoved))
            {
               var new_hie = Root.AllDescendant;
               var rem_its = AllHierarchy.Except(new_hie).ToArray();

               AllHierarchy = new_hie;

               foreach (var itm in rem_its)
               {
                  itm.OnChildAdded -= Itm_OnChildAdded;
                  itm.OnChildRemoved -= Itm_OnChildRemoved;

                  myActionOnItemRemoved(itm);
               }
            }
         }

         private void Itm_OnChildAdded(HierarchicalItem sender, HierarchicalItem childAdded)
         {
            if (!AllHierarchy.Contains(childAdded))
            {
               var add_its = childAdded.AllDescendant;

               AllHierarchy = Root.AllDescendant;

               foreach (var itm in add_its)
               {
                  itm.OnChildAdded += Itm_OnChildAdded;
                  itm.OnChildRemoved += Itm_OnChildRemoved;
                  myActionOnItemAdded(itm);
               }
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public DescendantOrderEnum DescendantOrder { get; set; } = DescendantOrderEnum.pre_order;

      /// <summary>
      /// 
      /// </summary>
      public HierarchicalItem[] SubItems => myListSubItem.ToArray();

      /// <summary>
      /// 
      /// </summary>
      public HierarchicalItem? ParentItem
      {
         get => myParentItem;

         protected set
         {
            if (value != myParentItem)
            {
               if (myParentItem != null) { myParentItem.myRemoveSubItem(this); }

               if (value != null) { value.myAddSubItem(this); }
            }
         }
      }

      /// <summary>
      /// Array of my anchestor from ME to ROOT included (backward to root item).
      /// </summary>
      public HierarchicalItem[] ParentItemChain
      {
         get
         {
            var lst_cha = new List<HierarchicalItem>();
            var itm = this;

            lst_cha.Add(itm);

            while (itm.ParentItem != null)
            {
               lst_cha.Add(itm.ParentItem);
               itm = itm.ParentItem;
            }

            return lst_cha.ToArray();
         }
      }

      /// <summary>
      /// Depth (number of parent recursively counted to root).
      /// </summary>
      public int ParenthoodDepth
      {
         get
         {
            var dpt = 0;
            var itm = this;

            while ((itm = itm.ParentItem) != null) { dpt++; }

            return dpt;
         }
      }

      /// <summary>
      /// Returns an array including this,SubItems,SubItems of SubItems,..
      /// </summary>
      /// <param name="descendantOrder"></param>
      /// <returns></returns>
      public HierarchicalItem[] GetDescendantWalk(DescendantOrderEnum descendantOrder)
      {
         switch (DescendantOrder)
         {
            case DescendantOrderEnum.pre_order:
               var stk = new Stack<HierarchicalItem>();
               var lst = new List<HierarchicalItem>();

               stk.Push(this);

               while (stk.Count > 0)
               {
                  var nod = stk.Pop();

                  lst.Add(nod);

                  foreach (var chi in nod.SubItems.Reverse()) { stk.Push(chi); }
               }

               return lst.ToArray();

            default: throw new Crash();
         }
      }

      /// <summary>
      /// Root <see cref="HierarchicalItem"/> of hierarchy.
      /// </summary>
      public HierarchicalItem Anchestor
      {
         get
         {
            var par = this;

            while (true)
            {
               if (par.ParentItem == null) { return par; }
               else { par = par.ParentItem; }
            }
         }
      }


      /// <summary>
      /// Array including this,SubItems,SubItems of SubItems,..
      /// </summary>
      public HierarchicalItem[] AllDescendant => GetDescendantWalk(DescendantOrder);

      /// <summary>
      /// Returns whether item is me or my parent or parent of my parent ...
      /// </summary>
      /// <param name="item"></param>
      /// <returns></returns>
      public bool IsMyAnchestor(HierarchicalItem item)
      {
         if (item == this) { return true; }
         else
         {
            var par = this;

            if ((par = par.ParentItem) != null)
            {
               if (par == item) { return true; }
            }

            return false;
         }
      }

      /// <summary>
      /// Returns whether item is my me my son (any of my subitems) or son of my son ...
      /// </summary>
      /// <param name="item"></param>
      /// <returns></returns>
      public bool IsMyDescendant(HierarchicalItem item) => item.IsMyAnchestor(this);

      private class InnerRefEqualComparer : IEqualityComparer<HierarchicalItem>
      {
         public bool Equals(HierarchicalItem? x, HierarchicalItem? y) => ReferenceEquals(x, y);

         public int GetHashCode(HierarchicalItem obj) => RuntimeHelpers.GetHashCode(obj);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="subItems"></param>
      protected void myAddSubItemRange(IEnumerable<HierarchicalItem> subItems) => myInsertSubItemRange(subItems, SubItems.Length);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="subItems"></param>
      /// <param name="atIndex"></param>
      /// <exception cref="System.ArgumentOutOfRangeException"></exception>
      protected void myInsertSubItemRange(IEnumerable<HierarchicalItem> subItems, int atIndex)
      {
         var sis = (subItems ?? []).Where(s => s != null).ToArray();

         ///all item shall be (by ref) different
         var sis_ne = sis.Distinct(new InnerRefEqualComparer()).ToArray();//sub-items not equal
         var all_hie = AllHierarchy.ToArray();

         /// any from <see cref="subItems"/> is already in hierarchy
         if (sis_ne.Any(s => all_hie.Any(h => ReferenceEquals(s, h))))
         {
            var wro_sub_its = sis_ne.Where(s => all_hie.Any(h => ReferenceEquals(s, h))).ToArray();

            throw new Gate.Tools.ToolsException($"Some items({string.Join(",", wro_sub_its.Select(s => s.ToString()))}) already in this hierarchy!");
         }

         ///then any from <see cref="sis"/> is duplicated 
         if (sis_ne.Length < sis.Count()) { throw new Gate.Tools.ToolsException("Subitems of difference instance!"); }

         if (sis_ne.Any(i => i.ParentItem != null && i.ParentItem != this)) { throw new Gate.Tools.ToolsException("Remove the parent before reassigning!"); }

         myListSubItem.InsertRange(atIndex, sis_ne);

         foreach (var itm in sis_ne)
         {
            itm.myParentItem = this;
            myActionOnChildAdded(itm);
            itm.myActionOnParentSet(this);
         }
      }

      public HierarchicalItem[] AllHierarchy => Anchestor.AllDescendant;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="subItem"></param>
      /// <param name="atIndex"></param>
      /// <returns></returns>
      /// <exception cref="System.ArgumentOutOfRangeException"></exception>
      protected bool myInsertSubItem(HierarchicalItem subItem, int atIndex)
      {
         if (subItem != null)
         {
            myInsertSubItemRange(new[] { subItem }, atIndex);

            return true;
         }

         return false;
      }

      /// <summary>
      /// <br>Adds a subitem <paramref name="subItem"/> is not null. </br>
      /// <br> Returns false in case of any error.</br>
      /// </summary>
      /// <param name="subItem"></param>
      /// <returns></returns>
      protected bool myAddSubItem(HierarchicalItem? subItem)
      {
         if (subItem != null)
         {
            myInsertSubItemRange([subItem], SubItems.Length);

            return true;
         }

         return false;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="subItems"></param>
      protected void myRemoveSubItemRange(IEnumerable<HierarchicalItem>? subItems)
      {
         if (subItems != null)
         {
            foreach (var sub_itm in subItems.ToArray()) { myRemoveSubItem(sub_itm); }
         }
      }

      /// <summary>
      /// Returns false in case of any error.
      /// </summary>
      /// <param name="subItem"></param>
      /// <returns></returns>
      protected bool myRemoveSubItem(HierarchicalItem? subItem)
      {
         if (subItem != null)
         {
            if (subItem.myParentItem == this)
            {
               myListSubItem.Remove(subItem);
               subItem.myParentItem = null;
               myActionOnChildRemoved(subItem);
               subItem.myActionOnParentReset(this);

               return true;
            }
         }

         return false;
      }

      protected bool myParentNotify() => ParentItem != null ? ParentItem.myOnParentNotify(this) : false;

      protected virtual bool myOnParentNotify(HierarchicalItem hierarchicalItem) => true;

      protected virtual void myActionOnParentSet(HierarchicalItem parentItem) => OnParentSet?.Invoke(this, parentItem);
      protected virtual void myActionOnParentReset(HierarchicalItem parentItem) => OnParentReset?.Invoke(this, parentItem);
      protected virtual void myActionOnChildAdded(HierarchicalItem childAdded) => OnChildAdded?.Invoke(this, childAdded);
      protected virtual void myActionOnChildRemoved(HierarchicalItem childAdded) => OnChildRemoved?.Invoke(this, childAdded);

      /// <summary>
      /// Check whether or not <paramref name="descendant"/> is a descendant of this, then perform <see cref="myAddSubItem(HierarchicalItem)"/>
      /// </summary>
      /// <param name="descendant"></param>
      /// <param name="itemToPush"></param>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      protected void myPushToDescendant(HierarchicalItem descendant, HierarchicalItem itemToPush)
      {
         if (AllDescendant.Contains(descendant)) { descendant.myAddSubItem(itemToPush); }
         else { throw new Gate.Tools.ToolsException("Item is not a descendant."); }
      }

      protected void myPopDescendant(HierarchicalItem descendant, bool isNoChildCheck)
      {
         if (AllDescendant.Contains(descendant))
         {
            if (isNoChildCheck && descendant.SubItems.Length > 0)
            {
               throw new Gate.Tools.ToolsException("Item to remove shall not have child");
            }

            descendant?.ParentItem?.myRemoveSubItem(descendant);
         }
         else
         {
            throw new Gate.Tools.ToolsException("Item is not a descendant.");
         }
      }

      protected void myReplaceParent(HierarchicalItem? newParent)
      {
         if (ParentItem != null) { ParentItem.myRemoveSubItem(this); }
         if (newParent != null) { newParent.myAddSubItem(this); }
      }
   }
}
