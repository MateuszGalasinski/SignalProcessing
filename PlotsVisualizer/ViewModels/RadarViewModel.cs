using Microsoft.FSharp.Collections;
using OxyPlot;
using OxyPlot.Series;
using SignalProcessing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using UILogic.Base;

namespace PlotsVisualizer.ViewModels
{
    public class RadarViewModel : BindableBase
    {
        private PlotModel _objectsPlotModel = new PlotModel()
        {
            Title = "Calculated positions"
        };
        private List<PlotModel> _correlationPlots = new List<PlotModel>();
        private int _currentCorrelationIndex = -1;
        private PlotModel _correlationPlotModel = new PlotModel()
        {
            Title = "Correlation"
        };
        private PlotModel _signalPlotModel = new PlotModel()
        {
            Title = "Signal plot"
        };

        private const double sampling = 900;
        private const double duration = 0.5;

        public RadarViewModel()
        {
            var lowFreqSig = SignalGeneration.signalGenerator(new Types.SignalMetadata(
                Types.SignalType.Sin,
                false,
                amplitude: 2,
                startTime: 0,
                duration: duration,
                dutyCycle: 0.5,
                signalFrequency: 19,
                samplingFrequency: sampling));
            var lowFreqSig2 = SignalGeneration.signalGenerator(new Types.SignalMetadata(
                Types.SignalType.Sin,
                false,
                amplitude: 1,
                startTime: 0,
                duration: duration,
                dutyCycle: 0.5,
                signalFrequency: 71,
                samplingFrequency: sampling));
            var lowFreqSig3 = SignalGeneration.signalGenerator(new Types.SignalMetadata(
                Types.SignalType.Sin,
                false,
                amplitude: 1,
                startTime: 0,
                duration: duration,
                dutyCycle: 0.5,
                signalFrequency: 127,
                samplingFrequency: sampling));
            var lowFreqSig4 = SignalGeneration.signalGenerator(new Types.SignalMetadata(
                Types.SignalType.Sin,
                false,
                amplitude: 1,
                startTime: 0,
                duration: duration,
                dutyCycle: 0.5,
                signalFrequency: 157,
                samplingFrequency: sampling));

            RadarSignal = Operations.operate(Operations.OperationType.Addition,
                lowFreqSig4,
                lowFreqSig
            );

            SignalPlotModel.Series.Add(CreateSeries(RadarSignal.points, OxyColors.Black));
            SignalPlotModel.InvalidatePlot(true);

            SimulateCommand = new RelayCommand(Simulate);
            NextPlotCommand = new RelayCommand(MoveToNextPlot);
            PreviousPlotCommand = new RelayCommand(MoveToPreviousPlot);
        }

        public PlotModel ObjectsPlotModel
        {
            get => _objectsPlotModel;
            set => SetProperty(ref _objectsPlotModel, value);
        }

        public PlotModel CorrelationPlotModel
        {
            get => _correlationPlotModel;
            set => SetProperty(ref _correlationPlotModel, value);
        }

        public PlotModel SignalPlotModel
        {
            get => _signalPlotModel;
            set => SetProperty(ref _signalPlotModel, value);
        }

        public Types.Signal RadarSignal { get; set; }
        public double SignalVelocity { get; set; } = 3000;
        public double ObjectVelocity { get; set; } = 30;
        public double StartingDistance { get; set; } = 100;

        public double SimulationTime { get; set; } = 3;
        public double SimulationStep { get; set; } = 0.3;

        public IRaiseCanExecuteCommand SimulateCommand { get; }
        public IRaiseCanExecuteCommand NextPlotCommand { get; }
        public IRaiseCanExecuteCommand PreviousPlotCommand { get; }

        public void Simulate()
        {
            var calculatedPositionSeries = new LineSeries { LineStyle = LineStyle.None, MarkerType = MarkerType.Circle, MarkerSize = 3, MarkerFill = OxyColors.SlateGray };
            ConcurrentBag<DataPoint> distancePoints = new ConcurrentBag<DataPoint>();
            ConcurrentBag<(PlotModel plot, double distance)> correlationPlots = new ConcurrentBag<(PlotModel plot, double distance)>();
            //var realPositionSeries = new LineSeries { LineStyle = LineStyle.Dash, MarkerType = MarkerType.Triangle, MarkerSize = 1.5, MarkerFill = OxyColors.SlateGray };
            Enumerable.Range(0, (int)(SimulationTime / SimulationStep))
                .AsParallel()
                .ForAll(stepIndex =>
                {
                    double currentTime = (stepIndex * SimulationStep);
                    double currentDistance = StartingDistance - (currentTime * ObjectVelocity);
                    double calculatedDistance = CalculateDistance(currentDistance, correlationPlots);

                    distancePoints.Add(new DataPoint(currentTime, calculatedDistance));
                });
            var sortedPoints = distancePoints.ToList();
            sortedPoints.Sort((p, n) => p.X.CompareTo(n.X));
            calculatedPositionSeries.Points.AddRange(distancePoints);
            ObjectsPlotModel.Series.Add(calculatedPositionSeries);
            ObjectsPlotModel.InvalidatePlot(true);

            var sortedCorrelation = correlationPlots.ToList();;
            sortedCorrelation.Sort((p, n) => -p.distance.CompareTo(n.distance));
            _correlationPlots = sortedCorrelation.Select(p => p.plot).ToList();
            CorrelationPlotModel = _correlationPlots.First();
            _currentCorrelationIndex = 0;
            CorrelationPlotModel.InvalidatePlot(true);
        }

        private double CalculateDistance(double currentDistance, ConcurrentBag<(PlotModel, double)> correlationPlot)
        {
            int samplesToMove = (int)(currentDistance / SignalVelocity * RadarSignal.metadata.samplingFrequency * 2);
            int samplesLeft = RadarSignal.points.Length - samplesToMove;
            var pointsLeft = RadarSignal.points.Take(samplesLeft).ToList();
            var receivedSignal = RadarSignal.points.Skip(samplesLeft).ToList();
            receivedSignal.AddRange(pointsLeft);
            var correlationSignal = Convolution.simpleConvolute(RadarSignal.points, ListModule.OfSeq(receivedSignal));
            correlationPlot.Add((new PlotModel()
            {
                Series = { CreateSeries(correlationSignal, OxyColors.DarkBlue, $"Correlation, distance: {currentDistance}") }
            }, currentDistance));

            var correlationSignalList = correlationSignal.ToList();
            var rightHalf = correlationSignalList
                .Skip((correlationSignalList.Count - 1) / 2)
                .Select(p => p.y.r)
                .ToList();
            int maximum = rightHalf.FindIndex(c => c == rightHalf.Max());
            var calculatedDistance = SignalVelocity * (maximum / RadarSignal.metadata.samplingFrequency) / 2;
            return calculatedDistance;
        }

        private LineSeries CreateSeries(FSharpList<Types.Point> points, OxyColor color, string title = "default")
        {
            LineSeries series = new LineSeries { LineStyle = LineStyle.Solid, MarkerType = MarkerType.Circle, MarkerSize = 1.5, MarkerFill = color, Title = title };
            int step = (int)Math.Ceiling((double)points.Length / 20000);
            for (int i = 0; i < points.Length; i += step)
            {
                var point = points[i];
                series.Points.Add(new DataPoint(point.x, point.y.r));
            }

            return series;
        }

        private void MoveToNextPlot()
        {
            if (_currentCorrelationIndex + 1 < _correlationPlots.Count)
            {
                _currentCorrelationIndex++;
                CorrelationPlotModel = _correlationPlots[_currentCorrelationIndex];
            }
            else
            {
                SystemSounds.Beep.Play();
            }
        }

        private void MoveToPreviousPlot()
        {
            if (_currentCorrelationIndex > 0)
            {
                _currentCorrelationIndex--;
                CorrelationPlotModel = _correlationPlots[_currentCorrelationIndex];
            }
            else
            {
                SystemSounds.Beep.Play();
            }
        }
    }
}
