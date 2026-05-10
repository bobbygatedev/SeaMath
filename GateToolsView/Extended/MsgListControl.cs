using Gate.Tools.Message;

namespace Gate.ToolsView.Extended
{
   /// <summary>
   /// <br>Control for the message list.</br> 
   /// <br>Double click of a referenced line causes the opening of the referenced document on referenced point.</br>
   /// </summary>
   public partial class MsgListControl : UserControl
   {
      private readonly List<Msg> myListMessages = new List<Msg>();

      /// <summary>
      /// Constructor.
      /// </summary>
      public MsgListControl()
      {
         InitializeComponent();
         CtrlLineControl.PpCurrentIdBackColor = Color.Empty;
      }

      /// <summary>
      /// 
      /// </summary>
      public Color PpCurrentBackColor => CtrlLineControl.PpCurrentIdBackColor; 

      /// <summary>
      /// 
      /// </summary>
      public bool PpIsInForegroundIfError { get; set; } = true;

      /// <summary>
      /// 
      /// </summary>
      public int PpSubMessageTab { get; set; } = 3;

      /// <summary>
      /// Instance of text opener(an internal/external application, that open the file marking the text message position eg Notepad++).
      /// </summary>
      public ITextOpener? PpTextOpener { get; set; } = null;

      /// <summary>
      /// Adds one or more text message(s).
      /// </summary>
      /// <param name="messages"></param>
      public void MthAddMsg(params Msg[] messages)
      {
         myListMessages.AddRange(messages);
         CtrlLineControl.MthLinesAddcolor(messages.Select(m => (m.FullMessage, m.MsgColor)).ToArray());
         CtrlLineControl.PpLineCurrentId = CtrlLineControl.PpLineCount - messages.Length + 1;
         myDoBringToFront(messages);
      }

      /// <summary>
      /// Clears the content.
      /// </summary>
      public void MthClear()
      {
         myListMessages.Clear();
         CtrlLineControl.MthClear();
      }

      /// <summary>
      /// Remove the oldest entries from the view.
      /// </summary>
      /// <param name="numEntriesToRemove">Number of the maybe_folder to be removed (This aEnumType is limited to size of control).</param>
      public void MthRemoveOldEntries(int numEntriesToRemove)
      {
         myListMessages.RemoveRange(myListMessages.Count - numEntriesToRemove, numEntriesToRemove);
         CtrlLineControl.MthLineRemoveRange(Enumerable.Range(0, numEntriesToRemove));
      }

      /// <summary>
      ///   <br>Does set the contro for using 'notepad++' as text file open application.</br>
      ///   <br>Notepad++ is read from env var 'NOTEPAD_PLUS_PLUS'</br>
      /// </summary>
      public void MthSetNppAsTextOpener() => PpTextOpener = NppRef.FromEnvVar().GetTextOpener();

      /// <summary>
      ///   <br>Does set the contro for using 'notepad++' as text file open application.</br>
      /// </summary>
      /// <param name="nppPath">Notepad ++ path to be used.</param>
      public void MthSetNppAsTextOpener(string nppPath)
      {
         if (File.Exists(nppPath))
         {
            var npp_ref = new NppRef(nppPath);

            PpTextOpener = npp_ref.GetTextOpener();
         }
      }

      private void myDoBringToFront(Msg[] messages)
      {
         if (
            PpIsInForegroundIfError &&
            messages.Any(
               m =>
                  m.MsgType == MsgType.fail ||
                  m.MsgType == MsgType.violation ||
                  m.MsgType == MsgType.fatal ||
                  m.MsgType == MsgType.success ))
         {
            CtrlLineControl.BringToFront();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="msg"></param>
      private void myDoOpenReference(Msg msg) => PpTextOpener?.Open(msg);

      private void CtrlLineControl_MouseDoubleClick(object? sender, MouseEventArgs e)
      {
         var msg = CtrlLineControl.PpLineCurrentId >= 1 ? myListMessages[CtrlLineControl.PpLineCurrentId - 1] : null;

         if (msg != null) { myDoOpenReference(msg); }
      }
   }
}