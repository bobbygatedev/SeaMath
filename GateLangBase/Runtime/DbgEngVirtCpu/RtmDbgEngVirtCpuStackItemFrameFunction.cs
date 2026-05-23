using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// Represent a function call inside <see cref="RtmDbgEngStackVirtCpu"/>, 
   /// which consist of a stack of <see cref="RtmDbgEngVirtCpuStackItemFrameFunction"/>
   /// </summary>
   public class RtmDbgEngVirtCpuStackItemFrameFunction : RtmDbgEngVirtCpuStackItemStackFrame, IRtmDbgEngStackFrameExecutableCall
   {
      public RtmDbgEngVirtCpuStackItemFrameFunction(RtmObjFunction rtmObjFunction, RtmDbgEngStackVirtCpu stack)
      {
         Stack = stack;
         RtmObjFunction = rtmObjFunction;
      }

      /// <summary>
      /// 
      /// </summary>
      public RtmObjFunction RtmObjFunction { get; }

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuInstruction? InstructionCurrent { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuInstruction? InstructionNext
      {
         get
         {
            if (InstructionCurrent != null)
            {
               var idx = Instructions.ToList().IndexOf(InstructionCurrent);

               return idx >= 0 ? Instructions.Skip(idx + 1).FirstOrDefault() : null;
            }
            else
            {
               return null;
            }
         }
      }

      /// <summary>
      /// Next instruction can cause halt (shall have <see cref="RtmDbgEngVirtCpuInstruction.Token"/> not null.
      /// </summary>
      public RtmDbgEngVirtCpuInstruction? InstructionNextHalt
      {
         get
         {
            if (InstructionCurrent != null)
            {
               var idx = Instructions.ToList().IndexOf(InstructionCurrent);

               return idx >= 0 ? Instructions.Skip(idx + 1).FirstOrDefault(i => i.Token != null) : null;
            }
            else
            {
               throw new Gate.LangBase.Runtime.RtmException("Not a valid current instruction");
            }
         }
      }

      /// <summary>
      /// <br> <see cref="RtmObj"/> instance in this frame (from stack function stack frame to next one)</br>
      /// <br> NOTICE anonimous object are included</br>
      /// </summary>
      public RtmObj[] ObjInStackOnly
      {
         get
         {
            var lst_its = Stack.Items.ToList();
            var idx_my = lst_its.IndexOf(this);
            var nxt_cal = lst_its.Take(idx_my).OfType<RtmDbgEngVirtCpuStackItemFrameFunction>().LastOrDefault();

            if (nxt_cal != null)
            {
               var nxt_cal_idx = lst_its.IndexOf(nxt_cal);

               return lst_its.Skip(nxt_cal_idx).Take(idx_my - nxt_cal_idx).OfType<RtmObj>().ToArray();
            }
            else { return lst_its.Take(idx_my).OfType<RtmObj>().ToArray(); }
         }
      }

      /// <summary>
      /// <see cref="ObjLocalsAll"/> + globals + static objects from the current function module.
      /// </summary>
      public RtmObj[] ObjAll
      {
         get
         {
            var lst_ojs = ObjLocalsAll.ToList();

            if (RtmObjFunction?.Module != null)
            {
               lst_ojs.AddRange(RtmObjFunction.Module.ObjectsPersistant.Where(o => o.Decl?.Visibility == ExprDeclVisibility.global_static));
            }

            if (Stack?.Thread?.Process?.ObjsGlobal != null)
            {
               lst_ojs.AddRange(Stack?.Thread?.Process?.ObjsGlobal ?? []);
            }

            return RtmObj.GetUniqueById(lst_ojs);
         }
      }

      /// <summary>
      /// <br> Comprise <see cref="ObjInStackOnly"/> and all static objects in nested function(s).</br>
      /// <br> eg void func(int p1) { void nested(void) { static int v1 } } </br>
      /// <br> in this case <see cref="ObjInStackOnly"/> contains p1, while <see cref="ObjLocalsAll"/> contains p1,v1  </br>
      /// <br> NOTICE anonimous object are omiited</br>
      /// </summary>
      public RtmObj[] ObjLocalsAll
      {
         get
         {
            //function frames from top to bottom (LIFO) 
            var lst_fnc_frs = Stack.FunctionFrames.ToList();
            var idx = lst_fnc_frs.IndexOf(this);
            lst_fnc_frs = lst_fnc_frs.Skip(idx).ToList();

            if (lst_fnc_frs.Count > 0)
            {
               // all stack objects of top + static objects parent frame (nested function calls) eg
               // void f1(void) {
               //  static int s1 = 1;
               //  int f2(void) { int a = 2; return s1 + a; }
               //  f2 "see" static vars of containg function
               var ojs = lst_fnc_frs[0].ObjInStackOnly.
                  Concat(lst_fnc_frs.Skip(1).
                  SelectMany(
                     c => c.ObjLocalsAll.
                        Where(o => o.Decl?.Visibility == ExprDeclVisibility.local_static))).ToArray();

               //remove all duplicated names (items on stack top have precedence)
               return RtmObj.GetUniqueById(ojs);
            }
            else { return []; }
         }
      }

      public RtmDbgEngStackVirtCpu Stack { get; }

      public IRtmDbgEngPoint? DbgPointCurrent => InstructionCurrent;

      public string? FunctionName => RtmObjFunction?.VarName;

      public string FunctionInfo => RtmObjFunction.Decl != null ?
         $"{RtmObjFunction.Decl.ReturnType} {RtmObjFunction.VarName}" +
            $"({string.Join("", RtmObjFunction.Decl.Parameters.Select(p => p.DeclType?.TypeSpecifier))})" :
         $"{RtmObjFunction.VarName}()";

      public RtmDbgEngVirtCpuInstruction[] Instructions => RtmObjFunction?.Decl?.Instructions ?? [];

      IRtmDbgEngInstruction? IRtmDbgEngStackFrameExecutableCall.InstructionCurrent
      {
         get => InstructionCurrent;
         set => InstructionCurrent = value as RtmDbgEngVirtCpuInstruction;
      }

      IRtmDbgEngInstruction[] IRtmDbgEngStackFrameExecutableCall.Instructions => Instructions;

      public override string ToString() => $"Function Frame: {FunctionInfo}";
   }
}
