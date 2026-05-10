using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaDeclVar : CDeclVar
   {
      public SeaDeclVar(string? identifier = null)
      {
         var dcl_spc = new CDeclSpecifiers();

         dcl_spc.AddDecl(this);
         dcl_spc.TypeBase = SeaType.Instance;
         Identifier = identifier;
      }
   }
}
