using Gate.CLanguage.Compiler;
using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Source;
using Gate.CLanguage.Statement;
using Gate.CLanguage.Types;
using Gate.LangBase;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using System.Text.RegularExpressions;

namespace Gate.CLanguage
{
   /// <summary>
   /// 
   /// </summary>
   public partial class CScopeHelper : CScopeHelperBase
   {
      private readonly InnerGetTypeLinkSignatureVisitor myTypeSignatureVisitor = new InnerGetTypeLinkSignatureVisitor();

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="settings"></param>
      public CScopeHelper(CScopeSettings settings) : base(settings) { }

      /// <summary>
      /// 
      /// </summary>
      private static class InnerGetUserTypesScopeVisitor
      {
         public static CTypeUserDefined[] GetUserTypesScope(CItemWithScopeSpace itemWithScope) => myGetUserTypesScope((dynamic)itemWithScope);

         private static CTypeUserDefined[] myGetUserTypesScope(CItemWithScopeSpace itemWithScope) => throw new Crash();

         private static CTypeUserDefined[] myGetUserTypesScope(CTypeStructBody structBody) =>
            myGetTypeHierarchy([structBody.ParentStruct?.Anchestor ?? throw new Crash()]);

         private static CTypeUserDefined[] myGetUserTypesScope(CSource source) => myGetUserTypesScopeBlock(source);

         private static CTypeUserDefined[] myGetUserTypesScope(CBlockCompound blockCompound) => myGetUserTypesScopeBlock(blockCompound);

         private static CTypeUserDefined[] myGetUserTypesScope(CCycleWhile.BodyType whileBody)
         {
            throw new NotImplementedException();//todo
         }

         private static CTypeUserDefined[] myGetUserTypesScope(CCycleDoWhile.BodyType doWhileBody)
         {
            throw new NotImplementedException();//todo
         }

         private static CTypeUserDefined[] myGetUserTypesScope(CCycleIfElse.BodyType ifElseBody)
         {
            throw new NotImplementedException();//todo
         }

         private static CTypeUserDefined[] myGetUserTypesScope(CCycleSwitch.BodyType switchBody)
         {
            throw new NotImplementedException();//todo
         }

         private static CTypeUserDefined[] myGetUserTypesScope(CCycleFor.BodyType forBody) =>
            forBody.InitSpecifiers != null ? myGetUserTypesScope(forBody.InitSpecifiers) : [];

         private static CTypeUserDefined[] myGetUserTypesScope(CBlockFunction blockFunction) =>
            myGetTypeHierarchy(
               myGetUserTypesScopeBlock(blockFunction).Concat(myGetUserTypesScope(blockFunction.ParentFunction?.FunctionContainer??throw new Crash())));

         private static CTypeUserDefined[] myGetUserTypesScopeBlock(IBlock block) =>
            myGetTypeHierarchy(((CItemWithScopeSpace)block).SubItems.OfType<CDeclSpecifiers>().Select(ds => ds.TypeBase).OfType<CTypeUserDefined>());

         private static CTypeUserDefined[] myGetUserTypesScope(CTypeFunctionContainer functionParameters) =>
            myGetTypeHierarchy(functionParameters.Select(fp => fp.DeclSpecifiers?.TypeBase).OfType<CTypeUserDefined>());

         private static CTypeUserDefined[] myGetUserTypesScope(CDeclSpecifiers declSpecifiers)
         {
            var blo = declSpecifiers.ParentItemChain.OfType<IBlock>().FirstOrDefault();

            if (blo != null) { return myGetUserTypesScope((dynamic)blo); }
            else if (declSpecifiers.TypeBase is CTypeUserDefined typ_usr) { return new[] { typ_usr }; }
            else { return new CTypeUserDefined[0]; }
         }

         private static CTypeUserDefined[] myGetTypeHierarchy(IEnumerable<CTypeUserDefined> typesUserDefined) =>
            typesUserDefined.Concat(typesUserDefined.OfType<CTypeStruct>().SelectMany(ts => ts.DescedentTypes)).Distinct().ToArray();
      }

      /// <summary>
      /// 
      /// </summary>
      private static class InnerScopeDeclVisitor
      {
         public static CDecl[] GetDeclFunctionVisible(CItemWithScopeSpace itemWithScope, CScopeHelper scopeHelper) =>
            myGetDeclsFunctionVisible((dynamic)itemWithScope, scopeHelper);

         private static CDecl[] myGetDeclsFunctionVisible(CTypeStructBody structBody, CScopeHelper scopeHelper)
         {
            var sco_stk = myGetBlockStack(structBody);

            if (sco_stk.LastOrDefault() is CBlock blo) { return myGetDeclsFunctionVisible((dynamic)blo, scopeHelper); }
            else if (sco_stk.LastOrDefault() is CSource sou) { return myGetDeclsFunctionVisible(sou, scopeHelper); }
            else { throw new Crash(); }
         }

         private static CDecl[] myGetDeclsFunctionVisible(CTypeFunctionContainer funcParams, CScopeHelper scopeHelper)
         {
            if (funcParams.BoundFunction != null)
            {
               return myGetDeclsFunctionVisible(funcParams.BoundFunction, scopeHelper);
            }
            else
            {
               return scopeHelper.myGetScopeDecls(funcParams);
            }
         }
         private static CDecl[] myGetDeclsFunctionVisible(CDeclSpecifiers declSpecifier, CScopeHelper scopeHelper)
         {
            var sco_stk = myGetBlockStack(declSpecifier.ContainingScope?.ItemWithScopeSpace ?? throw new Crash());
            var blo = sco_stk.OfType<CBlock>().FirstOrDefault();

            if (blo != null)//function scope 
            {
               return myGetDeclsFunctionVisible((dynamic)blo, scopeHelper);
            }
            else//global scope
            {
               return declSpecifier.HeaderSource != null ? myGetDeclsFunctionVisible(declSpecifier.HeaderSource, scopeHelper) : new CDeclStorage[0];
            }
         }

         private static CDecl[] myGetDeclsFunctionVisible(CSource source, CScopeHelper scopeHelper) => scopeHelper.myGetScopeDecls(source);

         private static CDecl[] myGetDeclsFunctionVisible(CBlockFunction blockFunction, CScopeHelper scopeHelper) =>
            myGetDeclsFunctionVisible(blockFunction.ParentFunction ?? throw new Crash(), scopeHelper);

         private static CDecl[] myGetDeclsFunctionVisible(CDeclFunction boundFunction, CScopeHelper scopeHelper)
         {
            if (boundFunction != null)
            {
               var fnc_bdy = boundFunction.Body;
               var fnc_cnt = boundFunction.FunctionContainer;
               var sco_stk = myGetBlockStack(fnc_bdy as CItemWithScopeSpace ?? fnc_cnt ?? throw new Crash());
               var lst = new List<CDecl>();

               if (sco_stk.FirstOrDefault() is CSource src)
               {
                  myAddToList(lst, scopeHelper.myGetScopeDecls(src).Where(d => !d.IsAnonimous));
               }

               //adding persistent of overlying persistent(static) declarations
               //eg static of parent function like in :
               // int g1;
               // void func(void)
               //{
               //  static int f1;
               //  void func_inner(void)
               //  {
               //    //I see g1,f1     
               //  }
               //}
               foreach (var sco in sco_stk.OfType<CBlock>())
               {
                  myAddToList(lst, scopeHelper.myGetScopeDecls(sco).OfType<CDeclStorage>().Where(d => !d.IsAnonimous && d.IsPersistent));
               }

               //adding parameters and local 
               if (fnc_bdy != null)
               {
                  myAddToList(lst, scopeHelper.myGetScopeDecls(fnc_bdy));
               }
               else
               {
                  myAddToList(lst, scopeHelper.myGetScopeDecls(fnc_cnt ?? throw new Crash()));
               }

               return lst.ToArray();
            }
            else
            {
               return [];
            }
         }

         private static CDecl[] myGetDeclsFunctionVisible(CBlockCompound blockCompound, CScopeHelper scopeHelper) =>
            myGetDeclsFunctionVisibleInsideFunction(blockCompound, scopeHelper);

         /// <summary>
         /// 
         /// </summary>
         /// <param name="simpleBlock"></param>
         /// <param name="scopeHelper"></param>
         /// <returns></returns>
         /// <exception cref="Crash"></exception>
         private static CDecl[] myGetDeclsFunctionVisibleInsideFunction(CItemWithScopeSpace simpleBlock, CScopeHelper scopeHelper)
         {
            //cmp->cmp->blo_fnc->blo_fnc->source
            var lst = new List<CDecl>();
            var sco_stk = myGetBlockStack(simpleBlock);
            var sup_fnc = sco_stk.OfType<CBlockFunction>().LastOrDefault() ?? throw new Crash();

            var sts = myGetDeclsFunctionVisible(sup_fnc.ParentFunction ?? throw new Crash(), scopeHelper);

            myAddToList(lst, sts);

            //compound sequence from scope space chain
            var cmp_seq = sco_stk.Skip(sco_stk.ToList().IndexOf(sup_fnc) + 1).ToArray();

            foreach (var cmp in cmp_seq) { myAddToList(lst, scopeHelper.myGetScopeDecls(cmp)); }

            return lst.ToArray();
         }

         private static CDecl[] myGetDeclsFunctionVisible(CCycleFor.BodyType forBody, CScopeHelper scopeHelper) =>
            myGetDeclsFunctionVisibleInsideFunction(forBody, scopeHelper).
               Concat((forBody.InitSpecifiers?.Decls ?? new CDecl[0])).ToArray();

         private static CDecl[] myGetDeclsFunctionVisible(CCycleBody cycleBody, CScopeHelper scopeHelper) =>
            myGetDeclsFunctionVisibleInsideFunction(cycleBody, scopeHelper);

         private static void myAddToList(List<CDecl> listDecl, IEnumerable<CDecl> decls)
         {
            var dcl_ids = decls.Select(d => d.Identifier).ToArray();

            listDecl.RemoveAll(d => dcl_ids.Contains(d.Identifier));
            listDecl.AddRange(decls);
         }


         /// <summary>
         /// <br> Array of <see cref="CItemWithScopeSpace"/> from <see cref="CSource"/> to current eg </br>
         /// <br> - <paramref name="itemWithScope"/> is <see cref="CBlockCompound"/> result is <see cref="CBlockCompound"/> </br>
         /// <br> - <paramref name="itemWithScope"/> is <see cref="CSource"/> result is <see cref="CSource"/> </br>
         /// <br> - <paramref name="itemWithScope"/> is <see cref="CCycleBody"/> result is containg <see cref="CBlockCompound"/> or <see cref="CBlockFunction"/> </br>
         /// </summary>
         /// <param name="itemWithScope"><see cref="CSource"/> or <see cref="CBlock"/> at top of scope chain stack </param>
         /// <returns></returns>
         private static CItemWithScopeSpace[] myGetBlockStack(CItemWithScopeSpace itemWithScope) =>
            itemWithScope.ParentItemChain.OfType<CSource>().Cast<CItemWithScopeSpace>().
               Concat(itemWithScope.ParentItemChain.OfType<CBlock>().Reverse()).ToArray();
      }

      /// <summary>
      /// 
      /// </summary>
      public override CLanguage Language => CLanguage.c;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="itemWithScope"></param>
      /// <returns></returns>
      public override CDeclStorage[] GetDeclStoragesFunctionVisible(CItemWithScopeSpace itemWithScope) =>
         InnerScopeDeclVisitor.GetDeclFunctionVisible(itemWithScope, this).OfType<CDeclStorage>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="itemWithScope"></param>
      /// <returns></returns>
      public override CTypeUserDefined[] GetUserTypesScope(CItemWithScopeSpace itemWithScope) =>
         InnerGetUserTypesScopeVisitor.GetUserTypesScope(itemWithScope);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="itemWithScope"></param>
      /// <returns></returns>
      public override CTypeUserDefined[] GetTypesUsersFunctionVisible(CItemWithScopeSpace itemWithScope) =>
         myGetFunctionVisibleItems(itemWithScope, iws => GetUserTypesScope(iws));

      /// <summary>
      /// 
      /// </summary>
      /// <param name="itemWithScope"></param>
      /// <returns></returns>
      public override CDeclTypedef[] GetTypedefsFunctionVisible(CItemWithScopeSpace itemWithScope) =>
         InnerScopeDeclVisitor.GetDeclFunctionVisible(itemWithScope, this).OfType<CDeclTypedef>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="ITEM"></typeparam>
      /// <param name="itemWithScope"></param>
      /// <param name="itemGetter"></param>
      /// <returns></returns>
      private static ITEM[] myGetFunctionVisibleItems<ITEM>(
         CItemWithScopeSpace itemWithScope, Func<CItemWithScopeSpace, ITEM[]> itemGetter) where ITEM : CItem, IWithIdentifier
      {
         var scp_chn = itemWithScope.Scope.ItemWithScopeSpace.ParentItemChain.OfType<CItemWithScopeSpace>().Reverse().ToArray();
         var dct_vrs = new Dictionary<string, ITEM>();

         //starts from global to local(local hides possibly globals with same var-name)
         foreach (var iws in scp_chn.Reverse())
         {
            var its = itemGetter.Invoke(iws);

            foreach (var itm in its) { dct_vrs[(itm.Identifier).ExtTrim()] = itm; }
         }

         return dct_vrs.Values.ToArray();
      }

      public override string? GetDeclSignature(CDecl decl) => decl.Identifier;

      public override string? GetTypeSignature(CType type) => myTypeSignatureVisitor.GetTypeLinkSignature(type);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="messages"></param>
      /// <param name="decl"></param>
      /// <param name="itemWithScope"></param>
      /// <returns></returns>
      public override bool CheckDecl(MsgCollection messages, CDecl decl, CItemWithScopeSpace itemWithScope)
      {
         var sco_dcs = myGetScopeDecls(itemWithScope).Except([decl]).Where(t => !t.IsAnonimous).ToArray();
         var old_dcl = sco_dcs.LastOrDefault(sd => sd.Identifier == decl.Identifier);

         if (old_dcl == null) { return true; }
         else if (old_dcl.IsDefinition && decl.IsDefinition)
         {
            messages.Add(CCompilerMsgs.RedefindedIdentifier(decl, old_dcl));

            return false;
         }
         else { return myCompareDecls(old_dcl, decl, messages); }
      }

      public override bool CheckUserDefType(MsgCollection messages, CTypeUserDefined typeUserDefined, CItemWithScopeSpace? itemWithScope)
      {
         var sco_tps = 
            GetUserTypesScope(itemWithScope ?? throw new Crash()).
            Except([typeUserDefined]).
            Where(t => !t.IsAnonimous).ToArray();
        
         var dup_typ = sco_tps.FirstOrDefault(t => t.Identifier == typeUserDefined.Identifier);

         if (dup_typ != null)
         {
            messages.Add(CCompilerMsgs.DuplicatedTypeIdentifier(typeUserDefined, dup_typ));

            return false;
         }
         else { return true; }
      }

      protected virtual CDecl[] myGetScopeDecls(CItemWithScopeSpace itemWithScopeSpace) => itemWithScopeSpace.ScopeDecls;


      /// <summary>
      /// Compares two declaration and raises error messages in case of error.
      /// </summary>
      /// <param name="oldDecl"></param>
      /// <param name="decl"></param>
      /// <param name="messages"></param>
      /// <returns></returns>
      private bool myCompareDecls(CDecl oldDecl, CDecl decl, MsgCollection messages) =>
         oldDecl == null ||
            myCompareStorageClasses(oldDecl, decl, messages) &&
            myCompareTypeQualifiers(oldDecl, decl, messages) &&
            myCompareTypeBasesOrFunctionProtototype(oldDecl, decl, messages) &&
            myAreSubscriptSetCompatible(oldDecl, decl, messages);

      /// <summary>
      /// <br>Check whether <see cref="CTypeSubscriptSet"/> of two <see cref="CDecl"/> are compatible: </br>
      /// <br>otherwise raise error message</br>
      /// </summary>
      /// <param name="oldDecl"></param>
      /// <param name="decl"></param>
      /// <param name="messages"></param>
      /// <returns></returns>
      private bool myAreSubscriptSetCompatible(CDecl oldDecl, CDecl decl, MsgCollection messages)
      {
         var old_ts = oldDecl.TypeAlias?.PrimitiveAlias.TypeSubscriptSet ?? throw new Crash();
         var ts = decl.TypeAlias?.PrimitiveAlias.TypeSubscriptSet ?? throw new Crash();

         if (ts.IsCompatibleWith(old_ts)) { return true; }
         else
         {
            messages.Add(CCompilerMsgs.ConflictingTypes(decl, oldDecl));

            return false;
         }
      }

      private bool myCompareTypeBasesOrFunctionProtototype(CDecl oldDecl, CDecl decl, MsgCollection messages)
      {
         if (Settings.IsInternalLinkWeak) { return true; }//control is skip
         else if (decl is CDeclFunction fnc)
         {
            var fnc_old = oldDecl as CDeclFunction ?? throw new Crash("Expected a function");

            if (
               (fnc_old.TypeAlias ?? throw new Crash()).PrimitiveAlias.Signature !=
               (fnc.TypeAlias ?? throw new Crash()).PrimitiveAlias.Signature)
            {
               messages.Add(CCompilerMsgs.ConflictingTypes(decl, oldDecl));

               return false;
            }
            else
            {
               return true;
            }
         }
         else
         {
            var old_tb = oldDecl.TypeAlias.PrimitiveAlias.TypeBase;
            var tb = decl.TypeAlias.PrimitiveAlias.TypeBase;

            if (old_tb?.Signature != tb?.Signature && !Settings.AreTypeSpecifierRedeclarable)
            {
               messages.Add(CCompilerMsgs.ConflictingTypes(decl, oldDecl));

               return false;
            }
            else
            {
               return true;
            }
         }
      }

      private bool myCompareTypeQualifiers(CDecl oldDecl, CDecl decl, MsgCollection messages)
      {
         var old_tq = oldDecl.DeclSpecifiers?.TypeQualifiers;
         var tq = decl.DeclSpecifiers?.TypeQualifiers;

         if (old_tq == tq) { return true; }
         else
         {
            messages.Add(CCompilerMsgs.IncompatibleTypeQualifiers(
               decl.DeclSpecifiers ?? throw new Crash(), oldDecl.DeclSpecifiers ?? throw new Crash()));

            return false;
         }
      }

      /// <summary>
      /// Compares storage classes of two declarations to determine compatibility according to C language rules.
      /// </summary>
      /// <remarks>
      /// Storage classes are compatible in these cases:
      /// - When they are identical (e.g. both static, both extern, etc.)
      /// - When one is 'extern' and the other has no storage class specified
      /// If incompatible, adds error message M077 to messages collection.
      /// </remarks>
      /// <param name="oldDecl">The original/existing declaration to compare</param>
      /// <param name="decl">The new declaration being checked</param>
      /// <param name="messages">Collection to receive any error messages</param>
      /// <returns>True if storage classes are compatible, false otherwise</returns>
      private bool myCompareStorageClasses(CDecl oldDecl, CDecl decl, MsgCollection messages)
      {
         var old_sc = oldDecl.StorageClass;
         var sc = decl.StorageClass;
         var pai = new[] { old_sc, sc };

         if (old_sc == sc || pai.Any(p => p == CTypeStorageClass.@extern) && pai.Any(p => p == CTypeStorageClass.none))
         {
            return true;
         }
         else
         {
            messages.Add(CCompilerMsgs.IncompatibleStorageClass(
               decl.DeclSpecifiers ?? throw new Crash(), oldDecl.DeclSpecifiers ?? throw new Crash()));

            return false;
         }
      }

      private static string myOptimizeSpaces(string @string)
      {
         //optimise spaces
         var rgx = new Regex(@"\s+");

         @string = @string.Trim();
         @string = rgx.Replace(@string, " ");

         var idx = 0;

         while (idx < @string.Length)
         {
            idx = @string.IndexOf(' ', idx);

            if (idx == -1) { break; }
            else
            {
               var lx_ch = char.IsLetterOrDigit(@string[idx - 1]) || @string[idx - 1] == '_';
               var rx_ch = char.IsLetterOrDigit(@string[idx + 1]) || @string[idx + 1] == '_';

               if (!lx_ch || !rx_ch) { @string = @string.Remove(idx, 1); }
               else
               {
                  idx++;
               }
            }
         }

         return @string;
      }
   }
}
