namespace Gate.Tools.AppParams
{
   /// <summary>
   /// Project-like <see cref="AppParamContainer"/> path is updated at any load/save. 
   /// </summary>
   /// <typeparam name="P_R"></typeparam>
   public abstract class AppParamProject<P_R> : AppParamContainerSpecialized<P_R> where P_R : AppParam.Record, new()
   {
      public AppParamProject() : base(false) { }

      public sealed override string? FixedPath => null;
   }
}

