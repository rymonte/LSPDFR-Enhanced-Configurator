using System.Collections.ObjectModel;
using Avalonia.Media;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services.Validation;

// Use the unified ValidationSeverity from the validation service
using RankValidationSeverity = LSPDFREnhancedConfigurator.Services.Validation.ValidationSeverity;

namespace LSPDFREnhancedConfigurator.UI.ViewModels
{
    /// <summary>
    /// ViewModel for hierarchical vehicle TreeView items
    /// </summary>
    public class VehicleTreeItemViewModel : ViewModelBase
    {
        private bool _isChecked;
        private bool _isExpanded = true;
        private readonly Action? _checkedChangedCallback;
        private RankValidationSeverity _validationSeverity = RankValidationSeverity.None;

        public VehicleTreeItemViewModel(string displayText, Vehicle? vehicle = null, StationAssignment? station = null, VehicleTreeItemViewModel? parent = null, Action? checkedChangedCallback = null)
        {
            DisplayText = displayText;
            Vehicle = vehicle;
            Station = station;
            Parent = parent;
            _checkedChangedCallback = checkedChangedCallback;
            Children = new ObservableCollection<VehicleTreeItemViewModel>();
        }

        public string DisplayText { get; }
        public Vehicle? Vehicle { get; }
        public StationAssignment? Station { get; }
        public VehicleTreeItemViewModel? Parent { get; }
        public ObservableCollection<VehicleTreeItemViewModel> Children { get; }

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

        public bool IsVehicleNode => Vehicle != null;
        public bool IsStationNode => Station != null;
        public bool IsGlobalNode => !IsVehicleNode && !IsStationNode;

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
