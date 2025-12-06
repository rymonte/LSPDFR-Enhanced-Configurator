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
        private readonly List<Vehicle> _previousTargetVehicles; // Backup of target's original vehicles
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

            // Backup current target vehicles before executing
            _previousTargetVehicles = _targetRank.Vehicles.ToList();

            var count = _sourceRank.Vehicles.Count;
            Description = $"Copy {count} vehicle{(count != 1 ? "s" : "")} from '{sourceRank.Name}' to '{targetRank.Name}' (overwrite)";
        }

        /// <summary>
        /// Executes the copy operation by clearing target and copying from source.
        /// </summary>
        public void Execute()
        {
            _targetRank.Vehicles.Clear();
            foreach (var vehicle in _sourceRank.Vehicles)
            {
                _targetRank.Vehicles.Add(vehicle);
            }

            _dataChangedCallback();
        }

        /// <summary>
        /// Undoes the copy operation by restoring the target rank's original vehicles.
        /// </summary>
        public void Undo()
        {
            _targetRank.Vehicles.Clear();
            foreach (var vehicle in _previousTargetVehicles)
            {
                _targetRank.Vehicles.Add(vehicle);
            }

            _dataChangedCallback();
        }
    }
}
