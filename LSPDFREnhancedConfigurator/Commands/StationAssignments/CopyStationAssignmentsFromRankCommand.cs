using System;
using System.Collections.Generic;
using System.Linq;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.StationAssignments
{
    /// <summary>
    /// Command for copying station assignments from a source rank to a target rank (additive operation).
    /// Only copies stations that don't already exist in the target.
    /// </summary>
    public class CopyStationAssignmentsFromRankCommand : IUndoRedoCommand
    {
        private readonly RankHierarchy _sourceRank;
        private readonly RankHierarchy _targetRank;
        private readonly List<StationAssignment> _actuallyAddedAssignments;
        private readonly Action _refreshCallback;
        private readonly Action _dataChangedCallback;

        public string Description { get; }

        public CopyStationAssignmentsFromRankCommand(
            RankHierarchy sourceRank,
            RankHierarchy targetRank,
            Action refreshCallback,
            Action dataChangedCallback)
        {
            _sourceRank = sourceRank ?? throw new ArgumentNullException(nameof(sourceRank));
            _targetRank = targetRank ?? throw new ArgumentNullException(nameof(targetRank));
            _refreshCallback = refreshCallback ?? throw new ArgumentNullException(nameof(refreshCallback));
            _dataChangedCallback = dataChangedCallback ?? throw new ArgumentNullException(nameof(dataChangedCallback));
            _actuallyAddedAssignments = new List<StationAssignment>();

            Description = $"Copy stations from '{sourceRank.Name}' to '{targetRank.Name}'";
        }

        public void Execute()
        {
            _actuallyAddedAssignments.Clear();

            foreach (var station in _sourceRank.Stations)
            {
                // Check if station already exists in target
                if (!_targetRank.Stations.Any(s => s.StationName == station.StationName))
                {
                    // Create a deep copy of the station assignment
                    var newAssignment = new StationAssignment(
                        station.StationName,
                        new List<string>(station.Zones),
                        station.StyleID)
                    {
                        StationReference = station.StationReference
                    };

                    _targetRank.Stations.Add(newAssignment);
                    _actuallyAddedAssignments.Add(newAssignment);
                }
            }

            _refreshCallback();
            _dataChangedCallback();
        }

        public void Undo()
        {
            foreach (var assignment in _actuallyAddedAssignments)
            {
                _targetRank.Stations.Remove(assignment);
            }

            _refreshCallback();
            _dataChangedCallback();
        }
    }
}
