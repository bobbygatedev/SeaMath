using Gate.Tools;
using Gate.Tools.DesignPattern;
using Gate.Tools.Extensions;
using Gate.Tools.Multithread;
using Gate.Tools.Text;
using Gate.ToolsView.Extensions;
using System.Collections.Concurrent;
using System.Text;
using static Gate.ToolsView.ConIO.ConsoleInputKeyEventStroke;
using static Gate.ToolsView.ConIO.ConsoleTask;

namespace Gate.ToolsView.ConIO
{
   /// <summary>
   /// 
   /// </summary>
   /// <param name="consoleIo"></param>
   /// <param name="chars"></param>
   public delegate void ConsoleIoStdOutHandler(ConsoleIo consoleIo, (EventType, char?)[] chars);

   /// <summary>
   /// 
   /// </summary>
   /// <param name="consoleIo"></param>
   /// <param name="inputLine"></param>
   public delegate void ConsoleIoInputLineHandler(ConsoleIo consoleIo, string inputLine);

   /// <summary>
   /// <br> Encapsulates an end point for a console control. </br>
   /// <br> Provides input enqueing (of user keyboard inputs) and subscription for input forwarded and output capture. </br>
   /// </summary>
   public class ConsoleIo : BaseClassWithFinalizer
   {
      public const double LISTEN_SECS = 0.0;
      public const int QUEUE_CAP = 32 * 1024;

      /// <summary>
      /// 
      /// </summary>
      public const string DEFAULT_PROMPT_STRING = ">";

      public event ConsoleIoInputLineHandler? OnInputLineEnded;

      private readonly List<string> myListHistory = new List<string>();
      private readonly CriticalSection myCritSecOneOnlyReadLineClient = new CriticalSection("OneOnlyReadLineClient");

      private static int myCounter = 0;

      /// <summary>
      /// 
      /// </summary>
      private QueueSafeThread<(EventType, char?)> myQueueCharStdOut;
      private ConsoleInputKeyEventStroke? myConsoleInputKeyEventStroke;
      private ConcurrentQueue<(EventType eventType, char? extraChar)> myQueueKeyStrokeEvent =
         new ConcurrentQueue<(EventType eventType, char? extraChar)>();

      private bool myIsReadLineCancelProcedure = false;
      private int myCancelIoSignal = 0;
      private int myHistoryIdx = -1;
      private ConsoleCmdHintListForm? myHintForm = null;
      private bool myIsActive;
      private CriticalSection myStdinCs = new CriticalSection("Conio.StdIn");
      private ConsoleCmdHint.Handler? myHintHandler;

      public ConsoleIo(ConsoleTask task)
      {
         Task = task;
         myQueueCharStdOut = new QueueSafeThread<(EventType, char?)>(QUEUE_CAP, "CONIO_STDOUT" + ++myCounter);
      }

      /// <summary>
      /// <br> Emulates behaviour of c stdin </br>
      /// </summary>
      public class StandardIn : Stream
      {
         private byte[]? myInternalBuffer = null;

         public StandardIn(ConsoleIo conio, Encoding encoding)
         {
            Conio = conio;
            Encoding = encoding;
         }

         public override bool CanRead => true;

         public override bool CanSeek => false;

         public override bool CanWrite => false;

         public override long Length => throw new Gate.Tools.ToolsException("Not used");

         public override long Position
         {
            get => -1;
            set => throw new Crash("Can't use");
         }

         /// <summary>
         /// <br> Reference to parent <see cref="ConsoleIo"/> </br>
         /// </summary>
         public ConsoleIo Conio { get; }

         /// <summary>
         /// <br> Encoding used to encode chars to bytes </br>
         /// </summary>
         public Encoding Encoding { get; set; }

         /// <summary>
         /// Not used
         /// </summary>
         /// <exception cref="Crash"></exception>
         public override void Flush() => throw new Crash("Can't use");

         public byte[] InternalBufferBytes => myInternalBuffer != null ? [.. myInternalBuffer] : [];

         /// <summary>
         /// <br>Emulates behaviour of c stdin </br>  
         /// <br>Check if <see cref="CurrentInput"/> is not null:</br>  
         /// <br> - if so reads chars in input onto buffer</br>  
         /// <br> - otw starts a <see cref="Conio"/>.ReadLine (this causes hang up until line feed is pressed) </br>  
         /// </summary>
         /// <param name="buffer"></param>
         /// <param name="offset"></param>
         /// <param name="count"></param>
         /// <returns>
         /// If <paramref name="input"/> is not null number of written chars else -1*number of written chars or <see cref="Int32.MinValue"/> number of written chars == 0
         /// </returns>
         public override int Read(byte[] buffer, int offset, int count)
         {
            var n_cap = count - offset;

            if (n_cap <= 0)
            {
               return 0;
            }
            else
            {
               using (Conio.myStdinCs.GetLock())
               {
                  var nc = 0;//number of chars written to buffer
                  var cur_inp = null as string;

                  n_cap = count - offset - nc;

                  //translates until buffer count is reached
                  while (nc < count)
                  {
                     if (myInternalBuffer == null || myInternalBuffer.Length < n_cap)
                     {
                        cur_inp = Conio.ReadLine();

                        //user has pressed Ctrl+D
                        if (cur_inp == null)
                        {
                           //copy remaining buffer
                           nc += myInternalCopyToBuffer(buffer, offset + nc, n_cap);

                           //-1 just if no char copied
                           return nc <= 0 ? -1 : nc;
                        }
                        else
                        {
                           myInternalTranslateStringEncodeTo(cur_inp);
                           nc += myInternalCopyToBuffer(buffer, offset + nc, n_cap);
                           n_cap = count - offset - nc;
                        }
                     }
                     else
                     {
                        return myInternalCopyToBuffer(buffer, offset + nc, n_cap);
                     }
                  }

                  return nc;
               }
            }
         }

         /// <summary>
         /// Not used
         /// </summary>
         /// <param name="offset"></param>
         /// <param name="origin"></param>
         /// <returns></returns>
         /// <exception cref="Crash"></exception>
         public override long Seek(long offset, SeekOrigin origin) => throw new Crash("Can't use");

         /// <summary>
         /// Not used
         /// </summary>
         /// <param name="value"></param>
         /// <exception cref="Crash"></exception>
         public override void SetLength(long value) => throw new Crash("Can't use");

         /// <summary>
         /// Not used
         /// </summary>
         /// <param name="buffer"></param>
         /// <param name="offset"></param>
         /// <param name="count"></param>
         /// <exception cref="Crash"></exception>
         public override void Write(byte[] buffer, int offset, int count) => throw new Crash("Can't use");

         /// <summary>
         /// 
         /// </summary>
         /// <param name="input"></param>
         /// <exception cref="Crash"></exception>
         private void myInternalTranslateStringEncodeTo(string? input)
         {
            //costraint
            if (input == null) { throw new Crash(); }

            //number of chars used(encoded)
            var n_ch_use = 0;

            //internal offset
            var int_off = 0;

            //just in case ReadLine returns an error
            input += "\n";

            var enc = Encoding.GetEncoder();
            var n_bys = enc.GetByteCount(input.ToCharArray(), 0, input.Length, false);

            if (myInternalBuffer == null)
            {
               myInternalBuffer = new byte[n_bys];
            }
            else
            {
               int_off = myInternalBuffer.Length;
               myInternalBuffer = myInternalBuffer.Concat(new byte[n_bys]).ToArray();
            }

            //encodes character on input line until buffer full or input line ended
            enc.Convert(
               input.ToCharArray(),
               0,
               input.Length,
               myInternalBuffer,
               int_off,
               myInternalBuffer.Length - int_off,
               true,
               out n_ch_use,
               out var nb,
               out _);

            if (input.Length != n_ch_use) { throw new Crash(); }
         }


         private int myInternalCopyToBuffer(byte[] buffer, int offset, int count)
         {
            if (myInternalBuffer != null)
            {
               var i = 0;
               var nr = Math.Min(count, myInternalBuffer.Length);

               for (; i < nr; i++)
               {
                  buffer[offset + i] = myInternalBuffer[i];
               }

               myInternalBuffer = i >= myInternalBuffer.Length ?
                  null :
                  myInternalBuffer.Skip(i).ToArray();

               return nr;
            }
            else
            {
               return 0;
            }
         }
      }

      /// <summary>
      /// Provides a writable stream for outputting data to the console using a specified encoding.
      /// </summary>
      /// <remarks><para> <see cref="StandardOut"/> is a custom <see cref="Stream"/> implementation that
      /// enables writing byte data to the console. Data written to this stream is buffered and then flushed to the
      /// console output using the provided <see cref="ConsoleIo"/> instance and <see cref="Encoding"/>. </para> <para>
      /// This stream supports writing only; reading and seeking operations are not supported and will throw exceptions
      /// if attempted. </para></remarks>
      public class StandardOut : Stream
      {
         private readonly ConcurrentQueue<byte> myQueue = new ConcurrentQueue<byte>();

         /// <summary>
         /// <br> Initializes a new instance of the <see cref="StandardOut"/> class. </br>
         /// </summary>
         /// <param name="conio"></param>
         /// <param name="encoding"></param>
         public StandardOut(ConsoleIo conio, Encoding encoding)
         {
            Conio = conio;
            Encoding = encoding;
         }

         /// <summary>
         /// <br> Dummy stream to read from <see cref="StandardOut"/> queue and the stream to decode to char array </br>
         /// </summary>
         private class InnerDummyStream : Stream
         {
            public InnerDummyStream(StandardOut stdOut) => StdOut = stdOut;

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length => -1;

            public override long Position
            {
               get => -1;
               set => throw new Crash("Can't use");
            }

            public StandardOut StdOut { get; }

            public override void Flush() { }

            public override int Read(byte[] buffer, int offset, int count)
            {
               for (int i = 0; i < count; i++)
               {
                  if (StdOut.myQueue.TryDequeue(out var byt)) { buffer[i + offset] = byt; }
                  else { return i; }
               }

               return count;
            }

            public override long Seek(long offset, SeekOrigin origin) => throw new Crash("Can't use");

            public override void SetLength(long value) => throw new Crash("Can't use");

            public override void Write(byte[] buffer, int offset, int count) => throw new Crash("Can't use");
         }

         public override bool CanRead => false;

         public override bool CanSeek => false;

         public override bool CanWrite => true;

         public override long Length => -1;

         public override long Position { get => -1; set => throw new Crash("Can't use"); }

         /// <summary>
         /// <br> Reference to parent <see cref="ConsoleIo"/> </br>
         /// </summary>
         public ConsoleIo Conio { get; }

         /// <summary>
         /// <br> Encoding used to decode bytes to chars </br>
         /// </summary>
         public Encoding Encoding { get; set; }

         /// <summary>
         /// <br> Flushes the buffered output to the console. </br>
         /// </summary>
         public override void Flush()
         {
            using (var sr = new StreamReader(new InnerDummyStream(this), Encoding))
            {
               Conio.myEnqueueToStdOut(sr.ReadToEnd());
               Conio.FlushOutput(1.0);
            }
         }

         /// <summary>
         /// Not used
         /// </summary>
         /// <param name="buffer"></param>
         /// <param name="offset"></param>
         /// <param name="count"></param>
         /// <returns></returns>
         /// <exception cref="Crash"></exception>
         public override int Read(byte[] buffer, int offset, int count) => throw new Crash("Can't use");

         /// <summary>
         /// Not used
         /// </summary>
         /// <param name="offset"></param>
         /// <param name="origin"></param>
         /// <returns></returns>
         /// <exception cref="Crash"></exception>>	SeaMath.dll!Gate.SeaMath.Workspace.Libs.SeaMathLibCSharpDeclFunction.myRunAction(Gate.LangBase.Runtime.DbgEngVirtCpu.RtmDbgEngStackVirtCpu stack) Line 79	C#

         public override long Seek(long offset, SeekOrigin origin) => throw new Crash("Can't use");

         /// <summary>
         /// Not used
         /// </summary>
         /// <param name="value"></param>
         /// <exception cref="Crash"></exception>
         public override void SetLength(long value) => throw new Crash("Can't use");

         /// <summary>
         /// Not used
         /// </summary>
         /// <param name="buffer"></param>
         /// <param name="offset"></param>
         /// <param name="count"></param>
         public override void Write(byte[] buffer, int offset, int count)
         {
            for (int i = 0; i < count; i++) { myQueue.Enqueue(buffer[offset + i]); }

            Flush();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public bool IsEchoActiveForReadLine { get; set; } = true;

      /// <summary>
      /// 
      /// </summary>
      public ConsoleController? ConsoleController => Task.ConsoleController;

      /// <summary>
      /// If true is disposed at end of task otherwise application is responsible to dispose
      /// </summary>
      public bool Is2DisposeAtTaskEnd { get; }

      /// <summary>
      /// 
      /// </summary>
      public string PromptString { get; set; } = DEFAULT_PROMPT_STRING;

      /// <summary>
      /// When not null <see cref="ConsoleIo"/> is connected to keyboard input stroke
      /// </summary>
      public ConsoleInputKeyEventStroke? ConsoleInputKeyEventStroke
      {
         get => myConsoleInputKeyEventStroke;

         set
         {
            if (myConsoleInputKeyEventStroke != value)
            {
               if (value == null)
               {
                  (myConsoleInputKeyEventStroke ?? throw new Crash()).OnConsume = null;
               }
               else
               {
                  value.OnConsume = myActionOnInputConsume;
               }

               myConsoleInputKeyEventStroke = value;
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public ConsoleTask? BoundConsoleTask => ConsoleController?.ConsoleTasks.FirstOrDefault(ct => ct.Conio == this);

      /// <summary>
      /// 
      /// </summary>
      public int StdOutQueueLen => myQueueCharStdOut.Count;

      /// <summary>
      /// If a <see cref="ReadLine"/> is active return current input line, otherwise return null.
      /// </summary>
      public string? ReadLineLine { get; private set; } = null;

      public IConsoleControl? IConsoleControl => ConsoleController?.IControl;

      public Control? ConsoleControl => ConsoleController?.Control;
      /// <summary>
      /// 
      /// </summary>
      public string[] History
      {
         get => myListHistory.ToArray();

         set
         {
            myListHistory.Clear();
            myListHistory.AddRange(value ?? new string[0]);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public string? NewLine { get; set; } = "\n";

      public int ReadLinePos
      {
         get => ConsoleController?.IControl.CurrentPos.Col - PromptString.Length - 1 ?? -1;

         private set
         {
            if (ConsoleController != null)
            {
               ConsoleController.IControl.CurrentPos =
                  new TxtPos(ConsoleController.IControl.CurrentPos.Line, PromptString.Length + value + 1);
            }
            else
            {
               throw new Crash();
            }
         }
      }

      public ConsoleTask Task { get; }

      public (string? word, int offset) HintTuple
      {
         get
         {
            if (string.IsNullOrEmpty(ReadLineLine) || ReadLinePos < 0) { return (null, -1); }

            // Find word start (scan backwards from cursor)
            var ws = Math.Min(ReadLinePos, ReadLineLine.Length - 1);

            while (ws > 0)
            {
               char c = ReadLineLine[ws - 1];

               if (!char.IsLetterOrDigit(c) && c != '_') { break; }

               ws--;
            }

            // Find word end (scan forwards from cursor) 
            var we = Math.Max(1, ReadLinePos);

            while (we < ReadLineLine.Length)
            {
               var c = ReadLineLine[we - 1];

               if (!char.IsLetterOrDigit(c) && c != '_') { break; }

               we++;
            }

            // Return the word if cursor is within or at boundaries of a word
            if (we > ws && ReadLinePos >= ws && ReadLinePos <= we)
            {
               return (ReadLineLine.Substring(ws, we - ws), ws);
            }
            else
            {
               return (null, -1);
            }
         }
      }

      public bool IsActive
      {
         get => myIsActive;

         set
         {
            if (myIsActive = value)
            {
               myQueueCharStdOut.Consumer = (q, ee) => myStdOutDirect(ee);
               ConsoleInputKeyEventStroke = IConsoleControl?.ConsoleInputKeyEventStroke;
               (ConsoleInputKeyEventStroke ?? throw new Crash()).IsActive = true;
            }
            else
            {
               (ConsoleInputKeyEventStroke ?? throw new Crash()).IsActive = false;
               ConsoleInputKeyEventStroke = null;
               myQueueCharStdOut.Consumer = null;
            }
         }
      }

      public bool IsSelectionOnLine
      {
         get
         {
            var pos = IConsoleControl?.CurrentPos;
            var sel = IConsoleControl?.Selection;

            if (sel?.start == null) { return false; }
            else if (sel?.start.Line == sel?.end?.Line && sel?.start.Line == pos?.Line)
            {
               var sta = Math.Max(PromptString.Length, sel?.start.Col ?? 0);
               var end = Math.Max(PromptString.Length, sel?.end?.Col ?? 0);

               return end > sta;
            }
            else
            {
               return false;
            }
         }
      }

      public void CancelIO() => Interlocked.Exchange(ref myCancelIoSignal, 1);

      public void ClearQueues() => myQueueCharStdOut.Clear();

      public bool FlushOutput(double drainInterval) => myQueueCharStdOut?.DrainAll(drainInterval) ?? false;

      public void HistoryBack() => myHistoryOperation(false);

      public void HistoryForth() => myHistoryOperation(true);

      public int Kbhit() => myQueueKeyStrokeEvent.IsEmpty ? 0 : 1;

      /// <summary>
      /// Blocking read of a char from console input.
      /// </summary>
      /// <param name="isWithEcho"></param>
      /// <returns></returns>
      public int Getch(bool isWithEcho)
      {
         using (myCritSecOneOnlyReadLineClient.GetLock())
         {
            try
            {
               IsEchoActiveForReadLine = isWithEcho;

               while (true)
               {
                  if (myQueueKeyStrokeEvent.TryDequeue(out var tup))
                  {
                     var ch = null as char?;

                     switch (tup.eventType)
                     {
                        case EventType.tab:
                           ch = '\t';
                           break;
                        case EventType.valid_char:
                           ch = tup.extraChar;
                           break;
                        case EventType.backspace:
                           ch = '\b';
                           break;
                        case EventType.enter:
                           ch = '\r';
                           break;

                        default:
                           break;
                     }

                     if (isWithEcho)
                     {
                        IConsoleControl?.Insert2CurrentPos(ch.HasValue ? $"{ch.Value}" : "Invalid_char");
                     }

                     return tup.extraChar ?? -1;
                  }
                  else { Thread.Sleep(200); }
               }
            }
            finally
            {
               IsEchoActiveForReadLine = true;
            }
         }
      }

      /// <summary>
      /// Blocking read of a line from console input.
      /// </summary>
      /// <param name="currentLine"></param>
      /// <param name="currentPos"></param>
      /// <returns>Null in case of Ctrl+D othw a valid string  </returns>
      /// <remarks>Doesn't include '\n'</remarks>
      public string? ReadLine(string? currentLine = null, int currentPos = -1)
      {
         try
         {
            using (myCritSecOneOnlyReadLineClient.GetLock())
            {
               if (ConsoleInputKeyEventStroke == null)
               {
                  return null;
               }

               if (myIsReadLineCancelProcedure)
               {
                  myIsReadLineCancelProcedure = myCritSecOneOnlyReadLineClient.LockedThreads.Length != 0;

                  return null;
               }

               Interlocked.Exchange(ref myCancelIoSignal, 0);

               if (currentLine == null)
               {
                  ReadLineLine = "";
                  ReadLinePos = 0;
               }
               else
               {
                  ReadLineLine = currentLine;
                  ReadLinePos = currentPos;
               }

               ConsoleControl?.MthInvoke(() => ConsoleControl.Focus());

               while (true)
               {
                  if (myCheckCancel())
                  {
                     myIsReadLineCancelProcedure = myCritSecOneOnlyReadLineClient.LockedThreads.Length > 0;

                     return null;
                  }

                  while (myQueueKeyStrokeEvent.TryDequeue(out var tup))
                  {
                     if (myReadLineKeyInAction(tup.eventType, tup.extraChar))
                     {
                        if (ReadLineLine.ExtTrim() != "")
                        {
                           myListHistory.Remove(ReadLineLine.ExtTrim());
                           myListHistory.Insert(0, ReadLineLine.ExtTrim());
                        }

                        return ReadLineLine;
                     }
                  }

                  Thread.Sleep(100);
               }
            }
         }
         finally
         {
            OnInputLineEnded?.Invoke(this, ReadLineLine ?? "");
            ReadLineLine = null;
         }
      }

      /// <summary>
      /// Writes the specified string followed by a newline character to the output without passing for stdout.
      /// </summary>
      /// <param name="line">The string to write. If <paramref name="line"/> is null or not provided, an empty line is written.</param>
      public void WriteLine(string line = "") => Write($"{line}{NewLine}");

      /// <summary>
      /// Writes the specified string to the output without passing for stdout.
      /// </summary>
      /// <remarks>Each character in the string is processed and sent to the output stream.  Ensure that the
      /// string contains valid characters for the intended output.</remarks>
      /// <param name="str">The string to write. Cannot be null.</param>
      public void Write(string @str) =>
         myStdOutDirect(str.ToCharArray().Select(c => (EventType.valid_char, (char?)c)).ToArray());

      /// <summary>
      /// 
      /// </summary>
      public void MoveToNextCleanLine()
      {
         if (ConsoleInputKeyEventStroke != null)
         {
            FlushOutput(1.0);

            var ctr = ConsoleController?.IControl ?? throw new Crash();
            var cur_ln_idx = ctr.CurrentPos.Line;
            var cur_ln = ctr.GetLine(cur_ln_idx);

            if (cur_ln != "")
            {
               ctr.CurrentPos = new TxtPos(cur_ln_idx + 1, 1);
            }
         }
         else { throw new Crash(); }
      }

      public void WritePrompt(string? promptString = null)
      {
         ConsoleController?.CriticalAction(() =>
         {
            PromptString = (promptString ?? PromptString).ExtTrim();
            MoveToNextCleanLine();

            var cur_pos = ConsoleController.IControl.CurrentPos;

            ConsoleController.IControl.CurrentPos = new TxtPos(cur_pos.Line, 1);
            ConsoleController.IControl.Insert2CurrentPos(PromptString);
         });
      }

      protected override void myFreeManaged()
      {
         ConsoleInputKeyEventStroke = null;
         myQueueCharStdOut.Dispose();
      }

      protected override void myFreeUnmanaged() { }

      private void myActionOnInputConsume(EventType eventType, char? extraChar)
      {
         if (eventType == EventType.abort_control_c)
         {
            Task.Abort(AbortReason.control_c);
         }
         else if (eventType == EventType.abort_control_d)
         {
            CancelIO();
         }
         else
         {
            myQueueKeyStrokeEvent.Enqueue((eventType, extraChar));
         }
      }

      /// <summary>
      /// Processes a key event during a <see cref="ReadLine"/> operation.
      /// </summary>
      /// <param name="keyEvent"></param>
      /// <param name="extraChar"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      private bool myReadLineKeyInAction(EventType keyEvent, char? extraChar)
      {
         var cns_ctl = ConsoleController ?? throw new Crash();
         var cns_ctr = IConsoleControl ?? throw new Crash();

         if (keyEvent == EventType.valid_char)
         {
            if (IsEchoActiveForReadLine)
            {
               var sel = cns_ctr?.Selection ?? throw new Crash();
               var cur_pos = cns_ctr.CurrentPos;

               //selection shall be inside input line 
               if (sel.start != null && sel.start.Col >= 2 && sel.start.Line == cur_pos.Line && sel.end?.Line == cur_pos.Line)
               {
                  cns_ctr.ReplaceSelection($"{extraChar ?? '\0'}");
               }
               else
               {
                  cns_ctr.Insert2CurrentPos($"{extraChar ?? '\0'}");
               }
            }

            if (myHintForm != null)
            {
               myHintForm.PpFilter += extraChar ?? '\0';
            }

            myUpdateReadLine();
         }
         else
         {
            switch (keyEvent)
            {
               case EventType.up:
               case EventType.down:
                  myHintHandler = null;

                  if (ReadLineLine != null)
                  {
                     if (keyEvent == EventType.up) { HistoryBack(); }
                     else { HistoryForth(); }
                  }

                  break;

               case EventType.cancel:
                  if (myHintHandler != null && ReadLinePos < myHintHandler.StartLinePos)
                  {
                     myHintHandler = null;
                  }

                  myDoDelete(false);
                  break;

               case EventType.backspace:
                  myDoDelete(true);
                  break;

               case EventType.enter:
                  myHintHandler = null;
                  cns_ctr.CurrentPos = new TxtPos(cns_ctr.CurrentPos.Line + 1, 1);

                  return true;

               case EventType.left:
                  if (ReadLinePos > 0) { ReadLinePos--; }
                  break;

               case EventType.right:
                  if (ReadLinePos < ReadLineLine?.Length) { ReadLinePos++; }
                  break;

               case EventType.tab_shift:
                  myHintForm?.MthScroll(false);
                  break;

               case EventType.tab:
                  var hns = BoundConsoleTask?.Hints ?? [];
                  var sp = cns_ctr.CurrentScreenPos;

                  if (myHintHandler != null)
                  {
                     myHintHandler.StartLinePos = ReadLinePos;

                     var idx = myHintHandler.GetIndexFilter(ReadLineLine ?? "");

                     if (idx < 0)
                     {
                        myHintHandler = null;
                        IConsoleControl.Selection = (null, null);

                        return false;
                     }

                     var ln = IConsoleControl.CurrentPos.Line;
                     var hnt_fld = myHintHandler.Fields[myHintHandler.FieldIndex] ?? throw new Crash();
                     var pl = PromptString.Length;

                     ReadLinePos = 1 + idx;
                     IConsoleControl.Selection = (new TxtPos(ln, 1 + idx + pl), new TxtPos(ln, 1 + idx + pl + hnt_fld?.Match?.Length ?? 0));

                     if (++myHintHandler.FieldIndex >= myHintHandler.Fields.Length)
                     {
                        myHintHandler = null;
                     }
                  }
                  else if (myHintForm == null)
                  {
                     ConsoleControl?.MthInvoke(() =>
                     {
                        myHintForm = new ConsoleCmdHintListForm();
                        myHintForm.FormClosed += MyHintForm_FormClosed;
                        myHintForm.KeyDown += MyHintForm_KeyDown;

                        if (!myHintForm.MthShow(ConsoleControl, sp, hns, HintTuple.word.ExtTrim()))
                        {
                           myHintForm = null;
                        }
                     });
                  }

                  myHintForm?.MthScroll(true);
                  break;

               case EventType.end:
                  ReadLinePos = ReadLineLine?.Length ?? 0;
                  myHintHandler = null;
                  break;

               case EventType.home:
                  ReadLinePos = 0;
                  myHintHandler = null;
                  break;

               case EventType.abort_control_c:
                  BoundConsoleTask?.Abort(AbortReason.control_c);
                  myHintHandler = null;
                  break;

               default: throw new Crash();
            }
         }

         return false;
      }

      /// <summary>
      /// Performs deletion of characters in the current input line.
      /// </summary>
      /// <param name="isBackward"></param>
      private void myDoDelete(bool isBackward)
      {
         var cns_ctr = IConsoleControl ?? throw new Crash();
         var sel = cns_ctr.Selection;

         if (IsSelectionOnLine)
         {
            var sta = new TxtPos((sel.start ?? throw new Crash()).Line, Math.Max(sel.start.Col, PromptString.Length + 1));

            if (sel.start.Col <= PromptString.Length)
            {
               //adjust prompt selection
               var pos = cns_ctr.CurrentPos;

               cns_ctr.Selection = (sta, sel.end);
            }

            cns_ctr.ReplaceSelection("");
            myUpdateReadLine();
            cns_ctr.CurrentPos = sta;
         }
         else
         {
            //if backward delete and not at begin of line or forward delete and not at end of line
            if (isBackward ? ReadLinePos > 0 : ReadLinePos < ReadLineLine?.Length)
            {
               cns_ctr.CancelChar(isBackward);
               myUpdateReadLine();
            }
         }
      }

      private void myUpdateReadLine()
      {
         var cns_ctr = IConsoleControl ?? throw new Crash();
         var inp = cns_ctr.GetLine(cns_ctr.CurrentPos.Line);

         ReadLineLine = inp.StartsWith(PromptString) ?
            inp.Substring(PromptString.Length) :
            throw new Crash($"Input line does not start with prompt string '{PromptString}'");
      }

      /// <summary>
      /// Translates a key event into a char.
      /// </summary>
      /// <param name="e"></param>
      /// <returns></returns>
      private char myGetCharFromKeyEvent(KeyEventArgs e)
      {
         // only letters, numbers, simple symbols
         var key = e.KeyCode;
         var shi = e.Shift;

         if (key >= Keys.A && key <= Keys.Z)
         {
            var c = (char)('a' + (key - Keys.A));

            return shi ? char.ToUpper(c) : c;
         }

         if (key >= Keys.D0 && key <= Keys.D9)
         {
            var c = (char)('0' + (key - Keys.D0));

            if (shi)
            {
               // sysmbols over numbers (eg US keyboard)
               var sms = ")!@#$%^&*(";

               return sms[key - Keys.D0];
            }

            return c;
         }

         return '\0';
      }

      private void myDoCloseHint()
      {
         (myHintForm ?? throw new Crash()).FormClosed -= MyHintForm_FormClosed;
         myHintForm.Close();
         myHintForm = null;
      }

      private void myApplyHint(ConsoleCmdHint hint)
      {
         var ctr = ConsoleController?.IControl ?? throw new Crash();
         var hnt_frm = hint.HintFormat ?? "";
         var hnt_hnd = hint.GetHandler();
         var hnt_tup = HintTuple;
         var off = hnt_tup.word.IsBlank() ? ReadLinePos : hnt_tup.offset;
         var wrd = hnt_tup.word.IsBlank() ? "" : (hnt_tup.word ?? "");
         var pl = PromptString.Length;
         var cur_pos = ctr.CurrentPos;

         ctr.Selection = (new TxtPos(cur_pos.Line, off + pl + 1), new TxtPos(cur_pos.Line, off + wrd.Length + pl + 1));
         ctr.ReplaceSelection(hnt_frm);
         ReadLineLine = ctr.GetLine(cur_pos.Line).Substring(pl);

         if (hnt_hnd.Fields.Length != 0)
         {
            var f0 = hnt_hnd.Fields?.FirstOrDefault() ?? throw new Crash();
            var mat = f0.Match ?? throw new Crash();

            myHintHandler = hnt_hnd;
            myHintHandler.StartLinePos = ReadLinePos;
            ReadLinePos = off + mat.Index + 1;
            ctr.Selection = (
               new TxtPos(cur_pos.Line, off + pl + 1 + mat.Index),
               new TxtPos(cur_pos.Line, off + pl + 1 + mat.Index + mat.Length));

            if (myHintHandler.Fields.Length > 1)
            {
               myHintHandler.FieldIndex = 1;
            }
            else
            {
               myHintHandler = null;
            }
         }
         else
         {
            ReadLinePos = off + hnt_frm.Length;
         }
      }

      private void myHistoryOperation(bool isForth)
      {
         if (BoundConsoleTask != null && ReadLineLine != null && myListHistory.Count > 0)
         {
            var new_val = isForth ? myHistoryIdx - 1 : myHistoryIdx + 1;

            new_val = Math.Max(0, Math.Min(myListHistory.Count - 1, new_val));

            if (myListHistory.Count > 0)
            {
               var ctr = ConsoleController?.IControl ?? throw new Crash();
               var cur_pos = ctr.CurrentPos;

               myHistoryIdx = new_val;
               ReadLineLine = myListHistory[myHistoryIdx];
               ctr.SetLine(PromptString + ReadLineLine);
               ReadLinePos = ReadLineLine.Length;
               ctr.CurrentPos = new TxtPos(cur_pos.Line, (PromptString + ReadLineLine).Length + 1);
            }
         }
      }

      private bool myCheckCancel()
      {
         var cio = 0;

         Interlocked.Exchange(ref cio, myCancelIoSignal);

         if (cio != 0)
         {
            Interlocked.Exchange(ref myCancelIoSignal, 0);

            return true;
         }//cancelled io out
         else
         {
            return false;
         }
      }

      private (EventType, object?)[] mySplitObjects((EventType, char?)[] strokeEvents)
      {
         var str = "";
         var lst = new List<(EventType, object?)>();

         foreach (var ev in strokeEvents)
         {
            var is_str = false;

            if (ev.Item1 == EventType.valid_char)
            {
               switch (ev.Item2)
               {
                  case '\r': //beginning of row
                  case '\n'://beginning of next row
                  case '\b':
                     lst.Add((ev.Item1, ev.Item2));
                     break;

                  default:
                     str += ev.Item2.ConvertOrCrash<char>();
                     is_str = true;
                     break;
               }
            }
            else
            {
               lst.Add((ev.Item1, ev.Item2));
            }

            if (!is_str && str != "")
            {
               lst.Insert(lst.Count - 1, (EventType.valid_char, str));
               str = "";
            }
         }

         if (str != "")
         {
            lst.Add((EventType.valid_char, str));
         }

         return lst.ToArray();
      }

      private void myStdOutDirect((EventType, char?)[] strokeEvents)
      {
         ConsoleController?.Control?.MthInvoke(() =>
         {
            var spo = mySplitObjects(strokeEvents);

            foreach (var str_ev in spo.Where(s => s.Item1 == EventType.valid_char))
            {
               switch (str_ev.Item2)
               {
                  case '\r': //beginning of row
                     (IConsoleControl ?? throw new Crash()).CurrentPos = new TxtPos(IConsoleControl.CurrentPos.Line, 1);
                     break;

                  case '\n'://beginning of next row
                     (IConsoleControl ?? throw new Crash()).CurrentPos = new TxtPos(IConsoleControl.CurrentPos.Line + 1, 1);
                     break;

                  case '\b':
                     IConsoleControl?.CancelChar(true);
                     break;

                  default:
                     IConsoleControl?.Insert2CurrentPos($"{(string)str_ev.Item2}");
                     break;
               }
            }
         });
      }

      private void myEnqueueToStdOut(string @string) =>
         myQueueCharStdOut.Produce(@string.ToCharArray().Select(c => (EventType.valid_char, (char?)c)).ToArray());

      private void MyHintForm_KeyDown(object? sender, KeyEventArgs e)
      {
         var ch = myGetCharFromKeyEvent(e);

         if (ch != 0)
         {
            var ev = new KeyPressEventArgs(ch);

            ConsoleInputKeyEventStroke?.SynthetizeKeyPress(sender, ev);//re-forward printable char
         }
      }

      private void MyHintForm_FormClosed(object? sender, FormClosedEventArgs e)
      {
         if (myHintForm?.PpHintSelected != null)
         {
            myApplyHint(myHintForm.PpHintSelected);
         }

         myDoCloseHint();
      }
   }
}