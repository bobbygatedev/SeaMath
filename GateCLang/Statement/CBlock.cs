using Gate.CLanguage.DeclSpecifiers;
using Gate.Tools.Message;

namespace Gate.CLanguage.Statement
{
   /// <summary>
   /// Statement container.
   /// </summary>
   public abstract class CBlock : CItemWithScopeSpace, IBlock
   {
      /// <summary>
      /// 
      /// </summary>
      protected CBlock() { }

      /// <summary>
      /// 
      /// </summary>
      public CStatement[] Statements => SubItems.OfType<CStatement>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public override string Rebuilt => myGetBraceRebuilt();

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
