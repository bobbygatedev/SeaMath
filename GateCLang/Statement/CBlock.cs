using Gate.CLanguage.DeclSpecifiers;
using Gate.Tools.Message;

namespace Gate.CLanguage.Statement
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class CBlock : CItemWithScopeSpace, IBlock
   {
      /// <summary>
      /// 
      /// </summary>
      protected CBlock() { }

      public void AddStatements(params CStatement[] statements) => myAddSubItemRange(statements);

      public override string Rebuilt => myGetBraceRebuilt();

      public override bool AddToScopeSpace(CItem item, CScopeHelperBase? scopeHelper, MsgCollection messages)
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
