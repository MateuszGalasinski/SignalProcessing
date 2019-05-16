using Microsoft.FSharp.Collections;
using OxyPlot;
using OxyPlot.Series;
using SignalProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using UILogic.Base;

namespace PlotsVisualizer.ViewModels
{
    public class RadarViewModel : BindableBase
    {
        private PlotModel _objectsPlotModel = new PlotModel()
        {
            Title = "Calculated positions"
        };
        private PlotModel _realPositionPlotModel = new PlotModel()
        {
            Title = "Real positions"
        };
        private PlotModel _signalPlotModel = new PlotModel()
        {
            Title = "Signal plot"
        };

        private const int sampling = 500;
        private const int duration = 3;

        public RadarViewModel(Types.Signal radarSignal)
        {
            //RadarSignal = radarSignal ?? throw new ArgumentNullException(nameof(radarSignal));
            var lowFreqSig = SignalGeneration.signalGenerator(new Types.SignalMetadata(
                Types.SignalType.Sin,
                false,
                amplitude: 2,
                startTime: 0,
                duration: duration,
                dutyCycle: 0.5,
                signalFrequency: 23,
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

            RadarSignal = (lowFreqSig

            //Operations.operate(Operations.OperationType.Addition,
            //    lowFreqSig2,
            //    lowFreqSig

                //Operations.operate(
                //Operations.OperationType.Addition,
                //lowFreqSig,
                //Operations.operate(Operations.OperationType.Addition,
                //    lowFreqSig2,
                //    Operations.operate(Operations.OperationType.Addition,
                //        lowFreqSig3,
                //        lowFreqSig4))
            );

            SignalPlotModel.Series.Add(CreateSeries(RadarSignal.points));
            SignalPlotModel.InvalidatePlot(true);

            SimulateCommand = new RelayCommand(Simulate);
        }

        public PlotModel ObjectsPlotModel
        {
            get => _objectsPlotModel;
            set => SetProperty(ref _objectsPlotModel, value);
        }

        public PlotModel RealPositionPlotModel
        {
            get => _realPositionPlotModel;
            set => SetProperty(ref _realPositionPlotModel, value);
        }

        public PlotModel SignalPlotModel
        {
            get => _signalPlotModel;
            set => SetProperty(ref _signalPlotModel, value);
        }

        public Types.Signal RadarSignal { get; set; }
        public double SignalVelocity { get; set; } = 3000_000;
        public double ObjectVelocity { get; set; } = 0;
        public double StartingDistance { get; set; } = 10;

        public int BufferSize { get; set; } = 300;
        public int MinBufferedPoints { get; set; } = 10;

        public double SimulationTime { get; set; } = 2;
        public double SimulationStep { get; set; } = 0.01;
        public int UpdatePositionStep { get; set; } = 20;

        public IRaiseCanExecuteCommand SimulateCommand { get; }

        public void Simulate()
        {
            var calculatedPositionSeries = new LineSeries { LineStyle = LineStyle.Dot, MarkerType = MarkerType.Circle, MarkerSize = 1.5, MarkerFill = OxyColors.SlateGray };
            var realPositionSeries = new LineSeries { LineStyle = LineStyle.Dash, MarkerType = MarkerType.Triangle, MarkerSize = 1.5, MarkerFill = OxyColors.SlateGray };

            LinkedList<Types.Point> generatedSignalBuffer = new LinkedList<Types.Point>();
            LinkedList<Types.Point> receivedSignalBuffer = new LinkedList<Types.Point>();
            List<>
            int bufferIndex = 0;
            int updatePositionCounter = 0;
            double currentTime = 0;
            double currentDistance = StartingDistance;
            while (currentTime < SimulationTime)
            {
                // calc current signal value
                var generatedSignalPoint = CalculateSignalValue(currentTime);
                generatedSignalBuffer.AddLast(generatedSignalPoint);

                currentDistance = StartingDistance - (currentTime * ObjectVelocity);
                double signalTravelingTime = 2 * currentDistance / SignalVelocity;
                var receivedSignalPoint = CalculateSignalValue(currentTime + signalTravelingTime);
                receivedSignalBuffer.AddLast(receivedSignalPoint);

                if (bufferIndex >= BufferSize)
                {
                    generatedSignalBuffer.RemoveFirst();
                    receivedSignalBuffer.RemoveFirst();
                }
                else
                {
                    bufferIndex++;
                }

                // update position
                if (bufferIndex > MinBufferedPoints)
                {
                    if (updatePositionCounter > UpdatePositionStep)
                    {
                        calculatedPositionSeries.Points.Add(new DataPoint(currentTime, CalculateDistance(generatedSignalBuffer, receivedSignalBuffer)));
                        realPositionSeries.Points.Add(new DataPoint(currentTime, currentDistance));
                        updatePositionCounter = 0;
                    }
                    else
                    {
                        updatePositionCounter++;
                    }
                }

                currentTime += SimulationStep;
            }

            ObjectsPlotModel.Series.Add(calculatedPositionSeries);
            ObjectsPlotModel.InvalidatePlot(true);
            RealPositionPlotModel.Series.Add(realPositionSeries);
            RealPositionPlotModel.InvalidatePlot(true);
        }

        /// <summary>
        /// Assumes signal is generated at least for one full cycle
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private Types.Point CalculateSignalValue(double time)
        {
            //double timeOffset = time - (time / RadarSignal.metadata.signalFrequency);
            //int sampleNumber = (int)(timeOffset * RadarSignal.metadata.samplingFrequency);
            int sampleNumber = (int)(time * RadarSignal.metadata.samplingFrequency);
            return RadarSignal.points[sampleNumber];
        }

        private double CalculateDistance(LinkedList<Types.Point> original,
            LinkedList<Types.Point> received)
        {
            var tempPoints = original.ToList();
            tempPoints.Reverse();
            var correlation = Convolution.convolutePoints(ListModule.OfSeq(tempPoints), ListModule.OfSeq(received));
            var half = correlation.ToList()
                .Skip(correlation.Length / 2)
                .Select(p => p.y.r)
                .ToList();
            var indexOfMaximum = half
                .IndexOf(half.Max());
            return SignalVelocity
                   * indexOfMaximum
                   / RadarSignal.metadata.samplingFrequency // TODO :::::::
                   / 2.0;
        }

        private LineSeries CreateSeries(FSharpList<Types.Point> points)
        {
            LineSeries series = new LineSeries { LineStyle = LineStyle.None, MarkerType = MarkerType.Circle, MarkerSize = 1.5, MarkerFill = OxyColors.SlateGray };
            int step = (int)Math.Ceiling((double)points.Length / 20000);
            for (int i = 0; i < points.Length; i += step)
            {
                var point = points[i];
                series.Points.Add(new DataPoint(point.x, point.y.r));
            }

            return series;
        }
    }
}
