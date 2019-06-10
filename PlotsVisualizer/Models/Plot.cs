using LiveCharts;
using SignalProcessing;
using System;
using UILogic.Base;

namespace PlotsVisualizer.Models
{
    public class Plot : BindableBase
    {
        private SeriesCollection _series;
        private string[] _labels;
        private Types.Signal _signal;

        public Plot(SeriesCollection series, Types.Signal signal, string[] labels)
        {
            SeriesCollection = series ?? throw new ArgumentNullException(nameof(series));
            Signal = signal ?? throw new ArgumentNullException(nameof(signal));
            Labels = labels ?? throw new ArgumentNullException(nameof(labels));
        }

        public SeriesCollection SeriesCollection
        {
            get => _series;
            set => SetProperty(ref _series, value);
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
