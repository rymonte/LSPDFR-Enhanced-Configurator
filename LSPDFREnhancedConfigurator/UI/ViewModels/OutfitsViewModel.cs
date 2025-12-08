using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using LSPDFREnhancedConfigurator.Commands;
using LSPDFREnhancedConfigurator.Commands.Outfits;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services;
using LSPDFREnhancedConfigurator.Services.Validation;
using LSPDFREnhancedConfigurator.Services.Validation.Models;

// Use the unified ValidationSeverity from the validation service
using RankValidationSeverity = LSPDFREnhancedConfigurator.Services.Validation.ValidationSeverity;

namespace LSPDFREnhancedConfigurator.UI.ViewModels
{
    public class OutfitsViewModel : ViewModelBase
    {
        private DataLoadingService? _dataService;
        private ValidationService? _validationService;
        private SelectionStateService? _selectionStateService;
        private List<RankHierarchy> _ranks = new List<RankHierarchy>();
        private RankHierarchy? _selectedRank;
        private RankHierarchy? _selectedCopyFromRank;
        private RankHierarchy? _selectedCopyToRank;
        private OutfitTreeItemViewModel? _selectedTreeItem;
        private string _outfitAdvisory = string.Empty;
        private bool _isUpdatingFromService = false;

        // Command Pattern Undo/Redo Manager
        private readonly UndoRedoManager _undoRedoManager = new UndoRedoManager(maxStackSize: 50);

        public OutfitsViewModel(DataLoadingService dataService, List<RankHierarchy>? loadedRanks)
        {
            _dataService = dataService;
            _validationService = new ValidationService(dataService);

            if (loadedRanks != null && loadedRanks.Count > 0)
            {
                _ranks = loadedRanks;
            }

            RankList = new ObservableCollection<RankHierarchy>();
            CopyFromRankList = new ObservableCollection<RankHierarchy>();
            CopyToRankList = new ObservableCollection<RankHierarchy>();
            OutfitTreeItems = new ObservableCollection<OutfitTreeItemViewModel>();

            // Subscribe to UndoRedoManager events
            _undoRedoManager.StacksChanged += (s, e) => UpdateCommandStates();

            // Commands
            AddOutfitsCommand = new RelayCommand(OnAddOutfits, CanAddOutfits);
            RemoveOutfitsCommand = new RelayCommand(OnRemoveOutfits, CanRemoveOutfits);
            RemoveAllOutfitsCommand = new RelayCommand(OnRemoveAllOutfits, CanRemoveAllOutfits);
            CopyFromRankCommand = new RelayCommand(OnCopyFromRank, CanCopyFromRank);
            CopyOutfitsCommand = new RelayCommand(OnCopyOutfits, CanCopyOutfits);
            UndoCommand = new RelayCommand(OnUndo, CanUndo);
            RedoCommand = new RelayCommand(OnRedo, CanRedo);

            // Load ranks into list
            if (_ranks.Count > 0)
            {
                foreach (var rank in _ranks)
                {
                    RankList.Add(rank);
                }
                SelectedRank = RankList.FirstOrDefault();
            }
        }

        #region Properties

        public ObservableCollection<RankHierarchy> RankList { get; }
        public ObservableCollection<RankHierarchy> CopyFromRankList { get; }
        public ObservableCollection<RankHierarchy> CopyToRankList { get; }
        public ObservableCollection<OutfitTreeItemViewModel> OutfitTreeItems { get; }

        public RankHierarchy? SelectedRank
        {
            get => _selectedRank;
            set
            {
                if (SetProperty(ref _selectedRank, value))
                {
                    OnRankChanged();
                    UpdateCommandStates();

                    // Notify selection service (unless we're updating from the service)
                    if (!_isUpdatingFromService && _selectionStateService != null)
                    {
                        _selectionStateService.SelectedRank = value;
                    }
                }
            }
        }

        public RankHierarchy? SelectedCopyFromRank
        {
            get => _selectedCopyFromRank;
            set
            {
                if (SetProperty(ref _selectedCopyFromRank, value))
                {
                    UpdateCommandStates();
                }
            }
        }

        public RankHierarchy? SelectedCopyToRank
        {
            get => _selectedCopyToRank;
            set
            {
                if (SetProperty(ref _selectedCopyToRank, value))
                {
                    UpdateCommandStates();
                }
            }
        }

        public OutfitTreeItemViewModel? SelectedTreeItem
        {
            get => _selectedTreeItem;
            set
            {
                if (SetProperty(ref _selectedTreeItem, value))
                {
                    UpdateCommandStates();
                }
            }
        }

        public string OutfitAdvisory
        {
            get => _outfitAdvisory;
            set
            {
                if (SetProperty(ref _outfitAdvisory, value))
                {
                    RaiseDataChanged(); // Notify MainWindow of advisory change
                }
            }
        }

        #endregion

        #region Commands

        public ICommand AddOutfitsCommand { get; }
        public ICommand RemoveOutfitsCommand { get; }
        public ICommand RemoveAllOutfitsCommand { get; }
        public ICommand CopyFromRankCommand { get; }
        public ICommand CopyOutfitsCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        #endregion

        #region Command Handlers

        private bool CanAddOutfits()
        {
            return SelectedRank != null && _dataService != null;
        }

        private async void OnAddOutfits()
        {
            Logger.Info("Add Outfits clicked");

            if (SelectedRank== null || _dataService == null) return;

            try
            {
                // Determine if we're adding to a specific station
                Station? contextStation = null;
                if (SelectedTreeItem != null && SelectedTreeItem.IsStationNode && SelectedTreeItem.Station != null)
                {
                    contextStation = SelectedTreeItem.Station.StationReference;
                }

                // Create dialog viewmodel with station context
                var dialogViewModel = new AddOutfitsDialogViewModel(_dataService, contextStation);

                // Show dialog
                var dialog = new Views.AddOutfitsDialog
                {
                    DataContext = dialogViewModel
                };

                if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var result = await dialog.ShowDialog<bool>(desktop.MainWindow);

                    if (result && dialogViewModel.SelectedOutfits.Count > 0)
                    {
                        // Check if we're adding to a station or globally
                        if (contextStation != null && SelectedTreeItem != null && SelectedTreeItem.Station != null)
                        {
                            // Adding to a specific station
                            var station = SelectedTreeItem.Station;

                            // Filter out outfits that already exist in this station
                            var outfitsToAdd = dialogViewModel.SelectedOutfits
                                .Where(o => !station.OutfitOverrides.Contains(o))
                                .ToList();

                            if (outfitsToAdd.Count > 0)
                            {
                                foreach (var outfit in outfitsToAdd)
                                {
                                    station.OutfitOverrides.Add(outfit);
                                }

                                LoadOutfitsForSelectedRank();
                                OnOutfitsChanged();

                                Logger.Info($"Added {outfitsToAdd.Count} outfit(s) to station '{station.StationName}'");
                            }
                        }
                        else
                        {
                            // Adding globally to rank
                            var outfitsToAdd = dialogViewModel.SelectedOutfits
                                .Where(o => !SelectedRank.Outfits.Contains(o))
                                .ToList();

                            if (outfitsToAdd.Count > 0)
                            {
                                // Create and execute command
                                var command = new BulkAddOutfitsCommand(
                                    SelectedRank,
                                    outfitsToAdd,
                                    LoadOutfitsForSelectedRank,
                                    OnOutfitsChanged
                                );

                                _undoRedoManager.ExecuteCommand(command);
                                Logger.Info($"Added {outfitsToAdd.Count} outfit(s) to rank '{SelectedRank.Name}' (global)");
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error($"Error adding outfits: {ex.Message}");
            }
        }

        private bool CanRemoveOutfits()
        {
            if (SelectedRank == null) return false;

            // Enable if either checkboxes are checked OR an outfit node is selected
            var hasCheckedOutfits = GetCheckedOutfits().Count > 0;
            var hasSelectedOutfit = SelectedTreeItem != null && SelectedTreeItem.IsOutfitNode;

            return hasCheckedOutfits || hasSelectedOutfit;
        }

        private void OnRemoveOutfits()
        {
            if (SelectedRank== null) return;

            // Collect outfits with their parent context
            var outfitsToRemove = new List<(string outfit, OutfitTreeItemViewModel? parent)>();

            // First, collect checked outfits with their parents
            foreach (var item in OutfitTreeItems)
            {
                CollectCheckedOutfitsWithParent(item, outfitsToRemove);
            }

            // If no checked outfits but there's a selected outfit node, use that
            if (outfitsToRemove.Count == 0 && SelectedTreeItem != null && SelectedTreeItem.IsOutfitNode && !string.IsNullOrEmpty(SelectedTreeItem.OutfitName))
            {
                outfitsToRemove.Add((SelectedTreeItem.OutfitName, SelectedTreeItem.Parent));
            }

            if (outfitsToRemove.Count == 0) return;

            // Group outfits by their parent context
            var groupedByParent = outfitsToRemove.GroupBy(o => o.parent);

            foreach (var group in groupedByParent)
            {
                var parent = group.Key;
                var outfits = group.Select(o => o.outfit).ToList();
                var outfitList = string.Join(", ", outfits);

                if (parent != null && parent.IsStationNode && parent.Station != null)
                {
                    // Removing from a specific station
                    var station = parent.Station;

                    foreach (var outfit in outfits)
                    {
                        station.OutfitOverrides.Remove(outfit);
                    }

                    Logger.Info($"[USER] Removed {outfits.Count} outfit(s) from station '{station.StationName}': {outfitList}");
                }
                else
                {
                    // Removing from global rank
                    Logger.Info($"[USER] Removing {outfits.Count} outfit(s) from rank '{SelectedRank.Name}' (global): {outfitList}");

                    // Create and execute command for global outfits
                    var command = new BulkRemoveOutfitsCommand(
                        SelectedRank,
                        outfits,
                        LoadOutfitsForSelectedRank,
                        OnOutfitsChanged
                    );

                    _undoRedoManager.ExecuteCommand(command);
                }
            }

            // Refresh UI if any station-specific removals occurred
            if (groupedByParent.Any(g => g.Key != null && g.Key.IsStationNode))
            {
                LoadOutfitsForSelectedRank();
                OnOutfitsChanged();
            }
        }

        private bool CanRemoveAllOutfits()
        {
            return SelectedRank != null && SelectedRank.Outfits.Count > 0;
        }

        private void OnRemoveAllOutfits()
        {
            if (SelectedRank == null || SelectedRank.Outfits.Count == 0) return;

            Logger.Info($"[USER] Removing all {SelectedRank.Outfits.Count} outfit(s) from rank '{SelectedRank.Name}'");

            // Create and execute command
            var command = new RemoveAllOutfitsCommand(
                SelectedRank,
                LoadOutfitsForSelectedRank,
                OnOutfitsChanged
            );

            _undoRedoManager.ExecuteCommand(command);
        }

        private bool CanCopyFromRank()
        {
            return SelectedRank != null && SelectedCopyFromRank != null && SelectedRank != SelectedCopyFromRank;
        }

        private async void OnCopyFromRank()
        {
            if (SelectedRank== null || SelectedCopyFromRank== null) return;

            // Count all outfits: global + station-specific overrides
            var allOutfits = new System.Collections.Generic.HashSet<string>(SelectedCopyFromRank.Outfits, System.StringComparer.OrdinalIgnoreCase);
            foreach (var station in SelectedCopyFromRank.Stations)
            {
                foreach (var outfit in station.OutfitOverrides)
                {
                    allOutfits.Add(outfit);
                }
            }
            var outfitCount = allOutfits.Count;
            Logger.Info($"[USER] Attempting to copy {outfitCount} outfit(s) from '{SelectedCopyFromRank.Name}' to '{SelectedRank.Name}'");

            // Show confirmation dialog
            var dialog = new Avalonia.Controls.Window
            {
                Title = "Confirm Copy Outfits",
                Width = 500,
                Height = 250,
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
                CanResize = false,
                Content = new Avalonia.Controls.StackPanel
                {
                    Margin = new Avalonia.Thickness(20),
                    Spacing = 15,
                    Children =
                    {
                        new Avalonia.Controls.StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            Spacing = 12,
                            Children =
                            {
                                new Avalonia.Controls.Image
                                {
                                    Source = new Avalonia.Media.Imaging.Bitmap(Avalonia.Platform.AssetLoader.Open(new Uri("avares://LSPDFREnhancedConfigurator/Resources/Icons/info-icon.png"))),
                                    Width = 32,
                                    Height = 32,
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                },
                                new Avalonia.Controls.TextBlock
                                {
                                    Text = "Copy Outfits from Rank?",
                                    FontSize = 18,
                                    FontWeight = Avalonia.Media.FontWeight.Bold,
                                    Foreground = Avalonia.Media.Brushes.Cyan,
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                }
                            }
                        },
                        new Avalonia.Controls.TextBlock
                        {
                            Text = $"Are you sure you want to copy {outfitCount} outfit(s) from '{SelectedCopyFromRank.Name}' to '{SelectedRank.Name}'?\n\nThis action can be undone using the Undo button.",
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap
                        },
                        new Avalonia.Controls.StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            Spacing = 12,
                            Margin = new Avalonia.Thickness(0, 15, 0, 0),
                            Children =
                            {
                                new Avalonia.Controls.Button
                                {
                                    Content = "Cancel",
                                    MinWidth = 120
                                },
                                new Avalonia.Controls.Button
                                {
                                    Content = "Copy",
                                    MinWidth = 120
                                }
                            }
                        }
                    }
                }
            };

            var buttonPanel = (Avalonia.Controls.StackPanel)((Avalonia.Controls.StackPanel)dialog.Content).Children[2];
            var cancelButton = (Avalonia.Controls.Button)buttonPanel.Children[0];
            var copyButton = (Avalonia.Controls.Button)buttonPanel.Children[1];

            bool result = false;
            cancelButton.Click += (s, e) => { result = false; dialog.Close(); };
            copyButton.Click += (s, e) => { result = true; dialog.Close(); };

            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                await dialog.ShowDialog(desktop.MainWindow);
            }

            if (!result)
            {
                Logger.Info("[OnCopyFromRank] User cancelled copy operation");
                return;
            }

            Logger.Info($"[USER] User confirmed - copying {outfitCount} outfit(s) from '{SelectedCopyFromRank.Name}' to '{SelectedRank.Name}'");

            // Create and execute command
            var command = new CopyOutfitsFromRankCommand(
                SelectedCopyFromRank,
                SelectedRank,
                LoadOutfitsForSelectedRank,
                OnOutfitsChanged
            );

            _undoRedoManager.ExecuteCommand(command);
            Logger.Debug($"Copy from rank command executed");
        }

        private bool CanCopyOutfits()
        {
            return SelectedRank != null && SelectedCopyToRank != null && SelectedRank != SelectedCopyToRank;
        }

        private async void OnCopyOutfits()
        {
            if (SelectedRank== null || SelectedCopyToRank== null) return;

            // Count all outfits: global + station-specific overrides
            var allOutfits = new System.Collections.Generic.HashSet<string>(SelectedRank.Outfits, System.StringComparer.OrdinalIgnoreCase);
            foreach (var station in SelectedRank.Stations)
            {
                foreach (var outfit in station.OutfitOverrides)
                {
                    allOutfits.Add(outfit);
                }
            }
            var outfitCount = allOutfits.Count;
            Logger.Info($"[USER] Attempting to copy {outfitCount} outfit(s) from '{SelectedRank.Name}' to '{SelectedCopyToRank.Name}'");

            // Show confirmation dialog
            var dialog = new Avalonia.Controls.Window
            {
                Title = "Confirm Copy Outfits",
                Width = 500,
                Height = 250,
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
                CanResize = false,
                Content = new Avalonia.Controls.StackPanel
                {
                    Margin = new Avalonia.Thickness(20),
                    Spacing = 15,
                    Children =
                    {
                        new Avalonia.Controls.StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            Spacing = 12,
                            Children =
                            {
                                new Avalonia.Controls.Image
                                {
                                    Source = new Avalonia.Media.Imaging.Bitmap(Avalonia.Platform.AssetLoader.Open(new Uri("avares://LSPDFREnhancedConfigurator/Resources/Icons/info-icon.png"))),
                                    Width = 32,
                                    Height = 32,
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                },
                                new Avalonia.Controls.TextBlock
                                {
                                    Text = "Copy Outfits to Rank?",
                                    FontSize = 18,
                                    FontWeight = Avalonia.Media.FontWeight.Bold,
                                    Foreground = Avalonia.Media.Brushes.Cyan,
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                }
                            }
                        },
                        new Avalonia.Controls.TextBlock
                        {
                            Text = $"Are you sure you want to copy {outfitCount} outfit(s) from '{SelectedRank.Name}' to '{SelectedCopyToRank.Name}'?\n\nThis action can be undone using the Undo button.",
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap
                        },
                        new Avalonia.Controls.StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            Spacing = 12,
                            Margin = new Avalonia.Thickness(0, 15, 0, 0),
                            Children =
                            {
                                new Avalonia.Controls.Button
                                {
                                    Content = "Cancel",
                                    MinWidth = 120
                                },
                                new Avalonia.Controls.Button
                                {
                                    Content = "Copy",
                                    MinWidth = 120
                                }
                            }
                        }
                    }
                }
            };

            var buttonPanel = (Avalonia.Controls.StackPanel)((Avalonia.Controls.StackPanel)dialog.Content).Children[2];
            var cancelButton = (Avalonia.Controls.Button)buttonPanel.Children[0];
            var copyButton = (Avalonia.Controls.Button)buttonPanel.Children[1];

            bool result = false;
            cancelButton.Click += (s, e) => { result = false; dialog.Close(); };
            copyButton.Click += (s, e) => { result = true; dialog.Close(); };

            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                await dialog.ShowDialog(desktop.MainWindow);
            }

            if (!result)
            {
                Logger.Info("[OnCopyOutfits] User cancelled copy operation");
                return;
            }

            Logger.Info($"[USER] User confirmed - copying {outfitCount} outfit(s) from '{SelectedRank.Name}' to '{SelectedCopyToRank.Name}'");

            // Create and execute command
            var command = new CopyOutfitsToRankCommand(
                SelectedRank,
                SelectedCopyToRank,
                OnOutfitsChanged
            );

            _undoRedoManager.ExecuteCommand(command);
        }

        private bool CanUndo()
        {
            return _undoRedoManager.CanUndo;
        }

        private void OnUndo()
        {
            if (_undoRedoManager.CanUndo)
            {
                Logger.Info($"[Undo] {_undoRedoManager.GetUndoDescription()}");
                _undoRedoManager.Undo();
                LoadOutfitsForSelectedRank();
            }
        }

        private bool CanRedo()
        {
            return _undoRedoManager.CanRedo;
        }

        private void OnRedo()
        {
            if (_undoRedoManager.CanRedo)
            {
                Logger.Info($"[Redo] {_undoRedoManager.GetRedoDescription()}");
                _undoRedoManager.Redo();
                LoadOutfitsForSelectedRank();
            }
        }

        #endregion

        #region Helper Methods

        private void OnRankChanged()
        {
            if (SelectedRank != null)
            {
                Logger.Info($"[USER] Selected rank in Outfits: '{SelectedRank.Name}' ({SelectedRank.Outfits.Count} outfits assigned)");
            }
            else
            {
                Logger.Trace("[USER] Deselected rank in Outfits");
            }
            LoadOutfitsForSelectedRank();
            CheckAdvisories();
        }

        private void LoadOutfitsForSelectedRank()
        {
            if (SelectedRank== null)
            {
                OutfitTreeItems.Clear();
                return;
            }

            OutfitTreeItems.Clear();

            // Global outfits node
            var globalNode = new OutfitTreeItemViewModel("All Stations (Global Outfits)", checkedChangedCallback: OnTreeItemCheckedChanged);

            foreach (var outfit in SelectedRank.Outfits)
            {
                var outfitNode = new OutfitTreeItemViewModel(outfit, outfit, parent: globalNode, checkedChangedCallback: OnTreeItemCheckedChanged);

                // Validate outfit against reference data
                if (_dataService != null)
                {
                    var outfitExistsInRefData = _dataService.OutfitVariations.Any(o => o.CombinedName.Equals(outfit, StringComparison.OrdinalIgnoreCase));
                    if (!outfitExistsInRefData)
                    {
                        outfitNode.UpdateValidationState(RankValidationSeverity.Error);
                    }
                }

                globalNode.Children.Add(outfitNode);
            }

            OutfitTreeItems.Add(globalNode);

            // Station-specific nodes
            foreach (var station in SelectedRank.Stations)
            {
                var agency = station.StationReference?.Agency ?? "UNKNOWN";
                var stationText = $"[{agency.ToUpper()}] {station.StationName}";
                var stationNode = new OutfitTreeItemViewModel(stationText, station: station, checkedChangedCallback: OnTreeItemCheckedChanged);

                // Add station-specific outfit overrides
                foreach (var outfit in station.OutfitOverrides)
                {
                    var outfitNode = new OutfitTreeItemViewModel(outfit, outfit, parent: stationNode, checkedChangedCallback: OnTreeItemCheckedChanged);

                    // Validate outfit against reference data
                    if (_dataService != null)
                    {
                        var outfitExistsInRefData = _dataService.OutfitVariations.Any(o => o.CombinedName.Equals(outfit, StringComparison.OrdinalIgnoreCase));
                        if (!outfitExistsInRefData)
                        {
                            outfitNode.UpdateValidationState(RankValidationSeverity.Error);
                        }
                    }

                    stationNode.Children.Add(outfitNode);
                }

                OutfitTreeItems.Add(stationNode);
            }

            // Update command states to reflect the current outfit count
            UpdateCommandStates();
        }

        private List<string> GetCheckedOutfits()
        {
            var checkedOutfits = new List<string>();

            foreach (var item in OutfitTreeItems)
            {
                CollectCheckedOutfits(item, checkedOutfits);
            }

            return checkedOutfits;
        }

        private void CollectCheckedOutfits(OutfitTreeItemViewModel item, List<string> checkedOutfits)
        {
            if (item.IsChecked && !string.IsNullOrEmpty(item.OutfitName))
            {
                checkedOutfits.Add(item.OutfitName);
            }

            foreach (var child in item.Children)
            {
                CollectCheckedOutfits(child, checkedOutfits);
            }
        }

        private void CollectCheckedOutfitsWithParent(OutfitTreeItemViewModel item, List<(string outfit, OutfitTreeItemViewModel? parent)> checkedOutfits)
        {
            if (item.IsChecked && !string.IsNullOrEmpty(item.OutfitName))
            {
                checkedOutfits.Add((item.OutfitName, item.Parent));
            }

            foreach (var child in item.Children)
            {
                CollectCheckedOutfitsWithParent(child, checkedOutfits);
            }
        }

        public void OnTreeItemCheckedChanged()
        {
            UpdateCommandStates();
        }

        private void UpdateCommandStates()
        {
            ((RelayCommand)AddOutfitsCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RemoveOutfitsCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RemoveAllOutfitsCommand).RaiseCanExecuteChanged();
            ((RelayCommand)CopyFromRankCommand).RaiseCanExecuteChanged();
            ((RelayCommand)CopyOutfitsCommand).RaiseCanExecuteChanged();
            ((RelayCommand)UndoCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RedoCommand).RaiseCanExecuteChanged();

            // Notify MainWindow that undo/redo states changed
            UndoRedoStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler DataChanged;
        public event EventHandler? UndoRedoStateChanged;

        private void OnOutfitsChanged()
        {
            // Notify parent that outfits have changed
            // This would trigger XML regeneration in the main window
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void CheckAdvisories()
        {
            OutfitAdvisory = string.Empty;

            if (SelectedRank == null || _validationService == null) return;

            // Use ValidationService to validate with advisory context
            var validationResult = _validationService.ValidateSingleRank(SelectedRank, _ranks, ValidationContext.Full);

            // Find advisory/warning issues related to outfits
            var outfitAdvisories = validationResult.Issues
                .Where(i => (i.Severity == ValidationSeverity.Advisory || i.Severity == ValidationSeverity.Warning) &&
                           i.RankId == SelectedRank.Id &&
                           i.Category == "Outfit")
                .ToList();

            if (outfitAdvisories.Any())
            {
                OutfitAdvisory = outfitAdvisories.First().Message;
            }
        }

        #endregion

        #region Data Loading

        public void SetDataService(DataLoadingService dataService)
        {
            _dataService = dataService;
        }

        public void SetSelectionStateService(SelectionStateService selectionStateService)
        {
            _selectionStateService = selectionStateService;

            // Subscribe to rank selection changes from the service
            _selectionStateService.RankSelectionChanged += OnSelectionStateServiceRankChanged;
        }

        private void OnSelectionStateServiceRankChanged(object? sender, RankSelectionChangedEventArgs e)
        {
            // Update local selection to match the service (prevent feedback loop)
            if (_selectedRank != e.SelectedRank)
            {
                _isUpdatingFromService = true;
                try
                {
                    SelectedRank = e.SelectedRank;
                }
                finally
                {
                    _isUpdatingFromService = false;
                }
            }
        }

        public void LoadRanks(List<RankHierarchy> ranks)
        {
            _ranks = ranks;
            RankList.Clear();
            CopyFromRankList.Clear();
            CopyToRankList.Clear();

            foreach (var rank in ranks)
            {
                if (rank.IsParent && rank.PayBands.Count > 0)
                {
                    // Add all pay bands
                    foreach (var payBand in rank.PayBands)
                    {
                        RankList.Add(payBand);
                        CopyFromRankList.Add(payBand);
                        CopyToRankList.Add(payBand);
                    }
                }
                else
                {
                    // Add the rank itself
                    RankList.Add(rank);
                    CopyFromRankList.Add(rank);
                    CopyToRankList.Add(rank);
                }
            }

            if (RankList.Count > 0)
            {
                SelectedRank = RankList[0];
            }
        }

        private void LoadSampleData()
        {
            // Sample ranks for testing
            var sampleRanks = new List<RankHierarchy>
            {
                new RankHierarchy("Rookie", 0, 30000),
                new RankHierarchy("Officer", 100, 40000),
                new RankHierarchy("Detective", 500, 50000)
            };

            // Add sample outfits
            sampleRanks[0].Outfits.Add("LSPD Uniform");
            sampleRanks[0].Outfits.Add("LSPD Patrol");

            LoadRanks(sampleRanks);
        }

        #endregion
    }
}
