using Microsoft.FSharp.Collections;
using OxyPlot;
using OxyPlot.Series;
using SignalProcessing;
using System;
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

        private const double sampling = 900;
        private const double duration = 0.5;

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

            RadarSignal = //( lowFreqSig

            Operations.operate(Operations.OperationType.Addition,
                lowFreqSig4,
                lowFreqSig

            //Operations.operate(
            //Operations.OperationType.Addition,
            //lowFreqSig,
            //Operations.operate(Operations.OperationType.Addition,
            //    lowFreqSig2,
            //    Operations.operate(Operations.OperationType.Addition,
            //        lowFreqSig3,
            //        lowFreqSig4))
            );

            SignalPlotModel.Series.Add(CreateSeries(RadarSignal.points, OxyColors.Black));
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
        public double SignalVelocity { get; set; } = 3000;
        public double ObjectVelocity { get; set; } = 30;
        public double StartingDistance { get; set; } = 100;

        public double SimulationTime { get; set; } = 3;
        public double SimulationStep { get; set; } = 0.3;

        public IRaiseCanExecuteCommand SimulateCommand { get; }

        public void Simulate()
        {
            var calculatedPositionSeries = new LineSeries { LineStyle = LineStyle.Dot, MarkerType = MarkerType.Circle, MarkerSize = 1.5, MarkerFill = OxyColors.SlateGray };
            var realPositionSeries = new LineSeries { LineStyle = LineStyle.Dash, MarkerType = MarkerType.Triangle, MarkerSize = 1.5, MarkerFill = OxyColors.SlateGray };

            //while (currentTime < SimulationTime)
            //{
            //    currentDistance = StartingDistance - (currentTime * ObjectVelocity);
            //    double signalTravelingTime = 2 * currentDistance / SignalVelocity;

            //    if (updatePositionCounter > UpdatePositionStep)
            //    {
            //        var (emitedSignal, receivedSignal) = GenerateSignals(currentTime, signalTravelingTime);

            //        calculatedPositionSeries.Points.Add(new DataPoint(currentTime, CalculateDistance(emitedSignal, receivedSignal)));
            //        realPositionSeries.Points.Add(new DataPoint(currentTime, currentDistance));

            //        if (updatePositionCounter >= UpdatePositionStep)
            //        {
            //            RealPositionPlotModel.Series.Add(CreateSeries(emitedSignal.points, OxyColors.SaddleBrown));
            //            RealPositionPlotModel.Series.Add(CreateSeries(receivedSignal.points, OxyColors.Red));
            //        }
            //        updatePositionCounter = 0;
            //    }
            //    else
            //    {
            //        updatePositionCounter++;
            //    }
            //    currentTime += SimulationStep;
            //}

            for (double currentTime = 0; currentTime < SimulationTime; currentTime += SimulationStep)
            {
                double currentDistance = StartingDistance - (currentTime * ObjectVelocity);
                double calculatedDistance = CalculateDistance(currentDistance);

                calculatedPositionSeries.Points.Add(new DataPoint(currentTime, calculatedDistance));
                realPositionSeries.Points.Add(new DataPoint(currentTime, currentDistance));
            }

            ObjectsPlotModel.Series.Add(calculatedPositionSeries);
            ObjectsPlotModel.InvalidatePlot(true);
            RealPositionPlotModel.Series.Add(realPositionSeries);
            RealPositionPlotModel.InvalidatePlot(true);
        }

        private double CalculateDistance(double currentDistance)
        {
            int samplesToMove = (int)(currentDistance / SignalVelocity * RadarSignal.metadata.samplingFrequency * 2);
            int samplesLeft = RadarSignal.points.Length - samplesToMove;
            var pointsLeft = RadarSignal.points.Take(samplesLeft).ToList();
            var receivedSignal = RadarSignal.points.Skip(samplesLeft).ToList();
            receivedSignal.AddRange(pointsLeft);
            var correlateSignal = Convolution.simpleConvolute(RadarSignal.points, ListModule.OfSeq(receivedSignal)).ToList();
            var rightHalf = correlateSignal
                .Skip((correlateSignal.Count - 1) / 2)
                .Select(p => p.y.r)
                .ToList();
            int maximum = rightHalf.FindIndex(c => c == rightHalf.Max());
            var calculatedDistance = SignalVelocity * (maximum / RadarSignal.metadata.samplingFrequency) / 2;
            return calculatedDistance;
        }

        private static (Types.Signal original, Types.Signal received) GenerateSignals(double currentTime, double signalTravelingTime)
        {

           
            var emitedSignal = Operations.operate(Operations.OperationType.Addition,
                SignalGeneration.signalGenerator(new Types.SignalMetadata(
                    Types.SignalType.Sin,
                    false,
                    amplitude: 1,
                    startTime: currentTime,
                    duration: duration,
                    dutyCycle: 0.5,
                    signalFrequency: 23,
                    samplingFrequency: sampling)),
                SignalGeneration.signalGenerator(new Types.SignalMetadata(
                    Types.SignalType.Sin,
                    false,
                    amplitude: 1,
                    startTime: currentTime,
                    duration: duration,
                    dutyCycle: 0.5,
                    signalFrequency: 71,
                    samplingFrequency: sampling)));

            var receivedSignal = Operations.operate(Operations.OperationType.Addition,
                SignalGeneration.signalGenerator(new Types.SignalMetadata(
                    Types.SignalType.Sin,
                    false,
                    amplitude: 1,
                    startTime: currentTime + signalTravelingTime,
                    duration: duration,
                    dutyCycle: 0.5,
                    signalFrequency: 23,
                    samplingFrequency: sampling)),
                SignalGeneration.signalGenerator(new Types.SignalMetadata(
                    Types.SignalType.Sin,
                    false,
                    amplitude: 1,
                    startTime: currentTime + signalTravelingTime,
                    duration: duration,
                    dutyCycle: 0.5,
                    signalFrequency: 71,
                    samplingFrequency: sampling)));
            return (emitedSignal, receivedSignal);
        }

        //private double CalculateDistance(Types.Signal original,
        //    Types.Signal received)
        //{
        //    var tempPoints = original.points.ToList();
        //    tempPoints.Reverse();
        //    var correlation = Convolution.convolutePoints(ListModule.OfSeq(tempPoints), ListModule.OfSeq(received.points));
        //    var half = correlation.ToList()
        //        .Skip(correlation.Length / 2)
        //        .Select(p => p.y.r)
        //        .ToList();
        //    var indexOfMaximum = half
        //        .IndexOf(half.Max());
        //    return SignalVelocity * (
        //               indexOfMaximum / RadarSignal.metadata.samplingFrequency
        //            ) / 2.0;
        //}

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
    }
}
