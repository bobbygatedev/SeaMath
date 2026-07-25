using Gate.CLanguage.Decl;
using Gate.CLanguage.Runtime;
using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Types;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.SeaMath.Console;
using Gate.Tools;
using Gate.Tools.Extensions;
using static Gate.SeaMath.Workspace.Libs.SeaMathLibCSharp;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// Provides a strategy for handling runtime objects specific to the SeaMath type system.
   /// </summary>
   /// <remarks>This class extends the <see cref="CRtmObjStrategy"/> base class to provide specialized behavior
   /// for creating, copying, assigning, and manipulating runtime objects that are part of the SeaMath type system. It
   /// includes support for Sea-specific types, arrays, and function parameters, as well as integration with the
   /// built-in settings defined in <see cref="SeaDefSettings"/>.</remarks>
   public class SeaRtmStrategy : CRtmObjStrategy
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="settings"></param>
      public SeaRtmStrategy(SeaDefSettings settings) => Settings = settings;

      /// <summary>
      /// Gets the Sea definition settings that configure the behavior of this strategy.
      /// </summary>
      /// <remarks>
      /// The settings are passed through the constructor and provide configuration for built-in types,
      /// type aliases, and other Sea-specific settings that affect how runtime objects are handled.
      /// </remarks>
      public SeaDefSettings Settings { get; }

      /// <summary>
      /// Gets the operator modifier associated with the current instance.
      /// </summary>
      public override IRtmOperatorModifier OperatorModifier => new SeaRtmOperatorModifier();

      /// <summary>
      /// Creates a constant runtime object based on the specified value and type alias.
      /// </summary>
      /// <param name="constValue">The constant value to be represented. Can be <see langword="null"/> if the type alias represents an empty
      /// value.</param>
      /// <param name="typeAlias">The type alias that defines the type of the constant. Must be a valid type alias.</param>
      /// <returns>A runtime object representing the constant value. If <paramref name="typeAlias"/> is a SeaType and <paramref
      /// name="constValue"/> is <see langword="null"/>,  an empty SeaType runtime object is returned. If <paramref
      /// name="typeAlias"/> is not a SeaType, the base implementation is used.</returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException">Thrown if <paramref name="typeAlias"/> is a SeaType and no built-in type is found for the provided <paramref
      /// name="constValue"/>.</exception>
      public override CRtmObj MakeConstant(ValueType constValue, CTypeAlias typeAlias)
      {
         if (typeAlias.IsSeaType())
         {
            if (constValue == null)//then is empty
            {
               return new SeaTypeRtmObj(Allocator);
            }
            else
            {
               var bin = Settings.BuiltInSet?.ListBuiltIns.FirstOrDefault(b => b.CSharpTypeForStorage == constValue.GetType());

               return bin == null ?
                  throw new Gate.LangBase.Runtime.RtmException($"Not found Built-In type for {constValue}") :
                  (CRtmObj)new CRtmObjLiteral(constValue, new CTypeAlias(bin));
            }
         }
         else
         {
            return base.MakeConstant(constValue, typeAlias);
         }
      }

      /// <summary>
      /// Creates a constant runtime object from the specified value.
      /// </summary>
      /// <param name="constValue">The value to be converted into a constant runtime object. Must be of a type that corresponds to a built-in
      /// type in the current settings.</param>
      /// <returns>A <see cref="CRtmObj"/> representing the constant runtime object created from the specified value.</returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException">Thrown if the type of <paramref name="constValue"/> does not correspond to any built-in type in the current
      /// settings.</exception>
      public virtual CRtmObj MakeConstant(ValueType constValue)
      {
         var bin =
            Settings.BuiltInSet?.ListBuiltIns.FirstOrDefault(b => b.CSharpTypeForStorage == constValue.GetType()) ??
            throw new Gate.LangBase.Runtime.RtmException($"Can't retrieve built-in for {nameof(constValue)}({constValue.GetType().Name})");

         return MakeConstant(constValue, bin);
      }

      /// <summary>
      /// Creates a copy of the specified runtime object, optionally copying by value or by reference depending on the
      /// object's type and content.
      /// </summary>
      /// <remarks>This method determines whether to copy the object by value or by reference based on its
      /// type. For scalar types, the content is copied by value. For other types, the object is copied by
      /// reference.</remarks>
      /// <param name="paramValue">The runtime object to be copied. This parameter must not be <see langword="null"/>.</param>
      /// <returns>A new <see cref="CRtmObj"/> instance that represents the copied object. If the input object is of a scalar
      /// type, the content is copied by value; otherwise, the object is copied by reference.</returns>
      /// <exception cref="Crash"></exception>
      public override CRtmObj CopyFunctionOptionalParamByValue(CRtmObj paramValue)
      {
         if (paramValue.DeclType?.IsSeaType() ?? false)
         {
            var dst = new SeaTypeRtmObj(Allocator);
            var src = paramValue as SeaTypeRtmObj ?? throw new Crash();
            var sca_src = src.RtmValue?.GetRtmScalarFromSea();

            //if scalar copy by value of content otw by reference
            dst.RtmValue = sca_src != null ?
               CopyFunctionOptionalParamByValue(sca_src) ?? throw new Crash() :
               src.RtmValue;

            return dst;
         }
         else { return MakeConstant(
            paramValue.CSharpObj.NnOrCrash(), 
            paramValue.DeclType.ConvertOrCrash<CType>()); }
      }

      /// <summary>
      /// Creates a copy of the specified function parameter value based on the parameter's declaration type.
      /// </summary>
      /// <param name="paramValue">The value of the parameter to be copied.</param>
      /// <param name="paramDecl">The declaration of the parameter, which specifies the expected type and other attributes.</param>
      /// <returns>A new instance of <see cref="CRtmObj"/> representing the copied parameter value.  The behavior of the copy
      /// depends on the type specified in <paramref name="paramDecl"/>: <list type="bullet"> <item>If the parameter
      /// type is a sea type, a sea-specific copy is created.</item> <item>If the parameter type is a C-type, a
      /// type-compatible copy is created.</item> <item>If the parameter value is empty, it can only be assigned to a
      /// sea type; otherwise, an exception is thrown.</item> </list></returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException">Thrown if the parameter value cannot be assigned to the specified parameter type, or if an empty sea value is
      /// assigned to a non-sea type.</exception>
      /// <exception cref="Crash"></exception>
      public override CRtmObj CopyFunctionParamByValue(CRtmObj paramValue, CDecl paramDecl)
      {
         if (paramDecl.TypeAlias.TypeBase is TypeRtm) { return paramValue; }//no copy for c# library
         else if (paramValue is SeaTypeRtmObj var_par)
         {
            if (var_par.IsEmpty)
            {
               //an empty can be assigned to sea only
               return paramDecl.TypeAlias.IsSeaType() ?
                  (CRtmObj)new SeaTypeRtmObj(Allocator) : //return empty sea
                  throw new Gate.LangBase.Runtime.RtmException($"Can't assign sea empty to not-sea function parameter");
            }
            else
            {
               if (paramDecl.TypeAlias.IsSeaType())
               {
                  if (var_par.Decl != null)
                  {
                     return new SeaTypeRtmObj(Allocator, var_par.Decl , var_par.RtmValue);
                  }
                  else
                  {
                     //if parameter is sea then sea-2-sea copy is made
                     return new SeaTypeRtmObj(Allocator, var_par.RtmValue);
                  }
               }
               else if (base.CanAssignTypeTo(
                     paramDecl.TypeAlias,
                     var_par.RtmValue?.DeclType as CTypeAlias ?? throw new Crash(),
                     RtmObjStrategyAssignContext.function_param))
               {
                  //if parameter is c-type copying rtm value is made
                  return base.CopyFunctionParamByValue(var_par.RtmValue as CRtmObj ?? throw new Crash(), paramDecl);
               }
               else
               {
                  throw new Gate.LangBase.Runtime.RtmException(
                     $"Can't assign  {var_par.RtmValue?.DeclType} to {paramDecl.TypeAlias}");
               }
            }
         }
         else if (paramDecl.TypeAlias.IsSeaType()) 
         { 
            return new SeaTypeRtmObj(Allocator, paramDecl, paramValue); 
         }
         else { return base.CopyFunctionParamByValue(paramValue, paramDecl); }
      }

      /// <summary>
      /// Creates a new runtime object based on the specified declaration.
      /// </summary>
      /// <remarks>This method overrides the base implementation to provide specialized handling for Sea
      /// types. If the type alias in the declaration is not a Sea type, the base implementation is invoked.</remarks>
      /// <param name="decl">The declaration used to determine the type of runtime object to create.  
      /// The <see cref="CDecl.TypeAlias"/>
      /// property is used to evaluate the type.</param>
      /// <returns>A new instance of <see cref="SeaTypeRtmObj"/> if the type alias is a Sea type;  otherwise, the result of the
      /// base implementation.</returns>
      public override CRtmObj MakeNewObject(CDecl decl) =>
         decl.TypeAlias.IsSeaType() ? new SeaTypeRtmObj(Allocator, decl) : base.MakeNewObject(decl);

      /// <summary>
      /// Retrieves an item from a multi-dimensional array represented by the specified <see cref="CRtmObj"/>.
      /// </summary>
      /// <remarks>This method overrides the base implementation to provide specialized handling for <see
      /// cref="SeaTypeRtmObj"/>  instances. If the input object is not a <see cref="SeaTypeRtmObj"/>, the base
      /// implementation is used.</remarks>
      /// <param name="rtmObj">The <see cref="CRtmObj"/> instance representing the array. This must be either a <see cref="SeaTypeRtmObj"/> 
      /// or another supported array type.</param>
      /// <param name="indices">An array of integers specifying the indices of the item to retrieve. The number of indices must match the 
      /// dimensions of the array.</param>
      /// <returns>A new <see cref="CRtmObj"/> instance representing the array item at the specified indices. If the input 
      /// <paramref name="rtmObj"/> is a <see cref="SeaTypeRtmObj"/>, the returned object will also be a  <see
      /// cref="SeaTypeRtmObj"/>.</returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException">Thrown if <paramref name="rtmObj"/> is a <see cref="SeaTypeRtmObj"/> but does not represent an array 
      /// containing <see cref="SeaType.NAME"/>.</exception>
      public override CRtmObj GetArrayItem(CRtmObj rtmObj, params int[] indices)
      {
         if (rtmObj is SeaTypeRtmObj sea)
         {
            if (sea.RtmValue is ICRtmObjPointer ptr)
            {
               //create a sea copy of array item reference 
               // eg int v[] = {1 , 2 };
               // s = v;
               // s[0]++++;
               // v[0],s[0] contain 3
               var itm_sea = new SeaTypeRtmObj(sea.Allocator.NnOrCrash());

               itm_sea.RtmValue = base.GetArrayItem(sea.RtmValue as CRtmObj ?? throw new Crash(), indices);

               return itm_sea;
            }
            else
            {
               throw new Gate.LangBase.Runtime.RtmException($"{rtmObj} not an array containg {SeaType.NAME}");
            }
         }
         else
         {
            return base.GetArrayItem(rtmObj, indices);
         }
      }

      /// <summary>
      /// Assigns a right-hand-side object to a left-hand-side object, handling specific cases for function objects.
      /// </summary>
      /// <remarks>This method distinguishes between function objects and other types of objects when
      /// performing the assignment. If <paramref name="rObj"/> is a function object, it is wrapped in a <see
      /// cref="CRtmObjPointerFunction"/>  before being assigned to <paramref name="lObj"/>.</remarks>
      /// <param name="lObj">The left-hand-side object to which the assignment is performed.</param>
      /// <param name="rObj">The right-hand-side object to be assigned. This object may represent a function or another type of object.</param>
      /// <returns>A <see cref="CRtmObj"/> representing the result of the assignment. If <paramref name="rObj"/> is a function
      /// object,  the result will encapsulate the function; otherwise, it will represent the assigned object.</returns>
      public override CRtmObj Assign(CRtmObj lObj, RtmObj rObj)
      {
         var ro = rObj.GetRtmFromSea();

         if (ro is IRtmObjFunction fnc)
         {
            return myAssignLRtm(lObj, CRtmObjPointerFunction.Make(fnc, this));
         }
         else
         {
            return myAssignLRtm(lObj, ro as CRtmObj);
         }
      }

      /// <summary>
      /// You always assign both sea-to-type type-to-sea.
      /// </summary>
      /// <param name="lType"></param>
      /// <param name="rType"></param>
      /// <returns></returns>
      public override bool CanAssignTypeTo(CTypeAlias lType, CTypeAlias rType, RtmObjStrategyAssignContext assignContext) =>
         lType.TypeSubscriptSet.SubscriptCount == 0 && lType.TypeBase is TypeRtm ||
         lType.IsSeaType() ||
         rType.IsSeaType() ||
         base.CanAssignTypeTo(lType, rType, assignContext);

      /// <summary>
      /// Creates a new reference value based on the specified object and type alias.
      /// </summary>
      /// <param name="other">The object from which to create the reference value. Must be of type <see cref="SeaTypeRtmObj"/> if the
      /// <paramref name="typeAlias"/> represents a sea type.</param>
      /// <param name="typeAlias">The type alias that determines the type of the reference value to create. Must represent a sea type if
      /// <paramref name="other"/> is of type <see cref="SeaTypeRtmObj"/>.</param>
      /// <returns>A new <see cref="SeaTypeRtmObj"/> instance if the conditions are met; otherwise, the result of the base
      /// implementation.</returns>
      /// <exception cref="Crash">Thrown if <paramref name="typeAlias"/> does not represent a sea type while <paramref name="other"/> is of type
      /// <see cref="SeaTypeRtmObj"/>.</exception>
      public override CRtmObj MakeRefValue(CRtmObj other, CTypeAlias typeAlias)
      {
         if (other is SeaTypeRtmObj soj)
         {
            if (typeAlias.IsSeaType())
            {
               var rf_val = new SeaTypeRtmObj(soj.Allocator.NnOrCrash());

               rf_val.RtmValue = soj.RtmValue;

               return rf_val;
            }
            else { throw new Crash(); }
         }
         else
         {
            return base.MakeRefValue(other, typeAlias);
         }
      }

      /// <summary>
      /// Resolves the specified pointer to its corresponding runtime object.
      /// </summary>
      /// <remarks>This method first resolves the runtime object from the provided pointer using <see
      /// cref="RtmObj.GetRtmFromSea"/> and then delegates the dereferencing operation to the base
      /// implementation.</remarks>
      /// <param name="rtmObjPointer">The pointer to the runtime object to be dereferenced.</param>
      /// <returns>The runtime object referenced by the specified pointer.</returns>
      public override CRtmObj Dereference(RtmObj rtmObjPointer) => 
         base.Dereference(rtmObjPointer.GetRtmFromSea().NnOrCrash());

      /// <summary>
      /// Retrieves the function associated with the specified RTM object.
      /// </summary>
      /// <remarks>This method attempts to retrieve a function from the provided RTM object by invoking its
      /// <c>GetRtmFromSea</c> method. The result is cast to <see cref="IRtmObjFunction"/>. If the cast fails, the
      /// method returns <see langword="null"/>.</remarks>
      /// <param name="rtmObj">The RTM object from which to retrieve the function. This parameter cannot be <see langword="null"/>.</param>
      /// <returns>An object implementing <see cref="IRtmObjFunction"/> if the RTM object contains a valid function; otherwise,
      /// <see langword="null"/>.</returns>
      public override IRtmObjFunction? GetFunction(RtmObj? rtmObj) => rtmObj?.GetRtmFromSea() as IRtmObjFunction;

      /// <summary>
      /// Creates a new instance of <see cref="CRtmObjArray"/> with the specified item type and dimensions.
      /// </summary>
      /// <param name="itemType">The type of the items to be stored in the array. This must match a type defined in the built-in settings.</param>
      /// <param name="sizes">An array of integers specifying the size of each dimension of the array. Must contain at least one element.</param>
      /// <returns>A new <see cref="CRtmObjArray"/> instance configured with the specified item type and dimensions.</returns>
      /// <exception cref="Crash">Thrown if <paramref name="itemType"/> does not match any type defined in the built-in settings.</exception>
      public CRtmObjArray MakeRtmArray(Type itemType, params int[] sizes)
      {
         var bin = Settings?.BuiltInSet?.FirstOrDefault(b => b.CSharpTypeForStorage == itemType) ?? throw new Crash();

         return new CRtmObjArray(this, CTypeAlias.Make(bin, sizes), sizes);
      }

      /// <summary>
      /// Creates a runtime array with the specified item type and dimensions.
      /// </summary>
      /// <param name="itemTypeAlias">The type alias for array items</param>
      /// <param name="sizes">The dimensions of the array</param>
      /// <returns>A new CRtmObjArray instance</returns>
      public CRtmObjArray MakeRtmArray(CTypeAlias itemTypeAlias, params int[] sizes)
      {
         if (itemTypeAlias.IsPointer) { return new CRtmObjArray(this, itemTypeAlias.PrimitiveAlias, sizes); }
         else if (itemTypeAlias.IsBuiltIn) { return MakeRtmArray(itemTypeAlias.CSharpTypeForStorage.NnOrCrash(), sizes); }
         else { throw new Gate.LangBase.Runtime.RtmException($"Array of {itemTypeAlias.Descriptor} not allowed!"); }
      }

      /// <summary>
      /// Creates a runtime object literal representing an integer value.
      /// </summary>
      /// <param name="value">The integer value to encapsulate in the runtime object.</param>
      /// <returns>A <see cref="CRtmObjLiteral"/> instance that encapsulates the specified integer value.</returns>
      public CRtmObjLiteral GetIntRtmObj(int value) => new CRtmObjLiteral(value, new CTypeAlias(Settings.BuiltInSet?["int"].NnOrCrash()));

      /// <summary>
      /// Retrieves a pointer value as a <see cref="CRtmObjPointer"/> for the specified type.
      /// </summary>
      /// <param name="pointedType">The type that the pointer refers to. This must match a type in the built-in set of supported types.</param>
      /// <param name="ptrValue">The raw pointer value to be converted.</param>
      /// <returns>A <see cref="CRtmObjPointer"/> representing the pointer value for the specified type.</returns>
      public CRtmObjPointer GetPointerValue(Type pointedType, IntPtr ptrValue)
      {
         var bin_typ =
            Settings?.BuiltInSet?.FirstOrDefault(b => b.CSharpTypeForStorage == pointedType) ??
            throw new Crash();

         return this.GetPointerValue(bin_typ, ptrValue);
      }

      /// <summary>
      /// Assigns a right-hand runtime object to a left-hand runtime object, with specific handling for sea objects.
      /// </summary>
      /// <remarks>This method provides specialized assignment logic for runtime objects, including handling
      /// for sea objects. - If the left-hand object is a sea object, its value is updated based on the type of the
      /// right-hand object. - If the right-hand object is invalid in the current context, an exception is thrown. - For
      /// non-sea objects, the base assignment logic is used.</remarks>
      /// <param name="lRtm">The left-hand runtime object to which the assignment is performed. This object may be a sea object or a
      /// general runtime object.</param>
      /// <param name="rRtmObjNonSea">The right-hand runtime object to be assigned. This object must not be a sea object in certain contexts.</param>
      /// <returns>The left-hand runtime object after the assignment operation.</returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException">Thrown if <paramref name="rRtmObjNonSea"/> is <see langword="null"/> and <paramref name="lRtm"/> is not a sea
      /// object.</exception>
      /// <exception cref="Crash">Thrown if <paramref name="rRtmObjNonSea"/> is a sea object, or if an invalid runtime object type is
      /// encountered during the assignment.</exception>
      protected virtual CRtmObj myAssignLRtm(CRtmObj lRtm, CRtmObj? rRtmObjNonSea)
      {
         if (rRtmObjNonSea == null && !(lRtm is SeaTypeRtmObj))
         {
            throw new Gate.LangBase.Runtime.RtmException("Can't assign an empty sea object to pure CRtmObj");
         }
         else if (rRtmObjNonSea is SeaTypeRtmObj)
         {
            throw new Crash("Sea object not valid in this context!");
         }
         else if (lRtm is SeaTypeRtmObj sea)
         {
            if (
               rRtmObjNonSea == null ||
               rRtmObjNonSea is CRtmObjArray ||
               rRtmObjNonSea is CRtmObjPointerFunction || rRtmObjNonSea is CRtmObjLiteral ||
               rRtmObjNonSea is CRtmObjPointerLiteral)
            {
               sea.RtmValue = rRtmObjNonSea;
            }
            else if(rRtmObjNonSea is CRtmObjRecord rec)
            {
               var cpy = MakeNewObject(rec.Decl.ConvertOrCrash<CDecl>());

               rec.CopyTo(cpy);
               sea.RtmValue = cpy;  
            }
            else if (lRtm is CRtmObjScalar)
            {
               sea.RtmValue = base.MakeConstant(
                  rRtmObjNonSea.CSharpObj ?? throw new Crash(),
                  rRtmObjNonSea.DeclType as CTypeAlias ?? throw new Crash());
            }
            else { throw new Crash(); }

            return lRtm;
         }
         else
         {
            return base.Assign(lRtm, rRtmObjNonSea.NnOrCrash());
         }
      }

      public override RtmObj[]? GetFunctionVisibleObject(RtmDbgEngStackVirtCpu? stack)
      {
         var stk = stack as RtmDbgEngStackVirtCpu ?? throw new Crash();
         var bas_ojs = base.GetFunctionVisibleObject(stack);


         if (stk.Thread?.Process is SeaMathProcessConsole con_pro)
         {
            var ps = con_pro.ProcessFamily.Except([con_pro]).Where(p => p.State == RtmDbgEngRunState.halt).ToArray();
            var chi_pro_ojs = ps.SelectMany(p => p.ObjVisibleFromBreakThreadAll).ToArray();

            //append also sea console process objects(process objects have precedence respect to console objects)
            return chi_pro_ojs.Concat(bas_ojs ?? []).ToArray();
         }
         else
         {
            return bas_ojs;
         }
      }
   }
}
