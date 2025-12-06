using System;
using System.Collections.Generic;
using System.Linq;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.Vehicles
{
    /// <summary>
    /// Command for removing all vehicles from a rank.
    /// </summary>
    public class RemoveAllVehiclesCommand : IUndoRedoCommand
    {
        private readonly RankHierarchy _targetRank;
        private readonly List<Vehicle> _previousVehicles;
        private readonly Action _refreshCallback;
        private readonly Action _dataChangedCallback;

        public string Description { get; }

        public RemoveAllVehiclesCommand(
            RankHierarchy targetRank,
            Action refreshCallback,
            Action dataChangedCallback)
        {
            _targetRank = targetRank ?? throw new ArgumentNullException(nameof(targetRank));
            _refreshCallback = refreshCallback ?? throw new ArgumentNullException(nameof(refreshCallback));
            _dataChangedCallback = dataChangedCallback ?? throw new ArgumentNullException(nameof(dataChangedCallback));

            // Backup current vehicles before clearing
            _previousVehicles = _targetRank.Vehicles.ToList();

            var count = _previousVehicles.Count;
            Description = $"Remove all {count} vehicle{(count != 1 ? "s" : "")} from '{targetRank.Name}'";
        }

        public void Execute()
        {
            _targetRank.Vehicles.Clear();

            _refreshCallback();
            _dataChangedCallback();
        }

        public void Undo()
        {
            foreach (var vehicle in _previousVehicles)
            {
                _targetRank.Vehicles.Add(vehicle);
            }

            _refreshCallback();
            _dataChangedCallback();
        }
    }
}
