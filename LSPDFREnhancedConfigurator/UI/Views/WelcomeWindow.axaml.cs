using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using LSPDFREnhancedConfigurator.UI.ViewModels;

namespace LSPDFREnhancedConfigurator.UI.Views
{
    public partial class WelcomeWindow : Window
    {
        public WelcomeWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void WebsiteLink_Click(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is WelcomeWindowViewModel vm)
            {
                vm.OpenWebsiteCommand.Execute(null);
            }
        }

        private void PluginLink_Click(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is WelcomeWindowViewModel vm)
            {
                vm.OpenPluginCommand.Execute(null);
            }
        }
    }
}
