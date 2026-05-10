using Gate.SeaMath.Plot;
using Gate.Tools;
using ScottPlot.WinForms;

namespace Gate.SeaMath.Windows.Plot
{
   /// <summary>
   /// Provides a base class for strategies that generate and display scatter plots using ScottPlot.
   /// </summary>
   /// <remarks>This abstract class defines the core functionality for creating and displaying scatter plots.
   /// Derived classes must implement the <see cref="ShowPlot(Func{Control}, string)"/> method to specify how the plot
   /// is displayed.</remarks>
   public abstract class SeaMathPlotStrategyScottPlot : ISeaMathPlotStrategy
   {
      /// <summary>
      /// Displays a plot within a control created by the specified delegate, using the provided title.
      /// </summary>
      /// <remarks>The method is abstract and must be implemented by a derived class. The implementation is
      /// responsible for  creating the control using the <paramref name="controlCreator"/> delegate and displaying the
      /// plot with the specified title.</remarks>
      /// <param name="controlCreator">A delegate that creates and returns the control in which the plot will be displayed.  The returned control
      /// must not be <see langword="null"/>.</param>
      /// <param name="title">The title of the plot to be displayed. This value cannot be <see langword="null"/> or empty.</param>
      public abstract void ShowPlot(Func<object> controlCreator, string title);

      /// <summary>
      /// Creates and returns a plot control that displays a scatter plot based on the provided data.
      /// </summary>
      /// <remarks>The scatter plot is generated when the control's parent is set. Ensure that the <paramref
      /// name="xAxis"/> and <paramref name="yAxis"/> arrays have the same length; otherwise, the plot may not render
      /// correctly.</remarks>
      /// <param name="xAxis">An array of double values representing the x-coordinates of the data points.</param>
      /// <param name="yAxis">An array of double values representing the y-coordinates of the data points.</param>
      /// <returns>A <see cref="Object"/> containing the scatter plot. The control (passed as an object from cross-platform compatibility) is fully docked and ready to display the
      /// plot.</returns>
      public virtual object MakePlotControl(double[] xAxis, double[] yAxis, bool isPixelScaled)
      {
         try
         {
            var ctr = new FormsPlot();

            ctr.Dock = DockStyle.Fill;

            //todo add tooltip x,y info box 
            ctr.ParentChanged += (s, e) =>
            {
               var sca = ctr.Plot.Add.Scatter(xAxis, yAxis);

               if (isPixelScaled)
               {
                  ctr.Plot.Axes.SquareUnits();
               }

               ctr.Refresh();
            };

            return ctr;
         }
         catch (Exception exc)
         {
            //captures exception and show it
            throw new Crash(exc);
         }
      }
   }
}
