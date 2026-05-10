using Gate.Tools;
using System;
using System.Windows.Forms;

namespace Gate.ToolsView
{
   /// <summary>
   /// Showing theException form.  
   /// Exception.Message is howed always. Other fiels such nested exception button stack trace , site , class_name.
   /// </summary>
   public partial class ToolsExceptionForm : Form
   {
      private const int EXCEPTION_TYPE_FULL_NAME_ROW_INDEX = 1;
      private const int EXCEPTION_TEST_SITE_ROW_INDEX = 2;
      private const int EXCEPTION_STACK_TRACE_ROW_INDEX = 3;
      private const int EXCEPTION_INNER_BUTTON_ROW_INDEX = 4;

      /// <summary>
      /// Describes the way the form is shown.
      /// </summary>
      [Flags]
      public enum ShowFlag
      {
         /// <summary>
         /// A button will be visible to display nested theException form
         /// </summary>
         show_nested_exception_button = 0x1,

         /// <summary>
         /// A Message box with the stack trace at the time the theException is raised will be displayed.
         /// </summary>
         show_stack_trace = 0x2,

         /// <summary>
         /// A Message box with the site of the theException will be displayed.
         /// </summary>
         show_site = 0x4,

         /// <summary>
         /// A Message box with the full theException class name will be displayed.
         /// </summary>
         show_class_name = 0x8,

         /// <summary>
         /// Composite flag for debug purpose ( all flag enabled) 
         /// </summary>
         debug_mode = show_nested_exception_button | show_stack_trace | show_site | show_class_name,

         /// <summary>
         /// By default just error Message is displayed.
         /// </summary>
         err_msg_mode = 0x0
      }

      private Exception? myException;
      private String myTitle;

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="aShowFlag">Flags for show mode.</param>
      /// <param name="aExceptionToShow">Exception to show.</param>
      public ToolsExceptionForm(ShowFlag aShowFlag, Exception aExceptionToShow)
      {
         InitializeComponent();

         PpShowFlag = aShowFlag;
         myException = aExceptionToShow;
         myTitle = (PpShowFlag != ShowFlag.err_msg_mode) ? $"Raised: {myException.GetType().Name}" : "Error Raised";
      }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="aShowFlag"></param>
      /// <param name="aTypeInitializationException"></param>
      public ToolsExceptionForm(ShowFlag aShowFlag, TypeInitializationException aTypeInitializationException)
      {
         InitializeComponent();

         PpShowFlag = aShowFlag;
         myException = aTypeInitializationException.InnerException;
         myTitle =
            $"{aTypeInitializationException.InnerException?.GetType().Name} during initialisation of {aTypeInitializationException.TypeName}";
      }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="aShowFlag">Flags for show mode.</param>
      /// <param name="aExceptionToShow"></param>
      /// <param name="aTitle"></param>
      public ToolsExceptionForm(ShowFlag aShowFlag, Exception aExceptionToShow, string aTitle)
      {
         InitializeComponent();

         PpShowFlag = aShowFlag;
         myException = aExceptionToShow;
         myTitle = aTitle;
      }

      /// <summary>
      /// Show flag.
      /// </summary>
      public ShowFlag PpShowFlag { get; }

      protected override void OnShown(EventArgs e)
      {
         base.OnShown(e);
         BringToFront();
      }

      private void myDoInit()
      {
         Text = myTitle;

         TheTextErrorMsg.Text = myException?.Message;
         TheTextStackTrace.Text = myException?.StackTrace;
         TheTextExceptionTypeFullName.Text = myException?.GetType().FullName;

         if (myException?.TargetSite != null) { TheTextSite.Text = myException.TargetSite.ToString(); }

         if (myException?.InnerException != null)
         {
            TheButtonNested.Visible = true;
            TheButtonNested.Text = "Show inner " + myException.InnerException.GetType().FullName;
         }
         else { TheButtonNested.Visible = false; }

         myDoApplyFlags();
      }

      private void myDoApplyFlags()
      {
         TheTextExceptionTypeFullName.Visible = ((PpShowFlag & ShowFlag.show_class_name) != 0);
         TheTextSite.Visible = ((PpShowFlag & ShowFlag.show_site) != 0);
         TheTextStackTrace.Visible = ((PpShowFlag & ShowFlag.show_stack_trace) != 0);
         TheButtonNested.Visible = (myException?.InnerException != null) && ((PpShowFlag & ShowFlag.show_nested_exception_button) != 0);

         if (!TheTextExceptionTypeFullName.Visible)
         {
            TheTableLayoutPanel.RowStyles[EXCEPTION_TYPE_FULL_NAME_ROW_INDEX].Height = 0;
         }

         if (!TheTextSite.Visible)
         {
            TheTableLayoutPanel.RowStyles[EXCEPTION_TEST_SITE_ROW_INDEX].Height = 0;
         }

         if (!TheTextStackTrace.Visible)
         {
            TheTableLayoutPanel.RowStyles[EXCEPTION_STACK_TRACE_ROW_INDEX].Height = 0;
         }

         if (!TheButtonNested.Visible)
         {
            TheTableLayoutPanel.RowStyles[EXCEPTION_INNER_BUTTON_ROW_INDEX].Height = 0;
         }
      }

      public static void DoTest()
      {
         var exc = new Exception("Mau");
         var frm = new ToolsExceptionForm(ShowFlag.debug_mode, exc);

         frm.ShowDialog();
      }

      private void GExceptionForm_Load(object? sender, EventArgs e) { myDoInit(); }

      private void TheButtonNested_Click(object? sender, EventArgs e)
      {
         var nst_frm = new ToolsExceptionForm(
            PpShowFlag, myException?.InnerException ?? throw new Crash(), $"Nested exception to {myException?.GetType().FullName}");

         nst_frm.ShowDialog(this);
      }
   }
}