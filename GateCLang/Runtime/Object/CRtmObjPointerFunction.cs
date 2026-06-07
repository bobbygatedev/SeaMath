using Gate.CLanguage.Decl;
using Gate.CLanguage.Types;
using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;

namespace Gate.CLanguage.Runtime.Object
{
   /// <summary>
   /// 
   /// </summary>
   public unsafe class CRtmObjPointerFunction : CRtmObjPointer, IRtmObjFunction
   {
      public CRtmObjPointerFunction(ICRtmObjStrategy rtmStrategy, RtmDbgEngVirtCpuFunction objFunction) : base(
         rtmStrategy, IntPtr.Zero, (objFunction?.DeclType as CTypeAlias ?? throw new Crash()).AddressOfType) => ObjFunction = objFunction;

      public CRtmObjPointerFunction(ICRtmObjStrategy rtmStrategy, CDecl decl) :
         base(rtmStrategy, decl)
      { }

      public static CRtmObjPointerFunction Make(IRtmObjFunction rtmObjFunction, CRtmObjStrategy cRtmObjStrategy)
      {
         if (rtmObjFunction is RtmDbgEngVirtCpuFunction fnc)
         {
            return new CRtmObjPointerFunction(cRtmObjStrategy, fnc);
         }
         else if (rtmObjFunction is CRtmObjPointerFunction pf)
         {
            return new CRtmObjPointerFunction(cRtmObjStrategy, pf.ObjFunction ?? throw new Crash());
         }
         else { throw new Gate.LangBase.Runtime.RtmException($"{rtmObjFunction} null or not of valid type"); }
      }

      public CTypeAlias? TypeAlias => DeclType as CTypeAlias;

      public RtmDbgEngVirtCpuFunction? ObjFunction { get; set; }

      public IDeclFunction? DeclFunction => ObjFunction?.Decl;

      public override string DisplayValue => $"C-pointer to function {ObjFunction}";

      public RtmObj Exec(RtmDbgEngStackVirtCpu? stack, IRtmObjStrategy? rtmStrategy, params RtmObj?[] @params) =>
         ObjFunction?.Exec(stack, rtmStrategy, @params) ?? throw new Crash();

      public void Assign(IRtmObjFunction rtmObjFunction)
      {
         if (rtmObjFunction is RtmDbgEngVirtCpuFunction fnc) { ObjFunction = fnc; }
         else if (rtmObjFunction is CRtmObjPointerFunction pf) { ObjFunction = pf.ObjFunction; }
         else { throw new Crash(); }
      }

      public override string ToString() => DisplayValue;
   }
}
