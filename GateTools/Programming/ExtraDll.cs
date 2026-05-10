using Gate.Tools.Extensions;
using System.Reflection;

namespace Gate.Tools.Programming
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class ExtraDll
   {
      /// <summary>
      /// 
      /// </summary>
      private Lazy<DllInstance> myLazyDllInstance;

      private readonly List<ExtraDll> myListRegisteredDlls = new List<ExtraDll>();

      protected ExtraDll(DirectoryInfo? alternateDir)
      {
         AlternateDir = alternateDir ?? WorkingDir ?? throw new Crash();
         myLazyDllInstance = new Lazy<DllInstance>(() => new DllInstance(DllFile.FullName));
      }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8603 // Possible null reference return.
      public static DirectoryInfo WorkingDirDefault => new FileInfo(Assembly.GetEntryAssembly().Location).Directory;
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8602 // Dereference of a possibly null reference.

      public DirectoryInfo WorkingDir => AlternateDir ?? WorkingDirDefault;

      public abstract string DllName { get; }

      /// <summary>
      /// 
      /// </summary>
      public DllInstance DllInstance => myLazyDllInstance.Value;

      public FileInfo DllFile => WorkingDir.GetCombinedToFile(DllName);

      public DirectoryInfo? AlternateDir { get; }

      public ExtraDll[] RegisteredDlls => myListRegisteredDlls.ToArray();

      public bool HasBeenRegistered { get; private set; } = false;

      protected bool myRegister()
      {
         lock (RegisteredDlls)
         {
            if (!HasBeenRegistered)
            {
               if (RegisteredDlls.Any(c => c.DllName.IsEqualNoContent(DllName)))
               {
                  throw new Crash(
                     $"A {GetType().Name} instance with unique DllName = {DllName} " +
                     $"has been registered inside assembly {Assembly.GetEntryAssembly()}");
               }

               myListRegisteredDlls.Add(this);
               HasBeenRegistered = true;
            }

            return HasBeenRegistered;
         }
      }
   }
}
