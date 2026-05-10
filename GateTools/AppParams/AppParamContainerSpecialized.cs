namespace Gate.Tools.AppParams
{
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="P_R"></typeparam>
   public abstract class AppParamContainerSpecialized<P_R> : AppParamContainer where P_R : AppParam.Record, new()
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="isAutoLoad"></param>
      public AppParamContainerSpecialized(bool isAutoLoad = false) : base(isAutoLoad) { }

      public new P_R Params => (P_R)base.Params;

      protected override AppParam.Record myMakeParamsRecord() => new P_R();
   }
}

