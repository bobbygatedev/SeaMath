using Gate.Dock.DockFactories;
using Gate.Dock.DockSkin;
using Gate.ToolsView.ControlObserve;
using Gate.ToolsView.Extensions;
using System.ComponentModel;

namespace Gate.Dock.DockTab
{
   /// <summary>
   /// Ctrl can be tabbed into tab control, base for document(implementing IGateDockDocu) and simple tab control(NOT implementing IGateDockDocu).
   /// </summary>
   public partial class GateDockTabPageCtrl : UserControl, IGateDockCtrlWithSkin
   {
      private GateDockMainForm? myMainFrm = null;
      private GateDockTabPageCtrlSkinDispacther? mySkinChildCtrlDispacther = null;
      private ControlObservableFocused myObservableFocused;
      private string? myTitle = "";
      private GateDockTabPageFactory? myFactory = null;

      public event EventHandler? OnTitleChange;

      public GateDockTabPageCtrl()
      {
         InitializeComponent();

         myObservableFocused = new ControlObservableFocused(this);
         myObservableFocused.OnIsFocusedChanged += MyObservableFocused_OnIsFocusedChanged;

         PpSkinChildCtrlDispacther = new GateDockTabPageCtrlSkinDispacther(this);
      }

      /// <summary>
      ///  
      /// </summary>
      public virtual GateDockMainForm? PpMainFrm
      {
         get => myMainFrm;

         internal set
         {
            if (myMainFrm != value)
            {
               if (myMainFrm != null) { myMainFrm.OnSkinChange -= MyMainFrm_OnSkinChange; }

               myMainFrm = value;

               if (myMainFrm != null)
               {
                  PpSkin = myMainFrm.PpSkin;
                  myMainFrm.OnSkinChange += MyMainFrm_OnSkinChange;
               }
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public bool PpIsDocuVisible => PpParentTab != null ? PpParentTab.PpTabPageVisible == this : Visible;

      /// <summary>
      /// 
      /// </summary>
      public GateDockTabCtrl? PpParentTab => this.MthGetAnchestor<GateDockTabCtrl>();

      /// <summary>
      /// 
      /// </summary>
      [Browsable(false)]
      public GateDockSkin? PpSkin
      {
         get => PpSkinChildCtrlDispacther?.Skin;
         set 
         {
            if (PpSkinChildCtrlDispacther != null)
            {
               PpSkinChildCtrlDispacther.Skin = value;
            }
         }
      }

      /// <summary>
      /// Factory used for making this control.
      /// </summary>
      public GateDockTabPageFactory? PpFactory
      {
         get => myFactory;

         internal set
         {
            if (value != myFactory)
            {
               myActionOnFactoryChange(myFactory = value);
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public GateDockTabPageCtrlSkinDispacther? PpSkinChildCtrlDispacther
      {
         get => mySkinChildCtrlDispacther;
         set
         {
            if (value != null) { mySkinChildCtrlDispacther = value; }
            else { MessageBox.Show($"{GetType().Name}.PpSkinChildCtrlDispacther can't be null"); }
         }
      }

      /// <summary>
      ///  the title of tab page, which is shown as tab page name.
      /// </summary>
      public string? PpTitle
      {
         get => myTitle;
         set
         {
            myTitle = value;
            OnTitleChange?.Invoke(this, new EventArgs());
         }
      }

      protected virtual void myActionOnLostFocus(object? sender) { }

      protected virtual void myActionOnGotFocus(object? sender) { }

      protected virtual void myActionOnFactoryChange(GateDockTabPageFactory? tabPageFactory) { }

      private void MyMainFrm_OnSkinChange(object? sender, GateDockSkin? skin) => PpSkin = skin;

      private void MyObservableFocused_OnIsFocusedChanged(Control? observable, Control? focusedControl, bool isSelected)
      {
         if (isSelected) { myActionOnGotFocus(observable); }
         else { myActionOnLostFocus(observable); }
      }
   }
}
