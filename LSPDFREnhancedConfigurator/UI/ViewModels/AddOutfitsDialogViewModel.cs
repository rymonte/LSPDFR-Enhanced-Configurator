using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using LSPDFREnhancedConfigurator.Services;

namespace LSPDFREnhancedConfigurator.UI.ViewModels
{
    public class AddOutfitsDialogViewModel : ViewModelBase
    {
        private readonly DataLoadingService _dataService;
        private readonly Models.Station? _contextStation;
        private readonly List<string>? _existingOutfits;
        private List<Models.OutfitVariation> _allOutfits = new List<Models.OutfitVariation>();
        private string _searchText = string.Empty;
        private string _statusText = "0 outfits selected";
        private bool _isStrictSearch = false;

        public AddOutfitsDialogViewModel(DataLoadingService dataService, Models.Station? contextStation = null, List<string>? existingOutfits = null)
        {
            _dataService = dataService;
            _contextStation = contextStation;
            _existingOutfits = existingOutfits;
            OutfitItems = new ObservableCollection<OutfitItemViewModel>();

            AddSelectedCommand = new RelayCommand(OnAddSelected, CanAddSelected);

            Logger.Debug($"AddOutfitsDialogViewModel constructor - {existingOutfits?.Count ?? 0} existing outfits to exclude");
            LoadData();
        }

        #region Properties

        public ObservableCollection<OutfitItemViewModel> OutfitItems { get; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterOutfits();
                }
            }
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public bool IsStrictSearch
        {
            get => _isStrictSearch;
            set
            {
                if (SetProperty(ref _isStrictSearch, value))
                {
                    OnPropertyChanged(nameof(SearchModeText));
                    FilterOutfits();
                }
            }
        }

        public string SearchModeText => IsStrictSearch ? "Strict" : "Basic";

        public List<string> SelectedOutfits { get; private set; } = new List<string>();

        public string Title
        {
            get
            {
                if (_contextStation != null)
                    return $"Add Outfits to {_contextStation.Name}";
                return "Add Outfits";
            }
        }

        #endregion

        #region Commands

        public ICommand AddSelectedCommand { get; }

        #endregion

        #region Command Handlers

        private bool CanAddSelected()
        {
            return OutfitItems.Any(o => o.IsSelected);
        }

        private void OnAddSelected()
        {
            SelectedOutfits = OutfitItems.Where(o => o.IsSelected).Select(o => o.CombinedName).ToList();
        }

        #endregion

        #region Helper Methods

        private void LoadData()
        {
            LoadOutfits();
        }

        private void LoadOutfits()
        {
            // Load all outfits and filter out existing ones
            _allOutfits = _dataService.OutfitVariations.ToList();

            if (_existingOutfits != null && _existingOutfits.Count > 0)
            {
                // Create a set of existing outfit names for fast lookup
                var existingNames = new HashSet<string>(_existingOutfits, StringComparer.OrdinalIgnoreCase);
                _allOutfits = _allOutfits.Where(o => !existingNames.Contains(o.CombinedName)).ToList();
                Logger.Debug($"Filtered out {_existingOutfits.Count} existing outfits, {_allOutfits.Count} available");
            }

            FilterOutfits();
        }

        private void FilterOutfits()
        {
            // Preserve selection state before clearing
            var selectedCombinedNames = OutfitItems
                .Where(o => o.IsSelected)
                .Select(o => o.CombinedName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            OutfitItems.Clear();

            var searchText = SearchText.Trim();
            var filteredOutfits = new List<(Models.OutfitVariation outfit, int relevance)>();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // No search text, add all outfits with default relevance
                foreach (var outfit in _allOutfits)
                {
                    filteredOutfits.Add((outfit, 0));
                }
            }
            else if (IsStrictSearch)
            {
                // Strict mode - use word boundary matching
                var pattern = $@"\b{System.Text.RegularExpressions.Regex.Escape(searchText)}\b";
                var regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                foreach (var outfit in _allOutfits)
                {
                    if (regex.IsMatch(outfit.CombinedName))
                    {
                        filteredOutfits.Add((outfit, 0));
                    }
                }
            }
            else
            {
                // Basic mode - substring matching with relevance scoring
                var searchLower = searchText.ToLower();

                foreach (var outfit in _allOutfits)
                {
                    var combinedLower = outfit.CombinedName.ToLower();

                    if (combinedLower.Contains(searchLower))
                    {
                        // Calculate relevance score (lower is better)
                        int relevance = 100;

                        // Exact match = highest relevance
                        if (combinedLower.Equals(searchLower))
                            relevance = 0;
                        // Starts with search text = very high relevance
                        else if (combinedLower.StartsWith(searchLower))
                            relevance = 10;
                        // Word starts with search text = high relevance
                        else if (combinedLower.Contains(" " + searchLower) || combinedLower.Contains("." + searchLower))
                            relevance = 20;
                        // Contains as whole word = medium relevance
                        else if (System.Text.RegularExpressions.Regex.IsMatch(combinedLower, $@"\b{System.Text.RegularExpressions.Regex.Escape(searchLower)}\b"))
                            relevance = 30;
                        // Contains anywhere = lower relevance
                        else
                            relevance = 50;

                        // Adjust by position (earlier = better)
                        relevance += combinedLower.IndexOf(searchLower) / 10;

                        filteredOutfits.Add((outfit, relevance));
                    }
                }
            }

            // Sort by relevance first (if in basic mode with search), then by outfit name
            if (!string.IsNullOrWhiteSpace(searchText) && !IsStrictSearch)
            {
                filteredOutfits.Sort((a, b) =>
                {
                    // First sort by relevance
                    var relevanceCompare = a.relevance.CompareTo(b.relevance);
                    if (relevanceCompare != 0)
                        return relevanceCompare;

                    // Then by outfit name
                    var outfitNameA = a.outfit.ParentOutfit?.Name ?? a.outfit.Name;
                    var outfitNameB = b.outfit.ParentOutfit?.Name ?? b.outfit.Name;
                    var outfitCompare = string.Compare(outfitNameA, outfitNameB, StringComparison.OrdinalIgnoreCase);

                    if (outfitCompare != 0)
                        return outfitCompare;

                    // Then by variation name
                    var variationNameA = a.outfit.ParentOutfit != null ? a.outfit.Name : string.Empty;
                    var variationNameB = b.outfit.ParentOutfit != null ? b.outfit.Name : string.Empty;
                    return string.Compare(variationNameA, variationNameB, StringComparison.OrdinalIgnoreCase);
                });
            }
            else
            {
                // Default alphabetical sort
                filteredOutfits.Sort((a, b) =>
                {
                    var outfitNameA = a.outfit.ParentOutfit?.Name ?? a.outfit.Name;
                    var outfitNameB = b.outfit.ParentOutfit?.Name ?? b.outfit.Name;
                    var outfitCompare = string.Compare(outfitNameA, outfitNameB, StringComparison.OrdinalIgnoreCase);

                    if (outfitCompare != 0)
                        return outfitCompare;

                    var variationNameA = a.outfit.ParentOutfit != null ? a.outfit.Name : string.Empty;
                    var variationNameB = b.outfit.ParentOutfit != null ? b.outfit.Name : string.Empty;
                    return string.Compare(variationNameA, variationNameB, StringComparison.OrdinalIgnoreCase);
                });
            }

            foreach (var (outfit, _) in filteredOutfits)
            {
                var item = new OutfitItemViewModel(outfit);

                // Restore selection state if this outfit was previously selected
                if (selectedCombinedNames.Contains(outfit.CombinedName))
                {
                    item.IsSelected = true;
                }

                item.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(OutfitItemViewModel.IsSelected))
                    {
                        UpdateStatusText();
                        ((RelayCommand)AddSelectedCommand).RaiseCanExecuteChanged();
                    }
                };
                OutfitItems.Add(item);
            }

            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            int count = OutfitItems.Count(o => o.IsSelected);
            StatusText = $"{count} outfit{(count != 1 ? "s" : "")} selected";
        }

        #endregion
    }
}
