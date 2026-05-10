using Gate.CLanguage;
using Gate.CLanguage.Decl;
using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.SeaMath.Sea
{
   public class SeaScopeHelper : CScopeHelper
   {
      public SeaScopeHelper(SeaMathDbgIde dbgIde, bool isForConsole) : base(CScopeSettings.Gcc)
      {
         DbgIde = dbgIde ?? throw new Crash();
         IsForConsole = isForConsole;
      }

      public SeaMathDbgIde DbgIde { get; }
      public bool IsForConsole { get; }

      public override CDeclStorage[] GetDeclStoragesFunctionVisible(CItemWithScopeSpace itemWithScope)
      {
         var bas_vis = base.GetDeclStoragesFunctionVisible(itemWithScope);
         var lbs = DbgIde.Workspace.Libs.All.SelectMany(lib => lib.Decls.OfType<CDeclStorage>()).ToArray();
         var dcs = DbgIde.Console.ObjVisibleForConsole.Select(o => o.Decl).OfType<CDeclStorage>().ToArray();

         if (IsForConsole)
         {
            return myGetUniqueByGlobalId(bas_vis.Concat(lbs).Concat(dcs));
         }
         else
         {
            return myGetUniqueByGlobalId(bas_vis.Concat(lbs));
         }
      }

      private static CDeclStorage[] myGetUniqueByGlobalId(IEnumerable<CDeclStorage> decls) => decls.Where(v => !v.Identifier.IsBlank()).
            OrderBy(v => v.GlobalId).
            GroupBy(v => v.Identifier).
            Select(g => g.Last()).
            ToArray();
   }
}
