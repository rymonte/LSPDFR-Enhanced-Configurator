using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace LSPDFREnhancedConfigurator.UI.Views
{
    public partial class ProfileSelectionDialog : Window
    {
        public ProfileSelectionDialog()
        {
            InitializeComponent();
        }

        private void SelectButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private void ListBox_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (DataContext is ViewModels.ProfileSelectionDialogViewModel vm && !string.IsNullOrEmpty(vm.SelectedProfile))
            {
                Close(true);
            }
        }
    }
}
