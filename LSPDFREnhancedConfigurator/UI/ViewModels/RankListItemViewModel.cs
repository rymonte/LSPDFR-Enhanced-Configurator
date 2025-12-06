using Avalonia.Media.Imaging;
using Avalonia.Platform;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.UI.ViewModels
{
    /// <summary>
    /// ViewModel for rank items in dropdowns/list views with validation state
    /// </summary>
    public class RankListItemViewModel : ViewModelBase
    {
        private RankValidationSeverity _validationSeverity = RankValidationSeverity.None;

        public RankListItemViewModel(RankHierarchy rank)
        {
            Rank = rank;
        }

        public RankHierarchy Rank { get; }

        public string DisplayName => Rank.Name;

        public RankValidationSeverity ValidationSeverity
        {
            get => _validationSeverity;
            set
            {
                if (SetProperty(ref _validationSeverity, value))
                {
                    OnPropertyChanged(nameof(HasValidationIssue));
                    OnPropertyChanged(nameof(ValidationIconPath));
                }
            }
        }

        public bool HasValidationIssue => ValidationSeverity != RankValidationSeverity.None;

        public Bitmap? ValidationIconPath
        {
            get
            {
                var path = ValidationSeverity switch
                {
                    RankValidationSeverity.Error => "avares://LSPDFREnhancedConfigurator/Resources/Icons/error-icon.png",
                    RankValidationSeverity.Warning => "avares://LSPDFREnhancedConfigurator/Resources/Icons/warning-icon.png",
                    RankValidationSeverity.Advisory => "avares://LSPDFREnhancedConfigurator/Resources/Icons/info-light.png",
                    _ => null
                };

                if (path == null) return null;

                try
                {
                    var uri = new Uri(path);
                    return new Bitmap(AssetLoader.Open(uri));
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Updates the validation state for this list item
        /// </summary>
        public void UpdateValidationState(RankValidationSeverity severity)
        {
            ValidationSeverity = severity;
        }
    }
}
