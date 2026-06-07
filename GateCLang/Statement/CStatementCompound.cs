using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;
using Gate.Tools;
using Gate.Tools.Message;
using GateCLang.Statement;

namespace Gate.CLanguage.Statement
{
   /// <summary>
   /// Space of source code containing statements (ie Declarations,Expression, Other statements (if,while,goto,..)
   /// </summary>
   public class CStatementCompound : CStatement , ICItemWithScopeSpace 
   {
      public CStatementCompound() => Scope = new CScope(this);

      public CItem[] Content => SubItems.Where(i=>i is CStatement || i is CDeclSpecifiers).Cast<CItem>().ToArray();

      public override string Rebuilt => myGetBraceRebuilt();

      public override string Descriptor
      {
         get
         {
            var sub_its = SubItems.OfType<CItem>().ToArray();

            if (SubItems.Count() == 0) { return "{}"; }
            else if (SubItems.Count() == 1) { return "{" + sub_its.First().Descriptor + "}"; }
            else { return "{" + sub_its.First().Descriptor + "(..)}"; }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public CStatementConditional? ContainingCycle => ParentItemChain.OfType<CStatementConditional>().FirstOrDefault();

      /// <summary>
      /// 
      /// </summary>
      public CDeclFunction? ParentFunction => ParentItem as CDeclFunction;

      /// <summary>
      /// 
      /// </summary>
      public CStatementConditional? ParentCycle => ParentItem as CStatementConditional;

      /// <summary>
      /// 
      /// </summary>
      public CStatementLoopFor? ParentCycleFor => ParentItem as CStatementLoopFor;

      /// <summary>
      /// 
      /// </summary>
      public bool IsForFunction => ParentItem is CDeclFunction;

      public CScope Scope { get; }

      public CDeclFunction? ContainingFunction => ParentItemChain.OfType<CDeclFunction>().FirstOrDefault();

      public CStatementGotoLabel[] Labels => AllDescendant.OfType<CStatementGotoLabel>().ToArray();

      public CStatementGoto[] Gotos => AllDescendant.OfType<CStatementGoto>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public CDecl[] ScopeDecls
      {
         get
         {
            if (IsForFunction)
            {
               return ContainingFunction?.FunctionContainer?.ScopeDecls?.
                  Concat(myGetScopeDeclsDefault(Scope)).ToArray() ?? [];
            }
            else
            {
               var dcs = ParentCycleFor?.InitSpecifiers?.Decls ?? [];

               return dcs.Concat(myGetScopeDeclsDefault(Scope)).ToArray();
            }
         }
      }

      public bool AddDeclSpec(MsgCollection messages, CDeclSpecifiers declSpecifier, CScopeHelperBase? scopeHelper = null)
      {
         scopeHelper = scopeHelper ?? CScopeHelperBase.GetDefault(Language) ?? throw new Crash();

         if (declSpecifier.Decls.Where(d => !d.IsAnonimous).All(dcl => scopeHelper.CheckDecl(messages, dcl, this)))
         {
            myAddSubItem(declSpecifier);

            return true;
         }
         else { return false; }
      }

      public void RemoveFromScopeSpace(CItem item) => myRemoveSubItem(item);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="statements"></param>
      public void AddStatements(params CStatement[] statements) => myAddSubItemRange(statements);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="item"></param>
      /// <param name="scopeHelper"></param>
      /// <param name="messages"></param>
      /// <returns></returns>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public bool AddToScopeSpace(CItem item, CScopeHelperBase? scopeHelper, MsgCollection messages)
      {
         if (item is CStatement sta)
         {
            AddStatements(sta);

            return true;
         }
         else if (item is CDeclSpecifiers dcl_spc) { return AddDeclSpec(messages, dcl_spc, scopeHelper); }
         else { throw new Gate.Tools.ToolsException($"{item.GetType().Name} not allowed!"); }
      }
   }
}
