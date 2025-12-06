using Avalonia.Controls;
using Avalonia.Interactivity;
using LSPDFREnhancedConfigurator.UI.ViewModels;

namespace LSPDFREnhancedConfigurator.UI.Views
{
    public partial class AddVehiclesDialog : Window
    {
        public AddVehiclesDialog()
        {
            InitializeComponent();
        }

        private void AddSelectedButton_Click(object? sender, RoutedEventArgs e)
        {
            // Execute the command to populate SelectedVehicles before closing
            if (DataContext is AddVehiclesDialogViewModel viewModel)
            {
                if (viewModel.AddSelectedCommand.CanExecute(null))
                {
                    viewModel.AddSelectedCommand.Execute(null);
                }
            }

            Close(true);
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
