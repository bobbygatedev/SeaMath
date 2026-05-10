namespace Gate.Tools.AppParams
{
   public interface INestedContainerArray
   {
      /// <summary>
      /// All <see cref="AppParamContainer"/> recurisively contained in this array.
      /// </summary>
      INestedContainer[] AllNestedContainersRecursively { get; }

      INestedContainer[] Items { get; }
   }
}