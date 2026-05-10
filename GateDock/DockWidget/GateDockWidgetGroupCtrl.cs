using Gate.Dock.DockSkin;
using Gate.Tools;
using Gate.ToolsView.Dockable;
using Gate.ToolsView.Extensions;
using System.ComponentModel;
using System.Data;

namespace Gate.Dock.DockWidget
{
   /// <summary>
   /// 
   /// </summary>
   public partial class GateDockWidgetGroupCtrl : UserControl, IGateDockCtrlWithSkin
   {
      private GateDockMainForm? myMainFrm = null;

      public GateDockWidgetGroupCtrl()
      {
         InitializeComponent();
         PpSkinChildCtrlDispacther = new SkinChildCtrlDispacther(this);
         DoubleBuffered = true;
      }

      public class SkinChildCtrlDispacther : GateDockSkinChildCtrlDispatcher
      {
         public SkinChildCtrlDispacther(GateDockWidgetGroupCtrl parent) : base(parent) { }

         protected override void myUpdate(GateDockSkin skin, Control control)
         {
            control.BackColor = skin.Params.BackFrameColor.Value;
            control.ForeColor = skin.Params.ForeColor.Value;
         }
      }

      public GateDockWidgetStateFlags PpDockState
      {
         get
         {
            var dck_are = this.MthGetAnchestor<DockableAreaCtrl>();

            if (dck_are == null) { return GateDockWidgetStateFlags.invisible; }
            else
            {
               var anc = dck_are.MthGetAnchorFromControl(this);

               return myGetStateFromAnchor(anc);
            }
         }
      }

      public DockableAreaCtrlSlotAnchorModeEnum PpAnchorMode { get; internal set; }

      public GateDockWidgetCtrl[] PpWidgets => CtrlDockTabbed.PpTabs.Cast<GateDockWidgetCtrl>().ToArray();

      public GateDockMainForm? PpMainFrm
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
      [Browsable(false)]
      public GateDockSkin? PpSkin { get => PpSkinChildCtrlDispacther.Skin; set => PpSkinChildCtrlDispacther.Skin = value; }

      /// <summary>
      /// 
      /// </summary>
      public int PpSelectedIndex { get => CtrlDockTabbed.PpTabVisibleIdx; set => CtrlDockTabbed.PpTabVisibleIdx = value; }

      /// <summary>
      /// 
      /// </summary>
      public SkinChildCtrlDispacther PpSkinChildCtrlDispacther { get; set; }

      public void MthWidgetAdd(params GateDockWidgetCtrl[] widgetCtrs)
      {
         foreach (var wdg in widgetCtrs)
         {
            CtrlDockTabbed.MthControlAdd(wdg);

            var but = CtrlDockTabbed.MthGetButton(wdg);

            but.PpText = wdg.PpTitle;
            wdg.OnTitleChange += Wdg_OnTitleChange;
         }
      }

      public void MthWidgetRemove(params GateDockWidgetCtrl[] widgets)
      {
         foreach (var wdg in widgets)
         {
            CtrlDockTabbed.MthControlRemove(wdg);
            wdg.OnTitleChange -= Wdg_OnTitleChange;
         }
      }

      public void MthWidgetBring2Front(GateDockWidgetCtrl widget)
      {
         if (PpWidgets.Contains(widget)) { CtrlDockTabbed.PpTabVisible = widget; }
      }

      private GateDockWidgetStateFlags myGetStateFromAnchor(DockableAreaCtrlSlotAnchorModeEnum anchor)
      {
         switch (anchor)
         {
            case DockableAreaCtrlSlotAnchorModeEnum.left: return GateDockWidgetStateFlags.group_left;
            case DockableAreaCtrlSlotAnchorModeEnum.right: return GateDockWidgetStateFlags.group_right;
            case DockableAreaCtrlSlotAnchorModeEnum.up: return GateDockWidgetStateFlags.group_up;
            case DockableAreaCtrlSlotAnchorModeEnum.down: return GateDockWidgetStateFlags.group_down;
            default: throw new Crash();
         }
      }

      private void MyMainFrm_OnSkinChange(object? sender, GateDockSkin? skin) => PpSkin = skin;

      private void Wdg_OnTitleChange(object? sender, EventArgs e)
      {
         var wdg = sender as GateDockWidgetCtrl ?? throw new Crash();
         var but = CtrlDockTabbed.MthGetButton(wdg);

         but.PpText = wdg.PpTitle;
      }
   }
}
