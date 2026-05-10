using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.DbgEng;
using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.LangBase.Runtime.Object
{
   /// <summary>
   /// Function object for execution <see cref="IRtmDbgEngIde"/> like Seamath. 
   /// </summary>
   public class RtmObjFunction : RtmObj, IRtmObjFunction
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="declFunction"></param>
      public RtmObjFunction(IDeclFunction declFunction) : base(declFunction) { }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="stack"></param>
      /// <param name="rtmStrategy"></param>
      /// <param name="params"></param>
      /// <returns></returns>
      public RtmObj? Exec(IRtmDbgEngStackExecutable? stack, IRtmObjStrategy? rtmStrategy, params RtmObj?[] @params)
      {
         (stack ?? throw new Crash()).Push(stack.MakeStackCall(this));
         stack.Push(@params);

         return myExec(stack, rtmStrategy);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="stack"></param>
      /// <returns></returns>
      protected virtual RtmObj? myExec(IRtmDbgEngStackExecutable? stack, IRtmObjStrategy? rtmStrategy)
      {
         if (Decl?.Instructions.Length == 0) { return null; }
         else
         {
            var res = null as RtmObj;
            var stk = stack ?? throw new Crash();
            var ttf = stk.TopFunctionFrame ?? throw new Crash();

            ttf.InstructionCurrent = ttf.Instructions.FirstOrDefault();

            //stay until a return/end_function occurs
            while (stack?.TopFunctionFrame?.RtmObjFunction == this)
            {
               var ins = stack.TopFunctionFrame.InstructionCurrent;

               if (stack?.Thread?.TryHalt(ins) ?? false)
               {
                  while (stack.Thread.ThreadState == RtmDbgEngRunState.halt)
                  {
                     Thread.Sleep(100);
                  }
               }

               stack?.TopFunctionFrame?.InstructionCurrent?.Run(stack, rtmStrategy);
            }

            return stack?.Pop();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public new IDeclFunction? Decl => base.Decl as IDeclFunction;

      /// <summary>
      /// 
      /// </summary>
      public override IntPtr? Address => null;


      /// <summary>
      /// 
      /// </summary>
      public override ValueType? CSharpObj
      {
         get => null;
         set => throw new Gate.LangBase.Runtime.RtmException("Not implemented");
      }

      public override RtmFormat CurrentFormat => new RtmFormatBase();

      public IDeclFunction? DeclFunction => base.Decl as IDeclFunction;

      public override string ToString() => $"RtmFunction:{VarName}{(Decl != null ? Decl.ToString() : "()")}";

      protected override void myFreeManaged() { }

      protected override void myFreeUnmanaged() { }
   }
}
