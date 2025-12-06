using System;
using System.Collections.Generic;
using System.Linq;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.Outfits
{
    /// <summary>
    /// Command for copying outfits to a target rank (destructive operation).
    /// Clears all outfits in the target rank first, then copies all outfits from source.
    /// </summary>
    public class CopyOutfitsToRankCommand : IUndoRedoCommand
    {
        private readonly RankHierarchy _sourceRank;
        private readonly RankHierarchy _targetRank;
        private readonly List<string> _previousTargetOutfits;
        private readonly Action _dataChangedCallback;

        public string Description { get; }

        public CopyOutfitsToRankCommand(
            RankHierarchy sourceRank,
            RankHierarchy targetRank,
            Action dataChangedCallback)
        {
            _sourceRank = sourceRank ?? throw new ArgumentNullException(nameof(sourceRank));
            _targetRank = targetRank ?? throw new ArgumentNullException(nameof(targetRank));
            _dataChangedCallback = dataChangedCallback ?? throw new ArgumentNullException(nameof(dataChangedCallback));

            _previousTargetOutfits = _targetRank.Outfits.ToList();

            var count = _sourceRank.Outfits.Count;
            Description = $"Copy {count} outfit{(count != 1 ? "s" : "")} from '{sourceRank.Name}' to '{targetRank.Name}' (overwrite)";
        }

        public void Execute()
        {
            _targetRank.Outfits.Clear();
            foreach (var outfit in _sourceRank.Outfits)
            {
                _targetRank.Outfits.Add(outfit);
            }

            _dataChangedCallback();
        }

        public void Undo()
        {
            _targetRank.Outfits.Clear();
            foreach (var outfit in _previousTargetOutfits)
            {
                _targetRank.Outfits.Add(outfit);
            }

            _dataChangedCallback();
        }
    }
}
