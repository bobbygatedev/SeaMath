using Gate.Tools.Extensions;

namespace Gate.Tools.AppParams
{
   public abstract partial class AppParam
   {
      /// <summary>
      /// Relative file or directory (to <see cref="AppParamContainer.FilePath"/> directory).
      /// </summary>
      public class RelativeType : Scalar
      {
         private RelativePath? myRelPath;

         /// <summary>
         /// 
         /// </summary>
         public RelativeType() : base(null, null, false) => myInit(RelativePath.OptionsType.none);

         /// <summary>
         /// 
         /// </summary>
         /// <param name="pathOptions"></param>
         /// <param name="defaultValue"></param>
         /// <param name="name"></param>
         /// <param name="caption"></param>
         /// <param name="isRequired"></param>
         public RelativeType(
            RelativePath.OptionsType pathOptions,
            string? defaultValue = null,
            string? name = null,
            string? caption = null,
            bool isRequired = false) :
            base(name, caption, isRequired)
         {
            DefaultValue = defaultValue;
            myInit(pathOptions);
         }

         protected override object? myActionOnGet() => RelPath?.RelativePathWindows;

         protected override void myActionOnSet(object? newValue) => RelPath.RelativePathWindows = newValue as string;

         public override Type ParamType => typeof(string);

         /// <summary>
         /// Relative path handler instance.
         /// </summary>
         public RelativePath RelPath
         {
            get
            {
               //update base dir 
               (myRelPath?? throw new Gate.Tools.ToolsException()).BaseDir = ParamContainer?.FileInfo?.DirectoryName;

               return myRelPath;
            }
         }

         /// <summary>
         /// Gets the <see cref="FileInfo"/> object representing the file associated with the relative path.
         /// </summary>
         public FileInfo? FileInfo => RelPath?.FileInfo;

         /// <summary>
         /// Gets the <see cref="DirectoryInfo"/> object representing the directory associated with the relative path.
         /// </summary>
         public DirectoryInfo? DirInfo => RelPath?.DirInfo;

         protected override void myActionOnContainerBeforeSave(AppParamContainer paramContainer, string? path)
         {
            if (path != null && !path.IsBlank())
            {
               var tmp = new FileInfo(path).DirectoryName;

               (myRelPath ?? throw new Gate.Tools.ToolsException()).BaseDir = tmp;
            }

            base.myActionOnContainerBeforeSave(paramContainer, path);
         }


         protected override void myActionOnContainerLoad(AppParamContainer paramContainer, string? path)
         {
            if (path != null && !path.IsBlank())
            {
               (myRelPath ?? throw new Crash()).BaseDir = new FileInfo(path).DirectoryName;
            }

            base.myActionOnContainerLoad(paramContainer, path);
         }

         /// <summary>
         /// 
         /// </summary>
         public string? DefaultValue { get; }

         public override void Clear() => RelPath.Absolute = RelPath.IsBaseDirDefined ? DefaultValue : null;

         public override void CopyTo(AppParam other)
         {
            if (other.GetType() == GetType())
            {
               var oth = other as RelativeType ?? throw new Crash();

               oth.myRelPath = new RelativePath(RelPath.Options, oth.RelPath.BaseDirInfo, RelPath.Absolute);
            }
            else
            {
               throw new Gate.Tools.ToolsException($"Can't copy from {GetType().Name} to {other.GetType().Name}");
            }
         }

         public override string Descriptor => $"{ParamName}={RelPath}";

         private void myInit(RelativePath.OptionsType pathOptions)
         {
            myRelPath = new RelativePath(pathOptions);
            RelPath.OnChangeAbsolute += RelPath_OnChangeAbsolute;
         }

         private void RelPath_OnChangeAbsolute(RelativePath path) => myActionOnAnyChange(this);

         public override int HashCode => (RelPath.Absolute ?? "").Trim().ToLower().GetHashCode();

         public override bool Compare(AppParam other)
         {
            if (other is RelativeType rel && rel.GetType() == GetType())
            {
               var abs = (RelPath.Absolute ?? "").Trim().ToLower();
               var abs_oth = (rel.RelPath.Absolute ?? "").Trim().ToLower();

               return abs_oth == abs;
            }
            else { return false; }
         }

         public override string ToString() => $"Relative {RelPath}";
      }
   }
}
