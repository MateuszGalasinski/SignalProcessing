using SignalProcessing;

namespace PlotsVisualizer.ViewModels
{
    public class ErrorsViewModel
    {
        public double MeanSquaredError { get; }
        public double SignalToNoiseRatio { get; }
        public double PeakSignalToNoiseRatio { get; }
        public double MaximumDifference { get; }

        public ErrorsViewModel(Types.Signal original, Types.Signal processed)
        {
            MeanSquaredError = ErrorCalculations.meanSquaredError(original.points, processed.points);
            SignalToNoiseRatio = ErrorCalculations.signalToNoiseRatio(original.points, processed.points);
            PeakSignalToNoiseRatio = ErrorCalculations.peakSignalToNoiseRatio(original.points, processed.points);
            MaximumDifference = ErrorCalculations.maxDifference(original.points, processed.points);
        }
    }
}
