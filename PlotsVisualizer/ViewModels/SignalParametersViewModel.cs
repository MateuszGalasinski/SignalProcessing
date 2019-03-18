using SignalProcessing;
using System;

namespace PlotsVisualizer.ViewModels
{
    public class SignalParametersViewModel
    {
        public SignalParametersViewModel(Types.Signal signal)
        {
            Signal = signal ?? throw new ArgumentNullException(nameof(signal));
        }

        public Types.Signal Signal { get; set; }
    }
}
