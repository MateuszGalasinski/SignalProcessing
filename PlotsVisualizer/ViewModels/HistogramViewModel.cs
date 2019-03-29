using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Wpf;
using SignalProcessing;
using System;
using System.IO;
using System.Linq;
using UILogic.Base;
using CategoryAxis = OxyPlot.Axes.CategoryAxis;
using ColumnSeries = OxyPlot.Series.ColumnSeries;

namespace PlotsVisualizer.ViewModels
{
    public class HistogramViewModel : BindableBase
    {
        private int _domainsCount = 10;
        private PlotModel _histogram;

        public HistogramViewModel(Types.Signal signal)
        {
            Signal = signal ?? throw new ArgumentNullException(nameof(signal));
            CalculateCommand = new RelayCommand(CalculateHistogram, () => DomainsCount > 1 && DomainsCount < 100);
            SavePlotImageCommand = new RelayCommand(SavePlotImage);

            CalculateHistogram();
        }

        public IRaiseCanExecuteCommand CalculateCommand { get; }
        public IRaiseCanExecuteCommand SavePlotImageCommand { get; }

        public Types.Signal Signal { get; set; }

        public PlotModel Histogram
        {
            get => _histogram;
            set => SetProperty(ref _histogram, value);
        }

        public int DomainsCount
        {
            get => _domainsCount;
            set => SetProperty(ref _domainsCount, value);
        }

        private void CalculateHistogram()
        {
            var points = Signal.points.ToList();
            points.Sort((p, n) => p.y.r.CompareTo(n.y.r));

            (double min, double max) = (points.First().y.r, points.Last().y.r);
            double domainWidth = (max - min) / DomainsCount;
            double[] domainsCounts = CalculateDomainCounts(points, min, domainWidth);
            string[] domainNames = GenerateDomainNames(min, domainWidth);

            var model = new PlotModel { Title = $"{Signal.metadata.amplitude:F4}" };
            var series = new ColumnSeries();
            for (int i = 0; i < domainsCounts.Length; i++)
            {
                series.Items.Add(new ColumnItem(domainsCounts[i]));
            }
            model.Series.Add(series);

            model.Axes.Add(new CategoryAxis()
            {
                ItemsSource = domainNames,
                FontSize = 9
            });
            Histogram = model;
        }

        private string[] GenerateDomainNames(double min, double domainWidth)
        {
            string[] domainNames = new string[DomainsCount];
            double lowerBound = min;
            for (int i = 0; i < DomainsCount; i++)
            {
                domainNames[i] = $"{lowerBound:F2} {lowerBound + domainWidth:F2}";
                lowerBound += domainWidth;
            }

            return domainNames;
        }

        private double[] CalculateDomainCounts(System.Collections.Generic.List<Types.Point> points, double min, double domainWidth)
        {
            double[] domainsCounts = new double[DomainsCount];
            double upperBound = min + domainWidth;
            int domainNumber = 0;
            for (int i = 0; i < points.Count; i++)
            {
                while (points[i].y.r >= upperBound && domainNumber < DomainsCount - 1)
                {
                    domainNumber++;
                    upperBound += domainWidth;
                }

                domainsCounts[domainNumber]++;
            }

            return domainsCounts;
        }

        private void SavePlotImage()
        {
            if (Histogram != null)
            {
                var pngExporter = new PngExporter { Width = 1200, Height = 800, Background = OxyColors.White };
                string title = Signal.metadata == null
                    ? $"Metadata unavailable"
                    : $"{Signal.metadata.signalType}{(Signal.metadata.isContinous ? "Continous" : "Discrete")} f_sig {Signal.metadata.signalFrequency:0.##} f_sam {Signal.metadata.samplingFrequency:0.##}";

                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "histograms");
                Directory.CreateDirectory(filePath);
                filePath = Path.Combine(filePath, Path.ChangeExtension(title, "png"));
                pngExporter.ExportToFile(Histogram, filePath);
            }
        }
    }
}
