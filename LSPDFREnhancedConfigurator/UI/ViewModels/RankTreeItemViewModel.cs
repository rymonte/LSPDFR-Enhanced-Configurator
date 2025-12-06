using System.Collections.ObjectModel;
using Avalonia.Media;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.UI.ViewModels
{
    /// <summary>
    /// Validation severity levels for tree item display
    /// </summary>
    public enum RankValidationSeverity
    {
        None,
        Advisory,
        Warning,
        Error
    }

    /// <summary>
    /// ViewModel for TreeView items representing ranks and pay bands
    /// </summary>
    public class RankTreeItemViewModel : ViewModelBase
    {
        private bool _isExpanded = false;
        private RankValidationSeverity _validationSeverity = RankValidationSeverity.None;
        private string _validationTooltip = string.Empty;

        public RankTreeItemViewModel(RankHierarchy rank)
        {
            Rank = rank;
            Children = new ObservableCollection<RankTreeItemViewModel>();

            // Add pay bands as children
            foreach (var payBand in rank.PayBands)
            {
                Children.Add(new RankTreeItemViewModel(payBand));
            }
        }

        public RankHierarchy Rank { get; }

        public string DisplayText => Rank.GetSummary();

        public ObservableCollection<RankTreeItemViewModel> Children { get; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public bool HasChildren => Children.Count > 0;

        public RankValidationSeverity ValidationSeverity
        {
            get => _validationSeverity;
            set
            {
                if (SetProperty(ref _validationSeverity, value))
                {
                    OnPropertyChanged(nameof(HasValidationIssue));
                    OnPropertyChanged(nameof(ValidationBackgroundBrush));
                }
            }
        }

        public string ValidationTooltip
        {
            get => _validationTooltip;
            set => SetProperty(ref _validationTooltip, value);
        }

        public bool HasValidationIssue => ValidationSeverity != RankValidationSeverity.None;

        public IBrush ValidationBackgroundBrush
        {
            get
            {
                return ValidationSeverity switch
                {
                    RankValidationSeverity.Error => new SolidColorBrush(Color.FromArgb(255, 211, 47, 47)), // Solid red - matches Remove All button
                    RankValidationSeverity.Warning => new SolidColorBrush(Color.FromArgb(255, 255, 159, 64)), // Solid orange
                    RankValidationSeverity.Advisory => Brushes.Transparent, // No background for advisories
                    _ => Brushes.Transparent
                };
            }
        }

        public void UpdateDisplayName()
        {
            OnPropertyChanged(nameof(DisplayText));
        }

        /// <summary>
        /// Updates the validation state for this tree item
        /// </summary>
        public void UpdateValidationState(RankValidationSeverity severity, string tooltip = "")
        {
            ValidationSeverity = severity;
            ValidationTooltip = tooltip;
        }
    }
}
