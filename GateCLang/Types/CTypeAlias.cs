using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Expressions.Nodes;
using Gate.CLanguage.Types.BuiltIns;
using Gate.LangBase.Runtime;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Arry;
using Gate.Tools.Text;
using System.Runtime.InteropServices;

namespace Gate.CLanguage.Types
{
   /// <summary>
   /// <br>Descriptor class for type alias</br> 
   /// <br>Type alias describes entirely a C-Type (type-name array-pointer--) eg 'int* [2]' is a type alias. </br>
   /// </summary>
   public class CTypeAlias : CType
   {
      private CTypeQualifiersFlags myInternalTypeQualifiers = CTypeQualifiersFlags.none;
      private readonly InnerTypeVisitor myTypeBaseVisitor = new InnerTypeVisitor();

      /// <summary>
      /// Used in case <see cref="CTypeAlias.PrimitiveAlias"/> in order to not need to clone, such as <see cref="CTypeAlias"/> can't be modified.
      /// </summary>
      private CTypeFunctionContainer? myFunctionContainerNotOwned;

      /// <summary>
      /// 
      /// </summary>
      public CTypeAlias(CType? typeBase = null)
      {
         myAddSubItem(TypeSubscriptSet = new CTypeSubscriptSet());
         TypeBase = typeBase;
      }

      public static CTypeAlias Make(CType typeBase, params int[] sizes)
      {
         var ali = new CTypeAlias(typeBase);

         ali.TypeSubscriptSet?.AddSubScripts(sizes);

         return ali;
      }

      private class InnerTypeVisitor
      {
         private CType? myType;

         public CType? GetTypeBase(CTypeAlias typeAlias) =>
            typeAlias.ParentItem == null ? myType : (CType)myGetTypeBase((dynamic)typeAlias.ParentItem, typeAlias);

         public void SetTypeBase(CTypeAlias typeAlias, CType? type)
         {
            if (typeAlias.ParentItem == null) { myType = type; }
            else { mySetTypeBase((dynamic)typeAlias.ParentItem, typeAlias, type); }
         }

         /// <summary>
         /// Not valid parent item type.
         /// </summary>
         /// <param name="parentItem"></param>
         /// <param name="typeAlias"></param>
         /// <returns></returns>
         /// <exception cref="Crash"></exception>
         private CType? myGetTypeBase(CItem parentItem, CTypeAlias typeAlias) => throw new Crash();

         /// <summary>
         /// 
         /// </summary>
         /// <param name="parentDecl"></param>
         /// <param name="typeAlias"></param>
         /// <returns></returns>
         private CType? myGetTypeBase(CDecl parentDecl, CTypeAlias typeAlias) => parentDecl.DeclSpecifiers?.TypeBase;

         /// <summary>
         /// Return type-base is same of <see cref="CTypeFunctionContainer.TypeAliasDeclaration"/> 
         /// </summary>
         /// <param name="parentFunctionParams"></param>
         /// <param name="typeAlias"></param>
         /// <returns></returns>
         private CType? myGetTypeBase(CTypeFunctionContainer parentFunctionParams, CTypeAlias typeAlias) =>
            parentFunctionParams.TypeAliasDeclaration?.TypeBase;

         private CType? myGetTypeBase(CExprNodeTypeName parentExprNodeTypeName, CTypeAlias typeAlias) => myType;

         /// <summary>
         /// Unbound <see cref="CTypeAlias"/> uses <see cref="CTypeAlias.InnerTypeVisitor.myType"/> for storing type.
         /// </summary>
         /// <param name="parentItem"></param>
         /// <param name="typeAlias"></param>
         /// <param name="typeBase"></param>
         /// <exception cref="Crash"></exception>
         /// <exception cref="NotImplementedException"></exception>
         private void mySetTypeBase(CItem parentItem, CTypeAlias typeAlias, CType typeBase) =>
            myType = parentItem == null ? typeBase : throw new Crash();

         /// <summary>
         /// <see cref="CExprNodeTypeName"/> always uses 
         /// </summary>
         /// <param name="parentExprNodeTypeName"></param>
         /// <param name="typeAlias"></param>
         /// <param name="typeBase"></param>
         private void mySetTypeBase(CExprNodeTypeName parentExprNodeTypeName, CTypeAlias typeAlias, CType typeBase) => myType = typeBase;
      }

      public CType? TypeBase
      {
         get => myTypeBaseVisitor.GetTypeBase(this);
         set => myTypeBaseVisitor.SetTypeBase(this, value);
      }

      /// <summary>
      /// 
      /// </summary>
      public CDecl? DeclBound => ParentItem as CDecl;

      /// <summary>
      /// If alias is associated with a typedef <see cref="CDeclTypedef"/>.Identifier othewrise null.
      /// </summary>
      public override string? Identifier => ParentItem is CDeclTypedef tdf ? tdf.Identifier : null;

      /// <summary>
      /// 
      /// </summary>
      public override string Rebuilt => $"{(Identifier != null ? Identifier + $"({myGetSpecifier()})" : myGetSpecifier())}";

      /// <summary>
      /// 
      /// </summary>
      public override bool IsReference => TypeSubscriptSet?.Subscripts.Any(s => s.Kind == CTypeSubscriptKind.reference) ?? false;

      /// <summary>
      /// <br>Type qualifiers of <see cref="CDeclSpecifiers"/> if this is bound to <see cref="CDecl"/> otherwise internal value.</br>
      /// </summary>
      /// <exception cref="Gate.CLanguage.CLangException"></exception>
      public CTypeQualifiersFlags TypeQualifiers
      {
         get
         {
            if (ParentItem is CDecl dcl)
            {
               return dcl.DeclSpecifiers != null ?
                  dcl.DeclSpecifiers.TypeQualifiers :
                  throw new Gate.CLanguage.CLangException($"Decl specifier not set in {dcl.GetType().Name} '{dcl.Descriptor}'");
            }
            else { return myInternalTypeQualifiers; }
         }

         set
         {
            if (ParentItem is CDecl dcl)
            {
               throw new Gate.CLanguage.CLangException($"Can't set {GetType().Name}.TypeQualifiers when bound to {typeof(CDecl).Name}");
            }
            else { myInternalTypeQualifiers = value; }
         }
      }

      /// <summary>
      /// <br> - typedef expanded type if <see cref="TypeBase"/> is of type <see cref="CTypeAlias"/> </br>
      /// <br> -----eg 'typedef int V[3];  V a[2];' <see cref="PrimitiveAlias"/> will be 'int [2][3]' </br>
      /// <br> - a cloned copy is otw</br>
      /// </summary>
      public CTypeAlias PrimitiveAlias
      {
         get
         {
            var pri_ali = null as CTypeAlias;

            if (FunctionContainer != null)
            {
               var pri_ret = FunctionContainer?.TypeAliasReturned?.PrimitiveAlias ?? throw new Crash();

               pri_ali = new CTypeAlias(pri_ret.TypeBase);
               pri_ali.SetAsFunction(true);
               (pri_ali.FunctionContainer ?? throw new Crash()).ReturnTypeSubscript?.CopyFrom(pri_ret.TypeSubscriptSet);
               pri_ali.TypeQualifiers = TypeQualifiers;
               pri_ali.TypeSubscriptSet.CopyFrom(TypeSubscriptSet);
               pri_ali.FunctionContainer.HasVarArgs = FunctionContainer.HasVarArgs;

               foreach (var par in FunctionContainer.Parameters)
               {
                  var ds = new CDeclSpecifiers();
                  var cp = new CDeclVar();
                  var par_t = par?.TypeAlias?.PrimitiveAlias ?? throw new Crash();

                  ds.AddDecl(cp);
                  ds.TypeBase = par_t?.TypeBase;
                  cp.Identifier = par.Identifier;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                  cp.TypeAlias.TypeSubscriptSet.CopyFrom(par_t.TypeSubscriptSet);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                  pri_ali.FunctionContainer.AddParameter(null, cp);
               }

               return pri_ali;
            }
            else if (TypeBase is CTypeAlias par_ali)
            {
               var par_ali_pri = par_ali.PrimitiveAlias;

               pri_ali = new CTypeAlias(par_ali_pri.TypeBase);

               var sbs = TypeSubscriptSet.SubscriptsOrdered.Concat(par_ali_pri.PrimitiveAlias.TypeSubscriptSet.SubscriptsOrdered).ToArray();

               pri_ali.TypeQualifiers |= par_ali.TypeQualifiers;
               pri_ali.TypeSubscriptSet.AddSubscriptsOrdered(sbs.Select(s => s.GetCopy()).ToArray());
               pri_ali.myFunctionContainerNotOwned = par_ali_pri.FunctionContainer;
            }
            else { pri_ali = GetCopy(); }//me-self

            if (pri_ali.TypeSubscriptSet.IsIncompleteArray)
            {
               var ini = ParentDecl is CDeclVar var ? var.OwnedInit : null;

               if (ini != null && ini.IncompleteArraySize.HasValue)
               {
                  pri_ali.TypeSubscriptSet.ArraySubscriptsOrdered[0].IncompleteArraySize = ini.IncompleteArraySize;
               }
            }

            //complete type
            if (pri_ali.TypeBase is CTypeIncomplete inc && inc.CompleteType != null)
            {
               pri_ali.TypeBase = inc.CompleteType;
            }

            return pri_ali;
         }
      }

      public CTypeAlias GetCopy()
      {
         var cpy = new CTypeAlias(TypeBase);

         cpy.TypeQualifiers = TypeQualifiers;
         cpy.TypeSubscriptSet.CopyFrom(TypeSubscriptSet);
         cpy.myFunctionContainerNotOwned = FunctionContainer;

         return cpy;
      }

      /// <summary>
      /// 
      /// </summary>
      public CDecl? ParentDecl => ParentItem as CDecl;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsUserDefined => false;

      /// <summary>
      /// <see cref="TypeBase"/> is <see cref="CTypeIncomplete"/> (eg 'struct MyStruct a;') and a <see cref="CTypeIncomplete.CompleteType"/> is null.
      /// </summary>
      public override bool IsIncompleteType => PrimitiveAlias.TypeBase is CTypeIncomplete inc && inc.CompleteType == null;

      /// <summary>
      /// True if I'm a pure scalar alias of a class/union/structType (even constant).
      /// </summary>
      public override bool IsClass => PrimitiveAlias.IsPrimitive && (PrimitiveAlias.TypeBase?.IsClass ?? false);

      /// <summary>
      /// True if I'm a pure scalar alias of a built-in type (even constant).
      /// </summary>
      public override bool IsBuiltIn => PrimitiveAlias.IsPrimitive && (PrimitiveAlias.TypeBase?.IsBuiltIn ?? false);

      /// <summary>
      /// Whether it is an array type (eg 'int * ,int (*)[5]' );
      /// </summary>
      public bool IsPointer => PrimitiveAlias.TypeSubscriptSet.IsPointer;

      /// <summary>
      /// Whether it is an array type (eg 'int [5] ,int *[5]' );
      /// </summary>
      public bool IsArray => PrimitiveAlias.TypeSubscriptSet.IsArray;

      /// <summary>
      /// True if I'm a pure scalar alias of a n enumerative type (even constant).
      /// </summary>
      public override bool IsEnum
      {
         get
         {
            var pri_ali = PrimitiveAlias;

            return pri_ali.IsPrimitive && pri_ali.TypeBase != null && pri_ali.TypeBase.IsEnum;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override string TypeSpecifier => Identifier ?? myGetSpecifier();

      /// <summary>
      /// 
      /// </summary>
      public override string Descriptor => $"Alias: {Rebuilt}";

      /// <summary>
      /// CSharp type used for memory storage which is Primitive/struct type for built-in and IntPtr
      /// </summary>
      public override Type? CSharpTypeForStorage => IsPointer || IsArray ? typeof(IntPtr) : PrimitiveAlias?.TypeBase?.CSharpTypeForStorage;

      /// <summary>
      /// <br> If <see cref="IsBuiltIn"/> equal to <see cref="PrimitiveAlias"/>.TypeBase  </br>
      /// <br> If <see cref="IsEnum"/> equal to underlying int type (always int in C), otherwise null. </br>
      /// </summary>
      public override CTypeBuiltIn? BuiltIn
      {
         get
         {
            if (IsBuiltIn) { return PrimitiveAlias.TypeBase as CTypeBuiltIn; }
            else if (IsEnum) { return (PrimitiveAlias.TypeBase as CTypeEnum)?.UnderlyingIntType; }
            else { return null; }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public CTypeSubscriptSet TypeSubscriptSet { get; }

      /// <summary>
      /// 
      /// </summary>
      public override string DescriptorGcc => $"{TypeSubscriptSet?.DescriptorGcc}_{TypeBase?.DescriptorGcc}";

      /// <summary>
      /// Whether the type is purely scalar (neither an array nor a pointer).
      /// </summary>
      public bool IsPrimitive => PrimitiveAlias.TypeSubscriptSet.Subscripts.Length == 0;

      /// <summary>
      /// Whether the type is a (simple) scalar (ie could be pure built-in and pointer, but neither an array nor a class ).
      /// </summary>
      public bool IsScalar => IsBuiltIn || IsEnum || PrimitiveAlias.TypeSubscriptSet.IsPointer;

      /// <summary>
      /// True is built-in(enum) and it's representation is int or float or complex.
      /// </summary>
      public bool IsNumeric
      {
         get
         {
            var bin = BuiltIn;

            if (bin != null)
            {
               switch (bin.RepresentedType)
               {
                  case CTypeBuiltInRepresent.integer:
                  case CTypeBuiltInRepresent.floating_point:
                  case CTypeBuiltInRepresent.complex_float:
                  case CTypeBuiltInRepresent.complex_int:
                     return true;
               }
            }

            return false;
         }
      }

      /// <summary>
      /// True when is not anonimous (ie in case of typedef interior to a CDecl).
      /// </summary>
      public override bool IsDefinition => Identifier != null;

      /// <summary>
      /// <br> True if <see cref="PrimitiveAlias"/>:</br>
      /// <br> - is a pointer and first (ordered) subscript is constant (eg 'int**const' is constant but 'int*const*' and 'const int*' are not)</br>
      /// <br> - is pure scalar with base type constant (eg 'const int a = 2;' </br>
      /// <br> - array is never constant </br>
      /// </summary>
      public override bool IsConstant
      {
         get
         {
            var pri_ali = PrimitiveAlias;

            if (pri_ali?.TypeSubscriptSet?.IsPointer ?? false) { return (pri_ali.TypeSubscriptSet.SubscriptsOrdered[0].TypeQualifiers & CTypeQualifiersFlags.@const) != 0; }
            else if (pri_ali?.TypeSubscriptSet?.IsArray ?? false) { return false; }
            else { return (pri_ali?.TypeQualifiers & CTypeQualifiersFlags.@const) != 0; }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public int[]? ArraySizesConst => TypeSubscriptSet?.ArraySizesConst;

      /// <summary>
      /// <br>  the type when object is dereferenced (equivalent to typeof(*o), where o is an object of CTypeAlias type). If type is not a pointer gets null.</br>
      /// <br> ie 'int* [3]' not a pointer </br>
      /// <br> ie 'int (*[3])' pointer type is 'int [3]'</br>
      /// </summary>
      public CTypeAlias? DereferencedType
      {
         get
         {
            if (IsFunctionPointer) { return GetCopy(); }
            else if (IsFunction)
            {
               var cpy = GetCopy();

               (cpy?.TypeSubscriptSet ?? throw new Gate.LangBase.Runtime.RtmException()).AddSubscript(CTypeSubscript.MakePointer(), 0);

               return cpy;
            }
            else if (TypeSubscriptSet?.Subscripts.Length > 0)
            {
               var ali = GetCopy();

               //top subscript eg 'int *a[2]' => int *';
               (ali.TypeSubscriptSet ?? throw new Gate.LangBase.Runtime.RtmException()).Dereference();

               return ali;
            }
            else { return null; }
         }
      }

      /// <summary>
      /// Type ==> *type ie int [2] ==> int (*)[2];
      /// </summary>
      public CTypeAlias AddressOfType
      {
         get
         {
            var pri_ali = PrimitiveAlias;
            var ss_ord = (pri_ali.TypeSubscriptSet ?? throw new Gate.LangBase.Runtime.RtmException()).SubscriptsOrdered;

            pri_ali.TypeSubscriptSet.Clear();
            pri_ali.TypeSubscriptSet.AddSubscriptsOrdered(new[] { CTypeSubscript.MakePointer() }.Concat(ss_ord).ToArray());

            return pri_ali;
         }
      }

      /// <summary>
      /// <br> Equivalent pointer type ie:</br>
      /// <br>If alias is a pointer return this.</br>
      /// <br>If alias is an array return pointer type eg 'int [2]'=> 'int*' 'int [2][3]'=> 'int (*[3]) pointer to int[3]' .</br>
      /// <br>If <see cref="TypeSubscriptSet"/> has Count > 0 get <see cref="PrimitiveAlias"/></br> 
      /// </summary>
      public CTypeAlias? EquivalentPointerType
      {
         get
         {
            if (IsPointer) { return this; }
            else if (IsArray) { return DereferencedType?.AddressOfType ?? throw new Gate.LangBase.Runtime.RtmException(); }
            else if (TypeBase is CTypeAlias) { return PrimitiveAlias.EquivalentPointerType; }
            else { return null; }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public bool IsShortChar
      {
         get
         {
            var pri_typ = PrimitiveAlias;

            return
               pri_typ.TypeBase is CTypeBinChar &&
               (pri_typ.TypeSubscriptSet ?? throw new Gate.LangBase.Runtime.RtmException()).SubscriptCount == 0;
         }
      }

      /// <summary>
      /// <see cref="CTypeFunctionContainer"/>, if null typealias is not a function.
      /// </summary>
      public CTypeFunctionContainer? FunctionContainer => myFunctionContainerNotOwned ?? SubItems.OfType<CTypeFunctionContainer>().FirstOrDefault();

      /// <summary>
      /// Set type alias as function type. If already a function does nothing.
      /// </summary>
      /// <exception cref="Gate.CLanguage.CLangException"></exception>
      public void SetAsFunction(bool isFunction)
      {
         if (isFunction)
         {
            if (myFunctionContainerNotOwned != null)
            {
               throw new Gate.CLanguage.CLangException(
                  $"Can't set as function a CTypeAlias which already owns an not-owned-function-container");
            }
            else if (FunctionContainer == null)
            {
               myAddSubItem(new CTypeFunctionContainer());
            }
         }
         else
         {
            myFunctionContainerNotOwned = null;
            myRemoveSubItem(SubItems.OfType<CTypeFunctionContainer>().FirstOrDefault());
         }
      }

      /// <summary>
      /// <see cref="CTypeAlias"/> represents a pure function pointer (neither an array, nor a pointer-of-pointer)
      /// </summary>
      public bool IsFunctionPointer => FunctionContainer != null && TypeSubscriptSet.IsPointer && TypeSubscriptSet.SubscriptCount == 1;

      /// <summary>
      /// <see cref="CTypeAlias"/> represents a pure function (not a function pointer).
      /// </summary>
      public bool IsFunction => FunctionContainer != null && TypeSubscriptSet.SubscriptCount == 0;

      /// <summary>
      /// 
      /// </summary>
      public override int SizeOf
      {
         get
         {
            if (IsPointer) { return Marshal.SizeOf(typeof(IntPtr)); }
            else
            {
               var pri_ali = PrimitiveAlias;

               if (pri_ali.IsIncompleteType) { return -1; }
               else
               {
                  return
                     (pri_ali?.TypeBase ?? throw new Gate.LangBase.Runtime.RtmException()).SizeOf *
                     (pri_ali.TypeSubscriptSet ?? throw new Gate.LangBase.Runtime.RtmException()).TotalItems;
               }
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public bool IsTypedef => ParentItem is CDeclTypedef;

      /// <summary>
      /// Gets the element type of the innermost array dimension represented by this type alias.
      /// </summary>
      /// <remarks>Use this property to determine the type of items contained within a multi-dimensional or
      /// nested array type. If this type alias does not represent an array, the property returns the type alias
      /// itself.</remarks>
      public CTypeAlias? ArrayItemType
      {
         get
         {
            var ord_sbs = TypeSubscriptSet?.SubscriptsOrdered ?? throw new Gate.LangBase.Runtime.RtmException();
            var ord_sbs_arr = ord_sbs.TakeWhile(o => o.Kind == CTypeSubscriptKind.array).ToArray();
            var itm_pri_ali = this;

            foreach (var _ in ord_sbs_arr) { itm_pri_ali = itm_pri_ali?.DereferencedType; }

            return itm_pri_ali;
         }
      }

      /// <summary>
      /// <br> Array of tuple (subscript,CType) of all scalar item + plus <see cref="CType"/> in memory appereance order. </br>
      /// <br> eg struct { int f1; float f2; }[2][3]: </br>
      /// <br> [0][0].f1 , int </br>
      /// <br> [0][0].f2 , float </br>
      /// <br> [0][1].f1 , int </br>
      /// <br> [0][1].f2 , float </br>
      /// <br> [0][2].f1 , int </br>
      /// <br> [0][2].f2 , float </br>
      /// <br> [1][0].f1 , int </br>
      /// <br> [1][0].f2 , float </br>
      /// <br> [1][1].f1 , int </br>
      /// <br> [1][1].f2 , float </br>
      /// <br> [1][2].f1 , int </br>
      /// <br> [1][2].f2 , float </br>
      /// </summary>
      public (CDeclSubscriptIndices indices, CTypeAlias type)[] ScalarSubscriptAndType
      {
         get
         {
            var ord_sbs = TypeSubscriptSet?.SubscriptsOrdered ?? throw new Gate.LangBase.Runtime.RtmException();
            var ord_sbs_arr = ord_sbs.TakeWhile(o => o.Kind == CTypeSubscriptKind.array).ToArray();

            var enu = new ArrayIndicesEnumerable(
               ArrayIndicesEnumerable.DirectionId.right2left,
               [.. Enumerable.Range(0, ord_sbs_arr.Length).Select(i => ord_sbs_arr[i].ArraySizeConst ?? -1)]);

            var ids = enu.Select(id => new CDeclSubscriptIndices(id.Cast<object>().ToArray())).ToArray();
            var lst = new List<(CDeclSubscriptIndices, CTypeAlias)>();
            var ali_bas = new CTypeAlias(ArrayItemType).PrimitiveAlias;

            if (ids.Length == 0)
            {
               ids = [new CDeclSubscriptIndices()];//scalar has an empty coefficient
            }

            if (ali_bas.TypeBase is ITypeClass cls)
            {
               //type is scalar struct/class/union
               foreach (var id in ids)
               {
                  foreach (var fld in cls.Fields)
                  {
                     foreach (var ss_typ in fld.TypeAlias.ScalarSubscriptAndType)
                     {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
#pragma warning disable CS8604 // Possible null reference argument.
                        lst.Add((id + fld?.Identifier + ss_typ.Item1, fld.TypeAlias.ArrayItemType));
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                     }
                  }
               }
            }
            else
            {
               //array of scalar(not struct/class/union)
               lst.AddRange(ids.Select(i => (i, ali_bas)));
            }

            return lst.ToArray();
         }
      }

      public CTypeAlias? StringTypeAlias
      {
         get
         {
            if (TypeSubscriptSet?.ArraySubscriptsOrdered.Length == 0)
            {
               return null;
            }
            else
            {
               var tmp = this;

               while (tmp?.TypeSubscriptSet?.ArraySizes?.Length > 1)
               {
                  tmp = tmp.DereferencedType;
               }

               return tmp;
            }
         }
      }

      public override int[] ArraySizes => TypeSubscriptSet.ArraySizesConst;

      public override Type? CSharpArrayItemType => ArrayItemType?.CSharpTypeForStorage;

      /// <summary>
      /// <br> Two types are similar when have same primitive type and similar subscript set,</br>
      /// <br> ie same pointer signature but different type qualifier (eg 'int* const[2]' is similar to 'int*[2]').</br>
      /// </summary>
      /// <param name="other"></param>
      /// <returns></returns>
      public bool IsSimilar(CTypeAlias other) =>
        PrimitiveAlias.TypeBase?.Signature == other.PrimitiveAlias?.TypeBase?.Signature &&
        PrimitiveAlias.TypeSubscriptSet.IsSimilarTo(other.PrimitiveAlias?.TypeSubscriptSet ?? throw new RtmException());

      /// <summary>
      /// 
      /// </summary>
      /// <param name="other"></param>
      /// <returns></returns>
      public bool IsEqual(CTypeAlias other)
      {
         var pri = PrimitiveAlias;
         var pri_oth = other.PrimitiveAlias;

         return
            pri.Signature == pri_oth.TypeBase?.Signature &&
            pri.TypeSubscriptSet.IsEqualTo(pri_oth.TypeSubscriptSet);
      }

      /// <summary>
      /// Can assign to <paramref name="lType"/>?
      /// </summary>
      /// <param name="lType"></param>
      /// <param name="isInitOrCast">If true assign is in init/cast context</param>
      /// <param name="language"></param>
      /// <returns></returns>
      public bool CanAssignTo(CTypeAlias lType, bool isInitOrCast, CLanguage language, RtmObjStrategyAssignContext assignContext) =>
         language == CLanguage.c ? CCanAssignTo(lType, assignContext) : CppCanAssignTo(lType, assignContext);

      /// <summary>
      /// Can assign to <paramref name="lType"/> ?(C++ version)
      /// </summary>
      /// <param name="lType"></param>
      /// <param name="isInitOrCast">If true assign is in init/cast context</param>
      /// <returns></returns>
      /// <exception cref="NotImplementedException"></exception>
      public bool CppCanAssignTo(CTypeAlias lType, RtmObjStrategyAssignContext assignContext) => 
         throw new NotImplementedException();//todo cpp

      /// <summary>
      /// Can assign to <paramref name="lType"/> ?(C version)
      /// </summary>
      /// <param name="lType"></param>
      /// <param name="isInitOrCast">If true assign is in init/cast context</param>
      /// <returns></returns>
      public bool CCanAssignTo(CTypeAlias lType, RtmObjStrategyAssignContext assignContext)
      {
         if (
            !(assignContext == RtmObjStrategyAssignContext.var_init ||
            assignContext == RtmObjStrategyAssignContext.cast) && lType.IsConstant && !IsConstant)
         {
            return false;
         }

         var pri_als = new[] { lType.PrimitiveAlias, PrimitiveAlias };

         if (pri_als.All(p => p.IsClass))
         {
            var css = pri_als.Select(p => TypeBase as ITypeClass ?? throw new Crash()).ToArray();

            return css[0].Kind == css[1].Kind && css[0].TypeSpecifier == css[1].TypeSpecifier;
         }
         else if (pri_als.All(p => p.IsScalar))
         {
            if (pri_als[0].IsFunctionPointer && pri_als[1].IsFunction) { return true; }
            else
            {
               var is_ass = //may assign
                  pri_als.All(t => t.BuiltIn != null) ||//both built-in(enum)
                  pri_als.All(t => t.IsPointer) || //both pointer
                  pri_als.Any(t => t.IsPointer) && pri_als.Any(t => t.IsInteger) ||//pointer-to-int or int-to-pointer
                  ((pri_als[0].IsPointer || pri_als[1].IsInteger) && pri_als[1].IsArray);

               return is_ass;
            }
         }
         else if (pri_als[0].IsPointer)
         {
            return pri_als[1].IsPointer || pri_als[1].IsArray || pri_als[1].IsInteger;
         }
         else if (pri_als[0].IsArray)
         {
            return pri_als[1].IsPointer || pri_als[1].IsArray && assignContext == RtmObjStrategyAssignContext.function_param;
         }
         else
         {
            return false;
         }
      }

      protected override void myActionOnParentReset(HierarchicalItem parentItem) => throw new Crash();

      protected override void myActionOnParentSet(HierarchicalItem parentItem)
      {
         if (!(parentItem is CDecl || parentItem is CExprNodeTypeName || parentItem is CTypeFunctionContainer))
         {
            throw new Crash($"{parentItem.GetType().Name} not allowed for {GetType().Name}");
         }

         base.myActionOnParentSet(parentItem);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      private string myGetSpecifier()
      {
         if (FunctionContainer != null)
         {
            var ret = FunctionContainer?.TypeAliasReturned?.TypeSpecifier;
            var id = TypeSubscriptSet.GetIdentifierDescriptor(null);

            //in this case is a pointer or pointer-of-pointer to function
            if (TypeSubscriptSet.SubscriptCount > 0)
            {
               var ts = new CTypeSubscriptSet();

               ts.CopyFrom(TypeSubscriptSet);
               ts.Dereference();

               return $"{ret}(*){FunctionContainer?.Rebuilt}{(ts.SubscriptCount > 0 ? $" {ts}" : "")}";
            }
            else
            {
               return $"{ret}{FunctionContainer?.Rebuilt}";
            }
         }
         else
         {
            return $"{TypeBase?.TypeSpecifier}{TypeSubscriptSet.GetIdentifierDescriptor(null)}".Trim();
         }
      }
   }
}
