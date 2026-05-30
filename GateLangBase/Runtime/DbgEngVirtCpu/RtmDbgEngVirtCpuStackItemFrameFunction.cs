using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// Represent a function call inside <see cref="RtmDbgEngStackVirtCpu"/>, 
   /// which consist of a stack of <see cref="RtmDbgEngVirtCpuStackItemFrameFunction"/>
   /// </summary>
   public class RtmDbgEngVirtCpuStackItemFrameFunction : RtmDbgEngVirtCpuStackItemStackFrame, IRtmDbgEngStackFrameExecutableCall
   {
      public RtmDbgEngVirtCpuStackItemFrameFunction(
         RtmDbgEngVirtCpuFunction rtmObjFunction, RtmDbgEngStackVirtCpu stack, RtmObj?[] @params)
      {
         Stack = stack;
         RtmObjFunction = rtmObjFunction;
         CallParams = @params;
      }

      private class InnerFrameHelper
      {
         private class Frame : HierarchicalItem
         {
            public enum TypeType
            {
               function,
               stack_frame,
            }

            public Frame(
               RtmDbgEngVirtCpuInstructionPush push,
               RtmDbgEngVirtCpuInstructionFramePop pop,
               RtmDbgEngVirtCpuInstruction[] instructionsAll)
            {
               Push = push;
               Pop = pop;
               InstructionsAll = instructionsAll;
               InstrucionsOfFrame =
                  Enumerable.Range(Push.Idx.NnOrCrash(), Pop.Idx.NnOrCrash() - Push.Idx.NnOrCrash() + 1).
                  Select(i => InstructionsAll[i]).ToArray();
            }

            public Frame GetInstructionFrame(RtmDbgEngVirtCpuInstruction instruction) =>
               AllDescendant.OfType<Frame>().FirstOrDefault(f => f.InstrucionsOfFrame.Contains(instruction)).NnOrCrash();

            public TypeType Type =>
               Push is RtmDbgEngVirtCpuInstructionPushFunctionFrame ? TypeType.function : TypeType.stack_frame;


            public Frame? ParentFrame => ParentItem as Frame;

            public Frame[] AnchestorFrames => ParentItemChain.OfType<Frame>().ToArray();

            public RtmDbgEngVirtCpuInstructionPush Push { get; }
            public RtmDbgEngVirtCpuInstructionFramePop Pop { get; }
            public RtmDbgEngVirtCpuInstruction[] InstructionsAll { get; }

            /// <summary>
            /// 
            /// </summary>
            public RtmDbgEngVirtCpuInstruction[] InstrucionsOfFrame { get; private set; }

            public void MoveInsideFrame(RtmDbgEngVirtCpuInstruction targetInstruction, RtmDbgEngStackVirtCpu stack)
            {
               if (InstrucionsOfFrame.Contains(targetInstruction))
               {
                  stack.TopFunctionFrame.NnOrCrash().InstructionCurrent = targetInstruction;
               }
               else
               {
                  throw new Crash();
               }
            }

            public RtmDbgEngVirtCpuInstructionPush? FramePush =>
               InstrucionsOfFrame.FirstOrDefault() as RtmDbgEngVirtCpuInstructionPush;

            public Frame MoveToFrame(Frame targetFrame, RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
            {
               var lst_pic = targetFrame.ParentItemChain.OfType<Frame>().Reverse().ToList();

               if (lst_pic.Contains(this))
               {
                  var idx = lst_pic.IndexOf(this) + 1;

                  for (int i = idx; i < lst_pic.Count; i++)
                  {
                     lst_pic[i].FramePush?.NnOrCrash().Run(stack, rtmStrategy);
                  }

                  return targetFrame;
               }
               else
               {
                  throw new Crash($"Frame {targetFrame} not under {this}");
               }
            }

            public void AddFrames(Frame[] frames)
            {
               if (frames.Length != 0)
               {
                  myAddSubItemRange(frames);

                  var min = frames.Min(f => f.InstrucionsOfFrame.First().Idx).NnOrCrash();
                  var max = frames.Max(f => f.InstrucionsOfFrame.Last().Idx).NnOrCrash();

                  var it = new Interval(min, max);

                  InstrucionsOfFrame = InstrucionsOfFrame.Where(i => !it.Contains(i.Idx.NnOrCrash())).ToArray();
               }
            }

            public Frame[] SubFrames => SubItems.OfType<Frame>().ToArray();

            public override string ToString() => $"{Type} Frame {InstrucionsOfFrame.First()}-{InstrucionsOfFrame.Last()}";

         }

         /// <summary>
         /// Move virtual CPU execution context to the specified instruction,
         /// updating the stack through frame push/pop operations.
         /// </summary>
         public static void MoveToInstruction(
            RtmDbgEngVirtCpuInstruction targetInstruction, RtmDbgEngStackVirtCpu stack, IRtmObjStrategy rtmStrategy)
         {
            var tgt_idx = targetInstruction.Idx;
            var tff = stack.TopFunctionFrame.NnOrCrash();
            var ic = tff.InstructionCurrent;
            var iss = tff.Instructions;

            if (tgt_idx == 0)
            {
               if (targetInstruction is RtmDbgEngVirtCpuInstructionPushFunctionFrame)
               {
                  tff.InstructionCurrent = targetInstruction;

                  return;
               }
               else
               {
                  throw new Crash();
               }
            }
            else if (tgt_idx == tff.InstructionCurrentIdx + 1)
            {
               tff.InstructionCurrent = targetInstruction;
               return;
            }

            //instructions
            var fun_frm = GetFunctionFrame(iss).NnOrCrash();
            var frm_cur = fun_frm.GetInstructionFrame(ic.NnOrCrash());
            var tgt_frm = fun_frm.GetInstructionFrame(targetInstruction);

            // same frame
            if (frm_cur == tgt_frm)
            {
               frm_cur.MoveInsideFrame(targetInstruction, stack);
               return;
            }

            // current frame anchestor chain
            var cur_anc = frm_cur.AnchestorFrames;

            // target frame anchestor chain
            var tgt_anc = tgt_frm.AnchestorFrames;

            // Lowest Common Ancestor
            var lca = cur_anc.Intersect(tgt_anc).FirstOrDefault();

            if (lca == null)
            {
               throw new Crash("No common frame ancestor found.");
            }

            var frm = frm_cur;

            //
            // 1. walk downwards to LCA
            //
            while (frm != lca)
            {
               stack.ExitFrame(stack.TopStackFrame.NnOrCrash());
               frm = frm.ParentFrame.NnOrCrash();
            }

            frm = frm.MoveToFrame(tgt_frm, stack, rtmStrategy);
            frm.MoveInsideFrame(targetInstruction, stack);
         }

         private static Frame? GetFunctionFrame(RtmDbgEngVirtCpuInstruction[] instructions)
         {
            if (instructions.Length != 0)
            {
               var roo = new Frame(
                  (instructions?.FirstOrDefault()).ConvertOrCrash<RtmDbgEngVirtCpuInstructionPush>(),
                  (instructions?.LastOrDefault()).ConvertOrCrash<RtmDbgEngVirtCpuInstructionFramePop>(),
                  instructions.NnOrCrash());

               var frs = myGetSubFrames(roo);

               roo.AddFrames(frs);

               return roo;
            }

            return null;
         }

         private static Frame[] myGetSubFrames(Frame frame)
         {
            var cnt = 0;
            var lst = new List<Frame>();
            var psh = null as RtmDbgEngVirtCpuInstructionPush;

            //last and first instruction (function frame stack) shall be discarded, so skip 1 and take length - 2
            var iss = frame.InstrucionsOfFrame.Skip(1).Take(frame.InstrucionsOfFrame.Length - 2).ToArray();

            foreach (var ins in iss)
            {
               if (ins is RtmDbgEngVirtCpuInstructionPush p)
               {
                  if (cnt++ == 0)
                  {
                     if (psh != null)
                     {
                        throw new Crash();
                     }
                     else
                     {
                        psh = p;
                     }
                  }
               }
               else if (ins is RtmDbgEngVirtCpuInstructionFramePop pop)
               {
                  if (--cnt == 0)
                  {
                     if (psh != null)
                     {
                        var frm = new Frame(psh, pop, frame.InstructionsAll);

                        psh = null;
                        lst.Add(frm);
                        frm.AddFrames(myGetSubFrames(frm));
                     }
                  }
                  else if (cnt < 0)
                  {
                     throw new Crash();
                  }
               }
            }

            if (psh != null)
            {
               throw new Crash();
            }
            else
            {
               return lst.ToArray();
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuFunction RtmObjFunction { get; }

      public RtmObj?[] CallParams { get; }

      /// <summary>
      /// 
      /// </summary>
      public int InstructionCurrentIdx =>
         InstructionCurrent != null ? Instructions.ToList().IndexOf(InstructionCurrent) : -1;

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuInstruction? InstructionCurrent { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="targetInstruction"></param>
      /// <param name="stack"></param>
      /// <param name="rtmStrategy"></param>
      public void MoveToInstruction(
         RtmDbgEngVirtCpuInstruction targetInstruction, RtmDbgEngStackVirtCpu stack, IRtmObjStrategy rtmStrategy) =>
            InnerFrameHelper.MoveToInstruction(targetInstruction, stack, rtmStrategy);

      public void MoveInstructionNext(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy rtmStrategy)
      {
         var idx = InstructionCurrentIdx;

         if (idx < 0 || idx + 1 >= Instructions.Length)
         {
            throw new Crash("Can't set target instruction");
         }
         else
         {
            MoveToInstruction(Instructions[idx + 1], stack, rtmStrategy);
         }
      }

      /// <summary>
      /// Move to end of current stack-frame
      /// assumption you are already inside the frame
      /// </summary>
      /// <param name="stack"></param>
      /// <param name="rtmStrategy"></param>
      /// <exception cref="Crash"></exception>
      public void MoveStackFrameEnd(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy rtmStrategy)
      {
         var cnt = 0;
         var idx = InstructionCurrentIdx;

         for (int i = idx + 1; i < Instructions.Length; i++)
         {
            if (Instructions[i] is RtmDbgEngVirtCpuInstructionPush)
            {
               cnt++;
            }
            else if (Instructions[i] is RtmDbgEngVirtCpuInstructionFramePop)
            {
               if (--cnt == 0)
               {
                  MoveToInstruction(Instructions[i], stack, rtmStrategy);
                  return;
               }
            }
         }

         throw new Crash("End frame not found");
      }

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
               lst_ojs.AddRange(RtmObjFunction.Module.ObjectsPersistant);
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
      }

      IRtmDbgEngInstruction[] IRtmDbgEngStackFrameExecutableCall.Instructions => Instructions;

      public override string ToString() => $"Function Frame: {FunctionInfo}";

      void IRtmDbgEngStackFrameExecutableCall.MoveToInstruction(
         IRtmDbgEngInstruction targetInstruction, IRtmDbgEngStackExecutable stack, IRtmObjStrategy? rtmStrategy) =>
         MoveToInstruction(
            targetInstruction.ConvertOrCrash<RtmDbgEngVirtCpuInstruction>(),
            stack.ConvertOrCrash<RtmDbgEngStackVirtCpu>(),
            rtmStrategy.NnOrCrash());
   }
}
