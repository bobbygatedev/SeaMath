using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Statement;
using Gate.CLanguage.Types;
using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
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

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CCompound compound) =>
         compound.Block.SubItems.SelectMany(itm => (RtmDbgEngVirtCpuInstruction[])myGetInstructions((dynamic)itm)).ToArray();

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

         var its = lnk_fnc?.Body?.SubItems.OfType<CItem>().ToArray() ?? [];
         var iss = its.SelectMany(i => GetInstructions(i)).ToArray();

         return iss;
      }

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CExprStatement cExpr) =>
         [new RtmDbgEngVirtCpuInstructionSimple(cExpr.TxtToken, (stk, str) => cExpr.Expr?.Eval(stk, RtmStrategy))];

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CCycleIfElse ifElse) => 
         throw new System.NotImplementedException();//todo develop

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CCycleWhile whileCycle)
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

         //index of first body instruction
         var cnd_ist = lst_ins.Count;

         if (whileCycle.Body.Condition != null)//if no condition always true (no check goto '3: body')
         {
            // 1: if-condition-goto 
            lst_ins.Add(
               new RtmDbgEngVirtCpuInstructionGoto(
                  whileCycle.Body.Condition.TxtToken, 
                  null, 
                  end_frm_pop, 
                  s => whileCycle.Body.Condition?.Expr?.Eval(s, RtmStrategy)));
         }

         // 3: body
         //eg for(;;)a*=2;
         if (whileCycle.Content is CExprStatement exp) { lst_ins.AddRange(myGetInstructions(exp)); }
         //eg for(;;){ a*=2; }
         else if (whileCycle.Content is CCompound cmp) { lst_ins.AddRange(myGetInstructions(cmp)); }
         //eg for(;;);
         else if (whileCycle.Content != null) { throw new Crash(); }

         //5: goto if-condition-goto '1: if-condition-goto'
         lst_ins.Add(new RtmDbgEngVirtCpuInstructionGoto(null, lst_ins[1], null, null));

         //6: end(pop frame)
         lst_ins.Add(end_frm_pop);

         return lst_ins.ToArray();
      }

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CCycleDoWhile doWhileCycle)
      {
         // 0: frame
         // 1: body 
         // 2: if-condition-goto (eg 'do { printf(""); } while(i<3);' -> if i < 3 goto '1: body' else goto '3: end')
         // 3: end (pop frame)

         var lst_ins = new List<RtmDbgEngVirtCpuInstruction>();

         /// 0: frame
         lst_ins.Add(new RtmDbgEngVirtCpuInstructionFramePush());

         // 1: body
         //eg for(;;)a*=2;
         if (doWhileCycle.Content is CExprStatement exp) { lst_ins.AddRange(myGetInstructions(exp)); }
         //eg for(;;){ a*=2; }
         else if (doWhileCycle.Content is CCompound cmp) { lst_ins.AddRange(myGetInstructions(cmp)); }
         //eg for(;;);
         else if (doWhileCycle.Content != null) { throw new Crash(); }

         if (doWhileCycle.Body.Condition != null)
         {
            // 2: if-condition-goto 
            lst_ins.Add(
               new RtmDbgEngVirtCpuInstructionGoto(
                  doWhileCycle.Body.Condition.TxtToken,
                  lst_ins[1],
                  null,
                  s => doWhileCycle.Body.Condition.Expr?.Eval(s, RtmStrategy)));
         }
         else { throw new Crash(); }

         //3: end(pop frame)
         lst_ins.Add(new RtmDbgEngVirtCpuInstructionFramePop());

         return lst_ins.ToArray();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="forCycle"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CCycleFor forCycle)
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
         if (forCycle.Body.Initialisation != null) { lst_ins.AddRange(GetInstructions(forCycle.Body.Initialisation)); }

         //index of first body instruction
         var cnd_ist = lst_ins.Count;

         if (forCycle.Body.Condition != null)//if no condition always true (no check goto '3: body')
         {
            // 2: if-condition-goto 
            lst_ins.Add(
               new RtmDbgEngVirtCpuInstructionGoto(
                  forCycle.Body.Condition.TxtToken,
                  null,
                  end_frm_pop,
                  s => forCycle.Body.Condition?.Expr?.Eval(s, RtmStrategy)));
         }

         // 3: body
         //eg for(;;)a*=2;
         if (forCycle.Content is CExprStatement exp) { lst_ins.AddRange(myGetInstructions(exp)); }
         //eg for(;;){ a*=2; }
         else if (forCycle.Content is CCompound cmp) { lst_ins.AddRange(myGetInstructions(cmp)); }
         //eg for(;;);
         else if (forCycle.Content != null) { throw new Crash(); }

         // 4: update
         if (forCycle.Body.Update != null) { lst_ins.AddRange(GetInstructions(forCycle.Body.Update)); }

         //in case neither a condition nor cycle content nor update are provided a "nop" is added
         if (cnd_ist >= lst_ins.Count) { lst_ins.Add(new RtmDbgEngVirtCpuInstructionSimple(null)); }

         //5: goto if-condition-goto 
         lst_ins.Add(new RtmDbgEngVirtCpuInstructionGoto(null, lst_ins[cnd_ist], null, null));

         //6: end(pop frame)
         lst_ins.Add(end_frm_pop);

         return lst_ins.ToArray();
      }

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(CCycleSwitch @switch) => 
         throw new System.NotImplementedException();//todo develop

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(Break @break) => 
         throw new System.NotImplementedException();//todo develop

      protected virtual RtmDbgEngVirtCpuInstruction[] myGetInstructions(Continue @continue) => 
         throw new System.NotImplementedException();//todo develop
   }
}
