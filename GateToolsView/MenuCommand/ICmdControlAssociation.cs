using System;
using System.Drawing;
using System.Windows.Forms;

namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// Interface for command association (with menu,button) strategy.
   /// </summary>
   public interface ICmdControlAssociation
   {
      /// <summary>
      /// 
      /// </summary>
      Cmd Command { get;}
      
      /// <summary>
      /// 
      /// </summary>
      bool IsEnabled { get; set; }

      /// <summary>
      /// 
      /// </summary>
      bool IsVisible { get; set; }

      /// <summary>
      /// 
      /// </summary>
      bool? IsChecked { get; set; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="action"></param>
      void SetAction(Action<Cmd>? action);
      
      /// <summary>
      /// 
      /// </summary>
      /// <param name="cmdCaption"></param>
      void SetCmdCaption(string? cmdCaption);
      
      /// <summary>
      /// 
      /// </summary>
      /// <param name="image"></param>
      void SetImage(Image? image);
      
      /// <summary>
      /// 
      /// </summary>
      /// <param name="shortCut"></param>
      /// <param name="shortCut2"></param>
      void SetShortCutPair(Keys shortCut, Keys shortCut2);
   }
}
