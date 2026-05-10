using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.ToolsView.Extended
{
   /// <summary>
   /// Control where On, Ok, Cancel, .. buttons are selectable.
   /// </summary>
   public partial class CtrlStdButtonsControl : UserControl
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="buttonId"></param>
      public delegate void OnAnyButtonHandler(DialogResult buttonId);

      /// <summary>
      /// 
      /// </summary>
      public delegate void OnButton();

      /// <summary>
      ///  event on any button is pressed.
      /// </summary>
      public event OnAnyButtonHandler? OnAnyButton;

      /// <summary>
      ///  event on OK button is pressed.
      /// </summary>
      public event OnButton? OnOk;

      /// <summary>
      ///  event on ABORT button is pressed.
      /// </summary>
      public event OnButton? OnAbort;

      /// <summary>
      ///  event on CANCEL button is pressed.
      /// </summary>
      public event OnButton? OnCancel;

      /// <summary>
      ///  event
      /// </summary>
      public event OnButton? OnRetry;

      /// <summary>
      ///  event on IGNORE button is pressed.
      /// </summary>
      public event OnButton? OnIgnore;

      /// <summary>
      ///  event on YES button is pressed.
      /// </summary>
      public event OnButton? OnYes;

      /// <summary>
      ///  event on NO button is pressed.
      /// </summary>
      public event OnButton? OnNo;

      private SortedDictionary<DialogResult, Button> myDictionaryButtons = new SortedDictionary<DialogResult, Button>();
      private List<DialogResult> myListActiveButtonId = new List<DialogResult>();
      private bool myIsUsingDlgStdBehaviour = true;
      private bool myIsUsingStandardShortCut = true;
      private DialogResult myAcceptButton = DialogResult.None;
      private DialogResult myCancelButton = DialogResult.None;

      /// <summary>
      /// Constructor.
      /// </summary>
      public CtrlStdButtonsControl()
      {
         InitializeComponent();
      }

      /// <summary>
      ///  the button used as accept button.
      /// </summary>
      public DialogResult PpAcceptButton
      {
         get { return myAcceptButton; }
         set
         {
            myAcceptButton = value;

            if (ParentForm != null)
            {

               if (myDictionaryButtons.TryGetValue(value, out var but)) { ParentForm.AcceptButton = but; }
               else { ParentForm.AcceptButton = null; }
            }
         }
      }

      /// <summary>
      ///  the button used as cancel button.
      /// </summary>
      public DialogResult PpCancelButton
      {
         get { return myCancelButton; }
         set
         {
            myCancelButton = value;

            if (ParentForm != null)
            {

               if (myDictionaryButtons.TryGetValue(value, out var but)) { ParentForm.CancelButton = but; }
               else { ParentForm.CancelButton = null; }
            }
         }
      }

      /// <summary>
      /// <br> State of use standard shortcut.</br>
      /// <br> If true Ok/Cancel are used as accept/cancel button if they are previously created.</br>
      /// <br> Otherwise system tries with Yes/No.</br>
      /// </summary>
      public bool PpIsUsingStdShortCut
      {
         get { return myIsUsingStandardShortCut; }
         set
         {
            myIsUsingStandardShortCut = value;

            if (ParentForm != null)
            {
               if (value)
               {
                  if (MthIdIsButtonContained(DialogResult.OK)) { PpAcceptButton = DialogResult.OK; }

                  if (MthIdIsButtonContained(DialogResult.Cancel)) { PpCancelButton = DialogResult.Cancel; }

                  if (!MthIdIsButtonContained(DialogResult.OK) && !MthIdIsButtonContained(DialogResult.Cancel))
                  {
                     if (MthIdIsButtonContained(DialogResult.Yes)) { PpAcceptButton = DialogResult.Yes; }

                     if (MthIdIsButtonContained(DialogResult.No)) { PpAcceptButton = DialogResult.No; }
                  }
               }
               else
               {
                  if (myDictionaryButtons.ContainsKey(DialogResult.OK)) { ParentForm.AcceptButton = null; }

                  if (myDictionaryButtons.ContainsKey(DialogResult.Cancel)) { ParentForm.CancelButton = null; }
               }
            }
         }
      }

      /// <summary>
      /// Standard behaviour active flag.
      /// </summary>
      public bool PpIsUsingDlgStdBehaviour { get { return myIsUsingDlgStdBehaviour; } set { myIsUsingDlgStdBehaviour = value; } }

      /// <summary>
      /// List of active buttons.
      /// </summary>
      public DialogResult[] PpActiveButtonIds
      {
         get { return myListActiveButtonId.ToArray(); }
         set
         {
            CtrlLayout.SuspendLayout();
            myMthDoClearButtons();

            //Filter the values ( excludes non ) and double entries
            foreach (var button_id in value) { if (button_id != DialogResult.None) { myMthDoAddButton(button_id); } }

            // Perform the layout
            CtrlLayout.RowCount = 1;

            if (myDictionaryButtons.Count > 0)
            {
               CtrlLayout.ColumnCount = myDictionaryButtons.Count;

               int integer_percent = 100 / myDictionaryButtons.Count;

               // AddRange columns styles
               CtrlLayout.ColumnStyles.Clear();
               foreach (DialogResult button_id in myDictionaryButtons.Keys)
               {
                  myListActiveButtonId.Add(button_id);

                  CtrlLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, (float)integer_percent));
               }
            }
            //update shortcut state if any
            PpIsUsingStdShortCut = PpIsUsingStdShortCut;

            var tmp_but = null as Button;

            if (!PpIsUsingDlgStdBehaviour)
            {
               if (ParentForm != null && PpAcceptButton != DialogResult.None && myDictionaryButtons.TryGetValue(PpAcceptButton, out tmp_but))
               {
                  ParentForm.AcceptButton = tmp_but;
               }
               if (ParentForm != null && PpCancelButton != DialogResult.None && myDictionaryButtons.TryGetValue(PpCancelButton, out tmp_but))
               {
                  ParentForm.AcceptButton = tmp_but;
               }
            }

            CtrlLayout.ResumeLayout(true);
         }
      }

      /// <summary>
      /// Returns true if assembly certain button ( relative to dialog rsesult ).
      /// </summary>
      /// <param name="aResultButton"></param>
      /// <returns></returns>
      public bool MthIdIsButtonContained(DialogResult buttonId) { return myDictionaryButtons.ContainsKey(buttonId); }

      private void myMthDoClearButtons()
      {
         if (ParentForm != null)
         {
            ParentForm.AcceptButton = null;
            ParentForm.CancelButton = null;
         }

         foreach (var button in myDictionaryButtons.Values) { CtrlLayout.Controls.Remove(button); }

         myDictionaryButtons.Clear();
         myListActiveButtonId.Clear();
         CtrlLayout.ColumnCount = 1;
      }

      private Button myMthDoAddButton(DialogResult aDialogResult)
      {
         if (!myDictionaryButtons.ContainsKey(aDialogResult))
         {
            var new_but = new Button();

            new_but.Visible = true;
            new_but.Text = "&" + aDialogResult.ToString();
            new_but.Dock = DockStyle.Fill;
            new_but.Click += new EventHandler(myOnAnButtonPressed);
            new_but.Tag = aDialogResult;

            myDictionaryButtons[aDialogResult] = new_but;
            CtrlLayout.Controls.Add(new_but);

            return new_but;
         }
         else { return myDictionaryButtons[aDialogResult]; }
      }

      private void myOnAnButtonPressed(object? sender, EventArgs e)
      {
         var but = sender as Button ?? throw new Crash();
         var but_id = but.Tag.ConvertOrCrash<DialogResult>();

         if (PpIsUsingDlgStdBehaviour) { myMthDialogStandardBehaviour(but_id); }
         if (OnAnyButton != null) { OnAnyButton.Invoke(but_id); }

         switch (but_id)
         {
            case DialogResult.Abort:
               OnAbort?.Invoke();
               break;

            case DialogResult.Cancel:
               OnCancel?.Invoke();
               break;

            case DialogResult.Ignore:
               OnIgnore?.Invoke();
               break;

            case DialogResult.No:
               OnNo?.Invoke();
               break;

            case DialogResult.OK:
               OnOk?.Invoke();
               break;

            case DialogResult.Retry:
               OnRetry?.Invoke();
               break;

            case DialogResult.Yes:
               OnYes?.Invoke();
               break;

            default: break;
         }
      }

      private void myMthDialogStandardBehaviour(DialogResult buttonId)
      {
         (ParentForm ?? throw new Crash()).DialogResult = buttonId;

         switch (buttonId)
         {
            case DialogResult.Abort:
               Application.Exit();
               break;

            case DialogResult.Cancel:
               ParentForm.Close();
               break;

            case DialogResult.No:
               ParentForm.Close();
               break;

            case DialogResult.OK:
               ParentForm.Close();
               break;

            case DialogResult.Yes:
               ParentForm.Close();
               break;

            default: break;
         }
      }

      /// <summary>
      /// If the ParentForm has not been yet created the system wait for button load.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void DialogButtons_Load(object? sender, EventArgs e)
      {
         PpIsUsingStdShortCut = PpIsUsingStdShortCut;
         PpAcceptButton = PpAcceptButton;
         PpCancelButton = PpCancelButton;
      }
   }
}