using Gate.CLanguage.Runtime;
using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Types;
using Gate.LangBase.Expressions;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
using Gate.LangBase.Runtime;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using System.Runtime.InteropServices;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// Operator array subscript need override in order to allow array constant (eg 3.0[2][3])
   /// </summary>
   public unsafe class SeaOperatorArraySubscript : COperatorArraySubscript
   {
      /// <summary>
      /// 
      /// </summary>
      public SeaOperatorArraySubscript() { }

      private class InnerDummyInitObject : RtmObj
      {
         public InnerDummyInitObject(CRtmObjScalar scalarItem) : base(null as IDeclType) => BaseItem = scalarItem;

         public readonly List<int> ListIndices = new List<int>();

         public override IntPtr? Address => null;

         public override ValueType? CSharpObj { get => null; set { } }

         public CRtmObjScalar BaseItem { get; }

         public override RtmFormat? CurrentFormat => null;

         protected override void myFreeManaged() { }

         protected override void myFreeUnmanaged() { }
      }

      public override RtmObj? EvalRtmArgs(
         ExprNodeOperator operatorNode, RtmObj?[]? rtmArgs, IRtmObjStrategy? rtmStrategy, IRtmDbgEngStackExecutable? stack)
      {
         var sea_str = rtmStrategy as SeaRtmStrategy ?? throw new Crash();
         var ers = new MsgCollection();

         //parent expression is another array subscript? if so sequence of subscript (eg [2][3]) is NOT terminated YET 
         var is_par = operatorNode.ParentExprNode is ExprNodeOperator opr && opr.Operator is SeaOperatorArraySubscript;
         var is_var = (rtmArgs ?? []).Any(r => r?.DeclType?.IsSeaType() ?? false);

         var arr_rtm = myGetOperand1(rtmArgs?.FirstOrDefault());
         var idx = myGetOperand2(rtmArgs?.ElementAtOrDefault(1), sea_str);//index of subsscript

         if (!idx.HasValue)
         {
            ers.Add(new Msg(
               MsgType.error, $"Not valid operand #2 for [] shall be of integer type", null, operatorNode.Token));
         }

         //an expression of type (2 + 3.0f)[2][3] is translated into a float[2][3] whose elements are set to 2.3f
         //a variant expression is forwarded to base handling
         if (arr_rtm == null && rtmArgs?.FirstOrDefault() is CRtmObjScalar sca && !(sca is SeaTypeRtmObj))
         {
            if (ers.Count > 0) { throw new Gate.LangBase.Runtime.RtmException(ers); }

            if (is_par)
            {
               var dum = new InnerDummyInitObject(sca);

               dum.ListIndices.Add(idx.NnOrCrash());

               return dum;
            }
            else
            {
               return myGetRtmArray(sca, rtmStrategy, idx.NnOrCrash());
            }
         }
         else if (rtmArgs?.FirstOrDefault() is InnerDummyInitObject dum_ini)
         {
            dum_ini.ListIndices.Add(idx.NnOrCrash());

            return is_par ?
               dum_ini :
               myGetRtmArray(dum_ini.BaseItem, rtmStrategy, dum_ini.ListIndices.ToArray());
         }
         else
         {
            if (arr_rtm == null)
            {
               ers.Add(new Msg(
                  MsgType.error, $"Not valid operand #1 for [] shall be array/pointer", null, operatorNode.Token));
            }

            if (ers.Count > 0) { throw new Gate.LangBase.Runtime.RtmException(ers); }

            /// idx as <see cref="RtmObj"/> 
            var idx_rtm = rtmArgs?.ElementAtOrDefault(1)?.GetRtmScalarFromSea();

            return base.EvalRtmArgs(operatorNode, [arr_rtm, idx_rtm], rtmStrategy, stack);
         }
      }

      private int? myGetOperand2(RtmObj? operand1, SeaRtmStrategy rtmStrategy)
      {
         var sca = operand1?.GetRtmScalarFromSea();

         if (sca == null) { return null; }
         else
         {
            try
            {
               return (int)(dynamic)(sca?.CSharpObj ?? throw new Crash());
            }
            catch
            {
               return null;
            }
         }
      }

      private CRtmObj? myGetOperand1(RtmObj? operand1)
      {
         var arr = operand1?.GetRtmArrayFromSea();
         var ptr = operand1?.GetRtmScalarFromSea() as ICRtmObjPointer;

         return (arr ?? ptr) as CRtmObj;
      }

      private CRtmObjArray myGetRtmArray(CRtmObjScalar baseItem, IRtmObjStrategy? rtmStrategy, params int[] indices)
      {
         var bin_typ = baseItem.DeclType.NnOrCrash().GetBuiltInType();
         var typ = new CTypeAlias(bin_typ);

         typ.TypeSubscriptSet.AddSubScripts(indices);

         var rtm = new CRtmObjArray(rtmStrategy.ConvertOrCrash<ICRtmObjStrategy>(), typ, indices);

         myFillOptimized(rtm.Address, baseItem.CSharpObj.NnOrCrash(), rtm.AllItemsCount);

         return rtm;
      }

      private void myFillOptimized(IntPtr? address, ValueType initValue, int itemsCount)
      {
         switch (Marshal.SizeOf(initValue))
         {
            case 1:
               {
                  byte b_val;
                  byte* b_ptr = (byte*)address.NnOrCrash();
                  Marshal.StructureToPtr(initValue, (IntPtr)(&b_val), false);

                  for (int i = 0; i < itemsCount; i++)
                  {
                     b_ptr[i] = b_val;
                  }
               }
               break;
            case 2:
               {
                  UInt16 b_val;
                  UInt16* b_ptr = (UInt16*)address.NnOrCrash();
                  Marshal.StructureToPtr(initValue, (IntPtr)(&b_val), false);

                  for (int i = 0; i < itemsCount; i++)
                  {
                     b_ptr[i] = b_val;
                  }
               }
               break;
            case 4:
               {
                  UInt32 b_val;
                  UInt32* b_ptr = (UInt32*)address.NnOrCrash();
                  Marshal.StructureToPtr(initValue, (IntPtr)(&b_val), false);

                  for (int i = 0; i < itemsCount; i++)
                  {
                     b_ptr[i] = b_val;
                  }
               }
               break;

            case 8:
               {
                  UInt64 b_val;
                  UInt64* b_ptr = (UInt64*)address.NnOrCrash();
                  Marshal.StructureToPtr(initValue, (IntPtr)(&b_val), false);

                  for (int i = 0; i < itemsCount; i++)
                  {
                     b_ptr[i] = b_val;
                  }
               }
               break;

            default:
               throw new Crash();
         }
      }
   }
}
