using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using LSPDFREnhancedConfigurator.UI.ViewModels;

namespace LSPDFREnhancedConfigurator.UI.Views
{
    public partial class StationAssignmentsView : UserControl
    {
        public StationAssignmentsView()
        {
            InitializeComponent();
        }

        private void OnBackgroundPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // Clear focus when clicking outside interactive elements
            if (e.Source is not TextBox && e.Source is not NumericUpDown &&
                e.Source is not ComboBox && e.Source is not ListBoxItem && e.Source is not CheckBox)
            {
                this.Focus();
            }
        }

        private void OnAvailableStationsDoubleTapped(object? sender, TappedEventArgs e)
        {
            if (DataContext is StationAssignmentsViewModel viewModel)
            {
                viewModel.AddStationsCommand?.Execute(null);
            }
        }

        private void OnAssignedStationsDoubleTapped(object? sender, TappedEventArgs e)
        {
            if (DataContext is StationAssignmentsViewModel viewModel)
            {
                viewModel.RemoveStationsCommand?.Execute(null);
            }
        }
    }
}
