using System;
using System.Collections.Generic;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.Outfits
{
    /// <summary>
    /// Command for adding multiple outfits to a rank in bulk.
    /// </summary>
    public class BulkAddOutfitsCommand : IUndoRedoCommand
    {
        private readonly RankHierarchy _targetRank;
        private readonly List<string> _outfitsToAdd;
        private readonly Action _refreshCallback;
        private readonly Action _dataChangedCallback;

        /// <summary>
        /// Gets a human-readable description of the command.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Creates a new bulk add outfits command.
        /// </summary>
        public BulkAddOutfitsCommand(
            RankHierarchy targetRank,
            List<string> outfitsToAdd,
            Action refreshCallback,
            Action dataChangedCallback)
        {
            _targetRank = targetRank ?? throw new ArgumentNullException(nameof(targetRank));
            _outfitsToAdd = outfitsToAdd ?? throw new ArgumentNullException(nameof(outfitsToAdd));
            _refreshCallback = refreshCallback ?? throw new ArgumentNullException(nameof(refreshCallback));
            _dataChangedCallback = dataChangedCallback ?? throw new ArgumentNullException(nameof(dataChangedCallback));

            var count = outfitsToAdd.Count;
            Description = $"Add {count} outfit{(count != 1 ? "s" : "")} to '{targetRank.Name}'";
        }

        public void Execute()
        {
            foreach (var outfit in _outfitsToAdd)
            {
                _targetRank.Outfits.Add(outfit);
            }

            _refreshCallback();
            _dataChangedCallback();
        }

        public void Undo()
        {
            foreach (var outfit in _outfitsToAdd)
            {
                _targetRank.Outfits.Remove(outfit);
            }

            _refreshCallback();
            _dataChangedCallback();
        }
    }
}
