namespace Gate.CLanguage.Decl
{
   /// <summary>
   /// <br>  Generalized array of indices for subscript (may include both array int indices and class field name string).</br>
   /// <br>  eg  'struct { int f1; } a[2][3]' defines subscript such as '([1][2]).f1'  </br>
   /// </summary>
   public struct CDeclSubscriptIndices
   {
      private object[] myArray;

      public CDeclSubscriptIndices(params object[] array) => myArray = array;

      public object[] Array
      {
         get => myArray ?? [];
         set
         {
            value = value ?? [];

            if (value.Any(itm => !(itm is string || itm is int)))
            {
               throw new Gate.CLanguage.CLangException($"Item not of type string/int");
            }

            myArray = value;
         }
      }

      public string StringVal
      {
         get
         {
            var str = "";

            for (var i = 0; i < Array.Length; i++)
            {
               object itm = Array[i];

               if (itm is int) { str += $"[{itm}]"; }
               else
               {
                  str = $"{str}.{itm}";//in case an array is preceding parenthesis are added
               }
            }

            return str;
         }
      }

      public CDeclSubscriptIndices Dereference => new CDeclSubscriptIndices(myArray.Take(myArray.Length - 1).ToArray());

      public override string ToString() => StringVal;

      public static bool operator ==(CDeclSubscriptIndices itm1, CDeclSubscriptIndices itm2) => itm1.Equals(itm2);

      public static bool operator !=(CDeclSubscriptIndices itm1, CDeclSubscriptIndices itm2) => !itm1.Equals(itm2);

      public static CDeclSubscriptIndices operator +(CDeclSubscriptIndices indices, string field) =>
         new CDeclSubscriptIndices(indices.Array.Append(field).ToArray());

      public static CDeclSubscriptIndices operator +(CDeclSubscriptIndices indices, int arrayIndex) =>
         new CDeclSubscriptIndices(indices.Array.Append(arrayIndex).ToArray());

      public static CDeclSubscriptIndices operator +(CDeclSubscriptIndices ids1, CDeclSubscriptIndices ids2) =>
         new CDeclSubscriptIndices(ids1.Array.Concat(ids2.Array).ToArray());

      public static CDeclSubscriptIndices operator +(CDeclSubscriptIndices ids1, int[] intIndices) =>
         new CDeclSubscriptIndices(ids1.Array.Concat(intIndices.Cast<object>()).ToArray());

      public override bool Equals(object? obj) => obj is CDeclSubscriptIndices idx && idx.Array.SequenceEqual(Array);

      /// <summary>
      /// Generates a hash code for the array indices and field names.
      /// Uses FNV-1a-style hash combining to avoid overflow.
      /// </summary>
      /// <returns>A hash code that represents the current indices</returns>
      public override int GetHashCode()
      {
         if (Array == null) return 0;
         
         unchecked // Prevent overflow checks
         {
            const int see = 17;
            var hsh = see;

            foreach (var item in Array)
            {
               hsh = hsh * 23 + (item?.GetHashCode() ?? 0);
            }

            return hsh;
         }
      }
   }
}
