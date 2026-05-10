using Microsoft.CSharp;
using System.CodeDom;

namespace Gate.Tools.Extensions
{
   public static class TypeExtender
   {
      private static readonly CSharpCodeProvider provider = new CSharpCodeProvider();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="type"></param>
      /// <param name="constructorType"></param>
      /// <returns></returns>
      public static bool IsBuildable(this Type type, Type[]? constructorType = null) =>
       !type.IsAbstract && type.GetConstructor(constructorType ?? new Type[0]) != null;

      /// <summary>
      /// Creates an instance of the specified type using the provided constructor parameters or throws an exception if
      /// no matching constructor is found.
      /// </summary>
      /// <param name="type">The type to instantiate.</param>
      /// <param name="Params">An optional array of arguments to pass to the constructor.</param>
      /// <param name="constructorType">An optional array of types representing the constructor's parameter types.</param>
      /// <returns>A new instance of the specified type.</returns>
      /// <exception cref="Crash">Thrown when no constructor matching the specified types is found.</exception>
      public static object InstanciateOrCrash(this Type type, object[]? Params = null, Type[]? constructorType = null) =>
         (type.GetConstructor(constructorType ?? []) ??
         throw new Crash(
            $"Not constructor found {type.Name}({string.Join(",", (constructorType ?? []).Select(t => t.Name))})")).Invoke(Params ?? []);

      /// <summary>
      /// Tries to create an instance of the specified type using the provided parameters and constructor types.
      /// </summary>
      /// <param name="type"></param>
      /// <param name="Params"></param>
      /// <param name="constructorType"></param>
      /// <returns></returns>
      public static object? InstanciateOrNull(this Type type, object[]? Params = null, Type[]? constructorType = null)
      {
         var cst = type.GetConstructor(constructorType ?? []);

         return cst != null ? cst.Invoke(Params ?? []) : null;
      }

      public static bool HasDefaultConstructor(this Type type) =>
         !type.IsAbstract && type.GetConstructor(new Type[0]) != null;

      public static bool IsMeOrSubClass(this Type type, Type other) =>
         type == other || type.IsSubclassOf(other);

      public static Type[] GetNestedTypesRecursively(this Type type)
      {
         var lst = type.GetNestedTypes().ToList();

         foreach (var typ in lst.ToArray())
         {
            lst.AddRange(typ.GetNestedTypesRecursively());
         }

         return lst.ToArray();
      }

      public static string GetTypeAlias(this Type type) => provider.GetTypeOutput(new CodeTypeReference(type));

      /// <summary>
      /// Tries to convert the object to the specified type T. If the conversion is successful, it returns the converted value. 
      /// If the conversion fails, it throws a Crash exception with a message indicating the failure.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="obj"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      public static T ConvertOrCrash<T>(this object? obj) => obj is T t ?
            t : throw new Crash($"Cannot convert object of type {obj?.GetType().Name ?? "null"} to type {typeof(T).Name}");

      /// <summary>
      /// Ensures that the specified object is not null, throwing a Crash exception if it is.
      /// </summary>
      /// <typeparam name="T">The reference type of the object to check for null.</typeparam>
      /// <param name="obj">The object to validate for non-nullity.</param>
      /// <returns>The original object if it is not null.</returns>
      /// <exception cref="Crash">Thrown if obj is null.</exception>
      public static T NnOrCrash<T>(this T? obj) where T : class => obj ?? throw new Crash($"Object of type {typeof(T).Name} is null");

      /// <summary>
      /// Ensures that the specified object is not null, throwing a Crash exception if it is.
      /// </summary>
      /// <typeparam name="T">The reference type of the object to check for null.</typeparam>
      /// <param name="obj">The object to validate for non-nullity.</param>
      /// <returns>The original object if it is not null.</returns>
      /// <exception cref="Crash">Thrown if obj is null.</exception>
      public static T NnOrCrash<T>(this T? obj) where T : struct => obj ?? throw new Crash($"Object of type {typeof(T).Name} is null");

      public static T ElementAtOrCrash<T>(this IEnumerable<T?>? source, int index) where T : class => 
         source.NnOrCrash().ElementAtOrDefault(index).NnOrCrash();

      public static T ElementAtOrCrash<T>(this IEnumerable<T?>? source, int index) where T : struct =>
         source.NnOrCrash().ElementAtOrDefault(index).NnOrCrash<T>();
   }
}
