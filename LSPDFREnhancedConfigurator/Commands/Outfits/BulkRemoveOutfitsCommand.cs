using System;
using System.Collections.Generic;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.Outfits
{
    /// <summary>
    /// Command for removing multiple outfits from a rank in bulk.
    /// </summary>
    public class BulkRemoveOutfitsCommand : IUndoRedoCommand
    {
        private readonly RankHierarchy _targetRank;
        private readonly List<string> _outfitsToRemove;
        private readonly Action _refreshCallback;
        private readonly Action _dataChangedCallback;

        public string Description { get; }

        public BulkRemoveOutfitsCommand(
            RankHierarchy targetRank,
            List<string> outfitsToRemove,
            Action refreshCallback,
            Action dataChangedCallback)
        {
            _targetRank = targetRank ?? throw new ArgumentNullException(nameof(targetRank));
            _outfitsToRemove = outfitsToRemove ?? throw new ArgumentNullException(nameof(outfitsToRemove));
            _refreshCallback = refreshCallback ?? throw new ArgumentNullException(nameof(refreshCallback));
            _dataChangedCallback = dataChangedCallback ?? throw new ArgumentNullException(nameof(dataChangedCallback));

            var count = outfitsToRemove.Count;
            Description = $"Remove {count} outfit{(count != 1 ? "s" : "")} from '{targetRank.Name}'";
        }

        public void Execute()
        {
            foreach (var outfit in _outfitsToRemove)
            {
                _targetRank.Outfits.Remove(outfit);
            }

            _refreshCallback();
            _dataChangedCallback();
        }

        public void Undo()
        {
            foreach (var outfit in _outfitsToRemove)
            {
                _targetRank.Outfits.Add(outfit);
            }

            _refreshCallback();
            _dataChangedCallback();
        }
    }
}
