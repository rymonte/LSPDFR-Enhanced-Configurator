namespace LSPDFREnhancedConfigurator.UI.ViewModels
{
    public class LoadingWindowViewModel : ViewModelBase
    {
        private string _statusText = "Loading...";
        private string _detailText = "Initializing...";
        private double _progress = 0;
        private bool _isIndeterminate = true;

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public string DetailText
        {
            get => _detailText;
            set => SetProperty(ref _detailText, value);
        }

        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set => SetProperty(ref _isIndeterminate, value);
        }

        public void UpdateProgress(string status, string detail, double progress)
        {
            StatusText = status;
            DetailText = detail;
            Progress = progress;
            IsIndeterminate = false;
        }

        public void SetIndeterminate(string status, string detail)
        {
            StatusText = status;
            DetailText = detail;
            IsIndeterminate = true;
        }
    }
}
