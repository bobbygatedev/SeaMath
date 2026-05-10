using Gate.Tools;
using Gate.ToolsView.MenuCommand;

namespace Gate.ToolsView.Extended
{
   public partial class ExtendedColumnListViewControl
   {
      public abstract class EditorType
      {
         public abstract void Edit(CellType cell, string? text2Edit);

         public abstract class ByControl<CTR> : EditorType where CTR : Control, new()
         {
            public CellType? CellEmbeddeded { get; private set; }

            public CTR TextControl { get; } = new CTR();

            public override void Edit(CellType cell, string? text2Edit)
            {
               CellEmbeddeded = cell;
               myDoSetText(TextControl, text2Edit ?? cell.Text ?? "");
               cell.OnUnembed += Cell_OnUnembed;
               cell.EmbedTempControl(TextControl);
               myActionOnEmbed(TextControl, cell);
            }

            private void Cell_OnUnembed(CellType cell)
            {
               cell.OnUnembed -= Cell_OnUnembed;
               myActionOnUnembed(TextControl, cell);
            }

            protected abstract void myActionOnUnembed(CTR textControl, CellType cell);

            protected abstract void myDoSetText(CTR textBoxControl, string text);

            protected abstract string myGetCellText(CTR textBoxControl);

            protected abstract void myActionOnEmbed(CTR control, CellType cell);
         }

         public class ByComboBox : ByControl<CmdManagedByComboBox>
         {
            private KeyboardObserver myKeyboardObserver = new KeyboardObserver();

            public ByComboBox(bool isDropDown = false, Func<string[]>? alternativeGetter = null)
            {
               if (IsDropDown = isDropDown)
               {
                  TextControl.AutoCompleteMode = AutoCompleteMode.Suggest;
                  TextControl.DropDownStyle = ComboBoxStyle.DropDownList;
               }
               else
               {
                  TextControl.DropDownStyle = ComboBoxStyle.DropDown;
               }

               AlternativeGetter = alternativeGetter;
            }

            public bool IsDropDown { get; }
            
            public Func<string[]>? AlternativeGetter { get; }

            protected override void myDoSetText(CmdManagedByComboBox textBoxControl, string text)
            {
               myGetAlternatives(textBoxControl);
               textBoxControl.Text = text;
            }

            private void myGetAlternatives(CmdManagedByComboBox textBoxControl)
            {
               if (AlternativeGetter != null)
               {
                  var als = AlternativeGetter.Invoke();

                  if (als != null)
                  {
                     var old_its = textBoxControl.Items.Cast<object>().Select(i => i.ToString()).ToArray();

                     if (!als.SequenceEqual(old_its))
                     {
                        textBoxControl.Items.Clear();
                        textBoxControl.Items.AddRange(als);
                     }
                  }
               }
            }

            protected override string myGetCellText(CmdManagedByComboBox textBoxControl) => textBoxControl.Text;

 
            protected override void myActionOnEmbed(CmdManagedByComboBox textControl, CellType cell)
            {
               if (IsDropDown)
               {
                  textControl.DropDownClosed += TextControl_DropDownClosed;
                  textControl.DropDown += TextControl_DropDown;
               }

               myGetAlternatives(textControl);
               myKeyboardObserver.Sender = textControl;
               myKeyboardObserver.OnKeyUp += TextControl_KeyUp;
               myKeyboardObserver.Start();
            }

            private void TextControl_DropDown(object? sender, EventArgs e) => myGetAlternatives(sender as CmdManagedByComboBox ?? throw new Crash());

            private void TextControl_KeyUp(object? sender, KeyEventArgs e)
            {
               if (e.KeyCode == Keys.Enter)
               {
                  myActionOnTextChanged(sender as CmdManagedByComboBox ?? throw new Crash(), CellEmbeddeded);
               }

               e.Handled = true;
               e.SuppressKeyPress = true;
            }

            private void TextControl_DropDownClosed(object? sender, EventArgs e) =>
               myActionOnTextChanged(sender as CmdManagedByComboBox ?? throw new Crash(), CellEmbeddeded);

            protected override void myActionOnUnembed(CmdManagedByComboBox textControl, CellType cell)
            {
               textControl.DropDown -= TextControl_DropDown;
               textControl.DropDownClosed -= TextControl_DropDownClosed;
               myKeyboardObserver.OnKeyUp -= TextControl_KeyUp;
               myKeyboardObserver.Stop();
            }

            private void myActionOnTextChanged(CmdManagedByComboBox textControl, CellType? cell)
            {
               var ars = new UpdateCellTextArgs();
               var txt = myGetCellText(textControl);

               ars.Text2Set = txt;
               
               if (cell != null)
               {
                  cell.ParentListView?.myActionOnCellTextUpdating(cell, ars);
                  cell.UnembedTempControl();
               }
            }
         }

         public class Default : ByControl<CmdManagedByTextBox>
         {
            public Default() { }

            protected override void myActionOnEmbed(CmdManagedByTextBox textControl, CellType cell) => textControl.KeyUp += TextControl_KeyUp;

            private void TextControl_KeyUp(object? sender, KeyEventArgs e)
            {
               var txt_ctr = sender as CmdManagedByTextBox ?? throw new Crash();

               if (e.KeyData == Keys.Enter)
               {
                  var ars = new UpdateCellTextArgs();
                  var txt = myGetCellText(txt_ctr);

                  ars.Text2Set = txt;
                  CellEmbeddeded?.ParentListView?.myActionOnCellTextUpdating(CellEmbeddeded, ars);
                  CellEmbeddeded?.UnembedTempControl();
                  e.SuppressKeyPress = true;
                  e.Handled = true;
               }
            }


            protected override void myActionOnUnembed(CmdManagedByTextBox textControl, CellType cell) =>
               textControl.KeyUp -= TextControl_KeyUp;

            protected override void myDoSetText(CmdManagedByTextBox textBoxControl, string text) => textBoxControl.Text = text;

            protected override string myGetCellText(CmdManagedByTextBox textBoxControl) => textBoxControl.Text;
         }
      }
   }
}
