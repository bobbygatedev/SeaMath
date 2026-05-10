using Gate.Tools;
using Gate.Tools.AppParams;
using Gate.Tools.AppParams.ValueControls;
using Gate.ToolsView.Extended;
using System.Text;

namespace Gate.ToolsView.AppParams.ValueControls
{
   public partial class ValueDropDownByChoiceControl : CtrlWithLabel, IValueControl
   {
      private object? myValue;
      private object[]? myChoiceItems;

      public event OnValueChangeHandler? OnValueChange;

      public ValueDropDownByChoiceControl()
      {
         InitializeComponent();
      }

      /// <summary>
      /// Provides a drop-down control for selecting a text encoding.
      /// </summary>
      /// <remarks>The <see cref="ForEncoding"/> class displays a list of available <see
      /// cref="System.Text.Encoding"/> options for user selection. By default, it includes all encodings available on
      /// the system. Derived classes can override the available choices, such as restricting to single-byte
      /// encodings.</remarks>
      public class ForEncoding : ValueDropDownByChoiceControl
      {
         public ForEncoding()
         {
            PpToStringHandler = ci => (ci as Encoding)?.EncodingName ?? "";
            PpChoiceItems = myGetEncodings();
         }

         /// <summary>
         /// Single byte encodings only.
         /// </summary>
         [ValueControlAssociation(Id = ValueControlStandardId.encoding_single_byte)]
         public class SingleByte : ForEncoding
         {
            public SingleByte() { }

            protected override Encoding[] myGetEncodings() => EncodingHelper.OrderedEncodingsSingleByte;
         }

         protected virtual Encoding[] myGetEncodings() => EncodingHelper.OrderedEncodings;
      }

      public object[] PpChoiceItems
      {
         get => myChoiceItems ?? [];

         set
         {
            myChoiceItems = value;
            CtrlCombo.Items.Clear();

            if (myChoiceItems != null)
            {
               var hnd = PpToStringHandler != null ? PpToStringHandler : (o) => o?.ToString() ?? "";

               foreach (var itm in myChoiceItems) { CtrlCombo.Items.Add(hnd(itm)); }

               if (myChoiceItems.Length > 0) { ParamValue = myChoiceItems[0]; }

               myDoAdjustDropDownWidth();
            }
         }
      }

      /// <summary>
      /// Delegate which convert drop down content to dropdown label 
      /// </summary>
      public Func<object?, string>? PpToStringHandler { get; set; }

      public object? ParamValue
      {
         get => myValue;

         set
         {
            myValue = value;
            CtrlCombo.SelectedIndex = value != null && PpChoiceItems != null ? PpChoiceItems.ToList().IndexOf(value) : -1;
         }
      }

      void IValueControl.ActionOnAppParamAssociationAction(AppParam param) { }

      /// <summary>
      /// Adjusts the drop-down width to fit the widest item.
      /// </summary>
      /// <param name="combo"></param>
      private void myDoAdjustDropDownWidth()
      {
         var mw = CtrlCombo.DropDownWidth;

         using (var gr = CtrlCombo.CreateGraphics())
         {
            foreach (var itm in CtrlCombo.Items)
            {
               var wd = (int)gr.MeasureString(itm.ToString(), CtrlCombo.Font).Width;

               if (wd > mw)
               {
                  mw = wd;
               }
            }
         }

         CtrlCombo.DropDownWidth = mw + 10; // + padding
      }

      private void CtrlCombo_SelectedIndexChanged(object? sender, EventArgs e)
      {
         myValue = CtrlCombo.SelectedIndex >= 0 && CtrlCombo.SelectedIndex < PpChoiceItems.Length ? PpChoiceItems[CtrlCombo.SelectedIndex] : null;
         OnValueChange?.Invoke(this, myValue);
      }
   }
}
