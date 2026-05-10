using Gate.Tools.Arry;
using Gate.Tools.Message;

namespace Gate.Tools.Watch
{
   /// <summary>
   /// Expression and try to parse and update a watch expression.
   /// </summary>
   public interface IWatchExprManager
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="watchExpr"></param>
      /// <returns></returns>
      ArrayMultidimensional<WatchExpr> GetChildMatrixArray(WatchExpr watchExpr);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="watchExpr"></param>
      /// <param name="newValue"></param>
      /// <param name="value"></param>
      /// <returns></returns>
      bool Update(WatchExpr watchExpr, string newValue, out object? value);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="expression"></param>
      /// <param name="value"></param>
      /// <param name="isReadOnly"></param>
      /// <param name="errorMsg"></param>
      /// <returns></returns>
      bool TryParse(string expression, out object? value, out bool isReadOnly, out Msg? errorMsg);

      /// <summary>
      /// Returns string representation of an object (cast to item type).
      /// </summary>
      /// <param name="value"></param>
      /// <returns></returns>
      string? GetObjectString(object value);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="value"></param>
      /// <returns></returns>
      string? GetTypeString(object value);
   }
}
