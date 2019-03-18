using OxyPlot;
using SignalProcessing;
using UILogic.Base;

namespace PlotsVisualizer.Models
{
    public class Plot : BindableBase
    {
        private PlotModel _plotModel;
        private Types.Signal _signal;

        public PlotModel PlotModel
        {
            get => _plotModel;
            set => SetProperty(ref _plotModel, value);
        }

        public Types.Signal Signal
        {
            get => _signal;
            set => SetProperty(ref _signal, value);
        }
    }
}
