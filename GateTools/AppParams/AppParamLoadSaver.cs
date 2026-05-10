using Gate.Tools.Text;

namespace Gate.Tools.AppParams
{
   /// <summary>
   /// 
   /// </summary>
   public abstract partial class AppParamLoadSaver
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="msgs"></param>
      /// <param name="appParamContainer"></param>
      /// <param name="stringConverter"></param>
      /// <param name="path"></param>
      /// <returns></returns>
      public abstract bool Save(AppParamContainer appParamContainer, TxtStringConverter stringConverter, string path);

      /// <summary>
      /// Loads 
      /// </summary>
      /// <param name="appParamContainer"></param>
      /// <param name="stringConverter"></param>
      /// <param name="path"></param>
      /// <returns>True if successfully False if any error.</returns>
      public abstract bool Load(AppParamContainer appParamContainer, TxtStringConverter stringConverter, string path);
   }

}
