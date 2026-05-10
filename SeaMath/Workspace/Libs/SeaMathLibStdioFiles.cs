using Gate.CLanguage;
using Gate.LangBase.Runtime;
using Gate.SeaMath.Console;
using Gate.Tools;
using System.Runtime.InteropServices;
using System.Text;
using static Gate.SeaMath.FileSystem.SeaFileSystemItem;

namespace Gate.SeaMath.Workspace.Libs
{
   /// <summary>
   /// Part of SeaMathLib that implements stdio file functions
   /// </summary>
   public unsafe class SeaMathLibStdioFiles : SeaMathLibCSharp
   {
      public const string NAME = "StdioFiles";

      public SeaMathLibStdioFiles(SeaMathDbgIde dbgIde) : base(NAME, dbgIde) => Console = dbgIde?.Console ?? throw new Crash();

      public SeaMathConsole Console { get; }

      [Method(Name = "fopen")]
      public void* DoFopen(sbyte* path, sbyte* mode)
      {
         return (void*)myHandleException(() =>
         {
            var pth = ((IntPtr)path).GetStringNarrowNt(RtmStrategy.Settings.NarrowCharEncoding);
            var mod = ((IntPtr)mode).GetStringNarrowNt(RtmStrategy.Settings.NarrowCharEncoding);

            var fil = DbgIde.FileSystem.CreateFile(pth, mod);

            return fil.Aliases.Last();
         });
      }

      [Method(Name = "fclose")]
      public void DoFClose(void* file) => myHandleException(() => DbgIde.FileSystem.CloseFile(file));

      [Method(Name = "freopen")]
      public void* DoFreopen(sbyte* path, sbyte* mode, void* stream)
      {
         return (void*)myHandleException(() =>
         {
            var pth = ((IntPtr)path).GetStringNarrowNt(RtmStrategy.Settings.NarrowCharEncoding);
            var mod = ((IntPtr)mode).GetStringNarrowNt(RtmStrategy.Settings.NarrowCharEncoding);

            var fil = DbgIde.FileSystem.ReOpenFile(pth, mod, (int)stream);

            return fil.Aliases.Last();
         });
      }

      [Method(Name = "fileno")]
      public int DoFileNo(void* file)
      {
         return myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetFile(file);

            return fil != null ? (int)file : -1;
         }, -1);
      }

      [Method(Name = "dup")]
      public int DoDup(int oldFd)
      {
         return myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetFile(oldFd);

            return fil != null ? fil.AddAlias() : -1;
         }, -1);
      }

      [Method(Name = "dup2")]
      public int DoDup2(int oldFd, int newFd)
      {
         return myHandleException(() =>
         {
            var old_fil = DbgIde.FileSystem.GetFile(oldFd);
            var new_fil = DbgIde.FileSystem.GetFile(newFd);

            new_fil?.RemoveAlias(newFd);

            return old_fil != null ? old_fil.AddAlias(newFd) : -1;
         }, -1);
      }


      [Method(Name = "fscanf")]
      public int DoFScanf(void* file, sbyte* format, params object[] @params)
      {
         return myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetFile(file);

            return fil == null ? -1 : fil.Scanf((IntPtr)format, RtmStrategy, @params);
         }, -1);
      }

      [Method(Name = "fgetc")]
      public int DoFGetc(void* file)
      {
         return myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetFile(file);

            return fil?.Getc() ?? -1;
         }, -1);
      }

      [Method(Name = "fread")]
      public uint DoFRead(void* ptr, uint size, uint nmemb, void* file)
      {
         return myHandleException<uint>(() =>
         {
            var fil = DbgIde.FileSystem.GetFile(file) ?? throw new RtmException(RtmErrno.EMFILE);
            var nb = size * nmemb;
            var buf = new byte[nb];

            //effective readbytes
            var nbe = Math.Max(0, fil.Stream.Read(buf, 0, buf.Length));

            if (nbe > 0)
            {
               Marshal.Copy(buf, 0, (IntPtr)ptr, nbe);
            }

            return (uint)nbe;
         }, 0);
      }

      [Method(Name = "fwrite")]
      public uint DoFWrite(void* ptr, uint size, uint nmemb, void* file)
      {
         return (uint)myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetFile(file) ?? throw new RtmException(RtmErrno.EMFILE);
            var nb = size * nmemb;
            var buf = new byte[nb];

            Marshal.Copy((IntPtr)ptr, buf, 0, (int)nb);

            fil.Stream.Write(buf, 0, buf.Length);

            return (int)nb;
         });
      }

      [Method(Name = "rewind")]
      public void DoRewind(void* file)
      {
         myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetFile(file) ?? throw new RtmException(RtmErrno.EMFILE);

            fil.Stream.Position = 0;

            return 0;
         });
      }

      [Method(Name = "fflush")]
      public int DoFflush(void* file)
      {
         return myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetFile(file) ?? throw new RtmException(RtmErrno.EMFILE);

            if (fil != null)
            {
               fil.Stream.Flush();

               return 0;
            }
            else
            {
               return -1;
            }
         });
      }

      [Method(Name = "fseek")]
      public int DoFSeek(void* file, long offset, int whence)
      {
         return myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetFile(file) ?? throw new RtmException(RtmErrno.EMFILE);

            return (int)fil.DoFSeek(offset, (FSeekWhence)whence);
         });
      }

      [Method(Name = "ftell")]
      public long DoFTell(void* file)
      {
         return myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetFile(file) ?? throw new RtmException(RtmErrno.EMFILE);

            return (int)fil.Stream.Position;
         });
      }

      /// <summary>
      /// Compatibility eqaul to ftell
      /// </summary>
      /// <param name="file"></param>
      /// <param name="pos"></param>
      /// <returns></returns>
      [Method(Name = "fgetpos")]
      public int DoFGetpos(void* file, Int64* pos)
      {
         return myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetFile(file) ?? throw new RtmException(RtmErrno.EMFILE);

            *pos = fil.Stream.Position;

            return 0;
         });
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="file"></param>
      /// <param name="pos"></param>
      /// <returns></returns>
      [Method(Name = "fsetpos")]
      public int DoFsetpos(void* file, Int64* pos)
      {
         return myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetFile(file) ?? throw new RtmException(RtmErrno.EMFILE);

            fil.Stream.Position = *pos;

            return 0;
         });
      }

      [Method(Name = "fgetwc")]
      public int DoFgetwc(void* file)
      {
         var fil = DbgIde.FileSystem.GetFile(file);

         return fil?.GetcW(RtmStrategy) ?? -1;
      }

      [Method(Name = "fputc")]
      public int DoFputc(int c, void* file)
      {
         var fil = DbgIde.FileSystem.GetFile(file);

         if (fil != null)
         {
            fil.Stream.WriteByte((byte)c);

            return 1;
         }
         else
         {
            return 0;
         }
      }


      [Method(Name = "fputwc")]
      public unsafe int DoFputwc(int ch, void* file)
      {
         return myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetFile(file) ?? throw new RtmException(RtmErrno.EMFILE);
            var nb =
               (DbgIde?.OptionPage?.WideCharEncoding ?? throw new Crash()).CodePage == Encoding.UTF32.CodePage ? 4 : 2;
            int c = ch;
            var p = (byte*)&c;
            var str = new byte[nb];

            for (int i = 0; i < nb; i++)
            {
               str[i] = p[i];
            }

            fil.Stream.Write(str, 0, str.Length);

            return 1;
         });
      }

      [Method(Name = "fprintf")]
      public int DoFprintf(void* file, int* format, params object[] @params)
      {
         return myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetFile(file);

            return fil == null ? -1 : fil.Printf(RtmStrategy, (sbyte*)format, @params);
         });
      }

      [Method(Name = "fwprintf")]
      public int DoFwprintf(void* file, int* format, params object[] @params)
      {
         return myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetFile(file);

            return fil != null ?
               fil.PrintfW(RtmStrategy, (sbyte*)format, @params) :
               -1;
         });
      }

      [Method(Name = "fgets")]
      public sbyte* DoFgets(sbyte* buffer, int n, void* file)
      {
         return (sbyte*)myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetFile(file);

            return fil != null ? (IntPtr)fil.Gets(buffer) : IntPtr.Zero;
         }, IntPtr.Zero);
      }

      [Method(Name = "fputs")]
      public int DoFputs(sbyte* @string, void* file)
      {
         return myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetFile(file);

            return fil != null ? fil.Puts(@string, RtmStrategy.Settings.NarrowCharEncoding) : -1;
         }, -1);
      }

      [Method(Name = "fgetws")]
      public void* DoFgetws(void* buffer, int n, void* file)
      {
         return (sbyte*)myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetFile(file);

            return fil != null ? (IntPtr)fil.WGets(buffer, RtmStrategy) : IntPtr.Zero;
         }, IntPtr.Zero);
      }

      [Method(Name = "fputws")]
      public int DoFputws(int* @string, void* file)
      {
         return myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetFile(file);

            return fil != null ? fil.PutsWide(RtmStrategy, @string) : -1;
         }, -1);
      }

      /// <summary>
      /// todo not implemented do nothing in our implementation
      /// </summary>
      /// <param name="file"></param>
      [Method(Name = "clearerr")]
      public void DoClearerr(void* file)
      {

      }

      [Method(Name = "feof")]
      public int DoFeof(void* file) => myHandleException(() =>
         (DbgIde?.FileSystem?.GetFile(file) ?? throw new RtmException(RtmErrno.EMFILE)).IsEndOfStream ? 1 : 0);

      /// <summary>
      /// todo not implemented always return no error
      /// </summary>
      /// <param name="file"></param>
      /// <returns></returns>
      [Method(Name = "ferror")]
      public int DoFerror(void* file) => 0;
   }
}
