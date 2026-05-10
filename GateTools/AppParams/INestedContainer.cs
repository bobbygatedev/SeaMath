using static Gate.Tools.AppParams.AppParam;

namespace Gate.Tools.AppParams
{
   /// <summary>
   /// 
   /// </summary>
   public interface INestedContainer
   {
      /// <summary>
      /// 
      /// </summary>
      NestedFlags Flags { get; }

      /// <summary>
      /// 
      /// </summary>
      AppParamContainer? LoadedParamContainer { get; }

      /// <summary>
      /// All <see cref="AppParamContainer"/> recurisively contained in this array.
      /// </summary>
      INestedContainer[] AllNestedContainersRecursively { get; }

      /// <summary>
      /// 
      /// </summary>
      RelativeType Relative { get; }
   }
}