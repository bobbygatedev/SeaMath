using Gate.CLanguage;
using Gate.LangBase.Runtime;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.SeaMath.Console;
using Gate.Tools;
using Gate.Tools.Extensions;
using static Gate.SeaMath.FileSystem.SeaFileSystemItem;

namespace Gate.SeaMath.FileSystem
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaFileSystem : HierarchicalItem
   {
      public const string STDIN_PATH = "CONIN$";
      public const string STDOUT_PATH = "CONOUT$";
      public const string STDERR_PATH = "CONERR$";

      public SeaFileSystem()
      {

      }

      private class InnerConcreteSpecialStream : SeaFileSystemItem
      {
         public InnerConcreteSpecialStream(
            SeaMathProcess process, InOutErrType inOutErr, Stream stream, CompiledStdio compiledStdio, int? fileId = null) : base(
               FileMode.CreateNew, myGetAccess(inOutErr), process, compiledStdio)
         {
            InOutErr = inOutErr;
            Stream = stream;
            AddAlias(fileId ?? process.GetStandardSpecialId(InOutErr));
         }

         private static FileAccess myGetAccess(InOutErrType inOutErr)
         {
            switch (inOutErr)
            {
               case InOutErrType.StdIn: return FileAccess.Read;
               case InOutErrType.StdOut: return FileAccess.Write;
               case InOutErrType.StdErr: return FileAccess.Write;

               case InOutErrType.None:
               default:
                  throw new Crash();
            }
         }

         public override InOutErrType InOutErr { get; }

         public override FileInfo? FileInfo => null;

         public override Stream Stream { get; }

         public override void PerformCloseOperation()
         {
            lock (FileSystem ?? throw new Crash())
            {
               FileSystem.myRemoveSubItem(this);
            }
         }

         public override bool IsEndOfStream =>
            throw new Gate.LangBase.Runtime.RtmException("Cannot determine end of stream for special streams!");
      }

      private class InnerConcreteFileInstance : SeaFileSystemItem
      {
         public InnerConcreteFileInstance(
            string path,
            FileMode mode,
            FileAccess access,
            RtmDbgEngVirtCpuProcess? processBound,
            int? fileId,
            CompiledStdio compiledStdio) :
            base(mode, access, processBound, compiledStdio)
         {
            FileInfo = new FileInfo(path);
            Stream = FileInfo.Open(mode, access);
            AddAlias(fileId);
         }

         public override InOutErrType InOutErr => InOutErrType.None;

         public override FileInfo? FileInfo { get; }

         public override Stream Stream { get; }

         public override bool IsEndOfStream => Stream.Position >= Stream.Length;

         public override void PerformCloseOperation()
         {
            lock (FileSystem ?? throw new Crash())
            {
               if (FileSystem.myRemoveSubItem(this))
               {
                  Stream.Close();
                  Stream.Dispose();
               }
            }
         }
      }

      public SeaMathDbgIde? DbgIde => ParentItem as SeaMathDbgIde;

      public SeaFileSystemItem[] AllFiles => SubItems.OfType<SeaFileSystemItem>().ToArray();

      public SeaFileSystemItem CreateFile(
         string path, string fileMode, CompiledStdio compiledStdio, int? fileId = null, RtmDbgEngVirtCpuProcess? processBound = null)
      {
         //is binary is disregarded
         var mod = myParseFileMode(fileMode);

         return CreateFile(path, mod.mode, mod.access, compiledStdio, fileId, processBound);
      }

      public SeaFileSystemItem CreateFile(
         string path, FileMode mode, FileAccess access, CompiledStdio compiledStdio, int? fileId = null, RtmDbgEngVirtCpuProcess? processBound = null)
      {
         try
         {
            return myRegisterFsItem(new InnerConcreteFileInstance(path, mode, access, processBound, fileId, compiledStdio));
         }
         catch (Exception exc)
         {
            throw new Gate.LangBase.Runtime.RtmException(
               $"Cannot open file '{path}' with mode '{mode}' and access '{access}': {exc.Message}");
         }
      }

      public SeaFileSystemItem ReOpenFile(string path, string fileMode, int fileId, RtmDbgEngVirtCpuProcess? process = null)
      {
         var std = DbgIde?.CompiledStdio;

         if (std != null)
         {
            CloseFile(fileId);

            var fil =
               myReCreateSpecialStream(path, fileMode, fileId) ??
               CreateFile(path, fileMode, std, fileId, process);

            return fil;
         }
         else
         {
            throw new RtmException("Not a compiled stdio");
         }
      }

      /// <summary>
      /// Recreates a special stream if applicable 
      /// if a special stream path <see cref="STDIN_PATH"/> <see cref="STDOUT_PATH"/> <see cref="STDERR_PATH"/> is given.
      /// </summary>
      /// <param name="file"></param>
      /// <param name="fileMode"></param>
      /// <returns></returns>
      private SeaFileSystemItem? myReCreateSpecialStream(string file, string fileMode, int fileId)
      {
         var pro = RtmDbgEngVirtCpuThread.GetRunningThread()?.Process as SeaMathProcess ?? throw new Crash();

         file = file.ExtTrim().ToUpper();

         var sio = DbgIde?.CompiledStdio;

         if (sio != null)
         {
            switch (file)
            {
               case STDIN_PATH: return CreateSpecialStream(pro.StdIn, InOutErrType.StdIn, pro, sio, fileId);
               case STDOUT_PATH: return CreateSpecialStream(pro.StdOut, InOutErrType.StdOut, pro, sio, fileId);
               case STDERR_PATH: return CreateSpecialStream(pro.StdErr, InOutErrType.StdErr, pro, sio, fileId);
               default: return null;
            }
         }

         return null;
      }

      private SeaFileSystemItem myRegisterFsItem(SeaFileSystemItem fileSystemItem)
      {
         myAddSubItem(fileSystemItem);

         fileSystemItem.ProcessBound.OnProcessChangeState += (s, ns, os) =>
         {
            if (ns == RtmDbgEngRunState.terminated)
            {
               lock (this)
               {
                  var its = AllFiles.Where(f => f.ProcessBound == fileSystemItem.ProcessBound).ToArray();

                  foreach (var it in its)
                  {
                     it.PerformCloseOperation();
                  }
               }
            }
         };

         return fileSystemItem;
      }

      public SeaFileSystemItem CreateSpecialStream(
         Stream stream, InOutErrType inOutErr, SeaMathProcess processBound, CompiledStdio compiledStdio, int? fileId = null) =>
            myRegisterFsItem(new InnerConcreteSpecialStream(processBound, inOutErr, stream, compiledStdio));

      public int GetStdStreamIdByType(InOutErrType inOutErr) =>
         myGetCurrentProcess().GetStandardSpecialId(inOutErr) ?? throw new Crash();

      private SeaMathProcess myGetCurrentProcess() =>
         RtmDbgEngVirtCpuThread.GetRunningThread()?.Process as SeaMathProcess ?? throw new Crash();

      public SeaFileSystemItem? GetStreamByType(InOutErrType inOutErr)
      {
         var pro = myGetCurrentProcess();

         return AllFiles.FirstOrDefault(f => f.ProcessBound == pro && f.Aliases.Any(ali => ali == pro.GetStandardSpecialId(inOutErr)));
      }

      public unsafe SeaFileSystemItem? GetFile(int file, RtmDbgEngVirtCpuProcess? process = null) =>
         DbgIde?.FileSystem.AllFiles.FirstOrDefault(
            f => f.Aliases.Any(a => a == file) && f.ProcessBound == (process ?? RtmDbgEngVirtCpuThread.GetRunningThread()?.Process));

      public unsafe SeaFileSystemItem? GetFile(void* file, RtmDbgEngVirtCpuProcess? process = null) =>
         GetFile((int)file, process);

      public unsafe void CloseFile(void* file) => CloseFile((int)file);

      public unsafe void CloseFile(int fileId)
      {
         var fil = GetFile(fileId);

         if (fil != null)
         {
            lock (fil)
            {
               fil.RemoveAlias(fileId);
            }
         }
         else
         {
            throw new Gate.LangBase.Runtime.RtmException($"File with id {fileId} not found", RtmErrno.EBADF);
         }
      }

      private static (FileAccess access, FileMode mode) myParseFileMode(string cFileMode)
      {
         if (cFileMode.IsBlank())
         {
            throw new Gate.LangBase.Runtime.RtmException("File mode string is blank");
         }
         else
         {
            var isr = false;
            var isw = false;
            var isa = false;
            var is_plu = false;

            foreach (var c in cFileMode)
            {
               switch (c)
               {
                  case 'r': isr = true; break;
                  case 'w': isw = true; break;
                  case 'a': isa = true; break;
                  case '+': is_plu = true; break;
                  case 'b': break;//binary is disregarded
                  default: throw new ArgumentException($"Invalid fopen mode character: '{c}'");
               }
            }

            var pri_cnt = (isr ? 1 : 0) + (isw ? 1 : 0) + (isa ? 1 : 0);

            if (pri_cnt != 1)
            {
               throw new Gate.LangBase.Runtime.RtmException("Invalid fopen mode: must contain exactly one of r, w, or a");
            }
            else
            {
               FileMode mode;
               FileAccess access;

               if (isr)
               {
                  mode = FileMode.Open;
                  access = is_plu ? FileAccess.ReadWrite : FileAccess.Read;
               }
               else if (isw)
               {
                  mode = FileMode.Create;
                  access = is_plu ? FileAccess.ReadWrite : FileAccess.Write;
               }
               else // hasA
               {
                  mode = FileMode.Append;
                  access = is_plu ? FileAccess.ReadWrite : FileAccess.Write;
               }

               return (access, mode);
            }
         }
      }
   }
}

