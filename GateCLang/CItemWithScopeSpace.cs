using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;
using Gate.Tools;
using Gate.Tools.Message;

namespace Gate.CLanguage
{
   /// <summary>
   /// Represents an item which has a scope space (eg function, struct/union/enum body, block).
   /// </summary>
   public abstract class CItemWithScopeSpace : CItem, ICItemWithScopeSpace
   {
      /// <summary>
      /// 
      /// </summary>
      public CItemWithScopeSpace() => Scope = new CScope(this);

      /// <summary>
      /// 
      /// </summary>
      public override bool HasAssociatedPragma => false;

      /// <summary>
      /// 
      /// </summary>
      public CScope Scope { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="item"></param>
      /// <param name="scopeHelper"></param>
      /// <param name="messages"></param>
      /// <returns></returns>
      public abstract bool AddToScopeSpace(CItem item, CScopeHelperBase? scopeHelper, MsgCollection messages);

      /// <summary>
      /// 
      /// </summary>
      public abstract CDecl[] ScopeDecls { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="item"></param>
      public void RemoveFromScopeSpace(CItem item) => myRemoveSubItem(item);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="messages"></param>
      /// <param name="declSpecifier"></param>
      /// <param name="scopeHelper"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      public virtual bool AddDeclSpec(MsgCollection messages, CDeclSpecifiers declSpecifier, CScopeHelperBase? scopeHelper = null)
      {
         scopeHelper = scopeHelper ?? CScopeHelperBase.GetDefault(Language) ?? throw new Crash();

         if (declSpecifier.Decls.Where(d => !d.IsAnonimous).All(dcl => scopeHelper.CheckDecl(messages, dcl, this)))
         {
            myAddSubItem(declSpecifier);

            return true;
         }
         else { return false; }
      }
   }
}
