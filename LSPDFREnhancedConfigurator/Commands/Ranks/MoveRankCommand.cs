using System;
using System.Collections.Generic;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.Ranks
{
    /// <summary>
    /// Command for moving a rank up or down in the hierarchy list.
    /// </summary>
    public class MoveRankCommand : IUndoRedoCommand
    {
        private readonly List<RankHierarchy> _ranks;
        private readonly string _rankId;
        private readonly int _fromIndex;
        private readonly int _toIndex;
        private readonly Action _refreshTreeView;
        private readonly Action _raiseDataChanged;

        /// <summary>
        /// Gets a human-readable description of the command.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Creates a new move rank command.
        /// </summary>
        /// <param name="ranks">The ranks list</param>
        /// <param name="rankId">ID of the rank to move</param>
        /// <param name="fromIndex">Current index of the rank</param>
        /// <param name="toIndex">Target index for the rank</param>
        /// <param name="rankName">Name of the rank (for description)</param>
        /// <param name="refreshTreeView">Callback to refresh the tree view</param>
        /// <param name="raiseDataChanged">Callback to raise data changed event</param>
        public MoveRankCommand(
            List<RankHierarchy> ranks,
            string rankId,
            int fromIndex,
            int toIndex,
            string rankName,
            Action refreshTreeView,
            Action raiseDataChanged)
        {
            _ranks = ranks ?? throw new ArgumentNullException(nameof(ranks));
            _rankId = rankId ?? throw new ArgumentNullException(nameof(rankId));
            _fromIndex = fromIndex;
            _toIndex = toIndex;
            _refreshTreeView = refreshTreeView ?? throw new ArgumentNullException(nameof(refreshTreeView));
            _raiseDataChanged = raiseDataChanged ?? throw new ArgumentNullException(nameof(raiseDataChanged));

            string direction = toIndex < fromIndex ? "up" : "down";
            Description = $"Move '{rankName}' {direction} (from index {fromIndex} to {toIndex})";
        }

        /// <summary>
        /// Executes the move operation.
        /// </summary>
        public void Execute()
        {
            MoveRank(_fromIndex, _toIndex);
        }

        /// <summary>
        /// Undoes the move operation by moving back to original position.
        /// </summary>
        public void Undo()
        {
            MoveRank(_toIndex, _fromIndex);
        }

        private void MoveRank(int fromIndex, int toIndex)
        {
            // Validate indices
            if (fromIndex < 0 || fromIndex >= _ranks.Count)
                throw new InvalidOperationException($"Invalid from index: {fromIndex}");

            if (toIndex < 0 || toIndex >= _ranks.Count)
                throw new InvalidOperationException($"Invalid to index: {toIndex}");

            // Get the rank
            var rank = _ranks[fromIndex];

            // Verify rank ID matches (safety check)
            if (rank.Id != _rankId)
                throw new InvalidOperationException($"Rank ID mismatch at index {fromIndex}");

            // Perform the move
            _ranks.RemoveAt(fromIndex);
            _ranks.Insert(toIndex, rank);

            // Update UI
            _refreshTreeView();
            _raiseDataChanged();
        }
    }
}
