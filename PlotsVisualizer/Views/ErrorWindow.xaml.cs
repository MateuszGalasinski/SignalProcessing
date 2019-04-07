using PlotsVisualizer.ViewModels;
using System.Windows;

namespace PlotsVisualizer.Views
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : Window
    {
        public ErrorWindow(ErrorsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
