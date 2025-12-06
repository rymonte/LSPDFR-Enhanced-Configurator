using System;
using System.Collections.Generic;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.Ranks
{
    /// <summary>
    /// Command for adding a new rank to the hierarchy.
    /// </summary>
    public class AddRankCommand : IUndoRedoCommand
    {
        private readonly List<RankHierarchy> _ranks;
        private readonly RankHierarchy _newRank;
        private readonly int _insertIndex;
        private readonly Action _refreshTreeView;
        private readonly Action _raiseDataChanged;

        /// <summary>
        /// Gets a human-readable description of the command.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the ID of the newly added rank (for selection after undo/redo).
        /// </summary>
        public string NewRankId => _newRank.Id;

        /// <summary>
        /// Creates a new add rank command.
        /// </summary>
        /// <param name="ranks">The ranks list</param>
        /// <param name="newRank">The new rank to add</param>
        /// <param name="insertIndex">Index at which to insert the rank</param>
        /// <param name="refreshTreeView">Callback to refresh the tree view</param>
        /// <param name="raiseDataChanged">Callback to raise data changed event</param>
        public AddRankCommand(
            List<RankHierarchy> ranks,
            RankHierarchy newRank,
            int insertIndex,
            Action refreshTreeView,
            Action raiseDataChanged)
        {
            _ranks = ranks ?? throw new ArgumentNullException(nameof(ranks));
            _newRank = newRank ?? throw new ArgumentNullException(nameof(newRank));
            _insertIndex = insertIndex;
            _refreshTreeView = refreshTreeView ?? throw new ArgumentNullException(nameof(refreshTreeView));
            _raiseDataChanged = raiseDataChanged ?? throw new ArgumentNullException(nameof(raiseDataChanged));

            Description = $"Add rank '{newRank.Name}' at index {insertIndex}";
        }

        /// <summary>
        /// Executes the add operation by inserting the rank.
        /// </summary>
        public void Execute()
        {
            // Validate index
            if (_insertIndex < 0 || _insertIndex > _ranks.Count)
                throw new InvalidOperationException($"Invalid insert index: {_insertIndex}");

            // Insert the rank
            _ranks.Insert(_insertIndex, _newRank);

            // Update UI
            _refreshTreeView();
            _raiseDataChanged();
        }

        /// <summary>
        /// Undoes the add operation by removing the rank.
        /// </summary>
        public void Undo()
        {
            // Find and remove the rank by ID (in case index changed)
            var index = _ranks.FindIndex(r => r.Id == _newRank.Id);
            if (index >= 0)
            {
                _ranks.RemoveAt(index);
            }
            else
            {
                throw new InvalidOperationException($"Cannot undo: Rank '{_newRank.Name}' not found in list");
            }

            // Update UI
            _refreshTreeView();
            _raiseDataChanged();
        }
    }
}
