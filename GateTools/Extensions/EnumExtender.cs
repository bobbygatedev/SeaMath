namespace Gate.Tools.Extensions
{
   /// <summary>
   ///     A generic extension method that aids in reflecting 
   ///     and retrieving any attribute that is applied to an `Enum`.
   /// </summary>
   public static class EnumExtender
   {
      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="TAttribute"></typeparam>
      /// <param name="enumValue"></param>
      /// <returns></returns>
      public static TAttribute? GetAttribute<TAttribute>(this Enum enumValue)
              where TAttribute : Attribute =>
         enumValue.GetType().GetMember(enumValue.ToString()).First().GetCustomAttributes(false).OfType<TAttribute>().FirstOrDefault();


      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="TAttribute"></typeparam>
      /// <param name="enumValue"></param>
      /// <param name="attribute"></param>
      /// <returns></returns>
      public static bool HasAttribute<TAttribute>(this Enum enumValue, out TAttribute? attribute)
         where TAttribute : Attribute
      {
         attribute = enumValue.GetAttribute<TAttribute>();

         return attribute != null;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="TAttribute"></typeparam>
      /// <param name="enumValue"></param>
      /// <returns></returns>
      public static bool HasAttribute<TAttribute>(this Enum enumValue) where TAttribute : Attribute => HasAttribute<TAttribute>(enumValue, out _);

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="TEnum"></typeparam>
      /// <param name="enumLabel"></param>
      /// <returns></returns>
      public static TEnum ParseEnum<TEnum>(this string enumLabel) where TEnum : Enum => (TEnum)Enum.Parse(typeof(TEnum), enumLabel);

      /// <summary>
      /// True if all of <paramref name="flags"/> are asserted (ie <paramref name="flags"/> AND <paramref name="enumValue"/> == <paramref name="flags"/> )
      /// </summary>
      /// <typeparam name="TEnum"></typeparam>
      /// <param name="enumValue"></param>
      /// <param name="flags"></param>
      /// <returns></returns>
      public static bool IsAllFlags<TEnum>(this TEnum enumValue, TEnum flags) where TEnum : Enum =>
         ((dynamic)enumValue & (dynamic)flags) == (dynamic)flags;

      /// <summary>
      /// True if any of <paramref name="flags"/> is asserted (ie <paramref name="flags"/> AND <paramref name="enumValue"/> != 0 )
      /// </summary>
      /// <typeparam name="TEnum"></typeparam>
      /// <param name="enumValue"></param>
      /// <param name="flags"></param>
      /// <returns></returns>
      public static bool IsAnyFlag<TEnum>(this TEnum enumValue, TEnum flags) where TEnum : Enum =>
         ((dynamic)enumValue & (dynamic)flags) != 0;

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="TEnum"></typeparam>
      /// <param name="enumValue"></param>
      /// <returns></returns>
      public static TEnum[] AllValues<TEnum>(this TEnum enumValue) where TEnum : Enum => Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="TEnum"></typeparam>
      /// <returns></returns>
      public static TEnum[] AllValues<TEnum>() where TEnum : Enum => Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();
   }
}
