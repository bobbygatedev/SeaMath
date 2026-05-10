namespace Gate.Tools.AppParams
{
   public abstract partial class AppParam
   {
      public class Simple<T> : Scalar
      {
         static Dictionary<Type, object> myDictionaryDefaultByType = new Dictionary<Type, object>();

         static Simple() => myDictionaryDefaultByType[typeof(string)] = "";

         private T? myValue = myDefaultHandler();

         private static T? myDefaultHandler() => myDictionaryDefaultByType.TryGetValue(typeof(T), out object? def) ? (T)def : default(T);

         public Simple() : base(null, null, false) { }

         public Simple(T? defValue, bool isRecordRequired = false) : base(null, null, isRecordRequired) => Value = DefaultValue = defValue;

         public Simple(string? name, string? caption, T? defValue = default(T), bool isRequired = false) : base(name, caption, isRequired) => Value = DefaultValue = defValue;

         public override Type ParamType => typeof(T);
     
         public T? Value
         {
            get => HasBeenEverSet ? (T?)ObjValue : myDefaultHandler();

            set => ObjValue = value;
         }

         protected override void myActionOnSet(object? newValue) => myValue = (T?)newValue;

         protected override object? myActionOnGet() => myValue;
   
         public T? DefaultValue { get; }

         public override void Clear() => myValue = DefaultValue;
      }
   }
}

