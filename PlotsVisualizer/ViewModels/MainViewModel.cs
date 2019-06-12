using LiveCharts;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.FSharp.Collections;
using Microsoft.Win32;
using Newtonsoft.Json;
using SignalProcessing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Numerics;
using System.Windows;
using UILogic.Base;
using Plot = PlotsVisualizer.Models.Plot;

namespace PlotsVisualizer.ViewModels
{
    class MainViewModel : BindableBase
    {
        private const int stepDivisor = 20000;

        private Plot _currentPlotModel;
        private int _currentPlotIndex = -1;
        private string _fileName = "my favorite plot";
        private int _plotsCount = 0;
        private double _amplitude = 1;
        private double _startTime = 0;
        private double _duration = 3;
        private double _dutyCycle = 0.5;
        private double _signalFrequency = 1;
        private double _samplingFrequency = 100;
        private Types.SignalType _signalType = Types.SignalType.Sin;
        private int _firstChosenPlot = 0;
        private int _secondChosenPlot = 1;
        private bool _isContinous;
        private int _filterOrder = 20;
        //private Filters.FilterType _filterType = Filters.FilterType.LowPass;
        //private Filters.WindowFunction _windowFunction = Filters.WindowFunction.Blackman;
        private double _filterCutOffFrequency = 5000;
        private SeriesCollection _seriesCollection;

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

        public SeriesCollection SeriesCollection
        {
            get => _seriesCollection;
            private set => SetProperty(ref _seriesCollection, value);
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

        public bool IsContinous
        {
            get => _isContinous;
            set => SetProperty(ref _isContinous, value);
        }

        //public int FilterOrder
        //{
        //    get => _filterOrder;
        //    set => SetProperty(ref _filterOrder, value);
        //}
        //public Filters.FilterType FilterType
        //{
        //    get => _filterType;
        //    set => SetProperty(ref _filterType, value);
        //}
        //public Filters.WindowFunction WindowFunction
        //{
        //    get => _windowFunction;
        //    set => SetProperty(ref _windowFunction, value);
        //}
        //public double FilterCutOffFrequency
        //{
        //    get => _filterCutOffFrequency;
        //    set => SetProperty(ref _filterCutOffFrequency, value);
        //}

        public Func<double, string> YFormatter { get;  }
        = value => value.ToString("F4");
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
            //SavePlotImageCommand = new RelayCommand(SavePlotImage);
            //ConvoluteCommand = new RelayCommand(Convolute);
            //GenerateFilterCommand = new RelayCommand(GenerateFilter);
            SeriesCollection = new SeriesCollection
            {
                new LiveCharts.Wpf.LineSeries
                {
                    Values = new ChartValues<double> { 3, 5, 7, 4 }
                },
                new LiveCharts.Wpf.ColumnSeries
                {
                    Values = new ChartValues<decimal> { 5, 6, 2, 7 }
                }
            };
            SimpleFourier();
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

                AddPlot(CreatePlot(signal, title));
                MoveToPlot(Plots.Count - 1);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                MessageBox.Show("Could not load plot from specified file.");
            }

            SystemSounds.Beep.Play();
        }

        //private void SavePlotImage()
        //{
        //    if (CurrentPlot?.PlotModel != null)
        //    {
        //        var pngExporter = new PngExporter { Width = 1200, Height = 800, Background = OxyColors.White };
        //        string title = CurrentPlot.PlotModel.Title.Replace(':', '_').Replace(' ', '_');
        //        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "plots");
        //        Directory.CreateDirectory(filePath);
        //        filePath = Path.Combine(filePath, Path.ChangeExtension(title, "png"));
        //        pngExporter.ExportToFile(CurrentPlot.PlotModel, filePath);
        //    }
        //}
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

        private void AddPlot(Plot plot)
        {
            Plots.Add(plot);
            PlotsCount = Plots.Count - 1;
            MoveToPlot(PlotsCount);
        }

        private Plot CreatePlot(Types.Signal signal, string title)
        {
            var points = signal.points;
            int step = (int)Math.Ceiling((double)points.Length / stepDivisor);

            var realValues = new ChartValues<double>();
            var imaginaryValues = new ChartValues<double>();
            List<string> labels = new List<string>();
            var orderedPoints = points.OrderBy(p => p.x).ToList();
            for (int i = 0; i < points.Length; i += step)
            {
                realValues.Add(orderedPoints[i].y.Real);
                imaginaryValues.Add(orderedPoints[i].y.Imaginary);
                labels.Add(orderedPoints[i].x.ToString("F4"));
            }

            var realPlot = new SeriesCollection
            {
                new LiveCharts.Wpf.LineSeries {Values = new ChartValues<double>(realValues), Title = title}
            };

            var imaginaryPlot = new SeriesCollection
            {
                new LiveCharts.Wpf.LineSeries {Values = new ChartValues<double>(imaginaryValues), Title = title}
            };


            return new Plot(realPlot, imaginaryPlot, signal, labels.ToArray());
        }

        private void AddNewPlot()
        {
            var signal = SignalGeneration.signalGenerator(new Types.SignalMetadata(SignalType, IsContinous, Amplitude, StartTime, Duration, DutyCycle, SignalFrequency, SamplingFrequency));

            AddPlot(CreatePlot(
                signal,
                $"{SignalType} {(signal.metadata.isContinous ? "Continous" : "Discrete")} A: {Amplitude:0.##} f_sig: {SignalFrequency:0.##} f_sam: {SamplingFrequency:0.##}"));

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
                AddPlot(CreatePlot(newSignal, title));
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

        private void SimpleFourier()
        {
            var signal = SignalGeneration.signalGenerator(new Types.SignalMetadata(
                    SignalType, IsContinous, 8, 0, 8, DutyCycle,
                    signalFrequency: 4, samplingFrequency: 16));
            //var signal = SignalGeneration.signalGenerator(new Types.SignalMetadata(
            //    SignalType, IsContinous, 2, Math.PI / 2.0, Duration, DutyCycle,
            //    Math.PI, 16));
            //var signal2 = SignalGeneration.signalGenerator(new Types.SignalMetadata(
            //    SignalType, IsContinous, 5, Math.PI / 2.0, Duration, DutyCycle,
            //    Math.PI / 2.0, 16));
            //signal = Operations.add(signal2, signal);
            AddPlot(CreatePlot(signal, "base"));

            var points = signal.points;

            //var xVector = CreateVector.DenseOfEnumerable(points.Select(p => p.y));

            //var Wn = new Complex(Math.E, 0.0).Power(
            //    new Complex(0.0, (2.0 * Math.PI) / (double)points.Length));

            //var f = CreateMatrix.Dense<Complex>(points.Length, points.Length);

            //for (int r = 0; r < points.Length; r++)
            //{
            //    for (int c = 0; c < points.Length; c++)
            //    {
            //        f[r, c] = Wn.Power(-1.0 * (r * c));
            //    }
            //}

            //var result = 1.0 / points.Length
            //             * f
            //             * xVector;
            //var newSignal = new Types.Signal(
            //    signal.metadata,
            //    ListModule.OfSeq(
            //        signal.points.Zip(
            //            result,
            //            (p, r) => new Types.Point(p.x, r)))
            //    );

            //AddPlot(CreatePlot(newSignal, "naive fourier inside"));

            //var newNaiveSignal = new Types.Signal(
            //    signal.metadata,
            //    ListModule.OfSeq(
            //        Fourier.FrequencyScale(points.Length, signal.metadata.samplingFrequency).Zip(
            //            ComputeFourier(xVector.ToArray()),
            //            (p, r) => new Types.Point(p, r)))
            //);
            //AddPlot(CreatePlot(newNaiveSignal, "Naive lib fourier"));


            var newQuickSignal = new Types.Signal(
                signal.metadata,
                ListModule.OfSeq(
                    signal.points.Zip(
                        Fft(points.Select(p => p.y).ToList()),
                        (p, r) => new Types.Point(p.x, r)))
            );
            AddPlot(CreatePlot(newQuickSignal, "our quick Fourier"));

            //var newQuickSignalFreq = new Types.Signal(
            //    signal.metadata,
            //    ListModule.OfSeq(
            //        Fourier.FrequencyScale(points.Length, signal.metadata.samplingFrequency).Zip(
            //            Fft(points.Select(p => p.y).ToList()),
            //            (p, r) => new Types.Point(p, r)))
            //);
            //AddPlot(CreatePlot(newQuickSignalFreq, "Our quick Fourier with freq scale"));

            var input = CreateVector.DenseOfEnumerable(points.Select(p => p.y)).ToArray();
            Fourier.Forward(input);
            var sth = new Types.Signal(
                signal.metadata,
                ListModule.OfSeq(
                    Fourier.FrequencyScale(points.Length, signal.metadata.samplingFrequency).Zip(
                        input,
                        (p, r) => new Types.Point(p, r)))
            );
            AddPlot(CreatePlot(sth, "Lib quick Fourier"));

            var input2 = CreateVector.DenseOfEnumerable(points.Select(p => p.y)).ToArray();
            var sth2 = new Types.Signal(
                signal.metadata,
                ListModule.OfSeq(
                    Fourier.FrequencyScale(points.Length, signal.metadata.samplingFrequency).Zip(
                        RecursiveFFT(input2),
                        (p, r) => new Types.Point(p, r)))
            );
            AddPlot(CreatePlot(sth2, "magic quick Fourier with freq scale"));
            var input3 = CreateVector.DenseOfEnumerable(points.Select(p => p.y)).ToArray();
            var sth3 = new Types.Signal(
                signal.metadata,
                ListModule.OfSeq(
                    signal.points.Zip(
                        RecursiveFFT(input3),
                        (p, r) => new Types.Point(p.x, r)))
            );
            AddPlot(CreatePlot(sth3, "magic quick Fourierout"));
        }

        public Complex[] ComputeFourier(Complex[] input)
        {
            int n = input.Length;
            Complex[] output = new Complex[n];
            for (int k = 0; k < n; k++)
            {  
                Complex sum = 0;
                for (int t = 0; t < n; t++)
                {  
                    double angle = 2 * Math.PI * t * k / n;
                    sum += input[t] * Complex.Exp(new Complex(0, -angle));
                }
                output[k] = sum;
            }
            return output;
        }


        public static Complex[] RecursiveFFT(Complex[] a)
        {
            int n = a.Length;
            int n2 = n / 2;

            if (n == 1)
                return a;

            Complex z = new Complex(0.0, 2.0 * Math.PI / n);
            Complex omegaN = Complex.Exp(z);
            Complex omega = new Complex(1.0, 0.0);
            Complex[] a0 = new Complex[n2];
            Complex[] a1 = new Complex[n2];
            Complex[] y0 = new Complex[n2];
            Complex[] y1 = new Complex[n2];
            Complex[] y = new Complex[n];

            for (int i = 0; i < n2; i++)
            {
                a0[i] = new Complex(0.0, 0.0);
                a0[i] = a[2 * i];
                a1[i] = new Complex(0.0, 0.0);
                a1[i] = a[2 * i + 1];
            }

            y0 = RecursiveFFT(a0);
            y1 = RecursiveFFT(a1);

            for (int k = 0; k < n2; k++)
            {
                y[k] = new Complex(0.0, 0.0);
                y[k] = y0[k] + (y1[k] * (omega));
                y[k + n2] = new Complex(0.0, 0.0);
                y[k + n2] = y0[k] - (y1[k] * (omega));
                omega = omega * omegaN;
            }

            return y;
        }

        public static int BitReverse(int n, int bits)
        {
            int reversedN = n;
            int count = bits - 1;

            n >>= 1;
            while (n > 0)
            {
                reversedN = (reversedN << 1) | (n & 1);
                count--;
                n >>= 1;
            }

            return ((reversedN << count) & ((1 << bits) - 1));
        }

        /* Uses Cooley-Tukey iterative in-place algorithm with radix-2 DIT case
         * assumes no of points provided are a power of 2 */
        public static Complex[] FFT(Complex[] buffer)
        {
            int size = 1024;
            var xEx = new Complex[size];
            for (int i = 0; i < buffer.Length; i++)
            {
                xEx[i] = buffer[i];
            }
            for (int i = buffer.Length; i < size; i++)
            {
                xEx[i] = new Complex(0.0, 0.0);
            }
            buffer = xEx;
            int bits = (int)Math.Log(buffer.Length, 2);
            for (int j = 1; j < buffer.Length / 2; j++)
            {

                int swapPos = BitReverse(j, bits);
                var temp = buffer[j];
                buffer[j] = buffer[swapPos];
                buffer[swapPos] = temp;
            }

            for (int N = 2; N <= buffer.Length; N <<= 1)
            {
                for (int i = 0; i < buffer.Length; i += N)
                {
                    for (int k = 0; k < N / 2; k++)
                    {

                        int evenIndex = i + k;
                        int oddIndex = i + k + (N / 2);
                        var even = buffer[evenIndex];
                        var odd = buffer[oddIndex];

                        double term = -2 * Math.PI * k / (double)N;
                        Complex exp = new Complex(Math.Cos(term), Math.Sin(term)) * odd;

                        buffer[evenIndex] = even + exp;
                        buffer[oddIndex] = even - exp;

                    }
                }
            }
            Debug.WriteLine("Results:");
            foreach (Complex c in buffer)
            {
                Console.WriteLine(c);
            }
            return buffer;
        }

        // ReSharper disable once InconsistentNaming
        private List<Complex> Fft(List<Complex> X)
        {
            int n = X.Count;
            int half = n / 2;
            if (n > 1)
            {
                var l = Fft(X.Where((x, i) => i % 2 == 0).ToList());
                l.AddRange(Fft(X.Where((x, i) => i % 2 != 0).ToList()));
                X = l;
                for (int k = 0; k < n / 2; k++)
                {
                    var wn = Iexp((-2.0 * k * Math.PI) / (double)n);
                    var xk = X[k];
                    X[k] = X[k] + X[k + half];
                    X[k + half] = (xk - X[k + half]) * wn;
                }
            }

            return X;

            Complex Iexp(double value)
            {
                return new Complex(Math.Cos(value), Math.Sin(value));
            }
            /*
             *
            def fft(X):
                N = len(X)
                half = N // 2
                if N > 1:
                    X = fft(X[::2]) + fft(X[1::2])
                    for k in range(N//2):
                        xk = X[k]
                        kernel = iexp(-2*math.pi*k/N)
                        c = kernel * X[k+half]
                        X[k] = xk + c
                        X[k+half] = xk - c
                return X
             */
        }
        #region extrapolation

        //private void GenerateFilter()
        //{
        //    Types.Signal filter = Filters.createFilter(FilterType,
        //        WindowFunction,
        //        FilterOrder,
        //        FilterCutOffFrequency,
        //        SamplingFrequency);
        //    AddPlot(
        //        CreatePlot(filter.points, $"Filter: {FilterType} {WindowFunction} M: {FilterOrder}"),
        //        filter);
        //}

        //private void Convolute()
        //{
        //    if (FirstChosenPlot > PlotsCount || SecondChosenPlot > PlotsCount)
        //    {
        //        MessageBox.Show($"Invalid plots indexes. Maximum plot index is: {PlotsCount}");
        //        return;
        //    }

        //    if (FirstChosenPlot == SecondChosenPlot)
        //    {
        //        MessageBox.Show($"Choose two different plots.");
        //        return;
        //    }

        //    var first = Plots[FirstChosenPlot].Signal;
        //    var second = Plots[SecondChosenPlot].Signal;

        //    var convoluted = Convolution.convolute(first,second);

        //    AddPlot(
        //        CreatePlot(convoluted.points, "convoluted plot"),
        //        convoluted);
        //}

        #endregion
    }
}