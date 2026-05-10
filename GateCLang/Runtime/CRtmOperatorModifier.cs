using Gate.CLanguage.Runtime.Object;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using System.Runtime.InteropServices;

namespace Gate.CLanguage.Runtime
{
   /// <summary>
   /// Implements C language behavior of int/pointer basic operation <see cref="OperatorBinaryBasic"/> in order to differentiate it from C#.
   /// </summary>
   public class CRtmOperatorModifier : IRtmOperatorModifier
   {
      /// <summary>
      /// Constructor
      /// </summary>
      public CRtmOperatorModifier() { }

      /// <summary>
      /// Selects dynamically <see cref="ExprNodeOperator"/> having operator of type <see cref="OperatorBinaryBasic"/> and overrides behavior.
      /// </summary>
      /// <param name="operatorNode"></param>
      /// <param name="rtmArgs"></param>
      /// <param name="rtmStrategy"></param>
      /// <param name="stack"></param>
      /// <returns></returns>
      public virtual RtmObj? EvalRtmArgsModified(
         ExprNodeOperator operatorNode, RtmObj?[]? rtmArgs, IRtmObjStrategy? rtmStrategy, IRtmDbgEngStackExecutable? stack)
      {
         if (operatorNode.Operator is OperatorBinaryBasic ob)
         {
            return myModifyOperator(ob, rtmArgs, operatorNode, rtmStrategy, stack);
         }
         else
         {
            return null;
         }
      }

      private RtmObj? myModifyOperator(
         OperatorBinaryBasic? operatorBasic,
         RtmObj?[]? rtmArgs,
         ExprNodeOperator operatorNode,
         IRtmObjStrategy? rtmStrategy,
         IRtmDbgEngStackExecutable? stack)
      {
         if ((rtmArgs ?? []).All(r => r is ICRtmObjPointer))
         {
            return myModifyOperatorBasic(
               operatorBasic,
               rtmArgs?.ElementAtOrDefault(0) as ICRtmObjPointer,
               rtmArgs?.ElementAtOrDefault(1) as ICRtmObjPointer,
               operatorNode,
               rtmStrategy,
               stack);
         }
         else if (rtmArgs?[0] is ICRtmObjPointer p0)
         {
            return myModifyOperatorBasic(operatorBasic, p0, rtmArgs?.ElementAtOrDefault(1) as CRtmObjScalar, operatorNode, rtmStrategy, stack);
         }
         else if (rtmArgs?[1] is ICRtmObjPointer p1)
         {
            return myModifyOperatorBasic(operatorBasic, rtmArgs?.FirstOrDefault() as CRtmObjScalar, p1, operatorNode, rtmStrategy, stack);
         }
         else { return null; }
      }

      private unsafe RtmObj? myModifyOperatorBasic(
         OperatorBinaryBasic? operatorBasic,
         ICRtmObjPointer? rtmObjPtr1,
         ICRtmObjPointer? rtmObjPtr2,
         ExprNodeOperator? operatorNode,
         IRtmObjStrategy? rtmStrategy,
         IRtmDbgEngStackExecutable? stack)
      {
         var sz = rtmObjPtr1?.DereferencedType.SizeOf;
         var dif = ((byte*)(rtmObjPtr1?.PointerValue ?? 0).ToPointer() - (byte*)(rtmObjPtr2?.PointerValue ?? 0).ToPointer()) / sz;

         if (dif.HasValue && operatorNode?.DeclType != null)
         {
            return rtmStrategy?.MakeConstant(dif, operatorNode?.DeclType);
         }
         else
         {
            return null;
         }
      }

      private unsafe RtmObj? myModifyOperatorBasic(
         OperatorBinaryBasic? operatorBasic,
         CRtmObjScalar? delta,
         ICRtmObjPointer? rtmObjPtr2,
         ExprNodeOperator operatorNode,
         IRtmObjStrategy? rtmStrategy,
         IRtmDbgEngStackExecutable? stack)
      {
         var c_stg = rtmStrategy as ICRtmObjStrategy;
         var sz = rtmObjPtr2?.DereferencedType.SizeOf;
         var d =
            ((int)(c_stg?.NumericConverter.DoConvertCsharpValue(typeof(int), delta?.CSharpObj ?? 0) ?? 0) * sz) ??
            throw new Crash();

         unchecked
         {
            if (Marshal.SizeOf(typeof(IntPtr)) == 8) // 64-bit
            {
               var i64 = rtmObjPtr2?.PointerValue.ToInt64() ?? throw new Crash();
               var res = operatorNode?.Operator?.Symbol == "+" ? d + i64 : throw new Crash();

               return rtmStrategy?.MakeConstant((IntPtr)res, operatorNode.DeclType);
            }
            else
            {
               var i64 = rtmObjPtr2?.PointerValue.ToInt32() ?? throw new Crash();
               var res = operatorNode.Operator?.Symbol == "+" ? d + i64 : throw new Crash();

               return rtmStrategy?.MakeConstant((IntPtr)res, operatorNode.DeclType);
            }
         }
      }

      private unsafe RtmObj? myModifyOperatorBasic(
         OperatorBinaryBasic? operatorBasic,
         ICRtmObjPointer? rtmObjPtr1,
         CRtmObjScalar? delta,
         ExprNodeOperator operatorNode,
         IRtmObjStrategy? rtmStrategy,
         IRtmDbgEngStackExecutable? stack)
      {
         var c_stg = rtmStrategy as ICRtmObjStrategy;
         var sz = rtmObjPtr1?.DereferencedType.SizeOf;
         var d = ((int)(c_stg?.NumericConverter?.DoConvertCsharpValue(typeof(int), delta?.CSharpObj ?? 0) ?? 0) * sz) ?? throw new Crash();

         unchecked
         {
            var sym = operatorNode?.Operator?.Symbol;
            var is_plu = sym == "+" || sym == "+=";
            var is_ref = sym == "-=" || sym == "+=";

            if (!is_plu && !(sym == "-" || sym == "-=")) { throw new Crash(); }

            IntPtr res;

            if (Marshal.SizeOf(typeof(IntPtr)) == 8) // 64-bit
            {
               var i64 = rtmObjPtr1?.PointerValue.ToInt64() ?? 0;

               res = (IntPtr)(is_plu ? i64 + d : i64 - d);
            }
            else
            {
               var i32 = rtmObjPtr1?.PointerValue.ToInt32() ?? 0;

               res = (IntPtr)(is_plu ? i32 + d : i32 - d);
            }

            if (is_ref)
            {
               (rtmObjPtr1 ?? throw new Crash()).PointerValue = res;

               return rtmObjPtr1 as RtmObj ?? throw new Crash();
            }
            else
            {
               return rtmStrategy?.MakeConstant(res, operatorNode?.DeclType);
            }
         }
      }
   }
}
