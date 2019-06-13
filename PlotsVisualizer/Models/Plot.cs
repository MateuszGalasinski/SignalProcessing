using System;
using LiveCharts;
using SignalProcessing;
using UILogic.Base;

namespace PlotsVisualizer.Models
{
    public class Plot : BindableBase
    {
        private SeriesCollection _topSeries;
        private SeriesCollection _bottomSeries;
        private readonly (SeriesCollection r, SeriesCollection i) _w1Pair;
        private readonly (SeriesCollection m, SeriesCollection x) _w2Pair;
        private readonly (string[] w1, string[] w2) _labelsPair;
        private string[] _labels;
        private Types.Signal _signal;

        public bool IsW1 { get; private set; } = true;

        public Plot(Types.Signal signal, string[] labels, string[] frequencyLabels,
            (SeriesCollection r, SeriesCollection i) w1Pair,
            (SeriesCollection m, SeriesCollection x) w2Pair)
        {
            TopSeries = _w1Pair.r;
            BottomSeries = _w1Pair.i;
            Signal = signal ?? throw new ArgumentNullException(nameof(signal));
            Labels = labels ?? throw new ArgumentNullException(nameof(labels));
            _labelsPair = (labels, frequencyLabels);
            _w1Pair = w1Pair;
            _w2Pair = w2Pair;
        }

        public SeriesCollection TopSeries
        {
            get => _topSeries;
            set => SetProperty(ref _topSeries, value);
        }

        public SeriesCollection BottomSeries
        {
            get => _bottomSeries;
            set => SetProperty(ref _bottomSeries, value);
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

        public void SwitchW()
        {
            if (IsW1)
            {
                TopSeries = _w2Pair.x;
                BottomSeries = _w2Pair.m;
                Labels = _labelsPair.w2;
                IsW1 = false;
            }
            else
            {
                TopSeries = _w1Pair.r;
                BottomSeries = _w1Pair.i;
                Labels = _labelsPair.w1;
                IsW1 = true;
            }
        }
    }
}
