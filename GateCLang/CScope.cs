using Gate.CLanguage.Decl;
using Gate.CLanguage.Source;
using Gate.CLanguage.Types;
using Gate.Tools.Extensions;

namespace Gate.CLanguage
{
   /// <summary>
   /// Deal with scope 
   /// </summary>
   public class CScope
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="parent"></param>
      public CScope(ICItemWithScopeSpace parent) => ItemWithScopeSpace = parent;

      /// <summary>
      /// 
      /// </summary>
      public CScopeHelperBase? Helper
      {
         get
         {
            var itm = ItemWithScopeSpace as CItem ?? throw new CLangException("Unexpected!");
            var src = itm?.Source;

            return src != null ? src.ScopeHelper : CScopeHelperBase.GetDefault(itm?.Language ?? CLanguage.c);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public ICItemWithScopeSpace ItemWithScopeSpace { get; }

      /// <summary>
      /// Returns first instance of <see cref="CDecl"/> in scope or null.
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      public CDecl? this[string id] => VisibleDeclarations.FirstOrDefault(d => d.Identifier == id);

      /// <summary>
      /// 
      /// </summary>
      public string Tag => IsGlobal ? "GLOBAL" : "LOCAL";

      /// <summary>
      /// 
      /// </summary>
      public bool IsGlobal => ItemWithScopeSpace.GetType().IsMeOrSubClass(typeof(CSource));

      /// <summary>
      /// 
      /// </summary>
      public bool IsLocal => !IsGlobal;

      /// <summary>
      /// 
      /// </summary>
      public CDecl[] VisibleDeclarations => Helper?.GetDeclStoragesFunctionVisible(ItemWithScopeSpace) ?? [];

      /// <summary>
      /// <br> All visible vars inside scope, ie all scope variables plus the variables contained in all parent scope(parent compound, .. , global)</br>
      /// <br> In case of variable of with same name the deepest is considered.</br>
      /// </summary>
      public CDecl[] VisibleVarDefinitions => Helper?.GetDeclStoragesFunctionVisible(ItemWithScopeSpace).Where(d => d.IsDefinition).ToArray() ?? [];

      /// <summary>
      /// All typedef's inside scope.
      /// </summary>
      public CTypeUserDefined[] TypesUser => Helper?.GetUserTypesScope(ItemWithScopeSpace) ?? [];

      /// <summary>
      /// 
      /// </summary>
      public CTypeEnumLabel[] EnumLabels => TypesUser.OfType<CTypeEnum>().SelectMany(te => te.Labels).ToArray();

      /// <summary>
      /// <br> All visible typedefs inside scope, ie all scope variables plus the variables contained in all parent scope(parent compound, .. , global)</br>
      /// <br> In case of variable of with same name the deepest is considered.</br>
      /// </summary>
      public CTypeUserDefined[] TypesUsersFunctionVisible => Helper?.GetTypesUsersFunctionVisible(ItemWithScopeSpace) ?? [];

      /// <summary>
      /// 
      /// </summary>
      public CDeclTypedef[] TypedefsFunctionVisible => Helper?.GetTypedefsFunctionVisible(ItemWithScopeSpace) ?? [];

      public override string ToString() => $"{ItemWithScopeSpace.Descriptor}::Scope";
   }
}
