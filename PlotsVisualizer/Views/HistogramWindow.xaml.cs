using PlotsVisualizer.ViewModels;
using System.Windows;

namespace PlotsVisualizer.Views
{
    /// <summary>
    /// Interaction logic for HistogramWindow.xaml
    /// </summary>
    public partial class HistogramWindow : Window
    {
        public HistogramWindow(HistogramViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
