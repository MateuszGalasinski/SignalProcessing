using Microsoft.FSharp.Collections;
using OxyPlot;
using OxyPlot.Series;
using SignalProcessing;
using System.Collections.Generic;
using System.IO;
using System.Media;
using UILogic.Base;

namespace PlotsVisualizer.ViewModels
{
    class MainViewModel : BindableBase
    {
        private PlotModel _currentPlotModel;
        private int _currentPlotIndex = -1;
        private string _fileName = "currentPlot";
        private int _plotsCount = 0;

        public List<(PlotModel plot, FSharpList<Types.Point> points)> Plots { get; } = new List <(PlotModel plot, FSharpList<Types.Point> points) >();

        public int CurrentPlotIndex
        {
            get => _currentPlotIndex;
            private set => SetProperty(ref _currentPlotIndex, value);
        }

        public PlotModel CurrentPlotModel
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
            set => SetProperty(ref _fileName, value);
        }

        public IRaiseCanExecuteCommand NextPlotCommand { get; }
        public IRaiseCanExecuteCommand PreviousPlotCommand { get; }
        public IRaiseCanExecuteCommand SavePlotCommand { get; }
        public IRaiseCanExecuteCommand LoadPlotCommand { get; }

        public MainViewModel()
        {
            NextPlotCommand =  new RelayCommand(MoveToNextPlot);
            PreviousPlotCommand =  new RelayCommand(MoveToPreviousPlot);
            SavePlotCommand =  new RelayCommand(SaveToFile);
            LoadPlotCommand =  new RelayCommand(LoadFromFile);

            AddExamplePlot(1);
            MoveToNextPlot();
            AddExamplePlot(-2);
        }

        #region plot navigation
        private void MoveToNextPlot()
        {
            if (CurrentPlotIndex + 1 < Plots.Count)
            {
                CurrentPlotIndex++;
                CurrentPlotModel = Plots[CurrentPlotIndex].plot;
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
                CurrentPlotModel = Plots[CurrentPlotIndex].plot;
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
                CurrentPlotModel = Plots[CurrentPlotIndex].plot;
            }
        }
        #endregion

        #region file save/load
        private void SaveToFile()
        {
            using (var fileStream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), FileName), FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                var pointsToSave = Plots[CurrentPlotIndex].points;
                binaryFormatter.Serialize(fileStream, pointsToSave);
            }
            SystemSounds.Beep.Play();
        }

        private void LoadFromFile()
        {
            FSharpList<Types.Point> points;
            using (var fileStream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), FileName), FileMode.OpenOrCreate))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                points = (FSharpList<Types.Point>)binaryFormatter.Deserialize(fileStream);
            }

            AddPlot(CreatePlot(points, "Loaded from file"), points);
            MoveToPlot(Plots.Count - 1);
            SystemSounds.Beep.Play();
        }
        #endregion

        private void AddPlot(PlotModel plot, FSharpList<Types.Point> points)
        {
            Plots.Add((plot, points));
            PlotsCount = Plots.Count - 1;
        }

        private PlotModel CreatePlot(FSharpList<Types.Point> points, string title)
        {
            var plot = new PlotModel { Title = title };
            var series = new LineSeries { MarkerType = MarkerType.Circle, MarkerSize = 2};
            foreach (var point in points)
            {
                series.Points.Add(new DataPoint(point.x.r, point.y.r));
            }
            plot.Series.Add(series);

            return plot;
        }

        private void AddExamplePlot(double amplitude)
        {
            var signal = SignalProcessing.SignalProcessing.testGenerator(amplitude);

            AddPlot(CreatePlot(signal.points, "example"), signal.points);
        }
    }
}
