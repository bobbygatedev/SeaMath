using Gate.Tools.Extensions;
using System.Reflection;
using System.Text;

namespace Gate.Tools.AppParams
{
   public abstract partial class AppParam
   {
      /// <summary>
      /// 
      /// </summary>
      public class Record : NotScalar
      {
         private const BindingFlags FLAGS_FOR_FIELDS = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

         private bool myHasSubParamsMade = false;

         private readonly List<AppParam> myListPredefinedParams = new List<AppParam>();

         /// <summary>
         /// 
         /// </summary>
         public Record() : this(null, null) { }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="name"></param>
         /// <param name="caption"></param>
         public Record(string? name, string? caption = null) : base(name, caption) => myMakePredefinedParams();

         public string RecordName => GetType().Name.ToLower().EndsWith("record") ? GetType().Name : $"Record:{GetType().Name}";

         public override string Descriptor => $"{RecordName}({ParamName}) = [{string.Join(";", SubParams.Select(f => f.ToString()))}]";

         public AppParam[] SubParams => SubItems.OfType<AppParam>().ToArray();

         /// <summary>
         /// 
         /// </summary>
         public AppParam[] SubParamsPredefined => myListPredefinedParams.ToArray();

         /// <summary>
         /// 
         /// </summary>
         public AppParam[] SubParamsDynamic => SubParams.Except(myListPredefinedParams).ToArray();

         /// <summary>
         /// 
         /// </summary>
         public Record[] SubRecords => SubParams.OfType<Record>().ToArray();

         /// <summary>
         /// 
         /// </summary>
         public Scalar[] SubScalars => SubParams.OfType<Scalar>().ToArray();

         /// <summary>
         /// A frame record is a record whose params are all <see cref="Scalar"/>.
         /// </summary>
         public bool IsFrame => SubParams.All(s => s is Scalar) && SubScalars.Length > 0;

         /// <summary>
         /// 
         /// </summary>
         public bool IsPageRecord => IsOptionTree && SubRecords.All(sr => sr.IsFrame);

         /// <summary>
         /// 
         /// </summary>
         public Record? ParentRecord => ParentParam as Record;

         /// <summary>
         /// In order to appear in AppParamOptionTreeRecordControl shall be overriden and returning true.
         /// </summary>
         public virtual bool IsInTreeNode => false;

         /// <summary>
         /// Option tree have just <see cref="Scalar"/> and <see cref="Record"/> on all descendant params.
         /// </summary>
         public bool IsOptionTree =>
            SubParams.All(s => (s is Scalar || s is Record)) &&
            SubRecords.All(sr => sr.IsOptionTree);

         /// <summary>
         /// 
         /// </summary>
         public bool IsBackupRecord => ParamContainer != null && ParamContainer.BackupParams.Contains(this);

         public FieldInfo[] Fields
         {
            get
            {
               var lst_cha = new List<Type>();
               var typ = GetType();

               //get the hierarchy to AppParam.Record
               while (typ != null && typ != typeof(Record))
               {
                  lst_cha.Add(typ);
                  typ = typ.BaseType;
               }

               //get fields list of all hierarchy parent fields before child fields
               return
                  lst_cha.
                  ToArray().
                  Reverse().
                  SelectMany(t => t.GetFields(FLAGS_FOR_FIELDS).
                  Where(
                     f => f.FieldType.IsSubclassOf(typeof(AppParam)) && f.IsInitOnly)).ToArray();
            }
         }

         public override void Clear()
         {
            var sp = SubParams;

            foreach (var par in sp) { par.Clear(); }
         }

         /// <summary>
         /// Matches property of an object by name to params of the record. If ok write property content to params.
         /// </summary>
         /// <param name="objToRead">Object whose props are read to fill record params.</param>
         /// <param name="isProCompulsory"></param>
         public void WriteProperties(object objToRead, bool isProCompulsory)
         {
            var typ = objToRead.GetType();
            var sca_prs = SubParams.OfType<Scalar>().ToArray();
            var sca_prs_mat =
               sca_prs.Where(p => typ.GetProperty(p.ParamName.ExtTrim()) != null &&
               typ.GetProperty(p.ParamName.ExtTrim())?.GetGetMethod() != null).ToArray();

            if (isProCompulsory && sca_prs_mat.Length < sca_prs.Length)
            {
               var sca_prs_no_mat = sca_prs.Except(sca_prs_mat).ToArray();

               throw new Crash();
            }

            foreach (var par in sca_prs_mat)
            {
               var pro = typ.GetProperty(par.ParamName.ExtTrim());

               par.ObjValue = pro?.GetValue(objToRead, []);
            }
         }

         /// <summary>
         /// Matches property of an object by name to params of the record. If ok read params values and write them to props.
         /// </summary>
         /// <param name="objToFill"></param>
         /// <param name="isProCompulsory"></param>
         public void ReadProperties(object objToFill, bool isProCompulsory)
         {
            var typ = objToFill.GetType();
            var sca_prs = SubParams.OfType<Scalar>().ToArray();
            var sca_prs_mat =
               sca_prs.
                  Where(p =>
                     typ.GetProperty(p.ParamName.ExtTrim()) != null &&
                     typ.GetProperty(p.ParamName.ExtTrim())?.GetSetMethod() != null).ToArray();

            if (isProCompulsory && sca_prs_mat.Length < sca_prs.Length)
            {
               var sca_prs_no_mat = sca_prs.Except(sca_prs_mat).ToArray();

               throw new Crash();
            }

            foreach (var par in sca_prs_mat)
            {
               var pro = typ.GetProperty(par.ParamName.ExtTrim());

               pro?.SetValue(objToFill, par.ObjValue, []);
            }
         }

         public void InsertSubParamDynamically(AppParam appParam, int atIndex = int.MaxValue)
         {
            if (appParam.ParamName != "")
            {
               if (SubParams.Any(p => p.ParamName == appParam.ParamName))
               {
                  throw new Crash($"'{GetType().Name}' already contains a '{typeof(AppParam).Name}' contains an item named '{appParam.ParamName}'");
               }
               else
               {
                  myInsertSubItem(appParam, Math.Min(atIndex, SubParams.Length));
               }
            }
            else
            {
               //by default param name is set to type name.
               appParam.ParamName = appParam.GetType().Name;
               InsertSubParamDynamically(appParam, atIndex);//recalls recursively
            }
         }

         public override void CopyTo(AppParam other)
         {
            if (other.GetType() == GetType())
            {
               var oth = other as Record ?? throw new Crash();

               foreach (var par in SubParams)
               {
                  var oth_par = oth.SubParams.FirstOrDefault(sp => sp.ParamName == par.ParamName);

                  if (oth_par == null)
                  {
                     oth.myAddSubItem(oth_par = par.MakeInstance());
                  }

                  par.CopyTo(oth_par);
               }
            }
            else
            {
               throw new Gate.Tools.ToolsException($"Can't copy from {GetType().Name} to {other.GetType().Name}");
            }
         }


         public override int HashCode => (int)SubParams.Sum(s => (Int64)s.HashCode);

         public override bool Compare(AppParam other) =>
            other is Record rec &&
            rec.GetType() == GetType() &&
            Enumerable.Range(0, SubParams.Length).
               All(i => SubParams[i].Compare(rec.SubParams[i]));

         private void myMakePredefinedParams()
         {
            lock (this)
            {
               if (!myHasSubParamsMade)//lazy init
               {
                  myListPredefinedParams.AddRange(myGetSubParamsFromFields());
                  myAddSubItemRange(myListPredefinedParams);
                  myHasSubParamsMade = true;
               }
            }
         }

         private void myCheckFieldsAndNoProperties()
         {
            var err = new StringBuilder();
            var typ = GetType();
            var fls = Fields;
            var wro_fls_no_ro = fls.Where(f => !f.IsInitOnly).ToArray();
            var wro_fls_nul = fls.Where(f => f.GetValue(this) == null).ToArray();

            if (wro_fls_no_ro.Length + wro_fls_no_ro.Length > 0)
            {
               if (wro_fls_no_ro.Length > 0)
               {
                  err.AppendLine(
                     $"{typeof(AppParam).Name} Field(s) <<{string.Join(",", wro_fls_no_ro.Select(f => f.Name))}>> are not readonly!");
               }

               if (wro_fls_nul.Length > 0)
               {
                  err.AppendLine(
                     $"{typeof(AppParam).Name} Field(s) <<{string.Join(",", wro_fls_nul.Select(f => f.Name))}>> having null value!");
               }
            }

            var err_msg = err.ToString();

            if (err_msg.Length > 0)
            {
               err.Insert(0, $"Fatal error constructing '{typeof(AppParam).Name}' subclass '{GetType().FullName}'\r\n");

               throw new Crash(err.ToString());
            }
         }

         private string myGetParamNameFromFieldName(FieldInfo field) => !field.IsPublic && field.Name.ToLower().StartsWith("my") ? field.Name.Substring(2) : field.Name;

         private AppParam[] myGetSubParamsFromFields()
         {
            myCheckFieldsAndNoProperties();

            var fss = Fields;
            var sub_prs = fss.Select(p => p.GetValue(this)).Cast<AppParam>().ToArray();

            for (int i = 0; i < sub_prs.Length; i++)
            {
               var fld = sub_prs[i];

               if (fld.ParamName == "") { fld.ParamName = myGetParamNameFromFieldName(fss[i]); }
            }

            return sub_prs;
         }
      }
   }
}

