using Avalonia.Controls;
using Avalonia.Interactivity;
using LSPDFREnhancedConfigurator.UI.ViewModels;

namespace LSPDFREnhancedConfigurator.UI.Views
{
    public partial class ValidationWarningDialog : Window
    {
        public ValidationWarningDialog()
        {
            InitializeComponent();
        }

        private void ViewAndFixButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void ContinueAnywayButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
