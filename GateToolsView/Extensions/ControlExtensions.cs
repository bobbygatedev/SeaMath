using Gate.Tools.Multithread;
using Gate.ToolsView.ControlFeature;
using Gate.ToolsView.ControlFeature.Extensions;
using Gate.ToolsView.Native;

namespace Gate.ToolsView.Extensions
{
   /// <summary>
   /// Extensions for control.
   /// </summary>
   public static class ControlExtensions
   {
      /// <summary>
      /// Thread safe <see cref="Control.Invoke(Delegate)"/>.
      /// </summary>
      /// <param name="control"></param>
      /// <param name="method"></param>
      /// <param name="args"></param>
      /// <returns></returns>
      public static object? MthInvoke(this Control control, Delegate method, params object[] args)
      {
         object? res = null;

         if (control.InvokeRequired)
         {
            var cur_thr = Thread.CurrentThread;
            var css = null as CriticalSection[];

            lock (CriticalSection.CriticalSections)
            {
               css = CriticalSection.CriticalSections;
            }

            //all critical sections locking this thread 
            var thr_css = css.Where(t => t.LockingThread == cur_thr).ToArray();

            try
            {
               //invoke inside control message queue
               control.Invoke(new Action(() => res = myMessageLooplInvokeWrapper(thr_css, method, args)));
            }
            catch(ThreadInterruptedException)
            {
               res = method.DynamicInvoke(args);
            }
            catch (ObjectDisposedException)
            {
               res = method.DynamicInvoke(args);
            }
         }
         else
         {
            res = method.DynamicInvoke(args);
         }

         return res;
      }

      /// <summary>
      /// Thread safe <see cref="Control.Invoke(Delegate)"/>.
      /// </summary>
      /// <param name="control"></param>
      /// <param name="function"></param>
      /// <returns></returns>
      public static object? MthInvoke(this Control control, Func<object?> function) => MthInvoke(control, (Delegate)function);

      /// <summary>
      /// Thread safe <see cref="Control.Invoke(Delegate)"/>.
      /// </summary>
      /// <param name="control"></param>
      /// <param name="action"></param>
      public static void MthInvoke(this Control control, Action action) => MthInvoke(control, (Delegate)action);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="control"></param>
      /// <param name="action"></param>
      /// <returns></returns>
      public static IAsyncResult? MthBeginInvoke(this Control control, Action action)
      {
         if (control.InvokeRequired) { return control.BeginInvoke(action); }
         else
         {
            action.Invoke();

            return null;
         }
      }

      /// <summary>
      /// All recursively sub controls of <paramref name="control"/>.  
      /// </summary>
      /// <param name="control"></param>
      /// <returns></returns>
      public static Control[] MthGetNephews(this Control control)
      {
         var chs = control.Controls.OfType<Control>().ToArray();

         return chs.Concat(chs.SelectMany(c => c.MthGetNephews())).ToArray();
      }

      /// <summary>
      /// All recursively sub controls of <paramref name="control"/> plus <paramref name="control"/>.  
      /// </summary>
      /// <param name="control"></param>
      /// <returns></returns>
      public static Control[] MthGetNephewsAndMe(this Control control) => new[] { control }.Concat(control.MthGetNephews()).ToArray();

      /// <summary>
      /// First instance of all <see cref="MthGetNephews(Control)"/> of type <typeparamref name="CTRL"/>.
      /// </summary>
      /// <typeparam name="CTRL"></typeparam>
      /// <param name="control"></param>
      /// <returns></returns>
      public static CTRL? MthGetNephew<CTRL>(this Control control) where CTRL : Control =>
         MthGetNephews(control).FirstOrDefault(c => c is CTRL) as CTRL;

      /// <summary>
      /// Calls <see cref="Control.SuspendLayout"/> for <paramref name="control"/> and all its nephews (<see cref="MthGetNephews(Control)"/>.
      /// </summary>
      /// <param name="control"></param>
      public static void MthSuspendLayoutNephews(this Control control)
      {
         foreach (var ctr in control.MthGetNephews())
         {
            ctr.SuspendLayout();
         }

         control.SuspendLayout();
      }

      /// <summary>
      /// Calls <see cref="Control.ResumeLayout()"/> for <paramref name="control"/> and all its nephews (<see cref="MthGetNephews(Control)"/>.
      /// </summary>
      /// <param name="control"></param>
      public static void MthResumeLayoutNephews(this Control control)
      {
         foreach (var ctr in control.MthGetNephews())
         {
            ctr.ResumeLayout();
         }

         control.ResumeLayout();
      }

      /// <summary>
      /// Returns parent hierarchy excluding child itself eg {control.Parent , control.Parent.Parent , .. }.
      /// </summary>
      /// <param name="control"></param>
      /// <returns></returns>
      public static Control[] MthGetAnchestors(this Control control)
      {
         var lst_ctr = new List<Control>();
         var par = control.Parent;

         while (par != null)
         {
            lst_ctr.Add(par);
            par = par.Parent;
         }

         return lst_ctr.ToArray();
      }

      /// <summary>
      /// Returns first instamce of a certain Control type from <seealso cref="MthGetAnchestor{CTRL}(Control)"/> 
      /// </summary>
      /// <typeparam name="CTRL"></typeparam>
      /// <param name="control"></param>
      /// <returns></returns>
      public static CTRL? MthGetAnchestor<CTRL>(this Control control) where CTRL : Control =>
         MthGetAnchestors(control).FirstOrDefault(c => c is CTRL) as CTRL;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="control"></param>
      /// <returns></returns>
      public static Form? MthGetParentForm(this Control control) => control is Form ? control as Form : control.MthGetAnchestor<Form>();

      /// <summary>
      /// Alternative (more effectve) method for <see cref="Control.BringToFront"/>
      /// </summary>
      /// <param name="control"></param>
      public static void MthBringToFront(this Control control) => NativeMethods.SetForegroundWindow(control.Handle);

      /// <summary>
      /// Wraps the given control in a new Form, sets the form's client size to the control's preferred size,
      /// adds the control to the form, and docks the control to fill the form.
      /// Useful for displaying a control as a dialog.
      /// </summary>
      /// <param name="control"></param>
      ///<param name="areOkCancelButtonToAdd"></param> 
      /// <returns></returns>
      public static Form GetDialogFormBased(this Control control, bool areOkCancelButtonToAdd = true)
      {
         var frm = new Form();

         if (areOkCancelButtonToAdd)
         {
            var cs = control.GetPreferredSize(control.Size);
            var fea = frm.AddFeature<ControlFeatureOkCancel>();

            var but_ok = new Button();
            var but_cnc = new Button();

            frm.Controls.Add(but_ok);
            frm.Controls.Add(but_cnc);
            frm.Controls.Add(control);
            but_ok.Text = "&Ok";
            but_ok.Location = new Point(0, cs.Height);
            but_cnc.Text = "&Cancel";
            but_cnc.Location = new Point(but_ok.Width + 5, cs.Height);
            fea.ButtonOk = but_ok;
            fea.ButtonCancel = but_cnc;
            frm.ClientSize = new Size(Math.Max(cs.Width, but_cnc.Right + 5), cs.Height + 50);
         }
         else
         {
            frm.ClientSize = control.GetPreferredSize(control.Size);
            frm.Controls.Add(control);
         }

         frm.FormBorderStyle = FormBorderStyle.FixedDialog;
         frm.MaximizeBox = frm.MinimizeBox = false;
         control.Dock = DockStyle.Fill;

         return frm;
      }

      private static object? myMessageLooplInvokeWrapper(CriticalSection[] threadsCriticalSection, Delegate method, object[] args)
      {
         var res = null as object;
         var cur_thr = Thread.CurrentThread;

         foreach (var cs in threadsCriticalSection)
         {
            cs.AddAllowedThread(cur_thr);
         }

         res = method.DynamicInvoke(args);

         foreach (var cs in threadsCriticalSection)
         {
            cs.RemoveAllowedThread(cur_thr);
         }

         return res;
      }
   }
}
