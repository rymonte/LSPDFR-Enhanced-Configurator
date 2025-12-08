using System;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Services
{
    /// <summary>
    /// Centralized service for managing shared UI state across ViewModels.
    /// Handles rank selection synchronization and other shared state.
    /// </summary>
    public class SelectionStateService
    {
        private RankHierarchy? _selectedRank;
        private bool _isUpdating;

        /// <summary>
        /// Event raised when the selected rank changes.
        /// Subscribers should update their UI to reflect the new selection.
        /// </summary>
        public event EventHandler<RankSelectionChangedEventArgs>? RankSelectionChanged;

        /// <summary>
        /// Gets or sets the currently selected rank across all tabs.
        /// </summary>
        public RankHierarchy? SelectedRank
        {
            get => _selectedRank;
            set
            {
                // Prevent circular updates
                if (_isUpdating || _selectedRank == value)
                    return;

                _isUpdating = true;
                try
                {
                    _selectedRank = value;
                    Logger.Trace($"[SelectionState] Rank selection changed to: {value?.Name ?? "<null>"}");
                    RankSelectionChanged?.Invoke(this, new RankSelectionChangedEventArgs(value));
                }
                finally
                {
                    _isUpdating = false;
                }
            }
        }
    }

    /// <summary>
    /// Event args for rank selection changes.
    /// </summary>
    public class RankSelectionChangedEventArgs : EventArgs
    {
        public RankHierarchy? SelectedRank { get; }

        public RankSelectionChangedEventArgs(RankHierarchy? selectedRank)
        {
            SelectedRank = selectedRank;
        }
    }
}
