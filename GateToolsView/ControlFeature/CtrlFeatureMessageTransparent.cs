using Gate.ToolsView.Native;
using System;
using System.Windows.Forms;

namespace Gate.ToolsView.ControlFeature
{
   /// <summary>
   /// <br> Makes a control/form message transparent.</br>
   /// <br> An event transparent control passes its window messages to underlying control/form.</br>
   /// </summary>
   public class CtrlFeatureMessageTransparent : CtrlFeatureByWndHook
   {      
      /// <summary>
      /// Implements a class which estabilish which part of the control 
      /// </summary>
      public abstract class TransparentToMsgCheckHandler
      {
         /// <summary>
         /// Returns always true: all control client area is transparent to windows messages.
         /// </summary>
         public class Always : TransparentToMsgCheckHandler
         {
            public override bool IsTransparent(int screenX, int screenY, CtrlFeatureMessageTransparent featureTransparent) => true;
         }

         /// <summary>
         /// Returns always false: all control client area is opaque  to windows messages.
         /// </summary>
         public class Never : TransparentToMsgCheckHandler
         {
            public override bool IsTransparent(int screenX, int screenY, CtrlFeatureMessageTransparent featureTransparent) => false;
         }

         /// <summary>
         /// Returns true if a certain point of control client area (passed as form relative x,y) is transparent to windows messages. 
         /// </summary>
         /// <param name="screenX"></param>
         /// <param name="screenY"></param>
         /// <param name="featureTransparent"></param>
         /// <returns>True is transparent to windows messages</returns>
         public abstract bool IsTransparent(int screenX, int screenY, CtrlFeatureMessageTransparent featureTransparent);
      }

      /// <summary>
      /// 
      /// </summary>
      public override Type? SpecificControlType => null;

      /// <summary>
      /// Windows messages transparent check handler, which states which part inside the control are transparent to windows messages.
      /// </summary>
      public TransparentToMsgCheckHandler TransparentToMsgCheck { get; set; } = new TransparentToMsgCheckHandler.Always();

      protected override void myOnControlAssociate(Control control) { }

      protected override void myOnControlDeassociate(Control control) { }

      protected unsafe override bool myWndProc(ref Message msg)
      {
         switch ((WinMsgEnum)msg.Msg)
         {
            case WinMsgEnum.WM_NCHITTEST:
               var px = (int)msg.LParam;
               var r_x = ((ushort*)&px)[0];
               var r_y = ((ushort*)&px)[1];

               if (TransparentToMsgCheck.IsTransparent(r_x, r_y, this))
               {
                  msg.Result = (IntPtr)HitTestValuesEnum.HTTRANSPARENT;

                  return false;
               }

               return true;

            default: return true;
         }
      }
   }
}

