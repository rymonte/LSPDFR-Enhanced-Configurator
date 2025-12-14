using System.Collections.Generic;
using FluentAssertions;
using LSPDFREnhancedConfigurator.Commands.StationAssignments;
using LSPDFREnhancedConfigurator.Models;
using LSPDFREnhancedConfigurator.Tests.Builders;
using Xunit;

namespace LSPDFREnhancedConfigurator.Tests.Commands.StationAssignments
{
    [Trait("Category", "Unit")]
    [Trait("Component", "Commands")]
    public class BulkStationAssignmentCommandsTests
    {
        #region BulkAddStationAssignmentsCommand Tests

        [Fact]
        public void BulkAddStationAssignmentsCommand_Execute_AddsStationsToRank()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var station1 = new StationAssignment("Mission Row", new List<string>(), 1);
            var station2 = new StationAssignment("Vespucci", new List<string>(), 1);
            var stationsToAdd = new List<StationAssignment> { station1, station2 };

            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new BulkAddStationAssignmentsCommand(
                rank,
                stationsToAdd,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            rank.Stations.Should().Contain(station1);
            rank.Stations.Should().Contain(station2);
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void BulkAddStationAssignmentsCommand_Undo_RemovesStationsFromRank()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var station1 = new StationAssignment("Mission Row", new List<string>(), 1);
            var station2 = new StationAssignment("Vespucci", new List<string>(), 1);
            var stationsToAdd = new List<StationAssignment> { station1, station2 };

            var command = new BulkAddStationAssignmentsCommand(
                rank,
                stationsToAdd,
                () => { },
                () => { });

            command.Execute();

            // Act
            command.Undo();

            // Assert
            rank.Stations.Should().NotContain(station1);
            rank.Stations.Should().NotContain(station2);
        }

        [Fact]
        public void BulkAddStationAssignmentsCommand_Description_IncludesCount()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var stationsToAdd = new List<StationAssignment>
            {
                new StationAssignment("Mission Row", new List<string>(), 1),
                new StationAssignment("Vespucci", new List<string>(), 1)
            };

            // Act
            var command = new BulkAddStationAssignmentsCommand(
                rank,
                stationsToAdd,
                () => { },
                () => { });

            // Assert
            command.Description.Should().Contain("2 stations");
            command.Description.Should().Contain("Officer");
        }

        #endregion

        #region BulkRemoveStationAssignmentsCommand Tests

        [Fact]
        public void BulkRemoveStationAssignmentsCommand_Execute_RemovesStationsFromRank()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var station1 = new StationAssignment("Mission Row", new List<string>(), 1);
            var station2 = new StationAssignment("Vespucci", new List<string>(), 1);
            rank.Stations.Add(station1);
            rank.Stations.Add(station2);

            var stationsToRemove = new List<StationAssignment> { station1, station2 };

            var refreshCalled = false;
            var dataChangedCalled = false;

            var command = new BulkRemoveStationAssignmentsCommand(
                rank,
                stationsToRemove,
                () => refreshCalled = true,
                () => dataChangedCalled = true);

            // Act
            command.Execute();

            // Assert
            rank.Stations.Should().NotContain(station1);
            rank.Stations.Should().NotContain(station2);
            refreshCalled.Should().BeTrue();
            dataChangedCalled.Should().BeTrue();
        }

        [Fact]
        public void BulkRemoveStationAssignmentsCommand_Undo_RestoresStationsToRank()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var station1 = new StationAssignment("Mission Row", new List<string>(), 1);
            var station2 = new StationAssignment("Vespucci", new List<string>(), 1);
            rank.Stations.Add(station1);
            rank.Stations.Add(station2);

            var stationsToRemove = new List<StationAssignment> { station1, station2 };

            var command = new BulkRemoveStationAssignmentsCommand(
                rank,
                stationsToRemove,
                () => { },
                () => { });

            command.Execute();

            // Act
            command.Undo();

            // Assert
            rank.Stations.Should().Contain(station1);
            rank.Stations.Should().Contain(station2);
        }

        [Fact]
        public void BulkRemoveStationAssignmentsCommand_Description_IncludesCount()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var stationsToRemove = new List<StationAssignment>
            {
                new StationAssignment("Mission Row", new List<string>(), 1),
                new StationAssignment("Vespucci", new List<string>(), 1)
            };

            // Act
            var command = new BulkRemoveStationAssignmentsCommand(
                rank,
                stationsToRemove,
                () => { },
                () => { });

            // Assert
            command.Description.Should().Contain("2 stations");
            command.Description.Should().Contain("Officer");
        }

        #endregion

        #region AddAllStationAssignmentsCommand Tests

        [Fact]
        public void AddAllStationAssignmentsCommand_Execute_AddsAllAvailableStations()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var assignmentsToAdd = new List<StationAssignment>
            {
                new StationAssignment("Mission Row", new List<string>(), 1),
                new StationAssignment("Vespucci", new List<string>(), 1)
            };

            var command = new AddAllStationAssignmentsCommand(
                rank,
                assignmentsToAdd,
                () => { },
                () => { });

            // Act
            command.Execute();

            // Assert
            rank.Stations.Should().HaveCount(2);
            rank.Stations.Should().Contain(s => s.StationName == "Mission Row");
            rank.Stations.Should().Contain(s => s.StationName == "Vespucci");
        }

        [Fact]
        public void AddAllStationAssignmentsCommand_Undo_RemovesAllAddedStations()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var assignmentsToAdd = new List<StationAssignment>
            {
                new StationAssignment("Mission Row", new List<string>(), 1),
                new StationAssignment("Vespucci", new List<string>(), 1)
            };

            var command = new AddAllStationAssignmentsCommand(
                rank,
                assignmentsToAdd,
                () => { },
                () => { });

            command.Execute();

            // Act
            command.Undo();

            // Assert
            rank.Stations.Should().BeEmpty();
        }

        [Fact]
        public void AddAllStationAssignmentsCommand_Description_IncludesRankName()
        {
            // Arrange
            var rank = new RankHierarchyBuilder().WithName("Officer").Build();
            var assignmentsToAdd = new List<StationAssignment>
            {
                new StationAssignment("Mission Row", new List<string>(), 1)
            };

            // Act
            var command = new AddAllStationAssignmentsCommand(
                rank,
                assignmentsToAdd,
                () => { },
                () => { });

            // Assert
            command.Description.Should().Contain("Officer");
            command.Description.Should().Contain("all");
        }

        #endregion
    }
}
