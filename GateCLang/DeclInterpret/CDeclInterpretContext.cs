namespace Gate.CLanguage.DeclInterpreter
{
   /// <summary>
   /// 
   /// </summary>
   public enum CDeclInterpretContext
   {
      /// <summary>
      /// Instance of <see cref="CDeclVar"/>  in local scope.
      /// </summary>
      local_var = 0,

      /// <summary>
      /// Instance of <see cref="CDeclVar"/>  in global scope.
      /// </summary>
      global_var,

      /// <summary>
      /// Inside identifier part of a function decl/def (eg 'void (fun)(int)' => '(fun)'
      /// </summary>
      function_id,

      /// <summary>
      /// Instance of <see cref="CDeclVar"/> inside function declaration param list (eg void f(int,int par2);)
      /// </summary>
      function_decl_param,

      /// <summary>
      /// Instance of <see cref="CDeclVar"/> inside function definition param list (eg void f(int par1,int par2){})
      /// </summary>
      function_def_param,

      /// <summary>
      /// class/struct/union field
      /// </summary>
      cclass_field,

      /// <summary>
      /// class method (C++ only)
      /// </summary>
      class_method,

      /// <summary>
      /// type name eg (void*), valid inside just expressions. 
      /// </summary>
      expr_type_name,

      /// <summary>
      /// Console (global variable only)
      /// </summary>
      console_var,
   }
}
