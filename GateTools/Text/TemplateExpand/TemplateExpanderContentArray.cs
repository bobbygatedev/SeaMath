using System.Collections;

namespace Gate.Tools.Text.TemplateExpand
{
   internal class TemplateExpanderContentArray : TemplateExpanderContent
   {
      private IEnumerable? myValue;
      private int? myArrayCount;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="symbolArray"></param>
      public TemplateExpanderContentArray(TemplateExpanderSymbolArray symbolArray) => SymbolArray = symbolArray;

      public int? ArrayCountInherited => ValueInherited is IEnumerable enr ? enr.Cast<object>().Count() : (int?)null;

      public override int? ArrayCount
      {
         get => myArrayCount ?? ArrayCountInherited;

         set
         {
            if (myArrayCount.HasValue)
            {
               myRemoveSubItemRange(SubContents);
            }

            if ((myArrayCount = value).HasValue)
            {
               var val = Math.Max(0, value ?? 0);

               myAddSubItemRange(Enumerable.Range(0, val).Select(i => new TemplateExpanderContentRecord()));
            }
         }
      }

      public override TemplateExpanderContent this[int id]
      {
         get
         {
            if (ArrayCount.HasValue && id < ArrayCount)
            {
               var scs = SubContents;

               return scs[id];
            }
            else
            {
               throw new Gate.Tools.ToolsException($"{id} exceed array count {ArrayCount}");
            }
         }
      }

      public override TemplateExpanderContent this[string id] => throw new Gate.Tools.ToolsException($"this[string id] not suitable for {GetType().Name}");

      public override TemplateExpanderSymbol Symbol => SymbolArray;

      public new TemplateExpanderContentRecord[] SubContents
      {
         get
         {
            if (myArrayCount == null)
            {
               if (ValueInherited is IEnumerable enr)
               {
                  var obs = enr.Cast<object>().ToArray();
                  var scs = base.SubContents.OfType<TemplateExpanderContentRecord>().ToArray();

                  myRemoveSubItemRange(scs);

                  foreach (var obj in obs)
                  {
                     var rec = new TemplateExpanderContentRecord();

                     myAddSubItem(rec);
                     rec.Value = obj;
                  }
               }
            }

            return SubItems.OfType<TemplateExpanderContentRecord>().ToArray();
         }
      }

      public override string Id => Symbol.Id ?? "";

      public override object? Value
      {
         get => myValue != null ? myValue.Cast<object>().ToArray() : null as object;

         set
         {
            if (value is IEnumerable enr)
            {
               var arr = enr.Cast<object>().ToArray();

               ArrayCount = arr.Length;

               for (int i = 0; i < arr.Length; i++)
               {
                  this[i].Value = arr[i];
               }

               myValue = enr;
            }
            else if (value == null)
            {
               ArrayCount = null;//empty array
            }
            else if (value is bool bv)
            {
               ArrayCount = bv ? 1 : 0;
            }
            else
            {
               throw new Gate.Tools.ToolsException($"value shall be {typeof(IEnumerable).Name}");
            }
         }
      }

      public override object? ValueInherited
      {
         get
         {
            if (TemplateExpander != null && TemplateExpander.IsInheritanceEnabled)
            {
               // anchestor scalar value 
               // check if any parent has set a value of same id
               // eg  ' [L1] {[A:][L1] [;]}' with L1 = xxx and A.Count = 2 =>' xxx {xxx xxx}'

               //all parent chain of record example
               var rcs = ParentItemChain.Skip(1).OfType<TemplateExpanderContentRecord>().ToArray();

               foreach (var rec in rcs)
               {
                  var arr = rec[Id] as TemplateExpanderContentArray;

                  if (arr != null && arr.myArrayCount.HasValue)
                  {
                     return arr.Value ?? arr.SubContents.Select(r => r.Dictionary).ToArray();
                  }
                  else if (rec.Value != null)
                  {
                     if (TemplateExpanderClassReader.TryGetReaderValue(rec.Value, Id, out var val) && val is IEnumerable en2)
                     {
                        return en2;
                     }
                  }
               }

               return null;
            }
            else
            {
               return null;
            }
         }
      }

      public override TxtTokenConst ExpandedToken
      {
         get
         {
            if (!ArrayCount.HasValue)
            {
               throw new Gate.Tools.ToolsException($"Array Content {Id} not set!");
            }
            else if (ArrayCount.HasValue && ArrayCount > 0)
            {
               var sc = SubContents;
               var nl = TemplateExpander?.TemplateStore?.Settings.NewLine;

               return SymbolArray.IsMultiline ?
                  new TxtTokenConst(string.Join(nl, sc.Select(s => s.ExpandedToken.Content))) :
                  new TxtTokenConst(string.Join("", sc.Select(s => s.ExpandedToken.Content)));
            }
            else
            {
               return new TxtTokenConst("");//empty array
            }
         }
      }

      public TemplateExpanderSymbolArray SymbolArray { get; }

      public override string ContentDescriptor
      {
         get
         {
            if (!ArrayCount.HasValue)
            {
               return "{not_set}";
            }
            else
            {
               var sc = SubContents;

               return $"[{string.Join(",", Enumerable.Range(0, ArrayCount.Value).Select(i => $"{i}:{sc[i].ContentDescriptor}"))}]";
            }
         }
      }

      public override string ToString() => $"{IdExtended}:[{ContentDescriptor}]";
   }
}
