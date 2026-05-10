using Gate.Tools.Extensions;
using System.Reflection;

namespace Gate.Tools.Text.TemplateExpand
{
   internal class TemplateExpanderClassReader
   {
      private static Dictionary<Type, TemplateExpanderClassReader> myDictionaryCache = new Dictionary<Type, TemplateExpanderClassReader>();

      public TemplateExpanderClassReader(Type type, SingleReader[] readers)
      {
         Type = type;
         Readers = readers;
      }

      public abstract class SingleReader
      {
         protected SingleReader(string id) => Id = id;

         public class ByField : SingleReader
         {
            public ByField(string id, FieldInfo field) : base(id) => Field = field;

            public FieldInfo Field { get; }

            public override object? GetValue(object inputObject) => Field.GetValue(inputObject);

            public override string ToString() => $"Reader-by-field {Field.FieldType.Name}.{Field.Name}";
         }

         public class ByProperty : SingleReader
         {
            public ByProperty(string id, PropertyInfo property) : base(id) => Property = property;

            public PropertyInfo Property { get; }

            public override object? GetValue(object inputObject) => Property.GetValue(inputObject);

            public override string ToString() => $"Reader-by-property {Property.PropertyType.Name}.{Property.Name}";
         }

         public string Id { get; }

         public abstract object? GetValue(object inputObject);
      }

      public SingleReader[] Readers { get; }

      public Type Type { get; }

      public static TemplateExpanderClassReader GetReader(Type type)
      {
         if (myDictionaryCache.TryGetValue(type, out var rdr))
         {
            return rdr;
         }
         else
         {
            myDictionaryCache[type] = myMakeReader(type);

            return GetReader(type);
         }
      }

      public static TemplateExpanderClassReader GetReader(object obj) => GetReader(obj.GetType());

      private static TemplateExpanderClassReader myMakeReader(Type type)
      {
         var lst_fls = type.GetFields().ToList();
         var lst_fls_ats = lst_fls.Select(p => p.GetCustomAttribute<TemplateExpanderAttribute>()).ToList();
         var lst_prs = type.GetProperties().ToList();
         var lst_prs_ats = lst_prs.Select(p => p.GetCustomAttribute<TemplateExpanderAttribute>()).ToList();

         var dct = new Dictionary<string, SingleReader>();

         for (var i = 0; i < lst_fls.Count; i++)
         {
            var id = (lst_fls_ats[i]?.Id ?? lst_fls[i].Name).ExtTrim();

            if (id.IsBlank())
            {
               throw new Gate.Tools.ToolsException($"Blank Id for field {type.Name}.{lst_fls[i].Name}");
            }
            else if (dct.ContainsKey(id))
            {
               throw new Gate.Tools.ToolsException($"Id '{id}' already used for class {type.Name}");
            }
            else
            {
               dct[id] = new SingleReader.ByField(id, lst_fls[i]);
            }
         }

         for (var i = 0; i < lst_prs.Count; i++)
         {
            var id = (lst_prs_ats[i]?.Id ?? lst_prs[i].Name).ExtTrim();

            if (id.IsBlank())
            {
               throw new Gate.Tools.ToolsException($"Blank Id for property {type.Name}.{lst_prs[i].Name}");
            }
            else if (dct.ContainsKey(id))
            {
               throw new Gate.Tools.ToolsException($"Id '{id}' already used for class {type.Name}");
            }
            else
            {
               dct[id] = new SingleReader.ByProperty(id, lst_prs[i]);
            }
         }

         var rea = new TemplateExpanderClassReader(type, dct.Values.ToArray());

         return rea;
      }

      public object? GetValue(object value, string id) =>
         TryGetValue(value, id, out var res) ?
            res :
            throw new Gate.Tools.ToolsException($"Id '{id}' not found in reader for type '{value.GetType().Name}'");

      public bool TryGetValue(object value, string id, out object? result)
      {
         var sr = Readers.FirstOrDefault(x => x.Id == id);

         if (sr != null)
         {
            result = sr.GetValue(value);

            return true;
         }
         else
         {
            result = null;

            return false;
         }
      }

      public static bool TryGetReaderValue(object inValue, string id, out object? value)
      {
         var rea = GetReader(inValue.GetType());

         return rea.TryGetValue(inValue, id, out value);
      }
   }
}
