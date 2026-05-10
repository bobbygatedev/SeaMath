using Gate.Tools.Extensions;
using Gate.Tools.Message;

namespace Gate.Tools.AppParams
{
   public abstract partial class AppParam
   {
      /// <summary>
      /// This comparer is used to determine equality between two INestedContainer instances
      /// by comparing the FileInfo of their Relative.RelPath properties. It is used for
      /// collections that require equality checks, such as Distinct() or Except() on nested containers.
      /// </summary>
      public class NestedContainerComparer : IEqualityComparer<INestedContainer>
      {
         public bool Equals(INestedContainer? x, INestedContainer? y) => 
            ReferenceEquals(x, y) || x != null && y != null && x.Relative.RelPath.FileInfo.IsEqual(y.Relative.RelPath.FileInfo);

         public int GetHashCode(INestedContainer obj)
         {
            var ful_nam = obj.Relative.RelPath.FileInfo?.FullName;

            return ful_nam != null ? ful_nam.GetHashCode() : -1;
         }

         public static NestedContainerComparer Instance { get; } = new NestedContainerComparer();
      }

      /// <summary>
      /// Relative file or directory (to <see cref="AppParamContainer.FilePath"/> directory).
      /// </summary>
      public class NestedContainer<PAR_CNT> : Record, INestedContainer where PAR_CNT : AppParamContainer, new()
      {
         public delegate void OnErrorHandler(NestedContainer<PAR_CNT> sender, MsgCollection msgs);
         public delegate void OnLoadHandler(NestedContainer<PAR_CNT> sender, PAR_CNT container);
         private delegate void OnDictionaryChangedHandler(NestedContainer<PAR_CNT> nestedContainer, string path, PAR_CNT paramContainer);

         public event OnErrorHandler? OnError;
         public event OnLoadHandler? OnLoad;

         private event OnDictionaryChangedHandler? OnDictionaryChanged;

         /// <summary>
         /// 
         /// </summary>
         private static Dictionary<string, PAR_CNT> myDictionaryParamContainer = new Dictionary<string, PAR_CNT>();

         private readonly RelativeType myRelative = new RelativeType(RelativePath.OptionsType.file, null, null, null);
         private PAR_CNT? myLoadedParamContainer;

         public NestedContainer() : this(NestedFlags.none) { }

         public NestedContainer(NestedFlags flags, string? name = null, string? caption = null) :
             base(name, caption)
         {
            Flags = flags;
            myRelative.IsRecordRequired = myIsFlag(NestedFlags.required);
            myRelative.OnAnyChange += MyRelPath_OnAnyChange;
         }

         private void myDefaultLoadHandling(NestedFlags flags)
         {
            if (myIsFlag(flags)) { myLoadedParamContainer = new PAR_CNT(); }
         }

         public PAR_CNT? DictionaryParamContainer
         {
            get
            {
               if (myIsFlag(NestedFlags.dictionary))
               {
                  var pth = myRelative.RelPath.FileInfo?.FullName.ToLower();

                  if (pth != null && myDictionaryParamContainer.TryGetValue(pth, out var par_cnt))
                  {
                     return par_cnt;
                  }
               }

               return null;
            }
         }

         protected virtual bool myLoadHandling(MsgCollection? messages)
         {
            if ((myLoadedParamContainer = DictionaryParamContainer) == null)
            {
               if ((myLoadedParamContainer = Load(messages)) == null)
               {
                  return false;
               }
            }

            return true;
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="changedParamField"></param>
         private void MyRelPath_OnAnyChange(AppParam changedParamField)
         {
            var mgs = ParamContainer?.InternalMessages;
            var pth = myRelative.RelPath.FileInfo?.FullName.ToLower();

            if (myLoadedParamContainer != null && myLoadedParamContainer?.FileInfo?.FullName != pth)
            {
               myLoadedParamContainer = null;
            }
         }

         public void Save()
         {
            if (myLoadedParamContainer != null && myLoadedParamContainer.IsToBeSaved && !myRelative.RelPath.Absolute.IsBlank())
            {
               myRelative.RelPath.FileInfo?.Directory?.Create();
               LoadedParamContainer?.Save(null, myRelative.RelPath.Absolute);
            }
         }

         protected override void myActionOnContainerBeforeSave(AppParamContainer paramContainer, string? path)
         {
            if (myIsFlag(NestedFlags.autosave))
            {
               Save();
            }

            base.myActionOnContainerBeforeSave(paramContainer, path);
         }

         private void myErrorHandling(MsgCollection messages)
         {
            if (myIsFlag(NestedFlags.exception)) { throw new Gate.Tools.ToolsException(messages); }
            else { OnError?.Invoke(this, messages); }
         }

         private bool myIsFlag(NestedFlags flag) => (flag & Flags) == flag;

         public PAR_CNT? Load(MsgCollection? messages)
         {
            var par_cnt = new PAR_CNT();
            var is_ok = true;

            messages = messages ?? new MsgCollection();
            myLoadedParamContainer = null;

            if (myRelative.RelPath.Absolute.IsBlank())
            {
               if (myIsFlag(NestedFlags.shall_exist))
               {
                  messages.Add(new Msg(MsgType.error, "Can't load since, path is blank"));
                  myErrorHandling(messages);
               }
            }
            else if (!myRelative.RelPath.FileExists)
            {
               if (myIsFlag(NestedFlags.shall_exist))
               {
                  messages.Add(new Msg(MsgType.error, $"{myRelative} doesn't exist"));
                  myErrorHandling(messages);
               }

               myDefaultLoadHandling(NestedFlags.default_if_not_exists);
            }
            else if (!par_cnt.Load(messages, myRelative.RelPath.FileInfo?.FullName) || !is_ok)
            {
               messages.Add(new Msg(MsgType.error, $"Failed to load {myRelative.RelPath.FileInfo?.FullName}"));
               myErrorHandling(messages);
               myLoadedParamContainer = null;
            }
            else
            {
               messages.Add(new Msg(MsgType.info, $"Loaded {myRelative.RelPath.FileInfo?.FullName}"));
               myLoadedParamContainer = par_cnt;
               OnLoad?.Invoke(this, par_cnt);
               myUpdateDictionary(par_cnt);
            }

            return myLoadedParamContainer;
         }

         private void myUpdateDictionary(PAR_CNT? paramContainer)
         {
            if (myIsFlag(NestedFlags.dictionary) && paramContainer?.FileInfo != null && paramContainer.FileInfo.Exists)
            {
               myDictionaryParamContainer[paramContainer.FileInfo.FullName.ToLower()] = paramContainer;
               OnDictionaryChanged?.Invoke(this, paramContainer.FileInfo.FullName, paramContainer);
            }
         }

         public PAR_CNT? LoadedParamContainer
         {
            get
            {
               if (myLoadedParamContainer == null && myIsFlag(NestedFlags.load))
               {
                  myLoadHandling(ParamContainer?.InternalMessages);
               }

               return myLoadedParamContainer;
            }

            set
            {
               myLoadedParamContainer = value;
               myRelative.RelPath.Absolute = LoadedParamContainer?.FileInfo?.FullName;
               myUpdateDictionary(myLoadedParamContainer);
            }
         }

         public FileInfo? FileInfo => Relative.RelPath.FileInfo;

         public RelativeType Relative => myRelative;

         public NestedFlags Flags { get; private set; }

         AppParamContainer? INestedContainer.LoadedParamContainer => LoadedParamContainer;

         public INestedContainer[] AllNestedContainersRecursively
         {
            get
            {
               var lst = new List<INestedContainer>();

               myAppend(lst, this);

               return lst.ToArray();
            }
         }

         private void myAppend(List<INestedContainer> listContainer, INestedContainer otherContainer)
         {
            if (!listContainer.Any(c => c.Relative.RelPath.FileInfo.IsEqual(otherContainer.Relative.RelPath.FileInfo)))
            {
               listContainer.Add(otherContainer);

               var dsc = ((AppParam)otherContainer).AllDescendant.OfType<INestedContainer>().Distinct(NestedContainerComparer.Instance).ToArray();
               var dsc_to_src = dsc.Except(listContainer, NestedContainerComparer.Instance).ToArray();

               foreach (var pc in dsc_to_src) { myAppend(listContainer, pc); }
            }
         }

         public override void Clear()
         {
            myRelative.Clear();
            myLoadedParamContainer = null;
         }

         public override void CopyTo(AppParam other)
         {
            base.CopyTo(other);

            if (other is NestedContainer<PAR_CNT> oth && oth.GetType() == GetType())
            {
               myRelative.CopyTo(oth.myRelative);
               oth.Flags = Flags;
               oth.myLoadedParamContainer = LoadedParamContainer;
            }
         }

         public override string ToString() => $"Nested {Relative.RelPath} ({(LoadedParamContainer != null ? "Loaded" : "Not loaded")})";
      }
   }
}
