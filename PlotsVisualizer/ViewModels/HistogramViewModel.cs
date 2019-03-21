using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using SignalProcessing;
using System;

namespace PlotsVisualizer.ViewModels
{
    public class HistogramViewModel
    {
        public HistogramViewModel(Types.Signal signal)
        {
            Signal = signal ?? throw new ArgumentNullException(nameof(signal));

            var model = new PlotModel { Title = "ColumnSeries" };
            // A ColumnSeries requires a CategoryAxis on the x-axis.
            model.Axes.Add(new CategoryAxis());
            var series = new ColumnSeries();
            model.Series.Add(series);
            series.Items.Add(new ColumnItem(100));
            Histogram = model;
        }

        public Types.Signal Signal { get; set; }
        public PlotModel Histogram { get; set; }
    }
}
