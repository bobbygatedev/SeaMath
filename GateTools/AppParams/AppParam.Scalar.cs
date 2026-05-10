namespace Gate.Tools.AppParams
{
   public abstract partial class AppParam
   {
      /// <summary>
      /// 
      /// </summary>
      public abstract class Scalar : AppParam
      {
         /// <summary>
         /// 
         /// </summary>
         /// <param name="name"></param>
         /// <param name="caption"></param>
         /// <param name="isRecordRequired"></param>
         public Scalar(string? name, string? caption, bool isRecordRequired) : base(name, caption) => IsRecordRequired = isRecordRequired;

         /// <summary>
         /// 
         /// </summary>
         public object? ObjValue
         {
            get => myActionOnGet();

            set
            {
               myActionOnSet(value);
               HasBeenEverSet = true;
               myActionOnAnyChange(this);
            }
         }

         public bool HasBeenEverSet { get; private set; }

         protected abstract void myActionOnSet(object? newValue);

         protected abstract object? myActionOnGet();

         /// <summary>
         /// 
         /// </summary>
         /// <returns></returns>
         /// <exception cref="Crash"></exception>
         public virtual object? GetCopy()
         {
            if (ObjValue == null) { return null; }
            else if (ParamType.IsValueType)
            {
               dynamic cpy = ObjValue;

               return cpy;
            }
            else if (ObjValue is ICloneable) { return ((ICloneable)ObjValue).Clone(); }
            else { throw new Crash($"Type {ParamType} can not be copied, shall "); }
         }

         /// <summary>
         /// 
         /// </summary>
         public abstract Type ParamType { get; }

         /// <summary>
         /// Whether the vaue shall be specified when scalar is part of a record.
         /// </summary>
         public bool IsRecordRequired { get; set; }

         public override string Descriptor => $"{ParamName} = {ObjValue}";

         public override void CopyTo(AppParam other)
         {
            if (other.GetType() == GetType())
            {
               var oth = other as Scalar ?? throw new Crash();

               oth.ObjValue = GetCopy();
            }
            else
            {
               throw new Gate.Tools.ToolsException($"Can't copy from {GetType().Name} to {other.GetType().Name}");
            }
         }

         public override int HashCode => ObjValue != null ? ObjValue.GetHashCode() : int.MinValue;

         public override bool Compare(AppParam other)
         {
            if (other is Scalar sca && sca.GetType() == GetType())
            {
               var obj = ObjValue;
               var obj_oth = sca.ObjValue;
               var par_typ = ParentParam?.GetType();
               var par_oth_typ = sca?.ParentParam?.GetType();
               var par_nam = ParamName;
               var par_oth_nam = other.ParamName;

               return
                   ReferenceEquals(par_typ, par_oth_typ) &&
                   par_typ == par_oth_typ &&
                   par_nam == par_oth_nam &&
                   obj != null ? obj.Equals(obj_oth) : obj_oth == null;
            }
            else { return false; }
         }
      }
   }
}

