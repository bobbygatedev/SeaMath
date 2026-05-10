namespace Gate.LangBase
{
   /// <summary>
   /// 
   /// </summary>
   public interface IWithIdentifier
   {
      /// <summary>
      /// Id(name) of the item, impersonate var/type/function/macro name.
      /// </summary>
      string? Identifier { get; }

      /// <summary>
      /// 
      /// </summary>
      bool IsAnonimous { get; }
   }
}
