using ClosedXML.Excel;
using PlotsVisualizer.Models;
using SignalProcessing;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using UILogic.Base;

namespace PlotsVisualizer.ViewModels
{
    public class ErrorsViewModel
    {
        private static List<ErrorResults> _results = new List<ErrorResults>();

        public string MeanSquaredError => _meanSquaredError.ToString(DoubleFormatString);

        public string SignalToNoiseRatio => _signalToNoiseRatio.ToString(DoubleFormatString);

        public string PeakSignalToNoiseRatio => _peakSignalToNoiseRatio.ToString(DoubleFormatString);

        public string MaximumDifference => _maximumDifference.ToString(DoubleFormatString);

        public IRaiseCanExecuteCommand SaveResultsCommand { get; }
        public IRaiseCanExecuteCommand RememberResultsCommand { get; }

        private const string DoubleFormatString = "F6";
        private Types.SignalType _signalType;
        private readonly double _samplingFrequency;
        private readonly int _neighboursCount;
        private readonly double _meanSquaredError;
        private readonly double _signalToNoiseRatio;
        private readonly double _peakSignalToNoiseRatio;
        private readonly double _maximumDifference;


        public ErrorsViewModel(Types.Signal original, Types.Signal processed, int neighboursCount, double samplingFrequency)
        {
            _neighboursCount = neighboursCount;
            _samplingFrequency = samplingFrequency;
            _meanSquaredError = ErrorCalculations.meanSquaredError(original.points, processed.points);
            _signalToNoiseRatio = ErrorCalculations.signalToNoiseRatio(original.points, processed.points);
            _peakSignalToNoiseRatio = ErrorCalculations.peakSignalToNoiseRatio(original.points, processed.points);
            _maximumDifference = ErrorCalculations.maxDifference(original.points, processed.points);

            SaveResultsCommand = new RelayCommand(SaveResults);
            RememberResultsCommand = new RelayCommand(RememberResults);
        }

        private void SaveResults()
        {
            if (!_results.Any())
            {
                MessageBox.Show("No results to save!");
                return;
            }

            string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "results");
            Directory.CreateDirectory(dirPath);

            DataTable table = new DataTable();
            table.TableName = _signalType.ToString();
            table.Columns.Add("Neighbours count");
            table.Columns.Add("Sampling frequency");
            table.Columns.Add("MSE");
            table.Columns.Add("SNR");
            table.Columns.Add("PSNR");
            table.Columns.Add("MD");

            foreach (var result in _results)
            {
                table.Rows.Add(
                    result.NeighoursCount,
                    result.SamplingFrequency,
                    result.MSE,
                    result.SNR,
                    result.PSNR,
                    result.MD);

            }

            XLWorkbook workbook = new XLWorkbook();
            var wb1 = workbook.Worksheets.Add(table, "Results").SetTabColor(XLColor.Amber);
            wb1.ColumnWidth = 10;

            string filePath = Path.Combine(dirPath,
                $"{_signalType}_{_samplingFrequency}_{_neighboursCount}.xlsx");
            workbook.SaveAs(filePath);
        }

        private void RememberResults()
        {
            _results.Add(new ErrorResults()
            {
                NeighoursCount = _neighboursCount,
                SamplingFrequency = _samplingFrequency,
                MD = MaximumDifference,
                MSE = MeanSquaredError,
                PSNR = PeakSignalToNoiseRatio,
                SNR = SignalToNoiseRatio
            });
        }
    }
}
