using System;
using System.Collections.Generic;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.Ranks
{
    /// <summary>
    /// Command for cloning a rank or pay band.
    /// </summary>
    public class CloneRankCommand : IUndoRedoCommand
    {
        private readonly List<RankHierarchy> _ranks;
        private readonly RankHierarchy _clonedRank;
        private readonly int _insertIndex;
        private readonly string? _parentId; // For pay band clones
        private readonly Action<RankHierarchy>? _renumberPayBands; // Callback to renumber pay bands (for pay band clones)
        private readonly Action _refreshTreeView;
        private readonly Action _raiseDataChanged;

        /// <summary>
        /// Gets a human-readable description of the command.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the ID of the cloned rank.
        /// </summary>
        public string ClonedRankId => _clonedRank.Id;

        /// <summary>
        /// Gets the ID of the parent rank if this was a pay band clone, otherwise null.
        /// </summary>
        public string? ParentRankId => _parentId;

        /// <summary>
        /// Creates a new clone rank command for a parent rank.
        /// </summary>
        /// <param name="ranks">The ranks list</param>
        /// <param name="clonedRank">The cloned rank</param>
        /// <param name="insertIndex">Index at which to insert the clone</param>
        /// <param name="originalRankName">Name of the original rank (for description)</param>
        /// <param name="refreshTreeView">Callback to refresh the tree view</param>
        /// <param name="raiseDataChanged">Callback to raise data changed event</param>
        public CloneRankCommand(
            List<RankHierarchy> ranks,
            RankHierarchy clonedRank,
            int insertIndex,
            string originalRankName,
            Action refreshTreeView,
            Action raiseDataChanged)
        {
            _ranks = ranks ?? throw new ArgumentNullException(nameof(ranks));
            _clonedRank = clonedRank ?? throw new ArgumentNullException(nameof(clonedRank));
            _insertIndex = insertIndex;
            _parentId = null;
            _renumberPayBands = null;
            _refreshTreeView = refreshTreeView ?? throw new ArgumentNullException(nameof(refreshTreeView));
            _raiseDataChanged = raiseDataChanged ?? throw new ArgumentNullException(nameof(raiseDataChanged));

            Description = $"Clone rank '{originalRankName}' to '{clonedRank.Name}'";
        }

        /// <summary>
        /// Creates a new clone rank command for a pay band.
        /// </summary>
        /// <param name="ranks">The ranks list</param>
        /// <param name="clonedPayBand">The cloned pay band</param>
        /// <param name="parent">The parent rank</param>
        /// <param name="insertIndex">Index at which to insert the clone in parent's PayBands list</param>
        /// <param name="originalPayBandName">Name of the original pay band (for description)</param>
        /// <param name="renumberPayBands">Callback to renumber pay bands</param>
        /// <param name="refreshTreeView">Callback to refresh the tree view</param>
        /// <param name="raiseDataChanged">Callback to raise data changed event</param>
        public CloneRankCommand(
            List<RankHierarchy> ranks,
            RankHierarchy clonedPayBand,
            RankHierarchy parent,
            int insertIndex,
            string originalPayBandName,
            Action<RankHierarchy> renumberPayBands,
            Action refreshTreeView,
            Action raiseDataChanged)
        {
            _ranks = ranks ?? throw new ArgumentNullException(nameof(ranks));
            _clonedRank = clonedPayBand ?? throw new ArgumentNullException(nameof(clonedPayBand));
            _insertIndex = insertIndex;
            _parentId = parent?.Id ?? throw new ArgumentNullException(nameof(parent));
            _renumberPayBands = renumberPayBands ?? throw new ArgumentNullException(nameof(renumberPayBands));
            _refreshTreeView = refreshTreeView ?? throw new ArgumentNullException(nameof(refreshTreeView));
            _raiseDataChanged = raiseDataChanged ?? throw new ArgumentNullException(nameof(raiseDataChanged));

            Description = $"Clone pay band '{originalPayBandName}' in '{parent.Name}'";
        }

        /// <summary>
        /// Executes the clone operation by inserting the cloned rank.
        /// </summary>
        public void Execute()
        {
            if (_parentId == null)
            {
                // Clone parent rank
                if (_insertIndex >= 0 && _insertIndex <= _ranks.Count)
                {
                    _ranks.Insert(_insertIndex, _clonedRank);
                }
                else
                {
                    _ranks.Add(_clonedRank);
                }
            }
            else
            {
                // Clone pay band
                var parent = _ranks.Find(r => r.Id == _parentId);
                if (parent == null)
                    throw new InvalidOperationException($"Cannot clone pay band: Parent rank not found");

                if (_insertIndex >= 0 && _insertIndex <= parent.PayBands.Count)
                {
                    parent.PayBands.Insert(_insertIndex, _clonedRank);
                }
                else
                {
                    parent.PayBands.Add(_clonedRank);
                }

                // Set parent reference
                _clonedRank.Parent = parent;

                // Update parent's IsParent flag
                parent.IsParent = true;

                // Renumber pay bands to maintain sequential Roman numerals
                _renumberPayBands!(parent);
            }

            // Update UI
            _refreshTreeView();
            _raiseDataChanged();
        }

        /// <summary>
        /// Undoes the clone operation by removing the cloned rank.
        /// </summary>
        public void Undo()
        {
            if (_parentId == null)
            {
                // Remove cloned parent rank
                var index = _ranks.FindIndex(r => r.Id == _clonedRank.Id);
                if (index >= 0)
                {
                    _ranks.RemoveAt(index);
                }
                else
                {
                    throw new InvalidOperationException($"Cannot undo clone: Rank '{_clonedRank.Name}' not found in list");
                }
            }
            else
            {
                // Remove cloned pay band
                var parent = _ranks.Find(r => r.Id == _parentId);
                if (parent == null)
                    throw new InvalidOperationException($"Cannot undo clone: Parent rank not found");

                var index = parent.PayBands.FindIndex(pb => pb.Id == _clonedRank.Id);
                if (index >= 0)
                {
                    parent.PayBands.RemoveAt(index);

                    // Update parent's IsParent flag if no more pay bands
                    if (parent.PayBands.Count == 0)
                    {
                        parent.IsParent = false;
                    }

                    // Renumber remaining pay bands
                    if (parent.PayBands.Count > 0)
                    {
                        _renumberPayBands!(parent);
                    }

                    // Clear parent reference
                    _clonedRank.Parent = null;
                }
                else
                {
                    throw new InvalidOperationException($"Cannot undo clone: Pay band '{_clonedRank.Name}' not found in parent's list");
                }
            }

            // Update UI
            _refreshTreeView();
            _raiseDataChanged();
        }
    }
}
