using Gate.Tools;
using Gate.Tools.AppParams;
using Gate.Tools.Arry;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.ToolsView.Extended;
using System.Globalization;
using System.Text;

namespace Gate.ToolsView.AppParams.ValueControls
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class ValueControlFactory
   {
      public class EnumLabelAttribute : Attribute
      {
         public string? Label { get; set; }
      }

      public class ForEnum : ValueControlFactory
      {
         public override bool IsTypeValid(Type type) => type.IsSubclassOf(typeof(Enum));

         public override IValueControl MakeValueControl(Type type)
         {
            var res = new ValueDropDownByChoiceControl();

            res.PpToStringHandler = l =>
            {
               if ((l as Enum)?.GetAttribute<EnumLabelAttribute>() is EnumLabelAttribute atr)
               {
                  return (atr?.Label).Nn();
               }
               else
               {
                  return (l?.ToString()).Nn();
               }
            };
            res.PpChoiceItems = new ArrayMultidimensional<object>(Enum.GetValues(type)).AllItems;

            return res;
         }
      }

      public class ForCultureInfo : ValueControlFactory
      {
         public override bool IsTypeValid(Type type) => type == typeof(CultureInfo);

         public override IValueControl MakeValueControl(Type type)
         {
            var res = new ValueDropDownByChoiceControl();

            res.PpToStringHandler = ci => (ci as CultureInfo)?.EnglishName ?? "";
            res.PpChoiceItems = CultureInfo.GetCultures(CultureTypes.NeutralCultures).OrderBy(c => c.EnglishName).ToArray();

            return res;
         }
      }

      public class ForString : ValueControlFactory
      {
         public class WithConverter<T> : ValueControlFactory
         {
            public WithConverter(TxtStringConverter? stringConverter = null) => StringConverter = stringConverter ?? new TxtStringConverter.Default();

            public override bool IsTypeValid(Type type) => type == typeof(T);

            public TxtStringConverter StringConverter { get; }

            public override IValueControl MakeValueControl(Type type)
            {
               var res = new ValueStringControl();

               res.PpToStringHandler = StringConverter.ToStr;
               res.PpTextConverter = (string str, out object? val) => StringConverter.TryParse(str, type, out val);

               return res;
            }
         }

         public override bool IsTypeValid(Type type) => type == typeof(string);

         public override IValueControl MakeValueControl(Type type) => new ValueStringControl();
      }

      public class ForBool : ValueControlFactory
      {
         private class InnerCheckBox : CheckBox, IValueControl
         {
            public event OnValueChangeHandler? OnValueChange;

            public object? ParamValue
            {
               get => Checked;

               set
               {
                  var old_val = Checked;

                  Checked = value is bool val && val;

                  if (old_val != Checked) { OnValueChange?.Invoke(this, Checked); }
               }
            }

            public string ParamName { get => Text; set => Text = value; }

            protected override void OnCheckedChanged(EventArgs e)
            {
               base.OnCheckedChanged(e);

               OnValueChange?.Invoke(this, Checked);
            }

            void IValueControl.ActionOnAppParamAssociationAction(AppParam param) { }
         }

         public override bool IsTypeValid(Type type) => type == typeof(bool);

         public override IValueControl MakeValueControl(Type type) => new InnerCheckBox();
      }

      public class ForFont : ValueControlFactory
      {
         private class InnerControl : CtrlWithLabel, IValueControl
         {
            private readonly Button myButton = new Button();
            private Font? myFont;

            public event OnValueChangeHandler? OnValueChange;

            public InnerControl()
            {
               PpBasedControl = myButton;
               myButton.Click += myActionOnButtonClick;
            }

            private void myActionOnButtonClick(object? sender, EventArgs e)
            {
               var fnt_dlg = new FontDialog();

               fnt_dlg.Font = PpFont ?? throw new Crash();

               if (fnt_dlg.ShowDialog() != DialogResult.Cancel)
               {
                  PpFont = fnt_dlg.Font;
                  OnValueChange?.Invoke(this, PpFont);
               }
            }

            public Font? PpFont
            {
               get => myFont;

               set
               {
                  myFont = value;
                  myButton.Text = myFont != null ? $"{myFont.Name},{myFont.Size}" : "";
                  PpFixedTextWidth = myButton.PreferredSize.Width;
               }
            }

            public object? ParamValue { get => PpFont; set => PpFont = value as Font; }

            void IValueControl.ActionOnAppParamAssociationAction(AppParam param) { }
         }

         public override bool IsTypeValid(Type type) => type == typeof(Font);

         public override IValueControl MakeValueControl(Type type) => new InnerControl();
      }

      public class ForEncoding : ValueControlFactory
      {
         public ForEncoding()
         {

         }

         public override bool IsTypeValid(Type type) => type == typeof(Encoding);

         public override IValueControl MakeValueControl(Type type) => new ValueDropDownByChoiceControl.ForEncoding();
      }

      public abstract IValueControl MakeValueControl(Type type);

      public abstract bool IsTypeValid(Type type);
   }
}
