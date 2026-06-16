using Gate.CLanguage.Decl;
using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Standards;
using Gate.CLanguage.Types;
using Gate.LangBase;
using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.DesignPattern;
using Gate.Tools.Extensions;

namespace Gate.CLanguage.Runtime
{
   /// <summary>
   /// Provides an implementation of the <see cref="ICRtmObjStrategy"/> interface, offering functionality for creating,
   /// managing, and manipulating runtime objects (CRtmObj) based on various declaration types and contexts.
   /// </summary>
   /// <remarks>This class is responsible for creating runtime objects (CRtmObj) from declarations, type
   /// aliases, and other inputs. It supports operations such as creating constants, dereferencing pointers, assigning
   /// values, and managing array and record members. The class also provides mechanisms for handling function
   /// parameters and other runtime object strategies. <para> The <see cref="CRtmObjStrategy"/> class uses an internal
   /// object maker to construct runtime objects based on the provided declarations and types. It also provides
   /// extensibility points for numeric conversion and operator modification. </para></remarks>
   public class CRtmObjStrategy : BaseClassWithFinalizer, ICRtmObjStrategy
   {
      private readonly InnerObjectMaker myObjectMaker;

      /// <summary>
      /// Constructor that initializes the runtime object strategy with built-in types and an optional numeric converter.
      /// </summary>
      /// <param name="builtIns"></param>
      public CRtmObjStrategy(INumericConverter? numericConverter = null)
      {
         Allocator = myMakeAllocator();
         myObjectMaker = new InnerObjectMaker(this);
         NumericConverter = numericConverter ?? new CLangNumericConverterStandard();
      }

      private class InnerObjectMaker
      {
         public InnerObjectMaker(CRtmObjStrategy parent) => Parent = parent;

         public CRtmObjStrategy Parent { get; }

         public CRtmObj Make(CDecl decl, CRtmObjAllocator allocator)
         {
            var pri_ali = decl.TypeAlias.PrimitiveAlias;

            if (pri_ali.IsClass) { return new CRtmObjRecord(Parent, decl); }
            else if (pri_ali.IsBuiltIn) { return new CRtmObjScalar(allocator, decl); }
            else if (pri_ali.IsFunctionPointer) { return new CRtmObjPointerFunction(Parent, decl); }
            else if (pri_ali.IsPointer) { return new CRtmObjPointer(Parent, decl); }
            else if (pri_ali.IsArray) { return new CRtmObjArray(Parent, decl); }
            else { throw new Crash($"{pri_ali} is not valid!"); }
         }

         public CRtmObj Make(CTypeAlias typeAlias, CRtmObjAllocator allocator)
         {
            var pri_ali = typeAlias.PrimitiveAlias;

            if (pri_ali.IsClass) { return new CRtmObjRecord(Parent, typeAlias); }
            else if (pri_ali.IsBuiltIn) { return new CRtmObjScalar(allocator, typeAlias); }
            else if (pri_ali.IsPointer) { return new CRtmObjPointer(Parent, typeAlias); }
            else if (pri_ali.IsArray) { return new CRtmObjArray(Parent, typeAlias, pri_ali?.ArraySizesConst ?? throw new Crash()); }
            else { throw new Crash($"{pri_ali} is not valid!"); }
         }

         public CRtmObj Make(CDeclClassField declField, IntPtr recordAddress)
         {
            var pri_ali = declField.TypeAlias.PrimitiveAlias;
            var fld_add = recordAddress + declField.Offset;

            if (pri_ali.IsClass) { return new CRtmObjRecord(Parent, fld_add, declField); }
            else if (pri_ali.IsBuiltIn)
            {
               BitField? bf = null;

               if (declField.BitFieldNumBits.HasValue)
               {
                  //distance in bit from byte beginning
                  var off_rnd = declField.BitOffset;

                  bf = new BitField(off_rnd, declField.BitFieldNumBits.Value);
               }

               return new CRtmObjScalar(fld_add, declField, bf);
            }
            else if (pri_ali.IsPointer) { return new CRtmObjPointer(Parent, recordAddress, declField); }
            else if (pri_ali.IsArray)
            {
               return new CRtmObjArray(Parent, recordAddress, declField);
            }
            else { throw new Crash($"{pri_ali} is not valid!"); }
         }

         public CRtmObj Make(CDecl decl, IntPtr address)
         {
            var pri_ali = decl.TypeAlias.PrimitiveAlias;

            if (pri_ali.IsClass) { return new CRtmObjRecord(Parent, address, decl); }
            else if (pri_ali.IsBuiltIn) { return new CRtmObjScalar(address, decl); }
            else if (pri_ali.IsPointer) { return new CRtmObjPointer(Parent, address, decl); }
            else if (pri_ali.IsArray) { return new CRtmObjArray(Parent, address, decl); }
            else { throw new Crash($"{pri_ali} is not valid!"); }
         }

         public CRtmObj Make(CTypeAlias typeAlias, IntPtr address)
         {
            var pri_ali = typeAlias.PrimitiveAlias;

            if (pri_ali.IsClass) { return new CRtmObjRecord(Parent, address, pri_ali); }
            else if (pri_ali.IsBuiltIn) { return new CRtmObjScalar(address, pri_ali); }
            else if (pri_ali.IsPointer) { return new CRtmObjPointer(Parent, address, pri_ali); }
            else if (pri_ali.IsArray) { return new CRtmObjArray(Parent, address, pri_ali); }
            else { throw new Crash($"{pri_ali} is not valid!"); }
         }
      }

      /// <summary>
      /// Gets the allocator used for allocating memory for runtime objects.
      /// </summary>
      public CRtmObjAllocator Allocator { get; }

      /// <summary>
      /// Gets the numeric converter used to perform conversions between numeric types.
      /// </summary>
      public virtual INumericConverter NumericConverter { get; }

      /// <summary>
      /// Gets the operator modifier used to modify operators for runtime objects.
      /// </summary>
      public virtual IRtmOperatorModifier OperatorModifier => new CRtmOperatorModifier();

      /// <summary>
      /// Gets the C# handler modifier for RTM operations.
      /// </summary>
      public IRtmOperatorCSharpHandlerModifier CSharpHandlerModifier => new CRtmOperatorCSharpHandlerModifier();

      /// <summary>
      /// Creates a new runtime object (CRtmObj) based on the provided declaration and allocator context.
      /// </summary>
      /// <param name="decl"></param>
      /// <returns></returns>
      public virtual CRtmObj MakeNewObject(CDecl decl) => myObjectMaker.Make(decl, Allocator);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="inString"></param>
      /// <param name="builtInSet"></param>
      /// <param name="cEncoding"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      public virtual CRtmObjLiteralString MakeString(GeneralizedString inString, CTypeBuiltInSet builtInSet, CCharEncodingLabel cEncoding)
      {
         var chr_typ = CharStandard.GetConstantCharType(cEncoding, builtInSet, false) ?? throw new Crash("Invalid char encoding");
         var typ_ali = new CTypeAlias(chr_typ);

         typ_ali.TypeSubscriptSet.AddSubscript(CTypeSubscript.MakeArray(inString.NullTerminatedLen), 0);

         return new CRtmObjLiteralString(this, typ_ali, inString);
      }

      public virtual CRtmObj CopyFunctionParamByRef(CRtmObj paramValue, CDecl paramDecl) =>
         myObjectMaker.Make(paramDecl, paramValue.Address ?? throw new Crash());

      public virtual CRtmObj CopyFunctionParamByValue(CRtmObj paramValue, CDecl paramDecl)
      {
         if (paramDecl.TypeAlias.IsArray) { return CopyFunctionParamByRef(paramValue, paramDecl); }
         else
         {
            //creating destination object
            var dst_rtm_obj = myObjectMaker.Make(paramDecl, Allocator);
            var ali = dst_rtm_obj.GetTypeAlias()?.PrimitiveAlias;

            //is destination built-in type
            var is_dst_bui = (ali?.IsBuiltIn ?? false) || (ali?.IsPointer ?? false);

            //if destination is built-in numeric convert is performed otherwise direct copy
            dst_rtm_obj.CSharpObj = is_dst_bui ?
               NumericConverter.DoConvertCsharpValue(
                  dst_rtm_obj?.CSharpObj?.GetType() ?? throw new Crash(),
                  paramValue?.CSharpObj ?? throw new Crash()) : paramValue.CSharpObj;

            return dst_rtm_obj;
         }
      }

      public virtual CRtmObj MakeConstant(ValueType constValue, CTypeAlias typeAlias)
      {
         if (typeAlias.IsPointer)
         {
            return new CRtmObjPointerLiteral(this, (IntPtr)(dynamic)constValue, typeAlias);
         }
         else if (typeAlias.IsBuiltIn)
         {
            var bin_typ = typeAlias.PrimitiveAlias.TypeBase as CTypeBuiltIn ?? throw new Crash();

            return new CRtmObjLiteral(
               NumericConverter.DoConvertCsharpValue(
                  bin_typ?.CSharpTypeForStorage ?? throw new Crash(), constValue) ?? throw new Crash(),
                  typeAlias);
         }
         else if (typeAlias.IsArray)
         {
            if (constValue is IntPtr ptr)
            {
               var pri = typeAlias.PrimitiveAlias;

               return new CRtmObjArray(this, ptr, pri);
            }
            else
            {
               throw new Gate.LangBase.Runtime.RtmException($"Expected an intptr");
            }
         }
         else { throw new Crash($"Constant for not built-in type {constValue.GetType().Name}"); }
      }

      public virtual CRtmObj MakeConstant(ValueType constValue, CType type) =>
         type is CTypeAlias ali ? MakeConstant(constValue, ali) : new CRtmObjLiteral(constValue, new CTypeAlias(type));

      /// <summary>
      /// Return array of item reference (contains pointer to location)
      /// </summary>
      /// <param name="rtmObj"></param>
      /// <param name="indices"></param>
      /// <returns></returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
      /// <exception cref="Crash"></exception>
      public unsafe virtual CRtmObj GetArrayItem(CRtmObj rtmObj, params int[] indices)
      {
         if (rtmObj is ICRtmObjPointer c_ptr)
         {
            var ali = rtmObj.DeclType.ConvertOrCrash<CTypeAlias>().PrimitiveAlias;
            var rtm_obj = (CRtmObj)c_ptr;
            var de_ref_typ = ali.DereferencedType;
            var itm_byt_ptr = (byte*)c_ptr.PointerValue;
            var ar = c_ptr as CRtmObjArray;//array or null
            var ar_szs = ar?.Sizes;//array sizes (or null)

            //if its an array indices are checked are inside array sizes
            if (ar_szs != null)
            {
               if (indices.Length > ar_szs.Length)
               {
                  throw new Gate.LangBase.Runtime.RtmException($"Too many input indices");
               }
               else if (Enumerable.Range(0, indices.Length).Any(i => indices[i] >= ar_szs[i]))
               {
                  throw new Gate.LangBase.Runtime.RtmException(
                     $"Indices [{string.Join(",", indices)}] out of bounds [{string.Join(",", ar?.Sizes ?? [])}]");
               }
            }

            for (var i = 0; i < indices.Length; i++)
            {
               //derefenced type sizeof (eg int[3] dt = int, float [2][3]; dt= float[3]
               var der_sof = de_ref_typ?.SizeOf ?? throw new Crash();

               itm_byt_ptr = itm_byt_ptr + der_sof * indices[i];

               if (i < indices.Length - 1) { de_ref_typ = de_ref_typ.DereferencedType; }
            }

            if (ar != null)
            {
               //array address
               var ar_adr = (byte*)(ar?.Address ?? throw new Crash());

               //array sizeof (type * sizes)
               var ar_sof = ar.SizeOf;

               //if distance in byte exceed byte capacity exception is raised
               var d = itm_byt_ptr - ar_adr;

               //memory outside bounds, probably BUG
               if (d >= ar.SizeOf)
               {
                  throw new Crash(
                     $"{(UInt64)itm_byt_ptr:x} outisde memory bound {(UInt64)ar_adr:x}-{((UInt64)ar_adr + (UInt64)ar.SizeOf):x}");
               }
            }

            return MakeRefValue((IntPtr)itm_byt_ptr, de_ref_typ ?? throw new Crash());
         }
         else
         {
            throw new Crash($"{rtmObj} not a {typeof(ICRtmObjPointer).Name} or {rtmObj.DeclType} not an {typeof(CTypeAlias).Name}");
         }
      }

      public virtual CRtmObj MakeRefValue(CRtmObj other, CTypeAlias typeAlias) =>
         other.Decl != null ? myObjectMaker.Make(
            (CDecl)other.Decl, other.Address ?? throw new Crash()) :
         MakeRefValue(other.Address ?? throw new Crash(), typeAlias);

      public virtual CRtmObj MakeRefValue(IntPtr address, CTypeAlias typeAlias) => myObjectMaker.Make(typeAlias, address);

      public virtual unsafe CRtmObj[] GetRecordMembers(CRtmObjRecord record)
      {
         var ali = record.DeclType as CTypeAlias ?? throw new Crash();

         return ali.PrimitiveAlias.TypeBase is ITypeClass cls ?
            cls.Fields.Select(f => myObjectMaker.Make(f, record.Address ?? throw new Crash())).ToArray() :
            throw new Crash();
      }

      public virtual unsafe CRtmObj GetRecordMember(CRtmObjRecord record, string memberId)
      {
         var ali = record.DeclType as CTypeAlias ?? throw new Crash();

         if (ali.IsPrimitive)
         {
            var cls = ali.PrimitiveAlias.TypeBase as ITypeClass ?? throw new Gate.CLanguage.CLangException($"Not a class");
            var fld =
               cls.Fields.FirstOrDefault(f => f.Identifier == memberId) ??
               throw new Gate.CLanguage.CLangException($"Field {memberId} not found in '{cls.Kind} {cls.Identifier}'");

            return fld.TypeAlias.PrimitiveAlias.IsBuiltIn ?
               myObjectMaker.Make(fld, record.Address ?? throw new Crash()) :
               MakeRefValue((IntPtr)((byte*)(record.Address ?? throw new Crash()) + fld.Offset), fld.TypeAlias.PrimitiveAlias);
         }
         else
         {
            throw new Gate.CLanguage.CLangException($"Not a primitive type!");
         }
      }

      public unsafe virtual CRtmObj Dereference(RtmObj rtmObjPointer)
      {
         if (rtmObjPointer is IRtmObjFunction fnc)//either a function or function pointer
         {
            return CRtmObjPointerFunction.Make(fnc, this);
         }
         else if (rtmObjPointer is ICRtmObjPointer ptr)
         {
            var pri_ali = (rtmObjPointer.DeclType as CTypeAlias ?? throw new Crash()).PrimitiveAlias;

            return MakeRefValue(ptr.PointerValue, pri_ali?.DereferencedType ?? throw new Crash());
         }
         else
         {
            throw new Crash();
         }
      }

      public unsafe virtual CRtmObj Assign(CRtmObj lObj, RtmObj rObj)
      {
         var l_typ = lObj.DeclType as CTypeAlias ?? throw new Crash();
         var r_typ = rObj.DeclType as CTypeAlias ?? throw new Crash();

         if (lObj is IRtmObjFunction lfp)
         {
            var c_lfp = lObj as CRtmObjPointerFunction ?? throw new Crash($"lObj shall be a {typeof(CRtmObjPointerFunction).Name}");
            var rfp = rObj as IRtmObjFunction ?? throw new Crash($"Expected a {typeof(IRtmObjFunction).Name}");

            c_lfp.Assign(rfp);
         }
         else if (lObj is ICRtmObjPointer l_ptr)
         {
            if (rObj is ICRtmObjPointer r_ptr)
            {
               lObj.CSharpObj = r_ptr.PointerValue;
               l_ptr.StringEncoding = r_ptr.StringEncoding;
            }
            else if (r_typ.IsBuiltIn && (r_typ?.BuiltIn?.IsInteger ?? false))
            {
               lObj.CSharpObj = (IntPtr)(dynamic)(rObj.CSharpObj ?? throw new Crash());
               l_ptr.StringEncoding = null;
            }
            else
            {
               throw new Gate.LangBase.Runtime.RtmException($"Can't assign pointer from {r_typ}");
            }
         }
         else if (lObj is CRtmObjRecord l_rec)
         {
            if (r_typ.TypeBase == l_typ.TypeBase)
            {
               var r_rec = rObj as CRtmObjRecord ?? throw new Crash();
               var lb = (byte*)(lObj.Address ?? throw new Crash());
               var rb = (byte*)(rObj.Address ?? throw new Crash());

               for (int i = 0; i < l_typ.SizeOf; i++) { lb[i] = rb[i]; }
            }
            else
            {
               throw new Gate.LangBase.Runtime.RtmException($"Can't assign struct type {l_typ} with {r_typ}");
            }
         }
         else if (l_typ.IsBuiltIn)
         {
            if (r_typ.IsBuiltIn)
            {
               lObj.CSharpObj = NumericConverter.DoConvertCsharpValue(
                  lObj.DeclType?.CSharpTypeForStorage ?? throw new Crash(), rObj.CSharpObj ?? throw new Crash());
            }
            else if (rObj is ICRtmObjPointer r_ptr)
            {
               if (l_typ?.BuiltIn?.IsInteger ?? false)
               {
                  lObj.CSharpObj = NumericConverter.DoConvertCsharpValue(
                     l_typ.CSharpTypeForStorage ?? throw new Crash(), r_ptr.PointerValue.ToInt64());
               }
               else
               {
                  throw new Gate.LangBase.Runtime.RtmException($"Incompatible struct types");
               }
            }
            else
            {
               throw new Gate.LangBase.Runtime.RtmException($"Incompatible struct types");
            }
         }
         else
         {
            //an array
            throw new Gate.LangBase.Runtime.RtmException($"Can't assign {l_typ} in C");
         }

         return lObj;
      }

      public virtual bool CanAssignTypeTo(CTypeAlias lType, CTypeAlias rType, RtmObjStrategyAssignContext assignContext) => rType.CanAssignTo(lType, true, lType.Language, assignContext);

      protected virtual CRtmObjAllocator myMakeAllocator() => new CRtmObjAllocatorByPrivateHeap();

      RtmObj IRtmObjStrategy.Assign(RtmObj lObject, RtmObj rObject) =>
         Assign(lObject as CRtmObj ?? throw new Crash(), rObject);

      RtmObj IRtmObjStrategy.CopyFunctionOptionalParamByValue(RtmObj paramValue) =>
         paramValue is CRtmObj cpa ? CopyFunctionOptionalParamByValue(cpa) : throw new Crash();

      public virtual CRtmObj CopyFunctionOptionalParamByValue(CRtmObj paramValue)
      {
         var rtm_obj = myObjectMaker.Make(paramValue.DeclType as CTypeAlias ?? throw new Crash(), Allocator);
         var cs = rtm_obj?.CSharpObj ?? throw new Crash();

         rtm_obj.CSharpObj = cs.GetType().IsPrimitive ?
            NumericConverter.DoConvertCsharpValue(
               cs.GetType(),
               paramValue.CSharpObj ?? throw new Gate.LangBase.Runtime.RtmException("Param required")) :
            paramValue.CSharpObj;

         return rtm_obj;
      }

      RtmObj IRtmObjStrategy.MakeConstant(ValueType constValue, IDeclType? declType) =>
         MakeConstant(constValue, declType as CTypeAlias ?? throw new Crash($"Not a {typeof(CTypeAlias).Name}"));

      RtmObj IRtmObjStrategy.GetArrayItem(RtmObj rtmObj, params int[] indices) =>
         GetArrayItem(rtmObj as CRtmObj ?? throw new Crash(), indices);

      RtmObj[] IRtmObjStrategy.GetRecordMembers(RtmObj rtmObj) =>
         rtmObj is CRtmObjRecord rtm_rec ?
            GetRecordMembers(rtm_rec) : throw new Gate.CLanguage.CLangException($"{rtmObj} not a record!");

      RtmObj IRtmObjStrategy.GetRecordMember(RtmObj rtmObj, string memberId) =>
        rtmObj is CRtmObjRecord rtm_rec ?
           GetRecordMember(rtm_rec, memberId) : throw new Gate.CLanguage.CLangException($"{rtmObj} not a record!");

      RtmObj IRtmObjStrategy.MakeRefValue(RtmObj other, IDeclType declType) =>
         other is CRtmObj c_rtm_obj && declType is CTypeAlias typ_ali ?
            (RtmObj)MakeRefValue(c_rtm_obj, typ_ali) :
            throw new Gate.Tools.ToolsException($"{other} not a {typeof(CRtmObj).Name} or not {declType} a {typeof(CTypeAlias).Name}");

      RtmObj IRtmObjStrategy.MakeNewObject(IDecl exprDecl) =>
         exprDecl is CDecl dcl ? MakeNewObject(dcl) : throw new Crash($"Not a {typeof(CTypeAlias).Name}!");

      Type IRtmObjStrategy.GetCSharpType(IDeclType? declType) =>
         declType is CTypeAlias ali ?
            ali.CSharpTypeForStorage ?? throw new Crash() :
         throw new Crash($"Not a {typeof(CTypeAlias).Name}!");

      RtmObj IRtmObjStrategy.CopyFunctionParamByRef(RtmObj paramValue, IDecl paramDecl) =>
         paramDecl is CDecl dcl ?
            CopyFunctionParamByRef(paramValue as CRtmObj ?? throw new Crash(), dcl) :
            throw new Crash($"Not a {typeof(CTypeAlias).Name}!");

      RtmObj IRtmObjStrategy.CopyFunctionParamByValue(RtmObj paramValue, IDecl paramDecl)
      {
         if (paramDecl is CDecl dcl)
         {
            if (paramValue is IRtmObjFunction of) { return CRtmObjPointerFunction.Make(of, this); }
            else
            {
               return CopyFunctionParamByValue(paramValue as CRtmObj ?? throw new Crash("paramValue can't be empty"), dcl);
            }
         }
         else
         {
            throw new Crash($"Not a {typeof(CTypeAlias).Name}!");
         }
      }

      bool IRtmObjStrategy.CanAssignTypeTo(IDeclType lType, IDeclType rType, RtmObjStrategyAssignContext assignContext) =>
         CanAssignTypeTo(lType as CTypeAlias ?? throw new Crash(), rType as CTypeAlias ?? throw new Crash(), assignContext);

      public IDeclType DereferenceType(IDeclType declType) =>
         (declType as CTypeAlias ?? throw new Crash()).DereferencedType ?? throw new Crash();

      public virtual IRtmObjFunction? GetFunction(RtmObj? rtmObj) => rtmObj as IRtmObjFunction;

      public virtual RtmObj[]? GetFunctionVisibleObject(RtmDbgEngStackVirtCpu? stack) => stack?.TopFunctionFrame?.ObjAll;

      protected override void myFreeManaged() => Allocator.Dispose();

      protected override void myFreeUnmanaged() { }

      public RtmObj[] GetParams(IDecl[] declParams, RtmObj[] rtmArgs)
      {
         var lst = new List<RtmObj>();

         //copies of arguments to function input parameters eg printf(ar,...)
         //with ar as argument and ... as optional parameters
         for (int i = 0; i < declParams.Length; i++)
         {
            var dcl_par = declParams[i].ConvertOrCrash<CDecl>();
            var par_val = rtmArgs.ElementAt(i);

            var cpy_par_val = dcl_par.TypeAlias.IsReference ?
               ((IRtmObjStrategy)this).CopyFunctionParamByRef(par_val, dcl_par) :
               ((IRtmObjStrategy)this).CopyFunctionParamByValue(par_val, dcl_par);

            lst.Add(cpy_par_val);
         }

         //optional parameters ( eg printf(const char*,...); ) are always copied by value
         for (int i = declParams.Length; i < rtmArgs.Length; i++)
         {
            var par_val = rtmArgs.ElementAtOrCrash(i);
            var cpy_par_val = CopyFunctionOptionalParamByValue(par_val.ConvertOrCrash<CRtmObj>());

            lst.Add(cpy_par_val);
         }

         return lst.ToArray();
      }

      RtmObj[]? IRtmObjStrategy.GetFunctionVisibleObject(IRtmDbgEngStack? stack) => 
         GetFunctionVisibleObject(stack as RtmDbgEngStackVirtCpu);
   }
}
