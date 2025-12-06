using System;
using System.Collections.Generic;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.Vehicles
{
    /// <summary>
    /// Command for adding multiple vehicles to a rank in bulk.
    /// </summary>
    public class BulkAddVehiclesCommand : IUndoRedoCommand
    {
        private readonly RankHierarchy _targetRank;
        private readonly List<Vehicle> _vehiclesToAdd;
        private readonly Action _refreshCallback;
        private readonly Action _dataChangedCallback;

        /// <summary>
        /// Gets a human-readable description of the command.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Creates a new bulk add vehicles command.
        /// </summary>
        /// <param name="targetRank">The rank to add vehicles to</param>
        /// <param name="vehiclesToAdd">List of vehicles to add</param>
        /// <param name="refreshCallback">Callback to refresh the UI</param>
        /// <param name="dataChangedCallback">Callback to raise data changed event</param>
        public BulkAddVehiclesCommand(
            RankHierarchy targetRank,
            List<Vehicle> vehiclesToAdd,
            Action refreshCallback,
            Action dataChangedCallback)
        {
            _targetRank = targetRank ?? throw new ArgumentNullException(nameof(targetRank));
            _vehiclesToAdd = vehiclesToAdd ?? throw new ArgumentNullException(nameof(vehiclesToAdd));
            _refreshCallback = refreshCallback ?? throw new ArgumentNullException(nameof(refreshCallback));
            _dataChangedCallback = dataChangedCallback ?? throw new ArgumentNullException(nameof(dataChangedCallback));

            var count = vehiclesToAdd.Count;
            Description = $"Add {count} vehicle{(count != 1 ? "s" : "")} to '{targetRank.Name}'";
        }

        /// <summary>
        /// Executes the add vehicles operation.
        /// </summary>
        public void Execute()
        {
            foreach (var vehicle in _vehiclesToAdd)
            {
                _targetRank.Vehicles.Add(vehicle);
            }

            _refreshCallback();
            _dataChangedCallback();
        }

        /// <summary>
        /// Undoes the add vehicles operation by removing the added vehicles.
        /// </summary>
        public void Undo()
        {
            foreach (var vehicle in _vehiclesToAdd)
            {
                _targetRank.Vehicles.Remove(vehicle);
            }

            _refreshCallback();
            _dataChangedCallback();
        }
    }
}
