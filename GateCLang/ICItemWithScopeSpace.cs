using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;
using Gate.Tools.Message;

namespace Gate.CLanguage
{
   public interface ICItemWithScopeSpace : ICItem
   {
      CScope Scope { get; }
      CDecl[] ScopeDecls { get; }

      bool AddDeclSpec(MsgCollection messages, CDeclSpecifiers declSpecifier, CScopeHelperBase? scopeHelper = null);
      bool AddToScopeSpace(CItem item, CScopeHelperBase? scopeHelper, MsgCollection messages);
      void RemoveFromScopeSpace(CItem item);
   }
}