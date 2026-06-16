using Gate.CLanguage.Decl;
using Gate.LangBase.Expressions;
using Gate.LangBase.Expressions.Operators;
using Gate.LangBase.Runtime;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;
using System.Reflection;
using static Gate.SeaMath.Workspace.Libs.SeaMathLibCSharp;

namespace Gate.SeaMath.Workspace.Libs
{
   public class SeaMathLibCSharpDeclFunction : CDeclFunction
   {
      public SeaMathLibCSharpDeclFunction(MethodInfo methodInfo, bool isSysMethod) : base(false, KindType.library, null)
      {
         MethodInfo = methodInfo;
         IsSysMethod = isSysMethod;
         Instructions = [new InnerInstruction(this)];
      }

      private unsafe class InnerInstruction : RtmDbgEngVirtCpuInstructionGotoNext
      {
         public InnerInstruction(SeaMathLibCSharpDeclFunction parent) : base(null) => Parent = parent;

         public override string Name => "c# lib function execution";

         public SeaMathLibCSharpDeclFunction Parent { get; }

         protected override void myRun(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
         {
            if (Parent.IsSysMethod)
            {
               myRunSys(stack, rtmStrategy);
            }
            else
            {
               myRunOrdinary(stack, rtmStrategy);
            }
         }

         private void myRunOrdinary(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
         {
            var ttf = stack.FunctionFrames.FirstOrDefault().NnOrCrash();
            var i_dcl_fnc = Parent as IDeclFunction;
            var prs_stk = stack.FunctionCallParameters;
            var par_dcs = i_dcl_fnc.Parameters;
            var eff_arg_cnt = par_dcs.Length;
            var mis_ars = i_dcl_fnc.Parameters.Skip(prs_stk.Length).ToArray();
            var mis_ars_dfs = (mis_ars ?? []).
               Select(ma => stack.Thread?.Process?.RtmStrategy.MakeNewObject(ma) ?? throw new Crash()).ToArray();

            prs_stk = prs_stk.Concat(mis_ars_dfs ?? []).ToArray();

            var prs = Parent.MethodInfo.GetParameters();

            //call parameter c# object
            var cs_par_obs = prs_stk.Select(p => p?.CSharpObj ?? throw new Crash()).Cast<object>().ToArray();

            var eff_ars = null as object[];

            if (i_dcl_fnc.HasVarArgs)
            {
               eff_ars = cs_par_obs.Take(eff_arg_cnt).Append(cs_par_obs.Skip(eff_arg_cnt).Cast<object>().ToArray()).ToArray();
            }
            else
            {
               eff_ars = cs_par_obs.Take(eff_arg_cnt).ToArray();
            }

            myExecuteMethod(stack, i_dcl_fnc, eff_ars);
         }

         private void myExecuteMethod(RtmDbgEngStackVirtCpu stack, IDeclFunction declFunction, object[] csMethodInfo)
         {
            try
            {
               var res = Parent.MethodInfo.Invoke(Parent.ParentLib, csMethodInfo);

               /// this is the case <see cref="MethodInfo"/> returns a pointer like <see cref="SeaMathLibStdio.DoGets(sbyte*)"/>
               if (res is Pointer ptr)
               {
                  res = (IntPtr)Pointer.Unbox(ptr);
               }

               if (res is ValueType val)
               {
                  var ro = stack.Thread?.Process?.RtmStrategy.MakeConstant((ValueType)res, declFunction.ReturnType);

                  stack.Return(ro);
               }
               else if (res is RtmObj rr)
               {
                  stack.Return(rr);
               }
            }
            catch (RtmException) { throw; }
            catch (TargetInvocationException exc)
            {
               if (exc.InnerException != null)
               {
                  throw exc.InnerException;
               }
               else
               {
                  throw new Crash();
               }
            }
            catch (ThreadInterruptedException) { throw; }
            catch { throw new Crash($"Failed {Parent.MethodInfo} in C# library {GetType()}"); }
         }

         private void myRunSys(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
         {
            var i_dcl_fnc = Parent as IDeclFunction;
            var prs_stk = stack.TopFunctionFrame?.CallParams.Select(p => p.NnOrCrash()).ToArray() ?? [];//uses parameter before parameter copy
            var eff_arg_cnt = i_dcl_fnc.Parameters.Length;

            //effective arguments converted to object[]
            var eff_ars = prs_stk.Take(eff_arg_cnt).Cast<object>().ToArray();

            if (i_dcl_fnc.HasVarArgs)
            {
               //optional parameter array
               RtmObj[] ops = prs_stk.Skip(eff_arg_cnt).ToArray();

               //effective arguments array as object[] + array of optional parameters
               eff_ars = eff_ars.Append(ops).ToArray();
            }

            myExecuteMethod(stack, i_dcl_fnc, eff_ars);
         }
      }

      public MethodInfo MethodInfo { get; }

      public MethodAttribute? MethodAttribute => MethodInfo.GetCustomAttribute<MethodAttribute>();

      public bool IsSysMethod { get; }

      public string? OverridePunctuator => MethodAttribute?.PunctuatorOverride;

      public int NumOverridePunctuator => OverridePunctuator != null ? MethodInfo.GetParameters().Length : -1;

      public bool IsOperatorOverride(Operator? @operator, int length) =>
         @operator is OperatorPunctuator pnc && pnc.Punctuator == OverridePunctuator && MethodInfo.GetParameters().Length == length;

      public SeaMathLibCSharp? ParentLib => ParentItemChain.OfType<SeaMathLibCSharp>().FirstOrDefault();
   }
}
