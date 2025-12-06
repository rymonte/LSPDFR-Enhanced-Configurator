using System;
using System.Collections.Generic;
using System.Linq;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.StationAssignments
{
    /// <summary>
    /// Command for removing all station assignments from a rank.
    /// </summary>
    public class RemoveAllStationAssignmentsCommand : IUndoRedoCommand
    {
        private readonly RankHierarchy _targetRank;
        private readonly List<StationAssignment> _previousAssignments;
        private readonly Action _refreshCallback;
        private readonly Action _dataChangedCallback;

        public string Description { get; }

        public RemoveAllStationAssignmentsCommand(
            RankHierarchy targetRank,
            Action refreshCallback,
            Action dataChangedCallback)
        {
            _targetRank = targetRank ?? throw new ArgumentNullException(nameof(targetRank));
            _refreshCallback = refreshCallback ?? throw new ArgumentNullException(nameof(refreshCallback));
            _dataChangedCallback = dataChangedCallback ?? throw new ArgumentNullException(nameof(dataChangedCallback));

            // Backup current assignments before clearing
            _previousAssignments = _targetRank.Stations.ToList();

            var count = _previousAssignments.Count;
            Description = $"Remove all {count} station{(count != 1 ? "s" : "")} from '{targetRank.Name}'";
        }

        public void Execute()
        {
            _targetRank.Stations.Clear();

            _refreshCallback();
            _dataChangedCallback();
        }

        public void Undo()
        {
            foreach (var assignment in _previousAssignments)
            {
                _targetRank.Stations.Add(assignment);
            }

            _refreshCallback();
            _dataChangedCallback();
        }
    }
}
