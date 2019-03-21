using PlotsVisualizer.ViewModels;
using System.Windows;

namespace PlotsVisualizer
{
    /// <summary>
    /// Interaction logic for SignalParametersWindow.xaml
    /// </summary>
    public partial class SignalParametersWindow : Window
    {
        public SignalParametersWindow(SignalParametersViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
