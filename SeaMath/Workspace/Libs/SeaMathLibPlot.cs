using Gate.LangBase.Runtime.Object;
using Gate.SeaMath.Sea;
using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.SeaMath.Workspace.Libs
{
   /// <summary>
   /// Provides functionality for creating and displaying various types of plots, including Cartesian and polar plots.
   /// </summary>
   /// <remarks>The <see cref="SeaMathLibPlot"/> class extends the <see cref="SeaMathLibCSharp"/> base class
   /// and offers methods for generating plots from mathematical expressions, functions, or data arrays. It supports
   /// Cartesian plots (e.g., <see cref="DoPlotXy"/> and <see cref="DoPlotFunction"/>) and polar plots (e.g., <see
   /// cref="DoPlotPolarFunction"/>  and <see cref="DoPlotPolarXy"/>). The class is designed to work with the SeaMath
   /// runtime environment and provides seamless integration with its data structures and conventions.</remarks>
   public unsafe class SeaMathLibPlot : SeaMathLibCSharp
   {
      public const string NAME = "Plot";
      private SeaPlotHelper myPlotHelper;

      public SeaMathLibPlot(SeaMathDbgIde dbgIde) : base(NAME, dbgIde) => myPlotHelper = new SeaPlotHelper(dbgIde);


      [Method(Name = "expr")]
      public RtmObj DoExpression(RtmObj expression)
      {
         var fnc_str = expression.GetRtmArrayFromSea()?.AsString;

         var fnc =
            (fnc_str != null ? myPlotHelper.MakeLambdaFunction(fnc_str) : null) ??
            throw new Gate.LangBase.Runtime.RtmException($"{nameof(expression)},Invalid lambda expression");

         return fnc;
      }

      [Method(Name = "plotxy")]
      public RtmObj DoPlotXy(RtmObj xAsys, RtmObj yAsys, params RtmObj[] @params)
      {
         (var xa, var ya) = myPlotHelper.GetXyRtmArray(xAsys, yAsys);

         var n = Math.Min(xa.Sizes[0], ya.Sizes[0]);
         var xs = new double[n];
         var ys = new double[n];

         for (int i = 0; i < n; i++)
         {
            xs[i] = xa[i].GetValue<double>() ?? throw new Crash();
            ys[i] = ya[i].GetValue<double>() ?? throw new Crash();
         }

         var tit = myPlotHelper.GetTitle(@params);

         return myPlotHelper.ShowPlot(xs, ys, tit, false);
      }

      /// <summary>
      /// Generates and displays a plot for a given function over a specified range and step size.
      /// </summary>
      /// <remarks>The method evaluates the provided function over the specified range and step size,
      /// computes the corresponding y-axis values,  and displays the resulting plot. If the function is provided as a
      /// string, it must be a valid lambda expression.</remarks>
      /// <param name="function">The function to be plotted. This must be a valid lambda expression or a callable function object.</param>
      /// <param name="params">Optional parameters to define the plot range and step size: <list type="number"> <item><description>The
      /// minimum value of the x-axis (numeric scalar).</description></item> <item><description>The maximum value of the
      /// x-axis (numeric scalar).</description></item> <item><description>The step size for the x-axis (numeric scalar,
      /// optional).</description></item> </list> If not provided, default values of -5.0 for the minimum, 5.0 for the
      /// maximum, and 0.1 for the step size are used.</param>
      /// <returns>An <see cref="RtmObj"/> representing the generated plot.</returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException">Thrown if: <list type="bullet"> <item><description><paramref name="function"/> is null or
      /// invalid.</description></item> <item><description><paramref name="params"/> contains non-numeric or invalid
      /// values.</description></item> </list></exception>
      [Method(Name = "plotfun")]
      public RtmObj DoPlotFunction(RtmObj function, params RtmObj[] @params)
      {
         var fnc = function.GetRtmObjFunctionSea();
         var fnc_tit = null as string;

         if (fnc == null)
         {
            var fnc_str = function.GetRtmArrayFromSea()?.AsString;

            fnc =
               (fnc_str != null ? myPlotHelper.MakeLambdaFunction(fnc_str) : null) ??
               throw new Gate.LangBase.Runtime.RtmException($"{nameof(function)},Invalid lambda expression");
            fnc_tit = fnc_str;
         }
         else { fnc_tit = fnc.VarName; }

         if (!myPlotHelper.CheckPlotFunction(fnc)) { throw new Gate.LangBase.Runtime.RtmException($"{nameof(function)},Function input cannot be null"); }

         (var xs, var tit) = myPlotHelper.GetXAxisAndTitle(@params);
         var ys = myPlotHelper.GetYDouble(fnc, xs);

         tit = tit.IsBlank() ? fnc_tit : $"{tit}({fnc_tit})";

         return myPlotHelper.ShowPlot(xs, ys, tit.Nn(), false);
      }

      /// <summary>
      /// Plots a polar function based on the provided function and parameters.
      /// </summary>
      /// <remarks>This method evaluates the provided function over a range of values and generates a polar
      /// plot. The function must be valid and compatible with the plotting system. If the function is invalid or null,
      /// an exception will be thrown.</remarks>
      /// <param name="function">The function to be plotted. This can be a lambda expression or an object representing a function.</param>
      /// <param name="params">Additional parameters used to configure the plot, such as axis values or other plot-specific settings.</param>
      /// <returns>An <see cref="RtmObj"/> representing the resulting plot object.</returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException">Thrown if <paramref name="function"/> is not a valid lambda expression or if the function input is null.</exception>
      [Method(Name = "plotpolarfun")]
      public RtmObj DoPlotPolarFunction(RtmObj function, params RtmObj[] @params)
      {
         var fnc = function.GetRtmObjFunctionSea();
         var fnc_tit = null as string;

         if (fnc == null)
         {

            var fnc_str = function.GetRtmArrayFromSea()?.AsString;

            fnc =
               (fnc_str != null ? myPlotHelper.MakeLambdaFunction(fnc_str) : null) ??
               throw new Gate.LangBase.Runtime.RtmException($"{nameof(function)},Invalid lambda expression");
            fnc_tit = fnc_str;
         }
         else { fnc_tit = fnc.VarName; }

         if (!myPlotHelper.CheckPlotFunction(fnc)) { throw new Gate.LangBase.Runtime.RtmException($"{nameof(function)},Function input cannot be null"); }

         (var xs, var tit) = myPlotHelper.GetXAxisAndTitle(@params);

         tit = tit.IsBlank() ? fnc_tit : $"{tit}({fnc_tit})";

         var ys = myPlotHelper.GetYComplexDouble(fnc, xs);

         return myPlotHelper.ShowPlot(ys.Select(i => i.Re).ToArray(), ys.Select(i => i.Im).ToArray(), tit.Nn(), true);
      }

      [Method(Name = "plotpolarxy")]
      public RtmObj DoPlotPolarXy(RtmObj complexArray, params RtmObj[] @params)
      {
         var arr = myPlotHelper.GetRtmObjArrayForPolar(complexArray);
         var tit = myPlotHelper.GetTitle(@params);

         return myPlotHelper.ShowPlot(arr.Select(x => x.Re).ToArray(), arr.Select(x => x.Im).ToArray(), tit, true);
      }
   }
}