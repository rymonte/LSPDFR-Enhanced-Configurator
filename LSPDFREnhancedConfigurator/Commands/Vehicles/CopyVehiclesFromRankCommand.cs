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
        private readonly List<Vehicle> _actuallyAddedVehicles; // Only vehicles that were added (not duplicates)
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
            _actuallyAddedVehicles = new List<Vehicle>();

            Description = $"Copy vehicles from '{sourceRank.Name}' to '{targetRank.Name}'";
        }

        /// <summary>
        /// Executes the copy operation, only adding vehicles that don't already exist.
        /// </summary>
        public void Execute()
        {
            _actuallyAddedVehicles.Clear();

            foreach (var vehicle in _sourceRank.Vehicles)
            {
                // Check if vehicle already exists (by model)
                if (!_targetRank.Vehicles.Any(v => v.Model == vehicle.Model))
                {
                    _targetRank.Vehicles.Add(vehicle);
                    _actuallyAddedVehicles.Add(vehicle);
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
            foreach (var vehicle in _actuallyAddedVehicles)
            {
                _targetRank.Vehicles.Remove(vehicle);
            }

            _refreshCallback();
            _dataChangedCallback();
        }
    }
}
