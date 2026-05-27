using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Statement;
using Gate.CLanguage.Types;
using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;
using static Gate.CLanguage.Statement.CStatement;
using static Gate.Tools.HierarchicalItem;

namespace Gate.CLanguage.Runtime
{
   /// <summary>
   /// 
   /// </summary>
   public class CFunctionInstructionTranslator : IFunctionInstructionTranslator
   {
      /// <summary>
      /// 
      /// </summary>
      public CFunctionInstructionTranslator(CRtmObjStrategy rtmStrategy) => RtmStrategy = rtmStrategy;

      public CRtmObjStrategy RtmStrategy { get; }

      public virtual RtmDbgEngVirtCpuInstruction[] GetInstructions(CItem item) => myGetInstructions((dynamic)item);

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(Return item) =>
         [new RtmDbgEngVirtCpuInstructionReturn(item?.TxtToken, item?.Expression?.Expr)];

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CDeclSpecifiers declSpecifiers) =>
         declSpecifiers.Decls.SelectMany(d => (RtmDbgEngVirtCpuInstruction[])myGetInstructions((dynamic)d)).ToArray();

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CItem item) => throw new Crash();

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CStatementCompound compound) =>
         compound.SubItems.SelectMany(itm => (RtmDbgEngVirtCpuInstruction[])myGetInstructions((dynamic)itm)).ToArray();

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(Collection<CAttribute> genObject) => [];

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(object obj) => throw new Crash($"Object of type {obj.GetType()} not allowed");

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CDeclVar declVar)
      {
         //persistant (global)
         var is_prs =
            declVar.IsGlobal && declVar.IsDefinition ||
            declVar.StorageClass == CTypeStorageClass.@static;

         var ini_glo_cnt = 0;

         return [ new RtmDbgEngVirtCpuInstructionSimple(declVar.DeclSpecifiers?.TxtToken,  (rtm_stk,rtm_str) =>
            {
               if (is_prs)
               {
                  //is persistant (eg in C/C++ a static or not extern global object) and not yet init
                  var rtm_ojs_prs = rtm_stk.Thread?.Process?.ObjsPersistant;
                  var rtm_obj_prs =
                     rtm_ojs_prs?.FirstOrDefault(g => g.Decl == declVar) ??
                     throw new Gate.LangBase.Runtime.RtmException($"Can't find '{declVar}'");

                  //performs init of object if not done yet
                  if ( ini_glo_cnt++ == 0 && declVar.OwnedInit != null)
                  {
                     declVar.OwnedInit.DoInit(rtm_obj_prs, rtm_stk, RtmStrategy);
                  }

                  //static(non global) object shall be visible only in context of its function 
                  if (rtm_obj_prs?.Decl?.Visibility == ExprDeclVisibility.local_static)
                  {
                     rtm_stk.Push(rtm_obj_prs);
                  }
               }
               else
               {
                  //is local object (eg n C/C++ local var or function param)
                  var var_rtm_loc = null as RtmObj;

                  var_rtm_loc = rtm_stk?.TopFunctionFrame?.ObjAll.FirstOrDefault(o=>o.Decl == declVar);

                  if ( var_rtm_loc == null )
                  {
                     var_rtm_loc = RtmStrategy.MakeNewObject(declVar);
                  }

                  rtm_stk?.Push(var_rtm_loc);

                  if (declVar.OwnedInit != null) { declVar.OwnedInit.DoInit(var_rtm_loc, rtm_stk??throw new Crash(), RtmStrategy); }
               }

               return null;
            }) ];
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name=""></param>
      /// <param name="runTimeModule"></param>
      /// <returns></returns>
      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CDeclFunction declFunction)
      {
         var lnk_fnc =
            declFunction.IsDefinition ? declFunction : null ??
            declFunction.Linkage as CDeclFunction ??
            throw new Gate.LangBase.Runtime.RtmException($"Declared {declFunction.Descriptor} has not linkage");

         //items inside of function body's block 
         var its = lnk_fnc?.Body?.SubItems.OfType<CItem>().ToArray() ?? [];

         //items after tranlation into instructions
         var iss = its.SelectMany(i => GetInstructions(i)).ToArray();

         if (iss.Length == 0 || iss.All(i => i.Token == null))
         {
            //last position inside function body (or null)
            var to_fnc_pos = lnk_fnc?.Body?.TxtToken?.To?.Primitive;

            //token to last line inside function body (or null)
            var to_fnc_tok = to_fnc_pos != null ? to_fnc_pos.Store?[to_fnc_pos.Line] : null;

            //adds a nop at last function line because a empty function causes an infinite loop 
            return [new RtmDbgEngVirtCpuInstructionSimple(to_fnc_tok)];
         }
         else
         {
            return iss;
         }
      }

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CExprStatement cExpr) =>
         [new RtmDbgEngVirtCpuInstructionSimple(cExpr.TxtToken, (stk, str) => cExpr.Expr?.Eval(stk, RtmStrategy))];

      /// <summary>
      /// 
      /// </summary>
      /// <param name="ifElse"></param>
      /// <returns></returns>
      /// <exception cref="System.NotImplementedException"></exception>
      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CStatementIfElse ifElse)
      {
         // 0: frame
         // 1: if cond1 body1 end else cond2
         // 2: body1 
         // 3: goto end
         // 4: if cond2 body2 end else else_body
         // 5: body2 
         // 6: goto end
         // 7: else_body
         // 8: end (pop frame)

         ///list of all if-else's
         var if_els = new[] { ifElse }.Concat(ifElse.IfElses).ToArray();

         var lst_ins = new List<RtmDbgEngVirtCpuInstruction?>();

         //frame pop(end)
         var end_frm_pop = new RtmDbgEngVirtCpuInstructionFramePop();

         /// 0: frame
         lst_ins.Add(new RtmDbgEngVirtCpuInstructionFramePush());

         for (int i = 0; i < if_els.Length; i++)
         {
            lst_ins.Add(null);//goto later

            // body instructions 
            lst_ins.AddRange(myGetCycleBodyInstructions(if_els[i]));

            //always goto end frame
            lst_ins.Add(new RtmDbgEngVirtCpuInstructionGoto(if_els[i].TxtToken, end_frm_pop, null));
         }

         var els_0 = null as RtmDbgEngVirtCpuInstruction;

         //if else is defined
         if (if_els.LastOrDefault().NnOrCrash().ElseBody != null)
         {
            var iss = myGetCycleBodyInstructions(if_els.LastOrDefault().NnOrCrash(), true);

            els_0 = iss.FirstOrDefault().NnOrCrash();

            // else body instructions goto end not needed
            lst_ins.AddRange(iss);
         }

         //placing gotos where instruction is null yet
         var got_ids = Enumerable.Range(0, lst_ins.Count).Where(i => lst_ins[i] == null).NnOrCrash().ToArray();

         if (got_ids.Length != if_els.Length) { throw new Crash(); }
         else
         {
            for (var i = got_ids.Length - 1; i >= 0; i--)
            {
               var id = got_ids[i];
               var exp = if_els[i].StayCondition.NnOrCrash().Expr.NnOrCrash();

               if (i == got_ids.Length - 1)
               {
                  //if true next instruction otherwise else_body or end
                  lst_ins[id] = new RtmDbgEngVirtCpuInstructionGoto(
                     if_els[i].StayCondition?.TxtToken, null, els_0 ?? end_frm_pop, exp);
               }
               else
               {
                  //if true next instruction otherwise else_body or end
                  lst_ins[id] = new RtmDbgEngVirtCpuInstructionGoto(
                     if_els[i].StayCondition?.TxtToken, null, lst_ins[got_ids[i + 1]], exp);
               }
            }
         }

         //6: end(pop frame)
         lst_ins.Add(end_frm_pop);

         return lst_ins.Select(i => i.NnOrCrash()).ToArray();
      }

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CStatementLoopWhile whileCycle)
      {
         // 0: frame
         // 1: if-condition-goto (eg 'while(i<3) { printf(""); }' -> if i < 3 '2: body' else goto '4: end'
         // 2: body 
         // 3: goto if-condition-goto
         // 4: end (pop frame)
         var lst_ins = new List<RtmDbgEngVirtCpuInstruction>();

         //frame pop(end)
         var end_frm_pop = new RtmDbgEngVirtCpuInstructionFramePop();

         /// 0: frame
         lst_ins.Add(new RtmDbgEngVirtCpuInstructionFramePush());

         // 1: if-condition-goto 
         lst_ins.Add(
            new RtmDbgEngVirtCpuInstructionGoto(
               whileCycle.StayCondition?.TxtToken,
               null,
               end_frm_pop,
               whileCycle.StayCondition.NnOrCrash().Expr.NnOrCrash()));

         lst_ins.AddRange(myGetCycleBodyInstructions(whileCycle));

         //5: goto if-condition-goto '1: if-condition-goto'
         var got = new RtmDbgEngVirtCpuInstructionGoto(null, lst_ins[1], null, null);

         lst_ins.Add(got);

         //6: end(pop frame)
         lst_ins.Add(end_frm_pop);

         myPatchBreakContinue(lst_ins, end_frm_pop, got);

         return lst_ins.ToArray();
      }

      private static void myPatchBreakContinue(
         List<RtmDbgEngVirtCpuInstruction> listInstructions,
         RtmDbgEngVirtCpuInstructionFramePop endFramePop,
         RtmDbgEngVirtCpuInstruction continueTarget)
      {
         foreach (var brk in listInstructions.ToArray().OfType<InnerDummyBreak>())
         {
            //break skipping to frame pop
            listInstructions[listInstructions.IndexOf(brk)] = new RtmDbgEngVirtCpuInstructionGoto(brk.Break.TxtToken, endFramePop, null);
         }

         foreach (var brk in listInstructions.ToArray().OfType<InnerDummyContinue>())
         {
            //break skipping to cycle goto
            listInstructions[listInstructions.IndexOf(brk)] = new RtmDbgEngVirtCpuInstructionGoto(brk.Continue.TxtToken, continueTarget, null);
         }
      }

      private class InnerDummyBreak : RtmDbgEngVirtCpuInstruction
      {
         public InnerDummyBreak(Break @break) : base(null) => Break = @break;

         public Break Break { get; }

         public override string Name => "dummy_break";

         public override void Run(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy) => throw new Crash("Dummy");
      }

      private class InnerDummyContinue : RtmDbgEngVirtCpuInstruction
      {
         public InnerDummyContinue(Continue @continue) : base(null) => Continue = @continue;

         public Continue Continue { get; }

         public override string Name => "dummy_continue";

         public override void Run(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy) => throw new Crash("Dummy");
      }

      private RtmDbgEngVirtCpuInstruction[] myGetCycleBodyInstructions(CStatementConditional cycle, bool isElse = false)
      {
         var lst_ins = new List<RtmDbgEngVirtCpuInstruction>();
         var sts = null as CStatement[];

         var bdy = isElse ? cycle.ConvertOrCrash<CStatementIfElse>().ElseBody : cycle.Body;

         //eg for(;;){ a*=2; }
         if (bdy is CStatementCompound cmp) { sts = cmp.Statements; }
         //eg do a*=2 while(i+<3);
         else if (bdy is CStatement sta) { sts = [sta]; }
         //eg for(;;); -> infinite cycle
         else if (bdy != null) { throw new Crash(); }

         foreach (var sta in sts.NnOrCrash())
         {
            if (sta is Break brk)
            {
               lst_ins.Add(new InnerDummyBreak(brk));
            }
            else if (sta is Continue cnt)
            {
               lst_ins.Add(new InnerDummyContinue(cnt));
            }
            else
            {
               lst_ins.AddRange(GetInstructions(sta));
            }
         }

         if (lst_ins.Count == 0)
         {
            lst_ins.Add(new RtmDbgEngVirtCpuInstructionSimple(null));//adding "nop"
         }

         return lst_ins.ToArray();
      }

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CStatementLoopDoWhile doWhileCycle)
      {
         // 0: frame (push frame)
         // 1: body 
         // 2: if-condition-goto (eg 'do { printf(""); } while(i<3);' -> if i < 3 goto '1: body' else goto '3: end')
         // 3: end (pop frame)

         var lst_ins = new List<RtmDbgEngVirtCpuInstruction>
         {
            /// 0: frame
            new RtmDbgEngVirtCpuInstructionFramePush()
         };

         lst_ins.AddRange(myGetCycleBodyInstructions(doWhileCycle));

         // 2: if-condition-goto 
         var got = new RtmDbgEngVirtCpuInstructionGoto(
               doWhileCycle.StayCondition?.TxtToken,
               lst_ins[1],
               null,
               doWhileCycle.StayCondition.NnOrCrash().Expr.NnOrCrash());

         lst_ins.Add(got);

         var end_frm_pop = new RtmDbgEngVirtCpuInstructionFramePop();

         //3: end(pop frame)
         lst_ins.Add(end_frm_pop);

         myPatchBreakContinue(lst_ins, end_frm_pop, got);

         return lst_ins.ToArray();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="forCycle"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CStatementLoopFor forCycle)
      {
         // 0: frame
         // 1: init (optional)
         // 2: if-condition-goto (eg 'for(i=0; i<3;i++) { printf(""); }' -> if i < 3 '3: body' else goto '6: end'
         // 3: body 
         // 4: update
         // 5: goto if-condition-goto
         // 6: end (pop frame)

         var lst_ins = new List<RtmDbgEngVirtCpuInstruction>
            {
               /// 0: frame
               new RtmDbgEngVirtCpuInstructionFramePush()
            };

         //frame pop(end)
         var end_frm_pop = new RtmDbgEngVirtCpuInstructionFramePop();

         /// 1: init
         if (forCycle.Initialisation != null) { lst_ins.AddRange(GetInstructions(forCycle.Initialisation)); }

         var cnd = null as Expr;

         //if condition is null infinite cycle
         if (forCycle.StayCondition != null)
         {
            cnd = forCycle.StayCondition?.Expr.NnOrCrash();
         }

         var got = new RtmDbgEngVirtCpuInstructionGoto(forCycle.StayCondition?.TxtToken, null, end_frm_pop, cnd);

         // 2: if-condition-goto 
         lst_ins.Add(got);

         lst_ins.AddRange(myGetCycleBodyInstructions(forCycle));

         var upd = null as RtmDbgEngVirtCpuInstruction;

         // 4: update
         if (forCycle.Update != null)
         {
            var new_iss = GetInstructions(forCycle.Update);

            upd = new_iss.ElementAtOrCrash(0);
            lst_ins.AddRange(new_iss);
         }

         //5: goto if-condition-goto 
         lst_ins.Add(new RtmDbgEngVirtCpuInstructionGoto(null, got, null, null));

         //6: end(pop frame)
         lst_ins.Add(end_frm_pop);

         //if update is defined continue shall jump to it ( eg for ( i = 0; ; i++ ) continue; -> jump to 'i++')
         myPatchBreakContinue(lst_ins, end_frm_pop, upd ?? got);

         return lst_ins.ToArray();
      }

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CStatementSwitch @switch) =>
         throw new System.NotImplementedException();//todo develop
   }
}
