namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   ///
   /// </summary>
   public class ShortCutPair : ICloneable
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="key1"></param>
      /// <param name="key2"></param>
      public ShortCutPair(Keys key1 = Keys.None, Keys key2 = Keys.None)
      {
         Key1 = key1;
         Key2 = key2;
      }

      /// <summary>
      /// 
      /// </summary>
      public Keys Key1 { get; set; } = Keys.None;

      /// <summary>
      /// 
      /// </summary>
      public Keys Key2 { get; set; } = Keys.None;

      /// <summary>
      /// 
      /// </summary>
      public string Text
      {
         get
         {
            //shortcut is valid if and only if shortCut is not none
            var pai = new Keys[] { Key1, Key1 == Keys.None ? Keys.None : Key2 }.Where(s => s != Keys.None).ToArray();

            return string.Join(",", pai.Select(s => my_TranslateShortCut(s)));
         }
      }

      public static bool operator ==(ShortCutPair? pair1, ShortCutPair? pair2) => ReferenceEquals(pair1, null) ? ReferenceEquals(pair2, null) : pair1.Equals(pair2);

      public static bool operator !=(ShortCutPair? pair1, ShortCutPair? pair2) => !(pair1 == pair2);

      public override bool Equals(object? obj) => obj is ShortCutPair pai && Key1 == pai.Key1 && Key2 == pai.Key2;

      public override string ToString() => Text;

      public override int GetHashCode()
      {
         var hashCode = 365011897;

         hashCode = hashCode * -1521134295 + Key1.GetHashCode();
         hashCode = hashCode * -1521134295 + Key2.GetHashCode();

         return hashCode;
      }

      private static string my_TranslateShortCut(Keys shortCut)
      {
         var lst_str = new List<string>();
         var key = shortCut;

         if ((key & Keys.Control) != 0)
         {
            lst_str.Add("Ctrl");
            key &= ~Keys.Control;
         }

         if ((key & Keys.Shift) != 0)
         {
            lst_str.Add("Shift");
            key &= ~Keys.Shift;
         }

         lst_str.Add(key.ToString());

         return string.Join("+", lst_str);
      }

      public object Clone()
      {
         var cpy = new ShortCutPair(Key1, Key2);

         return cpy;
      }
   }
}
