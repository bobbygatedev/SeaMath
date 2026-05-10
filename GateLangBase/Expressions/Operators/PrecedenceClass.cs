namespace Gate.LangBase.Expressions.Operators
{
   /// <summary>
   /// Precedence class of c/c++ operators.
   /// </summary>
   public class PrecedenceClass
   {
      static PrecedenceClass[] myVectorByPrec = Enumerable.Range(1, 17).Select(i => new PrecedenceClass(i)).ToArray();

      public PrecedenceClass(int precedenceGroup)
      {
         PrecedenceGroup = precedenceGroup;
         Associativity =
            Enumerable.Range(1, 2).Contains(precedenceGroup) ||
            Enumerable.Range(4, 11).Contains(precedenceGroup) ||
            Enumerable.Range(17, 1).Contains(precedenceGroup) ?
               Associativity.left2right : Associativity.right2left;
      }

      public static PrecedenceClass Level(int precedenceGroup) => myVectorByPrec.First(p => p.PrecedenceGroup == precedenceGroup);

      public Associativity Associativity { get; }

      public int PrecedenceGroup { get; }

      public override bool Equals(object? obj) => obj is PrecedenceClass && (obj as PrecedenceClass)?.PrecedenceGroup == PrecedenceGroup;

      public override int GetHashCode() => PrecedenceGroup;

      public override string ToString() => $"{PrecedenceGroup}({Associativity})";
   }
}
