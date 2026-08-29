namespace Gate.Dock
{

   public static class GateDockAppFolder
   {
      public static string GetStandard(string appName) => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
   }
}
