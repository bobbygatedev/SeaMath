using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Gate.ToolsView.Extended
{
   /// <summary>
   /// Input box form with (optionally) validator.
   /// </summary>
   public partial class InputBoxForm : Form
   {
      private Validator myValidator = new AlwaysTrueValidator();
      private string myQuestionText = "";
      private Label? myLabel = null;

      /// <summary>
      /// Constructor.
      /// </summary>
      public InputBoxForm()
      {
         InitializeComponent();
         TheToolTip.ShowAlways = true;
      }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="validator">Validtor for this box.</param>
      public InputBoxForm(Validator validator)
      {
         InitializeComponent();
         TheToolTip.ShowAlways = true;
         MthSetValidator(validator);
      }

      /// <summary>
      /// Override this class for implemening validtor behviour.
      /// </summary>
      public abstract class Validator
      {
         public delegate bool ValidatorHandler(string text, out string tooltipErrorMessage);

         private class InnerFromDelegate : Validator
         {
            public InnerFromDelegate(ValidatorHandler validatorHandler) => Validator = validatorHandler;

            public ValidatorHandler Validator { get; }

            public override bool IsValid(string text, out string tooltipErrorMessage) => Validator(text, out tooltipErrorMessage);
         }

         /// <summary>
         /// Returns true if the text is valid.
         /// </summary>
         /// <param name="text"></param>
         /// <returns></returns>
         public abstract bool IsValid(string text, out string tooltipErrorMessage);

         public static Validator FromDelegate(ValidatorHandler validator) => new InnerFromDelegate(validator);
      }

      /// <summary>
      /// Default validtor, always returns true.
      /// </summary>
      public class AlwaysTrueValidator : Validator
      {
         /// <summary>
         /// Returns true if the text is valid.
         /// </summary>
         /// <param name="text"></param>
         /// <returns></returns>
         public override bool IsValid(string text, out string tooltipErrorMessage)
         {
            tooltipErrorMessage = "";

            return true;
         }
      }

      /// <summary>
      /// Abstract class for validtor by regex.
      /// </summary>
      public abstract class ValidatorWithRegexAbstract : Validator
      {
         /// <summary>
         /// Regex instance used for filtering.
         /// </summary>
         public abstract Regex Regex { get; }

         /// <summary>
         /// Override for implementing behaviour of tool tip message generation.
         /// </summary>
         /// <param name="text"></param>
         /// <returns></returns>
         public abstract string GetErrorMessage(string text);

         /// <summary>
         /// Implementation of abstract method.
         /// </summary>
         /// <param name="text"></param>
         /// <param name="tooltipErrorMessage"></param>
         /// <returns></returns>
         public override bool IsValid(string text, out string tooltipErrorMessage)
         {
            var match = Regex.Match(text);

            if (match.Success && match.Length == text.Length)
            {
               tooltipErrorMessage = "";

               return true;
            }
            else
            {
               tooltipErrorMessage = GetErrorMessage(text);

               return false;
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public class ValidatorWithRegexSimple : ValidatorWithRegexAbstract
      {
         private readonly Regex myRegex;
         private readonly string myMessageInCaseOfError;

         /// <summary>
         /// Constructor.
         /// </summary>
         /// <param name="regex">The regex to be used.</param>
         /// <param name="fixedMsgFormatInCaseOfError">Fixed message in case of error(Specifiyng {0} the input value will be shown).</param>
         public ValidatorWithRegexSimple(Regex regex, string fixedMsgFormatInCaseOfError)
         {
            myRegex = regex;
            myMessageInCaseOfError = fixedMsgFormatInCaseOfError;
         }

         /// <summary>
         /// Regex instance used for filtering (implementation).
         /// </summary>
         public override Regex Regex => myRegex;

         /// <summary>
         /// Returns the (fixed) error message.
         /// </summary>
         /// <param name="text"></param>
         /// <returns></returns>
         public override string GetErrorMessage(string text) => string.Format(myMessageInCaseOfError, text);
      }

      /// <summary>
      /// Validtor class for type with parse
      /// </summary>
      public class ValidatorWithParsableType : Validator
      {
         private object? myLastValidationParseResult = null;
         private Type myTypeWithParse;
         private MethodInfo? myParseMethodInfo = null;

         /// <summary>
         /// Type is checked for a parse method, if check fails fatal handling.
         /// </summary>
         /// <param name="typeWithParse"></param>
         public ValidatorWithParsableType(Type typeWithParse)
         {
            var mts = typeWithParse.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod);

            foreach (var mth_inf in mts)
            {
               if (mth_inf.Name == "Parse")
               {
                  if (mth_inf.ReturnType == typeWithParse)
                  {
                     var parameters = mth_inf.GetParameters();

                     if (parameters.Length == 1 && parameters[0].Attributes == ParameterAttributes.None && parameters[0].ParameterType == typeof(string))
                     {
                        myParseMethodInfo = mth_inf;
                        myTypeWithParse = typeWithParse;

                        return;
                     }
                  }
               }
            }

            throw new Gate.Tools.Crash(-1, string.Format("Type {0}({1}) doesn't have a valid static [Type].Parser(string) method."));
         }

         /// <summary>
         /// Result of parsing during call of method 'isValid'.
         /// </summary>
         public object? LastValidationParseResult => myLastValidationParseResult;

         /// <summary>
         /// Implementation of isValid.
         /// </summary>
         /// <param name="text"></param>
         /// <param name="tooltipErrorMessage"></param>
         /// <returns></returns>
         public override bool IsValid(string text, out string tooltipErrorMessage)
         {
            try
            {
               myLastValidationParseResult = Parse(text);
               tooltipErrorMessage = "";

               return true;
            }
            catch
            {
               tooltipErrorMessage = $"{text} is not valid as an {myTypeWithParse.Name} value.";

               return false;
            }
         }
         /// <summary>
         /// Performs a parse of the text, using the parser of the parsable type.
         /// </summary>
         /// <param name="text"></param>
         /// <returns></returns>
         public object? Parse(string text) => myParseMethodInfo?.Invoke(null, [text]);
      }

      /// <summary>
      /// 
      /// </summary>
      public class ValidatorForFilePath : Validator
      {
         /// <summary>
         /// 
         /// </summary>
         /// <param name="text"></param>
         /// <param name="tooltipErrorMessage"></param>
         /// <returns></returns>
         public override bool IsValid(string text, out string tooltipErrorMessage)
         {
            if (File.Exists(text))
            {
               tooltipErrorMessage = "";

               return true;
            }
            else
            {
               tooltipErrorMessage = $"Not a file at '{text}'!";

               return false;
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public class ValidatorForDirPath : Validator
      {
         /// <summary>
         /// 
         /// </summary>
         /// <param name="text"></param>
         /// <param name="tooltipErrorMessage"></param>
         /// <returns></returns>
         public override bool IsValid(string text, out string tooltipErrorMessage)
         {
            if (Directory.Exists(text))
            {
               tooltipErrorMessage = "";

               return true;
            }
            else
            {
               tooltipErrorMessage = $"Not a file at '{text}'!";

               return false;
            }
         }
      }

      /// <summary>
      ///  the state of the option of text trimming.
      /// </summary>
      public bool PpIsTextToBeTrimmed { get; set; } = true;

      /// <summary>
      /// Validator ( by default an always true validtor
      /// </summary>
      public Validator PpValidator => myValidator;

      /// <summary>
      /// Option accept empty text 
      /// </summary>
      public bool PpIsEmptyTextToBeAccepted { get; set; } = false;

      /// <summary>
      ///  the text showed in input box.
      /// </summary>
      public string PpUserInputText
      {
         get => PpIsTextToBeTrimmed ? TheTextInputUser.Text.Trim() : TheTextInputUser.Text;
         set => TheTextInputUser.Text = value;
      }

      /// <summary>
      ///  the 
      /// </summary>
      public string PpQuestionText
      {
         get => myQuestionText;
         set
         {
            myQuestionText = (value != null) ? value : "";

            TheTableLayoutPanel.Dock = DockStyle.None;
            TheTableLayoutPanel.SuspendLayout();
            TheTableLayoutPanel.Controls.Clear();
            TheTableLayoutPanel.Size = new Size(0, 0);

            if (myQuestionText != "")
            {
               myLabel = new Label();
               myLabel.AutoSize = true;
               myLabel.Text = myQuestionText;
               myLabel.Dock = DockStyle.Fill;

               TheTableLayoutPanel.RowCount = 3;

               TheTableLayoutPanel.Controls.Add(myLabel, 0, 1);
               TheTableLayoutPanel.Controls.Add(TheTextInputUser, 0, 2);
               TheTableLayoutPanel.Controls.Add(TheDialogButtons, 0, 3);
            }
            else
            {
               myLabel = null;
               TheTableLayoutPanel.RowCount = 2;
               TheTableLayoutPanel.Controls.Add(TheTextInputUser, 0, 1);
               TheTableLayoutPanel.Controls.Add(TheDialogButtons, 0, 2);
            }

            TheTableLayoutPanel.ResumeLayout();
            TheTableLayoutPanel.Refresh();

            ClientSize = TheTableLayoutPanel.Size;
            MinimumSize = Size;
            TheTableLayoutPanel.Dock = DockStyle.Fill;
         }
      }

      /// <summary>
      /// Set a new validtor for the class a validtor.
      /// </summary>
      /// <param name="newValidator"></param>
      /// <returns></returns>
      public Validator MthSetValidator(Validator newValidator)
      {
         if (newValidator != null)
         {
            var res = myValidator;

            myValidator = newValidator;

            return res;
         }
         else { throw new Gate.Tools.ToolsException("The validtor can't be null!"); }
      }

      /// <summary>
      /// Override for changing behaviour.
      /// </summary>
      /// <param name="errorText"></param>
      protected virtual void myActionOnValidatorError(string errorText) => myShowToolTip((errorText != null && errorText != "") ? errorText : "Validation failed!");

      private void InputBox_FormClosing(object? sender, FormClosingEventArgs e)
      {
         string err_txt;

         if (!PpIsEmptyTextToBeAccepted & PpUserInputText.Trim() == "" & DialogResult == DialogResult.OK)
         {
            e.Cancel = true;
            myShowToolTip("The input text can be empty.");
         }
         else if (DialogResult == DialogResult.OK && !myValidator.IsValid(PpUserInputText, out err_txt))
         {
            e.Cancel = true;

            myActionOnValidatorError(err_txt);
         }
      }

      private void myShowToolTip(string toolTipText)
      {
         TheToolTip.SetToolTip(TheTextInputUser, toolTipText);
         TheToolTip.Show(toolTipText, this, 2500);
      }

      private void TheTextInputUser_Validated(object? sender, EventArgs e)
      {
         if (!myValidator.IsValid(PpUserInputText, out var err_txt)) { myActionOnValidatorError(err_txt); }
      }

      public static DialogResult[] Test()
      {
         var lst_res = new List<DialogResult>();

         {
            var inp_box = new InputBoxForm();

            inp_box.Text = "Test Caption";
            inp_box.PpIsEmptyTextToBeAccepted = false;
            inp_box.PpQuestionText = "Question to be made.\nIs Very Important";

            lst_res.Add(inp_box.ShowDialog());
         }

         {
            var inp_box = new InputBoxForm();

            inp_box.Text = "Test Caption";
            inp_box.PpIsEmptyTextToBeAccepted = false;
            inp_box.MthSetValidator(new InputBoxForm.ValidatorWithRegexSimple(new Regex(@"\w+"), "Not valid a simple word!"));

            lst_res.Add(inp_box.ShowDialog());
         }

         {
            var inp_box = new InputBoxForm();

            inp_box.Text = "Test Caption";
            inp_box.PpQuestionText = "Put an integer value";
            inp_box.MthSetValidator(new InputBoxForm.ValidatorWithParsableType(typeof(int)));
            lst_res.Add(inp_box.ShowDialog());
         }

         return lst_res.ToArray();
      }
   }
}