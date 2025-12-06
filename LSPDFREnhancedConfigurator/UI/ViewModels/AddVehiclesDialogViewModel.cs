using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services;

namespace LSPDFREnhancedConfigurator.UI.ViewModels
{
    public class AddVehiclesDialogViewModel : ViewModelBase
    {
        private readonly DataLoadingService _dataService;
        private readonly Models.Station? _contextStation;
        private List<Vehicle> _allVehicles = new List<Vehicle>();
        private List<AgencyFilterItem> _allAgencyFilters = new List<AgencyFilterItem>();
        private string _searchText = string.Empty;
        private string _agencySearchText = string.Empty;
        private string _statusText = "0 vehicles selected";

        public AddVehiclesDialogViewModel(DataLoadingService dataService, Models.Station? contextStation = null)
        {
            _dataService = dataService;
            _contextStation = contextStation;
            AgencyFilters = new ObservableCollection<AgencyFilterItem>();
            VehicleItems = new ObservableCollection<VehicleItemViewModel>();

            AddSelectedCommand = new RelayCommand(OnAddSelected, CanAddSelected);
            FilterByStationAgencyCommand = new RelayCommand(OnFilterByStationAgency, CanFilterByStationAgency);

            Logger.Debug($"AddVehiclesDialogViewModel constructor - DataService has {dataService.AllVehicles.Count} vehicles");
            LoadData();
            Logger.Debug($"After LoadData - VehicleItems has {VehicleItems.Count} items, AgencyFilters has {AgencyFilters.Count} filters");
        }

        #region Properties

        public ObservableCollection<AddVehiclesDialogViewModel.AgencyFilterItem> AgencyFilters { get; }
        public ObservableCollection<VehicleItemViewModel> VehicleItems { get; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterVehicles();
                }
            }
        }

        public string AgencySearchText
        {
            get => _agencySearchText;
            set
            {
                if (SetProperty(ref _agencySearchText, value))
                {
                    FilterAgencies();
                }
            }
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public List<Vehicle> SelectedVehicles { get; private set; } = new List<Vehicle>();

        public bool ShowStationAgencyFilter => _contextStation != null && !string.IsNullOrWhiteSpace(_contextStation.Agency);

        public string StationAgencyFilterButtonText
        {
            get
            {
                if (_contextStation == null || string.IsNullOrWhiteSpace(_contextStation.Agency))
                    return string.Empty;

                return $"Filter by Station Agency ({_contextStation.Agency.ToUpper()})";
            }
        }

        public string Title
        {
            get
            {
                if (_contextStation != null)
                    return $"Add Vehicles to {_contextStation.Name}";
                return "Add Vehicles";
            }
        }

        #endregion

        #region Commands

        public ICommand AddSelectedCommand { get; }
        public ICommand FilterByStationAgencyCommand { get; }

        #endregion

        #region Command Handlers

        private bool CanAddSelected()
        {
            return VehicleItems.Any(v => v.IsSelected);
        }

        private void OnAddSelected()
        {
            SelectedVehicles = VehicleItems.Where(v => v.IsSelected).Select(v => v.Vehicle).ToList();
        }

        private bool CanFilterByStationAgency()
        {
            return _contextStation != null && !string.IsNullOrWhiteSpace(_contextStation.Agency);
        }

        private void OnFilterByStationAgency()
        {
            if (_contextStation == null || string.IsNullOrWhiteSpace(_contextStation.Agency))
                return;

            Logger.Debug($"[USER] Filtering by station agency: {_contextStation.Agency}");

            // Find and check the matching agency filter
            var stationAgency = _contextStation.Agency.ToUpper();
            var matchingFilter = AgencyFilters.FirstOrDefault(f =>
                f.ShortName.Equals(stationAgency, StringComparison.OrdinalIgnoreCase));

            if (matchingFilter != null)
            {
                // Uncheck all filters first
                foreach (var filter in AgencyFilters)
                {
                    filter.IsChecked = false;
                }

                // Check only the station's agency
                matchingFilter.IsChecked = true;
                Logger.Debug($"Applied filter for agency: {stationAgency}");
            }
        }

        #endregion

        #region Helper Methods

        private void LoadData()
        {
            LoadAgencies();
            LoadVehicles();
        }

        private void LoadAgencies()
        {
            // Get all vehicle agencies (case-insensitive)
            var vehicleAgencies = _dataService.AllVehicles
                .SelectMany(v => v.Agencies.Select(a => a.ToUpper()))
                .ToHashSet();

            // Deduplicate and filter agencies that have vehicles, sort alphabetically
            var agenciesWithVehicles = _dataService.Agencies
                .GroupBy(a => a.ShortName.ToUpper())
                .Select(g => g.First())
                .Where(a => vehicleAgencies.Contains(a.ShortName.ToUpper()))
                .OrderBy(a => a.ShortName.ToUpper() == "UNKNOWN" ? "ZZZZZ" : a.ShortName)
                .ToList();

            foreach (var agency in agenciesWithVehicles)
            {
                var filterItem = new AgencyFilterItem(agency.ShortName, agency.Name, this);
                _allAgencyFilters.Add(filterItem);
            }

            // Add "Unknown Agency" filter if there are vehicles with empty agencies
            if (_dataService.AllVehicles.Any(v => v.Agencies == null || v.Agencies.Count == 0 || v.Agencies.All(string.IsNullOrWhiteSpace)))
            {
                var unknownFilter = new AgencyFilterItem("UNKNOWN", "Unknown Agency", this);
                _allAgencyFilters.Add(unknownFilter);
            }

            // Populate observable collection
            FilterAgencies();

            Logger.Debug($"AddVehiclesDialog loaded {agenciesWithVehicles.Count} agencies with vehicles");
        }

        public void FilterAgencies()
        {
            AgencyFilters.Clear();

            var searchText = AgencySearchText.ToLower();

            foreach (var agency in _allAgencyFilters)
            {
                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchText) &&
                    !agency.ShortName.ToLower().Contains(searchText) &&
                    !agency.FullName.ToLower().Contains(searchText))
                    continue;

                AgencyFilters.Add(agency);
            }
        }

        private void LoadVehicles()
        {
            _allVehicles = _dataService.AllVehicles.ToList();
            FilterVehicles();
        }

        public void FilterVehicles()
        {
            VehicleItems.Clear();

            // Get selected agencies
            var selectedAgencies = new HashSet<string>(
                AgencyFilters.Where(f => f.IsChecked).Select(f => f.ShortName),
                StringComparer.OrdinalIgnoreCase);

            var searchText = SearchText.ToLower();

            Logger.Debug($"FilterVehicles: {_allVehicles.Count} total vehicles, {selectedAgencies.Count} agencies selected, search='{searchText}'");

            var filteredVehicles = new List<Vehicle>();

            foreach (var vehicle in _allVehicles)
            {
                // Apply agency filter (if NO filters selected, show ALL vehicles)
                if (selectedAgencies.Count > 0)
                {
                    bool hasMatchingAgency = vehicle.Agencies.Any(a => selectedAgencies.Contains(a));
                    bool isUnknown = vehicle.Agencies == null || vehicle.Agencies.Count == 0 || vehicle.Agencies.All(string.IsNullOrWhiteSpace);
                    bool unknownFilterSelected = selectedAgencies.Contains("UNKNOWN");

                    if (!hasMatchingAgency && !(isUnknown && unknownFilterSelected))
                        continue;
                }

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchText) &&
                    !vehicle.DisplayName.ToLower().Contains(searchText) &&
                    !vehicle.Model.ToLower().Contains(searchText))
                    continue;

                filteredVehicles.Add(vehicle);
            }

            // Sort by display name
            filteredVehicles.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));

            foreach (var vehicle in filteredVehicles)
            {
                var item = new VehicleItemViewModel(vehicle);
                item.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(VehicleItemViewModel.IsSelected))
                    {
                        UpdateStatusText();
                        ((RelayCommand)AddSelectedCommand).RaiseCanExecuteChanged();
                    }
                };
                VehicleItems.Add(item);
            }

            Logger.Debug($"FilterVehicles: showing {filteredVehicles.Count} filtered vehicles");
            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            int selectedCount = VehicleItems.Count(v => v.IsSelected);
            int totalCount = VehicleItems.Count;
            StatusText = $"{selectedCount} of {totalCount} vehicle{(totalCount != 1 ? "s" : "")} selected";
        }

        #endregion

        #region Nested Class

        public class AgencyFilterItem : ViewModelBase
        {
            private bool _isChecked;
            private readonly AddVehiclesDialogViewModel _parent;

            public AgencyFilterItem(string shortName, string fullName, AddVehiclesDialogViewModel parent)
            {
                ShortName = shortName;
                FullName = fullName;
                _parent = parent;
            }

            public string ShortName { get; }
            public string FullName { get; }

            public bool IsChecked
            {
                get => _isChecked;
                set
                {
                    if (SetProperty(ref _isChecked, value))
                    {
                        _parent.FilterVehicles();
                    }
                }
            }
        }

        #endregion
    }
}
