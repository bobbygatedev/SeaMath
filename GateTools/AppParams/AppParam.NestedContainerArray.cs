using Gate.Tools.Extensions;
using Gate.Tools.Message;

namespace Gate.Tools.AppParams
{
   public abstract partial class AppParam
   {
      /// <summary>
      ///  
      /// </summary>
      /// <typeparam name="PAR_CNT"></typeparam>
      public class NestedContainerArray<PAR_CNT> : ArryRaw, INestedContainerArray where PAR_CNT : AppParamContainer, new()
      {
         public NestedContainerArray(NestedFlags flags, string? name = null, string? caption = null) :
            base(name, caption) => Flags = flags;
         public NestedContainerArray() : this(NestedFlags.none) { }

         public delegate void OnErrorHandler(NestedContainerArray<PAR_CNT> sender, NestedContainer<PAR_CNT> item, MsgCollection? msgs);
         public delegate void OnLoadHandler(NestedContainerArray<PAR_CNT> sender, NestedContainer<PAR_CNT> item, PAR_CNT container);

         public event OnErrorHandler? OnError;
         public event OnLoadHandler? OnLoad;

         public override Type ItemFieldType => typeof(NestedContainer<PAR_CNT>);

         public new NestedContainer<PAR_CNT> this[int index] => (NestedContainer<PAR_CNT>)SubItems[index];

         public new NestedContainer<PAR_CNT>[] Items => SubItems.Cast<NestedContainer<PAR_CNT>>().ToArray();


         /// <summary>
         /// Seek for all <see cref="INestedContainer"/> container recursively.  
         /// </summary>
         public NestedContainer<PAR_CNT>[] AllNestedContainersRecursivelySpecialized => AllNestedContainersRecursively.OfType<NestedContainer<PAR_CNT>>().ToArray();

         /// <summary>
         /// Seek for all <see cref="INestedContainer"/> container recursively.  
         /// </summary>
         public INestedContainer[] AllNestedContainersRecursively
         {
            get
            {
               var lst_cnt = new List<INestedContainer>();

               foreach (var itm in Items)
               {
                  if (itm.LoadedParamContainer != null)
                  {
                     lst_cnt = lst_cnt.Concat(itm.AllNestedContainersRecursively).ToList();
                  }
               }

               return lst_cnt.Distinct(NestedContainerComparer.Instance).ToArray();
            }
         }
    
         public NestedFlags Flags { get; private set; }

         INestedContainer[] INestedContainerArray.Items => Items;

         /// <summary>
         /// 
         /// </summary>
         /// <param name="par"></param>
         /// <returns></returns>
         public NestedContainer<PAR_CNT> AddNew(PAR_CNT par)
         {
            var nst_cnt = myMakeNewItem();

            AddParamRaw(nst_cnt);

            var bkp_flg = Flags;

            Flags = NestedFlags.none;//suspends flags for path update
            nst_cnt.LoadedParamContainer = par;
            Flags = bkp_flg;

            return nst_cnt;
         }

         public void Remove(PAR_CNT container)
         {
            foreach (var nst in Items.Where(i => object.ReferenceEquals(i.LoadedParamContainer, container)))
            {
               Remove(nst);
            }
         }

         public void Remove(NestedContainer<PAR_CNT> nestedContainer) => RemoveParamsRaw(nestedContainer);

         public NestedContainer<PAR_CNT> LoadNew(string path, MsgCollection? msgs = null)
         {
            var nst_cnt = myMakeNewItem();

            AddParamRaw(nst_cnt);
            nst_cnt.Relative.RelPath.Absolute = path;
            nst_cnt.Load(msgs ?? ParamContainer?.InternalMessages);

            return nst_cnt;
         }

         public PAR_CNT[]? LoadAll(MsgCollection? msgs = null)
         {
            var mgs = msgs ?? ParamContainer?.InternalMessages;

            return [.. Items.Select(i => i.Load(mgs)).OfType<PAR_CNT>()];
         }

         public override AppParam MakeItem() => myMakeNewItem();

         private NestedContainer<PAR_CNT> myMakeNewItem()
         {
            var nst_cnt = new NestedContainer<PAR_CNT>(Flags);

            nst_cnt.OnLoad += (s, i) => OnLoad?.Invoke(this, s, i);
            nst_cnt.OnError += (s, i) => OnError?.Invoke(this, s, ParamContainer?.InternalMessages);

            return nst_cnt;
         }

         public override void CopyTo(AppParam other)
         {
            if (other is NestedContainerArray<PAR_CNT> oth && oth.GetType() == GetType())
            {
               oth.Clear();

               foreach (var itm in Items)
               {
                  var cpy = new NestedContainer<PAR_CNT>();

                  itm.CopyTo(cpy);
                  oth.AddParamRaw(cpy);
               }
            }
            else
            {
               throw new Gate.Tools.ToolsException($"Can't copy from {GetType().Name} to {other?.GetType().Name}");
            }
         }

         public override AppParam MakeInstance(bool isCopy = false)
         {
            var cpy = base.MakeInstance(isCopy).ConvertOrCrash<NestedContainerArray<PAR_CNT>>();

            cpy.Flags = Flags;

            return cpy;
         }
      }
   }
}
