using Gate.Tools.Extensions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Gate.Tools.AppParams
{
   /// <summary>
   /// 
   /// </summary>
   public abstract partial class AppParam : HierarchicalItem
   {
      private readonly ParentObserver myParentObserver;

      private static Regex myRegexName = new Regex(@"\w+", RegexOptions.Compiled);
      private string? myParamName;
      private string? myParamCaption;
      private AppParamContainer? myAppParamContainerOld;
      private readonly ConstructorInfo myDefaultConstructor;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="changedParamField"></param>
      public delegate void OnAnyChangeHandler(AppParam changedParamField);

      /// <summary>
      /// 
      /// </summary>
      public event OnAnyChangeHandler? OnAnyChange;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="name"></param>
      /// <param name="caption"></param>
      public AppParam(string? name, string? caption)
      {
         myDefaultConstructor = myCheckForDefaultConstructor();
         ParamName = name;
         myParamCaption = caption.IsBlank() ? name : caption;
         myParentObserver = new ParentObserver(this);
         myParentObserver.OnParentRemoved += MyParentObserver_OnAnyChange;
         myParentObserver.OnParentAdded += MyParentObserver_OnAnyChange;
      }

      /// <summary>
      /// Abstract copy method (from this to other)
      /// </summary>
      /// <param name="other"></param>
      public abstract void CopyTo(AppParam other);

      /// <summary>
      /// 
      /// </summary>
      public abstract string Descriptor { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract void Clear();

      public abstract bool Compare(AppParam other);

      public abstract int HashCode { get; }

      public override bool Equals(object? obj) => obj is AppParam oth && Compare(oth);

      public override int GetHashCode() => HashCode;

      /// <summary>
      /// Field info for app param when an articulated parent <see cref="AppParam"/>
      /// </summary>
      public FieldInfo? AssociatedArticulatedField
      {
         get
         {
            if (ParentItem is Record rec)
            {
               var fls = rec.Fields;

               return fls.FirstOrDefault(f => f.GetValue(rec) == this);
            }
            else { return null; }
         }
      }

      /// <summary>
      /// Parameter name.
      /// </summary>
      public string? ParamName
      {
         get => myParamName.ExtTrim();

         private set
         {
            if (!value.IsBlank()) { myParamName = myCheckName(value); }
         }
      }

      /// <summary>
      /// Parameter caption, which is text appearing in controls.
      /// </summary>
      public string? ParamCaption => myParamCaption.IsBlank() ? ParamName : myParamCaption;

      /// <summary>
      /// 
      /// </summary>
      public bool IsRoot => ParentItem is not AppParam;

      /// <summary>
      /// 
      /// </summary>
      public string? ParamPath
      {
         get
         {
            var cnt = ParentItemChain.OfType<AppParamContainer>().FirstOrDefault();

            if (cnt == null) { return null; }
            else
            {
               var cnt_idx = ParentItemChain.ToList().IndexOf(cnt);

               return string.Join("/", ParentItemChain.Take(cnt_idx).OfType<AppParam>().Reverse().Select(f => f.ParamName));
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public AppParamContainer? ParamContainer => ParentItemChain.OfType<AppParamContainer>().FirstOrDefault();

      /// <summary>
      /// All paramss (recursively).
      /// </summary>
      public AppParam[] AllParams => AllDescendant.OfType<AppParam>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public AppParam? ParentParam => ParentItem as AppParam;

      /// <summary>
      /// General Purpose
      /// </summary>
      public object? Tag { get; set; }

      /// <summary>
      /// Makes a new instance of the same type.
      /// </summary>
      /// <param name="isCopy">When true new content is populate using <see cref="CopyTo(AppParam)"/></param>
      /// <returns></returns>
      public virtual AppParam MakeInstance(bool isCopy = false)
      {
         var new_par = myDefaultConstructor.Invoke([]).ConvertOrCrash<AppParam>();

         new_par.myParamName = ParamName;
         new_par.myParamCaption = ParamCaption;

         if (isCopy)
         {
            CopyTo(new_par);
         }

         return new_par;
      }

      public override string ToString() => Descriptor;

      protected virtual void myActionOnAnyChange(AppParam changedParamField)
      {
         OnAnyChange?.Invoke(changedParamField);

         myDispatchToParent();
      }

      protected override void myActionOnChildAdded(HierarchicalItem childAdded)
      {
         base.myActionOnChildAdded(childAdded);
         myDispatchToParent();
      }

      protected override void myActionOnChildRemoved(HierarchicalItem childAdded)
      {
         base.myActionOnChildRemoved(childAdded);
         myDispatchToParent();
      }

      private static string myCheckName(string? name)
      {
         var nam = name.ExtTrim();
         var mat = myRegexName.Match(nam);

         return mat.Success && mat.Length == nam.Length ?
            nam : throw new Crash($"'{name}' is not valid as '{typeof(AppParam).Name}' name!");
      }

      protected virtual void myActionOnContainerLoad(AppParamContainer paramContainer, string? path) { }

      protected virtual void myActionOnContainerBeforeSave(AppParamContainer paramContainer, string? path) { }

      protected virtual void myActionOnContainerAfterSave(AppParamContainer paramContainer, string? path) { }

      private void MyParentObserver_OnAnyChange(HierarchicalItem observer, HierarchicalItem childItem)
      {
         if (myAppParamContainerOld != ParamContainer)
         {
            if (myAppParamContainerOld != null)
            {
               myAppParamContainerOld.OnBeforeSave -= myActionOnContainerBeforeSave;
               myAppParamContainerOld.OnAfterSave -= myActionOnContainerAfterSave;
               myAppParamContainerOld.OnLoad -= myActionOnContainerLoad;
            }

            if (ParamContainer != null)
            {
               ParamContainer.OnBeforeSave += myActionOnContainerBeforeSave;
               ParamContainer.OnAfterSave += myActionOnContainerAfterSave;
               ParamContainer.OnLoad += myActionOnContainerLoad;
            }

            myAppParamContainerOld = ParamContainer;
         }
      }

      private void myDispatchToParent()
      {
         //dispatch change to parent
         if (ParentItem is AppParam app_par) { app_par.myActionOnAnyChange(this); }
      }

      private ConstructorInfo myCheckForDefaultConstructor() =>
         GetType().GetConstructor(new Type[0]) ??
         throw new Crash($"{GetType().FullName} has not default constructor!");
   }
}

