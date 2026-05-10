using Gate.CLanguage.Decl;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Runtime;
using Gate.CLanguage.Runtime.Object;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.CLanguage.Initialisation
{
   /// <summary>
   /// <br> Encapsulate hierachically an init eg :</br>
   /// <br> '{ 1 , { 2 , 3 } }'</br>
   /// <br>   '1'  '{ 2 , 3 }' </br>
   /// <br>          '2','3' </br>
   /// </summary>
   public abstract class CInitialisation : CItem
   {
      private string? myStructFieldName;
      private InnerInitVisitor myInitVarVisitor;

      /// <summary>
      /// 
      /// </summary>
      protected CInitialisation() => myInitVarVisitor = new InnerInitVisitor(this);

      /// <summary>
      /// 
      /// </summary>
      private class InnerInitVisitor
      {
         public InnerInitVisitor(CInitialisation initialisation) => Initialisation = initialisation;

         public CInitialisation Initialisation { get; }

         public void DoInit(RtmObj runTimeObj, RtmDbgEngStackVirtCpu stack, CRtmObjStrategy rtmStrategy) =>
            myDoInit((dynamic)runTimeObj, stack, (dynamic)Initialisation, rtmStrategy);

         protected static void myDoInit(
            CRtmObjPointerFunction function, RtmDbgEngStackVirtCpu? stack, CInitialisation init, CRtmObjStrategy? rtmStrategy)
         {
            var obj = init.ScalarExpression?.Expr?.Eval(stack, rtmStrategy);
            var fun = obj?.GetRtmObjFunction();

            if (fun != null)
            {
               function.ObjFunction = fun;
            }
            else if (obj is CRtmObjScalar sca)
            {
               myDoInit(function as CRtmObjScalar, stack, init, rtmStrategy);
            }
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="rtmScalar"></param>
         /// <param name="stack"></param>
         /// <param name="init"></param>
         /// <param name="rtmStrategy"></param>
         protected static void myDoInit(
            CRtmObjScalar rtmScalar, RtmDbgEngStackVirtCpu? stack, CInitialisation init, CRtmObjStrategy? rtmStrategy) =>
            rtmScalar.CSharpObj = rtmStrategy?.NumericConverter.DoConvertCsharpValue(
                  rtmScalar.CSharpObj?.GetType() ?? throw new Crash(),
                  init.ScalarExpression?.Expr?.Eval(stack, rtmStrategy)?.CSharpObj ?? throw new Crash());

         protected unsafe static void myDoInit(
            CRtmObjArray arrRtmObj, RtmDbgEngStackVirtCpu stack, CInitialisationArray init, CRtmObjStrategy rtmStrategy)
         {
            foreach (var ini in init.EffectiveInits)
            {
               var obj = myGetArrayItem(arrRtmObj, ini?.Indices ?? throw new Crash()) ?? throw new Crash();

               myDoInit((dynamic)obj, stack, (dynamic)ini, rtmStrategy);
            }
         }

         /// <summary>
         /// Init a pointer-c-runtime-object with init string, ie copy init string pointer to <see cref="CRtmObjPointer.PointerValue"/>.
         /// </summary>
         /// <param name="rtmPointerObj"></param>
         /// <param name="stack"></param>
         /// <param name="initString"></param>
         /// <param name="rtmStrategy"></param>
         protected unsafe static void myDoInit(
            CRtmObjPointer rtmPointerObj, RtmDbgEngStackVirtCpu stack, CInitialisationString initString, CRtmObjStrategy rtmStrategy) =>
            rtmPointerObj.PointerValue = initString.TokenString.RtmObjStringLiteral.Address ?? nint.Zero;

         /// <summary>
         /// Init an array-c-runtime-object with init string, 
         /// ie copy init string bytes to data location ie <see cref="CRtmObj.Address"/>.
         /// </summary>
         /// <param name="runTimeObj"></param>
         /// <param name="stack"></param>
         /// <param name="initString"></param>
         /// <param name="rtmStrategy"></param>
         /// <exception cref="Crash"></exception>
         protected unsafe static void myDoInit(
            CRtmObjArray runTimeObj, RtmDbgEngStackVirtCpu stack, CInitialisationString initString, CRtmObjStrategy rtmStrategy)
         {
            var sz_of = runTimeObj.ItemSizeOf;

            if (runTimeObj.Sizes.Length != 1) { throw new Crash(); }
            else if (runTimeObj.ItemType != initString.TokenString.RtmObjStringLiteral.ItemType)
            {
               throw new Gate.LangBase.Runtime.RtmException(
                  $"Init string has char type {initString.TokenString.RtmObjStringLiteral.ItemType} " +
                  $"different from declaration one {runTimeObj.ItemType}!");
            }

            var src = (byte*)(initString.TokenString.RtmObjStringLiteral.Address ?? nint.Zero);
            var dst = (byte*)(runTimeObj.Address ?? nint.Zero);
            var str_itm_cnt = initString.TokenString.RtmObjStringLiteral.AllItemsCount;
            var arr_itm_cnt = runTimeObj.Sizes[0] * sz_of;
            var eff_len = Math.Min(str_itm_cnt, arr_itm_cnt);

            for (int i = 0; i < eff_len * sz_of; i++) { dst[i] = src[i]; }
         }

         protected unsafe static void myDoInit(
            CRtmObjRecord runTimeObj, RtmDbgEngStackVirtCpu stack, CInitialisationArray init, CRtmObjStrategy rtmStrategy)
         {
            foreach (var ini in init.EffectiveInits)
            {
               if (ini.Indices?.Array.Length == 1 && ini.Indices?.Array[0] is string fld_nam)
               {
                  var fld = runTimeObj[fld_nam];

                  if (fld != null)
                  {
                     myDoInit((dynamic)fld, stack, (dynamic)ini, rtmStrategy);
                  }
                  else
                  {
                     throw new Gate.LangBase.Runtime.RtmException($"Rtm record doesn't contain field {fld_nam}");
                  }
               }
            }
         }

         protected virtual void myDoInit(
            CRtmObj runTimeObj, RtmDbgEngStackVirtCpu stack, CInitialisation init, CRtmObjStrategy rtmStrategy) =>
               throw new Crash($"{runTimeObj.GetType().Name} not valid for visitor!");

         private static CRtmObj? myGetArrayItem(CRtmObjArray arrRtmObj, CDeclSubscriptIndices indices)
         {
            var rtm_obj = arrRtmObj as CRtmObj;
            var ids = indices.Array.ToArray();

            while (ids.Length > 0)
            {
               var arr_ids = ids.TakeWhile(i => i is int).Cast<int>().ToArray();

               if (arr_ids.Length > 0)
               {
                  ids = ids.Skip(arr_ids.Length).ToArray();
                  rtm_obj = rtm_obj is CRtmObjArray arr ?
                     arr[arr_ids] :
                     throw new Gate.LangBase.Runtime.RtmException($"Expected an array!");
               }
               else if (ids.FirstOrDefault() is string str_idx)
               {
                  ids = ids.Skip(1).ToArray();
                  rtm_obj = rtm_obj is CRtmObjRecord rec ?
                     rec[str_idx] :
                     throw new Gate.LangBase.Runtime.RtmException($"Expected a record!");
               }
               else { throw new Crash(); }
            }

            return rtm_obj;
         }

      }

      /// <summary>
      /// 
      /// </summary>
      public abstract CExprStatement? ScalarExpression { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract int? IncompleteArraySize { get; }

      /// <summary>
      /// 
      /// </summary>
      public override bool HasAssociatedPragma => false;

      /// <summary>
      /// <br> Recursively gathered initalizers (ie whose <see cref="Indices"/> has value). </br>
      /// <br> With respect to <see cref="AllInits"/> excludes indices repetition.</br>
      /// </summary>
      public CInitialisationScalar[] EffectiveInits => AllInits.GroupBy(i => i.Indices).Select(g => g.Last()).ToArray();

      /// <summary>
      /// Recursively gathered initalizers (ie whose <see cref="Indices"/> has value).
      /// </summary>
      public CInitialisationScalar[] AllInits => AllDescendant.OfType<CInitialisationScalar>().Where(si => si.Indices.HasValue).ToArray();

      /// <summary>
      ///  
      /// </summary>
      public CInitialisation? ParentInitialisation => ParentItem as CInitialisation;

      /// <summary>
      ///  
      /// </summary>
      public CInitialisation? AnchestorInitialisation => ParentItemChain.OfType<CInitialisation>().LastOrDefault();

      /// <summary>
      /// <see cref="CDeclVar"/> being parent <see cref="AnchestorInitialisation"/> and therefore of all declaration block. 
      /// </summary>
      public CDeclVar? OwningDecl => AnchestorInitialisation?.ParentItem as CDeclVar;

      /// <summary>
      /// Field name
      /// </summary>
      public string StructFieldName
      {
         get => myStructFieldName.ExtTrim();
         set => myStructFieldName = value.ExtTrim();
      }

      /// <summary>
      /// 
      /// </summary>
      public CDeclStorage? ParentDecl => ParentItem as CDeclStorage;

      /// <summary>
      /// Is root initialisation?.
      /// </summary>
      public bool IsRoot => AnchestorInitialisation == this;

      /// <summary>
      /// 
      /// </summary>
      public override string? Rebuilt => Descriptor;

      public void DoInit(RtmObj runTimeObj, RtmDbgEngStackVirtCpu stack, CRtmObjStrategy rtmStrategy) =>
         myInitVarVisitor.DoInit(runTimeObj, stack, rtmStrategy);

      public CInitialisation[] SubInits => SubItems.OfType<CInitialisation>().ToArray();

      public bool IsStruct => (StructFieldName ?? "").Trim() != "";
   }
}
