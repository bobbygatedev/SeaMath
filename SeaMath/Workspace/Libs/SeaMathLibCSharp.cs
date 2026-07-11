using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Linker;
using Gate.CLanguage.Standards;
using Gate.CLanguage.Types;
using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime;
using Gate.LangBase.Runtime.Object;
using Gate.SeaMath.Sea;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using System.Reflection;
using static Gate.SeaMath.FileSystem.SeaFileSystemItem;

namespace Gate.SeaMath.Workspace.Libs
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaMathLibCSharp : CLibrary
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="name"></param>
      /// <param name="dbgIde"></param>
      public SeaMathLibCSharp(string name, SeaMathDbgIde dbgIde)
      {
         DbgIde = dbgIde;
         Name = name;
      }

      /// <summary>
      /// 
      /// </summary>
      public class MethodAttribute : Attribute
      {
         [Flags]
         public enum FlagsType
         {
            none = 0x0,

            /// <summary>
            /// 
            /// </summary>
            run = 0x1,

            /// <summary>
            /// 
            /// </summary>
            console = 0x2,

            /// <summary>
            /// 
            /// </summary>
            all = run | console
         }

         /// <summary>
         /// todo not implented yet
         /// </summary>
         public FlagsType Flags { get; set; }

         /// <summary>
         /// Name of command as appear in console/program function.
         /// </summary>
         public string? Name { get; set; }

         /// <summary>
         /// <br>When not blank indicates that a <see cref="Gate.LangBase.Expressions.Operators.OperatorPunctuator"/> with:</br>
         /// <br><see cref="Gate.LangBase.Expressions.Operators.OperatorPunctuator.Punctuator"/> == <see cref="PunctuatorOverride"/> </br>
         /// <br> uses marked library method as an evaluator.</br>  
         /// <br>You shall set <see cref="PunctuatorOverride"/> to valid punctuator (eg '*') and <see cref="Name"/> to a not c valid name (eg 'operator +') </br>
         /// <br>in fact an operator return null when operation can't be overriden (in this case standard operator action is made) </br>
         /// <br>a function instead throws <see cref="Gate.LangBase.Runtime.RtmException"/> when input parameters are not of valid type </br>
         /// <br>see <see cref="Gate.SeaMath.Workspace.Libs.SeaMathLibMatrix.DoMatrixMultiply(RtmObj, RtmObj)"/>, <see cref="Gate.SeaMath.Workspace.Libs.SeaMathLibMatrix.DoMultiplyOverride(RtmObj, RtmObj)(RtmObj, RtmObj)"/> as an example of it.</br>
         /// </summary>
         public string? PunctuatorOverride { get; set; }

         /// <summary>
         /// When true result of function is displayed in console
         /// </summary>
         public bool IsConsoleOmitReturn { get; set; } = false;
      }

      /// <summary>
      /// Not instanciable type for <see cref="CLibraryCSharp"/> function parameters having <see cref="RtmObj"/> as parameter or retunr type
      /// </summary>
      public class TypeRtm : CTypePrimitive
      {
         private const string TYPE_SPECIFIER = "Sea#Rtm";

         private TypeRtm() : base(TYPE_SPECIFIER) { }

         /// <summary>
         /// Singleton
         /// </summary>
         public static TypeRtm Instance { get; } = new TypeRtm();

         public override string TypeSpecifier => TYPE_SPECIFIER;

         public override bool IsBuiltIn => true;

         public override bool IsConstant => false;

         public override bool IsClass => false;

         public override bool IsEnum => false;

         public override bool IsUserDefined => false;

         public override string DescriptorGcc => TypeSpecifier;

         public override Type CSharpTypeForStorage => typeof(RtmObj);

         public override CTypeBuiltIn? BuiltIn => null;

         public override int SizeOf => -1;

         public override string Descriptor => TypeSpecifier;

         public override int[]? ArraySizes => null;

         public override Type? CSharpArrayItemType => null;
      }

      public override string Name { get; }

      public override FileInfo? FileInfo => null;

      public override string Descriptor => $"{GetType().Name}:{Name}";

      public override string? Rebuilt => null;


      /// <summary>
      /// 
      /// </summary>
      public SeaMathDbgIde DbgIde { get; }

      /// <summary>
      /// 
      /// </summary>
      public SeaRtmStrategy RtmStrategy => CStandard.CCompiler.RtmStrategy as SeaRtmStrategy ?? throw new NullReferenceException();

      public CStandard CStandard => DbgIde.Standard;

      public override IDeclFunction? InitDeclFunction => null;

      public override IDeclFunction? CleanupDeclFunction => null;

      public override CDecl[] Decls => SubItems.OfType<CDeclSpecifiers>().SelectMany(ds => ds.Decls).ToArray();

      public override IDeclType[] Types => [];

      public void Init(MsgCollection messages) => myAddSubItemRange(myMakeDeclarations(messages).Select(d => d.DeclSpecifiers.NnOrCrash()));

      /// <summary>
      /// Generalized print for C# libraries usage.
      /// </summary>
      /// <param name="string"></param>
      /// <returns></returns>
      protected unsafe int myPrint(string @string)
      {
         return myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetStreamByType(InOutErrType.StdOut);

            fixed (char* ptr = @string)
            {
               return fil != null ? fil.PrintfW(RtmStrategy, ptr) : -1;
            }
         }, -1);
      }

      protected virtual SeaMathLibCSharpDeclFunction? myGetDeclFunction(
         MethodInfo methodInfo, MethodAttribute? methodAttribute, MsgCollection messages, out bool isSysMethod)
      {
         if (isSysMethod = myIsSysMethod(methodInfo, messages, out bool is_err))
         {
            return is_err ? null : myGetSysMethod(methodInfo, methodAttribute);
         }
         else
         {
            var res = new SeaMathLibCSharpDeclFunction(methodInfo, false);
            var prs = methodInfo.GetParameters();
            var fnc_cnt = res.FunctionContainer.NnOrCrash();

            fnc_cnt.HasVarArgs = prs.LastOrDefault()?.ParameterType == typeof(object[]);

            var prs_eff = fnc_cnt.HasVarArgs ? prs.Take(prs.Length - 1).ToArray() : prs;

            var dcl_prs = prs_eff.Select(p => myGetDeclVarFromParameter(methodInfo, p, messages)).ToArray();

            if (dcl_prs.All(d => d != null))
            {
               fnc_cnt.AddParameter(CStandard.CCompiler.ScopeHelper, dcl_prs.Cast<CDeclVar>().NnOrCrash().ToArray());
               res.DeclSpecifiers.NnOrCrash().TypeBase = myGetTypeBase(methodInfo.ReturnParameter.ParameterType, out int ind_lev);
               fnc_cnt.TypeAliasReturned.NnOrCrash().TypeSubscriptSet.AddSubScripts(
                  Enumerable.Range(0, ind_lev).Select(_ => CTypeSubscript.MakePointer()).ToArray());
               res.Identifier = methodAttribute?.Name;

               return res;
            }
            else { return null; }
         }
      }

      protected void myHandleException(Action action)
      {
         myHandleException<int>(() =>
         {
            action();

            return 0;
         });
      }

      protected RT myHandleException<RT>(Func<RT> action, RT? errorCode = default)
      {
         try
         {
            return action();
         }
         catch (RtmException exc)
         {
            var fil = DbgIde.FileSystem.GetStreamByType(InOutErrType.StdErr);

            fil?.Print(exc.Message, RtmStrategy.NnOrCrash());

            return errorCode ?? default!;
         }
      }

      protected int myHandleException(Func<int> action, int errorCode = -1) =>
         myHandleException<int>(() => action(), errorCode);

      private SeaMathLibCSharpDeclFunction[] myMakeDeclarations(MsgCollection messages)
      {
         var typ = GetType();
         var mts = typ.GetMethods().Where(m => m.GetCustomAttributes(typeof(MethodAttribute), false).Length > 0).ToArray();
         var ats = mts.Select(m => m.GetCustomAttributes(typeof(MethodAttribute), false)).ToArray();

         return mts.Select(m =>
         {
            var atr = m.GetCustomAttribute<MethodAttribute>();
            var fnc = myGetDeclFunction(m, atr, messages, out var is_sys);

            return fnc;
         }).Nn().ToArray();
      }

      private SeaMathLibCSharpDeclFunction myGetSysMethod(MethodInfo methodInfo, MethodAttribute? methodAttribute)
      {
         var res = new SeaMathLibCSharpDeclFunction(methodInfo, true);
         var prs = methodInfo.GetParameters();
         var fnc_cnt = res.FunctionContainer.NnOrCrash();

         fnc_cnt.HasVarArgs = prs.LastOrDefault()?.ParameterType == typeof(RtmObj[]);
         res.Identifier = methodAttribute?.Name;

         var prs_eff = fnc_cnt.HasVarArgs ? prs.Take(prs.Length - 1).ToArray() : prs;

         foreach (var par in prs_eff)
         {
            var dcl = CDeclVar.MakeSimple(TypeRtm.Instance);

            dcl.Identifier = par.Name;
            fnc_cnt.AddParameter(CStandard.CCompiler.ScopeHelper, dcl);
         }

         if (methodInfo.ReturnType != typeof(void))
         {
            res.DeclSpecifiers.NnOrCrash().TypeBase =
               methodInfo.ReturnType == typeof(RtmObj) ? (CType)TypeRtm.Instance : throw new Crash();
         }

         return res;
      }


      /// <summary>
      /// <br>A sys method can be of following types</br>
      /// <br> void SysMethod(RtmObj) </br>
      /// <br> void SysMethod(RtmObj,RtmObj) </br>
      /// <br> void SysMethod(RtmObj,RtmObj, params RtmObj[]) </br>
      /// <br> RtmObj SysMethod(RtmObj) </br>
      /// <br> RtmObj SysMethod(RtmObj,RtmObj) </br>
      /// <br> RtmObj SysMethod(RtmObj,RtmObj, params RtmObj[]) </br>
      /// </summary>
      /// <param name="methodInfo"></param>
      /// <param name="messages"></param>
      /// <param name="isError"></param>
      /// <returns></returns>
      private bool myIsSysMethod(MethodInfo methodInfo, MsgCollection messages, out bool isError)
      {
         var prs = methodInfo.GetParameters();
         var lst_par = prs.LastOrDefault();
         var is_sys = false;

         isError = false;

         //last method parameter is RtmObj[] is sys
         if (lst_par?.ParameterType == typeof(RtmObj[]))
         {
            prs = prs.Take(prs.Length - 1).ToArray();
            is_sys = true;
         }

         is_sys |= prs.Any(p => p.ParameterType == typeof(RtmObj)) || prs.Length == 0 && methodInfo.ReturnType == typeof(RtmObj);

         if (is_sys)
         {
            if (prs.Any(p => p.ParameterType != typeof(RtmObj)))
            {
               isError = true;

               foreach (var par in prs.Where(p => p.ParameterType != typeof(RtmObj)))
               {
                  messages.Add(new Msg(MsgType.error, $"Parameter {methodInfo.Name}.{par.Name} of type {typeof(RtmObj).Name}"));
               }
            }

            if (methodInfo.ReturnType != typeof(void) && methodInfo.ReturnType != typeof(RtmObj))
            {
               isError = true;
               messages.Add(new Msg(MsgType.error, $"Return type of {methodInfo.Name} not {typeof(RtmObj).Name}"));
            }

            //last parameter is array different from RtmObj[])
            if (lst_par != null && lst_par.ParameterType.IsArray && lst_par.ParameterType != typeof(RtmObj[]))
            {
               //then is array of not RtmObj
               isError = true;
               messages.Add(new Msg(MsgType.error, $"In function {Name}.{methodInfo.Name}"));
               messages.Add(new Msg(MsgType.error, $"Optional params not of type {typeof(RtmObj).Name}[]"));
            }
         }
         else if (methodInfo.ReturnType == typeof(RtmObj))
         {
            messages.Add(new Msg(MsgType.error, $"In function {Name}.{methodInfo.Name}"));
            messages.Add(new Msg(MsgType.error, $"A not-system function can't return {typeof(RtmObj).Name}"));
         }

         return is_sys;
      }

      private CDeclVar? myGetDeclVarFromParameter(MethodInfo methodInfo, ParameterInfo paramerInfo, MsgCollection messages)
      {
         myGetPrimitiveType(paramerInfo.ParameterType, out int ind_lev, out var pri_typ);

         var typ_bas = CStandard.CCompiler.Settings.BuiltInSet?.FirstOrDefault(b => b.CSharpTypeForStorage == pri_typ);

         if (typ_bas != null)
         {
            var dcl = CDeclVar.MakeSimple(typ_bas);

            dcl.TypeAlias.TypeSubscriptSet.AddSubScripts(
               Enumerable.Range(0, ind_lev).Select(_ => CTypeSubscript.MakePointer()).ToArray());

            return dcl;
         }
         else
         {
            messages.Add(
               new Msg(
                  MsgType.fail,
                  $"Param type '{methodInfo.Name}.{pri_typ?.Name}' can't be associate to any valid CSharp primitive type"));

            return null;
         }
      }

      private CType? myGetTypeBase(Type csharpType, out int indirectionLevel)
      {
         myGetPrimitiveType(csharpType, out indirectionLevel, out var pri_typ);

         return CStandard.CCompiler.Settings.BuiltInSet?.FirstOrDefault(b => b.CSharpTypeForStorage == pri_typ);
      }

      private void myGetPrimitiveType(Type csharpType, out int indirectionLevel, out Type? primitiveType)
      {
         indirectionLevel = 0;
         primitiveType = csharpType;

         while (primitiveType?.GetElementType() != null)
         {
            indirectionLevel++;
            primitiveType = primitiveType.GetElementType();
         }
      }
   }
}
