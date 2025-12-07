using System;
using System.Collections.Generic;
using System.Linq;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.Outfits
{
    /// <summary>
    /// Command for copying outfits from a source rank to a target rank (additive operation).
    /// Only copies outfits that don't already exist in the target.
    /// </summary>
    public class CopyOutfitsFromRankCommand : IUndoRedoCommand
    {
        private readonly RankHierarchy _sourceRank;
        private readonly RankHierarchy _targetRank;
        private readonly List<string> _actuallyAddedOutfits;
        private readonly Action _refreshCallback;
        private readonly Action _dataChangedCallback;

        public string Description { get; }

        public CopyOutfitsFromRankCommand(
            RankHierarchy sourceRank,
            RankHierarchy targetRank,
            Action refreshCallback,
            Action dataChangedCallback)
        {
            _sourceRank = sourceRank ?? throw new ArgumentNullException(nameof(sourceRank));
            _targetRank = targetRank ?? throw new ArgumentNullException(nameof(targetRank));
            _refreshCallback = refreshCallback ?? throw new ArgumentNullException(nameof(refreshCallback));
            _dataChangedCallback = dataChangedCallback ?? throw new ArgumentNullException(nameof(dataChangedCallback));
            _actuallyAddedOutfits = new List<string>();

            Description = $"Copy outfits from '{sourceRank.Name}' to '{targetRank.Name}'";
        }

        public void Execute()
        {
            _actuallyAddedOutfits.Clear();

            // Collect all outfits from source: global + station-specific overrides
            var allSourceOutfits = new HashSet<string>(_sourceRank.Outfits, StringComparer.OrdinalIgnoreCase);
            foreach (var station in _sourceRank.Stations)
            {
                foreach (var outfit in station.OutfitOverrides)
                {
                    allSourceOutfits.Add(outfit);
                }
            }

            // Copy to target rank (global level only)
            foreach (var outfit in allSourceOutfits)
            {
                // Check if outfit already exists
                if (!_targetRank.Outfits.Contains(outfit, StringComparer.OrdinalIgnoreCase))
                {
                    _targetRank.Outfits.Add(outfit);
                    _actuallyAddedOutfits.Add(outfit);
                }
            }

            _refreshCallback();
            _dataChangedCallback();
        }

        public void Undo()
        {
            foreach (var outfit in _actuallyAddedOutfits)
            {
                _targetRank.Outfits.Remove(outfit);
            }

            _refreshCallback();
            _dataChangedCallback();
        }
    }
}
