namespace Gate.Tools.Enumerations.Extensions
{
   public static class EnumAttributes
   {
      /// <summary>
      ///     A generic extension method that aids in reflecting 
      ///     and retrieving any attribute that is applied to an `Enum`.
      /// </summary>
      public static TAttribute? GetAttribute<TAttribute>(this Enum enumValue)
              where TAttribute : Attribute =>
         enumValue.GetType().GetMember(enumValue.ToString()).First().GetCustomAttributes(false).OfType<TAttribute>().FirstOrDefault();


      public static TEnum ParseEnum<TEnum>(this string enumLabel) where TEnum : Enum => (TEnum)Enum.Parse(typeof(TEnum), enumLabel);
   }
}
