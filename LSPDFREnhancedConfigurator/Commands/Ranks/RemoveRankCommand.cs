using System;
using System.Collections.Generic;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.Ranks
{
    /// <summary>
    /// Command for removing a rank or pay band from the hierarchy.
    /// </summary>
    public class RemoveRankCommand : IUndoRedoCommand
    {
        private readonly List<RankHierarchy> _ranks;
        private readonly RankHierarchy _rankToRemove;
        private readonly string _rankId;
        private readonly int _originalIndex;
        private readonly string? _parentId; // For pay bands
        private readonly int _payBandIndex; // For pay bands
        private readonly Action<RankHierarchy> _renumberPayBands; // Callback to renumber pay bands
        private readonly Action _refreshTreeView;
        private readonly Action _raiseDataChanged;

        /// <summary>
        /// Gets a human-readable description of the command.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the ID of the parent rank if this was a pay band, otherwise null.
        /// </summary>
        public string? ParentRankId => _parentId;

        /// <summary>
        /// Gets the ID of the removed rank.
        /// </summary>
        public string RemovedRankId => _rankId;

        /// <summary>
        /// Creates a new remove rank command for a parent rank.
        /// </summary>
        /// <param name="ranks">The ranks list</param>
        /// <param name="rank">The rank to remove</param>
        /// <param name="index">Index of the rank in the list</param>
        /// <param name="refreshTreeView">Callback to refresh the tree view</param>
        /// <param name="raiseDataChanged">Callback to raise data changed event</param>
        public RemoveRankCommand(
            List<RankHierarchy> ranks,
            RankHierarchy rank,
            int index,
            Action refreshTreeView,
            Action raiseDataChanged)
        {
            _ranks = ranks ?? throw new ArgumentNullException(nameof(ranks));
            _rankToRemove = rank ?? throw new ArgumentNullException(nameof(rank));
            _rankId = rank.Id;
            _originalIndex = index;
            _parentId = null;
            _payBandIndex = -1;
            _renumberPayBands = null!;
            _refreshTreeView = refreshTreeView ?? throw new ArgumentNullException(nameof(refreshTreeView));
            _raiseDataChanged = raiseDataChanged ?? throw new ArgumentNullException(nameof(raiseDataChanged));

            Description = $"Remove rank '{rank.Name}'";
        }

        /// <summary>
        /// Creates a new remove rank command for a pay band.
        /// </summary>
        /// <param name="ranks">The ranks list</param>
        /// <param name="payBand">The pay band to remove</param>
        /// <param name="parent">The parent rank</param>
        /// <param name="payBandIndex">Index of the pay band in parent's PayBands list</param>
        /// <param name="renumberPayBands">Callback to renumber remaining pay bands</param>
        /// <param name="refreshTreeView">Callback to refresh the tree view</param>
        /// <param name="raiseDataChanged">Callback to raise data changed event</param>
        public RemoveRankCommand(
            List<RankHierarchy> ranks,
            RankHierarchy payBand,
            RankHierarchy parent,
            int payBandIndex,
            Action<RankHierarchy> renumberPayBands,
            Action refreshTreeView,
            Action raiseDataChanged)
        {
            _ranks = ranks ?? throw new ArgumentNullException(nameof(ranks));
            _rankToRemove = payBand ?? throw new ArgumentNullException(nameof(payBand));
            _rankId = payBand.Id;
            _originalIndex = -1; // Not in ranks list
            _parentId = parent?.Id ?? throw new ArgumentNullException(nameof(parent));
            _payBandIndex = payBandIndex;
            _renumberPayBands = renumberPayBands ?? throw new ArgumentNullException(nameof(renumberPayBands));
            _refreshTreeView = refreshTreeView ?? throw new ArgumentNullException(nameof(refreshTreeView));
            _raiseDataChanged = raiseDataChanged ?? throw new ArgumentNullException(nameof(raiseDataChanged));

            Description = $"Remove pay band '{payBand.Name}' from '{parent.Name}'";
        }

        /// <summary>
        /// Executes the remove operation.
        /// </summary>
        public void Execute()
        {
            if (_parentId == null)
            {
                // Remove parent rank
                var index = _ranks.FindIndex(r => r.Id == _rankId);
                if (index >= 0)
                {
                    _ranks.RemoveAt(index);
                }
                else
                {
                    throw new InvalidOperationException($"Cannot remove: Rank '{_rankToRemove.Name}' not found in list");
                }
            }
            else
            {
                // Remove pay band
                var parent = _ranks.Find(r => r.Id == _parentId);
                if (parent == null)
                    throw new InvalidOperationException($"Cannot remove pay band: Parent rank not found");

                var payBandIndex = parent.PayBands.FindIndex(pb => pb.Id == _rankId);
                if (payBandIndex >= 0)
                {
                    parent.PayBands.RemoveAt(payBandIndex);

                    // Update parent's IsParent flag if no more pay bands
                    if (parent.PayBands.Count == 0)
                    {
                        parent.IsParent = false;
                    }

                    // Renumber remaining pay bands
                    if (parent.PayBands.Count > 0)
                    {
                        _renumberPayBands(parent);
                    }

                    // Clear parent reference
                    _rankToRemove.Parent = null;
                }
                else
                {
                    throw new InvalidOperationException($"Cannot remove: Pay band '{_rankToRemove.Name}' not found in parent's list");
                }
            }

            // Update UI
            _refreshTreeView();
            _raiseDataChanged();
        }

        /// <summary>
        /// Undoes the remove operation by re-adding the rank/pay band.
        /// </summary>
        public void Undo()
        {
            if (_parentId == null)
            {
                // Restore parent rank
                if (_originalIndex >= 0 && _originalIndex <= _ranks.Count)
                {
                    _ranks.Insert(_originalIndex, _rankToRemove);
                }
                else
                {
                    _ranks.Add(_rankToRemove);
                }
            }
            else
            {
                // Restore pay band
                var parent = _ranks.Find(r => r.Id == _parentId);
                if (parent == null)
                    throw new InvalidOperationException($"Cannot undo remove: Parent rank not found");

                // Restore pay band at original position
                if (_payBandIndex >= 0 && _payBandIndex <= parent.PayBands.Count)
                {
                    parent.PayBands.Insert(_payBandIndex, _rankToRemove);
                }
                else
                {
                    parent.PayBands.Add(_rankToRemove);
                }

                // Restore parent reference
                _rankToRemove.Parent = parent;

                // Update parent's IsParent flag
                parent.IsParent = true;

                // Renumber pay bands to restore sequential Roman numerals
                _renumberPayBands(parent);
            }

            // Update UI
            _refreshTreeView();
            _raiseDataChanged();
        }
    }
}
