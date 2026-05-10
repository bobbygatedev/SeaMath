using Gate.ToolsView.ControlObserve;
using Gate.ToolsView.Extensions;
using Gate.ToolsView.MenuExtended;

namespace Gate.Dock.DockSkin
{
   /// <summary>
   /// 
   /// </summary>
   public class GateDockSkinChildCtrlDispatcher
   {
      private readonly List<Control> myListObservedControls = new List<Control>();
      private readonly InnerObservable myObservable;
      private readonly UpdateContextMenuVisitor myUpdateContextMenuVisitor = new UpdateContextMenuVisitor();
      private readonly UpdateVisitor myUpdateVisitor = new UpdateVisitor();
      private GateDockSkin? mySkin = null;

      public GateDockSkinChildCtrlDispatcher(Control control) => myObservable = new InnerObservable(this, control);

      private class InnerObservable : ControlObservableChild
      {
         private readonly GateDockSkinChildCtrlDispatcher myParentDispatcher;

         public InnerObservable(GateDockSkinChildCtrlDispatcher parent, Control rootControl)
         {
            myParentDispatcher = parent;
            RootControl = rootControl;
         }

         /// <summary>
         /// <br> Selects control to be observed based on following criteria:</br>
         /// <br> - controls implementing IBlockDispatchTag (and its all child, child of child .. controls ) are not added </br>
         /// <br> - controls implementing IGateDockCtrlWithSkin are added but not its child and child of childs.. </br>
         /// <br> - otherwise control is added</br>
         /// </summary>
         /// <param name="control"></param>
         protected override void myAddObservedControl(Control control)
         {
            var is_2_add = true;

            if (control != RootControl)
            {
               //hierarchy ( {control.parent,control.parent.parent, ... RootControl.child, RootControl , RootControl.parent ,... , Form }
               var hie = control.MthGetAnchestors();
               //parent hierarchy cut to Root Control ({control.parent,control.parent.parent, ... ,RootControl.child} )
             
               var hie_roo = RootControl == null ? [] : hie.Take(hie.ToList().IndexOf(RootControl)).ToArray();

               //control if excluded if any anchestor to root is 
               is_2_add = !hie_roo.Any(c => c is IGateDockCtrlWithSkin);
            }

            if (is_2_add && !myParentDispatcher.myListObservedControls.Contains(control)) { myParentDispatcher.myListObservedControls.Add(control); }

            base.myAddObservedControl(control);
         }

         protected override void myRemoveObservedControl(Control? control)
         {
            if (control != null)
            {
               myParentDispatcher.myListObservedControls.Remove(control);

               base.myRemoveObservedControl(control);
            }
         }
      }

      protected class UpdateVisitor
      {
         public virtual void Visit(GateDockSkin skin, Control control)
         {
            control.BackColor = skin.Params.BackFrameColor.Value;
            control.ForeColor = skin.Params.ForeColor.Value;
            control.Font = skin.Params.ControlsFont.Value;
         }
      }

      protected class UpdateContextMenuVisitor
      {
         public virtual void Visit(GateDockSkin skin, ExtendedMenuDropDown menuDropDown)
         {
            menuDropDown.PpBackColorMargin = menuDropDown.BackColor = skin.Params.MenuBackColor.Value;
            menuDropDown.ForeColor = skin.Params.ForeColor.Value;
            menuDropDown.Font = skin.Params.ControlsFont.Value;
            menuDropDown.PpBackColorSelected = skin.Params.BackFrameColor.Value;
         }

         public virtual void Visit(GateDockSkin skin, ContextMenuStrip contextMenu) { }
      }

      /// <summary>
      /// 
      /// </summary>
      public Control? RootControl => myObservable.RootControl;

      /// <summary>
      /// Skin
      /// </summary>
      public GateDockSkin? Skin
      {
         get => mySkin;
         set
         {
            if (value == null) { return; }

            //update is dispatched even if instance not changes(a property of skin could be changed).
            mySkin = value;

            //perform update of control with double dispatching:
            // - controls implementing IGateDockCtrlWithSkin PpSkin property is updated
            // - otherwise abstarct myUpdate is called(sub-class implement behaviour)
            foreach (var ctr in myListObservedControls)
            {
               if (ctr is IGateDockCtrlWithSkin skin && ctr != RootControl) { skin.PpSkin = mySkin; }
               else
               {
                  myUpdate(mySkin, ctr);

                  if (ctr.ContextMenuStrip != null) { myUpdateContextMenu(ctr.ContextMenuStrip); }
               }
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public Control[] ObservedControls => myListObservedControls.ToArray();

      protected virtual void myUpdateContextMenu(ContextMenuStrip contextMenu) => myUpdateContextMenuVisitor.Visit(mySkin, (dynamic)contextMenu);

      protected virtual void myUpdate(GateDockSkin skin, Control control) => myUpdateVisitor.Visit(skin, (dynamic)control);
   }
}
