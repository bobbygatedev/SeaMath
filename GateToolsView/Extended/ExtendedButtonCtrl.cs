
namespace Gate.ToolsView.Extended
{
   public delegate void OnToggledChangeHandler(object? sender, bool? toggleValue);

   /// <summary>
   /// Button for use in menus and toolbar.
   /// </summary>
   public partial class ExtendedButtonCtrl : Button, IButtonToggable
   {
      private bool myIsToggled = false;
      private bool myIsToggleActive = false;

      public event OnToggledChangeHandler? OnToggledChange;

      public ExtendedButtonCtrl()
      {
         InitializeComponent();
         SetStyle(ControlStyles.Selectable, false);
         myActionOnEvaluateToggle();
      }

      /// <summary>
      /// 
      /// </summary>
      public bool PpIsToggleActive
      {
         get => myIsToggleActive;
         set
         {
            myIsToggleActive = value;
            myActionOnEvaluateToggle();
         }
      }

      /// <summary>
      ///  toggled state if not has value button is always flat(not toggled) otw button toggled(flat) is 'on' and up is 'off'
      /// </summary>
      public bool? IsToggled
      {
         get => PpIsToggleActive ? (bool?)myIsToggled : null;

         set
         {
            if (myIsToggleActive = value.HasValue) { myIsToggled = value ?? false; }

            myActionOnEvaluateToggle();
         }
      }

      /// <summary>
      /// Discard handling of mnemonics (eg &File -> Alt+F), since this is performed by CmdMainMenu.HandleKeyForShortcuts,
      /// this avoids the main menu mnemonics (eg &File->f &Edit ->e ,..) have not generated WM_CHAR events in MainForm sub controls 
      /// (the reason why has not been understood) 
      /// </summary>
      /// <param name="charCode"></param>
      /// <returns></returns>
      protected override bool ProcessMnemonic(char charCode) => false;

      protected virtual void myActionOnEvaluateToggle()
      {
         FlatStyle = myIsToggleActive && myIsToggled ? FlatStyle.Standard : FlatStyle.Flat;
         OnToggledChange?.Invoke(this, IsToggled);
      }

      private void ExtendedButtonCtrl_Click(object? sender, System.EventArgs e)
      {
         if (myIsToggleActive) { IsToggled = !myIsToggled; }
      }
   }
}
