
namespace Gate.SeaMath.Console
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaMathConsoleStrategyBySystemConsole : SeaMathConsoleStrategy
   {
      private readonly static InnerDummy.Input myInput = new InnerDummy.Input();
      private readonly static InnerDummy.Output myOutput = new InnerDummy.Output();
      private readonly static InnerDummy.Error myError = new InnerDummy.Error();

      public SeaMathConsoleStrategyBySystemConsole() { }

      private class InnerDummy : Stream
      {
         private readonly Stream myStream;

         public class Input : InnerDummy
         {
            public Input() : base(System.Console.OpenStandardInput()) { }

            public override string ToString() => "InputFileDummy";
         }

         public class Output : InnerDummy
         {
            public Output() : base(System.Console.OpenStandardOutput()) { }

            public override string ToString() => "OutputFileDummy";
         }
         public class Error : InnerDummy
         {
            public Error() : base(System.Console.OpenStandardError()) { }

            public override string ToString() => "ErrorFileDummy";
         }

         protected InnerDummy(Stream stream) => myStream = stream;

         protected override void Dispose(bool disposing)
         {
            base.Dispose(disposing);
         }

         public override ValueTask DisposeAsync()
         {
            return base.DisposeAsync();
         }

         public override bool CanRead => myStream.CanRead;

         public override bool CanSeek => myStream.CanSeek;

         public override bool CanWrite => myStream.CanWrite;

         public override long Length => myStream.Length;

         public override long Position
         {
            get => myStream.Position;
            set => myStream.Position = value;
         }

         public override void Flush()
         {
            myStream.Flush();
         }

         public override int Read(byte[] buffer, int offset, int count)
         {
            return myStream.Read(buffer, offset, count);
         }

         public override long Seek(long offset, SeekOrigin origin)
         {
            return myStream.Seek(offset, origin);
         }

         public override void SetLength(long value)
         {
            myStream.SetLength(value);
         }

         public override void Write(byte[] buffer, int offset, int count)
         {
            myStream.Write(buffer, offset, count);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="console"></param>
      /// <returns></returns>
      protected override (Stream stdIn, Stream stdOut, Stream stdErr) myOnMakingConsole(SeaMathConsole console) =>
         (myInput, myOutput, myError);

      public override void BeforeInstructionRun() { }

      public override int Getch(bool isWithEcho) => System.Console.ReadKey(!isWithEcho).KeyChar;

      public override int Kbhit() => System.Console.KeyAvailable ? 1 : 0;

      public override void ClearScreen() => System.Console.Clear();

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override (Stream stdIn, Stream stdOut, Stream stdErr) MakeStreamsForVirtProcess() => (myInput, myOutput, myError);

      public override void OnConsoleTerminate() { }

      public override void OnNewProcessCreated(SeaMathProcessExecutable process) { }
   }
}
