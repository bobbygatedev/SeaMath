using Gate.CLanguage.Decl;
using Gate.CLanguage.Runtime;
using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.TokenParse;
using Gate.CLanguage.Types;
using Gate.LangBase.ExtraTypes;
using Gate.LangBase.Runtime;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.SeaMath.Sea;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System.Data;
using System.Text;

namespace Gate.SeaMath
{
   /// <summary>
   /// Provides helper methods for creating and validating lambda functions, plotting data, and working with
   /// scalar-to-scalar functions in the context of the SeaMath debugging environment.
   /// </summary>
   /// <remarks>This class includes methods for generating lambda functions from expressions, validating
   /// functions for specific use cases, and displaying plots. It is designed to work with the SeaMath debugging
   /// environment and its associated runtime components.</remarks>
   internal class SeaPlotHelper
   {
      public const string LAMBDA_ID = "lambda";
      public static UInt64 myPlotCounter = 0;

      /// <summary>
      /// Initializes a new instance of the <see cref="SeaPlotHelper"/> class with the specified debugger IDE.
      /// </summary>
      /// <param name="dbgIde">The debugger IDE instance used by this helper. Cannot be null.</param>
      public SeaPlotHelper(SeaMathDbgIde dbgIde) => DbgIde = dbgIde;

      /// <summary>
      /// Creates a lambda function object from the provided lambda expression string.
      /// </summary>
      /// <remarks>The method parses the provided lambda expression, validates its syntax, and attempts to
      /// create a lambda function object. If the lambda expression references variables, it ensures that exactly one
      /// input variable is identified and visible in the thread's context. If the validation fails or the lambda
      /// expression is invalid, the method writes error messages to the standard output stream of the process
      /// associated with the thread and returns <see langword="null"/>.</remarks>
      /// <param name="lambda">A string representing the lambda expression. The expression must be valid and conform to the expected syntax.</param>
      /// <param name="thread">An optional <see cref="RtmDbgEngVirtCpuThread"/> instance representing the thread context in which the lambda
      /// function is created. If not provided, the currently running thread is used.</param>
      /// <returns>A <see cref="SeaRtmLambdaObjFunction"/> representing the compiled lambda function if the operation succeeds;
      /// otherwise, <see langword="null"/>.</returns>
      /// <exception cref="Crash">Thrown if an unexpected error occurs during the parsing or compilation of the lambda expression.</exception>
      public SeaRtmLambdaObjFunction? MakeLambdaFunction(string lambda, RtmDbgEngVirtCpuThread? thread = null)
      {
         var cmp = DbgIde.Standard.CCompiler;

         var tok_prs = new CTokenParser();
         var mgs = new MsgCollection();
         var oup = new CTokenParserOutput();
         var mrk = new TxtMarker(new TxtStore(lambda));

         var res = tok_prs.Perform(mrk, cmp.GetInData(mgs), ref oup) == TxtElabResult.success;

         thread = thread ?? RtmDbgEngVirtCpuThread.GetRunningThread() ?? throw new Crash();

         var sw = new StreamWriter(thread?.Process?.StdOut ?? throw new Crash());

         var lmb_obj = null as SeaRtmLambdaObjFunction;

         if (res)
         {
            var ids =
               oup.GetTextTokenList().
               Cast<CToken>().
               Where(t => t.TokenType == CTokenType.identifier).
               Select(t => t.Content).
               Distinct().
               ToArray();

            var ojs = (thread?.Process?.ObjVisibleFromBreakThreadAll).NnOrCrash();
            var ojs_fnc = ojs.OfType<IRtmObjFunction>().Select(f=>f.ConvertOrCrash<RtmObj>()).ToArray();

            ids = ids.Except(ojs_fnc.Select(o => o.VarName)).Where(n => !n.IsBlank()).Nn().ToArray();

            if (ids.Length == 0)
            {
               mgs.Add(new Msg(MsgType.error, "Not an input variable"));
               res = false;
            }
            else if (ids.Length != 1)
            {
               mgs.Add(new Msg(MsgType.error, $"More than one input variable({string.Join(",", ids)})"));
               res = false;
            }
            else
            {
               //eg sea lambda(sea x){ return x*x + 2*x +1;} creates a function of type y= f(x)
               var src = cmp.Parse($"sea {LAMBDA_ID}(sea {ids[0]}){{ return {lambda};}}");

               ///retrieves <see cref="CDeclFunction"/> instance from hierarchy 
               var fnc = src.AllDescendant.OfType<CDeclFunction>().FirstOrDefault(f => f.Identifier == LAMBDA_ID) ?? throw new Crash();

               //instanciates <see cref="RtmObjFunction"/> from <see cref="CDeclFunction"/>
               lmb_obj = new SeaRtmLambdaObjFunction(fnc, lambda);
            }
         }

         foreach (var msg in mgs) { sw.WriteLine(msg.FullMessage); }

         sw.Dispose();

         return lmb_obj;
      }

      /// <summary>
      /// Gets the debugging interface for SeaMath operations.
      /// </summary>
      public SeaMathDbgIde DbgIde { get; }

      /// <summary>
      /// Determines whether the specified function meets the requirements for a scalar-to-scalar plot function.
      /// </summary>
      /// <remarks>A valid scalar-to-scalar plot function must: <list type="bullet"> <item>
      /// <description>Have exactly one parameter.</description> </item> <item> <description>The parameter must be of a
      /// numeric type or a recognized sea type.</description> </item> <item> <description>The return type must be
      /// either a numeric type or a recognized sea type.</description> </item> </list></remarks>
      /// <param name="function">The function to validate. This must be a scalar-to-scalar function with specific parameter and return type
      /// constraints.</param>
      /// <returns><see langword="true"/> if the function meets the requirements for a scalar-to-scalar plot function; otherwise,
      /// <see langword="false"/>.</returns>
      /// <exception cref="Crash">Thrown if the function declaration is invalid or cannot be processed.</exception>
      public bool CheckPlotFunction(RtmDbgEngVirtCpuFunction function)
      {
         // Check scalar-to-scalar function requirements
         var fnc_dcl = function.DeclFunction as CDeclFunction ?? throw new Crash();
         var fnc_prs = fnc_dcl?.FunctionContainer?.Parameters ?? [];

         return fnc_dcl != null &&
            fnc_prs.Length == 1 &&
            (fnc_prs[0].TypeAlias.IsNumeric || fnc_prs[0].TypeAlias.IsSeaType()) && (
               (fnc_dcl.FunctionContainer?.TypeAliasReturned?.IsSeaType() ?? false) ||
               (fnc_dcl?.FunctionContainer?.TypeAliasReturned?.IsNumeric ?? false));
      }

      public RtmObj ShowPlot(double[] xAxis, double[] yAxis, string title, bool isPixelScaled, RtmDbgEngVirtCpuThread? thread = null)
      {
         thread = thread ?? RtmDbgEngVirtCpuThread.GetRunningThread() ?? throw new Crash();

         var str = thread?.Process?.RtmStrategy as SeaRtmStrategy ?? throw new Crash();
         var ses = DbgIde?.Session ?? throw new Crash();

         ses.PlotStrategy.ShowPlot(() =>
         {
            var old_pri = Thread.CurrentThread.Priority;

            //raises temporary thread priority
            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;

            var ctr = ses.PlotStrategy.MakePlotControl(xAxis, yAxis, isPixelScaled);

            Thread.CurrentThread.Priority = old_pri;

            return ctr;
         }, title);

         var pc = ++myPlotCounter;

         return str.MakeConstant(pc);
      }

      /// <summary>
      /// Computes the Y-axis values corresponding to the given X-axis values by executing the specified runtime object
      /// function.
      /// </summary>
      /// <remarks>This method processes each X-axis value by invoking the provided runtime object function
      /// and returns the resulting Y-axis values. The behavior of the computation depends on the type of the first
      /// parameter of the runtime object function.</remarks>
      /// <param name="rtmObjFunction">The runtime object function to execute for each X-axis value. This function determines how the Y-axis values
      /// are calculated.</param>
      /// <param name="xAxis">An array of X-axis values for which the corresponding Y-axis values will be computed. Cannot be null.</param>
      /// <returns>An array of Y-axis values computed by applying the <paramref name="rtmObjFunction"/> to each value in the
      /// <paramref name="xAxis"/> array.</returns>
      /// <exception cref="Crash"></exception>
      public double[] GetYDouble(RtmDbgEngVirtCpuFunction rtmObjFunction, double[] xAxis) =>
         myGetY(rtmObjFunction, xAxis).Select(o => o.GetValue<double>() ?? throw new Crash()).ToArray();

      /// <summary>
      /// Computes an array of complex double values based on the specified RTM object function and x-axis values.
      /// </summary>
      /// <remarks>This method processes the input x-axis values using the specified RTM object function and
      /// converts the results into complex double values.</remarks>
      /// <param name="rtmObjFunction">The RTM object function used to calculate the complex double values.</param>
      /// <param name="xAxis">An array of x-axis values to be used as input for the computation. Cannot be null.</param>
      /// <returns>An array of <see cref="ComplexDouble"/> values corresponding to the computed results for the given x-axis
      /// values.</returns>
      public ComplexDouble[] GetYComplexDouble(RtmDbgEngVirtCpuFunction rtmObjFunction, double[] xAxis) =>
         myGetY(rtmObjFunction, xAxis).Select(o => o.GetValue<ComplexDouble>() ?? throw new Crash()).ToArray();

      public (double[] xAxis, string? title) GetXAxisAndTitle(RtmObj[] @params)
      {
         // Extract plotting parameters
         var x_min = -5.0;
         var x_max = 5.0;
         var d = 0.1;
         var np = (int)Math.Round((x_max - x_min) / d);

         var tit = null as string;

         if (@params != null && @params.Any(p => p.GetRtmFromSea() as ICRtmObjPointer != null))
         {
            var sts = @params.Where(p => p.GetRtmFromSea() as ICRtmObjPointer != null).ToArray();

            @params = @params.Except(sts).ToArray();
            tit = (sts.FirstOrDefault() as ICRtmObjPointer)?.AsString;
         }

         // Override defaults if parameters provided
         if (@params != null && @params.Length >= 2)
         {
            var p1 = @params[0].GetRtmScalarFromSea();
            var p2 = @params[1].GetRtmScalarFromSea();
            var p3 = @params.ElementAtOrDefault(2)?.GetRtmScalarFromSea();

            if (p1 == null || p2 == null) { throw new Gate.LangBase.Runtime.RtmException($"optional params #1,#2 shall be a numeric scalar"); }
            else
            {
               if (p1.GetTypeAlias()?.IsNumeric ?? false) { x_min = (dynamic)p1.CSharpObj.NnOrCrash(); }
               else { throw new Gate.LangBase.Runtime.RtmException($"({nameof(@params)}) must be a numeric scalar"); }

               if (p2.GetTypeAlias()?.IsNumeric ?? false) { x_max = (dynamic)p2.CSharpObj.NnOrCrash(); }
               else { throw new Gate.LangBase.Runtime.RtmException($"({nameof(@params)}) must be a numeric scalar"); }

               if (p3 != null)
               {
                  if (p3.GetTypeAlias()?.IsNumeric ?? false)
                  {
                     if (p3.CSharpObj is float fv) { d = fv; }
                     else if (p3.CSharpObj is double dv) { d = dv; }
                  }
                  else { throw new Gate.LangBase.Runtime.RtmException($"({nameof(@params)}) must be a numeric scalar"); }
               }

               np = (int)Math.Round((x_max - x_min) / d);
            }
         }

         return (Enumerable.Range(0, np).Select(i => x_min + i * d).ToArray(), tit);
      }

      public ComplexDouble[] GetRtmObjArrayForPolar(RtmObj complexArray)
      {
         var xa = complexArray.GetRtmArrayFromSea() ??
            throw new Gate.LangBase.Runtime.RtmException($"{nameof(complexArray)} shall be an array");
         var xa_itm = xa.GetRtmArrayItemType();

         var sb_err = new StringBuilder();

         if (!(xa_itm?.IsNumeric ?? false)) { sb_err.AppendLine($"{nameof(complexArray)} item type shall be numeric"); }
         if (xa.Sizes.Length != 1) { sb_err.AppendLine($"{nameof(complexArray)} item type shall be 1-size array"); }

         return sb_err.Length > 0 ?
            throw new Gate.LangBase.Runtime.RtmException(sb_err.ToString()) :
            Enumerable.Range(0, xa.Sizes[0]).Select(i => xa[i].GetValue<ComplexDouble>() ?? throw new Crash()).ToArray();
      }

      public (CRtmObjArray xa, CRtmObjArray ya) GetXyRtmArray(RtmObj axis, RtmObj yAsys)
      {
         var xa = axis.GetRtmArrayFromSea() ?? throw new Gate.LangBase.Runtime.RtmException($"{nameof(axis)} shall be an array");
         var xa_itm = xa.GetRtmArrayItemType();

         var ya = yAsys.GetRtmArrayFromSea() ?? throw new Gate.LangBase.Runtime.RtmException($"{nameof(yAsys)} shall be an array");
         var ya_itm = ya.GetRtmArrayItemType();

         var sb_err = new StringBuilder();

         if (!(xa_itm?.IsNumeric ?? false)) { sb_err.AppendLine($"{nameof(axis)} item type shall be numeric"); }
         if (xa.Sizes.Length != 1) { sb_err.AppendLine($"{nameof(axis)} item type shall be 1-size array"); }
         if (!(ya_itm?.IsNumeric ?? false)) { sb_err.AppendLine($"{nameof(yAsys)} item type shall be numeric"); }
         if (ya.Sizes.Length != 1) { sb_err.AppendLine($"{nameof(yAsys)} item type shall be 1-size array"); }

         return sb_err.Length > 0 ? throw new Gate.LangBase.Runtime.RtmException(sb_err.ToString()) : (xa, ya);
      }

      public string GetTitle(RtmObj[] @params)
      {
         switch (@params.Length)
         {
            case 0: return "";
            case 1: return ((@params[0].GetRtmFromSea() as ICRtmObjPointer)?.AsString).Nn();

            default: throw new Gate.LangBase.Runtime.RtmException($"Too many optional params");
         }
      }

      private RtmObj[] myGetY(RtmDbgEngVirtCpuFunction rtmObjFunction, double[] xAxis)
      {
         var thr = RtmDbgEngVirtCpuThread.GetRunningThread() ?? throw new Crash();
         var stk = thr?.Stack ?? throw new Crash();
         var str = thr?.Process?.RtmStrategy as SeaRtmStrategy ?? throw new Crash();

         var typ = new CTypeAlias(str.Settings.BuiltInSet?.FirstOrDefault(t => t.TypeSpecifier == "double") ?? throw new Crash());

         var p0 = rtmObjFunction.DeclFunction?.Parameters[0] as CDeclVar ?? throw new Crash();
         var ys = new RtmObj[xAxis.Length];

         if (p0.TypeAlias.IsSeaType())
         {
            var sca = new CRtmObjScalar(str.Allocator,
               new CTypeAlias(str.Settings.BuiltInSet.FirstOrDefault(t => t.TypeSpecifier == "double") ?? throw new Crash()));
            var sea = new SeaTypeRtmObj(str.Allocator, p0);

            sea.RtmValue = sca;

            for (int i = 0; i < ys.Length; i++)
            {
               sea.RtmValue.CSharpObj = xAxis[i];
               ys[i] = rtmObjFunction.Exec(stk, str, sea) ?? throw new RtmException($"Not found a valid value for y({i}");
            }
         }
         else
         {
            var sca = new CRtmObjScalar(str.Allocator, p0);

            for (int i = 0; i < ys.Length; i++)
            {
               sca.CSharpObj = xAxis[i];
               ys[i] = rtmObjFunction.Exec(stk, str, sca) ?? throw new RtmException($"Not found a valid value for y({i}");
            }
         }

         return ys;
      }
   }
}