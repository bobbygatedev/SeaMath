using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Statement;
using Gate.CLanguage.Types;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;
using GateCLang.Statement;
using static Gate.CLanguage.Statement.CStatement;
using static Gate.LangBase.Runtime.DbgEngVirtCpu.RtmDbgEngVirtCpuInstructionGotoNext;
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

      /// <summary>
      /// 
      /// </summary>
      /// <param name="declVar"></param>
      /// <returns></returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
      /// <exception cref="Crash"></exception>
      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CDeclVar declVar)
      {
         //persistant (global)
         var is_prs =
            declVar.IsGlobal && declVar.IsDefinition ||
            declVar.StorageClass == CTypeStorageClass.@static;

         if (is_prs)
         {
            return [new CRtmDbgEngVirtCpuInstructionDecl.Persistant(declVar)];
         }
         else
         {
            return [new CRtmDbgEngVirtCpuInstructionDecl.Automatic(declVar)];
         }
      }

      /// <summary>
      /// Gets all instruction excluding push function
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

         var frm_psh = new RtmDbgEngVirtCpuInstructionPushStackFrame(myMakeStatementDecls(lnk_fnc.Body.NnOrCrash()), declFunction);

         //items inside of function body's block 
         var its = lnk_fnc?.Body?.SubItems.OfType<CItem>().ToArray() ?? [];

         //items after tranlation into instructions
         var iss = its.SelectMany(i => GetInstructions(i)).ToArray();

         var pop = new RtmDbgEngVirtCpuInstructionFramePop();

         if (iss.Length == 0 || iss.All(i => i.Token == null))
         {
            //last position inside function body (or null)
            var to_fnc_pos = lnk_fnc?.Body?.TxtToken?.To?.Primitive;

            //token to last line inside function body (or null)
            var to_fnc_tok = to_fnc_pos != null ? to_fnc_pos.Store?[to_fnc_pos.Line] : null;

            //adds a nop at last function line because a empty function causes an infinite loop
            return
               new RtmDbgEngVirtCpuInstruction[] { frm_psh }.
               Append(new Nop(to_fnc_tok)).
               Append(pop).
               ToArray();
         }
         else
         {
            return new RtmDbgEngVirtCpuInstruction[] { frm_psh }.
               Concat(iss).
               Append(pop).
               ToArray();
         }
      }

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CExprStatement cExpr) =>
         [new CRtmDbgEngVirtCpuInstructionExpr(cExpr)];

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
         lst_ins.Add(
            new RtmDbgEngVirtCpuInstructionPushStackFrame(
               myMakeStatementDecls(ifElse.Body as CStatementCompound), ifElse));

         for (int i = 0; i < if_els.Length; i++)
         {
            lst_ins.Add(null);//goto later

            // body instructions 
            lst_ins.AddRange(myGetCycleBodyInstructions(if_els[i]));

            //always goto end frame
            lst_ins.Add(new RtmDbgEngVirtCpuInstructionGoto(if_els[i].TxtToken, end_frm_pop));
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
               var exp = if_els[i].StayConditionExpr.NnOrCrash().Expr.NnOrCrash();

               if (i == got_ids.Length - 1)
               {
                  //if true next instruction otherwise else_body or end
                  lst_ins[id] = new RtmDbgEngVirtCpuInstructionGoto(
                     if_els[i].StayConditionExpr?.TxtToken, null, els_0 ?? end_frm_pop, exp);
               }
               else
               {
                  //if true next instruction otherwise else_body or end
                  lst_ins[id] = new RtmDbgEngVirtCpuInstructionGoto(
                     if_els[i].StayConditionExpr?.TxtToken, null, lst_ins[got_ids[i + 1]], exp);
               }
            }
         }

         //6: end(pop frame)
         lst_ins.Add(end_frm_pop);

         return lst_ins.Select(i => i.NnOrCrash()).ToArray();
      }

      protected virtual CDecl[] myMakeStatementDecls(CStatementCompound? compound)
      {
         if (compound != null)
         {
            var dcs = compound.Content.OfType<CDeclSpecifiers>().SelectMany(d => d.Decls).ToArray();

            if (
               compound.ParentCycleFor != null &&
               compound.ParentCycleFor.Initialisation is CDeclSpecifiers ds)
            {
               dcs = ds.Decls.Concat(dcs).ToArray();
            }

            return dcs.Where(d => (d.StorageClass & CTypeStorageClass.@static) == 0x0).ToArray();
         }

         return [];
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
         lst_ins.Add(new RtmDbgEngVirtCpuInstructionPushStackFrame(
            myMakeStatementDecls(whileCycle.Body as CStatementCompound), whileCycle));

         // 1: if-condition-goto 
         lst_ins.Add(
            new RtmDbgEngVirtCpuInstructionGoto(
               whileCycle.StayConditionExpr?.TxtToken,
               null,
               end_frm_pop,
               whileCycle.StayConditionExpr.NnOrCrash().Expr.NnOrCrash()));

         lst_ins.AddRange(myGetCycleBodyInstructions(whileCycle));

         //5: goto if-condition-goto '1: if-condition-goto'
         var got = new RtmDbgEngVirtCpuInstructionGoto(null, lst_ins[1]);

         lst_ins.Add(got);

         //6: end(pop frame)
         lst_ins.Add(end_frm_pop);

         myPatchBreakContinue(lst_ins, end_frm_pop, got);

         return lst_ins.ToArray();
      }

      private static void myPatchBreakContinue(
         List<RtmDbgEngVirtCpuInstruction> listInstructions,
         RtmDbgEngVirtCpuInstructionFramePop endFramePop,
         RtmDbgEngVirtCpuInstruction? continueTarget)
      {
         foreach (var brk in listInstructions.ToArray().OfType<InnerDummyBreak>())
         {
            //break skipping to frame pop
            listInstructions[listInstructions.IndexOf(brk)] =
               new RtmDbgEngVirtCpuInstructionGoto(brk.Break.TxtToken, endFramePop);
         }

         if (continueTarget != null)
         {
            foreach (var brk in listInstructions.ToArray().OfType<InnerDummyContinue>())
            {
               //break skipping to cycle goto
               listInstructions[listInstructions.IndexOf(brk)] =
                  new RtmDbgEngVirtCpuInstructionGoto(brk.Continue.TxtToken, continueTarget);
            }
         }
      }

      IRtmObjStrategy IFunctionInstructionTranslator.RtmStrategy => RtmStrategy;

      private RtmDbgEngVirtCpuInstruction[] myGetCycleBodyInstructions(
         CStatementConditional statementCondtional, bool isElse = false)
      {
         var lst_ins = new List<RtmDbgEngVirtCpuInstruction>();
         var sts = null as CItem[];

         var bdy = isElse ? statementCondtional.ConvertOrCrash<CStatementIfElse>().ElseBody : statementCondtional.Body;

         //eg for(;;){ a*=2; }
         if (bdy is CStatementCompound cmp) { sts = cmp.Content; }
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
            lst_ins.Add(new RtmDbgEngVirtCpuInstructionByAction.Nop(null));//adding "nop"
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
            new RtmDbgEngVirtCpuInstructionPushStackFrame(
               myMakeStatementDecls(doWhileCycle.Body as CStatementCompound), doWhileCycle)
         };

         lst_ins.AddRange(myGetCycleBodyInstructions(doWhileCycle));

         // 2: if-condition-goto 
         var got = new RtmDbgEngVirtCpuInstructionGoto(
               doWhileCycle.StayConditionExpr?.TxtToken,
               lst_ins[1],
               null,
               doWhileCycle.StayConditionExpr.NnOrCrash().Expr.NnOrCrash());

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
               new RtmDbgEngVirtCpuInstructionPushStackFrame(
                  myMakeStatementDecls(forCycle.Body as CStatementCompound), forCycle)
            };

         //frame pop(end)
         var end_frm_pop = new RtmDbgEngVirtCpuInstructionFramePop();

         /// 1: init
         if (forCycle.Initialisation != null) { lst_ins.AddRange(GetInstructions(forCycle.Initialisation)); }

         var got = null as RtmDbgEngVirtCpuInstructionGoto;

         //if condition is null infinite cycle
         if (forCycle.StayConditionExpr != null)
         {
            var cnd = (forCycle.StayConditionExpr?.Expr).NnOrCrash();

            got = new RtmDbgEngVirtCpuInstructionGoto(
               forCycle.StayConditionExpr?.TxtToken, null, end_frm_pop, cnd);
         }
         else
         {
            got = new RtmDbgEngVirtCpuInstructionGoto(null, end_frm_pop);
         }

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
         lst_ins.Add(new RtmDbgEngVirtCpuInstructionGoto(null, got));

         //6: end(pop frame)
         lst_ins.Add(end_frm_pop);

         //if update is defined continue shall jump to it ( eg for ( i = 0; ; i++ ) continue; -> jump to 'i++')
         myPatchBreakContinue(lst_ins, end_frm_pop, upd ?? got);

         return lst_ins.ToArray();
      }

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CStatementSwitch.CaseLabel caseLabel) =>
         [new Nop(null, caseLabel)];

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CStatementSwitch.DefaultLabel defaultLabel) =>
         [new Nop(null, defaultLabel)];


      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CStatementSwitch switchStatement)
      {
         // every label and default correspond to a nop tagged with label/default
         // 0: frame
         // 1: evaluate condition and goto
         // 2: end (pop frame)
         var lst_ins = new List<RtmDbgEngVirtCpuInstruction>
         {
            /// 0: frame
            new RtmDbgEngVirtCpuInstructionPushStackFrame(
               myMakeStatementDecls(switchStatement.Body as CStatementCompound), switchStatement),
            new CRtmDbgEngVirtCpuInstructionSwitch(switchStatement)
         };

         lst_ins.AddRange(myGetCycleBodyInstructions(switchStatement));

         var end_frm_pop = new RtmDbgEngVirtCpuInstructionFramePop();

         //6: end(pop frame)
         lst_ins.Add(end_frm_pop);
         myPatchBreakContinue(lst_ins, end_frm_pop, null);

         return lst_ins.ToArray();
      }

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CStatementGoto @goto) =>
         throw new System.NotImplementedException();//tododo develop

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CStatementGotoLabel gotoLabel) => [];
   }
}
