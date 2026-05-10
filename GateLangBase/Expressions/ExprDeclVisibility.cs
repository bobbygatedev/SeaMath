namespace Gate.LangBase.Expressions
{
   /// <summary>
   /// 
   /// </summary>
   public enum ExprDeclVisibility
   {
      /// <summary>
      /// 
      /// </summary>
      local = 0,

      /// <summary>
      /// A global object that is not static
      /// </summary>
      global_extern = 1,

      /// <summary>
      /// An object contained in a function that is static
      /// </summary>
      local_static = 2,

      /// <summary>
      /// A global object that is static
      /// </summary>
      global_static = 4,

      /// <summary>
      /// Declaration inside a struct, union or class
      /// </summary>
      in_class = 3,
   }
}
