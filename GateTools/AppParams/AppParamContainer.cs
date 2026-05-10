using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text;
using System.Runtime.CompilerServices;

namespace Gate.Tools.AppParams
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class AppParamContainer : HierarchicalItem
   {
      public delegate void OnAppParamChangedHandler(AppParam appParam);

      public delegate void OnContainerHandler(AppParamContainer paramContainer, string? path);
      public delegate void OnPathChangeHandler(AppParamContainer project, string? newPath);

      public event OnPathChangeHandler? OnPathChange;
      public event OnAppParamChangedHandler? OnAppParamChanged;
      public event OnContainerHandler? OnBeforeSave;
      public event OnContainerHandler? OnAfterSave;
      public event OnContainerHandler? OnLoad;

      private readonly Lazy<AppParamLoadSaver> myLazyAppParamLoadSaver;
      private readonly Lazy<TxtStringConverter> myLazyParamStringConverter;
      private readonly Lazy<AppParam.Record> myLazyAppParamRecord;
      private volatile bool myIsAutoLoad;
      private volatile MsgCollection? myIntenalMessages = null;
      private volatile string? myFilePath;
      private static volatile MsgCollection myAppDefaultMessages = new MsgCollection();

      /// <summary>
      /// 
      /// </summary>
      public AppParamContainer(bool isAutoLoad = false)
      {
         myLazyAppParamLoadSaver = new Lazy<AppParamLoadSaver>(myMakeLoadSaver);
         myLazyParamStringConverter = new Lazy<TxtStringConverter>(myMakeStringConverter);
         myLazyAppParamRecord = new Lazy<AppParam.Record>(() =>
         {
            var par_rec = myMakeParamsRecord() ?? throw new Crash();

            if (par_rec?.ParamName.ExtTrim() == "") { throw new Crash($"You shall assign a name to root params record"); }

            myAddSubItem(par_rec);

            (par_rec ?? throw new Crash()).OnAnyChange += (p) => myActionOnAnyParamChanged(p);

            return par_rec;
         });

         OnAppParamChanged += AppParamContainer_OnAppParamChanged;
         IsAutoLoad = isAutoLoad;
      }

      /// <summary>
      /// 
      /// </summary>
      public static MsgCollection AppDefaultMessages
      {
         get => myAppDefaultMessages;

         set => myAppDefaultMessages = value ?? throw new Gate.Tools.ToolsException($"App-default-message-collection can't be null");
      }

      /// <summary>
      /// 
      /// </summary>
      public MsgCollection InternalMessages
      {
         get => myIntenalMessages ?? AppDefaultMessages;

         set => myIntenalMessages = value ?? throw new Gate.Tools.ToolsException($"Internal-message-collection can't be null");
      }

      protected virtual void myActionOnPathChange(string myProjectPath) => OnPathChange?.Invoke(this, myProjectPath);

      /// <summary>
      /// If has value which is not blank (null or Trim() == "").
      /// </summary>
      public abstract string? FixedPath { get; }

      /// <summary>
      /// Sets file path if <see cref="FixedPath"/> is not defined (null or "").
      /// </summary>
      public string? FilePath
      {
         get => FixedPath.IsBlank() ? myFilePath : FixedPath;

         set
         {
            if (!value.IsBlank() && !value.IsEqualNoContent(myFilePath, false))
            {
               myFilePath = System.IO.Path.GetFullPath(value ?? throw new Crash());
               myActionOnPathChange(myFilePath);
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public FileInfo? FileInfo => FilePath.IsBlank() ? null : new FileInfo(FilePath ?? throw new Crash());

      /// <summary>
      /// 
      /// </summary>
      public bool IsAutoLoad
      {
         get => myIsAutoLoad;

         set
         {
            if (myIsAutoLoad = value) { Load(InternalMessages ?? new MsgCollection()); }
         }
      }

      /// <summary>
      /// Returns make params record (factory method)
      /// </summary>
      /// <returns></returns>
      protected abstract AppParam.Record myMakeParamsRecord();

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected abstract TxtStringConverter myMakeStringConverter();

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected abstract AppParamLoadSaver myMakeLoadSaver();

      /// <summary>
      /// 
      /// </summary>
      public virtual AppParamLoadSaver LoadSaver => myLazyAppParamLoadSaver.Value;

      /// <summary>
      /// 
      /// </summary>
      public AppParam.Record Params => myLazyAppParamRecord.Value;

      public AppParam.Record[] BackupParams => SubItems.OfType<AppParam.Record>().Except(new[] { Params }).ToArray();

      public AppParam.Record AddBackupParams()
      {
         var bck = myMakeParamsRecord();

         myAddSubItem(bck);

         Params.CopyTo(bck);

         return bck;
      }

      public void BackupApply(AppParam? appParam)
      {
         appParam?.CopyTo(Params);
         myRemoveSubItem(appParam);
      }

      /// <summary>
      /// 
      /// </summary>
      public TxtStringConverter StringConverter => myLazyParamStringConverter.Value;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="path"></param>
      /// <returns></returns>
      public virtual bool Save(string? path = null) => Save(null, path);

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      [MethodImpl(MethodImplOptions.NoOptimization)]
      public virtual bool Save(MsgCollection? msgs, string? path = null)
      {
         //shall be forced 
         var tmp = Params;

         InternalMessages = msgs ?? InternalMessages;

         var pth = path ?? FilePath;

         myActionBeforeSave(this, pth);

         if (pth == null)
         {
            throw new Gate.Tools.ToolsException($"Not a path defined for {GetType().Name}");
         }

         var res = LoadSaver.Save(this, StringConverter, pth);

         if (FixedPath.IsBlank())
         {
            FilePath = pth;
         }

         if (res)
         {
            myActionAfterSave(this, path);
         }

         IsToBeSaved = false;

         return res;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="path"></param>
      /// <returns></returns>
      public bool Load(string? path = null) => Load(null, path);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="msgs"></param>
      /// <param name="path"></param>
      /// <returns>True if successfully False if any error.</returns>
      public virtual bool Load(MsgCollection? msgs, string? path = null)
      {
         try
         {
            IsLoading = true;
            InternalMessages = msgs ?? InternalMessages;

            var pth = path ?? FilePath;

            if (pth == null)
            {
               throw new Gate.Tools.ToolsException($"Not a path defined for {GetType().Name}");
            }

            var res = LoadSaver.Load(this, StringConverter, pth);

            if (FixedPath.IsBlank())
            {
               FilePath = pth;
            }

            myActionOnLoad(this, pth);

            return res;
         }
         finally
         {
            IsLoading = false;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public bool IsLoading { get; protected set; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsAutoSave { get; set; } = false;

      /// <summary>
      /// 
      /// </summary>
      public bool IsToBeSaved { get; private set; } = false;

      /// <summary>
      /// 
      /// </summary>
      public void ClearContent() => Params.Clear();

      protected virtual void myActionBeforeSave(AppParamContainer paramContainer, string? path) => OnBeforeSave?.Invoke(this, path);

      protected virtual void myActionAfterSave(AppParamContainer paramContainer, string? path) => OnAfterSave?.Invoke(this, path);

      protected virtual void myActionOnLoad(AppParamContainer paramContainer, string? path) => OnLoad?.Invoke(this, path);
      protected virtual void myActionOnAnyParamChanged(AppParam appParam)
      {
         OnAppParamChanged?.Invoke(appParam);
         IsToBeSaved = true;
      }

      private void AppParamContainer_OnAppParamChanged(AppParam appParam)
      {
         if (IsAutoSave && !IsLoading)
         {
            Save(InternalMessages);
         }
      }
   }
}

