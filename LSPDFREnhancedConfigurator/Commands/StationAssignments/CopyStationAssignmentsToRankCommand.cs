using System;
using System.Collections.Generic;
using System.Linq;
using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Commands.StationAssignments
{
    /// <summary>
    /// Command for copying station assignments to a target rank (destructive operation).
    /// Clears all stations in the target rank first, then copies all stations from source.
    /// </summary>
    public class CopyStationAssignmentsToRankCommand : IUndoRedoCommand
    {
        private readonly RankHierarchy _sourceRank;
        private readonly RankHierarchy _targetRank;
        private readonly List<StationAssignment> _previousTargetAssignments;
        private readonly Action _dataChangedCallback;

        public string Description { get; }

        public CopyStationAssignmentsToRankCommand(
            RankHierarchy sourceRank,
            RankHierarchy targetRank,
            Action dataChangedCallback)
        {
            _sourceRank = sourceRank ?? throw new ArgumentNullException(nameof(sourceRank));
            _targetRank = targetRank ?? throw new ArgumentNullException(nameof(targetRank));
            _dataChangedCallback = dataChangedCallback ?? throw new ArgumentNullException(nameof(dataChangedCallback));

            // Backup target's current assignments before clearing
            _previousTargetAssignments = _targetRank.Stations.ToList();

            var count = _sourceRank.Stations.Count;
            Description = $"Copy {count} station{(count != 1 ? "s" : "")} from '{sourceRank.Name}' to '{targetRank.Name}' (overwrite)";
        }

        public void Execute()
        {
            _targetRank.Stations.Clear();

            foreach (var station in _sourceRank.Stations)
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
            }

            _dataChangedCallback();
        }

        public void Undo()
        {
            _targetRank.Stations.Clear();

            foreach (var assignment in _previousTargetAssignments)
            {
                _targetRank.Stations.Add(assignment);
            }

            _dataChangedCallback();
        }
    }
}
