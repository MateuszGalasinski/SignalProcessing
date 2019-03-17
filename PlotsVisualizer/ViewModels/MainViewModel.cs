using Microsoft.FSharp.Collections;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Series;
using PlotsVisualizer.Models;
using SignalProcessing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows;
using UILogic.Base;

namespace PlotsVisualizer.ViewModels
{
    class MainViewModel : BindableBase
    {
        private Plot _currentPlotModel;
        private int _currentPlotIndex = -1;
        private string _fileName = "currentPlot";
        private int _plotsCount = 0;
        private double _amplitude = 1;
        private double _startTime = 0;
        private double _duration = 5;
        private double _dutyCycle = 0.5;
        private double _signalFrequency = 1;
        private double _samplingFrequency = 100;
        private Types.SignalType _signalType;

        public List<Plot> Plots { get; } = new List <Plot>();

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
        #endregion

        public IRaiseCanExecuteCommand NextPlotCommand { get; }
        public IRaiseCanExecuteCommand PreviousPlotCommand { get; }

        public IRaiseCanExecuteCommand ReadFilePathCommand { get; }
        public IRaiseCanExecuteCommand SavePlotCommand { get; }
        public IRaiseCanExecuteCommand LoadPlotCommand { get; }

        public IRaiseCanExecuteCommand AddCommand { get; }
        public IRaiseCanExecuteCommand DeleteCommand { get; }

        public MainViewModel()
        {
            NextPlotCommand =  new RelayCommand(MoveToNextPlot);
            PreviousPlotCommand =  new RelayCommand(MoveToPreviousPlot);
            ReadFilePathCommand =  new RelayCommand(ReadFilePath);
            SavePlotCommand =  new RelayCommand(SaveToFile, () => !string.IsNullOrWhiteSpace(FileName));
            LoadPlotCommand =  new RelayCommand(LoadFromFile, () => File.Exists(FileName));
            AddCommand =  new RelayCommand(AddNewPlot);
            DeleteCommand =  new RelayCommand(DeleteCurrent);

            SignalType = Types.SignalType.Sin;
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
            using (var fileStream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), FileName), FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                var pointsToSave = Plots[CurrentPlotIndex].Points;
                binaryFormatter.Serialize(fileStream, pointsToSave);
            }
            SystemSounds.Beep.Play();
        }

        private void LoadFromFile()
        {
            try
            {
                FSharpList<Types.Point> points;
                using (var fileStream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), FileName), FileMode.OpenOrCreate))
                {
                    var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    points = (FSharpList<Types.Point>)binaryFormatter.Deserialize(fileStream);
                }

                AddPlot(CreatePlot(points, "Loaded from file"), points);
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

        private void DeleteCurrent()
        {
            if (CurrentPlotIndex >= 0)
            {
                Plots.Remove(Plots[CurrentPlotIndex]);
                CurrentPlotIndex--;
                PlotsCount = Plots.Count - 1;
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

        private void AddPlot(PlotModel plot, FSharpList<Types.Point> points)
        {
            Plots.Add(new Plot()
            {
                PlotModel = plot,
                Points = points
            });
            PlotsCount = Plots.Count - 1;
        }

        private PlotModel CreatePlot(FSharpList<Types.Point> points, string title)
        {
            var plot = new PlotModel { Title = title };
            var series = new LineSeries { LineStyle = LineStyle.None, MarkerType = MarkerType.Circle, MarkerSize = 2};
            foreach (var point in points)
            {
                series.Points.Add(new DataPoint(point.x.r, point.y.r));
            }
            plot.Series.Add(series);

            return plot;
        }

        private void AddNewPlot()
        {
            var signal = SignalProcessing.SignalProcessing.signalGenerator(
                new Types.SignalMetadata(Amplitude, StartTime, Duration, DutyCycle, SignalFrequency, SamplingFrequency),
                SignalType);

            AddPlot(
                CreatePlot(signal.points, $"{SignalType} A: {Amplitude:0.##} f_sig: {SignalFrequency:0.##} f_sam: {SamplingFrequency:0.##}"),
                signal.points);

            SystemSounds.Beep.Play();
        }

    }
}
