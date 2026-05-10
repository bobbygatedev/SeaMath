using System.IO.Pipes;
using System.Text.Json;

namespace Gate.Tools;

public abstract class SingleInstanceApp
{
   private static SingleInstanceApp? myInstance;

   protected SingleInstanceApp()
   {
      
   }

   private class InnerPayload
   {
      public string[]? CmdLine { get; set; }
      public object? Data { get; set; }
   }


   public abstract string AppId { get; }

   public static SingleInstanceApp? Instance => myInstance;

   protected abstract void myEntryPointWithToken(string[] cmdLine);

   protected abstract object? myGetEntryPointNoMutexLocalToRemoteParams();

   protected abstract void myEntryPointNoMutexRemote(string[] cmdLine, object? localToRemoteParams);

   public bool Start(params string[] cmdLine)
   {
      myInstance ??= this;

      using var mutex = new Mutex(false, AppId);

      if (!mutex.WaitOne(0))
      {
         // SECOND INSTANCE → client
         SendToPrimary(cmdLine, myGetEntryPointNoMutexLocalToRemoteParams());
         return false;
      }

      // FIRST INSTANCE → server
      _ = Task.Run(() => StartPipeServer());

      myEntryPointWithToken(cmdLine);
      return true;
   }

   private async Task StartPipeServer()
   {
      while (true)
      {
         using var server = new NamedPipeServerStream(AppId, PipeDirection.In);

         await server.WaitForConnectionAsync();

         using var reader = new StreamReader(server);
         string json = await reader.ReadToEndAsync();

         var pay = JsonSerializer.Deserialize<InnerPayload>(json);

         myEntryPointNoMutexRemote(pay?.CmdLine ?? [], pay?.Data);
      }
   }

   private void SendToPrimary(string[] cmdLine, object? data)
   {
      using var client = new NamedPipeClientStream(".", AppId, PipeDirection.Out);

      client.Connect(2000);

      var payload = new InnerPayload
      {
         CmdLine = cmdLine,
         Data = data
      };

      string json = JsonSerializer.Serialize(payload);

      using var writer = new StreamWriter(client);
      writer.Write(json);
      writer.Flush();
   }
}