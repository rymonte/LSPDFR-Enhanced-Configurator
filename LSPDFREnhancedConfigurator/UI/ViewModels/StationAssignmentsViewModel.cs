using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using LSPDFREnhancedConfigurator.Commands;
using LSPDFREnhancedConfigurator.Commands.StationAssignments;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services;

namespace LSPDFREnhancedConfigurator.UI.ViewModels
{
    public class StationAssignmentsViewModel : ViewModelBase
    {
        private DataLoadingService? _dataService;
        private SelectionStateService? _selectionStateService;
        private List<RankHierarchy> _ranks = new List<RankHierarchy>();
        private RankHierarchy? _selectedRank;
        private RankHierarchy? _selectedCopyFromRank;
        private RankHierarchy? _selectedCopyToRank;
        private string _searchText = string.Empty;
        private string _removeButtonText = "Remove";
        private bool _removeButtonEnabled = false;
        private bool _isUpdatingFromService = false;

        // Command Pattern Undo/Redo Manager
        private readonly UndoRedoManager _undoRedoManager = new UndoRedoManager(maxStackSize: 50);

        public StationAssignmentsViewModel(DataLoadingService dataService)
        {
            _dataService = dataService;

            RankList = new ObservableCollection<RankHierarchy>();
            CopyFromRankList = new ObservableCollection<RankHierarchy>();
            CopyToRankList = new ObservableCollection<RankHierarchy>();
            AssignedStations = new ObservableCollection<StationAssignment>();
            AvailableStations = new ObservableCollection<Station>();
            AgencyFilters = new ObservableCollection<AgencyFilterItem>();
            SelectedAssignedStations = new ObservableCollection<StationAssignment>();
            SelectedAvailableStations = new ObservableCollection<Station>();

            // Subscribe to selection changes
            SelectedAssignedStations.CollectionChanged += (s, e) => OnAssignedStationsSelectionChanged();
            SelectedAvailableStations.CollectionChanged += (s, e) => OnAvailableStationsSelectionChanged();

            // Subscribe to UndoRedoManager events
            _undoRedoManager.StacksChanged += (s, e) => UpdateCommandStates();

            // Commands
            AddStationsCommand = new RelayCommand(OnAddStations, CanAddStations);
            AddAllStationsCommand = new RelayCommand(OnAddAllStations, CanAddAllStations);
            RemoveStationsCommand = new RelayCommand(OnRemoveStations, CanRemoveStations);
            RemoveAllStationsCommand = new RelayCommand(OnRemoveAllStations, CanRemoveAllStations);
            CopyFromRankCommand = new RelayCommand(OnCopyFromRank, CanCopyFromRank);
            CopyStationsCommand = new RelayCommand(OnCopyStations, CanCopyStations);
            CopyStationsToRankCommand = new RelayCommand(OnCopyStationsToRank, CanCopyStationsToRank);
            ClearFiltersCommand = new RelayCommand(OnClearFilters);
            UndoCommand = new RelayCommand(OnUndo, CanUndo);
            RedoCommand = new RelayCommand(OnRedo, CanRedo);

            // Load agency filters
            LoadAgencies();
        }

        #region Properties

        public ObservableCollection<RankHierarchy> RankList { get; }
        public ObservableCollection<RankHierarchy> CopyFromRankList { get; }
        public ObservableCollection<RankHierarchy> CopyToRankList { get; }
        public ObservableCollection<StationAssignment> AssignedStations { get; }
        public ObservableCollection<Station> AvailableStations { get; }
        public ObservableCollection<AgencyFilterItem> AgencyFilters { get; }
        public ObservableCollection<StationAssignment> SelectedAssignedStations { get; }
        public ObservableCollection<Station> SelectedAvailableStations { get; }

        public RankHierarchy? SelectedRank
        {
            get => _selectedRank;
            set
            {
                if (SetProperty(ref _selectedRank, value))
                {
                    OnRankChanged();
                    UpdateCopyFromRankList();
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
                    ((RelayCommand)CopyStationsCommand).RaiseCanExecuteChanged();
                    OnPropertyChanged(nameof(ShowCopyFromWarning));
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
                    ((RelayCommand)CopyStationsToRankCommand).RaiseCanExecuteChanged();
                    OnPropertyChanged(nameof(ShowCopyToWarning));
                }
            }
        }

        public bool ShowCopyFromWarning => SelectedCopyFromRank != null;
        public bool ShowCopyToWarning => SelectedCopyToRank != null;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterAvailableStations();
                }
            }
        }

        public string RemoveButtonText
        {
            get => _removeButtonText;
            set => SetProperty(ref _removeButtonText, value);
        }

        public bool RemoveButtonEnabled
        {
            get => _removeButtonEnabled;
            set => SetProperty(ref _removeButtonEnabled, value);
        }

        #endregion

        #region Commands

        public ICommand AddStationsCommand { get; }
        public ICommand AddAllStationsCommand { get; }
        public ICommand RemoveStationsCommand { get; }
        public ICommand RemoveAllStationsCommand { get; }
        public ICommand CopyFromRankCommand { get; }
        public ICommand CopyStationsCommand { get; }
        public ICommand CopyStationsToRankCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        #endregion

        #region Command Handlers

        private bool CanAddStations()
        {
            return SelectedRank != null && SelectedAvailableStations.Count > 0;
        }

        private void OnAddStations()
        {
            if (SelectedRank== null || SelectedAvailableStations.Count == 0) return;

            var stationNames = string.Join(", ", SelectedAvailableStations.Select(s => s.Name));
            Logger.Info($"[USER] Adding {SelectedAvailableStations.Count} station(s) to rank '{SelectedRank.Name}': {stationNames}");

            // Create list of assignments to add (excluding already assigned stations)
            var assignmentsToAdd = new List<StationAssignment>();
            foreach (var station in SelectedAvailableStations.ToList())
            {
                // Check if station is already assigned
                if (!SelectedRank.Stations.Any(s => s.StationName == station.Name))
                {
                    var assignment = new StationAssignment(station.Name, new List<string>(), 1)
                    {
                        StationReference = station
                    };
                    assignmentsToAdd.Add(assignment);
                }
            }

            if (assignmentsToAdd.Count > 0)
            {
                // Create and execute command
                var command = new BulkAddStationAssignmentsCommand(
                    SelectedRank,
                    assignmentsToAdd,
                    LoadStationsForSelectedRank,
                    OnStationsChanged
                );

                _undoRedoManager.ExecuteCommand(command);
            }
        }

        private bool CanAddAllStations()
        {
            return SelectedRank != null && AvailableStations.Count > 0;
        }

        private void OnAddAllStations()
        {
            if (SelectedRank== null || AvailableStations.Count == 0) return;

            Logger.Info($"[USER] Adding all {AvailableStations.Count} visible station(s) to rank '{SelectedRank.Name}'");

            // Create list of assignments to add (excluding already assigned stations)
            var assignmentsToAdd = new List<StationAssignment>();
            foreach (var station in AvailableStations.ToList())
            {
                // Check if station is already assigned
                if (!SelectedRank.Stations.Any(s => s.StationName == station.Name))
                {
                    var assignment = new StationAssignment(station.Name, new List<string>(), 1)
                    {
                        StationReference = station
                    };
                    assignmentsToAdd.Add(assignment);
                }
            }

            if (assignmentsToAdd.Count > 0)
            {
                // Create and execute command
                var command = new AddAllStationAssignmentsCommand(
                    SelectedRank,
                    assignmentsToAdd,
                    LoadStationsForSelectedRank,
                    OnStationsChanged
                );

                _undoRedoManager.ExecuteCommand(command);
            }
        }

        private bool CanRemoveAllStations()
        {
            return SelectedRank != null && AssignedStations.Count > 0;
        }

        private void OnRemoveAllStations()
        {
            if (SelectedRank== null || AssignedStations.Count == 0) return;

            Logger.Info($"[USER] Removing all {AssignedStations.Count} station(s) from rank '{SelectedRank.Name}'");

            // Create and execute command
            var command = new RemoveAllStationAssignmentsCommand(
                SelectedRank,
                LoadStationsForSelectedRank,
                OnStationsChanged
            );

            _undoRedoManager.ExecuteCommand(command);
        }

        private bool CanRemoveStations()
        {
            return SelectedRank != null && SelectedAssignedStations.Count > 0;
        }

        private void OnRemoveStations()
        {
            if (SelectedRank== null || SelectedAssignedStations.Count == 0) return;

            var stationNames = string.Join(", ", SelectedAssignedStations.Select(s => s.StationName));
            Logger.Info($"[USER] Removing {SelectedAssignedStations.Count} station(s) from rank '{SelectedRank.Name}': {stationNames}");

            // Create and execute command
            var command = new BulkRemoveStationAssignmentsCommand(
                SelectedRank,
                SelectedAssignedStations.ToList(),
                LoadStationsForSelectedRank,
                OnStationsChanged
            );

            _undoRedoManager.ExecuteCommand(command);
        }

        private bool CanCopyFromRank()
        {
            return SelectedRank != null;
        }

        private async void OnCopyFromRank()
        {
            Logger.Info("[USER] Copy From Rank clicked (Stations)");

            if (SelectedRank == null) return;

            try
            {
                // Create dialog viewmodel
                var dialogViewModel = new CopyFromRankDialogViewModel
                {
                    Title = "Copy Stations From Rank",
                    Description = $"Select a rank to copy station assignments to '{SelectedRank.Name}'"
                };

                // Add all ranks except the current one
                foreach (var rank in _ranks.Where(r => r != SelectedRank))
                {
                    if (rank.IsParent && rank.PayBands.Count > 0)
                    {
                        foreach (var payBand in rank.PayBands)
                        {
                            dialogViewModel.AvailableRanks.Add(payBand);
                        }
                    }
                    else
                    {
                        dialogViewModel.AvailableRanks.Add(rank);
                    }
                }

                // Show dialog
                var dialog = new Views.CopyFromRankDialog
                {
                    DataContext = dialogViewModel
                };

                if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var result = await dialog.ShowDialog<bool>(desktop.MainWindow);

                    if (result && dialogViewModel.SelectedRank != null)
                    {
                        // Copy stations from source rank
                        var sourceRank = dialogViewModel.SelectedRank;
                        if (sourceRank != null && SelectedRank != null)
                        {
                            // Create and execute command
                            var command = new CopyStationAssignmentsFromRankCommand(
                                sourceRank,
                                SelectedRank,
                                LoadStationsForSelectedRank,
                                OnStationsChanged
                            );

                            _undoRedoManager.ExecuteCommand(command);
                            Logger.Info($"Copied stations from '{sourceRank.Name}' to '{SelectedRank.Name}'");
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error($"Error copying stations: {ex.Message}");
            }
        }

        private void OnClearFilters()
        {
            SearchText = string.Empty;

            foreach (var filter in AgencyFilters)
            {
                filter.IsChecked = false;
            }

            FilterAvailableStations();
            Logger.Info("Cleared all filters - showing all available stations");
        }

        private bool CanCopyStations()
        {
            return SelectedRank != null && SelectedCopyFromRank != null;
        }

        private void OnCopyStations()
        {
            if (SelectedRank== null || SelectedCopyFromRank== null)
                return;

            Logger.Info($"[USER] Copying stations from '{SelectedCopyFromRank.Name}' to '{SelectedRank.Name}' using dropdown");

            // Create and execute command
            var command = new CopyStationAssignmentsFromRankCommand(
                SelectedCopyFromRank,
                SelectedRank,
                LoadStationsForSelectedRank,
                OnStationsChanged
            );

            _undoRedoManager.ExecuteCommand(command);
            Logger.Info($"Copied stations from '{SelectedCopyFromRank.Name}' to '{SelectedRank.Name}'");
        }

        private bool CanCopyStationsToRank()
        {
            return SelectedRank != null && SelectedCopyToRank != null && SelectedRank != SelectedCopyToRank;
        }

        private void OnCopyStationsToRank()
        {
            if (SelectedRank== null || SelectedCopyToRank== null)
                return;

            Logger.Info($"[USER] Copying stations from '{SelectedRank.Name}' to '{SelectedCopyToRank.Name}'");

            // Create and execute command
            var command = new CopyStationAssignmentsToRankCommand(
                SelectedRank,
                SelectedCopyToRank,
                OnStationsChanged
            );

            _undoRedoManager.ExecuteCommand(command);
            Logger.Info($"Copied {SelectedRank.Stations.Count} station(s) from '{SelectedRank.Name}' to '{SelectedCopyToRank.Name}'");
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
                LoadStationsForSelectedRank();
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
                LoadStationsForSelectedRank();
            }
        }

        #endregion

        #region Helper Methods

        private void OnRankChanged()
        {
            if (SelectedRank != null)
            {
                Logger.Info($"[USER] Selected rank in Station Assignments: '{SelectedRank.Name}' ({SelectedRank.Stations.Count} stations assigned)");
            }
            else
            {
                Logger.Trace("[USER] Deselected rank in Station Assignments");
            }
            LoadStationsForSelectedRank();
        }

        private void LoadStationsForSelectedRank()
        {
            if (SelectedRank== null || _dataService == null)
            {
                AssignedStations.Clear();
                AvailableStations.Clear();
                return;
            }

            // Load assigned stations
            AssignedStations.Clear();
            foreach (var station in SelectedRank.Stations)
            {
                AssignedStations.Add(station);
            }

            // Update remove button state
            UpdateRemoveButtonState();

            // Load available stations (filtered)
            FilterAvailableStations();
        }

        public void FilterAvailableStations()
        {
            if (_dataService == null)
            {
                AvailableStations.Clear();
                return;
            }

            if (SelectedRank== null)
            {
                AvailableStations.Clear();
                return;
            }

            AvailableStations.Clear();

            // Get selected agencies
            var selectedAgencies = new HashSet<string>(
                AgencyFilters.Where(f => f.IsChecked).Select(f => f.ShortName),
                StringComparer.OrdinalIgnoreCase);

            var searchText = SearchText.ToLower();
            var assignedStationNames = SelectedRank.Stations.Select(s => s.StationName).ToHashSet();

            // Filter and sort stations
            var stationsToShow = new List<Station>();

            foreach (var station in _dataService.Stations)
            {
                // Skip if already assigned
                if (assignedStationNames.Contains(station.Name))
                    continue;

                // Apply agency filter (if NO filters selected, show ALL agencies)
                if (selectedAgencies.Count > 0)
                {
                    bool hasMatchingAgency = selectedAgencies.Contains(station.Agency);
                    bool isUnknown = string.IsNullOrWhiteSpace(station.Agency);
                    bool unknownFilterSelected = selectedAgencies.Contains("UNKNOWN");

                    if (!hasMatchingAgency && !(isUnknown && unknownFilterSelected))
                        continue;
                }

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchText) && !station.Name.ToLower().Contains(searchText))
                    continue;

                stationsToShow.Add(station);
            }

            // Sort alphabetically by station name
            stationsToShow.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

            // Add to observable collection
            foreach (var station in stationsToShow)
            {
                AvailableStations.Add(station);
            }
        }

        private void UpdateRemoveButtonState()
        {
            var count = SelectedAssignedStations.Count;

            if (count > 0)
            {
                RemoveButtonText = count > 1 ? $"Remove selected ({count})" : "Remove";
                RemoveButtonEnabled = true;
            }
            else
            {
                RemoveButtonText = "Remove";
                RemoveButtonEnabled = false;
            }
        }

        public void OnAssignedStationsSelectionChanged()
        {
            UpdateRemoveButtonState();
            UpdateCommandStates();
        }

        public void OnAvailableStationsSelectionChanged()
        {
            UpdateCommandStates();
        }

        public void OnAgencyFilterChanged()
        {
            FilterAvailableStations();
        }

        private void UpdateCommandStates()
        {
            ((RelayCommand)AddStationsCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RemoveStationsCommand).RaiseCanExecuteChanged();
            ((RelayCommand)CopyFromRankCommand).RaiseCanExecuteChanged();
            ((RelayCommand)CopyStationsCommand).RaiseCanExecuteChanged();
            ((RelayCommand)UndoCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RedoCommand).RaiseCanExecuteChanged();

            // Notify MainWindow that undo/redo states changed
            UndoRedoStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateCopyFromRankList()
        {
            CopyFromRankList.Clear();

            if (SelectedRank == null)
                return;

            // Add all ranks except the currently selected one
            foreach (var rank in _ranks)
            {
                if (rank.IsParent && rank.PayBands.Count > 0)
                {
                    // Add all pay bands except the selected one
                    foreach (var payBand in rank.PayBands)
                    {
                        if (payBand != SelectedRank)
                        {
                            CopyFromRankList.Add(payBand);
                        }
                    }
                }
                else if (rank != SelectedRank)
                {
                    // Add the rank itself
                    CopyFromRankList.Add(rank);
                }
            }
        }

        public event EventHandler DataChanged;
        public event EventHandler? UndoRedoStateChanged;

        private void OnStationsChanged()
        {
            // Notify parent that stations have changed
            // This would trigger XML regeneration in the main window
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Data Loading

        public void SetDataService(DataLoadingService dataService)
        {
            _dataService = dataService;
            LoadAgencies();
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
            CopyToRankList.Clear();

            foreach (var rank in ranks)
            {
                if (rank.IsParent && rank.PayBands.Count > 0)
                {
                    // Add all pay bands
                    foreach (var payBand in rank.PayBands)
                    {
                        RankList.Add(payBand);
                        CopyToRankList.Add(payBand);
                    }
                }
                else
                {
                    // Add the rank itself
                    RankList.Add(rank);
                    CopyToRankList.Add(rank);
                }
            }

            if (RankList.Count > 0)
            {
                SelectedRank = RankList[0];
            }
        }

        private void LoadAgencies()
        {
            if (_dataService == null) return;

            AgencyFilters.Clear();

            // Get all station agencies
            var stationAgencies = _dataService.Stations
                .Select(s => s.Agency.ToUpper())
                .ToHashSet();

            // Deduplicate agencies by ShortName, filter to only those with stations, and sort
            var uniqueAgencies = _dataService.Agencies
                .GroupBy(a => a.ShortName.ToUpper())
                .Select(g => g.First())
                .Where(a => stationAgencies.Contains(a.ShortName.ToUpper()))
                .OrderBy(a => a.ShortName)
                .ToList();

            foreach (var agency in uniqueAgencies)
            {
                var filterItem = new AgencyFilterItem(agency.ShortName, agency.Name, this);
                AgencyFilters.Add(filterItem);
            }

            // Add "Unknown Agency" filter if there are stations with empty agencies
            if (_dataService.Stations.Any(s => string.IsNullOrWhiteSpace(s.Agency)))
            {
                var unknownFilter = new AgencyFilterItem("UNKNOWN", "Unknown Agency", this);
                AgencyFilters.Add(unknownFilter);
            }

            Logger.Info($"Loaded {uniqueAgencies.Count} agencies with stations");
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

            LoadRanks(sampleRanks);
        }

        #endregion
    }

    /// <summary>
    /// Wrapper for agency filter checkbox items
    /// </summary>
    public class AgencyFilterItem : ViewModelBase
    {
        private bool _isChecked;
        private readonly StationAssignmentsViewModel _parent;

        public AgencyFilterItem(string shortName, string name, StationAssignmentsViewModel parent)
        {
            ShortName = shortName;
            Name = name;
            _parent = parent;
        }

        public string ShortName { get; }
        public string Name { get; }
        public string DisplayText => $"{ShortName} - {Name}";

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (SetProperty(ref _isChecked, value))
                {
                    _parent.OnAgencyFilterChanged();
                }
            }
        }
    }
}
