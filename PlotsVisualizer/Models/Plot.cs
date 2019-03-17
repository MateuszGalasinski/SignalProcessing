using Microsoft.FSharp.Collections;
using OxyPlot;
using SignalProcessing;
using UILogic.Base;

namespace PlotsVisualizer.Models
{
    public class Plot : BindableBase
    {
        private PlotModel _plotModel;
        private FSharpList<Types.Point> _points;

        public PlotModel PlotModel
        {
            get => _plotModel;
            set => SetProperty(ref _plotModel, value);
        }

        public FSharpList<Types.Point> Points
        {
            get => _points;
            set => SetProperty(ref _points, value);
        }
    }
}
