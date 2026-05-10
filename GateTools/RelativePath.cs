using Gate.Tools.Extensions;

namespace Gate.Tools
{
   /// <summary>
   /// 
   /// </summary>
   public class RelativePath
   {
      public delegate void OnChangeHandler(RelativePath path);

      public event OnChangeHandler? OnChangeAbsolute;
      public event OnChangeHandler? OnChangeBaseDir;

      private string? myAbsolute;
      private string? myRelativeTemp;
      private string? myBaseDir;

      [Flags]
      public enum OptionsType
      {
         /// <summary>
         /// Shall be either a file or a directory.
         /// </summary>
         none = 0,

         /// <summary>
         /// Shall be a directory.
         /// </summary>
         dir = 1,

         /// <summary>
         /// Shall be a file.
         /// </summary>
         file = 2,
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="baseDir"></param>
      /// <param name="absolutePath"></param>
      public RelativePath(OptionsType options = OptionsType.none, DirectoryInfo? baseDir = null, string? absolutePath = null)
      {
         Options = options;
         BaseDir = baseDir?.FullName;
         Absolute = absolutePath;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="baseDir"></param>
      /// <param name="absolutePath"></param>
      public RelativePath(DirectoryInfo? baseDir = null, string? absolutePath = null) : this(OptionsType.none, baseDir, absolutePath) { }

      /// <summary>
      /// Copy constructor
      /// </summary>
      /// <param name="other"></param>
      public RelativePath(RelativePath other, string? absolutePath = null)
      {
         Options = other.Options;
         myBaseDir = other.myBaseDir;
         myAbsolute = absolutePath ?? other.myAbsolute;
      }

      /// <summary>
      /// 
      /// </summary>
      public OptionsType Options { get; }

      /// <summary>
      /// 
      /// </summary>
      public DirectoryInfo? BaseDirInfo => !myBaseDir.IsBlank() ? new DirectoryInfo(myBaseDir.ExtTrim()) : null;

      /// <summary>
      ///  
      /// </summary>
      public string? BaseDir
      {
         get => myBaseDir;

         set
         {
            if (!myBaseDir.IsEqualNoContent(value, false))
            {
               myBaseDir = myGetCaseSanitized(value);

               if (IsBaseDirDefined && myRelativeTemp != null)
               {
                  myAbsolute = myGetCaseSanitized(Path.Combine(myBaseDir.ExtTrim(), myRelativeTemp));
                  myRelativeTemp = null;
                  OnChangeAbsolute?.Invoke(this);
               }

               OnChangeBaseDir?.Invoke(this);
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="baseDir"></param>
      /// <returns></returns>
      public RelativePath GetRebased(DirectoryInfo baseDir) => new RelativePath(baseDir, Absolute);

      /// <summary>
      /// Returns a path where case is case-sensitively to file system.
      /// </summary>
      /// <param name="path"></param>
      /// <returns></returns>
      private static string? myGetCaseSanitized(string? path)
      {
         if (path.IsBlank())
         {
            return null;
         }
         else
         {
            return path != null && Directory.Exists(path) ?
               new DirectoryInfo(path).GetFullPathCase().FullName :
               new FileInfo(path.ExtTrim()).GetFullPathCase().FullName;
         }
      }

      public string? Absolute
      {
         get => myAbsolute;

         set
         {
            if (!myAbsolute.IsEqualNoContent(value, false))
            {
               myFlagsCheck(value);

               if (!value.IsBlank())
               {
                  myAbsolute = myGetCaseSanitized(value);
                  OnChangeAbsolute?.Invoke(this);
               }
            }
         }
      }

      /// <summary>
      /// Gets the <see cref="DirectoryInfo"/> object representing the directory associated with the current path.
      /// </summary>
      /// <remarks>The returned directory depends on the value of the <c>Options</c> property: <list
      /// type="bullet"> <item> <description>If <c>Options</c> is <c>OptionsType.none</c>, the method returns the
      /// directory if it exists; otherwise, it returns the directory of the associated file, if
      /// available.</description> </item> <item> <description>If <c>Options</c> is <c>OptionsType.dir</c>, the method
      /// returns the directory specified by the path.</description> </item> <item> <description>If <c>Options</c> is
      /// <c>OptionsType.file</c>, the method returns the directory of the associated file, if available.</description>
      /// </item> </list></remarks>
      public DirectoryInfo? DirInfo
      {
         get
         {
            if (Absolute.IsBlank()) { return null; }
            else
            {
               switch (Options)
               {
                  case OptionsType.none: return Directory.Exists(Absolute) ? new DirectoryInfo(Absolute) : FileInfo?.Directory;
                  case OptionsType.dir: return new DirectoryInfo(Absolute.ExtTrim());
                  case OptionsType.file: return FileInfo?.Directory;
                  default: throw new Crash();
               }
            }
         }
      }

      /// <summary>
      /// Gets the <see cref="FileInfo"/> object representing the file associated with the current path,  or <see
      /// langword="null"/> if the path is blank, represents a directory, or does not exist.
      /// </summary>
      /// <remarks>The returned <see cref="FileInfo"/> object provides access to metadata and operations for
      /// the file  specified by the current path. If the path is blank, represents a directory, or the <c>Options</c> 
      /// property is set to a non-file-related value, the property returns <see langword="null"/>.</remarks>
      public FileInfo? FileInfo
      {
         get
         {
            if (Absolute.IsBlank()) { return null; }
            else
            {
               switch (Options)
               {
                  case OptionsType.none: return Directory.Exists(Absolute) ? null : new FileInfo(Absolute.ExtTrim());
                  case OptionsType.dir: return null;
                  case OptionsType.file: return new FileInfo(Absolute.ExtTrim());
                  default: throw new Crash();
               }
            }
         }
      }

      public bool FileExists => FileInfo != null && FileInfo.Exists;

      public bool DirExists => DirInfo != null && DirInfo.Exists;

      public bool IsBaseDirDefined => !BaseDir.IsBlank();

      public bool IsAbsoluteDefined => !Absolute.IsBlank();

      public string? RelativePathWindows
      {
         get => IsBaseDirDefined && IsAbsoluteDefined ? GetRelativeWindows(BaseDir.ExtTrim(), Absolute.ExtTrim()) : null;

         set => mySetRelative(value);
      }

      private void mySetRelative(string? value)
      {
         if (value == null) { Absolute = null; }
         else if (IsBaseDirDefined) { Absolute = Path.GetFullPath(Path.Combine(BaseDir.ExtTrim(), value)); }
         else { myRelativeTemp = value; }
      }

      public string RelativePathLinux
      {
         get => GetRelativeLinux(BaseDir.ExtTrim(), Absolute.ExtTrim());

         set => mySetRelative(value);
      }

      /// <summary>
      /// File name if <see cref="Absolute"/> is valid otherwise null.
      /// </summary>
      public string? Name
      {
         get => Absolute.IsBlank() ? null : Path.GetFileName(Absolute);

         set
         {
            if (Absolute == null)
            {
               throw new Gate.Tools.ToolsException("Absolute path is not set!");
            }
            else
            {
               Absolute = !value.IsBlank() ?
                  Path.Combine(Directory.GetParent(Absolute.ExtTrim())?.FullName ?? "", value.ExtTrim()) :
                  throw new Gate.Tools.ToolsException("Empty file name not valid!");
            }
         }
      }

      public string? Extension => Path.GetExtension(Absolute);

      public string? NameNoExt => !Absolute.IsBlank() ? Path.GetFileNameWithoutExtension(Absolute) : null;

      public override string ToString() => RelativePathWindows ?? $"baseDir = {BaseDir} Abs={myAbsolute} tmp_rel ={myRelativeTemp}";

      public override bool Equals(object? obj) => obj is RelativePath pth && (pth.Absolute?.Equals(Absolute) ?? false);

      public override int GetHashCode() => Absolute?.GetHashCode() ?? int.MinValue;

      public static string GetRelativeLinux(string baseDir, string dir) => GetRelativeWindows(baseDir, dir).Replace('\\', '/');

      public static string GetRelativeWindows(string baseDir, string dir)
      {
         var com_prt = GetCommonPart(baseDir, dir);
         var com_prt_pts = myGetPts(com_prt);
         var bas_dir_pts = myGetPts(baseDir);
         var dir_pts = myGetPts(dir);
         var res = null as string;

         if (com_prt_pts.Length < bas_dir_pts.Length)
         {
            var d = bas_dir_pts.Length - com_prt_pts.Length;

            res = $"{myGetDelta(d)}\\{string.Join("\\", dir_pts.Skip(com_prt_pts.Length))}";
         }
         else { res = $"{string.Join("\\", dir_pts.Skip(com_prt_pts.Length))}"; }

         return res == "" ? "." : res;
      }

      private static string myGetDelta(int num)
      {
         var arr = Enumerable.Range(0, num).Select(i => "..").ToArray();

         return string.Join("\\", arr);
      }

      private void myFlagsCheck(string? absPath)
      {
         if (!absPath.IsBlank())
         {
            switch (Options)
            {
               case OptionsType.none: break;

               case OptionsType.dir:
                  if (File.Exists(absPath)) { throw new Gate.Tools.ToolsException($"{absPath} is a file and not a directory"); }
                  break;

               case OptionsType.file:
                  if (Directory.Exists(absPath)) { throw new Gate.Tools.ToolsException($"{absPath} is a directory and not a file"); }
                  break;

               default: throw new Crash();
            }
         }
      }

      private static string[] myGetPts(string? path)
      {
         var roo = Path.GetPathRoot(path);

         return path?.Substring(roo?.Length ?? 0).Split('\\').Where(p => p != "").ToArray() ?? [];
      }

      private static string GetCommonPart(string? path1, string? path2)
      {
         var roo_1 = Path.GetPathRoot(path1);
         var roo_2 = Path.GetPathRoot(path2);

         if (roo_1?.ToLower() == roo_2?.ToLower())
         {
            var pts_1 = myGetPts(path1);
            var pts_2 = myGetPts(path2);

            int i = 0;

            for (; i < Math.Min(pts_1.Length, pts_2.Length); i++)
            {
               if (pts_1[i].ToLower() != pts_2[i].ToLower()) { break; }
            }

            return $"{roo_1}{string.Join("\\", pts_1.Take(i))}";
         }
         else { throw new Exception(); }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="relativeWindows"></param>
      /// <param name="directory"></param>
      /// <returns></returns>
      public static RelativePath FromRelativeWindows(
         string relativeWindows, DirectoryInfo directory) => new RelativePath(directory, Path.Combine(directory.FullName, relativeWindows));

      /// <summary>
      /// 
      /// </summary>
      /// <param name="relativeLinux"></param>
      /// <param name="directory"></param>
      /// <returns></returns>
      public static RelativePath FromRelativeLinux(string relativeLinux, DirectoryInfo directory) => FromRelativeWindows(relativeLinux.Replace('/', '\\'), directory);
   }
}
