using Microsoft.FSharp.Collections;
using Microsoft.Win32;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Wpf;
using PlotsVisualizer.Views;
using SignalProcessing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using UILogic.Base;
using LineSeries = OxyPlot.Series.LineSeries;
using Plot = PlotsVisualizer.Models.Plot;

namespace PlotsVisualizer.ViewModels
{
    class MainViewModel : BindableBase
    {
        private const int stepDivisor = 10000;

        private Plot _currentPlotModel;
        private int _currentPlotIndex = -1;
        private string _fileName = "my favorite plot";
        private int _plotsCount = 0;
        private double _amplitude = 1;
        private double _startTime = 0;
        private double _duration = 2;
        private double _dutyCycle = 0.5;
        private double _signalFrequency = 1;
        private double _samplingFrequency = 10;
        private Types.SignalType _signalType = Types.SignalType.Sin;
        private int _firstChosenPlot = 0;
        private int _secondChosenPlot = 1;
        private int _firstToCompare = 0;
        private int _secondToCompare = 1;
        private bool _isContinous;
        private int _quantizationBits = 2;
        private double _extrapolationFrequency = 2000;
        private int _extrapolationNeighboursCount = 4;
        private int _filterOrder = 5;
        private Filters.FilterType _filterType = Filters.FilterType.LowPass;
        private Filters.WindowFunction _windowFunction = Filters.WindowFunction.Blackman;

        private List<Plot> Plots { get; } = new List<Plot>();


        #region observable props
        public int CurrentPlotIndex
        {
            get => _currentPlotIndex;
            private set => SetProperty(ref _currentPlotIndex, value);
        }
        public Plot CurrentPlot
        {
            get => _currentPlotModel;
            private set => SetProperty(ref _currentPlotModel, value);
        }
        public int PlotsCount
        {
            get => _plotsCount;
            set => SetProperty(ref _plotsCount, value);
        }
        public string FileName
        {
            get => _fileName;
            set
            {
                SetProperty(ref _fileName, value);
                LoadPlotCommand.RaiseCanExecuteChanged();
            }
        }
        public double Amplitude
        {
            get => _amplitude;
            set => SetProperty(ref _amplitude, value);
        }
        public double StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }
        public double Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }
        public double DutyCycle
        {
            get => _dutyCycle;
            set => SetProperty(ref _dutyCycle, value);
        }
        public double SignalFrequency
        {
            get => _signalFrequency;
            set => SetProperty(ref _signalFrequency, value);
        }
        public double SamplingFrequency
        {
            get => _samplingFrequency;
            set => SetProperty(ref _samplingFrequency, value);
        }
        public Types.SignalType SignalType
        {
            get => _signalType;
            set => SetProperty(ref _signalType, value);
        }
        public int FirstChosenPlot
        {
            get => _firstChosenPlot;
            set => SetProperty(ref _firstChosenPlot, value);
        }
        public int SecondChosenPlot
        {
            get => _secondChosenPlot;
            set => SetProperty(ref _secondChosenPlot, value);
        }
        public int FirstToCompare
        {
            get => _firstToCompare;
            set => SetProperty(ref _firstToCompare, value);
        }
        public int SecondToCompare
        {
            get => _secondToCompare;
            set => SetProperty(ref _secondToCompare, value);
        }
        public int ExtrapolationNeighboursCount
        {
            get => _extrapolationNeighboursCount;
            set => SetProperty(ref _extrapolationNeighboursCount, value);
        }

        public bool IsContinous
        {
            get => _isContinous;
            set => SetProperty(ref _isContinous, value);
        }
        public int QuantizationBits
        {
            get => _quantizationBits;
            set => SetProperty(ref _quantizationBits, value);
        }
        public double ExtrapolationFrequency
        {
            get => _extrapolationFrequency;
            set => SetProperty(ref _extrapolationFrequency, value);
        }

        public int FilterOrder
        {
            get => _filterOrder;
            set => SetProperty(ref _filterOrder, value);
        }
        public Filters.FilterType FilterType
        {
            get => _filterType;
            set => SetProperty(ref _filterType, value);
        }
        public Filters.WindowFunction WindowFunction
        {
            get => _windowFunction;
            set => SetProperty(ref _windowFunction, value);
        }
        #endregion

        #region Commands
        public IRaiseCanExecuteCommand NextPlotCommand { get; }
        public IRaiseCanExecuteCommand PreviousPlotCommand { get; }

        public IRaiseCanExecuteCommand ReadFilePathCommand { get; }
        public IRaiseCanExecuteCommand SavePlotCommand { get; }
        public IRaiseCanExecuteCommand LoadPlotCommand { get; }

        public IRaiseCanExecuteCommand AddPlotCommand { get; }
        public IRaiseCanExecuteCommand DeleteCommand { get; }

        public IRaiseCanExecuteCommand AddCommand { get; }
        public IRaiseCanExecuteCommand SubstractCommand { get; }
        public IRaiseCanExecuteCommand MultiplyCommand { get; }
        public IRaiseCanExecuteCommand DivideCommand { get; }
        public IRaiseCanExecuteCommand QuantizeCommand { get; }
        public IRaiseCanExecuteCommand ExtrapolateZeroCommand { get; }
        public IRaiseCanExecuteCommand ExtrapolateSincCommand { get; }
        public IRaiseCanExecuteCommand ShowErrorsCommand { get; }

        public IRaiseCanExecuteCommand ShowStatsCommand { get; }
        public IRaiseCanExecuteCommand ShowHistogramCommand { get; }
        public IRaiseCanExecuteCommand SavePlotImageCommand { get; }

        public IRaiseCanExecuteCommand ConvoluteCommand { get; }
        public IRaiseCanExecuteCommand GenerateFilterCommand { get; }
        #endregion

        public MainViewModel()
        {
            NextPlotCommand = new RelayCommand(MoveToNextPlot);
            PreviousPlotCommand = new RelayCommand(MoveToPreviousPlot);
            ReadFilePathCommand = new RelayCommand(ReadFilePath);
            SavePlotCommand = new RelayCommand(SaveToFile, () => !string.IsNullOrWhiteSpace(FileName));
            LoadPlotCommand = new RelayCommand(LoadFromFile, () => File.Exists(FileName));
            AddPlotCommand = new RelayCommand(AddNewPlot);
            DeleteCommand = new RelayCommand(() => RemovePlot(CurrentPlotIndex));
            AddCommand = new RelayCommand(() => PlotOperate(Operations.OperationType.Addition));
            SubstractCommand = new RelayCommand(() => PlotOperate(Operations.OperationType.Substraction));
            MultiplyCommand = new RelayCommand(() => PlotOperate(Operations.OperationType.Multiplication));
            DivideCommand = new RelayCommand(() => PlotOperate(Operations.OperationType.Division));
            QuantizeCommand = new RelayCommand(Quantize);
            ExtrapolateZeroCommand = new RelayCommand(ExtrapolateZero);
            ShowStatsCommand = new RelayCommand(ShowStats);
            ShowHistogramCommand = new RelayCommand(ShowHistogram);
            SavePlotImageCommand = new RelayCommand(SavePlotImage);
            ExtrapolateSincCommand = new RelayCommand(ExtrapolateSinc);
            ShowErrorsCommand = new RelayCommand(ShowErrors);
            ConvoluteCommand = new RelayCommand(Convolute);
            GenerateFilterCommand = new RelayCommand(GenerateFilter);
        }

        #region plot navigation
        private void MoveToNextPlot()
        {
            if (CurrentPlotIndex + 1 < Plots.Count)
            {
                CurrentPlotIndex++;
                CurrentPlot = Plots[CurrentPlotIndex];
            }
            else
            {
                SystemSounds.Beep.Play();
            }
        }

        private void MoveToPreviousPlot()
        {
            if (CurrentPlotIndex > 0)
            {
                CurrentPlotIndex--;
                CurrentPlot = Plots[CurrentPlotIndex];
            }
            else
            {
                SystemSounds.Beep.Play();
            }
        }

        private void MoveToPlot(int index)
        {
            if (Plots.Count > index)
            {
                CurrentPlotIndex = index;
                CurrentPlot = Plots[CurrentPlotIndex];
            }
        }
        #endregion

        #region file save/load

        private void ReadFilePath()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            FileName = fileDialog.ShowDialog() == true ? fileDialog.FileName : string.Empty;
        }

        private void SaveToFile()
        {
            if (Plots.Count < 1)
            {
                MessageBox.Show("Create some plot first.");
                return;
            }

            string path = Path.Combine(Directory.GetCurrentDirectory(), FileName);
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                var pointsToSave = Plots[CurrentPlotIndex].Signal.points;
                binaryFormatter.Serialize(fileStream, pointsToSave);
            }
            using (StreamWriter file = File.CreateText(Path.ChangeExtension(path, "json")))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, Plots[CurrentPlotIndex].Signal);
            }
            SystemSounds.Beep.Play();
        }

        private void LoadFromFile()
        {
            try
            {
                string path = Path.ChangeExtension(Path.Combine(Directory.GetCurrentDirectory(), FileName), "json");

                Types.Signal signal = null;
                using (StreamReader file = File.OpenText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    signal = (Types.Signal)serializer.Deserialize(file, typeof(Types.Signal));
                }

                string title = signal.metadata == null
                    ? $"Metadata unavailable"
                    : $"{signal.metadata.signalType} {(signal.metadata.isContinous ? "Continous" : "Discrete")} f_sig: {signal.metadata.signalFrequency:0.##} f_sam: {signal.metadata.samplingFrequency:0.##}";

                AddPlot(CreatePlot(signal.points, title), signal);
                MoveToPlot(Plots.Count - 1);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                MessageBox.Show("Could not load plot from specified file.");
            }

            SystemSounds.Beep.Play();
        }

        private void SavePlotImage()
        {
            if (CurrentPlot?.PlotModel != null)
            {
                var pngExporter = new PngExporter { Width = 1200, Height = 800, Background = OxyColors.White };
                string title = CurrentPlot.Signal.metadata == null
                    ? $"Metadata unavailable"
                    : $"{CurrentPlot.Signal.metadata.signalType}{(CurrentPlot.Signal.metadata.isContinous ? "Continous" : "Discrete")} f_sig {CurrentPlot.Signal.metadata.signalFrequency:0.##} f_sam {CurrentPlot.Signal.metadata.samplingFrequency:0.##}";

                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "plots");
                Directory.CreateDirectory(filePath);
                filePath = Path.Combine(filePath, Path.ChangeExtension(title, "png"));
                pngExporter.ExportToFile(CurrentPlot.PlotModel, filePath);
            }
        }
        #endregion

        private void RemovePlot(int index)
        {
            if (index >= 0 && index <= PlotsCount)
            {
                Plots.Remove(Plots[index]);
                PlotsCount = Plots.Count - 1;

                if (CurrentPlotIndex >= Plots.Count)
                {
                    CurrentPlotIndex--;
                }
                if (CurrentPlotIndex >= 0)
                {
                    CurrentPlot = Plots[CurrentPlotIndex];
                }
                else
                {
                    CurrentPlot = null;
                }
            }
            else
            {
                SystemSounds.Beep.Play();
            }
        }

        private void AddPlot(PlotModel plot, Types.Signal signal)
        {
            Plots.Add(new Plot()
            {
                PlotModel = plot,
                Signal = signal
            });
            PlotsCount = Plots.Count - 1;
            MoveToPlot(PlotsCount);
        }

        private PlotModel CreatePlot(FSharpList<Types.Point> points, string title)
        {
            var plot = new PlotModel { Title = title };
            var series = new LineSeries { LineStyle = LineStyle.None, MarkerType = MarkerType.Circle, MarkerSize = 1.5, MarkerFill = OxyColors.SlateGray };
            int step = (int)Math.Ceiling((double)points.Length / stepDivisor);
            Types.Point point = null;
            for (int i = 0; i < points.Length; i += step)
            {
                point = points[i];
                series.Points.Add(new DataPoint(point.x, point.y.r));
            }
            plot.Series.Add(series);

            return plot;
        }

        private void AddNewPlot()
        {
            var signal = SignalGeneration.signalGenerator(new Types.SignalMetadata(SignalType, IsContinous, Amplitude, StartTime, Duration, DutyCycle, SignalFrequency, SamplingFrequency));

            AddPlot(
                CreatePlot(signal.points, $"{SignalType} {(signal.metadata.isContinous ? "Continous" : "Discrete")} A: {Amplitude:0.##} f_sig: {SignalFrequency:0.##} f_sam: {SamplingFrequency:0.##}"),
                signal);

            SystemSounds.Beep.Play();
        }

        private void PlotOperate(Operations.OperationType type)
        {
            if (FirstChosenPlot > PlotsCount || SecondChosenPlot > PlotsCount)
            {
                MessageBox.Show($"Invalid plots indexes. Maximum plot index is: {PlotsCount}");
                return;
            }

            if (FirstChosenPlot == SecondChosenPlot)
            {
                MessageBox.Show($"Choose two different plots.");
                return;
            }

            var first = Plots[FirstChosenPlot].Signal;
            var second = Plots[SecondChosenPlot].Signal;

            if (first.points.Length != second.points.Length)
            {
                MessageBox.Show("Chosen plots are incompatible for operation.");
                return;
            }

            for (int i = 0; i < first.points.Length; i++)
            {
                if (first.points[i].x != second.points[i].x)
                {
                    MessageBox.Show("Plots have different x values.");
                    return;
                }
            }

            try
            {
                var newSignal = Operations.operate(type, first, second);
                if (FirstChosenPlot > SecondChosenPlot)
                {
                    RemovePlot(FirstChosenPlot);
                    RemovePlot(SecondChosenPlot);
                }
                else
                {
                    RemovePlot(SecondChosenPlot);
                    RemovePlot(FirstChosenPlot);
                }

                string title = first.metadata == null
                    ? $"{type} metadata unavailable"
                    : $"{type} {(first.metadata.isContinous ? "Continous" : "Discrete")} f_sig: {first.metadata.signalFrequency:0.##} f_sam: {first.metadata.samplingFrequency:0.##}";
                AddPlot(
                    CreatePlot(newSignal.points, title),
                    newSignal);
            }
            catch (Exception)
            {
                MessageBox.Show("Could not operate successfully on these plots. Cleaned up plots.");
                while (Plots.Count != 0)
                {
                    RemovePlot(PlotsCount - 1);
                }
            }
        }

        #region quantize
        private void Quantize()
        {
            if (CurrentPlot == null)
            {
                return;
            }
            try
            {
                var newPoints = Quantization.quantizate(CurrentPlot.Signal.points, QuantizationBits);
                var meta = CurrentPlot.Signal.metadata;
                string title = meta == null
                    ? $"{meta.startTime} metadata unavailable"
                    : $"{meta.startTime} {(meta.isContinous ? "Continous" : "Discrete")} f_sig: {meta.signalFrequency:0.##} f_sam: {meta.samplingFrequency:0.##}";
                var newSignal = new Types.Signal(meta, newPoints);
                RemovePlot(CurrentPlotIndex);
                AddPlot(
                    CreatePlot(newSignal.points, title),
                    newSignal);
            }
            catch (Exception)
            {
                MessageBox.Show("Could not operate successfully on these plots. Cleaned up plots.");
            }
        }
        #endregion


        #region new Windows
        private void ShowStats()
        {
            if (CurrentPlot != null)
            {
                SignalParametersWindow statsWindow = new SignalParametersWindow(new SignalParametersViewModel(CurrentPlot.Signal));
                statsWindow.Show();
            }
        }

        private void ShowHistogram()
        {
            if (CurrentPlot != null)
            {
                HistogramWindow histogramWindow = new HistogramWindow(new HistogramViewModel(CurrentPlot.Signal));
                histogramWindow.Show();
            }
        }

        private void ShowErrors()
        {
            if (FirstToCompare > PlotsCount || SecondToCompare > PlotsCount)
            {
                MessageBox.Show($"Invalid plots indexes. Maximum plot index is: {PlotsCount}");
                return;
            }

            if (FirstToCompare == SecondToCompare)
            {
                MessageBox.Show($"Choose two different plots.");
                return;
            }

            var first = Plots[FirstToCompare].Signal;
            var second = Plots[SecondToCompare].Signal;

            if (first.points.Length != second.points.Length)
            {
                MessageBox.Show("Chosen plots are incompatible for operation.");
                return;
            }

            for (int i = 0; i < first.points.Length; i++)
            {
                if (first.points[i].x != second.points[i].x)
                {
                    MessageBox.Show("Plots have different x values.");
                    return;
                }
            }

            ErrorWindow window = new ErrorWindow(new ErrorsViewModel(first, second, ExtrapolationNeighboursCount, first.metadata.samplingFrequency));
            window.Show();
        }
        #endregion

        #region extrapolation
        private void ExtrapolateZero()
        {
            if (CurrentPlot == null)
            {
                return;
            }
            try
            {
                var meta = CurrentPlot.Signal.metadata;
                var newPoints = Quantization.extrapolateZeroOrder(CurrentPlot.Signal.points,
                    meta.startTime,
                    meta.duration,
                    ExtrapolationFrequency
                );
                string title = meta == null
                    ? $"{meta.startTime} metadata unavailable"
                    : $"{meta.startTime} {(meta.isContinous ? "Continous" : "Discrete")} f_sig: {meta.signalFrequency:0.##} f_sam: {ExtrapolationFrequency:0.##}";
                RemovePlot(CurrentPlotIndex);
                var newSignal = new Types.Signal(meta, newPoints);
                AddPlot(
                    CreatePlot(newSignal.points, title),
                    newSignal);
            }
            catch (Exception)
            {
                MessageBox.Show("Could not operate successfully on these plots. Cleaned up plots.");
            }
        }

        private void GenerateFilter()
        {
            Types.Signal filter = Filters.generateFilter(Filters.WindowFunction.Hamming, 30, 0, 100);
            AddPlot(
                CreatePlot(filter.points, $"Filter: {FilterType} {WindowFunction} M: {30}"),
                filter);
        }

        private void Convolute()
        {
            if (FirstChosenPlot > PlotsCount || SecondChosenPlot > PlotsCount)
            {
                MessageBox.Show($"Invalid plots indexes. Maximum plot index is: {PlotsCount}");
                return;
            }

            if (FirstChosenPlot == SecondChosenPlot)
            {
                MessageBox.Show($"Choose two different plots.");
                return;
            }

            var first = Plots[FirstChosenPlot].Signal;
            var second = Plots[SecondChosenPlot].Signal;

            var convoluted = SignalProcessing.Convolution.convolute(first,second);

            AddPlot(
                CreatePlot(convoluted.points, "test plot"),
                convoluted);
            //var someSignal = SignalGeneration.signalGenerator(new Types.SignalMetadata(SignalType, IsContinous,
            //    Amplitude, StartTime, Duration, DutyCycle, SignalFrequency, SamplingFrequency));
            //var testSignal = SignalProcessing.Convolution.convolute(
            //    new Types.Signal(someSignal.metadata,
            //        ListModule.OfSeq(new List<Types.Point>()
            //        {
            //            new Types.Point(0, new Types.Complex(1, 0)),
            //            new Types.Point(1, new Types.Complex(2, 0)),
            //            new Types.Point(2, new Types.Complex(3, 0)),
            //            new Types.Point(3, new Types.Complex(4, 0)),
            //        })),
            //    new Types.Signal(someSignal.metadata,
            //        ListModule.OfSeq(new List<Types.Point>()
            //        {
            //            new Types.Point(0, new Types.Complex(5, 0)),
            //            new Types.Point(1, new Types.Complex(6, 0)),
            //            new Types.Point(2, new Types.Complex(7, 0)),
            //        })));

            //AddPlot(
            //    CreatePlot(testSignal.points, "test plot"),
            //    testSignal);
        }

        private void ExtrapolateSinc()
        {
            if (CurrentPlot == null)
            {
                return;
            }
            try
            {
                var meta = CurrentPlot.Signal.metadata;
                var newPoints = GenerateSincExtrapolation();
                string title = meta == null
                    ? $"{meta.startTime} metadata unavailable"
                    : $"extrapolated sinc {meta.startTime} {(meta.isContinous ? "Continous" : "Discrete")} f_sig: {meta.signalFrequency:0.##} f_sam: {ExtrapolationFrequency:0.##}";
                RemovePlot(CurrentPlotIndex);
                var newSignal = new Types.Signal(meta, ListModule.OfSeq(newPoints));
                AddPlot(
                    CreatePlot(newSignal.points, title),
                    newSignal);
            }
            catch (Exception)
            {
                MessageBox.Show("Could not operate successfully on these plots. Cleaned up plots.");
            }
        }

        private List<Types.Point> GenerateSincExtrapolation()
        {
            double newFreq = ExtrapolationFrequency;
            int howManyPoints = ExtrapolationNeighboursCount * 2;
            double startTime = CurrentPlot.Signal.metadata.startTime;
            double duration = CurrentPlot.Signal.metadata.duration;
            double samplingFreq = CurrentPlot.Signal.metadata.samplingFrequency;
            List<Types.Point> points = CurrentPlot.Signal.points.ToList();

            List<(Types.Point p, int n)> pointsWithN = new List<(Types.Point p, int n)>();
            for (int i = 0; i < points.Count; i++)
            {
                pointsWithN.Add((points[i], i));
            }

            ConcurrentBag<double> newX = new ConcurrentBag<double>(SignalGeneration.generateXValues(duration, newFreq, startTime).ToList());
            double T = 1.0 / samplingFreq;
            return newX
                .AsParallel()
                .Select(x =>
                new Types.Point(x, new Types.Complex(
                    pointsWithN
                        .OrderBy(p => Math.Abs(p.p.x - x))
                        .Take(Math.Min(howManyPoints, pointsWithN.Count))
                        .Select(c =>
                        {
                            return c.p.y.r* Sinc(x / T - c.n);
                        })
                        .Sum(),
                    0)))
                .OrderBy(p => p.x)
                .ToList();

            double Sinc(double t) 
                => t == 0 ? 1 : Math.Sin(Math.PI * t) / (Math.PI * t);
        }
        #endregion
    }
}