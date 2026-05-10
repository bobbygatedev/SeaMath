using Gate.CLanguage.Decl;
using Gate.CLanguage.Types;
using Gate.Tools.Message;

namespace Gate.CLanguage
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class CScopeHelperBase
   {
      private static List<CScopeHelperBase>? myListScopeHelperDefaults = null;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="settings"></param>
      protected CScopeHelperBase(CScopeSettings settings) => Settings = settings;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="language"></param>
      /// <returns></returns>
      public static CScopeHelperBase? GetDefault(CLanguage language)
      {
         if (myListScopeHelperDefaults == null)
         {
            myListScopeHelperDefaults = [new CScopeHelper(CScopeSettings.Default)];
         }

         return myListScopeHelperDefaults.FirstOrDefault(sh => sh.Language == language);
      }

      /// <summary>
      /// 
      /// </summary>
      public abstract CLanguage Language { get; }

      /// <summary>
      /// All declarations visible in scope of <paramref name="itemWithScope"/>.
      /// </summary>
      /// <param name="itemWithScope"></param>
      /// <returns></returns>
      public abstract CDeclStorage[] GetDeclStoragesFunctionVisible(CItemWithScopeSpace itemWithScope);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="decl"></param>
      /// <returns></returns>
      public abstract string? GetDeclSignature(CDecl decl);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="type"></param>
      /// <returns></returns>
      public abstract string? GetTypeSignature(CType type);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="itemWithScope"></param>
      /// <returns></returns>
      public abstract CTypeUserDefined[] GetTypesUsersFunctionVisible(CItemWithScopeSpace itemWithScope);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="itemWithScopeSpace"></param>
      /// <returns></returns>
      public abstract CDeclTypedef[] GetTypedefsFunctionVisible(CItemWithScopeSpace itemWithScopeSpace);

      /// <summary></summary>
      /// 
      /// <param name="itemWithScope"></param>
      /// <returns></returns>
      public abstract CTypeUserDefined[] GetUserTypesScope(CItemWithScopeSpace itemWithScope);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="messages"></param>
      /// <param name="declSpecs"></param>
      /// <param name="itemWithScope"></param>
      /// <returns></returns>
      public abstract bool CheckDecl(MsgCollection messages, CDecl decl, CItemWithScopeSpace itemWithScope);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="messages"></param>
      /// <param name="typeUserDefined"></param>
      /// <param name="itemWithScope"></param>
      /// <returns></returns>
      public abstract bool CheckUserDefType(MsgCollection messages, CTypeUserDefined typeUserDefined, CItemWithScopeSpace itemWithScope);

      /// <summary>
      /// 
      /// </summary>
      public CScopeSettings Settings { get; }
   }
}
