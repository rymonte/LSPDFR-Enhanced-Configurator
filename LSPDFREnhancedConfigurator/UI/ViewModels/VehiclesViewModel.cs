using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using LSPDFREnhancedConfigurator.Commands;
using LSPDFREnhancedConfigurator.Commands.Vehicles;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services;

namespace LSPDFREnhancedConfigurator.UI.ViewModels
{
    public class VehiclesViewModel : ViewModelBase
    {
        private DataLoadingService? _dataService;
        private List<RankHierarchy> _ranks = new List<RankHierarchy>();
        private RankHierarchy? _selectedRank;
        private RankHierarchy? _selectedCopyFromRank;
        private RankHierarchy? _selectedCopyToRank;
        private VehicleTreeItemViewModel? _selectedTreeItem;
        private string _vehicleAdvisory = string.Empty;

        // Command Pattern Undo/Redo Manager
        private readonly UndoRedoManager _undoRedoManager = new UndoRedoManager(maxStackSize: 50);

        public VehiclesViewModel(DataLoadingService dataService, List<RankHierarchy>? loadedRanks)
        {
            _dataService = dataService;

            if (loadedRanks != null && loadedRanks.Count > 0)
            {
                _ranks = loadedRanks;
            }

            RankList = new ObservableCollection<RankHierarchy>();
            CopyFromRankList = new ObservableCollection<RankHierarchy>();
            CopyToRankList = new ObservableCollection<RankHierarchy>();
            VehicleTreeItems = new ObservableCollection<VehicleTreeItemViewModel>();

            // Subscribe to UndoRedoManager events
            _undoRedoManager.StacksChanged += (s, e) => UpdateCommandStates();

            // Commands
            AddVehiclesCommand = new RelayCommand(OnAddVehicles, CanAddVehicles);
            RemoveVehiclesCommand = new RelayCommand(OnRemoveVehicles, CanRemoveVehicles);
            RemoveAllVehiclesCommand = new RelayCommand(OnRemoveAllVehicles, CanRemoveAllVehicles);
            CopyFromRankCommand = new RelayCommand(OnCopyFromRank, CanCopyFromRank);
            CopyVehiclesCommand = new RelayCommand(OnCopyVehicles, CanCopyVehicles);
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
        public ObservableCollection<VehicleTreeItemViewModel> VehicleTreeItems { get; }

        public string VehicleAdvisory
        {
            get => _vehicleAdvisory;
            set
            {
                if (SetProperty(ref _vehicleAdvisory, value))
                {
                    RaiseDataChanged(); // Notify MainWindow of advisory change
                }
            }
        }

        public RankHierarchy? SelectedRank
        {
            get => _selectedRank;
            set
            {
                if (SetProperty(ref _selectedRank, value))
                {
                    OnRankChanged();
                    UpdateCommandStates();
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

        public VehicleTreeItemViewModel? SelectedTreeItem
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

        #endregion

        #region Commands

        public ICommand AddVehiclesCommand { get; }
        public ICommand RemoveVehiclesCommand { get; }
        public ICommand RemoveAllVehiclesCommand { get; }
        public ICommand CopyFromRankCommand { get; }
        public ICommand CopyVehiclesCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        #endregion

        #region Command Handlers

        private bool CanAddVehicles()
        {
            return SelectedRank != null && _dataService != null;
        }

        private async void OnAddVehicles()
        {
            Logger.Info("Add Vehicles clicked");

            if (SelectedRank == null || _dataService == null) return;

            try
            {
                // Determine if we're adding to a specific station
                Station? contextStation = null;
                if (SelectedTreeItem != null && SelectedTreeItem.IsStationNode && SelectedTreeItem.Station != null)
                {
                    contextStation = SelectedTreeItem.Station.StationReference;
                }

                // Create dialog viewmodel with station context
                var dialogViewModel = new AddVehiclesDialogViewModel(_dataService, contextStation);

                // Show dialog
                var dialog = new Views.AddVehiclesDialog
                {
                    DataContext = dialogViewModel
                };

                if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var result = await dialog.ShowDialog<bool>(desktop.MainWindow);

                    if (result && dialogViewModel.SelectedVehicles.Count > 0)
                    {
                        // Check if we're adding to a station or globally
                        if (contextStation != null && SelectedTreeItem != null && SelectedTreeItem.Station != null)
                        {
                            // Adding to a specific station
                            var station = SelectedTreeItem.Station;

                            // Filter out vehicles that already exist in this station
                            var vehiclesToAdd = dialogViewModel.SelectedVehicles
                                .Where(v => !station.VehicleOverrides.Any(existing => existing.Model == v.Model))
                                .ToList();

                            if (vehiclesToAdd.Count > 0)
                            {
                                foreach (var vehicle in vehiclesToAdd)
                                {
                                    station.VehicleOverrides.Add(vehicle);
                                }

                                LoadVehiclesForSelectedRank();
                                OnVehiclesChanged();

                                Logger.Info($"Added {vehiclesToAdd.Count} vehicle(s) to station '{station.StationName}'");
                            }
                        }
                        else
                        {
                            // Adding globally to rank
                            var vehiclesToAdd = dialogViewModel.SelectedVehicles
                                .Where(v => !SelectedRank.Vehicles.Any(existing => existing.Model == v.Model))
                                .ToList();

                            if (vehiclesToAdd.Count > 0)
                            {
                                // Create and execute command
                                var command = new BulkAddVehiclesCommand(
                                    SelectedRank,
                                    vehiclesToAdd,
                                    LoadVehiclesForSelectedRank,
                                    OnVehiclesChanged
                                );

                                _undoRedoManager.ExecuteCommand(command);
                                Logger.Info($"Added {vehiclesToAdd.Count} vehicle(s) to rank '{SelectedRank.Name}' (global)");
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error($"Error adding vehicles: {ex.Message}");
            }
        }

        private bool CanRemoveVehicles()
        {
            if (SelectedRank == null) return false;

            // Enable if either checkboxes are checked OR a vehicle node is selected
            var hasCheckedVehicles = GetCheckedVehicles().Count > 0;
            var hasSelectedVehicle = SelectedTreeItem != null && SelectedTreeItem.IsVehicleNode;

            return hasCheckedVehicles || hasSelectedVehicle;
        }

        private void OnRemoveVehicles()
        {
            if (SelectedRank== null) return;

            // Collect vehicles with their parent context
            var vehiclesToRemove = new List<(Vehicle vehicle, VehicleTreeItemViewModel? parent)>();

            // First, collect checked vehicles with their parents
            foreach (var item in VehicleTreeItems)
            {
                CollectCheckedVehiclesWithParent(item, vehiclesToRemove);
            }

            // If no checked vehicles but there's a selected vehicle node, use that
            if (vehiclesToRemove.Count == 0 && SelectedTreeItem != null && SelectedTreeItem.IsVehicleNode && SelectedTreeItem.Vehicle != null)
            {
                vehiclesToRemove.Add((SelectedTreeItem.Vehicle, SelectedTreeItem.Parent));
            }

            if (vehiclesToRemove.Count == 0) return;

            // Group vehicles by their parent context
            var groupedByParent = vehiclesToRemove.GroupBy(v => v.parent);

            foreach (var group in groupedByParent)
            {
                var parent = group.Key;
                var vehicles = group.Select(v => v.vehicle).ToList();
                var vehicleList = string.Join(", ", vehicles.Select(v => v.DisplayName ?? v.Model));

                if (parent != null && parent.IsStationNode && parent.Station != null)
                {
                    // Removing from a specific station
                    var station = parent.Station;

                    foreach (var vehicle in vehicles)
                    {
                        var toRemove = station.VehicleOverrides.FirstOrDefault(v => v.Model == vehicle.Model);
                        if (toRemove != null)
                        {
                            station.VehicleOverrides.Remove(toRemove);
                        }
                    }

                    Logger.Info($"[USER] Removed {vehicles.Count} vehicle(s) from station '{station.StationName}': {vehicleList}");
                }
                else
                {
                    // Removing from global rank
                    Logger.Info($"[USER] Removing {vehicles.Count} vehicle(s) from rank '{SelectedRank.Name}' (global): {vehicleList}");

                    // Create and execute command for global vehicles
                    var command = new BulkRemoveVehiclesCommand(
                        SelectedRank,
                        vehicles,
                        LoadVehiclesForSelectedRank,
                        OnVehiclesChanged
                    );

                    _undoRedoManager.ExecuteCommand(command);
                }
            }

            // Refresh UI if any station-specific removals occurred
            if (groupedByParent.Any(g => g.Key != null && g.Key.IsStationNode))
            {
                LoadVehiclesForSelectedRank();
                OnVehiclesChanged();
            }
        }

        private bool CanRemoveAllVehicles()
        {
            return SelectedRank != null && SelectedRank.Vehicles.Count > 0;
        }

        private void OnRemoveAllVehicles()
        {
            if (SelectedRank == null || SelectedRank.Vehicles.Count == 0) return;

            Logger.Info($"[USER] Removing all {SelectedRank.Vehicles.Count} vehicle(s) from rank '{SelectedRank.Name}'");

            // Create and execute command
            var command = new RemoveAllVehiclesCommand(
                SelectedRank,
                LoadVehiclesForSelectedRank,
                OnVehiclesChanged
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

            // Count all vehicles: global + station-specific overrides
            var allVehicles = new System.Collections.Generic.HashSet<string>(SelectedCopyFromRank.Vehicles.Select(v => v.Model), System.StringComparer.OrdinalIgnoreCase);
            foreach (var station in SelectedCopyFromRank.Stations)
            {
                foreach (var vehicle in station.VehicleOverrides)
                {
                    allVehicles.Add(vehicle.Model);
                }
            }
            var vehicleCount = allVehicles.Count;
            Logger.Info($"[USER] Attempting to copy {vehicleCount} vehicle(s) from '{SelectedCopyFromRank.Name}' to '{SelectedRank.Name}'");

            // Show confirmation dialog
            var dialog = new Avalonia.Controls.Window
            {
                Title = "Confirm Copy Vehicles",
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
                                    Text = "Copy Vehicles from Rank?",
                                    FontSize = 18,
                                    FontWeight = Avalonia.Media.FontWeight.Bold,
                                    Foreground = Avalonia.Media.Brushes.Cyan,
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                }
                            }
                        },
                        new Avalonia.Controls.TextBlock
                        {
                            Text = $"Are you sure you want to copy {vehicleCount} vehicle(s) from '{SelectedCopyFromRank.Name}' to '{SelectedRank.Name}'?\n\nThis action can be undone using the Undo button.",
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

            Logger.Info($"[USER] User confirmed - copying {vehicleCount} vehicle(s) from '{SelectedCopyFromRank.Name}' to '{SelectedRank.Name}'");

            // Create and execute command
            var command = new CopyVehiclesFromRankCommand(
                SelectedCopyFromRank,
                SelectedRank,
                LoadVehiclesForSelectedRank,
                OnVehiclesChanged
            );

            _undoRedoManager.ExecuteCommand(command);
            Logger.Debug($"Copy from rank command executed");
        }

        private bool CanCopyVehicles()
        {
            return SelectedRank != null && SelectedCopyToRank != null && SelectedRank != SelectedCopyToRank;
        }

        private async void OnCopyVehicles()
        {
            if (SelectedRank== null || SelectedCopyToRank== null) return;

            // Count all vehicles: global + station-specific overrides
            var allVehicles = new System.Collections.Generic.HashSet<string>(SelectedRank.Vehicles.Select(v => v.Model), System.StringComparer.OrdinalIgnoreCase);
            foreach (var station in SelectedRank.Stations)
            {
                foreach (var vehicle in station.VehicleOverrides)
                {
                    allVehicles.Add(vehicle.Model);
                }
            }
            var vehicleCount = allVehicles.Count;
            Logger.Info($"[USER] Attempting to copy {vehicleCount} vehicle(s) from '{SelectedRank.Name}' to '{SelectedCopyToRank.Name}'");

            // Show confirmation dialog
            var dialog = new Avalonia.Controls.Window
            {
                Title = "Confirm Copy Vehicles",
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
                                    Text = "Copy Vehicles to Rank?",
                                    FontSize = 18,
                                    FontWeight = Avalonia.Media.FontWeight.Bold,
                                    Foreground = Avalonia.Media.Brushes.Cyan,
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                }
                            }
                        },
                        new Avalonia.Controls.TextBlock
                        {
                            Text = $"Are you sure you want to copy {vehicleCount} vehicle(s) from '{SelectedRank.Name}' to '{SelectedCopyToRank.Name}'?\n\nThis action can be undone using the Undo button.",
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
                Logger.Info("[OnCopyVehicles] User cancelled copy operation");
                return;
            }

            Logger.Info($"[USER] User confirmed - copying {vehicleCount} vehicle(s) from '{SelectedRank.Name}' to '{SelectedCopyToRank.Name}'");

            // Create and execute command
            var command = new CopyVehiclesToRankCommand(
                SelectedRank,
                SelectedCopyToRank,
                OnVehiclesChanged
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
                LoadVehiclesForSelectedRank(); // Refresh UI after undo
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
                LoadVehiclesForSelectedRank(); // Refresh UI after redo
            }
        }

        #endregion

        #region Helper Methods

        private void OnRankChanged()
        {
            if (SelectedRank != null)
            {
                Logger.Info($"[USER] Selected rank in Vehicles: '{SelectedRank.Name}' ({SelectedRank.Vehicles.Count} vehicles assigned)");
            }
            else
            {
                Logger.Trace("[USER] Deselected rank in Vehicles");
            }
            LoadVehiclesForSelectedRank();
            CheckAdvisories();
        }

        private void LoadVehiclesForSelectedRank()
        {
            if (SelectedRank== null)
            {
                VehicleTreeItems.Clear();
                return;
            }

            VehicleTreeItems.Clear();

            // Global vehicles node
            var globalNode = new VehicleTreeItemViewModel("All Stations (Global Vehicles)", checkedChangedCallback: OnTreeItemCheckedChanged);

            foreach (var vehicle in SelectedRank.Vehicles)
            {
                var agencyDisplay = vehicle.Agencies.FirstOrDefault(a => !string.IsNullOrWhiteSpace(a)) ?? "UNKNOWN";
                var vehicleText = $"[{agencyDisplay.ToUpper()}] {vehicle.DisplayName} ({vehicle.Model})";
                var vehicleNode = new VehicleTreeItemViewModel(vehicleText, vehicle, parent: globalNode, checkedChangedCallback: OnTreeItemCheckedChanged);

                // Validate vehicle against reference data
                if (_dataService != null)
                {
                    var vehicleExistsInRefData = _dataService.AllVehicles.Any(v => v.Model.Equals(vehicle.Model, StringComparison.OrdinalIgnoreCase));
                    if (!vehicleExistsInRefData)
                    {
                        vehicleNode.UpdateValidationState(RankValidationSeverity.Error);
                    }
                }

                globalNode.Children.Add(vehicleNode);
            }

            VehicleTreeItems.Add(globalNode);

            // Station-specific nodes
            foreach (var station in SelectedRank.Stations)
            {
                var agency = station.StationReference?.Agency ?? "UNKNOWN";
                var stationText = $"[{agency.ToUpper()}] {station.StationName}";
                var stationNode = new VehicleTreeItemViewModel(stationText, station: station, checkedChangedCallback: OnTreeItemCheckedChanged);

                // Add station-specific vehicle overrides
                foreach (var vehicle in station.VehicleOverrides)
                {
                    var agencyDisplay = vehicle.Agencies.FirstOrDefault(a => !string.IsNullOrWhiteSpace(a)) ?? "UNKNOWN";
                    var vehicleText = $"[{agencyDisplay.ToUpper()}] {vehicle.DisplayName} ({vehicle.Model})";
                    var vehicleNode = new VehicleTreeItemViewModel(vehicleText, vehicle, parent: stationNode, checkedChangedCallback: OnTreeItemCheckedChanged);

                    // Validate vehicle against reference data
                    if (_dataService != null)
                    {
                        var vehicleExistsInRefData = _dataService.AllVehicles.Any(v => v.Model.Equals(vehicle.Model, StringComparison.OrdinalIgnoreCase));
                        if (!vehicleExistsInRefData)
                        {
                            vehicleNode.UpdateValidationState(RankValidationSeverity.Error);
                        }
                    }

                    stationNode.Children.Add(vehicleNode);
                }

                VehicleTreeItems.Add(stationNode);
            }

            // Update command states to reflect the current vehicle count
            UpdateCommandStates();
        }

        private List<Vehicle> GetCheckedVehicles()
        {
            var checkedVehicles = new List<Vehicle>();

            foreach (var item in VehicleTreeItems)
            {
                CollectCheckedVehicles(item, checkedVehicles);
            }

            return checkedVehicles;
        }

        private void CollectCheckedVehicles(VehicleTreeItemViewModel item, List<Vehicle> checkedVehicles)
        {
            if (item.IsChecked && item.Vehicle != null)
            {
                checkedVehicles.Add(item.Vehicle);
            }

            foreach (var child in item.Children)
            {
                CollectCheckedVehicles(child, checkedVehicles);
            }
        }

        private void CollectCheckedVehiclesWithParent(VehicleTreeItemViewModel item, List<(Vehicle vehicle, VehicleTreeItemViewModel? parent)> checkedVehicles)
        {
            if (item.IsChecked && item.Vehicle != null)
            {
                checkedVehicles.Add((item.Vehicle, item.Parent));
            }

            foreach (var child in item.Children)
            {
                CollectCheckedVehiclesWithParent(child, checkedVehicles);
            }
        }

        public void OnTreeItemCheckedChanged()
        {
            UpdateCommandStates();
        }

        private void UpdateCommandStates()
        {
            ((RelayCommand)AddVehiclesCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RemoveVehiclesCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RemoveAllVehiclesCommand).RaiseCanExecuteChanged();
            ((RelayCommand)CopyFromRankCommand).RaiseCanExecuteChanged();
            ((RelayCommand)CopyVehiclesCommand).RaiseCanExecuteChanged();
            ((RelayCommand)UndoCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RedoCommand).RaiseCanExecuteChanged();

            // Notify MainWindow that undo/redo states changed
            UndoRedoStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler DataChanged;
        public event EventHandler? UndoRedoStateChanged;

        private void OnVehiclesChanged()
        {
            // Notify parent that vehicles have changed
            // This would trigger XML regeneration in the main window
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void CheckAdvisories()
        {
            VehicleAdvisory = string.Empty;

            if (SelectedRank == null) return;

            // Find previous rank for comparison
            int rankIndex = _ranks.IndexOf(SelectedRank);
            if (rankIndex <= 0) return; // No previous rank

            var previousRank = _ranks[rankIndex - 1];

            // Get all vehicles from previous rank (global + station-specific)
            var previousGlobalVehicles = previousRank.Vehicles.Select(v => v.Model).ToHashSet();
            var previousStationVehicles = previousRank.Stations
                .SelectMany(s => s.VehicleOverrides.Select(v => v.Model))
                .ToHashSet();
            var allPreviousVehicles = previousGlobalVehicles.Union(previousStationVehicles).ToHashSet();

            // Get all vehicles from current rank (global + station-specific)
            var currentGlobalVehicles = SelectedRank.Vehicles.Select(v => v.Model).ToHashSet();
            var currentStationVehicles = SelectedRank.Stations
                .SelectMany(s => s.VehicleOverrides.Select(v => v.Model))
                .ToHashSet();
            var allCurrentVehicles = currentGlobalVehicles.Union(currentStationVehicles).ToHashSet();

            // Find missing vehicles
            var missingVehicles = allPreviousVehicles.Except(allCurrentVehicles).ToList();

            if (missingVehicles.Count > 0)
            {
                var vehicleList = string.Join(", ", missingVehicles.Take(3));
                if (missingVehicles.Count > 3)
                    vehicleList += $" (+{missingVehicles.Count - 3} more)";
                VehicleAdvisory = $"{missingVehicles.Count} vehicle(s) from previous rank not present: {vehicleList}";
            }
        }

        #endregion

        #region Data Loading

        public void SetDataService(DataLoadingService dataService)
        {
            _dataService = dataService;
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

            // Add sample vehicles
            sampleRanks[0].Vehicles.Add(new Vehicle("police", "2011 Ford Crown Vic", "lspd"));
            sampleRanks[0].Vehicles.Add(new Vehicle("police2", "2013 Ford Explorer", "lspd"));

            LoadRanks(sampleRanks);
        }

        #endregion
    }
}
