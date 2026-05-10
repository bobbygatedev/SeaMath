using Gate.CLanguage.Expressions;
using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Types.BuiltIns;
using Gate.LangBase.Expressions;
using Gate.Tools;

namespace Gate.CLanguage.Types
{
   /// <summary>
   /// 
   /// </summary>
   public class CTypeSubscript : CItem, IComparable<CTypeSubscript>, ICTypeDescriptorGcc
   {
      private CTypeSubscriptKind myTypeSubScript = CTypeSubscriptKind.pointer;

      public CTypeSubscript() { }

      public static CTypeSubscript MakeArray(CExprStatement arraySizeExpr)
      {
         var ist = new CTypeSubscript();

         ist.Kind = CTypeSubscriptKind.array;
         ist.ArraySizeExpr = arraySizeExpr;

         return ist;
      }

      public static CTypeSubscript MakeArray(int size, CTypeBuiltInSet? typeBuiltIns = null)
      {
         var ce = new CExprStatement();

         ce.Expr = Expr.MakeLiteralConstant(new CRtmObjLiteral(size, new CTypeAlias(typeBuiltIns != null ? typeBuiltIns["int"] : new CTypeBinInt.Int("int", 4))));
        
         return MakeArray(ce);
      }

      public static CTypeSubscript MakePointer(CTypeQualifiersFlags typeQualifiers)
      {
         var inst = new CTypeSubscript();

         inst.Kind = CTypeSubscriptKind.pointer;
         inst.TypeQualifiers = typeQualifiers;

         return inst;
      }

      public static CTypeSubscript MakePointer() => MakePointer(0x0);

      /// <summary>
      /// 
      /// </summary>
      public override bool HasAssociatedPragma => false;

      /// <summary>
      /// 
      /// </summary>
      public CTypeSubscriptSet? ParentSet => ParentItem as CTypeSubscriptSet;

      /// <summary>
      /// 
      /// </summary>
      public override string Rebuilt => Descriptor;

      /// <summary>
      /// 
      /// </summary>
      public string Signature => Descriptor;

      /// <summary>
      /// 
      /// </summary>
      public int ParCount { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public CTypeQualifiersFlags TypeQualifiers { get; set; }

      /// <summary>
      /// (Constant) Array size has NO value if <see cref="IsIncomplete"/> (ie '[]').
      /// </summary>
      public int? ArraySizeConst => IsIncomplete ? 
         (IncompleteArraySize.HasValue ? IncompleteArraySize.Value : 0) : ArraySizeExpr?.ConstIntValue;

      /// <summary>
      /// 
      /// </summary>
      public CTypeSubscriptKind Kind
      {
         get => myTypeSubScript;

         set
         {
            switch (myTypeSubScript = value)
            {
               case CTypeSubscriptKind.pointer:
                  ArraySizeExpr = null;
                  break;

               case CTypeSubscriptKind.array:
                  ArraySizeExpr = null;
                  break;

               case CTypeSubscriptKind.reference:
                  throw new NotImplementedException();

               default: throw new Crash();
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public string DescriptorGcc
      {
         get
         {
            switch (Kind)
            {
               case CTypeSubscriptKind.pointer: return "P";
               case CTypeSubscriptKind.reference: throw new NotImplementedException();
               case CTypeSubscriptKind.array: return (ArraySizeConst >= 0) ? string.Format("A{0}_", ArraySizeConst) : "A";
               default: throw new Crash();
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public string SubScriptStr
      {
         get
         {
            switch (Kind)
            {
               case CTypeSubscriptKind.pointer: return $"*{((TypeQualifiers != 0) ? TypeQualifiers.ToString().Replace("|", " ") : "")}";
               case CTypeSubscriptKind.array: return IsIncomplete ? (IncompleteArraySize.HasValue ? $"[({IncompleteArraySize})]" : $"[]") : $"[{ArraySizeConst}]";
               case CTypeSubscriptKind.reference: throw new NotImplementedException();
               default: throw new Crash();
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override string Descriptor => myParWrap(SubScriptStr);

      /// <summary>
      /// 
      /// </summary>
      public CExprStatement? ArraySizeExpr
      {
         get => SubItems.FirstOrDefault(i => i is CExprStatement) as CExprStatement;

         set
         {
            myRemoveSubItem(ArraySizeExpr);

            if (value != null)
            {
               myTypeSubScript = CTypeSubscriptKind.array;
               myAddSubItem(value);
            }
         }
      }

      /// <summary>
      /// Incomplete is an array subscript whose FIRST size is not specified eg 'int []' 'int [][3]' '' 
      /// </summary>
      public bool IsIncomplete => Kind == CTypeSubscriptKind.array && ArraySizeExpr == null;

      /// <summary>
      /// 
      /// </summary>
      public int? IncompleteArraySize { get; set; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="other"></param>
      /// <returns></returns>
      public bool IsEqualTo(CTypeSubscript other)
      {
         if (Kind == other.Kind)
         {
            switch (Kind)
            {
               case CTypeSubscriptKind.reference:
               case CTypeSubscriptKind.pointer: return TypeQualifiers == other.TypeQualifiers;
               case CTypeSubscriptKind.array: return ArraySizeConst == other.ArraySizeConst;

               default: throw new Crash();
            }
         }

         return false;
      }

      /// <summary>
      /// <br> Two <see cref="CTypeSubscript"/> are compatible if: </br>
      /// <br> - Are equal </br>
      /// <br> - One from them is incomplete </br>
      /// </summary>
      /// <param name="other"></param>
      /// <returns></returns>
      public bool IsCompatibleWith(CTypeSubscript other) => IsEqualTo(other) || other.IsIncomplete || IsIncomplete;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="other"></param>
      /// <param name="isFirst"></param>
      /// <returns></returns>
      public bool IsSimilarTo(CTypeSubscript other, bool isFirst)
      {
         var ss = new[] { this, other };

         if (Kind == other.Kind)
         {
            switch (Kind)
            {
               case CTypeSubscriptKind.pointer: return true;
               case CTypeSubscriptKind.reference: throw new NotImplementedException();
               case CTypeSubscriptKind.array: return this.ArraySizeConst == other.ArraySizeConst;

               default: throw new Crash();
            }
         }
         else if (isFirst && ss.Any(s => s.Kind == CTypeSubscriptKind.array) && ss.Any(s => s.Kind == CTypeSubscriptKind.pointer))
         {
            return true;
         }

         return false;
      }

      /// <summary>
      /// <br> Returns ordering idx for usage in creation of signature criteria: </br>
      /// <br> - Parenthesis count (ascending). </paraZ>
      /// <br> - Array as precedence </br>
      /// <br> - Order of appereance.</br>
      /// </summary>
      /// <param name="other"></param>
      /// <returns></returns>
      public int CompareTo(CTypeSubscript? other)
      {
         var cmp_res = -ParCount.CompareTo(other?.ParCount);
         var idx = -1;
         var idx_oth = -1;

         if (ParentSet != null && ParentSet == other?.ParentSet)
         {
            idx = ParentSet.Subscripts.ToList().IndexOf(this);
            idx_oth = ParentSet.Subscripts.ToList().IndexOf(other);
         }

         if (cmp_res != 0) { return cmp_res; }
         else if (Kind == CTypeSubscriptKind.pointer && other?.Kind == CTypeSubscriptKind.array) { return +1; }
         else if (Kind == CTypeSubscriptKind.array && other?.Kind == CTypeSubscriptKind.pointer) { return -1; }
         else if (Kind == CTypeSubscriptKind.array && other?.Kind == CTypeSubscriptKind.array) { return idx.CompareTo(idx_oth); }
         else if (Kind == CTypeSubscriptKind.pointer && other?.Kind == CTypeSubscriptKind.pointer) { return idx_oth.CompareTo(idx); }
         else { throw new Crash(); }
      }

      public CTypeSubscript GetCopy()
      {
         var cpy = new CTypeSubscript();

         cpy.ParCount = ParCount;
         cpy.IncompleteArraySize = IncompleteArraySize;

         switch (cpy.Kind = Kind)
         {
            case CTypeSubscriptKind.pointer:
               cpy.TypeQualifiers = TypeQualifiers;
               return cpy;

            case CTypeSubscriptKind.array: //just the size (not expression) is copied
               cpy.ArraySizeExpr = ArraySizeExpr?.GetCopy();
               return cpy;

            case CTypeSubscriptKind.reference: throw new NotImplementedException();//todo future

            default: throw new Crash();
         }
      }

      private string myParWrap(string str)
      {
         var res = str;

         for (int i = 0; i < ParCount; i++) { res = $"({res})"; }

         return res;
      }
   }
}
