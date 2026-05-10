using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// 
   /// </summary>
   public class RtmDbgEngStackVirtCpu : IRtmDbgEngStackExecutable
   {
      private List<IRtmDbgEngVirtCpuStackItem?> myListItems = new List<IRtmDbgEngVirtCpuStackItem?>();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="rtmDbgEngDbgThread"></param>
      public RtmDbgEngStackVirtCpu(RtmDbgEngVirtCpuThread? rtmDbgEngDbgThread) => Thread = rtmDbgEngDbgThread;

      /// <summary>
      /// <see cref="RtmDbgEngVirtCpuStackItemFrameFunction"/> instances "stack ordered"(ie from top to bottom).
      /// </summary>
      public RtmDbgEngVirtCpuStackItemFrameFunction[] FunctionFrames => 
         Items.OfType<RtmDbgEngVirtCpuStackItemFrameFunction>().ToArray();

      /// <summary>
      /// On top stack call (current call)
      /// </summary>
      public RtmDbgEngVirtCpuStackItemFrameFunction? TopFunctionFrame => FunctionFrames?.FirstOrDefault();

      /// <summary>
      /// Items "stack ordered"(ie from top to bottom).
      /// </summary>
      public IRtmDbgEngVirtCpuStackItem?[] Items => myListItems.ToArray().Reverse().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public int Length => myListItems.Count;

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuThread? Thread { get; }

      /// <summary>
      /// 
      /// </summary>
      public string Descriptor => $"{GetType().Name}: {string.Join("\n", Items.Cast<object>())}";

      /// <summary>
      /// 
      /// </summary>
      /// <param name="returnValue"></param>
      /// <exception cref="Crash"></exception>
      public void Return(RtmObj? returnValue)
      {
         ExitFrame(FunctionFrames.FirstOrDefault() ?? throw new Crash());
         Push(returnValue);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="frame"></param>
      public void ExitFrame(RtmDbgEngVirtCpuStackItemStackFrame frame)
      {
         var frm_idx = myListItems.IndexOf(frame);

         if (frm_idx >= 0)
         {
            var its_2_rem = myListItems.Skip(frm_idx).ToArray();

            myListItems = Enumerable.Range(0, myListItems.Count).Where(i => i < frm_idx).Select(i => myListItems[i]).ToList();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      IRtmDbgEngStackFrameExecutableCall? IRtmDbgEngStackExecutable.TopFunctionFrame => TopFunctionFrame;

      IRtmDbgEngThread? IRtmDbgEngStackRO.Thread => Thread;

      IRtmDbgEngStackCall[] IRtmDbgEngStackRO.Calls => FunctionFrames;

      IRtmDbgEngStackCall? IRtmDbgEngStackRO.TopCall => TopFunctionFrame;

      /// <summary>
      /// 
      /// </summary>
      public void Clear() => myListItems.Clear();

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="ITM"></typeparam>
      /// <returns></returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
      public ITM? Pop<ITM>() where ITM : class, IRtmDbgEngVirtCpuStackItem
      {
         if (myListItems.Count > 0)
         {
            var itm = myListItems.Last();

            myListItems.Remove(itm);

            return itm as ITM;
         }
         else { throw new Gate.LangBase.Runtime.RtmException("Can't pop empty stack!"); }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="ITM"></typeparam>
      /// <param name="items"></param>
      public void Push<ITM>(params ITM?[] items) where ITM : class, IRtmDbgEngVirtCpuStackItem =>
         myListItems.AddRange(items);

      public override string ToString() => Descriptor;

      IRtmDbgEngStackFrameExecutableCall IRtmDbgEngStackExecutable.MakeStackCall(RtmObjFunction rtmFunction) => 
         new RtmDbgEngVirtCpuStackItemFrameFunction(rtmFunction, this);

      RtmObj? IRtmDbgEngStackExecutable.Pop() => Pop<RtmObj>();

      void IRtmDbgEngStackExecutable.Push(params IRtmDbgEngVirtCpuStackItem?[] @params) => Push(@params);

      void IRtmDbgEngStackExecutable.ExitFrame(IRtmDbgEngStackFrame frame) => ExitFrame(frame as RtmDbgEngVirtCpuStackItemStackFrame ??
            throw new Crash("Invalid frame type for exit!"));
   }
}
