using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;
using System.Drawing.Text;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// 
   /// </summary>
   public class RtmDbgEngStackVirtCpu : IRtmDbgEngStack
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
      /// <see cref="RtmDbgEngVirtCpuStackItemFrameFunction"/> instances "stack ordered"(ie from top to bottom).
      /// </summary>
      public RtmDbgEngVirtCpuStackItemStackFrame[] StackFrames =>
         Items.OfType<RtmDbgEngVirtCpuStackItemStackFrame>().ToArray();

      /// <summary>
      /// On top stack call (current call)
      /// </summary>
      public RtmDbgEngVirtCpuStackItemFrameFunction? TopFunctionFrame => FunctionFrames?.FirstOrDefault();

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuStackItemStackFrame? TopStackFrame => StackFrames.FirstOrDefault();

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
      /// <param name="frame"></param>
      public void ExitFrame(RtmDbgEngVirtCpuStackItemStackFrame frame, RtmObj? returnValue)
      {
         if (myListItems.Contains(frame))
         {
            var frm_idx = myListItems.IndexOf(frame);

            if (frm_idx >= 0)
            {
               var its_2_rem = myListItems.Skip(frm_idx).ToArray();

               foreach (var rtm_obj in its_2_rem.OfType<RtmObj>())
               {
                  rtm_obj.Dispose();
               }

               myListItems = Enumerable.Range(0, myListItems.Count).Where(i => i < frm_idx).Select(i => myListItems[i]).ToList();
            }
         }

         if (frame is RtmDbgEngVirtCpuStackItemFrameFunction ff)
         {
            Push(returnValue);
         }
         else if (frame is RtmDbgEngVirtCpuStackItemStackFrame)
         {
            if (returnValue != null)
            {
               throw new Crash($"Specify return value valid just for {typeof(RtmDbgEngVirtCpuStackItemFrameFunction).Name}");
            }
         }
         else
         {
            throw new Crash();
         }
      }

      public void Return(RtmObj? returnValue) => ExitFrame(TopFunctionFrame.NnOrCrash(), returnValue);

      IRtmDbgEngThread? IRtmDbgEngStack.Thread => Thread;

      IRtmDbgEngStackCall[] IRtmDbgEngStack.Calls => FunctionFrames;

      IRtmDbgEngStackCall? IRtmDbgEngStack.TopCall => TopFunctionFrame;

      public RtmObj[] FunctionCallParameters
      {
         get
         {
            var np = (TopFunctionFrame ?? throw new RtmException("Not a function frame")).CallParams.Length;
            var idx_tf = Items.ToList().IndexOf(TopFunctionFrame);

            if (idx_tf >= np)
            {
               return
                  Enumerable.Range(0, np).
                  Select(i => 
                     Items.ElementAtOrDefault(idx_tf - 1 - i) as RtmObj ?? 
                     throw new RtmException("Parameter missing")).ToArray();
            }
            else
            {
               throw new RtmException("Not enough parameter in stack!");
            }
         }
      }

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
         else { return null; }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="ITM"></typeparam>
      /// <param name="items"></param>
      public void Push<ITM>(params ITM?[] items) where ITM : class, IRtmDbgEngVirtCpuStackItem =>
         myListItems.AddRange(items);

      public RtmObj? Peek() => myListItems.Last() as RtmObj;

      public override string ToString() => Descriptor;
   }
}
