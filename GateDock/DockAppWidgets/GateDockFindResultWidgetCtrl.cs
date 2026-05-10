using Gate.Dock.DockApp;
using Gate.Dock.DockDocu;
using Gate.Dock.DockFactories;
using Gate.Dock.DockSkin;
using Gate.Dock.DockTab;
using Gate.Dock.DockWidget;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.ToolsView.Extended;
using Gate.ToolsView.Extensions;
using Gate.ToolsView.MenuCommand;
using Gate.ToolsView.TextCtrl;
using Gate.ToolsView.TextSearch;
using ScintillaNET;
using System.Text;

namespace Gate.Dock.DockAppWidgets
{
   /// <summary>
   ///  
   /// </summary>
   public partial class GateDockFindResultWidgetCtrl : GateDockWidgetCtrl
   {
      private GateTextControl myCtrlMessages = new GateTextControl();
      private readonly InnerTextOpener myTextOpener;
      private Msg[]? myTokenMessages = null;

      /// <summary>
      /// Constructor.
      /// </summary>
      public GateDockFindResultWidgetCtrl()
      {
         InitializeComponent();

         PpBody = myCtrlMessages;
         myCtrlMessages.PpIsReadOnly = true;
         myTextOpener = new InnerTextOpener(this);
         myCtrlMessages.PpScintilla.DoubleClick += Scintilla_DoubleClick;
         myCtrlMessages.PpScintilla.Margins.Capacity = 0;
         myCtrlMessages.PpIsLabelInfoVisible = false;
         myCtrlMessages.PpScintilla.CaretStyle = CaretStyle.Invisible;
         myCtrlMessages.PpScintilla.CaretLineVisible = true;
         myCtrlMessages.PpScintilla.CaretLineBackColor = GateDockSkin.DefaultValues.FindWindowLineHighlight;
         PpSkinChildCtrlDispacther = new InnerSkinDispatcher(this);
      }

      public class Factory : GateDockWidgetFactory
      {
         public Factory(GateDockApp app) : base(app) { }

         public override string CmdCaption => "Find";

         public override string WidgetTitle => "Find";

         public override string ContentDescriptor => "List of Find files result";

         public override string CtrlGuid => "{488EF3FC-FE06-474D-A677-07F0198D4D4C}";

         public override string MenuCmdId => "Gate.Widget.Find";

         public override GateDockWidgetStateFlags DefaultState => GateDockWidgetStateFlags.dock_down;

         public override Keys ShortCut => Keys.None;

         public override Keys ShortCut2 => Keys.None;

         public override void AddExtraMenus(CmdContainer cmdContainer) { }

         protected override GateDockWidgetCtrl myMakeWidget() => new GateDockFindResultWidgetCtrl();
      }

      private class InnerSkinDispatcher : GateDockWidgetSkinDispacther
      {
         public InnerSkinDispatcher(GateDockFindResultWidgetCtrl parent) : base(parent) { }

         protected class FindUpdateVisitor : UpdateViUpdateWidgetControlsVisitor
         {
            public virtual void Visit(GateDockSkin skin, GateTextControl control)
            {
               base.Visit(skin, control);
               control.PpScintilla.CaretLineBackColor = skin.Params.FindWindowLineHighlight.Value;
            }
         }

         private FindUpdateVisitor myFindUpdateVisitor = new FindUpdateVisitor();

         protected override void myUpdateWidgetControls(GateDockSkin skin, Control control) => myFindUpdateVisitor.Visit(skin, (dynamic)control);
      }

      private class InnerTextOpener : ITextOpener
      {
         public InnerTextOpener(GateDockFindResultWidgetCtrl parent) => ParentFindResultWidgetCtrl = parent;

         public GateDockFindResultWidgetCtrl ParentFindResultWidgetCtrl { get; }

         public void Open(Msg msg)
         {
            if (msg.Token != null)
            {
               var fnd_tok = msg.Token.Tag as TextSearchToken;
               var mai_frm = ParentFindResultWidgetCtrl.PpMainFrm;

               if (fnd_tok?.FindFile.FilePath == "")
               {
                  // when found is not associated to any valid path:
                  // searches for the doc associated to find instance the select the text
                  if (mai_frm?.PpTabPagesAll.Any(d => d.MthGetNephews().Contains(fnd_tok.FindFile.OpenTextControl)) ?? false)
                  {
                     var txt_ctr = fnd_tok.FindFile.OpenTextControl as GateTextControl;
                     var doc_ctr = txt_ctr?.MthGetAnchestor<GateDockTabPageCtrl>() as IGateDockDocuText;

                     doc_ctr?.MthSelectTokenFromOtherControl(fnd_tok.TxtToken);
                  }
               }
               else
               {
                  var doc_ctr = 
                     mai_frm?.PpDocuHandler.OpenPath(
                        ParentFindResultWidgetCtrl.PpMainFrm.NnOrCrash(), 
                        (fnd_tok?.FindFile.FilePath).ExtTrim()) as IGateDockDocuText;

                  if (fnd_tok?.TxtToken != null)
                  {
                     doc_ctr?.MthSelectTokenFromOtherControl(fnd_tok.TxtToken);
                  }
               }
            }
         }
      }

      public int PpCurrentIndex { get; private set; } = -1;

      /// <summary>
      /// 
      /// </summary>
      public MsgCollection PpMsgCollection { get; private set; } = new MsgCollection();

      public void MthAddInfo(string info) => MthAddMsg(new Msg(MsgType.info, info, null, null));

      public void MthAddMsg(params Msg[] messages)
      {
         var sb = new StringBuilder();

         PpMsgCollection.Add(messages);

         foreach (var msg in messages) { sb.AppendLine(msg.MsgTxt); }

         myCtrlMessages.PpIsReadOnly = false;
         myCtrlMessages.PpScintilla.AppendText(sb.ToString());
         myCtrlMessages.PpIsReadOnly = true;
         myCtrlMessages.PpScintilla.CaretLineVisibleAlways = true;
      }

      public void MthClear()
      {
         myCtrlMessages.PpIsReadOnly = false;
         myCtrlMessages.PpContentText = "";
         myCtrlMessages.PpIsReadOnly = true;
         PpMsgCollection.Clear();
         PpCurrentIndex = -1;
         myTokenMessages = null;
         myCtrlMessages.PpScintilla.CaretLineVisibleAlways = false;
      }

      public void MthGoToNext()
      {
         if (PpMsgCollection.Any(m => m.Token != null))
         {
            myTokenMessages = myTokenMessages ?? PpMsgCollection.Where(m => m.Token != null).ToArray();

            if (++PpCurrentIndex >= myTokenMessages.Length) { PpCurrentIndex = 0; }

            myTextOpener.Open(myTokenMessages[PpCurrentIndex]);
            myCtrlMessages.PpCurrLine = PpCurrentIndex + 2;
            myCtrlMessages.PpCurrCol = 1;
         }
      }

      public void MthGoToPrevious()
      {
         if (PpMsgCollection.Any(m => m.Token != null))
         {
            myTokenMessages = myTokenMessages ?? PpMsgCollection.Where(m => m.Token != null).ToArray();

            if (--PpCurrentIndex < 0) { PpCurrentIndex = myTokenMessages.Length - 1; }

            myTextOpener.Open(myTokenMessages[PpCurrentIndex]);
            myCtrlMessages.PpCurrLine = PpCurrentIndex + 2;
            myCtrlMessages.PpCurrCol = 1;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="findTokens"></param>
      public void MthAddFindTokens(TextSearchToken[] findTokens)
      {
         var mgs = findTokens.Select(fnd_tok =>
         {
            var tok = fnd_tok.TxtToken;
            var ln = tok?.Store?[tok?.From?.Line ?? -1].Content;
            var txt_ctr = fnd_tok.FindFile.OpenTextControl as GateTextControl;
            var tab_ctr = txt_ctr != null ? txt_ctr.MthGetAnchestor<GateDockTabPageCtrl>() as IGateDockDocuText : null;
            var pth = fnd_tok.FindFile.FilePath != "" ? fnd_tok.FindFile.FilePath : tab_ctr?.PpDocuName;
            var msg_txt = $"{pth}({tok?.From?.Line},{tok?.From?.Col}): {ln}";
            var msg = new Msg(MsgType.info, msg_txt, null, tok);

            (msg.Token.NnOrCrash()).Tag = fnd_tok;

            return msg;
         }).ToArray();

         MthAddMsg(mgs);
      }

      private void Scintilla_DoubleClick(object? sender, DoubleClickEventArgs e)
      {
         if (e.Line >= 0 && e.Line < PpMsgCollection.Count)
         {
            myTextOpener.Open(PpMsgCollection[e.Line]);
         }
      }
   }
}

