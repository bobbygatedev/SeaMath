namespace Gate.Tools.AppParams
{
   public abstract partial class AppParam
   {
      public abstract class ArryRaw : NotScalar
      {
         private enum UpdateIdxAction
         {
            add = 0,
            remove = 1,
         }

         public ArryRaw(string? name, string? caption = null) : base(name, caption) { }

         public AppParam this[int index] => (AppParam)SubItems[index];

         public AppParam[] Items => SubItems.Cast<AppParam>().ToArray();

         public abstract Type ItemFieldType { get; }

         public bool IsScalarArray => ItemFieldType.IsSubclassOf(typeof(Scalar));

         public bool IsArrayOfArray => ItemFieldType.IsSubclassOf(typeof(ArryRaw));

         public bool IsArrayOfRecord => ItemFieldType.IsSubclassOf(typeof(Record));

         public abstract AppParam MakeItem();

         public AppParam InsertParamRaw(AppParam param, int index)
         {
            if (param.GetType().FullName == ItemFieldType.FullName)
            {
               myInsertSubItem(param, index);
               myUpdateIdxIndicator(UpdateIdxAction.add, param, index);
               param.OnAnyChange += myActionOnAnyChange;
               myActionOnAnyChange(this);

               return param;
            }
            else { throw new Crash($"Field not of type{ItemFieldType.FullName}"); }
         }

         public bool RemoveParamsRaw(params AppParam[] @params)
         {
            if (@params.Length == 0) { return true; }
            else if (@params.All(p => p.GetType().FullName == ItemFieldType.FullName))
            {
               foreach (var par in @params) { myUpdateIdxIndicator(UpdateIdxAction.remove, par, -1); }

               if (@params.All(p => p.ParentItem == this))
               {
                  myRemoveSubItemRange(@params);
                  myActionOnAnyChange(this);

                  return true;
               }
               else { return false; }
            }
            else { throw new Crash($"Field not of type{ItemFieldType.FullName}"); }
         }

         public AppParam AddParamRaw(AppParam param) => InsertParamRaw(param, SubItems.Length);

         public void AddParamsRaw(params AppParam[] appParams)
         {
            foreach (var par in appParams) { AddParamRaw(par); }
         }

         public override void Clear()
         {
            //remove all object items
            if (SubItems.Length > 0)
            {
               myRemoveSubItemRange(SubItems);
               myActionOnAnyChange(this);
            }
         }
         private bool myUpdateIdxIndicator(UpdateIdxAction action, AppParam appParam, int index)
         {
            var lst_sub_itm = SubItems.OfType<AppParam>().ToList();

            if (action == UpdateIdxAction.add)
            {
               if (index >= 0 && index <= lst_sub_itm.Count) { lst_sub_itm.Insert(index, appParam); }
               else { return false; }
            }
            else
            {
               if (!lst_sub_itm.Remove(appParam)) { return false; }
            }

            var idx = 0;
            var par_nam = appParam.ParentParam?.ParamName ?? throw new Crash();

            foreach (var par in lst_sub_itm) { par.ParamName = $"{par_nam}_{idx++}"; }

            return true;
         }

         public int ItemCount => SubItems.Length;

         public override string Descriptor => $"{ParamName} = [{string.Join(";", SubItems.Select(f => f.ToString()))}]";

         public override int HashCode => (int)Items.Sum(i => (Int64)i.HashCode);

         public override bool Compare(AppParam other)
         {
            if (other?.GetType() == GetType())
            {
               var oth = other as ArryRaw;

               if (oth?.Items.Length == Items.Length)
               {
                  return Enumerable.Range(0, Items.Length).All(i => Items[i].Compare(oth.Items[i]));
               }
            }

            return false;
         }
      }
   }
}

