namespace Gate.Tools.Programming
{
   /// <summary>
   /// Represents a precompiled DLL with additional functionality.
   /// </summary>
   public class ExtraDllPreCompiled : ExtraDll
   {
      public ExtraDllPreCompiled(string dllName, DirectoryInfo? alternateDir = null) : base(alternateDir)
      {
         switch (Path.GetExtension(dllName).ToLower())
         {
            case ".dll":
               DllName = Path.GetFileNameWithoutExtension(dllName);
               break;

            case "":
               DllName = dllName;
               break;

            default: throw new Crash("Expected .dll or nothing!");
         }

         myRegister();
      }

      public override string DllName { get; }
   }
}
