namespace Gate.Tools.Text.TemplateExpand
{
   /// <summary>
   /// 
   /// </summary>
   internal class TemplateExpanderContentRecord : TemplateExpanderContent
   {
      private object? myValue;

      public TemplateExpanderContentRecord() { }

      public TemplateExpanderContentArray? ParentArray => ParentItem as TemplateExpanderContentArray;

      public override TemplateExpanderContent this[int id] => SubContents[id];

      public override TemplateExpanderContent this[string id] =>
         SubContents.FirstOrDefault(sc => sc.Id == id) ??
         throw new Gate.Tools.ToolsException($"Field {id} ot defined");

      public override string Id => ParentItem is TemplateExpanderContentArray ? $"[{ItemId}]" : "[Root]";

      public Dictionary<string, object?> Dictionary
      {
         get
         {
            var dct = new Dictionary<string, object?>();

            foreach (var cnt in SubContents) { dct[cnt.Id] = cnt.Value; }

            return dct;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override object? Value
      {
         get => myValue;

         set
         {
            if (ParentItem == null)
            {
               throw new Gate.Tools.ToolsException($"Can't set a {GetType().Name} not connected to any {typeof(TemplateExpander).Name}");
            }
            else if ((myValue = value) != null)
            {
               if (myIsScalar(value) && Dictionary.Count == 1)
               {
                  SubContents[0].Value = value;
               }
               else if (value is IDictionary<string, object> dct)
               {
                  foreach (var kp in dct)
                  {
                     TrySet(kp.Key, kp.Value);
                  }
               }
               else //is a class
               {
                  var rdr = TemplateExpanderClassReader.GetReader(value ?? throw new Crash());

                  foreach (var sc in SubContents)
                  {
                     if (rdr.TryGetValue(value, sc.Id, out var val))
                     {
                        sc.Value = val;
                     }
                  }
               }
            }
            else
            {
               foreach (var cnt in SubContents)
               {
                  cnt.Value = null;
               }
            }
         }
      }

      public bool TrySet(string key, object value)
      {
         var sc = SubContents.FirstOrDefault(s => s.Id == key);

         if (sc != null)
         {
            sc.Value = value;

            return true;
         }
         else
         {
            return false;
         }
      }

      public bool TryGet(string id, out TemplateExpanderContent? value)
      {
         var sc = SubContents.FirstOrDefault(sc => sc.Id == id);
         
         if (sc != null)
         {
            value = sc;
            return true;
         }
         else
         {
            value = null;
         
            return false;
         }
      }

      public override object? ValueInherited => throw new Gate.Tools.ToolsException($"ValueInherited not valid for {GetType().Name}");

      public override string ContentDescriptor
      {
         get
         {
            var sc = SubContents;

            return $"{{{string.Join(",", sc.Select(s => $"{s.Id}:{s.ContentDescriptor}"))}}}";
         }
      }

      private bool myIsScalar(object? value) => value is string || (value?.GetType().IsPrimitive ?? false);

      /// <summary>
      /// 
      /// </summary>
      public override TxtTokenConst ExpandedToken
      {
         get
         {
            if (ParentArray != null)
            {
               var sto = Symbol.Token.Store?.GetCopy() ?? throw new ToolsException();
               var sym_arr = ParentArray?.SymbolArray ?? throw new ToolsException();
               var scs = SubContents;

               //eg #[#[ => #[
               var sbs = Symbol.SubSymbols.OfType<TemplateExpanderSymbolStartBorderReplace>().ToArray();

               var sbs_rps = sbs.Select(s =>
                  new TxtStoreReplacement(
                     s.Token.Interval, new TxtTokenConst((TemplateExpander ?? throw new ToolsException()).TokenBorders.ini))).ToArray();

               var rps_tks = scs.Select(c => c.ExpandedToken).ToArray();
               var cnt_tks = scs.Select(c => c.Symbol.Token).ToArray();

               var rps = Enumerable.Range(0, rps_tks.Length).
                  Select(i => new TxtStoreReplacement(cnt_tks[i].Interval, rps_tks[i])).ToArray();

               if (sym_arr.IsMultiline)
               {
                  var ln_sta = sto[(sym_arr.StartToken.From ?? throw new Crash()).Line + 1];
                  var ln_end = sto[(sym_arr.EndToken.To ?? throw new Crash()).Line - 1];

                  sto.SplitSector(ln_sta.Interval.From, ln_end.Interval.To + 1);
               }
               else
               {
                  sto.SplitSector(
                     (sym_arr.StartToken.To ?? throw new Crash()).StoreIdx + 1,
                     (sym_arr.EndToken.From ?? throw new Crash()).StoreIdx);
               }

               sto.Replace(sbs_rps.Concat(rps));
               sto.RemoveIntervals(sto.OwnedSectors[0].Interval, sto.OwnedSectors.Last().Interval);

               return new TxtTokenConst(sto);
            }
            else
            {
               throw new Gate.Tools.ToolsException($"Can't expand a {GetType().Name} not connected to any {typeof(TemplateExpander).Name}");
            }
         }
      }

      public int ItemId => ParentArray?.SubContents.ToList().IndexOf(this) ?? -1;

      public override int? ArrayCount
      {
         get => throw new Gate.Tools.ToolsException($"Array count not valid for {GetType().Name}");
         set => throw new Gate.Tools.ToolsException($"Array count not valid for {GetType().Name}");
      }

      public override TemplateExpanderSymbol Symbol => ParentArray?.Symbol ?? throw new Gate.Tools.ToolsException($"Symbol not valid for {GetType().Name}");

      public TemplateExpanderSymbol[]? SubSymbols => Symbol != null ? Symbol.SubSymbols : (TemplateExpander?.Symbols.Items);

      protected override void myActionOnParentSet(HierarchicalItem parentItem)
      {
         base.myActionOnParentSet(parentItem);

         if (parentItem is TemplateExpanderContentArray arr)
         {
            myAddSubItemRange(arr.SymbolArray.SubSymbols.Select(s => Make(s)));
         }
         else if (parentItem is TemplateExpander exp)
         {
            myAddSubItemRange(exp.Symbols.Select(s => Make(s)));
         }
         else { throw new Crash(); }
      }

      /// <summary>
      /// Empties content
      /// </summary>
      /// <param name="parentItem"></param>
      protected override void myActionOnParentReset(HierarchicalItem parentItem)
      {
         base.myActionOnParentReset(parentItem);

         myRemoveSubItemRange(SubItems);
      }

      public override string ToString() => $"{IdExtended}:{string.Join(",", SubContents.Select(s => s.ContentDescriptor))}";
   }
}
