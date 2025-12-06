using System;
using System.Collections.Generic;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.StationAssignments
{
    /// <summary>
    /// Command for adding multiple station assignments to a rank in bulk.
    /// </summary>
    public class BulkAddStationAssignmentsCommand : IUndoRedoCommand
    {
        private readonly RankHierarchy _targetRank;
        private readonly List<StationAssignment> _assignmentsToAdd;
        private readonly Action _refreshCallback;
        private readonly Action _dataChangedCallback;

        public string Description { get; }

        public BulkAddStationAssignmentsCommand(
            RankHierarchy targetRank,
            List<StationAssignment> assignmentsToAdd,
            Action refreshCallback,
            Action dataChangedCallback)
        {
            _targetRank = targetRank ?? throw new ArgumentNullException(nameof(targetRank));
            _assignmentsToAdd = assignmentsToAdd ?? throw new ArgumentNullException(nameof(assignmentsToAdd));
            _refreshCallback = refreshCallback ?? throw new ArgumentNullException(nameof(refreshCallback));
            _dataChangedCallback = dataChangedCallback ?? throw new ArgumentNullException(nameof(dataChangedCallback));

            var count = assignmentsToAdd.Count;
            Description = $"Add {count} station{(count != 1 ? "s" : "")} to '{targetRank.Name}'";
        }

        public void Execute()
        {
            foreach (var assignment in _assignmentsToAdd)
            {
                _targetRank.Stations.Add(assignment);
            }

            _refreshCallback();
            _dataChangedCallback();
        }

        public void Undo()
        {
            foreach (var assignment in _assignmentsToAdd)
            {
                _targetRank.Stations.Remove(assignment);
            }

            _refreshCallback();
            _dataChangedCallback();
        }
    }
}
