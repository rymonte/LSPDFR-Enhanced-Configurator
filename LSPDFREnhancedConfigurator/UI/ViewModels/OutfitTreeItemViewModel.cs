using System.Collections.ObjectModel;
using Avalonia.Media;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services.Validation;

// Use the unified ValidationSeverity from the validation service
using RankValidationSeverity = LSPDFREnhancedConfigurator.Services.Validation.ValidationSeverity;

namespace LSPDFREnhancedConfigurator.UI.ViewModels
{
    /// <summary>
    /// ViewModel for hierarchical outfit TreeView items
    /// </summary>
    public class OutfitTreeItemViewModel : ViewModelBase
    {
        private bool _isChecked;
        private bool _isExpanded = true;
        private readonly Action? _checkedChangedCallback;
        private RankValidationSeverity _validationSeverity = RankValidationSeverity.None;

        public OutfitTreeItemViewModel(string displayText, string? outfitName = null, StationAssignment? station = null, OutfitTreeItemViewModel? parent = null, Action? checkedChangedCallback = null)
        {
            DisplayText = displayText;
            OutfitName = outfitName;
            Station = station;
            Parent = parent;
            _checkedChangedCallback = checkedChangedCallback;
            Children = new ObservableCollection<OutfitTreeItemViewModel>();
        }

        public string DisplayText { get; }
        public string? OutfitName { get; }
        public StationAssignment? Station { get; }
        public OutfitTreeItemViewModel? Parent { get; }
        public ObservableCollection<OutfitTreeItemViewModel> Children { get; }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (SetProperty(ref _isChecked, value))
                {
                    _checkedChangedCallback?.Invoke();
                }
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public bool IsOutfitNode => !string.IsNullOrEmpty(OutfitName);
        public bool IsStationNode => Station != null;
        public bool IsGlobalNode => !IsOutfitNode && !IsStationNode;

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

        /// <summary>
        /// Updates the validation state for this tree item
        /// </summary>
        public void UpdateValidationState(RankValidationSeverity severity)
        {
            ValidationSeverity = severity;
        }
    }
}
