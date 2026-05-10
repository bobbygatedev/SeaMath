using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Gate.Tools.Extensions
{
   /// <summary>
   /// 
   /// </summary>
   public static class IoExtender
   {
      [DllImport("kernel32.dll")]
      static extern bool GetBinaryType(string lpApplicationName, out BinaryType lpBinaryType);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="dirInfo"></param>
      /// <param name="searchOption"></param>
      /// <param name="patterns"></param>
      /// <returns></returns>
      public static FileInfo[] EnumerateFilesForMultiplePattern(
         this DirectoryInfo dirInfo, SearchOption searchOption, params string[] patterns) =>
         patterns.SelectMany(p => dirInfo.EnumerateFiles(p, searchOption)).ToArray();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="fsInfo"></param>
      /// <returns></returns>
      public static string GetFileNameWithoutExtension(this FileSystemInfo fsInfo) => Path.GetFileNameWithoutExtension(fsInfo.Name);

      /// <summary>
      /// File with exact case eg c:\temp\myfile.txt -> c:\temp\MyFile.txt)
      /// </summary>
      /// <param name="file"></param>
      /// <returns></returns>
      public static FileInfo GetFullPathCase(this FileInfo file)
      {
         if (file.Exists)
         {
            return file.
               Directory?.
               GetFullPathCase().
               EnumerateFiles().
               FirstOrDefault(f => f.FullName.IsEqualNoContent(file.FullName)) ?? throw new Crash();
         }
         else
         {
            return new FileInfo(Path.Combine((file.Directory ?? throw new Crash()).GetFullPathCase().FullName, file.Name));
         }
      }

      /// <summary>
      /// Directory with exact case eg c:\tEmP -> c:\temp)
      /// </summary>
      /// <param name="dir"></param>
      /// <returns></returns>
      public static DirectoryInfo GetFullPathCase(this DirectoryInfo dir)
      {
         if (dir.Exists)
         {
            var tmp = new DirectoryInfo(dir.Root.FullName.ToUpper());
            var prs = dir.FullName.Split('\\').Skip(1).Where(d => !d.IsBlank()).ToArray();

            foreach (var par in prs)
            {
               tmp = tmp.EnumerateDirectories().FirstOrDefault(d => d.Name.IsEqualNoContent(par)) ?? throw new Crash();
            }
            return tmp;
         }

         return dir;
      }

      public static FileInfo GetCombinedToFile(this FileSystemInfo info, string path) => new FileInfo(Path.Combine(info.FullName, path));

      public static DirectoryInfo GetCombinedToDir(this FileSystemInfo info, string path) => new DirectoryInfo(Path.Combine(info.FullName, path));

      public static bool IsEqual<T>(this T? info1, T? info2) where T : FileSystemInfo
      {
         if (info1 == null && info2 == null) { return true; }
         else if (info1 != null && info2 != null) { return info1.FullName.IsEqualNoContent(info2.FullName); }
         else { return false; }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="info1"></param>
      /// <param name="info2"></param>
      /// <returns></returns>
      public static bool IsContainedStrict(this DirectoryInfo info1, DirectoryInfo info2) => 
         info1.FullName.StartsWithNoContent(myAppendBackSlash(info2));

      /// <summary>
      /// 
      /// </summary>
      /// <param name="info1"></param>
      /// <param name="info2"></param>
      /// <returns></returns>
      public static bool IsContainedIn(this DirectoryInfo info1, DirectoryInfo info2) =>
         info1.IsContainedStrict(info2) || info1.FullName.IsEqualNoContent(info2.FullName);

      public static DirectoryInfo[] AllSubDirectoriesAndMe(this DirectoryInfo directoryInfo) =>
         directoryInfo.Exists ?
            directoryInfo.EnumerateDirectories("*", SearchOption.AllDirectories).
            Append(directoryInfo).ToArray() :
            ([]);

      /// <summary>
      /// Directory currently exist ( <see cref="DirectoryInfo.Exists"/> is not updated after constructor.
      /// </summary>
      /// <param name="directory"></param>
      /// <returns></returns>
      public static bool IsExisting(this DirectoryInfo directory) => Directory.Exists(directory.FullName);

      /// <summary>
      /// File currently exist ( <see cref="FileInfo.Exists"/> is not updated after constructor.
      /// </summary>
      /// <param name="file"></param>
      /// <returns></returns>
      public static bool IsExisting(this FileInfo file) => File.Exists(file.FullName);

      /// <summary>
      /// Number of item in a path (eg c:\temp => 1)
      /// </summary>
      /// <param name="fsItem"></param>
      /// <returns></returns>
      public static int GetDepth(this FileSystemInfo fsItem) => fsItem.FullName.Split('\\').Length - 1;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="fileInfo"></param>
      /// <returns></returns>
      public static DateTime GetLastAccessTime(this FileInfo fileInfo) => File.GetLastAccessTime(fileInfo.FullName);

      /// <summary>
      /// True last write time (<see cref="FileSystemInfo.LastWriteTime"/> is not updated after constructor).
      /// </summary>
      /// <param name="fileInfo"></param>
      /// <returns></returns>
      public static DateTime GetLastWriteTime(this FileInfo fileInfo) => File.GetLastWriteTime(fileInfo.FullName);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="dirInfo"></param>
      /// <returns></returns>
      public static DateTime GetLastAccessTime(this DirectoryInfo dirInfo) => Directory.GetLastAccessTime(dirInfo.FullName);

      /// <summary>
      /// True last write time (<see cref="FileSystemInfo.LastWriteTime"/> is not updated after constructor).
      /// </summary>
      /// <param name="dirInfo"></param>
      /// <returns></returns>
      public static DateTime GetLastWriteTime(this DirectoryInfo dirInfo) => Directory.GetLastWriteTime(dirInfo.FullName);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="fileInfo"></param>
      /// <returns></returns>
      public static BinaryType? GetBinaryType(this FileInfo fileInfo) => GetBinaryType(fileInfo.FullName, out var bt) ? bt : (BinaryType?)null;

      // Importa la funzione ReadFile dalle WinAPI
      [DllImport("kernel32.dll", SetLastError = true)]
      private static extern bool ReadFile(
          IntPtr hFile,
          IntPtr lpBuffer,
          uint nNumberOfBytesToRead,
          out uint lpNumberOfBytesRead,
          IntPtr lpOverlapped);


      /// <summary>
      /// 
      /// </summary>
      /// <param name="fileStream"></param>
      /// <param name="pointer"></param>
      /// <param name="len"></param>
      /// <returns></returns>
      /// <exception cref="System.IO.IOException"></exception>
      public static uint ReadFile(this FileStream fileStream, IntPtr pointer, int len) =>
         ReadFile(fileStream.SafeFileHandle.DangerousGetHandle(), pointer, (uint)len, out var len_rb, IntPtr.Zero) ?
            len_rb : throw new System.IO.IOException();


      // Importa la funzione WriteFile dalle WinAPI
      [DllImport("kernel32.dll", SetLastError = true)]
      private static extern bool WriteFile(
          IntPtr hFile,
          IntPtr lpBuffer,
          uint nNumberOfBytesToWrite,
          out uint lpNumberOfBytesWritten,
          IntPtr lpOverlapped);

      /// <summary>
      /// Metodo di estensione per FileStream che utilizza WriteFile
      /// </summary>
      /// <param name="fileStream"></param>
      /// <param name="pointer"></param>
      /// <param name="len"></param>
      /// <returns></returns>
      /// <exception cref="System.IO.IOException"></exception>
      public static uint WriteFile(this FileStream fileStream, IntPtr pointer, int len)
      {
         if (WriteFile(fileStream.SafeFileHandle.DangerousGetHandle(), pointer, (uint)len, out var len_wb, IntPtr.Zero))
         {
            return len_wb;
         }
         else
         {
            throw new System.IO.IOException();
         }
      }

      public static StreamWriter OpenWriter(this FileInfo fileInfo) => new StreamWriter(fileInfo.OpenWrite());

      public static void IoActionTimeout(this Action ioAction, double timeout)
      {
         var sw = new Stopwatch();

         sw.Start();

         while (true)
         {
            try
            {
               ioAction();

               return;
            }
            catch (IOException)
            {
               if (sw.Elapsed.TotalSeconds > 3.0) { throw; }
            }
            catch (Exception exc) { throw new Crash(exc); }
         }
      }

      public static DirectoryInfo GetCommon(this DirectoryInfo dir1, DirectoryInfo dir2)
      {
         if (dir1.Root.FullName.IsEqualNoContent(dir2.Root.FullName))
         {
            var s1 = dir1.FullName.Split('\\');
            var s2 = dir2.FullName.Split('\\');

            var len = Math.Min(s1.Length, s2.Length);
            var oup = dir1.Root;

            for (int i = 1; i < len; i++)
            {
               if (s1[i].IsEqualNoContent(s2[i]))
               {
                  oup = oup.GetCombinedToDir(s1[i]);
               }
               else
               {
                  return oup;
               }
            }

            return dir1;
         }
         else
         {
            throw new Gate.Tools.ToolsException($"Not same root");
         }
      }

      private static string myAppendBackSlash(DirectoryInfo info) => info.FullName.EndsWith("\\") ? info.FullName : $"{info.FullName}\\";
   }
}

