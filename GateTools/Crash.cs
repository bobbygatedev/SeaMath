using System;
using System.Diagnostics;
using System.Threading;

namespace Gate.Tools
{
   /// <summary>
   /// <br> Crash handling, the call of contructor causes application exit with exception window show. </br>
   /// <br> Using eg 'throw new gl.hladectool.Tools.Crash()' to emphatize application exit.</br>
   /// </summary>
   public class Crash : ToolsException
   {
      /// <summary>
      /// Constructor.
      /// </summary>
      public Crash() : base("Generic Crash!")
      {
         StackTrace = new StackTrace(true).ToString();
         myExcShow();
         Environment.Exit(-1);
      }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="message"></param>
      /// <param name="args"></param>
      public Crash(string message) : base(message)
      {
         StackTrace = new System.Diagnostics.StackTrace(true).ToString();
         myExcShow();
         Environment.Exit(-1);
      }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="exitCode"></param>
      /// <param name="message"></param>
      public Crash(int exitCode, string message) : base(message)
      {
         StackTrace = new StackTrace(true).ToString();
         myExcShow();
         Environment.Exit(exitCode);
      }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="innerException"></param>
      public Crash(Exception? innerException)
         : base("Crash with internal exception", innerException)
      {
         StackTrace = new StackTrace(true).ToString();
         myExcShow();
         Environment.Exit(-1);
      }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="innerException"></param>
      public Crash(int exitCode, Exception? innerException)
         : base("Crash with internal exception", innerException)
      {
         StackTrace = new StackTrace(true).ToString();
         myExcShow();
         Environment.Exit(exitCode);
      }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="message"></param>
      /// <param name="innerException"></param>
      public Crash(string message, Exception innerException)
         : base(message, innerException)
      {
         StackTrace = new StackTrace(true).ToString();
         myExcShow();
         Environment.Exit(-1);
      }

      /// <summary>
      /// Calling stack trace.
      /// </summary>
      public override string StackTrace { get; }

      private void myExcShow() => ExcShow($"Crash in thread managed id={Thread.CurrentThread.ManagedThreadId}");
   }
}
