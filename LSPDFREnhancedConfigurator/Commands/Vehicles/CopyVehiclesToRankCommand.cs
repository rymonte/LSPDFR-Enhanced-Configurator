using System;
using System.Collections.Generic;
using System.Linq;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.Vehicles
{
    /// <summary>
    /// Command for copying vehicles to a target rank (destructive operation).
    /// Clears all vehicles in the target rank first, then copies all vehicles from source.
    /// </summary>
    public class CopyVehiclesToRankCommand : IUndoRedoCommand
    {
        private readonly RankHierarchy _sourceRank;
        private readonly RankHierarchy _targetRank;
        private readonly List<Vehicle> _previousTargetGlobalVehicles;
        private readonly Dictionary<string, List<Vehicle>> _previousTargetStationVehicles; // StationName -> List of vehicles
        private readonly Action _dataChangedCallback;

        /// <summary>
        /// Gets a human-readable description of the command.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Creates a new copy vehicles to rank command.
        /// </summary>
        /// <param name="sourceRank">The rank to copy vehicles from</param>
        /// <param name="targetRank">The rank to copy vehicles to (will be cleared)</param>
        /// <param name="dataChangedCallback">Callback to raise data changed event</param>
        public CopyVehiclesToRankCommand(
            RankHierarchy sourceRank,
            RankHierarchy targetRank,
            Action dataChangedCallback)
        {
            _sourceRank = sourceRank ?? throw new ArgumentNullException(nameof(sourceRank));
            _targetRank = targetRank ?? throw new ArgumentNullException(nameof(targetRank));
            _dataChangedCallback = dataChangedCallback ?? throw new ArgumentNullException(nameof(dataChangedCallback));

            // Save previous state: global vehicles
            _previousTargetGlobalVehicles = _targetRank.Vehicles.ToList();

            // Save previous state: station-specific vehicles
            _previousTargetStationVehicles = new Dictionary<string, List<Vehicle>>(StringComparer.OrdinalIgnoreCase);
            foreach (var station in _targetRank.Stations)
            {
                if (station.VehicleOverrides.Count > 0)
                {
                    _previousTargetStationVehicles[station.StationName] = station.VehicleOverrides.ToList();
                }
            }

            // Count all source vehicles for description
            var allSourceVehicles = new HashSet<string>(_sourceRank.Vehicles.Select(v => v.Model), StringComparer.OrdinalIgnoreCase);
            foreach (var station in _sourceRank.Stations)
            {
                foreach (var vehicle in station.VehicleOverrides)
                {
                    allSourceVehicles.Add(vehicle.Model);
                }
            }
            var count = allSourceVehicles.Count;
            Description = $"Copy {count} vehicle{(count != 1 ? "s" : "")} from '{sourceRank.Name}' to '{targetRank.Name}' (overwrite)";
        }

        /// <summary>
        /// Executes the copy operation by clearing target and copying from source.
        /// </summary>
        public void Execute()
        {
            // Clear all vehicles from target: global and station-specific
            _targetRank.Vehicles.Clear();
            foreach (var station in _targetRank.Stations)
            {
                station.VehicleOverrides.Clear();
            }

            // Copy global vehicles from source to target
            foreach (var vehicle in _sourceRank.Vehicles)
            {
                _targetRank.Vehicles.Add(vehicle);
            }

            // Copy station-specific vehicles
            foreach (var sourceStation in _sourceRank.Stations)
            {
                foreach (var vehicle in sourceStation.VehicleOverrides)
                {
                    // Try to find matching station in target
                    var targetStation = _targetRank.Stations.FirstOrDefault(s =>
                        s.StationName.Equals(sourceStation.StationName, StringComparison.OrdinalIgnoreCase));

                    if (targetStation != null)
                    {
                        // Add to matching station
                        targetStation.VehicleOverrides.Add(vehicle);
                    }
                    else
                    {
                        // No matching station - add to global
                        _targetRank.Vehicles.Add(vehicle);
                    }
                }
            }

            _dataChangedCallback();
        }

        /// <summary>
        /// Undoes the copy operation by restoring the target rank's original vehicles.
        /// </summary>
        public void Undo()
        {
            // Clear all vehicles from target
            _targetRank.Vehicles.Clear();
            foreach (var station in _targetRank.Stations)
            {
                station.VehicleOverrides.Clear();
            }

            // Restore previous global vehicles
            foreach (var vehicle in _previousTargetGlobalVehicles)
            {
                _targetRank.Vehicles.Add(vehicle);
            }

            // Restore previous station-specific vehicles
            foreach (var kvp in _previousTargetStationVehicles)
            {
                var station = _targetRank.Stations.FirstOrDefault(s =>
                    s.StationName.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase));

                if (station != null)
                {
                    foreach (var vehicle in kvp.Value)
                    {
                        station.VehicleOverrides.Add(vehicle);
                    }
                }
            }

            _dataChangedCallback();
        }
    }
}
