using Gate.Tools;
using System.ComponentModel;

namespace Gate.ToolsView.Extended
{
   public partial class ExtendedColumnListViewControl
   {
      [Browsable(false)]
      public abstract class BaseType : Component
      {
         protected delegate void OnListViewControlChangedHandler(ExtendedColumnListViewControl listViewControl);

         protected HierarchicalItem.ParentObserver myParentObserver;

         private DummyItem myDummyItem;

         public BaseType()
         {
            myDummyItem = new DummyItem(this);
            myParentObserver = new HierarchicalItem.ParentObserver(myDummyItem);
            myParentObserver.OnParentAdded += myActionOnParentAdded;
            myParentObserver.OnParentRemoved += myActionOnParentRemoved;
         }

         private class DummyItem : HierarchicalItem
         {
            public DummyItem(BaseType item) => LinkedBaseItem = item;

            public void Clear() => myRemoveSubItemRange(SubItems);

            public void AddItems(params BaseType[] items) => myAddSubItemRange(items.Select(i => i.myDummyItem));

            public void InsertItem(BaseType item, int atIndex) => myInsertSubItem(item.myDummyItem, atIndex < 0 ? SubItems.Length : atIndex);

            public BaseType LinkedBaseItem { get; }

            public void RemoveItems(params BaseType[] items) => myRemoveSubItemRange(items.Select(i => i.myDummyItem));
         }

         protected BaseType[] myGetSubItems() => myDummyItem.SubItems.Cast<DummyItem>().Select(d => d.LinkedBaseItem).ToArray();

         protected void myAddToContainer(params BaseType[] items) => myDummyItem.AddItems(items);

         /// <summary>
         /// 
         /// </summary>
         /// <typeparam name="ITEM"></typeparam>
         /// <param name="items"></param>
         /// <param name="atItemTypeIndex">Index where item is of type <typeparamref name="ITEM"/></param>
         protected void myInsertToContainer<ITEM>(ITEM[] items, int atItemTypeIndex) where ITEM : BaseType
         {
            var lst_its = myDummyItem.SubItems.OfType<DummyItem>().Select(di => di.LinkedBaseItem).OfType<ITEM>().ToList();

            myDummyItem.RemoveItems(lst_its.ToArray());
            lst_its.InsertRange(atItemTypeIndex, items);
            myDummyItem.AddItems(lst_its.ToArray());
         }

         protected void myRemoveFromContainer(BaseType[]? item) => myDummyItem.RemoveItems(item ?? []);

         protected void myClearContainer() => myDummyItem.Clear();

         protected void myResetContainer(BaseType[] value)
         {
            myClearContainer();

            foreach (var cel in value ?? []) { myAddToContainer(cel); }
         }

         protected BaseType? myGetParentItem() => myDummyItem.ParentItem is DummyItem dum ? dum.LinkedBaseItem : null;

         protected abstract void myActionOnListViewReset(ExtendedColumnListViewControl listViewControl);

         protected abstract void myActionOnListViewSet(ExtendedColumnListViewControl listViewControl);

         private void myActionOnParentRemoved(HierarchicalItem observer, HierarchicalItem parent)
         {
            if (((DummyItem)parent).LinkedBaseItem is InnerItemContainer row_cnt) { myActionOnListViewReset(row_cnt.Parent); }
         }

         private void myActionOnParentAdded(HierarchicalItem observer, HierarchicalItem parent)
         {
            if (((DummyItem)parent).LinkedBaseItem is InnerItemContainer row_cnt) { myActionOnListViewSet(row_cnt.Parent); }
         }
      }
   }
}
