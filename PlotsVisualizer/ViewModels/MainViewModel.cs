using OxyPlot;
using OxyPlot.Series;
using SignalProcessing;

namespace PlotsVisualizer.ViewModels
{
    class MainViewModel
    {
        public MainViewModel()
        {
            Signal = SignalProcessing.SignalProcessing.testGenerator;

            var tmp = new PlotModel { Title = "Simple example", Subtitle = "using OxyPlot" };

            // Create two line series (markers are hidden by default)
            var series1 = new LineSeries { Title = "Series 1", MarkerType = MarkerType.Triangle };
            
            foreach (var signalPoint in Signal.points)
            {
                series1.Points.Add(new DataPoint(signalPoint.x.r, signalPoint.y.r));
            }

            // Add the series to the plot model
            tmp.Series.Add(series1);

            // Axes are created automatically if they are not defined

            // Set the Model property, the INotifyPropertyChanged event will make the WPF Plot control update its content
            this.Model = tmp;
        }

        public Types.Signal Signal { get; }
        public PlotModel Model { get; }
    }
}
