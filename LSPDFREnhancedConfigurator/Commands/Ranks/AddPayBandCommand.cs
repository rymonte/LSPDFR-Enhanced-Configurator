using System;
using System.Collections.Generic;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.Ranks
{
    /// <summary>
    /// Command for adding a pay band to a parent rank.
    /// </summary>
    public class AddPayBandCommand : IUndoRedoCommand
    {
        private readonly List<RankHierarchy> _ranks;
        private readonly RankHierarchy _newPayBand;
        private readonly string _parentId;
        private readonly int _insertIndex;
        private readonly Action<RankHierarchy> _renumberPayBands;
        private readonly Action _refreshTreeView;
        private readonly Action _raiseDataChanged;

        /// <summary>
        /// Gets a human-readable description of the command.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the ID of the parent rank.
        /// </summary>
        public string ParentRankId => _parentId;

        /// <summary>
        /// Gets the ID of the newly added pay band.
        /// </summary>
        public string NewPayBandId => _newPayBand.Id;

        /// <summary>
        /// Creates a new add pay band command.
        /// </summary>
        /// <param name="ranks">The ranks list</param>
        /// <param name="newPayBand">The new pay band to add</param>
        /// <param name="parent">The parent rank</param>
        /// <param name="insertIndex">Index at which to insert the pay band in parent's PayBands list</param>
        /// <param name="renumberPayBands">Callback to renumber pay bands</param>
        /// <param name="refreshTreeView">Callback to refresh the tree view</param>
        /// <param name="raiseDataChanged">Callback to raise data changed event</param>
        public AddPayBandCommand(
            List<RankHierarchy> ranks,
            RankHierarchy newPayBand,
            RankHierarchy parent,
            int insertIndex,
            Action<RankHierarchy> renumberPayBands,
            Action refreshTreeView,
            Action raiseDataChanged)
        {
            _ranks = ranks ?? throw new ArgumentNullException(nameof(ranks));
            _newPayBand = newPayBand ?? throw new ArgumentNullException(nameof(newPayBand));
            _parentId = parent?.Id ?? throw new ArgumentNullException(nameof(parent));
            _insertIndex = insertIndex;
            _renumberPayBands = renumberPayBands ?? throw new ArgumentNullException(nameof(renumberPayBands));
            _refreshTreeView = refreshTreeView ?? throw new ArgumentNullException(nameof(refreshTreeView));
            _raiseDataChanged = raiseDataChanged ?? throw new ArgumentNullException(nameof(raiseDataChanged));

            Description = $"Add pay band '{newPayBand.Name}' to '{parent.Name}'";
        }

        /// <summary>
        /// Executes the add pay band operation.
        /// </summary>
        public void Execute()
        {
            var parent = _ranks.Find(r => r.Id == _parentId);
            if (parent == null)
                throw new InvalidOperationException($"Cannot add pay band: Parent rank not found");

            // Insert the pay band
            if (_insertIndex >= 0 && _insertIndex <= parent.PayBands.Count)
            {
                parent.PayBands.Insert(_insertIndex, _newPayBand);
            }
            else
            {
                parent.PayBands.Add(_newPayBand);
            }

            // Set parent reference
            _newPayBand.Parent = parent;

            // Update parent's IsParent flag
            parent.IsParent = true;

            // Renumber pay bands to maintain sequential Roman numerals
            _renumberPayBands(parent);

            // Update UI
            _refreshTreeView();
            _raiseDataChanged();
        }

        /// <summary>
        /// Undoes the add pay band operation by removing the pay band.
        /// </summary>
        public void Undo()
        {
            var parent = _ranks.Find(r => r.Id == _parentId);
            if (parent == null)
                throw new InvalidOperationException($"Cannot undo add pay band: Parent rank not found");

            // Find and remove the pay band
            var index = parent.PayBands.FindIndex(pb => pb.Id == _newPayBand.Id);
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
                    _renumberPayBands(parent);
                }

                // Clear parent reference
                _newPayBand.Parent = null;
            }
            else
            {
                throw new InvalidOperationException($"Cannot undo add pay band: Pay band '{_newPayBand.Name}' not found in parent's list");
            }

            // Update UI
            _refreshTreeView();
            _raiseDataChanged();
        }
    }
}
