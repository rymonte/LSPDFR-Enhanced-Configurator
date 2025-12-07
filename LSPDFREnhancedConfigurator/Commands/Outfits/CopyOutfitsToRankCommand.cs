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
        private readonly List<string> _previousTargetGlobalOutfits;
        private readonly Dictionary<string, List<string>> _previousTargetStationOutfits; // StationName -> List of outfits
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

            // Save previous state: global outfits
            _previousTargetGlobalOutfits = _targetRank.Outfits.ToList();

            // Save previous state: station-specific outfits
            _previousTargetStationOutfits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var station in _targetRank.Stations)
            {
                if (station.OutfitOverrides.Count > 0)
                {
                    _previousTargetStationOutfits[station.StationName] = station.OutfitOverrides.ToList();
                }
            }

            // Count all source outfits for description
            var allSourceOutfits = new HashSet<string>(_sourceRank.Outfits, StringComparer.OrdinalIgnoreCase);
            foreach (var station in _sourceRank.Stations)
            {
                foreach (var outfit in station.OutfitOverrides)
                {
                    allSourceOutfits.Add(outfit);
                }
            }
            var count = allSourceOutfits.Count;
            Description = $"Copy {count} outfit{(count != 1 ? "s" : "")} from '{sourceRank.Name}' to '{targetRank.Name}' (overwrite)";
        }

        public void Execute()
        {
            // Clear all outfits from target: global and station-specific
            _targetRank.Outfits.Clear();
            foreach (var station in _targetRank.Stations)
            {
                station.OutfitOverrides.Clear();
            }

            // Copy global outfits from source to target
            foreach (var outfit in _sourceRank.Outfits)
            {
                _targetRank.Outfits.Add(outfit);
            }

            // Copy station-specific outfits
            foreach (var sourceStation in _sourceRank.Stations)
            {
                foreach (var outfit in sourceStation.OutfitOverrides)
                {
                    // Try to find matching station in target
                    var targetStation = _targetRank.Stations.FirstOrDefault(s =>
                        s.StationName.Equals(sourceStation.StationName, StringComparison.OrdinalIgnoreCase));

                    if (targetStation != null)
                    {
                        // Add to matching station
                        targetStation.OutfitOverrides.Add(outfit);
                    }
                    else
                    {
                        // No matching station - add to global
                        _targetRank.Outfits.Add(outfit);
                    }
                }
            }

            _dataChangedCallback();
        }

        public void Undo()
        {
            // Clear all outfits from target
            _targetRank.Outfits.Clear();
            foreach (var station in _targetRank.Stations)
            {
                station.OutfitOverrides.Clear();
            }

            // Restore previous global outfits
            foreach (var outfit in _previousTargetGlobalOutfits)
            {
                _targetRank.Outfits.Add(outfit);
            }

            // Restore previous station-specific outfits
            foreach (var kvp in _previousTargetStationOutfits)
            {
                var station = _targetRank.Stations.FirstOrDefault(s =>
                    s.StationName.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase));

                if (station != null)
                {
                    foreach (var outfit in kvp.Value)
                    {
                        station.OutfitOverrides.Add(outfit);
                    }
                }
            }

            _dataChangedCallback();
        }
    }
}
