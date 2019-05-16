using PlotsVisualizer.ViewModels;
using System.Windows;

namespace PlotsVisualizer.Views
{
    /// <summary>
    /// Interaction logic for RadarWindow.xaml
    /// </summary>
    public partial class RadarWindow : Window
    {
        public RadarWindow(RadarViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
