using System;
using System.Collections.Generic;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.StationAssignments
{
    /// <summary>
    /// Command for removing multiple station assignments from a rank in bulk.
    /// </summary>
    public class BulkRemoveStationAssignmentsCommand : IUndoRedoCommand
    {
        private readonly RankHierarchy _targetRank;
        private readonly List<StationAssignment> _assignmentsToRemove;
        private readonly Action _refreshCallback;
        private readonly Action _dataChangedCallback;

        public string Description { get; }

        public BulkRemoveStationAssignmentsCommand(
            RankHierarchy targetRank,
            List<StationAssignment> assignmentsToRemove,
            Action refreshCallback,
            Action dataChangedCallback)
        {
            _targetRank = targetRank ?? throw new ArgumentNullException(nameof(targetRank));
            _assignmentsToRemove = assignmentsToRemove ?? throw new ArgumentNullException(nameof(assignmentsToRemove));
            _refreshCallback = refreshCallback ?? throw new ArgumentNullException(nameof(refreshCallback));
            _dataChangedCallback = dataChangedCallback ?? throw new ArgumentNullException(nameof(dataChangedCallback));

            var count = assignmentsToRemove.Count;
            Description = $"Remove {count} station{(count != 1 ? "s" : "")} from '{targetRank.Name}'";
        }

        public void Execute()
        {
            foreach (var assignment in _assignmentsToRemove)
            {
                _targetRank.Stations.Remove(assignment);
            }

            _refreshCallback();
            _dataChangedCallback();
        }

        public void Undo()
        {
            foreach (var assignment in _assignmentsToRemove)
            {
                _targetRank.Stations.Add(assignment);
            }

            _refreshCallback();
            _dataChangedCallback();
        }
    }
}
