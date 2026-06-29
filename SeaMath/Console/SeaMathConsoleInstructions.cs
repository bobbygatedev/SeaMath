using Gate.CLanguage.Decl;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Runtime;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.SeaMath.Workspace.Libs;
using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.SeaMath.Console
{
   /// <summary>
   /// Console instruction container class.
   /// </summary>
   public static class SeaMathConsoleInstructions
   {
      /// <summary>
      /// Console instruction consisting of a <see cref="CExprStatement"/> invokation. 
      /// </summary>
      public class ForExpressions : RtmDbgEngVirtCpuInstruction
      {
         /// <summary>
         /// 
         /// </summary>
         /// <param name="expr"></param>
         /// <param name="console"></param>
         public ForExpressions(CExprStatement expr, SeaMathConsole console) : base(null)
         {
            ExprStatement = expr;
            Console = console;
         }

         public override string Name => "console_expression";

         /// <summary>
         /// 
         /// </summary>
         public CExprStatement ExprStatement { get; }

         /// <summary>
         /// 
         /// </summary>
         public SeaMathConsole Console { get; }

         /// <summary>
         /// A c# method can indicate <see cref="SeaMathLibCSharp.MethodAttribute.IsConsoleOmitReturn"/> as an attribute
         /// </summary>
         public bool IsOmitReturn
         {
            get
            {
               if (ExprStatement?.Expr?.RootNode is ExprNodeOperator opr && opr.Operator is OperatorCall oc)
               {
                  var v = opr.OperandNodes.FirstOrDefault() as ExprNodeOperandVariable;

                  if (v.Decl is SeaMathLibCSharpDeclFunction lf)
                  {
                     return lf.MethodAttribute?.IsConsoleOmitReturn ?? false;
                  }
               }

               return false;
            }
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="stack"></param>
         /// <param name="rtmStrategy"></param>
         public override void Run(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
         {
            var res = ExprStatement?.Expr?.Eval(stack, rtmStrategy);

            Console.ConsoleStrategy.BeforeInstructionRun();

            if (!IsOmitReturn)
            {
               (Console.Writer ?? throw new Crash()).WriteLine($"ret = {(res != null ? res.DisplayValue : "void")}");
            }
         }
      }

      /// <summary>
      /// Instruction consisting of a variable instantation and init.
      /// </summary>
      public class ForVariable : RtmDbgEngVirtCpuInstruction
      {
         /// <summary>
         /// 
         /// </summary>
         /// <param name="declVar"></param>
         /// <param name="processVirtual"></param>
         /// <param name="rtmStrategy"></param>
         public ForVariable(
            CDeclVar declVar,
            CRtmObjStrategy rtmStrategy,
            SeaMathConsole console) : base(null)
         {
            DeclVar = declVar;
            RtmStrategy = rtmStrategy;
            Console = console;
         }

         public override string Name => "console_var";

         /// <summary>
         /// 
         /// </summary>
         public CDeclVar DeclVar { get; }

         public CRtmObjStrategy RtmStrategy { get; }

         public SeaMathConsole Console { get; }

         public override void Run(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
         {
            var pro = RtmDbgEngVirtCpuThread.GetRunningThread()?.Process ?? throw new Crash();

            //is local object (eg n C/C++ local var or function param)
            var rtm_var = RtmStrategy.MakeNewObject(DeclVar);
            var rtm_ojs = pro.AdditionalObjectsRuntime;

            //search an existing variable of same var name
            var rtm_var_old = rtm_ojs.FirstOrDefault(o => o.VarName == DeclVar.Identifier);

            //remove possibly old value and replace with new one 
            rtm_ojs = rtm_ojs.Except([rtm_var_old]).Nn().ToArray();
            rtm_ojs = rtm_ojs.Append(rtm_var).ToArray();
            pro.AdditionalObjectsRuntime = rtm_ojs.Nn().ToArray();
            DeclVar.OwnedInit?.DoInit(rtm_var, stack, RtmStrategy);

            if (!(ParentItem is SeaMathConsoleCommandInstructions ins && ins.Instructions.OfType<ForExpressions>().Any()))
            {
               (Console.Writer ?? throw new Crash()).WriteLine($"{rtm_var.VarName}={rtm_var.DisplayValue}");
               Console.Writer.Flush();
            }
         }
      }
   }
}
