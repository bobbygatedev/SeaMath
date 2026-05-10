using System;

namespace Gate.SeaMath.Plot
{
   /// <summary>
   /// Defines a strategy for creating and displaying scatter plots using the SeaMath library.
   /// </summary>
   public interface ISeaMathPlotStrategy
   {
      /// <summary>
      /// Creates and returns a plot control that displays a scatter plot based on the provided data.
      /// </summary>
      /// <remarks>
      /// The scatter plot is generated when the control's parent is set. Ensure that the <paramref name="xAxis" /> and <paramref name="yAxis" /> arrays have the same length; otherwise, the plot may not render
      /// correctly.
      /// </remarks>
      /// <param name="xAxis">An array of double values representing the x-coordinates of the data points.</param>
      /// <param name="yAxis">An array of double values representing the y-coordinates of the data points.</param>
      /// <returns>
      /// A <see cref="Control" /> containing the scatter plot. The control is fully docked and ready to display the
      /// plot.
      /// </returns>
      object MakePlotControl(double[] xAxis, double[] yAxis, bool isPixelScaled);

      /// <summary>
      /// Displays a plot within a control created by the specified delegate and assigns a title to the plot window.
      /// </summary>
      /// <remarks>This method is typically used to render a plot in a dynamically created control, such as
      /// a panel or a custom UI element. Ensure that the <paramref name="controlCreator"/> delegate provides a valid
      /// and initialized control.</remarks>
      /// <param name="controlCreator">A delegate that creates and returns the control in which the plot will be displayed.  The returned control
      /// must not be null.</param>
      /// <param name="title">The title to be displayed on the plot window. This value cannot be null or empty.</param>
      void ShowPlot(Func<object> controlCreator, string title);
   }
}
