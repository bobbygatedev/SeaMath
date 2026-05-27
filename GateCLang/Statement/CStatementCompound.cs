using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Initialisation;
using Gate.Tools;
using Gate.Tools.Message;

namespace Gate.CLanguage.Statement
{
   /// <summary>
   /// Space of source code containing statements (ie Declarations,Expression, Other statements (if,while,goto,..)
   /// </summary>
   public class CStatementCompound : CStatement , ICItemWithScopeSpace
   {
      public CStatementCompound() => Scope = new CScope(this);

      public CStatement[] Statements => SubItems.OfType<CStatement>().ToArray();

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

      public CItem[] ExecutableLines => AllDescendant.Where(
         d => d is CDeclVar || (d is CStatement && !(d.ParentItem is CInitialisation))).OfType<CItem>().ToArray();

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
