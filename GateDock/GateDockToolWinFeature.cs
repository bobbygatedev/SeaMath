using Gate.Dock.DockSkin;
using Gate.Tools;
using Gate.ToolsView.ControlFeature;
using Gate.ToolsView.ControlFeature.Extensions;

namespace Gate.Dock
{
   /// <summary>
   /// <br> Implements behaviour of a toolwin associated to GateDock form:</br>
   /// <br> - Associate a <see cref="GateDockSkinChildCtrlDispatcher"/> to from, which causes skin is applied</br>
   /// <br> - Shotcut behaviour:</br>
   /// <br>    1) Shift+ESC causes toolwin close </br>
   /// <br>    2) Forward shrtcut to main form except in these cases:</br>
   /// <br>    3) -Ctrl+C,Ctrl+V,Ctrl+X form is Modal/Dialog Box</br>
   /// </summary>
   public class GateDockToolWinFeature : CtrlFeature.Specialized<Form>
   {
      public GateDockSkinChildCtrlDispatcher? SkinDispatcher { get; set; }

      public GateDockMainForm? MainForm { get; private set; }

      protected override void myOnControlAssociate(Control control)
      {
         var frm = (Form)control;

         frm.AddFeature<FormFeatureKeyDownObserver>().OnKeyDown += GateDockToolWinFeature_OnKeyDown;
         frm.VisibleChanged += Frm_VisibleChanged;
         frm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
         SkinDispatcher = new GateDockSkinChildCtrlDispatcher(control);
      }

      private void GateDockToolWinFeature_OnKeyDown(object? sender, KeyEventArgs e)
      {
         if (
            BoundControl.Modal || //then form is modal (ShowDialog())
            e.KeyData == (Keys.Control | Keys.A) ||
            e.KeyData == (Keys.Control | Keys.C) ||
            e.KeyData == (Keys.Control | Keys.V) ||
            e.KeyData == (Keys.Control | Keys.X) ||
            !e.Control && e.Alt && !e.Shift)
         {
            return;//skip forwarding
         }
         else if (!e.Control && !e.Alt && !e.Shift && e.KeyCode == Keys.Escape)
         {
            //Shift+ESC causes toolbox close
            e.Handled = true;
            e.SuppressKeyPress = true;
            BoundControl.DialogResult = DialogResult.Cancel;
            BoundControl.Close();
         }
         else if (e.Handled = MainForm?.PpCmdMainMenu?.HandleKeyForShortcuts(e.KeyData, true) ?? false)
         {
            //forward shortcut handling to main form
            e.SuppressKeyPress = true;
         }
      }

      protected override void myOnControlDeassociate(Control control) => ((Form)control).VisibleChanged -= Frm_VisibleChanged;

      private void Frm_VisibleChanged(object? sender, EventArgs e)
      {
         var frm = sender as Form ?? throw new Crash();

         if (frm.Owner is GateDockMainForm mai_frm)
         {
            MainForm = mai_frm;

            if (SkinDispatcher!= null)
            {
               SkinDispatcher.Skin = mai_frm.PpSkin;
            }
         }
         else
         {
            throw new Crash($"Form shall have owner of type {typeof(GateDockMainForm).Name}");
         }
      }
   }
}
