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
        private readonly List<string> _actuallyAddedGlobalOutfits;
        private readonly Dictionary<string, List<string>> _actuallyAddedStationOutfits; // StationName -> List of outfits
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
            _actuallyAddedGlobalOutfits = new List<string>();
            _actuallyAddedStationOutfits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            Description = $"Copy outfits from '{sourceRank.Name}' to '{targetRank.Name}'";
        }

        public void Execute()
        {
            _actuallyAddedGlobalOutfits.Clear();
            _actuallyAddedStationOutfits.Clear();

            // Copy global outfits from source to target
            foreach (var outfit in _sourceRank.Outfits)
            {
                if (!_targetRank.Outfits.Contains(outfit, StringComparer.OrdinalIgnoreCase))
                {
                    _targetRank.Outfits.Add(outfit);
                    _actuallyAddedGlobalOutfits.Add(outfit);
                }
            }

            // Copy station-specific outfits
            foreach (var sourceStation in _sourceRank.Stations)
            {
                foreach (var outfit in sourceStation.Outfits)
                {
                    // Try to find matching station in target
                    var targetStation = _targetRank.Stations.FirstOrDefault(s =>
                        s.StationName.Equals(sourceStation.StationName, StringComparison.OrdinalIgnoreCase));

                    if (targetStation != null)
                    {
                        // Add to matching station if not already present
                        if (!targetStation.Outfits.Contains(outfit, StringComparer.OrdinalIgnoreCase))
                        {
                            targetStation.Outfits.Add(outfit);

                            if (!_actuallyAddedStationOutfits.ContainsKey(targetStation.StationName))
                            {
                                _actuallyAddedStationOutfits[targetStation.StationName] = new List<string>();
                            }
                            _actuallyAddedStationOutfits[targetStation.StationName].Add(outfit);
                        }
                    }
                    else
                    {
                        // No matching station - add to global if not already present
                        if (!_targetRank.Outfits.Contains(outfit, StringComparer.OrdinalIgnoreCase))
                        {
                            _targetRank.Outfits.Add(outfit);
                            _actuallyAddedGlobalOutfits.Add(outfit);
                        }
                    }
                }
            }

            _refreshCallback();
            _dataChangedCallback();
        }

        public void Undo()
        {
            // Remove global outfits
            foreach (var outfit in _actuallyAddedGlobalOutfits)
            {
                _targetRank.Outfits.Remove(outfit);
            }

            // Remove station-specific outfits
            foreach (var kvp in _actuallyAddedStationOutfits)
            {
                var station = _targetRank.Stations.FirstOrDefault(s =>
                    s.StationName.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase));

                if (station != null)
                {
                    foreach (var outfit in kvp.Value)
                    {
                        station.Outfits.Remove(outfit);
                    }
                }
            }

            _refreshCallback();
            _dataChangedCallback();
        }
    }
}
