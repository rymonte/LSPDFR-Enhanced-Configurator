using System;
using System.Collections.Generic;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.Ranks
{
    /// <summary>
    /// Command for promoting a pay band to a parent rank.
    /// </summary>
    public class PromoteRankCommand : IUndoRedoCommand
    {
        private readonly List<RankHierarchy> _ranks;
        private readonly RankHierarchy _payBand;
        private readonly string _payBandId;
        private readonly string _originalParentId;
        private readonly int _originalPayBandIndex;
        private readonly int _insertIndex; // Index in _ranks list after promotion
        private readonly Action<RankHierarchy> _renumberPayBands;
        private readonly Action _refreshTreeView;
        private readonly Action _raiseDataChanged;

        /// <summary>
        /// Gets a human-readable description of the command.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the ID of the promoted rank.
        /// </summary>
        public string PromotedRankId => _payBandId;

        /// <summary>
        /// Creates a new promote rank command.
        /// </summary>
        /// <param name="ranks">The ranks list</param>
        /// <param name="payBand">The pay band to promote</param>
        /// <param name="originalParent">The original parent rank</param>
        /// <param name="payBandIndex">Original index in parent's PayBands list</param>
        /// <param name="insertIndex">Index at which to insert in _ranks list</param>
        /// <param name="renumberPayBands">Callback to renumber pay bands</param>
        /// <param name="refreshTreeView">Callback to refresh the tree view</param>
        /// <param name="raiseDataChanged">Callback to raise data changed event</param>
        public PromoteRankCommand(
            List<RankHierarchy> ranks,
            RankHierarchy payBand,
            RankHierarchy originalParent,
            int payBandIndex,
            int insertIndex,
            Action<RankHierarchy> renumberPayBands,
            Action refreshTreeView,
            Action raiseDataChanged)
        {
            _ranks = ranks ?? throw new ArgumentNullException(nameof(ranks));
            _payBand = payBand ?? throw new ArgumentNullException(nameof(payBand));
            _payBandId = payBand.Id;
            _originalParentId = originalParent?.Id ?? throw new ArgumentNullException(nameof(originalParent));
            _originalPayBandIndex = payBandIndex;
            _insertIndex = insertIndex;
            _renumberPayBands = renumberPayBands ?? throw new ArgumentNullException(nameof(renumberPayBands));
            _refreshTreeView = refreshTreeView ?? throw new ArgumentNullException(nameof(refreshTreeView));
            _raiseDataChanged = raiseDataChanged ?? throw new ArgumentNullException(nameof(raiseDataChanged));

            Description = $"Promote pay band '{payBand.Name}' from '{originalParent.Name}' to parent rank";
        }

        /// <summary>
        /// Executes the promote operation.
        /// </summary>
        public void Execute()
        {
            var originalParent = _ranks.Find(r => r.Id == _originalParentId);
            if (originalParent == null)
                throw new InvalidOperationException($"Cannot promote: Original parent rank not found");

            // Remove from parent's PayBands list
            var payBandIndex = originalParent.PayBands.FindIndex(pb => pb.Id == _payBandId);
            if (payBandIndex >= 0)
            {
                originalParent.PayBands.RemoveAt(payBandIndex);
            }
            else
            {
                throw new InvalidOperationException($"Cannot promote: Pay band '{_payBand.Name}' not found in parent's list");
            }

            // Update parent's IsParent flag if no more pay bands
            if (originalParent.PayBands.Count == 0)
            {
                originalParent.IsParent = false;
            }

            // Renumber remaining pay bands
            if (originalParent.PayBands.Count > 0)
            {
                _renumberPayBands(originalParent);
            }

            // Clear parent reference
            _payBand.Parent = null;

            // Add to top-level ranks list
            if (_insertIndex >= 0 && _insertIndex <= _ranks.Count)
            {
                _ranks.Insert(_insertIndex, _payBand);
            }
            else
            {
                _ranks.Add(_payBand);
            }

            // Update UI
            _refreshTreeView();
            _raiseDataChanged();
        }

        /// <summary>
        /// Undoes the promote operation by demoting back to pay band.
        /// </summary>
        public void Undo()
        {
            var originalParent = _ranks.Find(r => r.Id == _originalParentId);
            if (originalParent == null)
                throw new InvalidOperationException($"Cannot undo promote: Original parent rank not found");

            // Remove from _ranks list
            var rankIndex = _ranks.FindIndex(r => r.Id == _payBandId);
            if (rankIndex >= 0)
            {
                _ranks.RemoveAt(rankIndex);
            }
            else
            {
                throw new InvalidOperationException($"Cannot undo promote: Rank '{_payBand.Name}' not found in ranks list");
            }

            // Restore to parent's PayBands list at original position
            if (_originalPayBandIndex >= 0 && _originalPayBandIndex <= originalParent.PayBands.Count)
            {
                originalParent.PayBands.Insert(_originalPayBandIndex, _payBand);
            }
            else
            {
                originalParent.PayBands.Add(_payBand);
            }

            // Restore parent reference
            _payBand.Parent = originalParent;

            // Update parent's IsParent flag
            originalParent.IsParent = true;

            // Renumber pay bands to restore sequential Roman numerals
            _renumberPayBands(originalParent);

            // Update UI
            _refreshTreeView();
            _raiseDataChanged();
        }
    }
}
