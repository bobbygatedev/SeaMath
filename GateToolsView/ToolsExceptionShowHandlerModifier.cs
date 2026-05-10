using Gate.Tools;
using System;

namespace Gate.ToolsView
{
   public class ToolsExceptionShowHandlerModifier : IToolsExceptionShowHandlerModifier
   {
      public ToolsExceptionShowHandlerModifier()
      {

      }

      /// <summary>
      /// Show an exception form (with the indication of the state of stack etc ), and allows the user to specify the title of it.
      /// </summary>
      /// <param name="exception">The exception to be show in exception form.</param>
      /// <param name="dbgExceptionFormTitle">Title of the exception form.</param>
      public static void ExcShowByForm(Exception exception, string dbgExceptionFormTitle)
      {
         //trick for be intercepted by debugger when exception break is active
         try { throw exception; }
         catch (Exception exc)
         {
            if (exc is TypeInitializationException te)
            {
               var exc_frm = new ToolsExceptionForm(ToolsExceptionForm.ShowFlag.debug_mode, te, dbgExceptionFormTitle);

               exc_frm.ShowDialog();
            }
            else
            {
               var exc_frm = new ToolsExceptionForm(ToolsExceptionForm.ShowFlag.debug_mode, exc, dbgExceptionFormTitle);

               exc_frm.ShowDialog();
            }
         }
      }

      public ToolExceptionShowHandler GetOverridenShowHandler() => ExcShowByForm;
   }
}
