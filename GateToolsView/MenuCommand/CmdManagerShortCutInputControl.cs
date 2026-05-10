using Gate.Tools;
using Gate.ToolsView.Native;

namespace Gate.ToolsView.MenuCommand
{
   public partial class CmdManagerShortCutInputControl : UserControl
   {
      private InnerAdHocTextBox myTextBox;
      private ShortCutPair? myShortCutPair = null;
      private int myPhase = 0;

      public delegate void OnShortCutChangedHandler(object? sender, ShortCutPair? shortCutPair);

      public event OnShortCutChangedHandler? OnShortCutChanged;

      public CmdManagerShortCutInputControl()
      {
         InitializeComponent();

         myTextBox = new InnerAdHocTextBox();
         myTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
         myTextBox.Location = new System.Drawing.Point(0, 0);
         myTextBox.Name = "CtrlTextBox";
         myTextBox.ReadOnly = true;
         Controls.Add(myTextBox);
         myTextBox.Select();
      }

      private class InnerAdHocTextBox : TextBox
      {
         private Keys myKeys = Keys.None;

         public CmdManagerShortCutInputControl? ParentCtrl => Parent as CmdManagerShortCutInputControl;

         protected override void WndProc(ref Message m)
         {
            var is_pro = true;
            var is_eve = false;

            var msg = (WinMsgEnum)m.Msg;
            var key = Keys.None;

            switch (msg)
            {
               case WinMsgEnum.WM_KEYDOWN:
                  key = myTranslateKey(m.WParam);
                  myKeys |= key;
                  is_eve = true;
                  break;

               case WinMsgEnum.WM_KEYUP:
                  key = myTranslateKey(m.WParam);
                  myKeys &= ~key;
                  is_eve = true;
                  break;

               case WinMsgEnum.WM_SYSKEYDOWN:
                  key = myTranslateKey(m.WParam);
                  myKeys |= key;
                  is_pro = false;
                  is_eve = true;
                  break;

               case WinMsgEnum.WM_SYSKEYUP:
                  key = myTranslateKey(m.WParam);
                  myKeys &= ~key;
                  is_pro = false;
                  is_eve = true;
                  break;
            }

            if (is_eve) { ParentCtrl?.myDoProcess(myKeys); }

            if (is_pro) { base.WndProc(ref m); }
         }

         private Keys myTranslateKey(IntPtr wParam)
         {
            var key = (Keys)wParam.ToInt32();

            switch (key)
            {
               case Keys.Menu: return Keys.Alt;
               case Keys.ControlKey: return Keys.Control;
               case Keys.LShiftKey:
               case Keys.RShiftKey:
               case Keys.ShiftKey: return Keys.Shift;
               default: return key;
            }
         }
      }

      private void myDoProcess(Keys keys)
      {
         if (myPhase == 0)
         {
            if ((keys & (Keys.Control | Keys.Alt)) != 0 && (keys & ~(Keys.Control | Keys.Alt | Keys.Shift)) != 0)
            {
               if (PpShortCutPair != null)
               {
                  PpShortCutPair.Key1 = keys;
                  PpShortCutPair.Key2 = Keys.None;
               }

               myTextBox.Text = PpShortCutPair?.Text;
               myPhase = 1;
               OnShortCutChanged?.Invoke(this, PpShortCutPair);
            }
         }
         else if (myPhase == 1)
         {
            if ((keys & ~(Keys.Control | Keys.Alt | Keys.Shift)) != 0)
            {
               if (PpShortCutPair != null)
               {
                  PpShortCutPair.Key2 = keys;
               }

               myTextBox.Text = PpShortCutPair?.Text;
               myPhase = 0;
               OnShortCutChanged?.Invoke(this, PpShortCutPair);
            }
         }
         else { throw new Crash(); }
      }

      public ShortCutPair? PpShortCutPair
      {
         get => myShortCutPair;

         set
         {
            if (value != null)
            {
               myShortCutPair = (ShortCutPair)value.Clone();
               myTextBox.Text = myShortCutPair?.Text;
            }
            else
            {
               myShortCutPair = null;
               myTextBox.Text = "";
            }
         }
      }
   }
}
