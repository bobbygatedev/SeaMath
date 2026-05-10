namespace Gate.CLanguage.PrePx.Directives.Macro.Predefined
{
   /// <summary>
   /// Data storage for system macro data population (eg __LINE__ , __FILE__)
   /// </summary>
   public class CPredefMacroData
   {
      /// <summary>
      /// Constructor.
      /// </summary>
      public CPredefMacroData()
      {
         var now = DateTime.Now;

         CompilationDate = myFormatDate(now);
         CompilationTime = myFormatTime(now);
      }

      /// <summary>
      /// 
      /// </summary>
      public string? CurrFile { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public int? CurrLine { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public string CompilationTime { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public string CompilationDate { get; private set; }

      private static string myFormatDate(DateTime now)
      {
         var months = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

         return string.Format("{0} {1:00} {2:0000}", months[now.Month - 1], now.Day, now.Year);
      }

      private static string myFormatTime(DateTime now) { return string.Format("{0:00}:{1:00}:{2:00}", now.Hour, now.Minute, now.Second); }
   }
}
