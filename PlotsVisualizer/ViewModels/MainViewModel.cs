using SignalProcessing;

namespace PlotsVisualizer.ViewModels
{
    class MainViewModel
    {
        public MainViewModel()
        {
            Signal = SignalProcessing.SignalProcessing.testGenerator;
        }

        public Types.Signal Signal { get; }
    }
}
