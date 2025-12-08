using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.Services.Validation;
using LSPDFREnhancedConfigurator.Services.Validation.Models;
using LSPDFREnhancedConfigurator.UI.Views;

// Use the unified ValidationSeverity from the validation service
using RankValidationSeverity = LSPDFREnhancedConfigurator.Services.Validation.ValidationSeverity;

namespace LSPDFREnhancedConfigurator.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly DataLoadingService _dataService;
        private readonly SelectionStateService _selectionStateService;
        private readonly ValidationDismissalService _dismissalService;
        private readonly string _gtaRootPath;
        private readonly string _currentProfile;
        private readonly SettingsManager _settingsManager;
        private System.Action? _restoreBackupAction;
        private System.Action? _retryLoadAction;
        private DispatcherTimer? _statusResetTimer;

        private string _statusMessage = "Ready";
        private bool _isLoading;
        private int _currentTabIndex;
        private int _lastModifiedTabIndex = 0; // Track which tab was most recently modified for Undo/Redo
        private bool _isXmlPreviewVisible = true;
        private int _loadedAgenciesCount;
        private int _loadedVehiclesCount;
        private int _loadedStationsCount;
        private int _loadedOutfitsCount;
        private string? _selectedProfile;
        private bool _isValidationPanelVisible = false;
        private string _validationErrorsText = string.Empty;
        private int _validationErrorCount = 0;
        private string _xmlPreviewText = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<!-- XML Preview will be shown here -->";
        private bool _isInitializing = true;
        private string _loadingStatusText = "Initializing...";
        private string _loadingDetailText = "Starting up...";
        private int _loadingProgress = 0;
        private bool _isLoadingIndeterminate = false;
        private bool _isXmlPreviewDarkMode = true;
        private bool _hasLoadingError = false;
        private bool _hasBackupsAvailable = false;
        private string _loadingStatusForeground = "#00D9FF"; // Cyan
        private string _loadingProgressForeground = "#00D9FF"; // Cyan
        private string _loadingProgressBorderBrush = "#00D9FF"; // Cyan
        private bool _isDebugLoggingEnabled = false;
        private bool _showDismissedValidations = false;

        public MainWindowViewModel(
            DataLoadingService dataService,
            List<Models.RankHierarchy>? loadedRanks,
            string gtaRootPath,
            string currentProfile,
            SettingsManager settingsManager)
        {
            _dataService = dataService;
            _selectionStateService = new SelectionStateService();
            _dismissalService = new ValidationDismissalService(settingsManager);
            _gtaRootPath = gtaRootPath;
            _currentProfile = currentProfile;
            _settingsManager = settingsManager;

            GenerateCommand = new RelayCommand(OnGenerate, CanGenerate);
            ExitCommand = new RelayCommand(OnExit);
            ShowValidationErrorsCommand = new RelayCommand(OnShowValidationErrors);
            RestoreBackupCommand = new RelayCommand(OnRestoreBackup);
            ToggleXmlPreviewCommand = new RelayCommand(OnToggleXmlPreview);
            ToggleXmlPreviewThemeCommand = new RelayCommand(OnToggleXmlPreviewTheme);
            CloseApplicationCommand = new RelayCommand(OnCloseApplication);
            OpenLogFileCommand = new RelayCommand(OnOpenLogFile);
            RestoreFromBackupCommand = new RelayCommand(OnRestoreFromBackup);
            OpenRanksXmlCommand = new RelayCommand(OnOpenRanksXml);
            RetryLoadCommand = new RelayCommand(OnRetryLoad);
            UndoCommand = new RelayCommand(OnUndo, CanUndo);
            RedoCommand = new RelayCommand(OnRedo, CanRedo);
            DismissAllWarningsCommand = new RelayCommand(OnDismissAllWarnings, CanDismissAllWarnings);
            DismissAllAdvisoriesCommand = new RelayCommand(OnDismissAllAdvisories, CanDismissAllAdvisories);
            RefreshValidationCommand = new RelayCommand(OnRefreshValidation);
            ToggleShowDismissedCommand = new RelayCommand(OnToggleShowDismissed);

            // Load actual profiles from disk
            var profiles = Parsers.RanksParser.GetAvailableProfiles(gtaRootPath);
            AvailableProfiles = new ObservableCollection<string>(profiles);
            SelectedProfile = currentProfile;

            // Initialize empty tab ViewModels
            RanksViewModel = new RanksViewModel(loadedRanks, dataService);
            StationAssignmentsViewModel = new StationAssignmentsViewModel(dataService);
            VehiclesViewModel = new VehiclesViewModel(dataService, loadedRanks);
            OutfitsViewModel = new OutfitsViewModel(dataService, loadedRanks);
            SettingsViewModel = new SettingsViewModel(settingsManager);

            // Subscribe to RanksViewModel events (will be re-subscribed in Initialize if data loads)
            RanksViewModel.RequestFocusRanksTab += OnRequestFocusRanksTab;
            RanksViewModel.StatusMessageChanged += OnRanksViewModelStatusMessageChanged;

            // Subscribe to undo/redo state changes from all ViewModels
            RanksViewModel.UndoRedoStateChanged += OnViewModelUndoRedoStateChanged;
            StationAssignmentsViewModel.UndoRedoStateChanged += OnViewModelUndoRedoStateChanged;
            VehiclesViewModel.UndoRedoStateChanged += OnViewModelUndoRedoStateChanged;
            OutfitsViewModel.UndoRedoStateChanged += OnViewModelUndoRedoStateChanged;

            // Subscribe to validation severity changes
            SettingsViewModel.ValidationSeverityChanged += OnValidationSeverityChanged;

            // Subscribe to debug logging changes to update status bar
            SettingsViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SettingsViewModel.EnableDebugLogging))
                {
                    IsDebugLoggingEnabled = SettingsViewModel.EnableDebugLogging;
                }
            };

            // Initialize debug logging status from settings
            IsDebugLoggingEnabled = _settingsManager.GetLogVerbosity() == LogLevel.Debug ||
                                   _settingsManager.GetLogVerbosity() == LogLevel.Trace;

            // If data was provided, initialize immediately (for error cases)
            if (loadedRanks != null)
            {
                Initialize(dataService, loadedRanks);
            }

            // Initialization complete - allow profile changes
            _isInitializing = false;
        }

        private void OnValidationSeverityChanged(object? sender, EventArgs e)
        {
            // Re-run validation to update the filtered list
            if (IsValidationPanelVisible)
            {
                RunValidation();
            }
        }

        public ObservableCollection<string> AvailableProfiles { get; }

        // Tab ViewModels
        private RanksViewModel _ranksViewModel;
        private StationAssignmentsViewModel _stationAssignmentsViewModel;
        private VehiclesViewModel _vehiclesViewModel;
        private OutfitsViewModel _outfitsViewModel;

        public RanksViewModel RanksViewModel
        {
            get => _ranksViewModel;
            private set => SetProperty(ref _ranksViewModel, value);
        }

        public StationAssignmentsViewModel StationAssignmentsViewModel
        {
            get => _stationAssignmentsViewModel;
            private set => SetProperty(ref _stationAssignmentsViewModel, value);
        }

        public VehiclesViewModel VehiclesViewModel
        {
            get => _vehiclesViewModel;
            private set => SetProperty(ref _vehiclesViewModel, value);
        }

        public OutfitsViewModel OutfitsViewModel
        {
            get => _outfitsViewModel;
            private set => SetProperty(ref _outfitsViewModel, value);
        }

        public SettingsViewModel SettingsViewModel { get; }

        public string? SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                if (SetProperty(ref _selectedProfile, value))
                {
                    OnProfileChanged();
                }
            }
        }

        public string XmlPreviewButtonText => IsXmlPreviewVisible ? "Hide XML Preview" : "Show XML Preview";

        public string XmlPreviewIconPath => IsXmlPreviewVisible
            ? "avares://LSPDFREnhancedConfigurator/Resources/Icons/hide-icon.png"
            : "avares://LSPDFREnhancedConfigurator/Resources/Icons/show-icon.png";

        public bool IsXmlPreviewDarkMode
        {
            get => _isXmlPreviewDarkMode;
            set
            {
                if (SetProperty(ref _isXmlPreviewDarkMode, value))
                {
                    OnPropertyChanged(nameof(XmlPreviewBackground));
                    OnPropertyChanged(nameof(XmlPreviewForeground));
                    OnPropertyChanged(nameof(XmlPreviewThemeButtonText));
                }
            }
        }

        public string XmlPreviewBackground => IsXmlPreviewDarkMode ? "#001928" : "#FFFFFF";
        public string XmlPreviewForeground => IsXmlPreviewDarkMode ? "#E9EAEA" : "#1E1E1E";
        public string XmlPreviewThemeButtonText => IsXmlPreviewDarkMode ? "Light Mode" : "Dark Mode";

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    ((RelayCommand)GenerateCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public int CurrentTabIndex
        {
            get => _currentTabIndex;
            set
            {
                if (SetProperty(ref _currentTabIndex, value))
                {
                    var tabNames = new[] { "Ranks", "Station Assignments", "Vehicles", "Outfits", "Settings" };
                    var tabName = value >= 0 && value < tabNames.Length ? tabNames[value] : "Unknown";
                    Logger.Trace($"[USER] Navigated to tab: {tabName} (index {value})");
                }
            }
        }

        public bool IsXmlPreviewVisible
        {
            get => _isXmlPreviewVisible;
            set
            {
                if (SetProperty(ref _isXmlPreviewVisible, value))
                {
                    OnPropertyChanged(nameof(XmlPreviewButtonText));
                    OnPropertyChanged(nameof(XmlPreviewIconPath));
                }
            }
        }

        public string XmlPreviewText
        {
            get => _xmlPreviewText;
            set
            {
                if (SetProperty(ref _xmlPreviewText, value))
                {
                    Logger.Info($"XmlPreviewText updated. Length: {value?.Length ?? 0} chars");
                }
            }
        }

        public bool IsValidationPanelVisible
        {
            get => _isValidationPanelVisible;
            set => SetProperty(ref _isValidationPanelVisible, value);
        }

        public string ValidationErrorsText
        {
            get => _validationErrorsText;
            set => SetProperty(ref _validationErrorsText, value);
        }

        public ObservableCollection<ValidationErrorItem> ValidationErrorItems { get; } = new ObservableCollection<ValidationErrorItem>();

        public int ValidationErrorCount
        {
            get => _validationErrorCount;
            set
            {
                if (SetProperty(ref _validationErrorCount, value))
                {
                    OnPropertyChanged(nameof(ValidationButtonText));
                    OnPropertyChanged(nameof(HasValidationErrors));
                    OnPropertyChanged(nameof(HasValidationErrorsOnly));
                    OnPropertyChanged(nameof(HasAnyValidationIssues));
                    OnPropertyChanged(nameof(ValidationButtonIconPath));
                    OnPropertyChanged(nameof(GenerateButtonIconPath));
                }
            }
        }

        public string ValidationButtonText => ValidationErrorCount > 0
            ? $"Show Validation Issues ({ValidationErrorCount})"
            : "No validation issues";

        public bool HasValidationErrors => ValidationErrorCount > 0;

        // Check if there are any errors (Type = "Error")
        public bool HasValidationErrorsOnly => ValidationErrorItems.Any(e => e.Type == "Error");

        // Check if there are any warnings (Type = "Warning")
        public bool HasWarnings => ValidationErrorItems.Any(e => e.Type == "Warning");

        // Check if there are any advisories (Type = "Advisory")
        public bool HasAdvisories => ValidationErrorItems.Any(e => e.Type == "Advisory");

        /// <summary>
        /// Comprehensive check for ANY validation issues including tree item validations
        /// </summary>
        public bool HasAnyValidationIssues
        {
            get
            {
                // Check startup validation
                if (ValidationErrorCount > 0)
                    return true;

                // Check rank tree items
                if (_ranksViewModel?.RankTreeItems != null)
                {
                    foreach (var treeItem in _ranksViewModel.RankTreeItems)
                    {
                        if (HasTreeItemValidationIssue(treeItem))
                            return true;
                    }
                }

                // Check vehicle tree items
                if (_vehiclesViewModel?.VehicleTreeItems != null)
                {
                    foreach (var treeItem in _vehiclesViewModel.VehicleTreeItems)
                    {
                        if (HasVehicleTreeItemValidationIssue(treeItem))
                            return true;
                    }
                }

                // Check outfit tree items
                if (_outfitsViewModel?.OutfitTreeItems != null)
                {
                    foreach (var treeItem in _outfitsViewModel.OutfitTreeItems)
                    {
                        if (HasOutfitTreeItemValidationIssue(treeItem))
                            return true;
                    }
                }

                return false;
            }
        }

        private bool HasTreeItemValidationIssue(RankTreeItemViewModel treeItem)
        {
            if (treeItem.HasValidationIssue)
                return true;

            foreach (var child in treeItem.Children)
            {
                if (HasTreeItemValidationIssue(child))
                    return true;
            }

            return false;
        }

        private bool HasVehicleTreeItemValidationIssue(VehicleTreeItemViewModel treeItem)
        {
            if (treeItem.HasValidationIssue)
                return true;

            foreach (var child in treeItem.Children)
            {
                if (HasVehicleTreeItemValidationIssue(child))
                    return true;
            }

            return false;
        }

        private bool HasOutfitTreeItemValidationIssue(OutfitTreeItemViewModel treeItem)
        {
            if (treeItem.HasValidationIssue)
                return true;

            foreach (var child in treeItem.Children)
            {
                if (HasOutfitTreeItemValidationIssue(child))
                    return true;
            }

            return false;
        }

        // Icon for validation button - priority: Error > Warning > Advisory > None
        public string ValidationButtonIconPath
        {
            get
            {
                if (HasValidationErrorsOnly)
                    return "/Resources/Icons/error-icon.png";
                if (HasWarnings)
                    return "/Resources/Icons/warning-icon.png";
                if (HasAdvisories)
                    return "/Resources/Icons/info-icon.png";
                return "/Resources/Icons/accept-icon.png"; // No issues
            }
        }

        public string GenerateButtonIconPath
        {
            get
            {
                if (HasValidationErrorsOnly)
                    return "/Resources/Icons/error-icon.png";
                if (HasWarnings || HasAdvisories)
                    return "/Resources/Icons/warning-icon.png";
                return "/Resources/Icons/accept-icon.png";
            }
        }

        public int LoadedAgenciesCount
        {
            get => _loadedAgenciesCount;
            set
            {
                if (SetProperty(ref _loadedAgenciesCount, value))
                {
                    OnPropertyChanged(nameof(LoadedDataSummary));
                }
            }
        }

        public int LoadedVehiclesCount
        {
            get => _loadedVehiclesCount;
            set
            {
                if (SetProperty(ref _loadedVehiclesCount, value))
                {
                    OnPropertyChanged(nameof(LoadedDataSummary));
                }
            }
        }

        public int LoadedStationsCount
        {
            get => _loadedStationsCount;
            set
            {
                if (SetProperty(ref _loadedStationsCount, value))
                {
                    OnPropertyChanged(nameof(LoadedDataSummary));
                }
            }
        }

        public int LoadedOutfitsCount
        {
            get => _loadedOutfitsCount;
            set
            {
                if (SetProperty(ref _loadedOutfitsCount, value))
                {
                    OnPropertyChanged(nameof(LoadedDataSummary));
                }
            }
        }

        public string LoadedDataSummary =>
            $"Loaded: {LoadedAgenciesCount} agencies, {LoadedVehiclesCount} vehicles, {LoadedStationsCount} stations, {LoadedOutfitsCount} outfits";

        public string LoadingStatusText
        {
            get => _loadingStatusText;
            set => SetProperty(ref _loadingStatusText, value);
        }

        public string LoadingDetailText
        {
            get => _loadingDetailText;
            set => SetProperty(ref _loadingDetailText, value);
        }

        public int LoadingProgress
        {
            get => _loadingProgress;
            set => SetProperty(ref _loadingProgress, value);
        }

        public bool IsLoadingIndeterminate
        {
            get => _isLoadingIndeterminate;
            set => SetProperty(ref _isLoadingIndeterminate, value);
        }

        public bool HasLoadingError
        {
            get => _hasLoadingError;
            set => SetProperty(ref _hasLoadingError, value);
        }

        public bool HasBackupsAvailable
        {
            get => _hasBackupsAvailable;
            set => SetProperty(ref _hasBackupsAvailable, value);
        }

        public string LoadingStatusForeground
        {
            get => _loadingStatusForeground;
            set => SetProperty(ref _loadingStatusForeground, value);
        }

        public string LoadingProgressForeground
        {
            get => _loadingProgressForeground;
            set => SetProperty(ref _loadingProgressForeground, value);
        }

        public string LoadingProgressBorderBrush
        {
            get => _loadingProgressBorderBrush;
            set => SetProperty(ref _loadingProgressBorderBrush, value);
        }

        public bool IsDebugLoggingEnabled
        {
            get => _isDebugLoggingEnabled;
            set => SetProperty(ref _isDebugLoggingEnabled, value);
        }

        public bool ShowDismissedValidations
        {
            get => _showDismissedValidations;
            set
            {
                if (SetProperty(ref _showDismissedValidations, value))
                {
                    OnPropertyChanged(nameof(ShowDismissedButtonText));
                    RunValidation(); // Refresh validation display to show/hide dismissed items
                }
            }
        }

        public string ShowDismissedButtonText => ShowDismissedValidations ? "Hide Dismissed" : "Show Dismissed";

        public ICommand GenerateCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand ShowValidationErrorsCommand { get; }
        public ICommand RestoreBackupCommand { get; }
        public ICommand ToggleXmlPreviewCommand { get; }
        public ICommand ToggleXmlPreviewThemeCommand { get; }
        public ICommand CloseApplicationCommand { get; }
        public ICommand OpenLogFileCommand { get; }
        public ICommand RestoreFromBackupCommand { get; }
        public ICommand OpenRanksXmlCommand { get; }
        public ICommand RetryLoadCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand DismissAllWarningsCommand { get; }
        public ICommand DismissAllAdvisoriesCommand { get; }
        public ICommand RefreshValidationCommand { get; }
        public ICommand ToggleShowDismissedCommand { get; }

        private bool CanUndo()
        {
            // Check if the most recently modified tab has undo capability
            return _lastModifiedTabIndex switch
            {
                0 => RanksViewModel?.UndoCommand.CanExecute(null) ?? false,
                1 => StationAssignmentsViewModel?.UndoCommand.CanExecute(null) ?? false,
                2 => VehiclesViewModel?.UndoCommand.CanExecute(null) ?? false,
                3 => OutfitsViewModel?.UndoCommand.CanExecute(null) ?? false,
                _ => false
            };
        }

        private void OnUndo()
        {
            // Switch to the tab that has the action to undo, then execute undo
            if (CurrentTabIndex != _lastModifiedTabIndex)
            {
                CurrentTabIndex = _lastModifiedTabIndex;
            }

            switch (_lastModifiedTabIndex)
            {
                case 0:
                    RanksViewModel?.UndoCommand.Execute(null);
                    break;
                case 1:
                    StationAssignmentsViewModel?.UndoCommand.Execute(null);
                    break;
                case 2:
                    VehiclesViewModel?.UndoCommand.Execute(null);
                    break;
                case 3:
                    OutfitsViewModel?.UndoCommand.Execute(null);
                    break;
            }
        }

        private bool CanRedo()
        {
            // Check if the most recently modified tab has redo capability
            return _lastModifiedTabIndex switch
            {
                0 => RanksViewModel?.RedoCommand.CanExecute(null) ?? false,
                1 => StationAssignmentsViewModel?.RedoCommand.CanExecute(null) ?? false,
                2 => VehiclesViewModel?.RedoCommand.CanExecute(null) ?? false,
                3 => OutfitsViewModel?.RedoCommand.CanExecute(null) ?? false,
                _ => false
            };
        }

        private void OnRedo()
        {
            // Switch to the tab that has the action to redo, then execute redo
            if (CurrentTabIndex != _lastModifiedTabIndex)
            {
                CurrentTabIndex = _lastModifiedTabIndex;
            }

            switch (_lastModifiedTabIndex)
            {
                case 0:
                    RanksViewModel?.RedoCommand.Execute(null);
                    break;
                case 1:
                    StationAssignmentsViewModel?.RedoCommand.Execute(null);
                    break;
                case 2:
                    VehiclesViewModel?.RedoCommand.Execute(null);
                    break;
                case 3:
                    OutfitsViewModel?.RedoCommand.Execute(null);
                    break;
            }
        }

        private void OnViewModelUndoRedoStateChanged(object? sender, EventArgs e)
        {
            // Determine which tab was modified and update the last modified tab index
            if (sender == RanksViewModel)
            {
                _lastModifiedTabIndex = 0;
            }
            else if (sender == StationAssignmentsViewModel)
            {
                _lastModifiedTabIndex = 1;
            }
            else if (sender == VehiclesViewModel)
            {
                _lastModifiedTabIndex = 2;
            }
            else if (sender == OutfitsViewModel)
            {
                _lastModifiedTabIndex = 3;
            }

            // Update MainWindow's Undo/Redo command states when any ViewModel's stack changes
            ((RelayCommand)UndoCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RedoCommand).RaiseCanExecuteChanged();
        }

        private bool CanDismissAllWarnings()
        {
            return ValidationErrorItems.Any(item => item.Type == "Warning" && item.CanDismiss);
        }

        private void OnDismissAllWarnings()
        {
            var warnings = ValidationErrorItems.Where(item => item.Type == "Warning" && !item.IsDismissed).ToList();
            foreach (var warning in warnings)
            {
                if (warning.RankId != null)
                {
                    var itemName = warning.ItemName ?? "";
                    _dismissalService.Dismiss(warning.RankId, warning.Severity, itemName, warning.Message);
                }
            }
            Logger.Info($"Dismissed {warnings.Count} warning(s)");
            RunValidation(); // Refresh display
        }

        private bool CanDismissAllAdvisories()
        {
            return ValidationErrorItems.Any(item => item.Type == "Advisory" && item.CanDismiss);
        }

        private void OnDismissAllAdvisories()
        {
            var advisories = ValidationErrorItems.Where(item => item.Type == "Advisory" && !item.IsDismissed).ToList();
            foreach (var advisory in advisories)
            {
                if (advisory.RankId != null)
                {
                    var itemName = advisory.ItemName ?? "";
                    _dismissalService.Dismiss(advisory.RankId, advisory.Severity, itemName, advisory.Message);
                }
            }
            Logger.Info($"Dismissed {advisories.Count} advisor{(advisories.Count == 1 ? "y" : "ies")}");
            RunValidation(); // Refresh display
        }

        private void OnRefreshValidation()
        {
            Logger.Info("[USER] Refresh validation - clearing all dismissals");
            _dismissalService.ClearAll();
            ShowDismissedValidations = false; // Reset toggle
            RunValidation(); // Rerun validation
        }

        private void OnToggleShowDismissed()
        {
            ShowDismissedValidations = !ShowDismissedValidations;
            Logger.Info($"[USER] {(ShowDismissedValidations ? "Showing" : "Hiding")} dismissed validations");
        }

        private void DismissValidationIssue(ValidationIssue issue)
        {
            if (issue.RankId != null)
            {
                // Call the ValidationIssue overload which applies message hash for unique keys
                _dismissalService.Dismiss(issue);

                var itemName = issue.ItemName ?? "";
                var itemDesc = string.IsNullOrEmpty(itemName) ? issue.Category : $"{issue.Category} '{itemName}'";
                Logger.Info($"[USER] Dismissed {issue.Severity} for {itemDesc} in rank '{issue.RankName}'");
                RunValidation(); // Refresh display
            }
        }

        private bool CanGenerate()
        {
            return !IsLoading;
        }

        private async void OnGenerate()
        {
            try
            {
                Logger.Info("[USER] Generate XML button clicked");

                // Get current ranks
                var ranks = RanksViewModel.RankHierarchies.ToList();

                if (ranks.Count == 0)
                {
                    StatusMessage = "Error: No ranks to save";
                    Logger.Warn("Cannot generate XML - no ranks data");
                    return;
                }

                // Commit any pending changes in RanksViewModel before validation
                RanksViewModel.CommitChanges();
                Logger.Info("Committed pending changes before validation");

                // Force a full validation refresh to ensure all state is up-to-date
                RunValidation();
                Logger.Info("Full validation refresh completed");

                // Run validation
                var validationService = new StartupValidationService(_dataService);
                var report = validationService.ValidateRanks(ranks);

                // Determine backup path
                var ranksPath = Parsers.RanksParser.FindRanksXml(_gtaRootPath, _currentProfile);
                if (ranksPath == null)
                {
                    // Create default path
                    var profilePath = System.IO.Path.Combine(_gtaRootPath, "plugins", "LSPDFR", "LSPDFR Enhanced", "Profiles", _currentProfile);
                    System.IO.Directory.CreateDirectory(profilePath);
                    ranksPath = System.IO.Path.Combine(profilePath, "Ranks.xml");
                }

                var backupPath = Services.BackupPathHelper.GetBackupFilePath(_settingsManager, _currentProfile, System.DateTime.Now);

                // Show appropriate confirmation dialog based on validation results
                bool shouldGenerate = false;

                if (report.HasErrors)
                {
                    // CRITICAL ERRORS - Must fix before generating
                    shouldGenerate = await ShowValidationErrorModal(report);
                }
                else if (report.HasWarnings)
                {
                    // WARNINGS - Can generate anyway
                    shouldGenerate = await ShowValidationWarningModal(report, backupPath);
                }
                else
                {
                    // NO ISSUES - Show simple confirmation
                    shouldGenerate = await ShowBackupConfirmationModal(backupPath);
                }

                if (!shouldGenerate)
                {
                    Logger.Info("User cancelled XML generation");
                    StatusMessage = "XML generation cancelled";
                    return;
                }

                // Proceed with generation
                StatusMessage = "Generating configuration...";

                // Generate XML
                string xmlContent = RanksXmlGenerator.GenerateXml(ranks);

                // Backup existing file if it exists
                if (System.IO.File.Exists(ranksPath))
                {
                    // Ensure backup directory exists
                    var backupDir = System.IO.Path.GetDirectoryName(backupPath);
                    if (!string.IsNullOrEmpty(backupDir) && !System.IO.Directory.Exists(backupDir))
                    {
                        System.IO.Directory.CreateDirectory(backupDir);
                        Logger.Info($"Created backup directory: {backupDir}");
                    }

                    System.IO.File.Copy(ranksPath, backupPath, overwrite: true);
                    Logger.Info($"Created backup: {backupPath}");

                    // Cleanup old backups
                    Services.BackupPathHelper.CleanupOldBackups(_settingsManager, _currentProfile);
                }

                // Write XML to file with UTF-8 encoding (no BOM)
                System.IO.File.WriteAllText(ranksPath, xmlContent, new System.Text.UTF8Encoding(false));
                Logger.Info($"Successfully saved Ranks.xml to: {ranksPath}");

                // Update XML preview
                RegenerateXmlPreview();

                StatusMessage = $"Configuration saved successfully to {_currentProfile} profile";
            }
            catch (System.Exception ex)
            {
                Logger.Error($"Failed to generate configuration: {ex.Message}");
                StatusMessage = $"Error: Failed to save configuration - {ex.Message}";
            }
        }

        private void OnExit()
        {
            Logger.Info("Exit button clicked");

            // Close the application
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }

        private void OnCloseApplication()
        {
            Logger.Info("Close application clicked from loading error");
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }

        private void OnOpenLogFile()
        {
            Logger.Info("Open log file clicked from loading error");
            try
            {
                var logFilePath = System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                    "LSPDFREnhancedConfigurator",
                    "logs",
                    $"lspdfr-configurator-{System.DateTime.Now:yyyy-MM-dd}.log"
                );

                if (System.IO.File.Exists(logFilePath))
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = logFilePath,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                }
                else
                {
                    Logger.Warn($"Log file does not exist: {logFilePath}");
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error($"Failed to open log file: {ex.Message}");
            }
        }

        private void OnRestoreFromBackup()
        {
            Logger.Info("Restore from backup clicked from loading error");
            _restoreBackupAction?.Invoke();
        }

        public void SetRestoreBackupAction(System.Action action)
        {
            _restoreBackupAction = action;
        }

        private void OnRetryLoad()
        {
            Logger.Info("[USER] Retry Load clicked from loading error");
            _retryLoadAction?.Invoke();
        }

        public void SetRetryLoadAction(System.Action action)
        {
            _retryLoadAction = action;
        }

        private void OnOpenRanksXml()
        {
            Logger.Info("[USER] Open Ranks.xml clicked from loading error");
            try
            {
                var ranksPath = Parsers.RanksParser.FindRanksXml(_gtaRootPath, _currentProfile);
                if (ranksPath != null && System.IO.File.Exists(ranksPath))
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = ranksPath,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                    Logger.Info($"Opened Ranks.xml: {ranksPath}");
                }
                else
                {
                    Logger.Warn($"Ranks.xml not found for profile: {_currentProfile}");
                    StatusMessage = "Ranks.xml file not found";
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error($"Failed to open Ranks.xml: {ex.Message}");
                StatusMessage = $"Error opening Ranks.xml: {ex.Message}";
            }
        }

        /// <summary>
        /// Shows modal when no validation issues are present
        /// </summary>
        private async System.Threading.Tasks.Task<bool> ShowBackupConfirmationModal(string backupPath)
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                return false;

            var dialog = new Window
            {
                Title = "Confirm XML Generation",
                Width = 550,
                Height = 280,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                Content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(20),
                    Spacing = 15,
                    Children =
                    {
                        new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            Spacing = 12,
                            Children =
                            {
                                new Image
                                {
                                    Source = new Avalonia.Media.Imaging.Bitmap(Avalonia.Platform.AssetLoader.Open(new System.Uri("avares://LSPDFREnhancedConfigurator/Resources/Icons/success-icon.png"))),
                                    Width = 32,
                                    Height = 32,
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                },
                                new TextBlock
                                {
                                    Text = "Ready to Generate",
                                    FontSize = 18,
                                    FontWeight = Avalonia.Media.FontWeight.Bold,
                                    Foreground = Avalonia.Media.Brushes.LimeGreen,
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                }
                            }
                        },
                        new TextBlock
                        {
                            Text = "A backup of your Ranks.xml file will be saved to:",
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap
                        },
                        new Border
                        {
                            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#1A1A1A")),
                            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#00D9FF")),
                            BorderThickness = new Avalonia.Thickness(1),
                            Padding = new Avalonia.Thickness(8),
                            CornerRadius = new Avalonia.CornerRadius(4),
                            Child = new TextBlock
                            {
                                Text = backupPath,
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                FontFamily = new Avalonia.Media.FontFamily("Consolas,Courier New,monospace"),
                                FontSize = 11,
                                Foreground = Avalonia.Media.Brushes.White
                            }
                        },
                        new TextBlock
                        {
                            Text = "💡 Tip: You can change the backup directory in Settings",
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            FontSize = 11,
                            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#00ADEA")),
                            FontStyle = Avalonia.Media.FontStyle.Italic
                        },
                        new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            Spacing = 12,
                            Margin = new Avalonia.Thickness(0, 15, 0, 0),
                            Children =
                            {
                                new Button
                                {
                                    Content = "OK",
                                    MinWidth = 120
                                },
                                new Button
                                {
                                    Content = "Cancel",
                                    MinWidth = 120
                                }
                            }
                        }
                    }
                }
            };

            var buttonPanel = (StackPanel)((StackPanel)dialog.Content).Children[4];
            var okButton = (Button)buttonPanel.Children[0];
            var cancelButton = (Button)buttonPanel.Children[1];

            bool result = false;
            okButton.Click += (s, e) => { result = true; dialog.Close(); };
            cancelButton.Click += (s, e) => { result = false; dialog.Close(); };

            await dialog.ShowDialog(desktop.MainWindow);
            return result;
        }

        /// <summary>
        /// Shows modal when validation warnings are present (can generate anyway)
        /// </summary>
        private async System.Threading.Tasks.Task<bool> ShowValidationWarningModal(ValidationResult validationResult, string backupPath)
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                return false;

            var warningsList = string.Join("\n", validationResult.Warnings.Select(w => $"• {w.Message}"));

            var dialog = new Window
            {
                Title = "Validation Warnings",
                Width = 700,
                Height = 550,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = true,
                Content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(20),
                    Spacing = 12,
                    Children =
                    {
                        new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            Spacing = 12,
                            Children =
                            {
                                new Image
                                {
                                    Source = new Avalonia.Media.Imaging.Bitmap(Avalonia.Platform.AssetLoader.Open(new System.Uri("avares://LSPDFREnhancedConfigurator/Resources/Icons/warning-icon.png"))),
                                    Width = 32,
                                    Height = 32,
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                },
                                new TextBlock
                                {
                                    Text = $"Validation Warnings Detected ({validationResult.WarningCount})",
                                    FontSize = 18,
                                    FontWeight = Avalonia.Media.FontWeight.Bold,
                                    Foreground = Avalonia.Media.Brushes.Orange,
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                }
                            }
                        },
                        new ScrollViewer
                        {
                            MaxHeight = 200,
                            Content = new TextBlock
                            {
                                Text = warningsList,
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                FontFamily = new Avalonia.Media.FontFamily("Consolas,Courier New,monospace"),
                                FontSize = 11
                            }
                        },
                        new TextBlock
                        {
                            Text = "You can still generate your ranks file, but there is invalid data that can possibly cause issues in your game.",
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            FontStyle = Avalonia.Media.FontStyle.Italic,
                            Foreground = Avalonia.Media.Brushes.Orange
                        },
                        new TextBlock
                        {
                            Text = "Backup will be saved to:",
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            Margin = new Avalonia.Thickness(0, 10, 0, 0)
                        },
                        new Border
                        {
                            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#1A1A1A")),
                            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#00D9FF")),
                            BorderThickness = new Avalonia.Thickness(1),
                            Padding = new Avalonia.Thickness(8),
                            CornerRadius = new Avalonia.CornerRadius(4),
                            Child = new TextBlock
                            {
                                Text = backupPath,
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                FontFamily = new Avalonia.Media.FontFamily("Consolas,Courier New,monospace"),
                                FontSize = 11,
                                Foreground = Avalonia.Media.Brushes.White
                            }
                        },
                        new TextBlock
                        {
                            Text = "💡 Tip: You can change the backup directory in Settings",
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            FontSize = 11,
                            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#00ADEA")),
                            FontStyle = Avalonia.Media.FontStyle.Italic
                        },
                        new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            Spacing = 12,
                            Margin = new Avalonia.Thickness(0, 15, 0, 0),
                            Children =
                            {
                                new Button
                                {
                                    Content = "Go Back",
                                    MinWidth = 140
                                },
                                new Button
                                {
                                    Content = "Show Validation Errors Panel",
                                    MinWidth = 200
                                },
                                new Button
                                {
                                    Content = "Generate Anyway",
                                    MinWidth = 140
                                }
                            }
                        }
                    }
                }
            };

            var buttonPanel = (StackPanel)((StackPanel)dialog.Content).Children[6];
            var goBackButton = (Button)buttonPanel.Children[0];
            var showErrorsButton = (Button)buttonPanel.Children[1];
            var generateAnywayButton = (Button)buttonPanel.Children[2];

            bool result = false;
            goBackButton.Click += (s, e) => { result = false; dialog.Close(); };
            showErrorsButton.Click += (s, e) =>
            {
                result = false;
                dialog.Close();
                IsValidationPanelVisible = true;
                RunValidation();
            };
            generateAnywayButton.Click += (s, e) => { result = true; dialog.Close(); };

            await dialog.ShowDialog(desktop.MainWindow);
            return result;
        }

        /// <summary>
        /// Shows modal when critical validation errors are present (cannot generate)
        /// </summary>
        private async System.Threading.Tasks.Task<bool> ShowValidationErrorModal(ValidationResult validationResult)
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                return false;

            var errorsList = string.Join("\n", validationResult.Errors.Select(e => $"• {e.Message}"));

            var dialog = new Window
            {
                Title = "Critical Validation Errors",
                Width = 700,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = true,
                Content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(20),
                    Spacing = 12,
                    Children =
                    {
                        new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            Spacing = 12,
                            Children =
                            {
                                new Image
                                {
                                    Source = new Avalonia.Media.Imaging.Bitmap(Avalonia.Platform.AssetLoader.Open(new System.Uri("avares://LSPDFREnhancedConfigurator/Resources/Icons/error-icon.png"))),
                                    Width = 32,
                                    Height = 32,
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                },
                                new TextBlock
                                {
                                    Text = $"Critical Errors Detected ({validationResult.ErrorCount})",
                                    FontSize = 18,
                                    FontWeight = Avalonia.Media.FontWeight.Bold,
                                    Foreground = Avalonia.Media.Brushes.Red,
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                }
                            }
                        },
                        new ScrollViewer
                        {
                            MaxHeight = 250,
                            Content = new TextBlock
                            {
                                Text = errorsList,
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                FontFamily = new Avalonia.Media.FontFamily("Consolas,Courier New,monospace"),
                                FontSize = 11,
                                Foreground = Avalonia.Media.Brushes.Red
                            }
                        },
                        new TextBlock
                        {
                            Text = "These errors must be fixed before generating the XML file. The current configuration would break XML parsing or cause major issues in the game.",
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            FontWeight = Avalonia.Media.FontWeight.Bold,
                            Foreground = Avalonia.Media.Brushes.Red
                        },
                        new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            Spacing = 12,
                            Margin = new Avalonia.Thickness(0, 15, 0, 0),
                            Children =
                            {
                                new Button
                                {
                                    Content = "Go Back",
                                    MinWidth = 140
                                }
                            }
                        }
                    }
                }
            };

            var buttonPanel = (StackPanel)((StackPanel)dialog.Content).Children[3];
            var goBackButton = (Button)buttonPanel.Children[0];

            goBackButton.Click += (s, e) => dialog.Close();

            await dialog.ShowDialog(desktop.MainWindow);
            return false; // Always return false for error modal
        }

        /// <summary>
        /// Shows modal with a list of error messages
        /// </summary>
        private async System.Threading.Tasks.Task ShowRanksValidationErrorModal(List<string> errors)
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                return;

            var errorsList = string.Join("\n", errors.Select(e => $"• {e}"));

            var dialog = new Window
            {
                Title = "Ranks Validation Errors",
                Width = 700,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = true,
                Content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(20),
                    Spacing = 12,
                    Children =
                    {
                        new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            Spacing = 12,
                            Children =
                            {
                                new Image
                                {
                                    Source = new Avalonia.Media.Imaging.Bitmap(Avalonia.Platform.AssetLoader.Open(new System.Uri("avares://LSPDFREnhancedConfigurator/Resources/Icons/error-icon.png"))),
                                    Width = 32,
                                    Height = 32,
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                },
                                new TextBlock
                                {
                                    Text = $"Validation Errors ({errors.Count})",
                                    FontSize = 18,
                                    FontWeight = Avalonia.Media.FontWeight.Bold,
                                    Foreground = Avalonia.Media.Brushes.Red,
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                }
                            }
                        },
                        new ScrollViewer
                        {
                            MaxHeight = 250,
                            Content = new TextBlock
                            {
                                Text = errorsList,
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                FontFamily = new Avalonia.Media.FontFamily("Consolas,Courier New,monospace"),
                                FontSize = 11,
                                Foreground = Avalonia.Media.Brushes.Red
                            }
                        },
                        new TextBlock
                        {
                            Text = "Cannot save configuration. Please fix these errors before generating the XML file.",
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            FontWeight = Avalonia.Media.FontWeight.Bold,
                            Foreground = Avalonia.Media.Brushes.Red
                        },
                        new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            Spacing = 12,
                            Margin = new Avalonia.Thickness(0, 15, 0, 0),
                            Children =
                            {
                                new Button
                                {
                                    Content = "OK",
                                    MinWidth = 140
                                }
                            }
                        }
                    }
                }
            };

            var buttonPanel = (StackPanel)((StackPanel)dialog.Content).Children[3];
            var okButton = (Button)buttonPanel.Children[0];

            okButton.Click += (s, e) => dialog.Close();

            await dialog.ShowDialog(desktop.MainWindow);
        }

        private async void OnProfileChanged()
        {
            // Skip during initialization to avoid redundant load
            if (_isInitializing)
                return;

            if (!string.IsNullOrEmpty(SelectedProfile) && SelectedProfile != _currentProfile)
            {
                Logger.Info($"Profile changed to: {SelectedProfile}");
                StatusMessage = $"Loading profile: {SelectedProfile}...";

                try
                {
                    // Save the selected profile
                    _settingsManager.SetSelectedProfile(SelectedProfile);

                    // Load ranks for new profile
                    var ranksPath = Parsers.RanksParser.FindRanksXml(_gtaRootPath, SelectedProfile);
                    if (ranksPath != null)
                    {
                        var loadedRanks = Parsers.RanksParser.ParseRanksFile(ranksPath);
                        Logger.Info($"Loaded {loadedRanks.Count} ranks from {ranksPath}");

                        // Link station references for loaded ranks
                        _dataService.LinkStationReferencesForHierarchies(loadedRanks);
                        Logger.Info($"Linked station references for {loadedRanks.Count} rank hierarchies");

                        // Update RanksViewModel with new data
                        RanksViewModel.LoadRanks(loadedRanks);

                        // Update other ViewModels
                        StationAssignmentsViewModel.LoadRanks(loadedRanks);
                        VehiclesViewModel.LoadRanks(loadedRanks);
                        OutfitsViewModel.LoadRanks(loadedRanks);

                        // Regenerate XML preview
                        RegenerateXmlPreview();

                        StatusMessage = $"Profile '{SelectedProfile}' loaded successfully";
                    }
                    else
                    {
                        Logger.Warn($"No Ranks.xml found for profile: {SelectedProfile}");
                        StatusMessage = $"No Ranks.xml found for profile '{SelectedProfile}'";
                    }
                }
                catch (System.Exception ex)
                {
                    Logger.Error($"Failed to load profile: {ex.Message}");
                    StatusMessage = $"Error loading profile: {ex.Message}";
                }
            }
        }

        private void OnShowValidationErrors()
        {
            Logger.Info("Show Validation Errors clicked");
            IsValidationPanelVisible = !IsValidationPanelVisible;

            // Run validation on current ranks
            if (IsValidationPanelVisible)
            {
                RunValidation();
            }
        }

        private void RunValidation()
        {
            try
            {
                var validationService = new StartupValidationService(_dataService);
                var ranks = RanksViewModel.RankHierarchies.ToList();
                var report = validationService.ValidateRanks(ranks);

                // Get validation severity filter from settings
                var validationFilter = (ValidationFilterLevel)_settingsManager.GetValidationSeverity();

                // Parse errors into structured items
                ValidationErrorItems.Clear();

                // Collect all items to be displayed
                var itemsToAdd = new List<ValidationErrorItem>();

                // Add items based on severity filter
                switch (validationFilter)
                {
                    case ValidationFilterLevel.ErrorsOnly:
                        // Only show errors
                        foreach (var error in report.Errors)
                        {
                            itemsToAdd.Add(ParseValidationIssue(error));
                        }
                        break;

                    case ValidationFilterLevel.WarningsAndErrorsOnly:
                        // Show errors and warnings
                        foreach (var error in report.Errors)
                        {
                            itemsToAdd.Add(ParseValidationIssue(error));
                        }
                        foreach (var warning in report.Warnings)
                        {
                            itemsToAdd.Add(ParseValidationIssue(warning));
                        }
                        break;

                    case ValidationFilterLevel.ShowAll:
                    default:
                        // Show everything (errors, warnings, and advisories)
                        foreach (var error in report.Errors)
                        {
                            itemsToAdd.Add(ParseValidationIssue(error));
                        }
                        foreach (var warning in report.Warnings)
                        {
                            itemsToAdd.Add(ParseValidationIssue(warning));
                        }
                        foreach (var advisory in report.Advisories)
                        {
                            itemsToAdd.Add(ParseValidationIssue(advisory));
                        }
                        break;
                }

                // Filter out dismissed items if ShowDismissedValidations is false
                foreach (var item in itemsToAdd)
                {
                    if (ShowDismissedValidations || !item.IsDismissed)
                    {
                        ValidationErrorItems.Add(item);
                    }
                }

                // All validation issues are now reported through ValidationResult
                // Tree items display visual indicators, but don't add separate issues
                AddTreeItemValidationIssues(validationFilter);

                // Update error count - only count non-dismissed items (or all items if showing dismissed)
                if (ShowDismissedValidations)
                {
                    // Show total count including dismissed items
                    ValidationErrorCount = itemsToAdd.Count;
                }
                else
                {
                    // Only count non-dismissed items
                    ValidationErrorCount = itemsToAdd.Count(item => !item.IsDismissed);
                }

                if (report.HasIssues)
                {
                    ValidationErrorsText = report.GetSummary();
                }
                else
                {
                    ValidationErrorsText = "✅ No validation errors found.\n\nAll ranks have valid progression and references.";
                }

                Logger.Info($"Validation completed: {report.ErrorCount} error(s), {report.WarningCount} warning(s), severity filter: {validationFilter}");

                // Notify UI that validation button properties have changed
                OnPropertyChanged(nameof(ValidationButtonText));
                OnPropertyChanged(nameof(ValidationButtonIconPath));
                OnPropertyChanged(nameof(GenerateButtonIconPath));
                OnPropertyChanged(nameof(HasValidationErrors));
                OnPropertyChanged(nameof(HasValidationErrorsOnly));
                OnPropertyChanged(nameof(HasWarnings));
                OnPropertyChanged(nameof(HasAdvisories));

                // Check validation state to update button enabled state
                CheckValidationState();
            }
            catch (System.Exception ex)
            {
                Logger.Error($"Error running validation: {ex.Message}");
                ValidationErrorsText = $"❌ Error running validation:\n{ex.Message}";
            }
        }

        private void AddTreeItemValidationIssues(ValidationFilterLevel filter)
        {
            // Tree item validation is now handled entirely by the validation service
            // Tree items display validation state visually, but issues are reported through ValidationResult
            // This method is kept for future extensibility if needed
        }


        public ValidationErrorItem ParseValidationIssue(ValidationIssue issue)
        {
            // Map severity to type string
            string type = issue.Severity switch
            {
                ValidationSeverity.Error => "Error",
                ValidationSeverity.Warning => "Warning",
                ValidationSeverity.Advisory => "Advisory",
                _ => "Info"
            };

            // Check if issue has auto-fix capability
            RelayCommand? removeCommand = null;
            if (issue.IsAutoFixable && issue.ItemName != null && issue.Category != null)
            {
                removeCommand = new RelayCommand(() => RemoveInvalidItem(
                    issue.RankName ?? "",
                    issue.Category,
                    issue.ItemName ?? ""));
            }

            // Check if issue can be shown (has a specific location)
            RelayCommand? showCommand = null;
            if (!string.IsNullOrEmpty(issue.RankName) && !string.IsNullOrEmpty(issue.Category))
            {
                showCommand = new RelayCommand(() => ShowValidationIssue(
                    issue.RankName ?? "",
                    issue.Category,
                    issue.ItemName ?? ""));
            }

            // Check if issue can be dismissed (warnings and advisories only, NOT errors)
            // Dismissal requires RankId and Category at minimum (ItemName can be empty for general advisories)
            RelayCommand? dismissCommand = null;
            if (issue.Severity != ValidationSeverity.Error && issue.RankId != null && !string.IsNullOrEmpty(issue.Category))
            {
                dismissCommand = new RelayCommand(() => DismissValidationIssue(issue));
            }

            // Check if issue is currently dismissed
            bool isDismissed = _dismissalService.IsDismissed(issue);

            if (isDismissed)
            {
                Logger.Debug($"Issue marked as dismissed: {issue.Severity} - Category: '{issue.Category}', ItemName: '{issue.ItemName ?? ""}', RankName: '{issue.RankName}'");
            }

            return new ValidationErrorItem
            {
                Type = type,
                Severity = issue.Category,
                RankName = issue.RankName ?? "",
                ItemName = issue.ItemName ?? "",
                Message = issue.Message,
                RemoveCommand = removeCommand,
                ShowCommand = showCommand,
                RankId = issue.RankId,
                DismissCommand = dismissCommand,
                IsDismissed = isDismissed
            };
        }

        private void RemoveInvalidItem(string rankName, string itemType, string itemName)
        {
            Logger.Info($"Removing invalid {itemType} '{itemName}' from rank '{rankName}'");

            try
            {
                // Find the rank in the hierarchy (check both parent ranks and pay bands)
                Models.RankHierarchy? rank = RanksViewModel.RankHierarchies.FirstOrDefault(r => r.Name == rankName);

                // If not found in parent ranks, search pay bands
                if (rank == null)
                {
                    foreach (var parentRank in RanksViewModel.RankHierarchies)
                    {
                        if (parentRank.IsParent && parentRank.PayBands.Count > 0)
                        {
                            rank = parentRank.PayBands.FirstOrDefault(pb => pb.Name == rankName);
                            if (rank != null)
                                break;
                        }
                    }
                }

                if (rank == null)
                {
                    Logger.Warn($"Could not find rank '{rankName}' to remove item from");
                    return;
                }

                bool removed = false;

                switch (itemType)
                {
                    case "Vehicle":
                        var vehicle = rank.Vehicles.FirstOrDefault(v => v.Model == itemName);
                        if (vehicle != null)
                        {
                            rank.Vehicles.Remove(vehicle);
                            removed = true;
                            Logger.Info($"Removed vehicle '{itemName}' from rank '{rankName}'");
                        }
                        break;

                    case "Station":
                        var station = rank.Stations.FirstOrDefault(s => s.StationName == itemName);
                        if (station != null)
                        {
                            rank.Stations.Remove(station);
                            removed = true;
                            Logger.Info($"Removed station '{itemName}' from rank '{rankName}'");
                        }
                        break;

                    case "Outfit":
                        if (rank.Outfits.Contains(itemName))
                        {
                            rank.Outfits.Remove(itemName);
                            removed = true;
                            Logger.Info($"Removed outfit '{itemName}' from rank '{rankName}'");
                        }
                        break;
                }

                if (removed)
                {
                    // Refresh the affected ViewModels
                    if (itemType == "Vehicle")
                    {
                        VehiclesViewModel.LoadRanks(RanksViewModel.RankHierarchies.ToList());
                    }
                    else if (itemType == "Station")
                    {
                        StationAssignmentsViewModel.LoadRanks(RanksViewModel.RankHierarchies.ToList());
                    }
                    else if (itemType == "Outfit")
                    {
                        OutfitsViewModel.LoadRanks(RanksViewModel.RankHierarchies.ToList());
                    }

                    // Re-run validation to update the list
                    RunValidation();

                    // Trigger RanksViewModel to update tree item validation icons/tooltips
                    RanksViewModel.LoadRanks(RanksViewModel.RankHierarchies.ToList());

                    StatusMessage = $"Removed invalid {itemType.ToLower()} '{itemName}' from rank '{rankName}'";
                }
                else
                {
                    Logger.Warn($"Item '{itemName}' not found in rank '{rankName}'");
                    StatusMessage = $"Could not find {itemType.ToLower()} '{itemName}' in rank '{rankName}'";
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error($"Error removing invalid item: {ex.Message}");
                StatusMessage = $"Error removing item: {ex.Message}";
            }
        }

        private void ShowValidationIssue(string rankName, string itemType, string itemName)
        {
            Logger.Info($"[USER] Show validation issue: {itemType} '{itemName}' in rank '{rankName}'");

            try
            {
                // Find the rank in the hierarchy
                Models.RankHierarchy? rank = RanksViewModel.RankHierarchies.FirstOrDefault(r => r.Name == rankName);

                // If not found in parent ranks, search pay bands
                if (rank == null)
                {
                    foreach (var parentRank in RanksViewModel.RankHierarchies)
                    {
                        if (parentRank.IsParent && parentRank.PayBands.Count > 0)
                        {
                            rank = parentRank.PayBands.FirstOrDefault(pb => pb.Name == rankName);
                            if (rank != null)
                                break;
                        }
                    }
                }

                if (rank == null)
                {
                    Logger.Warn($"Could not find rank '{rankName}' to show issue");
                    StatusMessage = $"Could not find rank '{rankName}'";
                    return;
                }

                // Switch to the appropriate tab and select the rank
                switch (itemType)
                {
                    case "Vehicle":
                        CurrentTabIndex = 2; // Vehicles tab
                        VehiclesViewModel.SelectedRank = rank;
                        StatusMessage = $"Showing vehicles for rank '{rankName}'";
                        break;

                    case "Station":
                        CurrentTabIndex = 1; // Station Assignments tab
                        StationAssignmentsViewModel.SelectedRank = rank;
                        StatusMessage = $"Showing station assignments for rank '{rankName}'";
                        break;

                    case "Outfit":
                        CurrentTabIndex = 3; // Outfits tab
                        OutfitsViewModel.SelectedRank = rank;
                        StatusMessage = $"Showing outfits for rank '{rankName}'";
                        break;

                    case "Rank":
                    default:
                        CurrentTabIndex = 0; // Ranks tab
                        StatusMessage = $"Showing rank '{rankName}'";
                        break;
                }

                Logger.Info($"Successfully navigated to {itemType} issue in rank '{rankName}'");
            }
            catch (System.Exception ex)
            {
                Logger.Error($"Error showing validation issue: {ex.Message}");
                StatusMessage = $"Error showing issue: {ex.Message}";
            }
        }

        private async void OnRestoreBackup()
        {
            Logger.Info("[OnRestoreBackup] Restore Backup clicked");

            try
            {
                Logger.Info("[OnRestoreBackup] Creating RestoreBackupDialogViewModel");
                var restoreViewModel = new RestoreBackupDialogViewModel(_gtaRootPath, _currentProfile, _settingsManager);

                Logger.Info("[OnRestoreBackup] Creating RestoreBackupDialog");
                var restoreDialog = new RestoreBackupDialog
                {
                    DataContext = restoreViewModel
                };

                if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    Logger.Info("[OnRestoreBackup] Showing restore dialog...");
                    var result = await restoreDialog.ShowDialog<bool>(desktop.MainWindow);
                    Logger.Info($"[OnRestoreBackup] Dialog closed with result: {result}");

                    if (result)
                    {
                        Logger.Info("[OnRestoreBackup] Setting status message");
                        StatusMessage = "Backup restored successfully. Please restart the application to reload the data.";
                        Logger.Info("[OnRestoreBackup] Backup restored successfully - status message set");
                    }
                    else
                    {
                        Logger.Info("[OnRestoreBackup] Restore was cancelled or failed");
                    }
                }

                Logger.Info("[OnRestoreBackup] OnRestoreBackup method completing normally");
            }
            catch (System.Exception ex)
            {
                Logger.Error($"[OnRestoreBackup] Error in restore backup: {ex.Message}", ex);
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private void OnToggleXmlPreview()
        {
            IsXmlPreviewVisible = !IsXmlPreviewVisible;
            Logger.Info($"XML Preview toggled: {IsXmlPreviewVisible}");
        }

        private void OnToggleXmlPreviewTheme()
        {
            IsXmlPreviewDarkMode = !IsXmlPreviewDarkMode;
            Logger.Info($"XML Preview theme toggled: {(IsXmlPreviewDarkMode ? "Dark" : "Light")} mode");
        }

        public void UpdateLoadingProgress(string status, string detail, int progress)
        {
            LoadingStatusText = status;
            LoadingDetailText = detail;
            LoadingProgress = progress;
            IsLoadingIndeterminate = false;
        }

        public void SetLoadingIndeterminate(string status, string detail)
        {
            LoadingStatusText = status;
            LoadingDetailText = detail;
            IsLoadingIndeterminate = true;
        }

        public void Initialize(DataLoadingService dataService, List<Models.RankHierarchy>? loadedRanks)
        {
            // Set loaded data counts
            LoadedAgenciesCount = dataService.Agencies.Count;
            LoadedVehiclesCount = dataService.AllVehicles.Count;
            LoadedStationsCount = dataService.Stations.Count;
            LoadedOutfitsCount = dataService.OutfitVariations.Count;

            // Create ViewModels with loaded data - this replaces the stub ViewModels
            RanksViewModel = new RanksViewModel(loadedRanks, dataService);
            StationAssignmentsViewModel = new StationAssignmentsViewModel(dataService);
            VehiclesViewModel = new VehiclesViewModel(dataService, loadedRanks);
            OutfitsViewModel = new OutfitsViewModel(dataService, loadedRanks);

            // Set selection state service for rank selection synchronization
            StationAssignmentsViewModel.SetSelectionStateService(_selectionStateService);
            VehiclesViewModel.SetSelectionStateService(_selectionStateService);
            OutfitsViewModel.SetSelectionStateService(_selectionStateService);

            // Subscribe to data change events for real-time XML preview updates
            RanksViewModel.DataChanged += OnChildViewModelDataChanged;
            StationAssignmentsViewModel.DataChanged += OnChildViewModelDataChanged;
            VehiclesViewModel.DataChanged += OnChildViewModelDataChanged;
            OutfitsViewModel.DataChanged += OnChildViewModelDataChanged;

            // Subscribe to tab focus request from RanksViewModel (for Undo/Redo)
            RanksViewModel.RequestFocusRanksTab += OnRequestFocusRanksTab;

            // Subscribe to status message updates from RanksViewModel
            RanksViewModel.StatusMessageChanged += OnRanksViewModelStatusMessageChanged;

            // Subscribe to undo/redo state changes from all ViewModels
            RanksViewModel.UndoRedoStateChanged += OnViewModelUndoRedoStateChanged;
            StationAssignmentsViewModel.UndoRedoStateChanged += OnViewModelUndoRedoStateChanged;
            VehiclesViewModel.UndoRedoStateChanged += OnViewModelUndoRedoStateChanged;
            OutfitsViewModel.UndoRedoStateChanged += OnViewModelUndoRedoStateChanged;

            // Load ranks into other ViewModels
            if (loadedRanks != null && loadedRanks.Count > 0)
            {
                StationAssignmentsViewModel.LoadRanks(loadedRanks);
                VehiclesViewModel.LoadRanks(loadedRanks);
                OutfitsViewModel.LoadRanks(loadedRanks);
            }

            // Generate initial XML preview
            RegenerateXmlPreview();

            StatusMessage = "Ready";
        }

        public void RegenerateXmlPreview()
        {
            try
            {
                // Get current ranks from RanksViewModel
                var ranks = RanksViewModel.RankHierarchies.ToList();

                Logger.Debug($"RegenerateXmlPreview called. Ranks count: {ranks.Count}");

                // Log first rank details for debugging
                if (ranks.Count > 0)
                {
                    var firstRank = ranks[0];
                    Logger.Debug($"First rank: Name='{firstRank.Name}', RequiredPoints={firstRank.RequiredPoints}, Salary={firstRank.Salary}");
                }

                if (ranks.Count > 0)
                {
                    // Generate XML using RanksXmlGenerator
                    string xmlContent = RanksXmlGenerator.GenerateXml(ranks);
                    Logger.Debug($"Generated XML content length: {xmlContent.Length} chars");

                    // Find the first Name tag in the XML to verify actual content
                    var nameTagStart = xmlContent.IndexOf("<Name>");
                    if (nameTagStart >= 0)
                    {
                        var nameTagEnd = xmlContent.IndexOf("</Name>", nameTagStart);
                        if (nameTagEnd >= 0)
                        {
                            var nameInXml = xmlContent.Substring(nameTagStart + 6, nameTagEnd - nameTagStart - 6);
                            Logger.Debug($"First Name in generated XML: '{nameInXml}'");
                        }
                    }

                    var oldText = XmlPreviewText;
                    var changed = oldText != xmlContent;

                    XmlPreviewText = xmlContent;

                    Logger.Debug($"XML Preview regenerated. Changed: {changed}, Old length: {oldText?.Length ?? 0}, New length: {xmlContent.Length}");
                }
                else
                {
                    XmlPreviewText = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<!-- No ranks data available -->";
                    Logger.Warn("Cannot generate XML preview - no ranks loaded");
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error($"Failed to generate XML preview: {ex.Message}");
                XmlPreviewText = $"<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<!-- Error generating XML: {ex.Message} -->";
            }
        }

        private void OnChildViewModelDataChanged(object sender, EventArgs e)
        {
            // Regenerate XML preview whenever child ViewModels report data changes
            Logger.Debug($"Data changed event received from {sender?.GetType().Name}");
            RegenerateXmlPreview();

            // Always run validation to update button text/icon in real-time
            RunValidation();

            // Check validation state and update UI
            CheckValidationState();

            // If station assignments changed, refresh vehicle and outfit trees to show new stations
            if (sender == StationAssignmentsViewModel)
            {
                VehiclesViewModel?.RefreshVehicleTree();
                OutfitsViewModel?.RefreshOutfitTree();
                Logger.Debug("Refreshed vehicle and outfit trees after station assignment change");
            }
        }

        /// <summary>
        /// Checks validation state and auto-closes panel if all issues are resolved
        /// </summary>
        private void CheckValidationState()
        {
            // Notify that validation properties may have changed
            OnPropertyChanged(nameof(HasAnyValidationIssues));
            OnPropertyChanged(nameof(ValidationErrorCount));
            OnPropertyChanged(nameof(ValidationButtonText));
            OnPropertyChanged(nameof(ValidationButtonIconPath));
            OnPropertyChanged(nameof(GenerateButtonIconPath));
            OnPropertyChanged(nameof(HasValidationErrors));
            OnPropertyChanged(nameof(HasValidationErrorsOnly));
            OnPropertyChanged(nameof(HasWarnings));
            OnPropertyChanged(nameof(HasAdvisories));

            // Auto-close validation panel if it's open and there are no more issues
            if (IsValidationPanelVisible && !HasAnyValidationIssues)
            {
                Logger.Info("All validation issues resolved - auto-closing validation panel");
                IsValidationPanelVisible = false;
            }
        }

        private void OnRequestFocusRanksTab(object? sender, EventArgs e)
        {
            // Switch to Ranks tab when Undo/Redo is triggered
            Logger.Info("Switching to Ranks tab for Undo/Redo operation");
            CurrentTabIndex = 0; // Ranks is the first tab
        }

        private void OnRanksViewModelStatusMessageChanged(object? sender, StatusMessageEventArgs e)
        {
            // Update status bar when RanksViewModel sends a status message with auto-reset
            SetTemporaryStatusMessage(e.Message);
        }

        /// <summary>
        /// Sets a temporary status message that automatically resets to "Ready" after 3 seconds
        /// </summary>
        private void SetTemporaryStatusMessage(string message, int durationSeconds = 3)
        {
            StatusMessage = message;
            Logger.Trace($"Status message set: {message} (auto-reset in {durationSeconds}s)");

            // Stop existing timer if running
            _statusResetTimer?.Stop();

            // Create new timer if needed
            if (_statusResetTimer == null)
            {
                _statusResetTimer = new DispatcherTimer
                {
                    Interval = System.TimeSpan.FromSeconds(durationSeconds)
                };
                _statusResetTimer.Tick += (s, e) =>
                {
                    StatusMessage = "Ready";
                    _statusResetTimer?.Stop();
                    Logger.Trace("Status message auto-reset to 'Ready'");
                };
            }
            else
            {
                _statusResetTimer.Interval = System.TimeSpan.FromSeconds(durationSeconds);
            }

            _statusResetTimer.Start();
        }
    }
}
