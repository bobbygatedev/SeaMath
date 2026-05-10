using Gate.Tools.Extensions;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Gate.Tools.Message
{
   /// <summary>
   /// todo
   /// </summary>
   public class MsgLoggerNLogImpl : IMsgLogger
   {
      public const string NL = "${newLine}";
      public const string DEFAULT_LAYOUT = @"${longdate}|${level:uppercase=true}|${message}" +
                  "${onexception:Raised '${exception:format=type}'" + NL +
                  "${exception:format=StackTrace}" + NL +
                  "${exception:format=message}" + NL +
                  "}";
      public const string LOG_FILE_NAME = "LogFile";

      private Logger? myLogger;
      private string? myLogPath;

      public MsgLoggerNLogImpl()
      {
         LogManager.Configuration = new LoggingConfiguration();

         myCreateLoggerConsole();
      }

      public string? LogPath
      {
         get => myLogPath;

         set
         {
            if (!myLogPath.IsBlank())
            {
               myDestroyFileLogger();
            }

            myLogPath = value;

            if (!myLogPath.IsBlank())
            {
               myCreateFileLogger(myLogPath??"");
            }
         }
      }

      private void myDestroyFileLogger()
      {
         var log_cfg = new LoggingConfiguration();



         var x = LogManager.Configuration;
      }

      public string Layout { get; set; } = DEFAULT_LAYOUT;

      private void myCreateLoggerConsole()
      {
         var tgt_con = new ConsoleTarget
         {
            Name = "Console",
            Layout = Layout,
         };

         var log_cfg = new LoggingConfiguration();

         log_cfg.AddRule(LogLevel.Trace, LogLevel.Off, tgt_con, "*");
         LogManager.Configuration = log_cfg;
         myLogger = LogManager.GetCurrentClassLogger();
      }

      private void myCreateFileLogger(string logPath)
      {
         var tgt_fil = new FileTarget
         {
            Name = LOG_FILE_NAME,
            Layout = Layout,
            FileName = logPath,
         };

         var rul = new LoggingRule("*", LogLevel.Trace, LogLevel.Off, tgt_fil);

         rul.RuleName = "File";

         LogManager.Configuration?.LoggingRules.Add(rul);
         myLogger = LogManager.GetCurrentClassLogger();
      }

      public void AddMsg(Msg msg)
      {
         switch (msg.MsgType)
         {
            case MsgType.info:
            case MsgType.success:
               myLogger?.Info(msg.FullMessage);
               break;

            case MsgType.warning:
               myLogger?.Warn(msg.FullMessage);
               break;

            case MsgType.fail:
            case MsgType.violation:
            case MsgType.fatal:
               myLogger?.Error(msg.FullMessage);
               break;

            default: throw new Crash();
         }
      }

      public void AddText(string text) => AddMsg(new Msg(MsgType.info, text));

      public void FatalException(Exception exc) => myLogger?.Fatal(exc, "");
   }
}