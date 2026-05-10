using Gate.CLanguage.Decl;

namespace Gate.CLanguage.Statement
{
   /// <summary>
   /// Body(Block) of compound containing the list of statement.
   /// </summary>
   public class CBlockCompound : CBlock
   {
      public CBlockCompound()
      {
         
      }

      public CCompound? ParentCompound => ParentItem as CCompound;

      public override string Descriptor => $"Block of {ParentCompound?.Descriptor}";

      /// <summary>
      /// 
      /// </summary>
      public override CDecl[] ScopeDecls
      {
         get
         {
            var par_cyc = ParentCompound?.ParentCycleFor;

            if (par_cyc != null)
            {
               return par_cyc.Body.ScopeDecls.Concat(myGetScopeDeclsDefault()).ToArray();
            }
            else
            {
               return myGetScopeDeclsDefault();
            }
         }
      }
   }
}
