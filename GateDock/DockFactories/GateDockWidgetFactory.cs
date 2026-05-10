using Gate.Dock.DockApp;
using Gate.Dock.DockWidget;
using Gate.ToolsView.MenuCommand;
using System;
using System.Windows.Forms;

namespace Gate.Dock.DockFactories
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class GateDockWidgetFactory : GateDockCtrlFactory
   {
      private Lazy<Cmd> myLazyCommand;
      private Lazy<GateDockWidgetCtrl> myLazyWidget;

      public GateDockWidgetFactory(GateDockApp app)
      {
         myLazyCommand = new Lazy<Cmd>(myMakeCommand);
         myLazyWidget = new Lazy<GateDockWidgetCtrl>(myMakeWidgetInflate);
         App = app;
      }

      /// <summary>
      /// 
      /// </summary>
      public abstract string MenuCmdId { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract string CmdCaption { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract string WidgetTitle { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract GateDockWidgetStateFlags DefaultState { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract Keys ShortCut { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract Keys ShortCut2 { get; }

      /// <summary>
      /// 
      /// </summary>
      public GateDockWidgetCtrl Widget => myLazyWidget.Value;

      /// <summary>
      /// 
      /// </summary>
      public Cmd Cmd => myLazyCommand.Value;

      /// <summary>
      /// 
      /// </summary>
      public GateDockApp App { get; }

      /// <summary>
      /// Factory method for widget.
      /// </summary>
      /// <returns></returns>
      protected abstract GateDockWidgetCtrl myMakeWidget();

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected virtual Cmd myMakeCommand()
      {
         var cmd = new Cmd(MenuCmdId, CmdCaption);

         cmd.ShortCut = ShortCut;
         cmd.ShortCut2 = ShortCut2;
         cmd.Action = new Action<Cmd>(c => App.MainForm.MthWidgetShow(Widget));

         return cmd;
      }

      protected virtual GateDockWidgetCtrl myMakeWidgetInflate()
      {
         var wdg = myMakeWidget();

         wdg.PpFactory = this;
         wdg.PpTitle = WidgetTitle;

         return wdg;
      }
   }
}

