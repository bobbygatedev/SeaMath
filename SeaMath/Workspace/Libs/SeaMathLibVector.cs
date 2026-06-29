using Gate.CLanguage.Runtime;
using Gate.CLanguage.Types;
using Gate.LangBase.Runtime;
using Gate.LangBase.Runtime.Object;
using Gate.SeaMath.Sea;
using Gate.Tools.Extensions;

namespace Gate.SeaMath.Workspace.Libs
{
   /// <summary>
   /// 
   /// </summary>
   public unsafe class SeaMathLibVector : SeaMathLibCSharp
   {
      public const string NAME = "Vector";

      public SeaMathLibVector(SeaMathDbgIde dbgIde) : base(NAME, dbgIde) { }

      /// <summary>
      /// Range with 1 input parameter
      /// </summary>
      /// <param name="nItems">Number of range items</param>
      /// <returns></returns>
      [Method(Name = "rng1", Flags = MethodAttribute.FlagsType.all)]
      public RtmObj DoRange1(RtmObj nItems) => myDoRange(0, myGetParam(nItems, nameof(nItems)), 1);

      /// <summary>
      /// Range with 2 input parameters
      /// </summary>
      /// <param name="from"></param>
      /// <param name="nItems">Number of range items</param>
      /// <returns></returns>
      [Method(Name = "rng2", Flags = MethodAttribute.FlagsType.all)]
      public RtmObj DoRange2(RtmObj from, RtmObj nItems) => myDoRange(myGetParam(from, nameof(from)), myGetParam(nItems, nameof(nItems)), 1);

      /// <summary>
      /// Range with 3 input parameters
      /// </summary>
      /// <param name="from"></param>
      /// <param name="nItems">Number of range items</param>
      /// <param name="step"></param>
      /// <returns></returns>
      [Method(Name = "rng3", Flags = MethodAttribute.FlagsType.all)]
      public RtmObj DoRange3(RtmObj from, RtmObj nItems, RtmObj step) =>
         myDoRange(
            myGetParam(from, nameof(from)),
            myGetParam(nItems, nameof(nItems)),
            myGetParam(step, nameof(step)));

      private CTypeBuiltIn myGetInt64BitType() => (RtmStrategy.Settings.BuiltInSet?.FirstOrDefault(t =>
                                                           t.RepresentedType == CTypeBuiltInRepresent.integer &&
                                                           !t.IsUnsigned &&
                                                           t.SizeOf == 8)).NnOrCrash();
      private RtmObj myDoRange(Int64 from, Int64 nItems, Int64 step)
      {
         var int_typ = myGetInt64BitType();
         var out_typ = CTypeAlias.Make(int_typ, (int)nItems);
         var res = out_typ.GetNewRtmArray(RtmStrategy);

         for (int i = 0; i < nItems; i++)
         {
            res[i].CSharpObj = from + i * step;
         }

         return res;
      }

      private static long myGetParam(RtmObj param, string paramName) => 
         (Int64)(dynamic)(param.GetRtmScalarFromSea()?.CSharpObj ?? throw new RtmException(""));
   }
}