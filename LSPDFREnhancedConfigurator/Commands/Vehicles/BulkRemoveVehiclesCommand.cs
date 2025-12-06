using System;
using System.Collections.Generic;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.Vehicles
{
    /// <summary>
    /// Command for removing multiple vehicles from a rank in bulk.
    /// </summary>
    public class BulkRemoveVehiclesCommand : IUndoRedoCommand
    {
        private readonly RankHierarchy _targetRank;
        private readonly List<Vehicle> _vehiclesToRemove;
        private readonly Action _refreshCallback;
        private readonly Action _dataChangedCallback;

        /// <summary>
        /// Gets a human-readable description of the command.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Creates a new bulk remove vehicles command.
        /// </summary>
        /// <param name="targetRank">The rank to remove vehicles from</param>
        /// <param name="vehiclesToRemove">List of vehicles to remove</param>
        /// <param name="refreshCallback">Callback to refresh the UI</param>
        /// <param name="dataChangedCallback">Callback to raise data changed event</param>
        public BulkRemoveVehiclesCommand(
            RankHierarchy targetRank,
            List<Vehicle> vehiclesToRemove,
            Action refreshCallback,
            Action dataChangedCallback)
        {
            _targetRank = targetRank ?? throw new ArgumentNullException(nameof(targetRank));
            _vehiclesToRemove = vehiclesToRemove ?? throw new ArgumentNullException(nameof(vehiclesToRemove));
            _refreshCallback = refreshCallback ?? throw new ArgumentNullException(nameof(refreshCallback));
            _dataChangedCallback = dataChangedCallback ?? throw new ArgumentNullException(nameof(dataChangedCallback));

            var count = vehiclesToRemove.Count;
            Description = $"Remove {count} vehicle{(count != 1 ? "s" : "")} from '{targetRank.Name}'";
        }

        /// <summary>
        /// Executes the remove vehicles operation.
        /// </summary>
        public void Execute()
        {
            foreach (var vehicle in _vehiclesToRemove)
            {
                _targetRank.Vehicles.Remove(vehicle);
            }

            _refreshCallback();
            _dataChangedCallback();
        }

        /// <summary>
        /// Undoes the remove vehicles operation by re-adding the removed vehicles.
        /// </summary>
        public void Undo()
        {
            foreach (var vehicle in _vehiclesToRemove)
            {
                _targetRank.Vehicles.Add(vehicle);
            }

            _refreshCallback();
            _dataChangedCallback();
        }
    }
}
