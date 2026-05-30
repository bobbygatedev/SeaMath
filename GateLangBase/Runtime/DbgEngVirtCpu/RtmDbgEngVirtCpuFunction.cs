using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// Function object for execution <see cref="IRtmDbgEngIde"/> like Seamath. 
   /// </summary>
   public class RtmDbgEngVirtCpuFunction : RtmObj, IRtmObjFunction
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="declFunction"></param>
      public RtmDbgEngVirtCpuFunction(IDeclFunction declFunction) : base(declFunction) { }

      /// <summary>
      /// Executes the function with the given stack, strategy, and parameters.
      /// </summary>
      /// <param name="stack">The stack to use for execution.</param>
      /// <param name="rtmStrategy">The strategy for handling runtime objects.</param>
      /// <param name="params">The parameters to pass to the function.</param>
      /// <returns>The result of the function execution, if any.</returns>
      public RtmObj? Exec(IRtmDbgEngStackExecutable? stack, IRtmObjStrategy? rtmStrategy, params RtmObj?[] @params)
      {
         var stk = stack.NnOrCrash();

         stk.Push(stk.MakeStackCall(this, @params));

         if (Decl?.Instructions.Length == 0) { return null; }
         else
         {
            var res = null as RtmObj;
            var ttf = stk.TopFunctionFrame.NnOrCrash();

            ttf.MoveToInstruction(ttf.Instructions.FirstOrDefault().NnOrCrash(), stack.NnOrCrash(), rtmStrategy);
          
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
