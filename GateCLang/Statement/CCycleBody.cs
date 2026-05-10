using Gate.CLanguage.Decl;
using Gate.CLanguage.Expressions;

namespace Gate.CLanguage.Statement
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class CCycleBody : CItemWithScopeSpace
   {
      private CExprStatement? myCondition;

      protected CCycleBody()
      {
         
      }

      /// <summary>
      /// 
      /// </summary>
      public CExprStatement? Condition
      {
         get => myCondition;
         set
         {
            if (myCondition != value)
            {
               myRemoveSubItem(myCondition);

               if ((myCondition = value) != null) { myAddSubItem(myCondition); }
            }
         }
      }

      public override CDecl[] ScopeDecls => AllDescendant.OfType<CDecl>().ToArray();

      public CCycle? Cycle => ParentItem as CCycle;
   }
}
