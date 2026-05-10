namespace Gate.LangBase.Expressions.Operators
{
   /// <summary>
   /// <br> Operator flags 1-bit flags shall be used as <seealso cref="BasicOperatorAttribute"/> value, </br>
   /// <br> compound for operator selection in <seealso cref="Gate.LangBase.Expressions.ExprSolver{IN_DATA}.BasicOperatorFlags"/> </br>
   /// </summary>
   [Flags]
   public enum BasicOperatorTypeFlags
   {
      /// <summary>
      /// Expr solver system decides the set of operators by own
      /// </summary>
      none = 0x0,

      /// <summary>
      /// Typical operator for every expression solver systems.
      /// </summary>
      minimal = 0x1,

      /// <summary>
      /// Operator which is typical in C/C++ (like postfix/prefix inc '()++','++()').
      /// </summary>
      c_operator = 0x2,

      /// <summary>
      /// Operator used in preprocessing
      /// </summary>
      c_preprox = 0x4,

      /// <summary>
      /// 
      /// </summary>
      c_always = c_operator | c_preprox,
   }
}
