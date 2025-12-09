using System;
using System.Collections.Generic;
using System.Linq;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.Vehicles
{
    /// <summary>
    /// Command for copying vehicles from a source rank to a target rank (additive operation).
    /// Only copies vehicles that don't already exist in the target.
    /// </summary>
    public class CopyVehiclesFromRankCommand : IUndoRedoCommand
    {
        private readonly RankHierarchy _sourceRank;
        private readonly RankHierarchy _targetRank;
        private readonly List<Vehicle> _actuallyAddedGlobalVehicles;
        private readonly Dictionary<string, List<Vehicle>> _actuallyAddedStationVehicles; // StationName -> List of vehicles
        private readonly Action _refreshCallback;
        private readonly Action _dataChangedCallback;

        /// <summary>
        /// Gets a human-readable description of the command.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Creates a new copy vehicles from rank command.
        /// </summary>
        /// <param name="sourceRank">The rank to copy vehicles from</param>
        /// <param name="targetRank">The rank to copy vehicles to</param>
        /// <param name="refreshCallback">Callback to refresh the UI</param>
        /// <param name="dataChangedCallback">Callback to raise data changed event</param>
        public CopyVehiclesFromRankCommand(
            RankHierarchy sourceRank,
            RankHierarchy targetRank,
            Action refreshCallback,
            Action dataChangedCallback)
        {
            _sourceRank = sourceRank ?? throw new ArgumentNullException(nameof(sourceRank));
            _targetRank = targetRank ?? throw new ArgumentNullException(nameof(targetRank));
            _refreshCallback = refreshCallback ?? throw new ArgumentNullException(nameof(refreshCallback));
            _dataChangedCallback = dataChangedCallback ?? throw new ArgumentNullException(nameof(dataChangedCallback));
            _actuallyAddedGlobalVehicles = new List<Vehicle>();
            _actuallyAddedStationVehicles = new Dictionary<string, List<Vehicle>>(StringComparer.OrdinalIgnoreCase);

            Description = $"Copy vehicles from '{sourceRank.Name}' to '{targetRank.Name}'";
        }

        /// <summary>
        /// Executes the copy operation, only adding vehicles that don't already exist.
        /// </summary>
        public void Execute()
        {
            _actuallyAddedGlobalVehicles.Clear();
            _actuallyAddedStationVehicles.Clear();

            // Copy global vehicles from source to target
            foreach (var vehicle in _sourceRank.Vehicles)
            {
                // Check if vehicle already exists (by model)
                if (!_targetRank.Vehicles.Any(v => v.Model == vehicle.Model))
                {
                    _targetRank.Vehicles.Add(vehicle);
                    _actuallyAddedGlobalVehicles.Add(vehicle);
                }
            }

            // Copy station-specific vehicles
            foreach (var sourceStation in _sourceRank.Stations)
            {
                foreach (var vehicle in sourceStation.Vehicles)
                {
                    // Try to find matching station in target
                    var targetStation = _targetRank.Stations.FirstOrDefault(s =>
                        s.StationName.Equals(sourceStation.StationName, StringComparison.OrdinalIgnoreCase));

                    if (targetStation != null)
                    {
                        // Add to matching station if not already present
                        if (!targetStation.Vehicles.Any(v => v.Model == vehicle.Model))
                        {
                            targetStation.Vehicles.Add(vehicle);

                            if (!_actuallyAddedStationVehicles.ContainsKey(targetStation.StationName))
                            {
                                _actuallyAddedStationVehicles[targetStation.StationName] = new List<Vehicle>();
                            }
                            _actuallyAddedStationVehicles[targetStation.StationName].Add(vehicle);
                        }
                    }
                    else
                    {
                        // No matching station - add to global if not already present
                        if (!_targetRank.Vehicles.Any(v => v.Model == vehicle.Model))
                        {
                            _targetRank.Vehicles.Add(vehicle);
                            _actuallyAddedGlobalVehicles.Add(vehicle);
                        }
                    }
                }
            }

            _refreshCallback();
            _dataChangedCallback();
        }

        /// <summary>
        /// Undoes the copy operation by removing only the vehicles that were added.
        /// </summary>
        public void Undo()
        {
            // Remove global vehicles
            foreach (var vehicle in _actuallyAddedGlobalVehicles)
            {
                _targetRank.Vehicles.Remove(vehicle);
            }

            // Remove station-specific vehicles
            foreach (var kvp in _actuallyAddedStationVehicles)
            {
                var station = _targetRank.Stations.FirstOrDefault(s =>
                    s.StationName.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase));

                if (station != null)
                {
                    foreach (var vehicle in kvp.Value)
                    {
                        station.Vehicles.Remove(vehicle);
                    }
                }
            }

            _refreshCallback();
            _dataChangedCallback();
        }
    }
}
