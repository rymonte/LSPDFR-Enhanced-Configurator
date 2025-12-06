using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using LSPDFREnhancedConfigurator.Commands;
using LSPDFREnhancedConfigurator.Commands.Ranks;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Services;

namespace LSPDFREnhancedConfigurator.UI.ViewModels
{
    public class RanksViewModel : ViewModelBase
    {
        private List<RankHierarchy> _ranks = new List<RankHierarchy>();
        private RankTreeItemViewModel? _selectedTreeItem;
        private bool _isUpdatingUI;
        private string _rankName = string.Empty;
        private int? _requiredPoints;
        private int? _salary;
        private string _validationMessage = string.Empty;
        private string _requiredPointsValidation = string.Empty;
        private string _salaryValidation = string.Empty;
        private string _nameValidation = string.Empty;
        private string _stationAdvisory = string.Empty;
        private string _salaryAdvisory = string.Empty;
        private bool _showXpSalaryFields = true;
        private Stack<string> _undoStack = new Stack<string>();
        private Stack<string> _redoStack = new Stack<string>();
        private const int MaxUndoStackSize = 50;
        private bool _hasUncommittedChanges = false;

        // Track the last affected rank ID for smart expansion after Undo/Redo
        private string? _lastAffectedRankId = null;

        // Command Pattern Undo/Redo Manager
        private readonly UndoRedoManager _undoRedoManager = new UndoRedoManager(maxStackSize: 50);

        // Old value tracking for property changes (captured BEFORE first change in batch)
        private int? _oldRequiredPoints;
        private int? _oldSalary;
        private string? _oldRankName;

        /// <summary>
        /// Gets the ID of the parent rank that should be expanded after the current operation
        /// </summary>
        public string? LastAffectedRankId => _lastAffectedRankId;

        public RanksViewModel(List<RankHierarchy>? loadedRanks, DataLoadingService dataService)
        {
            RankTreeItems = new ObservableCollection<RankTreeItemViewModel>();

            // Initialize visibility - no rank selected by default
            _showXpSalaryFields = false;

            // Subscribe to UndoRedoManager events
            _undoRedoManager.StacksChanged += (s, e) => UpdateCommandStates();

            // Commands
            AddRankCommand = new RelayCommand(OnAddRank, CanAddRank);
            AddPayBandCommand = new RelayCommand(OnAddPayBand, CanAddPayBand);
            UndoCommand = new RelayCommand(OnUndo, CanUndo);
            RedoCommand = new RelayCommand(OnRedo, CanRedo);
            PromoteCommand = new RelayCommand(OnPromote, CanPromote);
            CloneCommand = new RelayCommand(OnClone, CanClone);
            RemoveCommand = new RelayCommand(OnRemove, CanRemove);
            RemoveAllRanksCommand = new RelayCommand(OnRemoveAllRanks, CanRemoveAllRanks);
            MoveUpCommand = new RelayCommand(OnMoveUp, CanMoveUp);
            MoveDownCommand = new RelayCommand(OnMoveDown, CanMoveDown);

            // Load actual ranks
            if (loadedRanks != null && loadedRanks.Count > 0)
            {
                _ranks = loadedRanks;
                RefreshTreeView();
                Logger.Info($"RanksViewModel loaded {_ranks.Count} rank hierarchies");
            }
            else
            {
                // No ranks loaded yet - Initialize will be called later
                // Don't load sample data during startup
                Logger.Info("RanksViewModel initialized with no ranks - waiting for data load");
            }
        }

        #region Properties

        public ObservableCollection<RankTreeItemViewModel> RankTreeItems { get; }

        public List<RankHierarchy> RankHierarchies => _ranks;

        public RankTreeItemViewModel? SelectedTreeItem
        {
            get => _selectedTreeItem;
            set
            {
                if (SetProperty(ref _selectedTreeItem, value))
                {
                    Logger.Info($"[SelectedTreeItem] Changed to: {(value != null ? $"'{value.Rank.Name}'" : "null")}");
                    OnRankSelected();
                    UpdateCommandStates();
                }
            }
        }

        public string RankName
        {
            get => _rankName;
            set
            {
                if (SetProperty(ref _rankName, value) && !_isUpdatingUI && SelectedTreeItem != null)
                {
                    // Don't allow editing pay band names - they must match parent + roman numeral
                    if (SelectedTreeItem.Rank.Parent != null) return;

                    // Capture old value BEFORE first change in batch
                    if (!_hasUncommittedChanges)
                    {
                        _oldRankName = SelectedTreeItem.Rank.Name;
                        Logger.Info($"[Property Change] Captured old RankName: '{_oldRankName}'");
                    }

                    SelectedTreeItem.Rank.Name = value;
                    SelectedTreeItem.UpdateDisplayName();

                    // If this rank has pay bands, update their names to match parent name
                    if (SelectedTreeItem.Rank.PayBands.Count > 0)
                    {
                        RenumberPayBands(SelectedTreeItem.Rank);
                        RefreshTreeView(); // Need to refresh to show updated pay band names (includes validation update)
                    }
                    else
                    {
                        // Update validation icon in tree if no pay bands (RefreshTreeView handles it otherwise)
                        UpdateTreeItemValidation(SelectedTreeItem);
                    }

                    _hasUncommittedChanges = true;
                    ValidateName();
                    Logger.Info($"[Property Change] RankName changed from '{_oldRankName}' to '{value}'");
                }
            }
        }

        public int? RequiredPoints
        {
            get => _requiredPoints;
            set
            {
                if (SetProperty(ref _requiredPoints, value) && !_isUpdatingUI && SelectedTreeItem != null)
                {
                    // Capture old value BEFORE first change in batch
                    if (!_hasUncommittedChanges)
                    {
                        _oldRequiredPoints = SelectedTreeItem.Rank.RequiredPoints;
                        Logger.Info($"[Property Change] Captured old RequiredPoints: {_oldRequiredPoints}");
                    }

                    // Treat null as 0
                    SelectedTreeItem.Rank.RequiredPoints = value ?? 0;
                    ValidateRequiredPoints();
                    SelectedTreeItem.UpdateDisplayName();

                    // Don't update validation icon yet - wait for LostFocus

                    // If this is a pay band, update parent's display too
                    if (SelectedTreeItem.Rank.Parent != null)
                    {
                        UpdateParentRankDisplay(SelectedTreeItem.Rank.Parent);
                    }

                    _hasUncommittedChanges = true;
                    Logger.Info($"[Property Change] RequiredPoints changed from {_oldRequiredPoints} to {value ?? 0}");
                }
            }
        }

        public int? Salary
        {
            get => _salary;
            set
            {
                if (SetProperty(ref _salary, value) && !_isUpdatingUI && SelectedTreeItem != null)
                {
                    // Capture old value BEFORE first change in batch
                    if (!_hasUncommittedChanges)
                    {
                        _oldSalary = SelectedTreeItem.Rank.Salary;
                        Logger.Info($"[Property Change] Captured old Salary: {_oldSalary}");
                    }

                    // Treat null as 0
                    SelectedTreeItem.Rank.Salary = value ?? 0;
                    ValidateSalary();
                    SelectedTreeItem.UpdateDisplayName();

                    // Don't update validation icon yet - wait for LostFocus

                    // If this is a pay band, update parent's display too
                    if (SelectedTreeItem.Rank.Parent != null)
                    {
                        UpdateParentRankDisplay(SelectedTreeItem.Rank.Parent);
                    }

                    _hasUncommittedChanges = true;
                    Logger.Info($"[Property Change] Salary changed from {_oldSalary} to {value ?? 0}");
                }
            }
        }

        public bool ShowXpSalaryFields
        {
            get => _showXpSalaryFields;
            set => SetProperty(ref _showXpSalaryFields, value);
        }

        public string ValidationMessage
        {
            get => _validationMessage;
            set => SetProperty(ref _validationMessage, value);
        }

        public string RequiredPointsValidation
        {
            get => _requiredPointsValidation;
            set
            {
                if (SetProperty(ref _requiredPointsValidation, value))
                {
                    OnPropertyChanged(nameof(RequiredPointsBorderBrush));
                    OnPropertyChanged(nameof(RequiredPointsBorderThickness));
                    // Don't call RaiseDataChanged() here - validation state updates on LostFocus only
                }
            }
        }

        public string SalaryValidation
        {
            get => _salaryValidation;
            set
            {
                if (SetProperty(ref _salaryValidation, value))
                {
                    OnPropertyChanged(nameof(SalaryBorderBrush));
                    OnPropertyChanged(nameof(SalaryBorderThickness));
                    // Don't call RaiseDataChanged() here - validation state updates on LostFocus only
                }
            }
        }

        public string NameValidation
        {
            get => _nameValidation;
            set
            {
                if (SetProperty(ref _nameValidation, value))
                {
                    OnPropertyChanged(nameof(NameBorderBrush));
                    OnPropertyChanged(nameof(NameBorderThickness));
                    // Don't call RaiseDataChanged() here - validation state updates on LostFocus only
                }
            }
        }

        public string RequiredPointsBorderBrush => string.IsNullOrEmpty(_requiredPointsValidation) ? "#00ADEA" : "#DC3545";
        public Thickness RequiredPointsBorderThickness => string.IsNullOrEmpty(_requiredPointsValidation) ? new Thickness(1) : new Thickness(2);

        public string SalaryBorderBrush => string.IsNullOrEmpty(_salaryValidation) ? "#00ADEA" : "#DC3545";
        public Thickness SalaryBorderThickness => string.IsNullOrEmpty(_salaryValidation) ? new Thickness(1) : new Thickness(2);

        public string NameBorderBrush => string.IsNullOrEmpty(_nameValidation) ? "#00ADEA" : "#FFA500"; // Orange for warnings
        public Thickness NameBorderThickness => string.IsNullOrEmpty(_nameValidation) ? new Thickness(1) : new Thickness(2);

        public string StationAdvisory
        {
            get => _stationAdvisory;
            set
            {
                if (SetProperty(ref _stationAdvisory, value))
                {
                    RaiseDataChanged(); // Notify MainWindow of advisory change
                }
            }
        }

        public string SalaryAdvisory
        {
            get => _salaryAdvisory;
            set
            {
                if (SetProperty(ref _salaryAdvisory, value))
                {
                    RaiseDataChanged(); // Notify MainWindow of advisory change
                }
            }
        }

        public bool CanPromoteToParent => SelectedTreeItem?.Rank.Parent != null;
        public bool CanAddPayBandToSelected => SelectedTreeItem?.Rank.Parent == null;

        /// <summary>
        /// True if selected item is a pay band (has a parent rank)
        /// </summary>
        public bool IsPayBandSelected => SelectedTreeItem?.Rank.Parent != null;

        /// <summary>
        /// True if selected item can have its name edited (not a pay band)
        /// </summary>
        public bool CanEditRankName => SelectedTreeItem != null && SelectedTreeItem.Rank.Parent == null;

        #endregion

        #region Commands

        public ICommand AddRankCommand { get; }
        public ICommand AddPayBandCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand PromoteCommand { get; }
        public ICommand CloneCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand RemoveAllRanksCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }

        #endregion

        #region Events

        public event EventHandler? DataChanged;
        public event EventHandler? RequestFocusRanksTab;
        public event EventHandler? UndoRedoStateChanged;
        public event EventHandler<StatusMessageEventArgs>? StatusMessageChanged;

        private void RaiseDataChanged()
        {
            // Update validation states for all tree items when data changes
            UpdateAllValidationStates();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseRequestFocusRanksTab()
        {
            RequestFocusRanksTab?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseStatusMessage(string message)
        {
            StatusMessageChanged?.Invoke(this, new StatusMessageEventArgs(message));
        }

        #endregion

        #region Command Handlers

        private bool CanAddRank()
        {
            // Allow adding rank if: (1) there are no ranks at all, OR (2) a rank is selected
            return _ranks.Count == 0 || SelectedTreeItem != null;
        }

        private void OnAddRank()
        {
            int defaultXP;
            int defaultSalary;
            int insertIndex;
            RankHierarchy newRank;
            AddRankCommand command;
            RankTreeItemViewModel? newTreeItem;

            // Handle case when there are no ranks at all
            if (_ranks.Count == 0)
            {
                // First rank defaults to XP 0, salary $30
                defaultXP = 0;
                defaultSalary = 30;
                insertIndex = 0;

                newRank = new RankHierarchy($"New Rank 1", defaultXP, defaultSalary);
                _lastAffectedRankId = newRank.Id;

                command = new AddRankCommand(
                    _ranks,
                    newRank,
                    insertIndex,
                    RefreshTreeView,
                    RaiseDataChanged
                );

                _undoRedoManager.ExecuteCommand(command);
                Logger.Info($"Added first rank '{newRank.Name}' with XP={defaultXP}, Salary=${defaultSalary}");

                // Select the newly added rank
                newTreeItem = FindTreeItem(newRank.Id);
                if (newTreeItem != null)
                {
                    SelectedTreeItem = newTreeItem;
                    Logger.Info($"Auto-selected first rank '{newRank.Name}'");
                }
                return;
            }

            // Normal case - there are existing ranks and one is selected
            if (SelectedTreeItem?.Rank == null) return;

            var selectedRank = SelectedTreeItem.Rank;

            // Find the parent rank to insert after
            var targetParentRank = selectedRank.Parent ?? selectedRank;

            // Get actual XP/Salary values (use last pay band if parent has pay bands)
            int prevXP, prevSalary;
            if (targetParentRank.PayBands.Count > 0)
            {
                var lastPayBand = targetParentRank.PayBands[targetParentRank.PayBands.Count - 1];
                prevXP = lastPayBand.RequiredPoints;
                prevSalary = lastPayBand.Salary;
            }
            else
            {
                prevXP = targetParentRank.RequiredPoints;
                prevSalary = targetParentRank.Salary;
            }

            // Default to same values as previous rank
            defaultXP = prevXP;
            defaultSalary = prevSalary;

            // If there's a next rank, use midpoint values instead
            var index = _ranks.IndexOf(targetParentRank);
            if (index >= 0 && index < _ranks.Count - 1)
            {
                var nextRank = _ranks[index + 1];
                defaultXP = prevXP + ((nextRank.RequiredPoints - prevXP) / 2);
                defaultSalary = prevSalary + ((nextRank.Salary - prevSalary) / 2);
            }

            // Create new rank with calculated defaults
            newRank = new RankHierarchy($"New Rank {_ranks.Count + 1}", defaultXP, defaultSalary);

            // Track new rank for undo expansion and selection
            _lastAffectedRankId = newRank.Id;

            // Determine insert index
            insertIndex = index >= 0 ? index + 1 : _ranks.Count;

            // Create and execute AddRankCommand
            command = new AddRankCommand(
                _ranks,
                newRank,
                insertIndex,
                RefreshTreeView,
                RaiseDataChanged
            );

            _undoRedoManager.ExecuteCommand(command);
            Logger.Info($"Added new rank '{newRank.Name}' at index {insertIndex} with XP={defaultXP}, Salary=${defaultSalary}");

            // Select the newly added rank
            newTreeItem = FindTreeItem(newRank.Id);
            if (newTreeItem != null)
            {
                SelectedTreeItem = newTreeItem;
                Logger.Info($"Auto-selected newly added rank '{newRank.Name}'");
            }
        }

        private bool CanAddPayBand()
        {
            // Can add pay band if a parent rank OR a pay band is selected
            return SelectedTreeItem?.Rank != null;
        }

        private void OnAddPayBand()
        {
            if (SelectedTreeItem?.Rank == null) return;

            var selectedRank = SelectedTreeItem.Rank;

            // Determine the parent rank
            var parentRank = selectedRank.Parent ?? selectedRank;

            // Track parent rank for undo expansion
            _lastAffectedRankId = parentRank.Id;

            RankHierarchy newPayBand;
            int insertIndex;

            // Calculate smart default values
            int defaultXP;
            int defaultSalary;

            if (parentRank.PayBands.Count == 0)
            {
                // First pay band - inherit from parent
                defaultXP = parentRank.RequiredPoints;
                defaultSalary = parentRank.Salary;
            }
            else if (selectedRank.Parent != null)
            {
                // Inserting after a specific pay band - use increment from selected pay band
                var index = parentRank.PayBands.IndexOf(selectedRank);
                insertIndex = index >= 0 ? index + 1 : parentRank.PayBands.Count;

                // If there's a next pay band, use midpoint
                if (insertIndex < parentRank.PayBands.Count)
                {
                    var nextPayBand = parentRank.PayBands[insertIndex];
                    defaultXP = selectedRank.RequiredPoints + ((nextPayBand.RequiredPoints - selectedRank.RequiredPoints) / 2);
                    defaultSalary = selectedRank.Salary + ((nextPayBand.Salary - selectedRank.Salary) / 2);
                }
                else
                {
                    // Adding at end - same as previous pay band
                    defaultXP = selectedRank.RequiredPoints;
                    defaultSalary = selectedRank.Salary;
                }
            }
            else
            {
                // Adding to parent rank - same as last pay band
                var lastPayBand = parentRank.PayBands[parentRank.PayBands.Count - 1];
                defaultXP = lastPayBand.RequiredPoints;
                defaultSalary = lastPayBand.Salary;
            }

            if (selectedRank.Parent != null)
            {
                // A pay band is selected - insert after it
                var index = parentRank.PayBands.IndexOf(selectedRank);
                insertIndex = index >= 0 ? index + 1 : parentRank.PayBands.Count;

                var payBandNumber = insertIndex + 1;
                var payBandName = $"{parentRank.Name} {RankHierarchy.GetRomanNumeral(payBandNumber)}";
                newPayBand = new RankHierarchy(payBandName, defaultXP, defaultSalary);

                Logger.Info($"Adding pay band '{payBandName}' after '{selectedRank.Name}' at index {insertIndex} with XP={defaultXP}, Salary=${defaultSalary}");
            }
            else
            {
                // A parent rank is selected - add pay band to the end
                insertIndex = parentRank.PayBands.Count;
                var payBandNumber = insertIndex + 1;
                var payBandName = $"{parentRank.Name} {RankHierarchy.GetRomanNumeral(payBandNumber)}";
                newPayBand = new RankHierarchy(payBandName, defaultXP, defaultSalary);

                Logger.Info($"Adding pay band '{payBandName}' to parent '{parentRank.Name}' with XP={defaultXP}, Salary=${defaultSalary}");
            }

            // Create and execute AddPayBandCommand
            var command = new AddPayBandCommand(
                _ranks,
                newPayBand,
                parentRank,
                insertIndex,
                RenumberPayBands,
                RefreshTreeView,
                RaiseDataChanged
            );

            _undoRedoManager.ExecuteCommand(command);
        }

        private bool CanUndo()
        {
            // Check both old JSON-based stack AND new command-based manager
            // During migration, we support both
            return _undoStack.Count > 0 || _undoRedoManager.CanUndo;
        }

        private void OnUndo()
        {
            // Request to switch to Ranks tab if not already there
            RaiseRequestFocusRanksTab();

            // Try new command-based undo first
            if (_undoRedoManager.CanUndo)
            {
                Logger.Info($"[Undo] Using command-based undo. Description: {_undoRedoManager.GetUndoDescription()}");
                _undoRedoManager.Undo();
                Logger.Info($"Undo performed. Undo stack: {_undoRedoManager.UndoStackSize}, Redo stack: {_undoRedoManager.RedoStackSize}");
                return;
            }

            // Fall back to old JSON-based undo
            if (_undoStack.Count == 0) return;

            // LastAffectedRankId is already set by the operation that was performed (e.g., OnRemove)
            // This will be used by RestoreExpansionStates to expand the affected rank
            Logger.Info($"[Undo] Using JSON-based undo. LastAffectedRankId: {_lastAffectedRankId?.Substring(0, Math.Min(8, _lastAffectedRankId?.Length ?? 0)) ?? "null"}");

            // Save current state to redo stack
            _redoStack.Push(SerializeRanks());

            // Restore previous state
            var previousState = _undoStack.Pop();
            RestoreRanks(previousState);

            UpdateCommandStates();
            Logger.Info($"Undo performed. Undo stack: {_undoStack.Count}, Redo stack: {_redoStack.Count}");
        }

        private bool CanRedo()
        {
            // Check both old JSON-based stack AND new command-based manager
            // During migration, we support both
            return _redoStack.Count > 0 || _undoRedoManager.CanRedo;
        }

        private void OnRedo()
        {
            // Request to switch to Ranks tab if not already there
            RaiseRequestFocusRanksTab();

            // Try new command-based redo first
            if (_undoRedoManager.CanRedo)
            {
                Logger.Info($"[Redo] Using command-based redo. Description: {_undoRedoManager.GetRedoDescription()}");
                _undoRedoManager.Redo();
                Logger.Info($"Redo performed. Undo stack: {_undoRedoManager.UndoStackSize}, Redo stack: {_undoRedoManager.RedoStackSize}");
                return;
            }

            // Fall back to old JSON-based redo
            if (_redoStack.Count == 0) return;

            // LastAffectedRankId should still be set from the previous operation
            // This will be used by RestoreExpansionStates to expand the affected rank
            Logger.Info($"[Redo] Using JSON-based redo. LastAffectedRankId: {_lastAffectedRankId?.Substring(0, Math.Min(8, _lastAffectedRankId?.Length ?? 0)) ?? "null"}");

            // Save current state to undo stack (without clearing redo)
            var currentState = SerializeRanks();
            _undoStack.Push(currentState);

            // Restore next state
            var nextState = _redoStack.Pop();
            RestoreRanks(nextState);

            UpdateCommandStates();
            Logger.Info($"Redo performed. Undo stack: {_undoStack.Count}, Redo stack: {_redoStack.Count}");
        }

        private bool CanPromote()
        {
            return SelectedTreeItem?.Rank.Parent != null;
        }

        private void OnPromote()
        {
            if (SelectedTreeItem?.Rank == null) return;

            var rank = SelectedTreeItem.Rank;

            // Track the rank being promoted for undo expansion
            _lastAffectedRankId = rank.Id;

            // Must have a parent to promote
            if (rank.Parent == null)
            {
                Logger.Warn("Cannot promote rank with no parent");
                return;
            }

            var parent = rank.Parent;
            Logger.Info($"Promoting pay band '{rank.Name}' from parent '{parent.Name}' to top-level rank");

            // Get the pay band's current index in parent's PayBands list
            var payBandIndex = parent.PayBands.IndexOf(rank);

            // Determine where to insert in _ranks list (after the parent)
            var parentIndex = _ranks.IndexOf(parent);
            int insertIndex = parentIndex >= 0 ? parentIndex + 1 : _ranks.Count;

            // Create and execute PromoteRankCommand
            var command = new PromoteRankCommand(
                _ranks,
                rank,
                parent,
                payBandIndex,
                insertIndex,
                RenumberPayBands,
                RefreshTreeView,
                RaiseDataChanged
            );

            _undoRedoManager.ExecuteCommand(command);
            Logger.Info($"Promoted {rank.Name} to parent, tracking rank ID for undo");
        }

        private bool CanClone()
        {
            return SelectedTreeItem != null;
        }

        private void OnClone()
        {
            if (SelectedTreeItem?.Rank == null) return;

            var selectedRank = SelectedTreeItem.Rank;
            var cloned = selectedRank.Clone();

            CloneRankCommand command;

            if (selectedRank.Parent != null)
            {
                // Clone pay band - insert after selected pay band
                var parent = selectedRank.Parent;
                var index = parent.PayBands.IndexOf(selectedRank);

                // Track parent rank for undo expansion
                _lastAffectedRankId = parent.Id;

                int insertIndex = index >= 0 ? index + 1 : parent.PayBands.Count;

                // Create CloneRankCommand for pay band
                command = new CloneRankCommand(
                    _ranks,
                    cloned,
                    parent,
                    insertIndex,
                    selectedRank.Name,
                    RenumberPayBands,
                    RefreshTreeView,
                    RaiseDataChanged
                );

                Logger.Info($"Cloning pay band '{selectedRank.Name}' after itself at index {insertIndex}, tracking parent ID for undo");
            }
            else
            {
                // Clone parent rank - insert after selected parent rank
                var index = _ranks.IndexOf(selectedRank);

                // Track the cloned rank for undo expansion
                _lastAffectedRankId = cloned.Id;

                int insertIndex = index >= 0 ? index + 1 : _ranks.Count;

                // Create CloneRankCommand for parent rank
                command = new CloneRankCommand(
                    _ranks,
                    cloned,
                    insertIndex,
                    selectedRank.Name,
                    RefreshTreeView,
                    RaiseDataChanged
                );

                Logger.Info($"Cloning rank '{selectedRank.Name}' after itself at index {insertIndex}, tracking clone ID for undo");
            }

            _undoRedoManager.ExecuteCommand(command);
        }

        private bool CanRemove()
        {
            return SelectedTreeItem != null;
        }

        private void OnRemove()
        {
            Logger.Info($"[OnRemove] Called. SelectedTreeItem: {SelectedTreeItem != null}, Rank: {SelectedTreeItem?.Rank != null}");

            if (SelectedTreeItem?.Rank == null)
            {
                Logger.Info($"[OnRemove] No rank selected, returning");
                return;
            }

            var rank = SelectedTreeItem.Rank;

            Logger.Info($"[OnRemove] Removing '{rank.Name}', IsParent: {rank.Parent != null}");

            RemoveRankCommand command;
            string? nextSelectionId = null;

            if (rank.Parent != null)
            {
                // Remove pay band - track parent for undo expansion
                var parent = rank.Parent;
                _lastAffectedRankId = parent.Id;

                var payBandIndex = parent.PayBands.IndexOf(rank);
                Logger.Info($"[OnRemove] Removing pay band '{rank.Name}' from parent '{parent.Name}' at index {payBandIndex}, tracking parent ID for undo");

                // Determine what to select after removal
                if (parent.PayBands.Count > 1)
                {
                    // Select next pay band if available, otherwise previous
                    if (payBandIndex < parent.PayBands.Count - 1)
                    {
                        nextSelectionId = parent.PayBands[payBandIndex + 1].Id;
                        Logger.Info($"[OnRemove] Will select next pay band after removal");
                    }
                    else if (payBandIndex > 0)
                    {
                        nextSelectionId = parent.PayBands[payBandIndex - 1].Id;
                        Logger.Info($"[OnRemove] Will select previous pay band after removal");
                    }
                }
                else
                {
                    // Last pay band - select parent rank
                    nextSelectionId = parent.Id;
                    Logger.Info($"[OnRemove] Last pay band - will select parent rank after removal");
                }

                // Create RemoveRankCommand for pay band
                command = new RemoveRankCommand(
                    _ranks,
                    rank,
                    parent,
                    payBandIndex,
                    RenumberPayBands,
                    RefreshTreeView,
                    RaiseDataChanged
                );
            }
            else
            {
                // Remove parent rank - track its ID for undo expansion
                _lastAffectedRankId = rank.Id;

                var rankIndex = _ranks.IndexOf(rank);
                Logger.Info($"[OnRemove] Removing parent rank '{rank.Name}' from _ranks list at index {rankIndex}, tracking its ID for undo");

                // Determine what to select after removal (next or previous rank)
                if (rankIndex < _ranks.Count - 1)
                {
                    nextSelectionId = _ranks[rankIndex + 1].Id;
                    Logger.Info($"[OnRemove] Will select next rank after removal");
                }
                else if (rankIndex > 0)
                {
                    nextSelectionId = _ranks[rankIndex - 1].Id;
                    Logger.Info($"[OnRemove] Will select previous rank after removal");
                }

                // Create RemoveRankCommand for parent rank
                command = new RemoveRankCommand(
                    _ranks,
                    rank,
                    rankIndex,
                    RefreshTreeView,
                    RaiseDataChanged
                );
            }

            _undoRedoManager.ExecuteCommand(command);
            Logger.Info($"Removed {rank.Name}");

            // Select the next appropriate item
            if (nextSelectionId != null)
            {
                var nextTreeItem = FindTreeItem(nextSelectionId);
                if (nextTreeItem != null)
                {
                    SelectedTreeItem = nextTreeItem;
                    Logger.Info($"Auto-selected '{nextTreeItem.Rank.Name}' after removal");
                }
            }
            else
            {
                // No items left - clear selection
                SelectedTreeItem = null;
                Logger.Info("No items left - cleared selection after removal");
            }
        }

        private bool CanRemoveAllRanks()
        {
            return _ranks.Count > 0;
        }

        private async void OnRemoveAllRanks()
        {
            if (_ranks.Count == 0)
            {
                Logger.Info("[OnRemoveAllRanks] No ranks to remove, returning");
                return;
            }

            Logger.Info($"[OnRemoveAllRanks] Attempting to remove all {_ranks.Count} rank(s)");

            // Show confirmation dialog
            var dialog = new Avalonia.Controls.Window
            {
                Title = "Confirm Remove All Ranks",
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
                                    Source = new Avalonia.Media.Imaging.Bitmap(Avalonia.Platform.AssetLoader.Open(new Uri("avares://LSPDFREnhancedConfigurator/Resources/Icons/warning-icon.png"))),
                                    Width = 32,
                                    Height = 32,
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                },
                                new Avalonia.Controls.TextBlock
                                {
                                    Text = "Remove All Ranks?",
                                    FontSize = 18,
                                    FontWeight = Avalonia.Media.FontWeight.Bold,
                                    Foreground = Avalonia.Media.Brushes.Orange,
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                }
                            }
                        },
                        new Avalonia.Controls.TextBlock
                        {
                            Text = $"Are you sure you want to remove all {_ranks.Count} rank(s)?\n\nThis action can be undone using the Undo button.",
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
                                    Content = "Remove All",
                                    MinWidth = 120
                                }
                            }
                        }
                    }
                }
            };

            var buttonPanel = (Avalonia.Controls.StackPanel)((Avalonia.Controls.StackPanel)dialog.Content).Children[2];
            var cancelButton = (Avalonia.Controls.Button)buttonPanel.Children[0];
            var removeButton = (Avalonia.Controls.Button)buttonPanel.Children[1];

            bool result = false;
            cancelButton.Click += (s, e) => { result = false; dialog.Close(); };
            removeButton.Click += (s, e) => { result = true; dialog.Close(); };

            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                await dialog.ShowDialog(desktop.MainWindow);
            }

            if (!result)
            {
                Logger.Info("[OnRemoveAllRanks] User cancelled removal");
                return;
            }

            var rankCount = _ranks.Count;
            Logger.Info($"[OnRemoveAllRanks] User confirmed - removing all {rankCount} rank(s)");

            var command = new RemoveAllRanksCommand(
                _ranks,
                RefreshTreeView,
                RaiseDataChanged
            );

            _undoRedoManager.ExecuteCommand(command);
            Logger.Info($"Removed all ranks");

            // Clear selection after removing all ranks
            SelectedTreeItem = null;
            Logger.Info("[OnRemoveAllRanks] Cleared selection after removing all ranks");

            RaiseStatusMessage($"Removed all {rankCount} rank(s)");
        }

        private bool CanMoveUp()
        {
            if (SelectedTreeItem?.Rank == null)
            {
                Logger.Info("[CanMoveUp] No item selected - returning false");
                return false;
            }

            var rank = SelectedTreeItem.Rank;

            // Pay bands cannot be moved up/down
            if (rank.Parent != null)
            {
                Logger.Info($"[CanMoveUp] '{rank.Name}' is a pay band (Parent: {rank.Parent.Name}) - returning false");
                return false;
            }

            var index = _ranks.IndexOf(rank);
            bool canMove = index > 0;
            Logger.Info($"[CanMoveUp] '{rank.Name}' is a parent rank at index {index} - returning {canMove}");
            return canMove;
        }

        private void OnMoveUp()
        {
            if (SelectedTreeItem?.Rank == null) return;

            var rank = SelectedTreeItem.Rank;

            // Only move parent ranks, not pay bands
            if (rank.Parent != null) return;

            Logger.Info($"[USER] Moving rank '{rank.Name}' up");

            var index = _ranks.IndexOf(rank);
            if (index > 0)
            {
                // Create and execute MoveRankCommand
                var command = new MoveRankCommand(
                    _ranks,
                    rank.Id,
                    index,
                    index - 1,
                    rank.Name,
                    RefreshTreeView,
                    RaiseDataChanged
                );

                _undoRedoManager.ExecuteCommand(command);
                Logger.Info($"Moved top-level rank '{rank.Name}' up (new index: {index - 1})");
            }
        }

        private bool CanMoveDown()
        {
            if (SelectedTreeItem?.Rank == null)
            {
                Logger.Info("[CanMoveDown] No item selected - returning false");
                return false;
            }

            var rank = SelectedTreeItem.Rank;

            // Pay bands cannot be moved up/down
            if (rank.Parent != null)
            {
                Logger.Info($"[CanMoveDown] '{rank.Name}' is a pay band (Parent: {rank.Parent.Name}) - returning false");
                return false;
            }

            var index = _ranks.IndexOf(rank);
            bool canMove = index < _ranks.Count - 1;
            Logger.Info($"[CanMoveDown] '{rank.Name}' is a parent rank at index {index} - returning {canMove}");
            return canMove;
        }

        private void OnMoveDown()
        {
            if (SelectedTreeItem?.Rank == null) return;

            var rank = SelectedTreeItem.Rank;

            // Only move parent ranks, not pay bands
            if (rank.Parent != null) return;

            Logger.Info($"[USER] Moving rank '{rank.Name}' down");

            var index = _ranks.IndexOf(rank);
            if (index < _ranks.Count - 1)
            {
                // Create and execute MoveRankCommand
                var command = new MoveRankCommand(
                    _ranks,
                    rank.Id,
                    index,
                    index + 1,
                    rank.Name,
                    RefreshTreeView,
                    RaiseDataChanged
                );

                _undoRedoManager.ExecuteCommand(command);
                Logger.Info($"Moved top-level rank '{rank.Name}' down (new index: {index + 1})");
            }
        }

        #endregion

        #region Helper Methods

        private void OnRankSelected()
        {
            if (SelectedTreeItem?.Rank == null)
            {
                _isUpdatingUI = true;
                RankName = string.Empty;
                RequiredPoints = 0;
                Salary = 0;
                ShowXpSalaryFields = false;
                _isUpdatingUI = false;
                Logger.Trace("[USER] Deselected rank - cleared editor fields");
                return;
            }

            var rank = SelectedTreeItem.Rank;
            Logger.Info($"[USER] Selected rank: '{rank.Name}' (Parent: {rank.Parent?.Name ?? "None"}, PayBands: {rank.PayBands.Count})");
            _isUpdatingUI = true;

            RankName = rank.Name;
            RequiredPoints = rank.RequiredPoints;
            Salary = rank.Salary;

            // Hide XP/Salary fields for parent ranks with pay bands
            bool isParentWithPayBands = (rank.Parent == null && rank.PayBands.Count > 0);
            ShowXpSalaryFields = !isParentWithPayBands;

            _isUpdatingUI = false;

            ValidateRequiredPoints();
            ValidateSalary();
            ValidateName();
            CheckAdvisories();

            OnPropertyChanged(nameof(CanPromoteToParent));
            OnPropertyChanged(nameof(CanAddPayBandToSelected));
            OnPropertyChanged(nameof(IsPayBandSelected));
            OnPropertyChanged(nameof(CanEditRankName));
        }

        /// <summary>
        /// Updates the parent rank's display name when a pay band changes
        /// </summary>
        private void UpdateParentRankDisplay(RankHierarchy parentRank)
        {
            // Find the parent's tree item and update its display
            var parentTreeItem = FindTreeItem(RankTreeItems, parentRank);
            if (parentTreeItem != null)
            {
                parentTreeItem.UpdateDisplayName();
            }
        }

        /// <summary>
        /// Renumbers all pay bands to maintain sequential Roman numerals (I, II, III, etc.)
        /// </summary>
        private void RenumberPayBands(RankHierarchy parentRank)
        {
            for (int i = 0; i < parentRank.PayBands.Count; i++)
            {
                var payBand = parentRank.PayBands[i];
                var expectedName = $"{parentRank.Name} {RankHierarchy.GetRomanNumeral(i + 1)}";

                if (payBand.Name != expectedName)
                {
                    payBand.Name = expectedName;
                    Logger.Info($"Renumbered pay band to '{expectedName}'");
                }
            }
        }

        /// <summary>
        /// Recursively finds a tree item by rank
        /// </summary>
        private RankTreeItemViewModel? FindTreeItem(IEnumerable<RankTreeItemViewModel> items, RankHierarchy rank)
        {
            foreach (var item in items)
            {
                if (item.Rank == rank) return item;

                var found = FindTreeItem(item.Children, rank);
                if (found != null) return found;
            }
            return null;
        }

        private void RefreshTreeView()
        {
            Logger.Info($"[RefreshTreeView] Refreshing tree view with {_ranks.Count} rank(s)");

            var currentSelection = SelectedTreeItem;

            RankTreeItems.Clear();
            foreach (var rank in _ranks)
            {
                var treeItem = new RankTreeItemViewModel(rank);
                // Subscribe to IsExpanded changes to update validation highlighting
                treeItem.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(RankTreeItemViewModel.IsExpanded) && s is RankTreeItemViewModel item)
                    {
                        Logger.Info($"[RefreshTreeView] IsExpanded changed for '{item.Rank.Name}' to {item.IsExpanded}");
                        UpdateTreeItemValidation(item);
                        Logger.Info($"[RefreshTreeView] After UpdateTreeItemValidation: Severity={item.ValidationSeverity}, Tooltip='{item.ValidationTooltip}'");
                    }
                };
                RankTreeItems.Add(treeItem);
            }

            // Try to restore selection
            if (currentSelection != null)
            {
                var foundItem = FindTreeItem(currentSelection.Rank.Id);
                if (foundItem != null)
                {
                    SelectedTreeItem = foundItem;
                }
            }

            // Update validation states for all tree items
            UpdateAllValidationStates();

            Logger.Info($"[RefreshTreeView] Refresh complete - {RankTreeItems.Count} item(s) in tree");
        }

        /// <summary>
        /// Updates validation states for all tree items based on current validation
        /// </summary>
        private void UpdateAllValidationStates()
        {
            foreach (var treeItem in RankTreeItems)
            {
                UpdateTreeItemValidation(treeItem);

                // Also update child pay bands
                foreach (var child in treeItem.Children)
                {
                    UpdateTreeItemValidation(child);
                }
            }
        }

        /// <summary>
        /// Updates validation state for a single tree item
        /// </summary>
        private void UpdateTreeItemValidation(RankTreeItemViewModel treeItem)
        {
            var rank = treeItem.Rank;
            var severity = RankValidationSeverity.None;
            var tooltip = string.Empty;

            // Parent ranks with pay bands don't have their own XP/Salary - they derive from children
            bool isParentWithPayBands = rank.Parent == null && rank.PayBands.Count > 0;

            if (!isParentWithPayBands)
            {
                // Only validate XP/Salary for ranks without pay bands (or pay bands themselves)

                // Check XP validation (highest priority - ERROR)
                var xpError = GetXpValidationError(rank);
                Logger.Trace($"[UpdateTreeItemValidation] Rank: {rank.Name}, XP: {rank.RequiredPoints}, XpError: '{xpError}'");
                if (!string.IsNullOrEmpty(xpError))
                {
                    severity = RankValidationSeverity.Error;
                    tooltip = xpError;
                    Logger.Info($"[UpdateTreeItemValidation] Setting ERROR severity for {rank.Name}");
                    treeItem.UpdateValidationState(severity, tooltip);
                    return;
                }

                // Check salary validation (WARNING)
                var salaryError = GetSalaryValidationError(rank);
                if (!string.IsNullOrEmpty(salaryError))
                {
                    severity = RankValidationSeverity.Warning;
                    tooltip = salaryError;
                    treeItem.UpdateValidationState(severity, tooltip);
                    return;
                }

                // Check salary warnings (WARNING)
                var warnings = GetWarnings(rank);
                if (warnings.Count > 0)
                {
                    severity = RankValidationSeverity.Warning;
                    tooltip = string.Join("\n", warnings);
                    treeItem.UpdateValidationState(severity, tooltip);
                    return;
                }
            }
            else
            {
                // For parent ranks with pay bands: check structural validation first
                // Structural error: rank must have more than one pay band
                if (rank.PayBands.Count == 1)
                {
                    severity = RankValidationSeverity.Error;
                    tooltip = "Rank must have more than one pay band";
                    Logger.Info($"[UpdateTreeItemValidation] Setting ERROR severity for {rank.Name} - only 1 pay band");
                    treeItem.UpdateValidationState(severity, tooltip);
                    return;
                }

                // For parent ranks with pay bands: bubble up child errors when collapsed
                Logger.Info($"[UpdateTreeItemValidation] Parent rank '{rank.Name}' - IsExpanded={treeItem.IsExpanded}, Children count={treeItem.Children.Count}");
                if (!treeItem.IsExpanded)
                {
                    Logger.Info($"[UpdateTreeItemValidation] Parent rank '{rank.Name}' is COLLAPSED - checking children for errors to bubble up");
                    // Check all children for validation issues
                    var childSeverity = RankValidationSeverity.None;
                    var errorMessages = new List<string>();

                    foreach (var child in treeItem.Children)
                    {
                        var childRank = child.Rank;

                        // Check child XP errors
                        var childXpError = GetXpValidationError(childRank);
                        if (!string.IsNullOrEmpty(childXpError))
                        {
                            childSeverity = RankValidationSeverity.Error;
                            errorMessages.Add($"{childRank.Name}: {childXpError}");
                        }

                        // Check child salary errors/warnings
                        var childSalaryError = GetSalaryValidationError(childRank);
                        if (!string.IsNullOrEmpty(childSalaryError))
                        {
                            if (childSeverity == RankValidationSeverity.None)
                                childSeverity = RankValidationSeverity.Warning;
                            errorMessages.Add($"{childRank.Name}: {childSalaryError}");
                        }

                        var childWarnings = GetWarnings(childRank);
                        if (childWarnings.Count > 0)
                        {
                            if (childSeverity == RankValidationSeverity.None)
                                childSeverity = RankValidationSeverity.Warning;
                            foreach (var warning in childWarnings)
                            {
                                errorMessages.Add($"{childRank.Name}: {warning}");
                            }
                        }

                        // Check child name warnings
                        var childNameWarning = GetNameValidationWarning(childRank);
                        if (!string.IsNullOrEmpty(childNameWarning))
                        {
                            if (childSeverity == RankValidationSeverity.None)
                                childSeverity = RankValidationSeverity.Advisory;
                            errorMessages.Add($"{childRank.Name}: {childNameWarning}");
                        }
                    }

                    severity = childSeverity;
                    tooltip = string.Join("\n", errorMessages);
                }
                // When expanded, parent shows no errors (children show their own)
            }

            // Check name validation (ADVISORY) - applies to all ranks
            var nameWarning = GetNameValidationWarning(rank);
            if (!string.IsNullOrEmpty(nameWarning))
            {
                // Only override if we don't already have a higher severity
                if (severity == RankValidationSeverity.None)
                {
                    severity = RankValidationSeverity.Advisory;
                    tooltip = nameWarning;
                }
            }

            Logger.Trace($"[UpdateTreeItemValidation] Final severity for {rank.Name}: {severity}");
            treeItem.UpdateValidationState(severity, tooltip);
        }

        /// <summary>
        /// Gets XP validation error for a rank (returns error message or empty string)
        /// </summary>
        private string GetXpValidationError(RankHierarchy rank)
        {
            if (rank.RequiredPoints < 0)
            {
                return "Required Points must be greater than or equal to 0";
            }

            // Determine the minimum XP this rank must exceed
            int? minimumRequiredXp = null;

            if (rank.Parent != null)
            {
                // For pay bands
                var payBandIndex = rank.Parent.PayBands.IndexOf(rank);
                if (payBandIndex > 0)
                {
                    // Not first pay band - compare against previous pay band
                    minimumRequiredXp = rank.Parent.PayBands[payBandIndex - 1].RequiredPoints;
                }
                else if (payBandIndex == 0)
                {
                    // First pay band - must compare against previous RANK in _ranks list, not parent
                    // Parent derives XP from children, so we can't use parent.RequiredPoints
                    var parentIndex = _ranks.IndexOf(rank.Parent);
                    if (parentIndex > 0)
                    {
                        var previousRank = _ranks[parentIndex - 1];
                        minimumRequiredXp = previousRank.IsParent && previousRank.PayBands.Count > 0
                            ? previousRank.PayBands.Max(pb => pb.RequiredPoints)
                            : previousRank.RequiredPoints;
                    }
                    else
                    {
                        // First rank's first pay band - should be >= 0
                        minimumRequiredXp = 0;
                    }
                }
            }
            else
            {
                // For parent ranks
                var rankIndex = _ranks.IndexOf(rank);
                if (rankIndex > 0)
                {
                    var previousRank = _ranks[rankIndex - 1];
                    minimumRequiredXp = previousRank.IsParent && previousRank.PayBands.Count > 0
                        ? previousRank.PayBands.Max(pb => pb.RequiredPoints)
                        : previousRank.RequiredPoints;
                }
            }

            if (minimumRequiredXp.HasValue)
            {
                bool isFirstPayBand = rank.Parent != null && rank.Parent.PayBands.IndexOf(rank) == 0;

                if (isFirstPayBand && rank.RequiredPoints < minimumRequiredXp.Value)
                {
                    return $"Required Points must be greater than or equal to {minimumRequiredXp.Value} (previous rank)";
                }
                else if (!isFirstPayBand && rank.RequiredPoints <= minimumRequiredXp.Value)
                {
                    var context = rank.Parent != null ? "previous pay band" : "previous rank";
                    return $"Required Points must be greater than {minimumRequiredXp.Value} ({context})";
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets salary validation error for a rank (returns error message or empty string)
        /// </summary>
        private string GetSalaryValidationError(RankHierarchy rank)
        {
            if (rank.Salary < 0)
            {
                return "Salary must be greater than or equal to 0";
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets name validation warning for a rank (returns warning message or empty string)
        /// </summary>
        private string GetNameValidationWarning(RankHierarchy rank)
        {
            var allRanks = new List<RankHierarchy>();
            foreach (var r in _ranks)
            {
                allRanks.Add(r);
                allRanks.AddRange(r.PayBands);
            }

            var duplicates = allRanks.Where(r => r.Id != rank.Id &&
                                                  string.Equals(r.Name, rank.Name, StringComparison.OrdinalIgnoreCase))
                                     .ToList();

            if (duplicates.Count > 0)
            {
                return "Name: Another rank already uses this name";
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets warnings for a rank (salary lower than previous)
        /// </summary>
        private List<string> GetWarnings(RankHierarchy rank)
        {
            var warnings = new List<string>();

            // Find previous rank for comparison
            RankHierarchy? previousRank = null;
            int? previousSalary = null;

            if (rank.Parent != null)
            {
                var payBandIndex = rank.Parent.PayBands.IndexOf(rank);
                if (payBandIndex > 0)
                {
                    // Not first pay band - compare against previous pay band
                    previousRank = rank.Parent.PayBands[payBandIndex - 1];
                    previousSalary = previousRank.Salary;
                }
                else if (payBandIndex == 0)
                {
                    // First pay band - must compare against previous RANK in _ranks list, not parent
                    // Parent derives Salary from children, so we can't use parent.Salary
                    var parentIndex = _ranks.IndexOf(rank.Parent);
                    if (parentIndex > 0)
                    {
                        previousRank = _ranks[parentIndex - 1];
                        previousSalary = previousRank.IsParent && previousRank.PayBands.Count > 0
                            ? previousRank.PayBands.Max(pb => pb.Salary)
                            : previousRank.Salary;
                    }
                    // If parentIndex == 0, there's no previous rank, so no warning applies
                }
            }
            else
            {
                var rankIndex = _ranks.IndexOf(rank);
                if (rankIndex > 0)
                {
                    previousRank = _ranks[rankIndex - 1];
                    previousSalary = previousRank.IsParent && previousRank.PayBands.Count > 0
                        ? previousRank.PayBands.Max(pb => pb.Salary)
                        : previousRank.Salary;
                }
            }

            if (previousRank != null)
            {
                // Warning: Check for lower salary
                if (previousSalary.HasValue && rank.Salary < previousSalary.Value)
                {
                    warnings.Add($"Salary is lower than previous ({previousSalary.Value})");
                }
            }

            return warnings;
        }

        /// <summary>
        /// Called when XP or Salary field loses focus - triggers validation update
        /// </summary>
        public void OnFieldLostFocus()
        {
            if (SelectedTreeItem != null)
            {
                UpdateTreeItemValidation(SelectedTreeItem);

                // If this is a pay band, also update the parent's validation
                // This ensures collapsed parents show bubbled-up child errors
                if (SelectedTreeItem.Rank.Parent != null)
                {
                    var parentTreeItem = FindTreeItem(SelectedTreeItem.Rank.Parent.Id);
                    if (parentTreeItem != null)
                    {
                        Logger.Info($"[OnFieldLostFocus] Updating parent '{parentTreeItem.Rank.Name}' validation after child change");
                        UpdateTreeItemValidation(parentTreeItem);
                    }
                }

                RaiseDataChanged();
            }
        }

        private RankTreeItemViewModel? FindTreeItem(string rankId)
        {
            foreach (var item in RankTreeItems)
            {
                if (item.Rank.Id == rankId)
                    return item;

                foreach (var child in item.Children)
                {
                    if (child.Rank.Id == rankId)
                        return child;
                }
            }
            return null;
        }

        private void ValidateRequiredPoints()
        {
            RequiredPointsValidation = string.Empty;

            if (SelectedTreeItem?.Rank == null) return;

            // Check if value is null/empty
            if (_requiredPoints == null)
            {
                RequiredPointsValidation = "Required Points cannot be empty";
                return;
            }

            var rank = SelectedTreeItem.Rank;
            if (rank.RequiredPoints < 0)
            {
                RequiredPointsValidation = "Required Points must be greater than or equal to 0";
                return;
            }

            // Determine the minimum XP this rank must exceed
            int? minimumRequiredXp = null;

            if (rank.Parent != null)
            {
                // For pay bands, compare against the previous pay band within the same parent
                var payBandIndex = rank.Parent.PayBands.IndexOf(rank);
                if (payBandIndex > 0)
                {
                    // Not first pay band - compare against previous pay band
                    var previousPayBand = rank.Parent.PayBands[payBandIndex - 1];
                    minimumRequiredXp = previousPayBand.RequiredPoints;
                }
                else if (payBandIndex == 0)
                {
                    // First pay band - must compare against previous RANK in _ranks list, not parent
                    // Parent derives XP from children, so we can't use parent.RequiredPoints
                    var parentIndex = _ranks.IndexOf(rank.Parent);
                    if (parentIndex > 0)
                    {
                        var previousRank = _ranks[parentIndex - 1];
                        minimumRequiredXp = previousRank.IsParent && previousRank.PayBands.Count > 0
                            ? previousRank.PayBands.Max(pb => pb.RequiredPoints)
                            : previousRank.RequiredPoints;
                    }
                    else
                    {
                        // First rank's first pay band - should be >= 0
                        minimumRequiredXp = 0;
                    }
                }
            }
            else
            {
                // For parent ranks, compare against the previous parent rank
                var rankIndex = _ranks.IndexOf(rank);
                if (rankIndex > 0)
                {
                    var previousRank = _ranks[rankIndex - 1];
                    // For ranks with pay bands, use the highest pay band value
                    minimumRequiredXp = previousRank.IsParent && previousRank.PayBands.Count > 0
                        ? previousRank.PayBands.Max(pb => pb.RequiredPoints)
                        : previousRank.RequiredPoints;
                }
            }

            // Check if this rank's required points overlaps with minimum
            if (minimumRequiredXp.HasValue)
            {
                // Check if this is the first pay band
                bool isFirstPayBand = rank.Parent != null && rank.Parent.PayBands.IndexOf(rank) == 0;

                if (isFirstPayBand)
                {
                    // First pay band can equal previous rank's XP, but not be less
                    if (rank.RequiredPoints < minimumRequiredXp.Value)
                    {
                        RequiredPointsValidation = $"Must be greater than or equal to {minimumRequiredXp.Value} (previous rank)";
                    }
                }
                else if (rank.RequiredPoints <= minimumRequiredXp.Value)
                {
                    // All other cases: must be strictly greater than previous
                    var context = rank.Parent != null ? "previous pay band" : "previous rank";
                    RequiredPointsValidation = $"Must be greater than {minimumRequiredXp.Value} ({context})";
                }
            }
        }

        private void ValidateSalary()
        {
            SalaryValidation = string.Empty;

            if (SelectedTreeItem?.Rank == null) return;

            // Check if value is null/empty
            if (_salary == null)
            {
                SalaryValidation = "Salary cannot be empty";
                return;
            }

            var rank = SelectedTreeItem.Rank;
            if (rank.Salary < 0)
            {
                SalaryValidation = "Salary must be greater than or equal to 0";
            }
        }

        private void ValidateName()
        {
            NameValidation = string.Empty;

            if (SelectedTreeItem?.Rank == null) return;

            var currentRank = SelectedTreeItem.Rank;

            // Check for duplicate names (warning, not error)
            // We need to check all ranks including pay bands
            var allRanks = new List<RankHierarchy>();
            foreach (var rank in _ranks)
            {
                allRanks.Add(rank);
                allRanks.AddRange(rank.PayBands);
            }

            // Check if another rank has the same name
            var duplicates = allRanks.Where(r => r.Id != currentRank.Id &&
                                                  string.Equals(r.Name, currentRank.Name, StringComparison.OrdinalIgnoreCase))
                                     .ToList();

            if (duplicates.Count > 0)
            {
                NameValidation = "Another rank already uses this name";
            }
        }

        private void CheckAdvisories()
        {
            StationAdvisory = string.Empty;
            SalaryAdvisory = string.Empty;

            if (SelectedTreeItem?.Rank == null) return;

            var currentRank = SelectedTreeItem.Rank;

            // Find previous rank for comparison
            RankHierarchy? previousRank = null;
            int? previousSalary = null;

            if (currentRank.Parent != null)
            {
                // For pay bands, compare against previous pay band or previous rank
                var payBandIndex = currentRank.Parent.PayBands.IndexOf(currentRank);
                if (payBandIndex > 0)
                {
                    // Not first pay band - compare against previous pay band
                    previousRank = currentRank.Parent.PayBands[payBandIndex - 1];
                    previousSalary = previousRank.Salary;
                }
                else if (payBandIndex == 0)
                {
                    // First pay band - must compare against previous RANK in _ranks list, not parent
                    // Parent derives Salary from children, so we can't use parent.Salary
                    var parentIndex = _ranks.IndexOf(currentRank.Parent);
                    if (parentIndex > 0)
                    {
                        previousRank = _ranks[parentIndex - 1];
                        previousSalary = previousRank.IsParent && previousRank.PayBands.Count > 0
                            ? previousRank.PayBands.Max(pb => pb.Salary)
                            : previousRank.Salary;
                    }
                    // If parentIndex == 0, there's no previous rank, so no advisory applies
                }
            }
            else
            {
                // For parent ranks, compare against previous parent rank
                var rankIndex = _ranks.IndexOf(currentRank);
                if (rankIndex > 0)
                {
                    previousRank = _ranks[rankIndex - 1];
                    // For ranks with pay bands, use the highest pay band salary
                    previousSalary = previousRank.IsParent && previousRank.PayBands.Count > 0
                        ? previousRank.PayBands.Max(pb => pb.Salary)
                        : previousRank.Salary;
                }
            }

            if (previousRank != null)
            {
                // Advisory: Check for missing stations
                var previousStations = previousRank.Stations.Select(s => s.StationName).ToHashSet();
                var currentStations = currentRank.Stations.Select(s => s.StationName).ToHashSet();
                var missingStations = previousStations.Except(currentStations).ToList();

                if (missingStations.Count > 0)
                {
                    var stationList = string.Join(", ", missingStations.Take(3));
                    if (missingStations.Count > 3)
                        stationList += $" (+{missingStations.Count - 3} more)";
                    StationAdvisory = $"{missingStations.Count} station(s) from previous rank not present: {stationList}";
                }

                // Warning: Check for lower salary (matches severity in GetWarningsForRank)
                if (previousSalary.HasValue && currentRank.Salary < previousSalary.Value)
                {
                    SalaryAdvisory = $"Salary is lower than previous ({previousSalary.Value})";
                }
            }
        }

        private void UpdateCommandStates()
        {
            ((RelayCommand)AddRankCommand).RaiseCanExecuteChanged();
            ((RelayCommand)AddPayBandCommand).RaiseCanExecuteChanged();
            ((RelayCommand)UndoCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RedoCommand).RaiseCanExecuteChanged();
            ((RelayCommand)PromoteCommand).RaiseCanExecuteChanged();
            ((RelayCommand)CloneCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RemoveCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RemoveAllRanksCommand).RaiseCanExecuteChanged();
            ((RelayCommand)MoveUpCommand).RaiseCanExecuteChanged();
            ((RelayCommand)MoveDownCommand).RaiseCanExecuteChanged();

            // Notify MainWindow that undo/redo states changed
            UndoRedoStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void PushUndoState()
        {
            var state = SerializeRanks();
            _undoStack.Push(state);

            // Limit stack size
            if (_undoStack.Count > MaxUndoStackSize)
            {
                var tempStack = new Stack<string>(_undoStack.Reverse().Take(MaxUndoStackSize).Reverse());
                _undoStack = tempStack;
            }

            // Clear redo stack when new action is performed
            _redoStack.Clear();

            UpdateCommandStates();
        }

        private string SerializeRanks()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };
            return JsonSerializer.Serialize(_ranks, options);
        }

        /// <summary>
        /// Commits pending changes and triggers XML preview update.
        /// Called when text boxes lose focus.
        /// </summary>
        public void CommitChanges()
        {
            if (!_hasUncommittedChanges) return;

            if (SelectedTreeItem?.Rank == null)
            {
                // Clear flags if no rank selected
                _hasUncommittedChanges = false;
                _oldRequiredPoints = null;
                _oldSalary = null;
                _oldRankName = null;
                return;
            }

            var rank = SelectedTreeItem.Rank;
            var composite = new CompositeCommand("Property changes");
            bool hasChanges = false;

            // Add command for RequiredPoints if it changed
            if (_oldRequiredPoints.HasValue && _oldRequiredPoints.Value != rank.RequiredPoints)
            {
                var oldVal = _oldRequiredPoints.Value;
                var newVal = rank.RequiredPoints;
                composite.AddCommand(new PropertyChangeCommand<int>(
                    value => {
                        if (SelectedTreeItem?.Rank != null)
                        {
                            SelectedTreeItem.Rank.RequiredPoints = value;
                            // Update UI without triggering property change tracking
                            _isUpdatingUI = true;
                            RequiredPoints = value;
                            ValidateRequiredPoints();
                            SelectedTreeItem.UpdateDisplayName();
                            if (SelectedTreeItem.Rank.Parent != null)
                            {
                                UpdateParentRankDisplay(SelectedTreeItem.Rank.Parent);
                            }
                            RefreshTreeView();
                            RaiseDataChanged();
                            _isUpdatingUI = false;
                        }
                    },
                    oldVal,
                    newVal,
                    "RequiredPoints",
                    rank.Name
                ));
                hasChanges = true;
                Logger.Info($"[CommitChanges] RequiredPoints changed from {oldVal} to {newVal}");
            }

            // Add command for Salary if it changed
            if (_oldSalary.HasValue && _oldSalary.Value != rank.Salary)
            {
                var oldVal = _oldSalary.Value;
                var newVal = rank.Salary;
                composite.AddCommand(new PropertyChangeCommand<int>(
                    value => {
                        if (SelectedTreeItem?.Rank != null)
                        {
                            SelectedTreeItem.Rank.Salary = value;
                            // Update UI without triggering property change tracking
                            _isUpdatingUI = true;
                            Salary = value;
                            ValidateSalary();
                            SelectedTreeItem.UpdateDisplayName();
                            if (SelectedTreeItem.Rank.Parent != null)
                            {
                                UpdateParentRankDisplay(SelectedTreeItem.Rank.Parent);
                            }
                            RefreshTreeView();
                            RaiseDataChanged();
                            _isUpdatingUI = false;
                        }
                    },
                    oldVal,
                    newVal,
                    "Salary",
                    rank.Name
                ));
                hasChanges = true;
                Logger.Info($"[CommitChanges] Salary changed from {oldVal} to {newVal}");
            }

            // Add command for RankName if it changed
            if (_oldRankName != null && _oldRankName != rank.Name)
            {
                var oldVal = _oldRankName;
                var newVal = rank.Name;
                composite.AddCommand(new PropertyChangeCommand<string>(
                    value => {
                        if (SelectedTreeItem?.Rank != null)
                        {
                            SelectedTreeItem.Rank.Name = value;
                            // Update UI without triggering property change tracking
                            _isUpdatingUI = true;
                            RankName = value;
                            SelectedTreeItem.UpdateDisplayName();
                            if (SelectedTreeItem.Rank.PayBands.Count > 0)
                            {
                                RenumberPayBands(SelectedTreeItem.Rank);
                            }
                            RefreshTreeView();
                            RaiseDataChanged();
                            _isUpdatingUI = false;
                        }
                    },
                    oldVal,
                    newVal,
                    "RankName",
                    oldVal
                ));
                hasChanges = true;
                Logger.Info($"[CommitChanges] RankName changed from '{oldVal}' to '{newVal}'");
            }

            // Execute composite command if there were actual changes
            if (hasChanges && composite.CommandCount > 0)
            {
                _undoRedoManager.ExecuteCommand(composite);
                Logger.Info($"[CommitChanges] Committed {composite.CommandCount} property change(s)");
            }

            // Reset batch tracking
            _hasUncommittedChanges = false;
            _oldRequiredPoints = null;
            _oldSalary = null;
            _oldRankName = null;

            // Trigger data changed event if there were changes
            if (hasChanges)
            {
                RaiseDataChanged();
            }
        }

        private void RestoreRanks(string json)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                };
                var restoredRanks = JsonSerializer.Deserialize<List<RankHierarchy>>(json, options);
                if (restoredRanks != null)
                {
                    // Save the currently selected rank's ID before restoration
                    var selectedRankId = SelectedTreeItem?.Rank.Id;

                    _ranks = restoredRanks;

                    // Rebuild parent-child relationships after deserialization
                    RebuildParentReferences();

                    RefreshTreeView();

                    // Try to restore selection to the same rank (by ID)
                    if (selectedRankId != null)
                    {
                        var restoredSelection = FindTreeItem(selectedRankId);
                        if (restoredSelection != null)
                        {
                            // Clear and re-set to force UI property updates
                            SelectedTreeItem = null;
                            SelectedTreeItem = restoredSelection;
                            Logger.Info($"Restored selection to '{restoredSelection.Rank.Name}' after undo/redo");
                        }
                        else
                        {
                            // Rank was removed, clear selection
                            SelectedTreeItem = null;
                            Logger.Info("Rank no longer exists after undo/redo, cleared selection");
                        }
                    }

                    RaiseDataChanged();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to restore ranks from undo/redo: {ex.Message}");
            }
        }

        /// <summary>
        /// Rebuilds parent-child references after deserialization.
        /// JSON deserialization breaks the Parent property references in PayBands.
        /// </summary>
        private void RebuildParentReferences()
        {
            foreach (var rank in _ranks)
            {
                if (rank.PayBands.Count > 0)
                {
                    rank.IsParent = true;
                    foreach (var payBand in rank.PayBands)
                    {
                        payBand.Parent = rank;
                    }
                }
            }
        }

        private void LoadSampleData()
        {
            // Sample data for testing
            _ranks = new List<RankHierarchy>
            {
                new RankHierarchy("Rookie", 0, 30000),
                new RankHierarchy("Officer", 100, 40000),
                new RankHierarchy("Detective", 500, 50000)
            };

            RefreshTreeView();
        }

        public void LoadRanks(List<RankHierarchy> ranks)
        {
            _ranks = ranks;
            RefreshTreeView();
        }

        public List<RankHierarchy> GetRanks()
        {
            return _ranks;
        }

        #endregion
    }

    /// <summary>
    /// Event args for status message updates
    /// </summary>
    public class StatusMessageEventArgs : EventArgs
    {
        public string Message { get; }

        public StatusMessageEventArgs(string message)
        {
            Message = message;
        }
    }
}
