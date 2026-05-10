using Gate.Dock.DockAppMenu;
using Gate.Dock.DockDocu;
using Gate.Dock.DockTab;
using Gate.ToolsView.MenuCommand;
using Gate.ToolsView.TextCtrl.Lexers;
using static Gate.ToolsView.TextCtrl.GateTextControl;

namespace Gate.Dock.DockFactories.Text
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class GateDockCtrlFactoryDocuText : GateDockDocuFactory
   {
      /// <summary>
      /// 
      /// </summary>
      public abstract GateTextLexer Lexer { get; }

      /// <summary>
      /// Name of the language (no format constraints eg 'C/C++').
      /// </summary>
      public abstract string LanguageName { get; }

      /// <summary>
      /// Language id (format constraints varname like eg 'CPP').
      /// </summary>
      public abstract string LanguageId { get; }

      public override string ContentDescriptor => $"{LanguageName} File";

      public override string CtrlGuid => "{CB63BC74-055D-40A3-A1C3-2ED7A3083E2D}";

      protected sealed override GateDockTabPageCtrl myMakeTabPageControl() => new GateDockDocuTextCtrl();

      public abstract string MenuContextId { get; }

      public abstract FoldZoneGetter? FoldZoneGetter { get; }

      public virtual GateDockDocuMarkerBreakpoint.Positioner BreakpointPositioner => new GateDockDocuMarkerBreakpoint.Positioner.Standard();

      public override void AddExtraMenus(CmdContainer cmdContainer)
      {
         var ctx_men = new CmdMenu(true, MenuContextId);

         cmdContainer.AllMenus.Add(ctx_men);
         ctx_men.AddCmdIdsRange(
            true,
            CmdCommonlyUsedIds.COPY,
            CmdCommonlyUsedIds.CUT,
            CmdCommonlyUsedIds.PASTE,
            GateDockAppMenuBuilderFile.CMD_UNDO,
            GateDockAppMenuBuilderFile.CMD_REDO,
            GateDockAppMenuBuilderEdit.CMD_SELECT_ALL);
      }
   }
}

