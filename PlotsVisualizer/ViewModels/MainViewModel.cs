using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Numerics;
using System.Windows;
using LiveCharts;
using MathNet.Numerics.IntegralTransforms;
using Microsoft.FSharp.Collections;
using Microsoft.Win32;
using Newtonsoft.Json;
using SignalProcessing;
using UILogic.Base;
using Plot = PlotsVisualizer.Models.Plot;

namespace PlotsVisualizer.ViewModels
{
    class MainViewModel : BindableBase
    {
        private const int stepDivisor = 20000;

        private Plot _currentPlotModel;
        private int _currentPlotIndex = -1;
        private string _fileName = "My favorite plot";
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

        public Func<double, string> YFormatter { get; }
        = value => value.ToString("F4");
        #endregion

        #region Commands
        public IRaiseCanExecuteCommand NextPlotCommand { get; }
        public IRaiseCanExecuteCommand PreviousPlotCommand { get; }
        public IRaiseCanExecuteCommand SavePlotCommand { get; }
        public IRaiseCanExecuteCommand LoadPlotCommand { get; }

        public IRaiseCanExecuteCommand AddPlotCommand { get; }
        public IRaiseCanExecuteCommand DeleteCommand { get; }

        public IRaiseCanExecuteCommand AddCommand { get; }
        public IRaiseCanExecuteCommand SubstractCommand { get; }
        public IRaiseCanExecuteCommand MultiplyCommand { get; }
        public IRaiseCanExecuteCommand DivideCommand { get; }
        public IRaiseCanExecuteCommand SwitchWCommand { get; }
        public IRaiseCanExecuteCommand DFTCommand { get; }
        public IRaiseCanExecuteCommand FFTCommand { get; }
        public IRaiseCanExecuteCommand WaveletCommand { get; }
        #endregion

        public MainViewModel()
        {
            NextPlotCommand = new RelayCommand(MoveToNextPlot);
            PreviousPlotCommand = new RelayCommand(MoveToPreviousPlot);
            SavePlotCommand = new RelayCommand(SaveToFile);
            LoadPlotCommand = new RelayCommand(LoadFromFile);
            AddPlotCommand = new RelayCommand(AddNewPlot);
            DeleteCommand = new RelayCommand(() => RemovePlot(CurrentPlotIndex));
            AddCommand = new RelayCommand(() => PlotOperate(Operations.OperationType.Addition));
            SubstractCommand = new RelayCommand(() => PlotOperate(Operations.OperationType.Substraction));
            MultiplyCommand = new RelayCommand(() => PlotOperate(Operations.OperationType.Multiplication));
            DivideCommand = new RelayCommand(() => PlotOperate(Operations.OperationType.Division));
            SwitchWCommand = new RelayCommand(SwitchW);
            DFTCommand = new RelayCommand(SimpleFourier);
            FFTCommand = new RelayCommand(FFT);
            WaveletCommand = new RelayCommand(WaveletTransformation);

            AddNewPlot();
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

        private void SaveToFile()
        {
            SaveFileDialog fileDialog = new SaveFileDialog()
            {
                CreatePrompt = true
            };

            FileName = fileDialog.ShowDialog() == true ? fileDialog.FileName : string.Empty;
            if (!string.IsNullOrWhiteSpace(FileName))
                return;

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
                OpenFileDialog fileDialog = new OpenFileDialog();

                FileName = fileDialog.ShowDialog() == true ? fileDialog.FileName : string.Empty;
                if (!File.Exists(FileName))
                    return;

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
            var magnitudeValues = new ChartValues<double>();
            var phaseValues = new ChartValues<double>();
            List<string> labels = new List<string>();
            string[] frequencyLabels = Fourier.FrequencyScale(points.Length, signal.metadata.samplingFrequency)
                .Select(v => v.ToString()).ToArray();
            var orderedPoints = points.OrderBy(p => p.x).ToList();
            for (int i = 0; i < points.Length; i += step)
            {
                realValues.Add(orderedPoints[i].y.Real);
                imaginaryValues.Add(orderedPoints[i].y.Imaginary);
                magnitudeValues.Add(orderedPoints[i].y.Magnitude);
                phaseValues.Add(orderedPoints[i].y.Phase);
                labels.Add(orderedPoints[i].x.ToString("F4"));
            }

            var realPlot = new SeriesCollection
            {
                new LiveCharts.Wpf.LineSeries {Values = new ChartValues<double>(realValues), Title = $"{title} real"}
            };
            var imaginaryPlot = new SeriesCollection
            {
                new LiveCharts.Wpf.LineSeries {Values = new ChartValues<double>(imaginaryValues), Title = $"{title} imaginary"}
            };
            var magnitudePlot = new SeriesCollection
            {
                new LiveCharts.Wpf.LineSeries {Values = new ChartValues<double>(magnitudeValues), Title = $"{title} mag"}
            };
            var phasePlot = new SeriesCollection
            {
                new LiveCharts.Wpf.LineSeries {Values = new ChartValues<double>(phaseValues), Title = $"{title} phase"}
            };


            return new Plot(signal, labels.ToArray(), frequencyLabels, (realPlot, imaginaryPlot), (magnitudePlot, phasePlot));
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
            var signal = CurrentPlot.Signal;
            var points = signal.points;

            var newQuickSignal = new Types.Signal(
                signal.metadata,
                ListModule.OfSeq(
                    GenerateFrequencyScale(points.Length, signal.metadata.samplingFrequency).Zip(
                        ComputeFourier(points.Select(p => p.y).ToArray()),
                        (p, r) => new Types.Point(p, r)))
            );
            AddPlot(CreatePlot(newQuickSignal, "DFT"));
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

        private void FFT()
        {
            var signal = CurrentPlot.Signal;
            var points = signal.points;

            var newQuickSignal = new Types.Signal(
                signal.metadata,
                ListModule.OfSeq(
                    GenerateFrequencyScale(points.Length, signal.metadata.samplingFrequency).Zip(
                        RecursiveFFT(points.Select(p => p.y).ToArray()),
                        (p, r) => new Types.Point(p, r)))
            );
            AddPlot(CreatePlot(newQuickSignal, "FFT"));
        }

        public static Complex[] RecursiveFFT(Complex[] a)
        {
            int n = a.Length;
            int n2 = n / 2;

            if (n == 1)
                return a;

            Complex[] a0 = new Complex[n2];
            Complex[] a1 = new Complex[n2];
            Complex[] y0 = new Complex[n2];
            Complex[] y1 = new Complex[n2];
            Complex[] y = new Complex[n];

            //divide into even and odd
            for (int i = 0; i < n2; i++)
            {
                a0[i] = new Complex(0.0, 0.0);
                a0[i] = a[2 * i];
                a1[i] = new Complex(0.0, 0.0);
                a1[i] = a[2 * i + 1];
            }

            //calc halves
            y0 = RecursiveFFT(a0);
            y1 = RecursiveFFT(a1);

            Complex z = new Complex(0.0, 2.0 * Math.PI / n);
            Complex omegaN = Complex.Exp(z);
            Complex omega = new Complex(1.0, 0.0);
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

        private void WaveletTransformation()
        {
            var result = Db8(Extreme.Mathematics.Vector.Create(CurrentPlot.Signal.points.Select(p => p.y.Real).ToArray()));
            AddPlot(
                CreatePlot(new Types.Signal(
                    CurrentPlot.Signal.metadata,
                    ListModule.OfSeq(
                        result.Item1.Zip(
                            Enumerable.Range(0, result.Item1.Count),
                            (p, i) => new Types.Point(i, new Complex(p, 0.0))))),
                    "Wavelet transform 1"
                    ));

            AddPlot(
                CreatePlot(new Types.Signal(
                        CurrentPlot.Signal.metadata,
                        ListModule.OfSeq(
                            result.Item2.Zip(
                                Enumerable.Range(0, result.Item2.Count),
                                (p, i) => new Types.Point(i, new Complex(p, 0.0))))),
                    "Wavelet transform 2"
                ));
        }

        private static readonly Extreme.Mathematics.Vector<double> HDB8 = Extreme.Mathematics.Vector.Create(new[]
        {
            0.32580343, 1.01094572,
            0.8922014, -0.03957503,
            -0.26450717, 0.0436163,
            0.0465036, -0.01498699
        });

        private static readonly Extreme.Mathematics.Vector<double> G8 = Extreme.Mathematics.Vector.Create(new[]
        {
            HDB8[7],
            -HDB8[6],
            HDB8[5],
            -HDB8[4],
            HDB8[3],
            -HDB8[2],
            HDB8[1],
            -HDB8[0],
        });

        private (List<double>, List<double>) Db8(Extreme.Mathematics.Vector<double> xs)
        {
            var H = Extreme.Mathematics.Vector.Convolution<double>(xs, HDB8);
            var G = Extreme.Mathematics.Vector.Convolution<double>(xs, G8);

            var x1 = H.Where((p, i) => i % 2 == 0).ToList();
            var x2 = G.Where((p, i) => i % 2 != 0).ToList();
            return (x1, x2);
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
        }

        private void SwitchW()
        {
            CurrentPlot?.SwitchW();
            CurrentPlot = CurrentPlot;
        }

        private double[] GenerateFrequencyScale(int pointsLength, double samplingFrequency)
        {
            var f0 = samplingFrequency / pointsLength;
            var labels = new double[pointsLength];
            for (int i = 0; i < pointsLength; i++)
            {
                labels[i] = i * f0;
            }

            return labels;
        }
    }
}