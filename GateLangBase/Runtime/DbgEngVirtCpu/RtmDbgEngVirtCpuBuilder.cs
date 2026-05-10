using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class RtmDbgEngVirtCpuBuilder
   {
      private List<IRtmDbgEngVirtCpuPseudoSource> myListPseudoSource = new List<IRtmDbgEngVirtCpuPseudoSource>();
      private FileInfo[]? mySourceFiles;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="dbgIde"></param>
      protected RtmDbgEngVirtCpuBuilder(IRtmDbgEngIde dbgIde) => DbgIde = dbgIde;

      /// <summary>
      /// 
      /// </summary>
      public abstract IRtmDbgEngVirtCpuCompiler VirtCpuCompiler { get; }


      /// <summary>
      /// 
      /// </summary>
      public abstract IRtmDbgEngVirtCpuLinker VirtCpuLinker { get; }

      /// <summary>
      /// 
      /// </summary>
      public IRtmDbgEngIde DbgIde { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract IRtmObjStrategy? RtmStrategy { get; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsLinkRequired { get; private set; } = true;

      /// <summary>
      /// A build set is dirty when any file is still to recompile.
      /// </summary>
      public bool IsDirty
      {
         get => SourceFileToCompile.Length > 0 || IsLinkRequired;

         set
         {
            if (IsDirty != value)
            {
               if (value)
               {
                  myListPseudoSource.Clear();
                  IsLinkRequired = true;
                  PseudoExe = null;
               }
            }
         }
      }

      /// <summary>
      /// Array of source files.
      /// </summary>
      public FileInfo[] SourceFiles
      {
         get => mySourceFiles ?? [];

         set
         {
            mySourceFiles = value;
            IsDirty = true;
         }
      }

      /// <summary>
      /// Files where compile is required.
      /// </summary>
      public FileInfo[] SourceFileToCompile =>
         SourceFiles.Where(sf => 
            PseudoSources.All(of => !(of.FileInfo?.FullName.IsEqualNoContent(sf.FullName) ?? false))).ToArray();

      /// <summary>
      /// 
      /// </summary>
      public IRtmDbgEngVirtCpuPseudoSource[] PseudoSources => myListPseudoSource.ToArray();

      /// <summary>
      /// 
      /// </summary>
      public IRtmDbgEngVirtCpuPseudoLibrary[]? Libraries { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuPseudoExe? PseudoExe { get; private set; }

      /// <summary>
      /// Check if a file is dirty (its content has changed).
      /// </summary>
      public void UpdateSourceState() => myListPseudoSource = myListPseudoSource.Where(ps => !ps.IsDirty).ToList();

      /// <summary>
      /// Compile all files
      /// </summary>
      /// <param name="isRebuild"></param>
      /// <param name="messages"></param>
      /// <returns></returns>
      public virtual bool Compile(bool isRebuild, MsgCollection messages)
      {
         var res = true;

         UpdateSourceState();

         if (isRebuild)
         {
            IsDirty = true;
         }

         var src_fls_cmp = SourceFileToCompile;

         if (src_fls_cmp.Length > 0)
         {
            PseudoExe = null;
         }
         else
         {
            messages.Add(new Msg(MsgType.info, "No files to compile"));

            return true;
         }

         foreach (var src_fil in src_fls_cmp)
         {
            if (VirtCpuCompiler.Compiler(messages, src_fil, out var src))
            {
               myListPseudoSource.Add(src ?? throw new Crash());
               messages.Add(new Msg(MsgType.success, $"{src.FileInfo?.Name} compiled successfull"));
            }
            else
            {
               messages.Add(new Msg(MsgType.fail, $"{src_fil.Name} compile failed"));
               res = false;
            }
         }

         return res;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="exeName"></param>
      /// <param name="isRebuild"></param>
      /// <param name="messages"></param>
      /// <returns></returns>
      public virtual bool Build(string exeName, bool isRebuild, MsgCollection messages)
      {
         if (IsDirty || isRebuild)
         {
            IsDirty = true;

            return Compile(isRebuild, messages) && Link(exeName, isRebuild, messages);
         }
         else { return true; }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="exeName"></param>
      /// <param name="isRebuild"></param>
      /// <param name="messages"></param>
      /// <returns></returns>
      public virtual bool Link(string exeName, bool isRebuild, MsgCollection messages)
      {
         if (IsDirty && VirtCpuLinker.Link(messages, this))
         {
            PseudoExe = new RtmDbgEngVirtCpuPseudoExe(exeName);
            PseudoExe.AddLibraries(Libraries ?? []);
            PseudoExe.AddSources(PseudoSources);
            IsLinkRequired = false;

            return true;
         }
         else
         {
            return false;
         }
      }
   }
}
