using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LSPDFREnhancedConfigurator.UI.Views
{
    public partial class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
