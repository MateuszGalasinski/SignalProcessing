using LiveCharts;
using SignalProcessing;
using System;
using UILogic.Base;

namespace PlotsVisualizer.Models
{
    public class Plot : BindableBase
    {
        private SeriesCollection _realSeries;
        private SeriesCollection _imaginarySeries;
        private string[] _labels;
        private Types.Signal _signal;

        public Plot(SeriesCollection realSeries, SeriesCollection imaginarySeries, Types.Signal signal, string[] labels)
        {
            RealSeries = realSeries ?? throw new ArgumentNullException(nameof(realSeries));
            ImaginarySeries = imaginarySeries ?? throw new ArgumentNullException(nameof(imaginarySeries));
            Signal = signal ?? throw new ArgumentNullException(nameof(signal));
            Labels = labels ?? throw new ArgumentNullException(nameof(labels));
        }

        public SeriesCollection RealSeries
        {
            get => _realSeries;
            set => SetProperty(ref _realSeries, value);
        }

        public SeriesCollection ImaginarySeries
        {
            get => _imaginarySeries;
            set => SetProperty(ref _imaginarySeries, value);
        }


        public Types.Signal Signal
        {
            get => _signal;
            set => SetProperty(ref _signal, value);
        }

        public string[] Labels
        {
            get => _labels;
            set => SetProperty(ref _labels, value);
        }
    }
}
