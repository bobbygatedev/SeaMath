using Gate.CLanguage.Decl;
using Gate.LangBase.Expressions;
using Gate.LangBase.Expressions.Operators;
using Gate.LangBase.Runtime;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
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
         Instructions = [new RtmDbgEngVirtCpuInstructionSimple(null, (stk, str) => myRunAction(stk))];
      }

      public MethodInfo MethodInfo { get; }

      public MethodAttribute? MethodAttribute => MethodInfo.GetCustomAttribute<MethodAttribute>();

      public bool IsSysMethod { get; }

      public string? OverridePunctuator => MethodAttribute?.PunctuatorOverride;

      public int NumOverridePunctuator => OverridePunctuator != null ? MethodInfo.GetParameters().Length : -1;

      public bool IsOperatorOverride(Operator? @operator, int length) =>
         @operator is OperatorPunctuator pnc && pnc.Punctuator == OverridePunctuator && MethodInfo.GetParameters().Length == length;

      public SeaMathLibCSharp? ParentLib => ParentItemChain.OfType<SeaMathLibCSharp>().FirstOrDefault();

      private unsafe RtmObj? myRunAction(RtmDbgEngStackVirtCpu stack)
      {
         var i_dcl_fnc = this as IDeclFunction;
         var prs_stk = stack.TopFunctionFrame?.ObjInStackOnly.Reverse().ToArray();
         var par_dcs = i_dcl_fnc.Parameters;
         var eff_arg_cnt = i_dcl_fnc.Parameters.Length;
         var mis_ars = i_dcl_fnc.Parameters.Skip(prs_stk?.Length ?? 0).ToArray();
         var mis_ars_dfs = mis_ars.Select(ma => stack.Thread?.Process?.RtmStrategy.MakeNewObject(ma) ?? throw new Crash()).ToArray();

         prs_stk = (prs_stk ?? []).Concat(mis_ars_dfs).ToArray();

         var prs = MethodInfo.GetParameters();
         var eff_ars = null as object[];
         var cal_par_obs = null as object[];

         if (IsSysMethod)
         {
            eff_ars = i_dcl_fnc.HasVarArgs ?
               prs_stk.Take(eff_arg_cnt).Cast<object>().Append(prs_stk.Skip(eff_arg_cnt).ToArray()).ToArray() :
               prs_stk.Take(eff_arg_cnt).Cast<object>().ToArray();
         }
         else
         {
            cal_par_obs = prs_stk.Select(p => p?.CSharpObj ?? throw new Crash()).ToArray();

            eff_ars = i_dcl_fnc.HasVarArgs ?
               cal_par_obs.Take(eff_arg_cnt).Append(cal_par_obs.Skip(eff_arg_cnt).Cast<object>().ToArray()).ToArray() :
               cal_par_obs.Take(eff_arg_cnt).ToArray();
         }

         try
         {
            var res = MethodInfo.Invoke(ParentLib, eff_ars);

            /// this is the case <see cref="MethodInfo"/> returns a pointer like <see cref="SeaMathLibStdio.DoGets(sbyte*)"/>
            if (res is Pointer ptr)
            {
               res = (IntPtr)Pointer.Unbox(ptr);
            }

            return res is ValueType val ?
               stack.Thread?.Process?.RtmStrategy.MakeConstant((ValueType)res, i_dcl_fnc.ReturnType) :
               res as RtmObj;
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
         catch { throw new Crash($"Failed {MethodInfo} in C# library {GetType()}"); }
      }
   }
}
