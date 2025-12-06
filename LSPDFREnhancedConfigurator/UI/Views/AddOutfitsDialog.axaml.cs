using Avalonia.Controls;
using Avalonia.Interactivity;
using LSPDFREnhancedConfigurator.UI.ViewModels;

namespace LSPDFREnhancedConfigurator.UI.Views
{
    public partial class AddOutfitsDialog : Window
    {
        public AddOutfitsDialog()
        {
            InitializeComponent();
        }

        private void AddSelectedButton_Click(object? sender, RoutedEventArgs e)
        {
            // Execute the command to populate SelectedOutfits before closing
            if (DataContext is AddOutfitsDialogViewModel viewModel)
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
