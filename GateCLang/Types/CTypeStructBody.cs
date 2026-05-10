using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;
using Gate.Tools.Message;

namespace Gate.CLanguage.Types
{
   /// <summary>
   /// 
   /// </summary>
   public class CTypeStructBody : CItemWithScopeSpace, ITypeClassBody
   {
      public CTypeStructBody()
      {

      }

      public override string Descriptor => $"Body of {ParentStruct}";

      /// <summary>
      /// 
      /// </summary>
      public override bool HasAssociatedPragma => false;

      public CTypeStruct? ParentStruct => ParentItem as CTypeStruct;

      public override string Rebuilt => myGetBraceRebuilt();

      public override CDecl[] ScopeDecls =>
         ParentStruct?.ContainingClass != null &&
            ParentStruct.ParentDeclSpecifiers != null &&
            ParentStruct.ParentDeclSpecifiers.IsAnonimous ?
               ParentStruct.ContainingClass.Fields : ParentStruct?.Fields ?? [];

      ITypeClass? ITypeClassBody.ParentClass => ParentStruct;

      public override bool AddToScopeSpace(CItem item, CScopeHelperBase? scopeHelper, MsgCollection messages) =>
            item is CDeclSpecifiers dcl_spc ?
               AddDeclSpec(messages, dcl_spc, scopeHelper) :
               throw new Gate.Tools.ToolsException($"{item.GetType().Name} not allowed here!");

      public override string ToString() => Descriptor;
   }
}
