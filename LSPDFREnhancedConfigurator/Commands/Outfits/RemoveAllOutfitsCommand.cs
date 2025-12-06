using System;
using System.Collections.Generic;
using System.Linq;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.Outfits
{
    /// <summary>
    /// Command for removing all outfits from a rank.
    /// </summary>
    public class RemoveAllOutfitsCommand : IUndoRedoCommand
    {
        private readonly RankHierarchy _targetRank;
        private readonly List<string> _previousOutfits;
        private readonly Action _refreshCallback;
        private readonly Action _dataChangedCallback;

        public string Description { get; }

        public RemoveAllOutfitsCommand(
            RankHierarchy targetRank,
            Action refreshCallback,
            Action dataChangedCallback)
        {
            _targetRank = targetRank ?? throw new ArgumentNullException(nameof(targetRank));
            _refreshCallback = refreshCallback ?? throw new ArgumentNullException(nameof(refreshCallback));
            _dataChangedCallback = dataChangedCallback ?? throw new ArgumentNullException(nameof(dataChangedCallback));

            // Backup current outfits before clearing
            _previousOutfits = _targetRank.Outfits.ToList();

            var count = _previousOutfits.Count;
            Description = $"Remove all {count} outfit{(count != 1 ? "s" : "")} from '{targetRank.Name}'";
        }

        public void Execute()
        {
            _targetRank.Outfits.Clear();

            _refreshCallback();
            _dataChangedCallback();
        }

        public void Undo()
        {
            foreach (var outfit in _previousOutfits)
            {
                _targetRank.Outfits.Add(outfit);
            }

            _refreshCallback();
            _dataChangedCallback();
        }
    }
}
