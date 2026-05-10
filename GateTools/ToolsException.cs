using Gate.Tools.Message;

namespace Gate.Tools
{
   /// <summary>
   /// 
   /// </summary>
   /// <param name="exception"></param>
   /// <param name="dbgExceptionFormTitle"></param>
   public delegate void ToolExceptionShowHandler(Exception exception, string dbgExceptionFormTitle);

   /// <summary>
   /// Base class for all glibrary exception. Contains some utility (static methods).
   /// </summary>
   public class ToolsException : Exception
   {
      private static ToolExceptionShowHandler myShowHandler = ExcMessageShowConsole;

      static ToolsException()
      {
         var ass = AppDomain.CurrentDomain.GetAssemblies();
         var cls = ass.SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IToolsExceptionShowHandlerModifier)))).ToArray();

         myUpdateShowHandlerOverride(cls);

         AppDomain.CurrentDomain.AssemblyLoad += (sender, args) =>
         {
            var arr = new[] { args.LoadedAssembly };

            myUpdateShowHandlerOverride(args.LoadedAssembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IToolsExceptionShowHandlerModifier))).ToArray());
         };
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="messages"></param>
      public ToolsException(MsgCollection messages) : base(string.Join("\n", messages.Select(m => m.FullMessage))) => Messages = messages;

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="message"></param>
      public ToolsException(string message) : base(message) => Messages.Add(new Msg(MsgType.fail, message));

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="exception"></param>        
      public ToolsException(Exception exception) : base("Generic Exception Raised", exception) => Messages.Add(new Msg(MsgType.fatal, $"{exception.GetType().Name}: {exception.Message}"));

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="message"></param>
      /// <param name="innerException"></param>
      public ToolsException(string message, Exception? innerException) : base(message, innerException) => Messages.Add(new Msg(MsgType.fail, message));

      /// <summary>
      /// Constructor.
      /// </summary>
      public ToolsException() { }

      /// <summary>
      /// 
      /// </summary>
      public MsgCollection Messages { get; private set; } = new MsgCollection();

      /// <summary>
      /// Delegate for show an exception form (with the indication of the state of stack etc ), and allows the user to specify the title of it.
      /// </summary>
      public static ToolExceptionShowHandler ShowHandler
      {
         get => myShowHandler;
         set
         {
            if (value != null)
            {
               myShowHandler = value;
            }
         }
      }

      /// <summary>
      /// Gets the global override for the tool exception show handler, if any is set.
      /// </summary>
      /// <remarks>When this property is not null, it is used in place of the default exception show handler
      /// for tool operations. This property is intended for advanced scenarios where custom exception display logic is
      /// required.</remarks>
      public static ToolExceptionShowHandler? ShowHandlerOverride { get; private set; } = null;

      /// <summary>
      /// Show an exception form relative to this exception (with the indication of the state of stack etc ), and allows the user to specify the title of it.
      /// </summary>
      /// <param name="dbgExceptionFormTitle">Title of the exception form.</param>
      public void ExcShow(string dbgExceptionFormTitle) => ToolsException.ShowHandler(this, dbgExceptionFormTitle);

      /// <summary>
      /// Returns the stack state of the exception, formatted as a string.
      /// </summary>
      /// <param name="exception">The exception, that is requested of the stack.</param>
      /// <returns></returns>
      static public string StackTraceDescriptor(Exception exception) =>
             ((exception.TargetSite != null) ? ($"Source code in\r\n{exception.TargetSite}") : "") +
             ((exception.StackTrace != null) ? ($"\nStack Trace:\r\n{exception.StackTrace}") : "") +
             ((exception.InnerException != null) ?
             ("Inner Exception:\r\n" + StackTraceDescriptor(exception.InnerException)) : "");

      /// <summary>
      /// Returns a message containing the complete info of the exception ( message,stack,inner exception.. ).
      /// </summary>
      /// <param name="exception">The exception, that is requested of the complete messsage.</param>
      /// <returns></returns>
      public static string CompleteExcMsg(Exception exception)
      {
         var res = "";

         if (exception.Message != null && exception.Message != "") { res += exception.Message; }

         if (exception.InnerException != null && exception.InnerException.Message != null && exception.InnerException.Message != "")
         {
            res += "\r\nAddintional info:\r\n" + (exception.InnerException.Message);
         }

         res += "\r\n" + StackTraceDescriptor(exception);

         return res;
      }

      /// <summary>
      /// Writes a formatted exception message to the console, including a custom title and detailed exception
      /// information.
      /// </summary>
      /// <remarks>This method is intended for debugging or diagnostic purposes. The output includes both
      /// the provided title and the complete exception message, which may contain sensitive information. Do not use
      /// this method to display exception details in production environments.</remarks>
      /// <param name="exception">The exception to display. Cannot be null.</param>
      /// <param name="dbgExceptionFormTitle">The title to display above the exception details in the console output.</param>
      public static void ExcMessageShowConsole(Exception exception, string dbgExceptionFormTitle) => Console.WriteLine($"{dbgExceptionFormTitle}\n{CompleteExcMsg(exception)}");

      private static void myUpdateShowHandlerOverride(Type[] types)
      {
         if (types.Length != 0)
         {
            if (ShowHandlerOverride != null) { Console.WriteLine("Exception show handler already exists!"); }
            else
            {
               if (types.Length > 1) { Console.WriteLine("Multiple Exception show handlers!"); }

               myShowHandler = ShowHandlerOverride = myGetShowHandlerOverride(types[0]) ?? throw new Crash();
            }
         }
      }

      private static ToolExceptionShowHandler? myGetShowHandlerOverride(Type type)
      {
         var cst = type.GetConstructor([]);
         var new_hnd = null as ToolExceptionShowHandler;

         if (cst != null)
         {
            var ist = cst.Invoke([]) as IToolsExceptionShowHandlerModifier;

            new_hnd = ist?.GetOverridenShowHandler();
         }

         if (new_hnd == null)
         {
            Console.WriteLine("Can't create a new exception show handler");
         }

         return new_hnd;
      }
   }
}