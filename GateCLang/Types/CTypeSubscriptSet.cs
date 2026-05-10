using Gate.CLanguage.Expressions;
using Gate.Tools;
using Gate.Tools.Extensions;
using System.Text;

namespace Gate.CLanguage.Types
{
   /// <summary>
   /// 
   /// </summary>
   public class CTypeSubscriptSet : CItem
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="subscripts"></param>
      public CTypeSubscriptSet(params CTypeSubscript[] subscripts) => AddSubScripts(subscripts);

      /// <summary>
      /// 
      /// </summary>
      public override bool HasAssociatedPragma => false;

      /// <summary>
      /// 
      /// </summary>
      public override string Descriptor => GetIdentifierDescriptor(null);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="index"></param>
      /// <returns></returns>
      public CTypeSubscript this[int index] => SubItems.OfType<CTypeSubscript>().ElementAt(index);

      /// <summary>
      /// Whether the declaration binded to this is pointer (ie first subscript is a pointer).
      /// </summary>
      public bool IsPointer => SubscriptCount > 0 && SubscriptsOrdered[0].Kind == CTypeSubscriptKind.pointer;

      /// <summary>
      /// Whether the declaration binded to this is pointer (ie first subscript is a pointer).
      /// </summary>
      public bool IsArray => SubscriptsOrdered.FirstOrDefault()?.Kind == CTypeSubscriptKind.array;

      /// <summary>
      /// Number of subscript 
      /// </summary>
      public int SubscriptCount => Subscripts.Length;

      /// <summary>
      /// 1 for pointer total item of size for an array
      /// </summary>
      public int TotalItems
      {
         get
         {
            if (IsIncompleteArray && !IncompleteArraySize.HasValue) { return -1; }
            else if (ArraySizesConst.Length == 0) { return 1; }
            else { return ArraySizesConst.Aggregate((s1, s2) => s1 * s2); }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override string Rebuilt => Descriptor;

      /// <summary>
      /// 
      /// </summary>
      public string Signature => string.Join(" ", SubscriptsOrdered.Select(s => s.Signature));

      /// <summary>
      /// 
      /// </summary>
      public string DescriptorGcc => string.Join("", SubscriptsOrdered.Select(s => s.DescriptorGcc));


      /// <summary>
      /// <br>If <see cref="IsArray"/> is true returns array subscript (they not necessary coincide with all array subscript) </br>
      /// <br>eg in 'int (*v[2])[3]' <see cref="ArraySubscriptsOrdered"/> is { [2] }  </br> 
      /// </summary>
      public CTypeSubscript[] ArraySubscriptsOrdered => SubscriptsOrdered.TakeWhile(s => s.Kind == CTypeSubscriptKind.array).ToArray();

      /// <summary>
      /// 
      /// </summary>
      public CExprStatement?[]? ArraySizes =>
         IsPointer ? null : ArraySubscriptsOrdered.Select(s => s.IsIncomplete ? null : s.ArraySizeExpr).ToArray();

      /// <summary>
      /// 
      /// </summary>
      public int[] ArraySizesConst
      {
         get
         {
            if (IsConstantSizeArray)
            {
               return IsIncompleteArray ?
                  [
                     IncompleteArraySize ?? -1,
                     .. (ArraySizes ?? []).Skip(1).
                        Select(s => (int)(s?.ConstValue?.CSharpObj ?? 0)),
                  ] :
                  [.. (ArraySizes ?? []).Select(s => (int)(s?.ConstValue?.CSharpObj ?? 0))];
            }
            else
            {
               throw new Gate.CLanguage.CLangException($"Not a constant size array");
            }
         }
      }

      public bool IsConstantSizeArray => IsIncompleteArray ?
         ArraySubscriptsOrdered.Skip(1).All(s => s.ArraySizeConst.HasValue) :
         ArraySubscriptsOrdered.All(s => s.ArraySizeConst.HasValue);

      public int? IncompleteArraySize => IsIncompleteArray ? ArraySubscriptsOrdered[0].IncompleteArraySize : null;

      /// <summary>
      /// 
      /// </summary>
      public string PointerTag => string.Join("",
            Subscripts.
            Where(p => p.Kind == CTypeSubscriptKind.pointer).
            Select(p => p.Descriptor));

      /// <summary>
      /// 
      /// </summary>
      public int PointerDepth => Subscripts.Count(p => p.Kind == CTypeSubscriptKind.pointer);

      /// <summary>
      /// Incomplete type ie first array subscript is empty (eg 'int a[]', 'int* a[][3]')
      /// </summary>
      public bool IsIncompleteArray => IsArray && SubscriptsOrdered[0].IsIncomplete;

      /// <summary>
      /// Subscripts as they appear in source code.
      /// </summary>
      public CTypeSubscript[] Subscripts => SubItems.OfType<CTypeSubscript>().ToArray();

      /// <summary>
      /// Subscripts ordrered functionally (eg 'int *a[2]'=> '[2]','*' ;  'int (*a)[2]'=> '*','[2]' ).
      /// </summary>
      public CTypeSubscript[] SubscriptsOrdered => Subscripts.OrderBy(s => s).ToArray();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="subscripts"></param>
      public void AddSubscriptsOrdered(params CTypeSubscript[] subscripts)
      {
         var sub_ord = SubscriptsOrdered.Concat(subscripts).ToArray();
         var lst_sub_ord = sub_ord.ToList();

         for (int i = 0; i < sub_ord.Length; i++)
         {
            var sub = sub_ord[i];

            if (sub.Kind == CTypeSubscriptKind.pointer)
            {
               var idx = lst_sub_ord.IndexOf(sub);

               sub.ParCount = sub_ord.Skip(idx + 1).Count(s => s.Kind == CTypeSubscriptKind.array);
            }
         }

         for (int i = 0; i < sub_ord.Length; i++)
         {
            var sub = sub_ord[i];

            if (sub.Kind == CTypeSubscriptKind.array)
            {
               if (i == 0) { sub.ParCount = 0; }
               else if (sub_ord[i - 1].Kind == CTypeSubscriptKind.pointer) { sub.ParCount = sub_ord[i - 1].ParCount - 1; }
               else { sub.ParCount = sub_ord[i - 1].ParCount; }
            }
         }

         var pns = sub_ord.Where(s => s.Kind == CTypeSubscriptKind.pointer).Reverse().ToArray();
         var ars = sub_ord.Where(s => s.Kind == CTypeSubscriptKind.array).ToArray();

         myRemoveSubItemRange(SubItems);
         myAddSubItemRange(pns.Concat(ars).Select(a => a.GetCopy()));
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="arraySubscriptsSizes"></param>
      public void AddSubScripts(params int[] arraySubscriptsSizes) => AddSubScripts(null, arraySubscriptsSizes);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="builtInSet"></param>
      /// <param name="arraySubscriptsSizes"></param>
      public void AddSubScripts(CTypeBuiltInSet? builtInSet, params int[] arraySubscriptsSizes) =>
         AddSubScripts(arraySubscriptsSizes.Select(s => CTypeSubscript.MakeArray(s, builtInSet)).ToArray());

      /// <summary>
      /// Add subscript in order of appereance of source code
      /// </summary>
      /// <param name="subscripts"></param>
      public void AddSubScripts(params CTypeSubscript[] subscripts)
      {
         foreach (var sub in subscripts)
         {
            if (sub.Kind == CTypeSubscriptKind.pointer)
            {
               if (Subscripts.Any(s => s.Kind == CTypeSubscriptKind.array))
               {
                  throw new Gate.CLanguage.CLangException($"Can't enqueue a pointer subscript to an array");
               }
            }
         }

         myCheckParenthesis(Subscripts.Concat(subscripts).ToArray());

         myAddSubItemRange(subscripts);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="subscript"></param>
      /// <param name="parCount"></param>
      public void AddSubscript(CTypeSubscript subscript, int parCount)
      {
         subscript.ParCount = parCount;
         AddSubScripts(subscript);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="subscript"></param>
      /// <returns></returns>
      public bool RemoveSubScript(CTypeSubscript subscript) => myRemoveSubItem(subscript);

      /// <summary>
      /// True when two subscript have all identical subscript.
      /// </summary>
      /// <param name="other"></param>
      /// <returns></returns>
      public bool IsEqualTo(CTypeSubscriptSet other) => SubscriptCount == other.SubscriptCount && Enumerable.Range(0, SubscriptCount).All(i => this[i].IsEqualTo(other[i]));

      /// <summary>
      /// True if compatible ie equal <see cref="CTypeSubscriptSet"/> are compatible and well 'int [][3]'<->'int [2][3]'
      /// </summary>
      /// <param name="other"></param>
      /// <returns></returns>
      public bool IsCompatibleWith(CTypeSubscriptSet other) =>
         this.SubscriptCount == other.SubscriptCount &&
         Enumerable.Range(0, this.SubscriptCount).All(i => this[i].IsCompatibleWith(other[i]));

      /// <summary>
      /// <br>True when two subscript have all similar subscript  <seealso cref="CTypeSubscript.IsSimilarTo(CTypeSubscript, bool)"/> </br> 
      /// <br>eg 'int *' is similar to 'int *const' </br>
      /// </summary>
      /// <param name="other"></param>
      /// <returns></returns>
      /// <see cref=""/>
      public bool IsSimilarTo(CTypeSubscriptSet other) =>
         SubscriptCount == other.SubscriptCount &&
         Enumerable.Range(0, SubscriptCount).
            All(i => this[i].IsSimilarTo(other[i], i == 0));

      /// <summary>
      /// 
      /// </summary>
      /// <param name="identifier"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      public string GetIdentifierDescriptor(string? identifier)
      {
         var sb = new StringBuilder();
         var pc = 0;

         foreach (var sub in Subscripts.Where(s => s.Kind == CTypeSubscriptKind.pointer))
         {
            if (sub.ParCount > pc)
            {
               sb.Append("(");

               pc = sub.ParCount;
            }
            else if (sub.ParCount < pc) { throw new Crash(); }

            sb.Append(sub.SubScriptStr);
         }

         sb.Append(identifier.ExtTrim());

         foreach (var sub in Subscripts.Where(s => s.Kind == CTypeSubscriptKind.array))
         {
            if (sub.ParCount < pc)
            {
               sb.Append(")");

               pc = sub.ParCount;
            }
            else if (sub.ParCount > pc) { throw new Crash(); }

            sb.Append(sub.SubScriptStr);
         }

         for (var i = 0; i < pc; i++) { sb.Append(')'); }

         return sb.ToString().Trim();
      }

      public void Clear() => myRemoveSubItemRange(Subscripts);

      public void Dereference() => RemoveSubScript(SubscriptsOrdered[0]);

      public void CopyFrom(CTypeSubscriptSet other)
      {
         Clear();
         AddSubScripts(other.Subscripts.Select(s => s.GetCopy()).ToArray());
      }

      private void myCheckParenthesis(CTypeSubscript[] typeSubscripts)
      {
         var cur_cnt = 0;

         foreach (var sub in Subscripts)
         {
            if (sub.Kind == CTypeSubscriptKind.pointer)
            {
               if (sub.ParCount < cur_cnt)
               {
                  throw new Gate.CLanguage.CLangException($"Pointer parenthesis count can't decrease!");
               }
            }
            else
            {
               if (sub.ParCount > cur_cnt)
               {
                  throw new Gate.CLanguage.CLangException($"Pointer parenthesis count can't increase!");
               }
            }

            cur_cnt = sub.ParCount;
         }
      }
   }
}
