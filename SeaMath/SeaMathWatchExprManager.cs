using Gate.CLanguage.Decl;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Standards;
using Gate.CLanguage.TokenParse;
using Gate.CLanguage.Types;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.SeaMath.Sea;
using Gate.Tools;
using Gate.Tools.Arry;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using Gate.Tools.Watch;
using GateLangBase;

namespace Gate.SeaMath
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaMathWatchExprManager : HierarchicalItem, IWatchExprManager
   {
      private readonly InnerVisitors.ForGetChild myVisitorForGetChild;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="dbgIde"></param>
      public SeaMathWatchExprManager(SeaMathDbgIde dbgIde)
      {
         myVisitorForGetChild = new InnerVisitors.ForGetChild(this);
         DbgIde = dbgIde;
      }

      private static class InnerVisitors
      {
         public class ForGetChild
         {
            public ForGetChild(SeaMathWatchExprManager parent) => Parent = parent;

            public SeaMathWatchExprManager Parent { get; }

            public ArrayMultidimensional<WatchExpr> GetChildMatrixArray(WatchExpr watchExpr) => 
               myGetChildMatrixArrayCall(watchExpr, watchExpr.Value as RtmObj ?? throw new Crash());

            private ArrayMultidimensional<WatchExpr> myGetChildMatrixArrayCall(WatchExpr watchExpr, RtmObj rtmObj) => 
               myGetChildMatrixArray(watchExpr, (dynamic)rtmObj);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="watchExpr"></param>
            /// <param name="rtmArray"></param>
            /// <returns></returns>
            private ArrayMultidimensional<WatchExpr> myGetChildMatrixArray(WatchExpr watchExpr, CRtmObjArray rtmArray)
            {
               var ali = rtmArray.DeclType as CTypeAlias ?? throw new Crash();
               var pri_ali = ali.PrimitiveAlias;
               var res = new ArrayMultidimensional<WatchExpr>(pri_ali.ArraySizesConst ?? []);
               var is_cst = ali.IsConstant || !pri_ali.IsBuiltIn;//just built-in an be updated

               foreach (var itm in res)
               {
                  itm.Value = watchExpr.Factory.Make(
                     $"{watchExpr.Expression}[{string.Join(",", itm.Indices)}]",
                     watchExpr.Factory.ExprManager,
                     new WatchExpr.SubExprType(null, itm.Indices.ToArray(), watchExpr));
                  itm.Value.Init(rtmArray[itm.Indices], is_cst);
               }

               return res;
            }

            private ArrayMultidimensional<WatchExpr> myGetChildMatrixArray(WatchExpr watchExpr, CRtmObjPointer rtmObjPointer)
            {
               throw new NotImplementedException();//todo
            }

            private ArrayMultidimensional<WatchExpr>? myGetChildMatrixArray(WatchExpr watchExpr, RtmObj value) => null;

            private ArrayMultidimensional<WatchExpr> myGetChildMatrixArray(WatchExpr watchExpr, CRtmObjRecord value)
            {
               var ali = value.DeclType as CTypeAlias ?? throw new Crash();
               var pri_ali = ali.PrimitiveAlias;

               if (pri_ali.TypeBase is ITypeClass cls)
               {
                  var fls = cls.Fields;
                  var res = new ArrayMultidimensional<WatchExpr>(cls.Fields.Length);

                  foreach (var itm in res)
                  {
                     var fld = fls[itm.Indices[0]];

                     itm.Value =
                        watchExpr.Factory.Make(
                           $"{watchExpr.Expression}.[{fld.Identifier}]",
                           watchExpr.Factory.ExprManager,
                           new WatchExpr.SubExprType(fld.Identifier, itm.Indices, watchExpr));
                  }

                  return res;
               }
               else { throw new Crash(); }
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public SeaMathDbgIde DbgIde { get; }

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuProcess[] Processes => DbgIde.Processes;

      /// <summary>
      /// 
      /// </summary>
      public CStandard CStandard => DbgIde.Builder.CStandard ?? throw new Gate.LangBase.Runtime.RtmException();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="watchExpr"></param>
      /// <returns></returns>
      public ArrayMultidimensional<WatchExpr> GetChildMatrixArray(WatchExpr watchExpr) => myVisitorForGetChild.GetChildMatrixArray(watchExpr);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="rtmObj"></param>
      /// <returns></returns>
      public string? GetObjectString(CRtmObj rtmObj)
      {
         if (rtmObj.CSharpObj is ValueType v)
         {
            var un = (UniversalNumeric)(dynamic)v;

            return un.DisplayValue;
         }
         else
         {
            return "";
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="rtmObj"></param>
      /// <returns></returns>
      public string? GetTypeString(CRtmObj rtmObj) => rtmObj.DeclType?.TypeSpecifier;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="expression"></param>
      /// <param name="value"></param>
      /// <param name="isReadOnly"></param>
      /// <param name="errorMsg"></param>
      /// <returns></returns>
      public bool TryParse(string expression, out object? value, out bool isReadOnly, out Msg? errorMsg)
      {
         var mgs = new MsgCollection();
         var prs = new CTokenParser();
         var out_prs = new CTokenParserOutput();
         var oup = new CTokenInterpreterOutput();
         var in_dat = CStandard.CCompiler.GetInData(mgs);

         var exp_slv = new SeaExprSolver(
            DbgIde.Standard.CCompiler.Interpreter?.DeclInterpretFactory ?? throw new Crash(),
            CStandard.CCompiler.Settings.LangFlags,
            DbgIde.Standard.CCompiler.Interpreter.ExprInterpret,
            DbgIde.Standard.CCompiler.Interpreter.AttributesInterpret);

         var prs_res =
            prs.Perform(new TxtMarker(new TxtStore(expression)), CStandard.CCompiler.GetInData(mgs), ref out_prs);

         var bre_thr =
            DbgIde.DbgEng.CurrentBreakThread?.Process?.DbgIde ==
               DbgIde ? DbgIde.DbgEng.CurrentBreakThread : null;

         //break thread objects + console (only global) objects
         var rtm_obj_pro = bre_thr?.Stack?.TopCall?.ObjAll ?? [];
         var rtm_obj_cns = DbgIde.ConsoleProcess?.ObjsGlobal ?? [];

         //rtm objects consist of concat of console + seamath process rtm objects
         //in case of object with same name seamath process has precedence
         var dcs = rtm_obj_cns.Concat(rtm_obj_pro).Select(r => r.Decl).OfType<CDecl>().ToArray();

         if (prs_res == TxtElabResult.success && exp_slv.InterpretTokens(
               out_prs.GetTextTokenList(),
               CStandard.CCompiler.GetInData(mgs),
               dcs,
               ref oup,
               out var exp,
               true) == TxtElabResult.success)
         {
            var dum_thr = null as RtmDbgEngVirtCpuThread;
            var stk =
               (bre_thr as RtmDbgEngVirtCpuThread)?.Stack ??
               DbgIde.ConsoleProcess?.ThreadsActive.FirstOrDefault()?.Stack;

            try
            {
               var res = exp?.Eval(stk, DbgIde.Builder.CStandard?.CCompiler.RtmStrategy);
               var is_bin = exp?.RootNode?.DeclType is CType ct && ct.IsBuiltIn; //is expression built-in
               var is_lv = exp?.RootNode?.IsLValue ?? false;//is l-value

               value = res;
               isReadOnly = (res != null && res.IsConstant) || !is_lv && is_bin;
               errorMsg = null;
            }
            catch (Gate.LangBase.Runtime.RtmException exc)
            {
               errorMsg = new Msg(MsgType.error, $"Runtime exception: {exc.Message}");
               value = null;
               isReadOnly = true;

               return false;
            }

            return true;
         }
         else
         {
            errorMsg =
               mgs.FirstOrDefault(m => m.MsgType == MsgType.fail || m.MsgType == MsgType.fatal) ??
               mgs.FirstOrDefault();
            value = null;
            isReadOnly = true;

            return false;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="watchExpr"></param>
      /// <param name="newValue"></param>
      /// <param name="value"></param>
      /// <returns></returns>
      public bool Update(WatchExpr watchExpr, string newValue, out object? value)
      {
         if (TryParse(newValue, out var val, out _, out _))
         {
            var rtm = (value = watchExpr.Value) as RtmObj ?? throw new Crash();
            var new_val = val as RtmObj ?? throw new Crash();

            var rtm_str = DbgIde.Builder.CStandard?.CCompiler.RtmStrategy.NnOrCrash();

            rtm.CSharpObj = rtm_str?.NumericConverter.DoConvertCsharpValue(
               rtm?.CSharpObj?.GetType() ?? throw new Crash(),
               new_val?.CSharpObj ?? throw new Crash());

            return true;
         }
         else
         {
            value = null;

            return false;
         }
      }

      string? IWatchExprManager.GetObjectString(object value) => value is CRtmObj c_rtm_obj ? GetObjectString(c_rtm_obj) : null;

      string? IWatchExprManager.GetTypeString(object value) => value is CRtmObj c_rtm_obj ? GetTypeString(c_rtm_obj) : null;
   }
}
